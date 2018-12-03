using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
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
		public void Retrieve(IServiceProvider serviceProvider)
		{
			IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
			if (context.MessageName != MessageName.Retrieve && context.MessageName != MessageName.RetrieveMultiple)
				//Invalid event attached
				return;

			IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
			IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

			List<Entity> entities = new List<Entity>();

			//This case handles where IOrganizationService is called with "LoadProperty".  For this, you need to attach this plugin to the "Retrieve" method of the parent Entity.
			if (context.OutputParameters.ContainsKey(CrmConstants.BusinessEntityKey) &&
					context.OutputParameters[CrmConstants.BusinessEntityKey] is Entity &&
					((Entity)context.OutputParameters[CrmConstants.BusinessEntityKey]).RelatedEntities != null &&
					((Entity)context.OutputParameters[CrmConstants.BusinessEntityKey]).RelatedEntities.Values is ICollection<EntityCollection> &&
					((Entity)context.OutputParameters[CrmConstants.BusinessEntityKey]).RelatedEntities.Values.Count > 0)
			{
				foreach (EntityCollection entityCollection in ((Entity)context.OutputParameters[CrmConstants.BusinessEntityKey]).RelatedEntities.Values.Where(ec => ec.EntityName == CrmConstants.AnnotationEntityName || ec.EntityName == CrmConstants.AttachmentEntityName))
				{
					entities.AddRange(entityCollection.Entities);
				}
			}
			else if (context.MessageName == MessageName.Retrieve && context.OutputParameters.Contains(CrmConstants.BusinessEntityKey) && context.OutputParameters[CrmConstants.BusinessEntityKey] is Entity)
			{
				//Handles retrieve message
				Entity entity = (Entity)context.OutputParameters[CrmConstants.BusinessEntityKey];
				if (entity.LogicalName != CrmConstants.AnnotationEntityName && entity.LogicalName != CrmConstants.AttachmentEntityName)
					return;
				entities.Add(entity);
			}
			else if (context.MessageName == MessageName.RetrieveMultiple && context.OutputParameters.Contains(CrmConstants.BusinessEntityCollectionKey) && context.OutputParameters[CrmConstants.BusinessEntityCollectionKey] is EntityCollection)
			{
				EntityCollection entityCollection = (EntityCollection)context.OutputParameters[CrmConstants.BusinessEntityCollectionKey];
				if (entityCollection.TotalRecordCount > 0 && entityCollection.EntityName != CrmConstants.AnnotationEntityName && entityCollection.EntityName != CrmConstants.AttachmentEntityName)
					return;
				entities.AddRange(entityCollection.Entities);
			}

			if (entities.Count > 0)
			{
				Configuration.IConfigurationProvider configurationProvider = Configuration.Factory.GetConfigurationProvider(service, entities[0].LogicalName, unsecurePluginStepConfiguration, securePluginStepConfiguration);
				if (configurationProvider.StorageProviderType == Providers.StorageProviderType.CrmDefault)
				{
					//In this case, not doing anything.
					return;
				}

				Providers.IBinaryStorageProvider storageProvider = Providers.Factory.GetStorageProvider(configurationProvider);
				entities.ForEach(e => 
					HandleEntity(service, context.UserId, storageProvider, e, GenericConstants.Constants[e.LogicalName][GenericConstants.DocumentBodyAttributeKey], GenericConstants.Constants[e.LogicalName][GenericConstants.FileNameAttributeKey])
				);
			}
		}

		private void HandleEntity(IOrganizationService service, Guid userId, Providers.IBinaryStorageProvider storageProvider, Entity entity, string documentBodyAttributeKey, string fileNameAttributeKey)
		{
			try
			{
				if (!(entity.Attributes.ContainsKey(documentBodyAttributeKey) || entity.Attributes.ContainsKey(CrmConstants.FileSizeKey)))
				{
					//If none of these columns are queried, we don't have to do anything, as it will just return whatever is in base tables.
					return;
				}

				if (entity.Attributes.ContainsKey(CrmConstants.FileSizeKey) &&
						(int)entity.Attributes[CrmConstants.FileSizeKey] == GenericConstants.EmptyBodyContentDataLength &&
						entity.Attributes.ContainsKey(fileNameAttributeKey))
				{
					entity.Attributes[CrmConstants.FileSizeKey] = storageProvider.GetFileSize(entity.Id, (string)entity.Attributes[fileNameAttributeKey]);
				}

				if (entity.Attributes.ContainsKey(documentBodyAttributeKey) && 
						(string)entity.Attributes[documentBodyAttributeKey] == GenericConstants.EmptyBodyContent &&
						entity.Attributes.ContainsKey(fileNameAttributeKey))
				{
					//If the body is requested, go fetch it and populate it where CRM wants it
					byte[] data = storageProvider.Read(entity.Id, (string)entity.Attributes[fileNameAttributeKey]);
					entity.Attributes[documentBodyAttributeKey] = Convert.ToBase64String(data);
				}
			}
			catch (Exception ex)
			{
				if (entity.LogicalName == CrmConstants.AttachmentEntityName)
				{
					//Search for other attacments pointing to the same attachment id (not the activitymimeattachment).
					var otherData = SearchForAttachment(service, storageProvider, entity, documentBodyAttributeKey, fileNameAttributeKey);
					if (otherData != null && entity.Attributes.ContainsKey(fileNameAttributeKey))
					{
						if (entity.Attributes.ContainsKey(CrmConstants.FileSizeKey) &&
								(int)entity.Attributes[CrmConstants.FileSizeKey] == GenericConstants.EmptyBodyContentDataLength)
						{
							entity.Attributes[CrmConstants.FileSizeKey] = otherData.Item1;
						}

						if (entity.Attributes.ContainsKey(documentBodyAttributeKey) && 
								(string)entity.Attributes[documentBodyAttributeKey] == GenericConstants.EmptyBodyContent)
						{
							entity.Attributes[documentBodyAttributeKey] = Convert.ToBase64String(otherData.Item2);
						}
						return;
					}
				}

				throw new InvalidPluginExecutionException(OperationStatus.Failed, ex.ToString());
			}
		}

		private Tuple<int, byte[]> SearchForAttachment(IOrganizationService service, Providers.IBinaryStorageProvider storageProvider, Entity entity, string documentBodyAttributeKey, string fileNameAttributeKey)
		{
			//This is a little bit of a hack, but there really isn't very many good options.
			//When there's an error loading an email attachment, there's a good chance that crm is requesting a duplicate of an existing attachment, as that's how it seems to be stored.
			//So, go through the list of activitymimeattachments pointing to the same attachment id, and see if you can get the binary data using that.
			using (OrganizationServiceContext context = new OrganizationServiceContext(service))
			{
				var attachmentId = ((EntityReference)(from ama in context.CreateQuery(CrmConstants.AttachmentEntityName)
																							where ((EntityReference)ama[CrmConstants.AttachmentEntityId]).Id == entity.Id
																							select ama[CrmConstants.AttachmentId]).FirstOrDefault()).Id;
				var activitymimeattachmentIds = (from ama in context.CreateQuery(CrmConstants.AttachmentEntityName)
																				 where (Guid)ama[CrmConstants.AttachmentId] == attachmentId
																				 select ama[CrmConstants.AttachmentEntityId])
																				  .ToList();

				foreach (Guid activitymimeattachmentId in activitymimeattachmentIds)
				{
					try
					{
						int fileSize = storageProvider.GetFileSize(activitymimeattachmentId, (string)entity.Attributes[fileNameAttributeKey]);
						byte[] data = storageProvider.Read(activitymimeattachmentId, (string)entity.Attributes[fileNameAttributeKey]);

						return new Tuple<int, byte[]>(fileSize, data);
					}
					catch { }
				}
			}
			return null;
		}
	}
}
