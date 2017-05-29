using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryStorageOptions
{
	public class BinaryDataRetrieve
	{
		protected void Retrieve(IServiceProvider serviceProvider, string entityName, string documentBodyAttributeKey, string fileNameAttributeKey)
		{
			IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
			if (context.MessageName != MessageName.Retrieve && context.MessageName != MessageName.RetrieveMultiple)
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

			//This case handles where IOrganizationService is called with "LoadProperty".  For this, you need to attach this plugin to the "Retrieve" method of the parent Entity.
			if (context.OutputParameters.ContainsKey(CrmConstants.BusinessEntityKey) &&
					context.OutputParameters[CrmConstants.BusinessEntityKey] is Entity &&
					((Entity)context.OutputParameters[CrmConstants.BusinessEntityKey]).RelatedEntities != null &&
					((Entity)context.OutputParameters[CrmConstants.BusinessEntityKey]).RelatedEntities.Values is ICollection<EntityCollection> &&
					((Entity)context.OutputParameters[CrmConstants.BusinessEntityKey]).RelatedEntities.Values.Count > 0)
			{
				foreach (EntityCollection entityCollection in ((Entity)context.OutputParameters[CrmConstants.BusinessEntityKey]).RelatedEntities.Values.Where(ec => ec.EntityName == entityName))
				{
					entityCollection.Entities.ToList().ForEach(e => HandleEntity(storageProvider, e, documentBodyAttributeKey, fileNameAttributeKey));
				}
			}
			else if (context.MessageName == MessageName.Retrieve && context.OutputParameters.Contains(CrmConstants.BusinessEntityKey) && context.OutputParameters[CrmConstants.BusinessEntityKey] is Entity)
			{
				Entity entity = (Entity)context.OutputParameters[CrmConstants.BusinessEntityKey];
				if (entity.LogicalName != entityName)
					return;
				HandleEntity(storageProvider, entity, documentBodyAttributeKey, fileNameAttributeKey);
			}
			else if (context.MessageName == MessageName.RetrieveMultiple && context.OutputParameters.Contains(CrmConstants.BusinessEntityCollectionKey) && context.OutputParameters[CrmConstants.BusinessEntityCollectionKey] is EntityCollection)
			{
				EntityCollection entityCollection = (EntityCollection)context.OutputParameters[CrmConstants.BusinessEntityCollectionKey];
				if (entityCollection.TotalRecordCount > 0 && entityCollection.Entities.First().LogicalName != entityName)
					return;
				entityCollection.Entities.ToList().ForEach(e => HandleEntity(storageProvider, e, documentBodyAttributeKey, fileNameAttributeKey));
			}
		}

		private void HandleEntity(Providers.IBinaryStorageProvider storageProvider, Entity entity, string documentBodyAttributeKey, string fileNameAttributeKey)
		{
			try
			{
				if (entity.Attributes.Keys.Contains(fileNameAttributeKey) && ((string)entity.Attributes[fileNameAttributeKey]).StartsWith(Constants.ExternalFilePrefix))
				{
					byte[] data = null;
					string actualFileName = ((string)entity.Attributes[fileNameAttributeKey]).Substring(Constants.ExternalFilePrefix.Length);
					entity.Attributes[fileNameAttributeKey] = actualFileName;
					if (entity.Attributes.Keys.Contains(documentBodyAttributeKey) && (string)entity.Attributes[documentBodyAttributeKey] == Constants.EmptyBodyContent)
					{
						data = storageProvider.Read(entity.Id, actualFileName);
						entity.Attributes[documentBodyAttributeKey] = Convert.ToBase64String(data);
					}
					if (entity.Attributes.Keys.Contains(CrmConstants.FileSizeKey))
					{
						if (data != null)
						{
							entity.Attributes[CrmConstants.FileSizeKey] = data.Length;
						}
						else
						{
							entity.Attributes[CrmConstants.FileSizeKey] = storageProvider.GetFileSize(entity.Id, actualFileName);
						}
					}
					if (entity.Attributes.Keys.Contains(CrmConstants.IsDocumentKey))
					{
						if (entity.Attributes.Keys.Contains(CrmConstants.FileSizeKey))
						{
							entity.Attributes[CrmConstants.IsDocumentKey] = ((int?)entity.Attributes[CrmConstants.FileSizeKey]) > 0;
						}
						else
						{
							entity.Attributes[CrmConstants.IsDocumentKey] = false;
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new InvalidPluginExecutionException(OperationStatus.Failed, ex.ToString());
			}
		}
	}
}
