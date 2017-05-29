using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryStorageOptions
{
	public class BinaryDataCreate
	{
		protected void Create(IServiceProvider serviceProvider, bool saveEntity, string entityName, string documentBodyAttributeKey, string fileNameAttributeKey)
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
					if (entity.LogicalName != entityName)
						return;

					if (!entity.Attributes.Keys.Contains(documentBodyAttributeKey) || string.IsNullOrWhiteSpace((string)entity.Attributes[documentBodyAttributeKey]))
					{
						//No binary data
						return;
					}

					IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
					IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

					Configuration.IConfigurationProvider configurationProvider = Configuration.Factory.GetConfigurationProvider(service, entityName);
					if (configurationProvider.StorageProviderType == Providers.BinaryStorageProviderType.CrmDefault)
					{
						//In this case, not doing anything with the binary data.
						return;
					}

					//This check is actually a bit useless for sandboxed pluxings.  CRM is bugged and won't allow anything bigger than 5mb, the plugin sandbox will just crash.
					if (entity.Attributes.Keys.Contains(CrmConstants.FileSizeKey) && (int)entity.Attributes[CrmConstants.FileSizeKey] > configurationProvider.MaxFileSize)
						throw new InvalidPluginExecutionException(OperationStatus.Failed, string.Format("FileSize Limit of {0} bytes was exceeded.", configurationProvider.MaxFileSize));

					Providers.IBinaryStorageProvider storageProvider = Providers.Factory.GetStorageProvider(configurationProvider);

					byte[] data = Convert.FromBase64String((string)entity.Attributes[documentBodyAttributeKey]);
					string fileName = (string)entity.Attributes[fileNameAttributeKey];
					string newFileName = string.Format("{0}{1}", Constants.ExternalFilePrefix, fileName);

					if (storageProvider.Create(entity.Id, fileName, data))
					{
						entity.Attributes[fileNameAttributeKey] = newFileName;
						entity.Attributes[documentBodyAttributeKey] = Constants.EmptyBodyContent;
						if (saveEntity)
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
