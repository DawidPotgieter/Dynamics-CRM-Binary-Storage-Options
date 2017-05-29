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
		private const string PreDeleteImageKey = "PreDelete";

		public void Delete(IServiceProvider serviceProvider)
		{
			try
			{
				IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
				if (context.MessageName != MessageName.Delete)
					//Invalid event attached
					return;

				if (!context.PreEntityImages.ContainsKey(PreDeleteImageKey) || context.PreEntityImages[PreDeleteImageKey] == null)
				{
					throw new InvalidPluginExecutionException(OperationStatus.Failed, string.Format("PreEntityImage could not be found when deleting {0}", context.PrimaryEntityName));
				}
				Entity entity = context.PreEntityImages[PreDeleteImageKey];
				if (entity.LogicalName != CrmConstants.AnnotationEntityName && entity.LogicalName != CrmConstants.AttachmentEntityName)
				{
					//only valid for attachments and annotations
					return;
				}

				IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
				IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

				Configuration.IConfigurationProvider configurationProvider = Configuration.Factory.GetConfigurationProvider(service, entity.LogicalName, unsecurePluginStepConfiguration, securePluginStepConfiguration);
				if (configurationProvider.StorageProviderType == Providers.BinaryStorageProviderType.CrmDefault)
				{
					//In this case, not doing anything for the external file.
					return;
				}

				Providers.IBinaryStorageProvider storageProvider = Providers.Factory.GetStorageProvider(configurationProvider);

				if (entity.Attributes.ContainsKey(GenericConstants.Constants[entity.LogicalName][GenericConstants.FileNameAttributeKey]))
				{
					storageProvider.Delete(entity.Id, (string)entity.Attributes[GenericConstants.Constants[entity.LogicalName][GenericConstants.FileNameAttributeKey]]);
				}
			}
			catch (Exception ex)
			{
				throw new InvalidPluginExecutionException(OperationStatus.Suspended, ex.ToString());
			}
		}
	}
}
