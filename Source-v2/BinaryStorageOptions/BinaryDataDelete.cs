using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryStorageOptions
{
	public class BinaryDataDelete
	{
		protected void Delete(IServiceProvider serviceProvider, string entityName)
		{
			try
			{
				IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
				if (context.MessageName != MessageName.Delete)
					//Invalid event attached
					return;

				IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
				IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
				Configuration.IConfigurationProvider configurationProvider = Configuration.Factory.GetConfigurationProvider(service, entityName);
				if (configurationProvider.StorageProviderType == Providers.BinaryStorageProviderType.CrmDefault)
				{
					//In this case, not doing anything.
					return;
				}

				Providers.IBinaryStorageProvider storageProvider = Providers.Factory.GetStorageProvider(configurationProvider);
				storageProvider.Delete(context.PrimaryEntityId);
			}
			catch (Exception ex)
			{
				throw new InvalidPluginExecutionException(OperationStatus.Suspended, ex.ToString());
			}
		}
	}
}
