using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryStorageOptions
{
	public partial class Plugin
	{
		private const string PreUpdateKey = "PreUpdate";

		public void Update(IServiceProvider serviceProvider)
		{
			IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
			if (context.MessageName != MessageName.Update)
				//Invalid event attached
				return;

			if (context.InputParameters.Contains(CrmConstants.TargetParameterKey) && context.InputParameters[CrmConstants.TargetParameterKey] is Entity)
			{
				try
				{
					Entity entity = (Entity)context.InputParameters[CrmConstants.TargetParameterKey];
					if (entity.LogicalName != CrmConstants.AnnotationEntityName && entity.LogicalName != CrmConstants.AttachmentEntityName)
						return;

					if (!context.PreEntityImages.ContainsKey(PreUpdateKey) || context.PreEntityImages[PreUpdateKey] == null)
					{
						throw new InvalidPluginExecutionException(OperationStatus.Failed, string.Format("PreEntityImage could not be found when updating {0}", context.PrimaryEntityName));
					}
					Entity preUpdateEntity = context.PreEntityImages[PreUpdateKey];

					if (!entity.Attributes.Keys.Contains(GenericConstants.Constants[entity.LogicalName][GenericConstants.DocumentBodyAttributeKey]))
					{
						//No change to binary data
						return;
					}

					IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
					IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

					Configuration.IConfigurationProvider configurationProvider = Configuration.Factory.GetConfigurationProvider(service, entity.LogicalName, unsecurePluginStepConfiguration, securePluginStepConfiguration);
					if (configurationProvider.StorageProviderType == Providers.BinaryStorageProviderType.CrmDefault)
					{
						//In this case, not doing anything with the binary data.
						return;
					}

					Providers.IBinaryStorageProvider storageProvider = Providers.Factory.GetStorageProvider(configurationProvider);

					//If the document body is empty, delete the external binary (filename is in the preupdate image)
					if (string.IsNullOrWhiteSpace((string)entity.Attributes[GenericConstants.Constants[entity.LogicalName][GenericConstants.DocumentBodyAttributeKey]]))
					{
						storageProvider.Delete(preUpdateEntity.Id, (string)preUpdateEntity.Attributes[GenericConstants.Constants[entity.LogicalName][GenericConstants.FileNameAttributeKey]]);
						return;
					}

					byte[] data = Convert.FromBase64String((string)entity.Attributes[GenericConstants.Constants[entity.LogicalName][GenericConstants.DocumentBodyAttributeKey]]);
					string fileName = (string)entity.Attributes[GenericConstants.Constants[entity.LogicalName][GenericConstants.FileNameAttributeKey]];
					if (storageProvider.Create(entity.Id, fileName, data))
					{
						entity.Attributes[GenericConstants.Constants[entity.LogicalName][GenericConstants.DocumentBodyAttributeKey]] = GenericConstants.EmptyBodyContent;
						//If the plugin is running in async mode and is for attachment, we have to make a call to save the attachment.
						if (context.Stage == CrmConstants.PostOperationStateNumber && entity.LogicalName == CrmConstants.AttachmentEntityName)
						{
							service.Update(entity);
						}
					}
					else
					{
						throw new InvalidPluginExecutionException(OperationStatus.Suspended, string.Format("The storage provider '{0}' failed to when calling 'Create' method.", configurationProvider.StorageProviderType));
					}
				}
				catch (Exception ex)
				{
					throw new InvalidPluginExecutionException(OperationStatus.Suspended, ex.ToString());
				}
			}
		}
	}
}