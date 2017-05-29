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
		public void Create(IServiceProvider serviceProvider)
		{
			IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
			if (context.MessageName != MessageName.Create)
				//Invalid event attached
				return;

			if (context.InputParameters.Contains(CrmConstants.TargetParameterKey) && context.InputParameters[CrmConstants.TargetParameterKey] is Entity)
			{
				try
				{
					Entity entity = (Entity)context.InputParameters[CrmConstants.TargetParameterKey];
					if (entity.LogicalName != CrmConstants.AnnotationEntityName && entity.LogicalName != CrmConstants.AttachmentEntityName)
						return;

					if (!entity.Attributes.Keys.Contains(GenericConstants.Constants[entity.LogicalName][GenericConstants.DocumentBodyAttributeKey]) || string.IsNullOrWhiteSpace((string)entity.Attributes[GenericConstants.Constants[entity.LogicalName][GenericConstants.DocumentBodyAttributeKey]]))
					{
						//No binary data
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

					//This check is actually a bit useless for sandboxed pluxings.  CRM is bugged and won't allow anything bigger than 5mb, the plugin sandbox will just crash.
					if (entity.Attributes.Keys.Contains(CrmConstants.FileSizeKey) && (int)entity.Attributes[CrmConstants.FileSizeKey] > configurationProvider.MaxFileSize)
						throw new InvalidPluginExecutionException(OperationStatus.Failed, string.Format("FileSize Limit of {0} bytes was exceeded.", configurationProvider.MaxFileSize));

					Providers.IBinaryStorageProvider storageProvider = Providers.Factory.GetStorageProvider(configurationProvider);

					byte[] data = Convert.FromBase64String((string)entity.Attributes[GenericConstants.Constants[entity.LogicalName][GenericConstants.DocumentBodyAttributeKey]]);
					string fileName = (string)entity.Attributes[GenericConstants.Constants[entity.LogicalName][GenericConstants.FileNameAttributeKey]];
					if (storageProvider.Create(entity.Id, fileName, data))
					{
						if (UserHasPrivilege(service, context.UserId, string.Format(GenericConstants.AttachmentExtensionPrivilegeTemplate, "Create")))
						{
							try
							{
								//Create an additional record to basically store the filesize
								Entity attachmentExtension = new Entity(GenericConstants.AttachementExtensionEntityName);
								attachmentExtension.Attributes.Add(GenericConstants.AttachementExtensionEntityNameKey, entity.LogicalName);
								attachmentExtension.Attributes.Add(GenericConstants.AttachementExtensionEntityIdKey, entity.Id.ToString());
								attachmentExtension.Attributes.Add(GenericConstants.AttachementExtensionFileSizeKey, data.Length);
								service.Create(attachmentExtension);
							}
							catch
							{
								//In case the user doesn't have access or something is changed, this is only usefull for filesize queries.
							}
						}

						//FileSize (3) attributes are handled by CRM and is useless to set here.  Must have a body else we get funny requests in retrieve messages.
						entity.Attributes[GenericConstants.Constants[entity.LogicalName][GenericConstants.DocumentBodyAttributeKey]] = GenericConstants.EmptyBodyContent;

						//If the plugin is running in async mode, we have to make a call to save the attachment/annotation document body
						if (context.Stage == CrmConstants.PostOperationStateNumber)
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