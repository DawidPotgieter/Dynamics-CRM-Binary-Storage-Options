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
			//If this was a retrievemultiple query, we need to check whether the filesize was referenced and modify the results accordingly
			else if (context.InputParameters.ContainsKey(CrmConstants.Query) && context.OutputParameters.Contains(CrmConstants.BusinessEntityCollectionKey) && context.OutputParameters[CrmConstants.BusinessEntityCollectionKey] is EntityCollection)
			{
				EntityCollection entityCollection = (EntityCollection)context.OutputParameters[CrmConstants.BusinessEntityCollectionKey];
				if (entityCollection.EntityName != CrmConstants.AnnotationEntityName && entityCollection.EntityName != CrmConstants.AttachmentEntityName)
					return;

				QueryExpression query = context.InputParameters[CrmConstants.Query] as QueryExpression;
				if (query != null)
				{
					//For optimization purposes, only return these records after the last page of results and only if totalrecordcountlimit is not exceeded (crm doesn't allow that anyways)
					if (!entityCollection.MoreRecords && !entityCollection.TotalRecordCountLimitExceeded)
					{
						entities.AddRange(entityCollection.Entities);
						//Get a list of all the entity ids to modify and their new file sizes
						var modifyNeeded = GetEntitiesToModifyFileSizes(service, context.UserId, query.Criteria, entityCollection.EntityName);
						foreach (var mod in modifyNeeded)
						{
							var entity = entities.FirstOrDefault(e => e.Id == mod.Key);
							if (entity == null)
							{
								//Need to load the entity with the same colums as any other entities
								entity = service.Retrieve(entityCollection.EntityName, mod.Key, new ColumnSet(entities.First().Attributes.Keys.ToArray()));
								entities.Add(entity);
							}
							if (entity != null)
							{
								if (entity.Attributes.ContainsKey(CrmConstants.FileSizeKey))
								{
									entity.Attributes[CrmConstants.FileSizeKey] = mod.Value;
								}
							}
						}
						if (entities.Count > 0)
						{
							((EntityCollection)context.OutputParameters[CrmConstants.BusinessEntityCollectionKey]).Entities.Clear();
							((EntityCollection)context.OutputParameters[CrmConstants.BusinessEntityCollectionKey]).Entities.AddRange(entities);
							return;
						}
					}
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
				if (configurationProvider.StorageProviderType == Providers.BinaryStorageProviderType.CrmDefault)
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

		private Dictionary<Guid, int> GetEntitiesToModifyFileSizes(IOrganizationService service, Guid userId, FilterExpression expression, string entityName)
		{
			Dictionary<Guid, int> entities = new Dictionary<Guid, int>();
			if (expression != null)
			{
				foreach (var filterExpression in expression.Filters)
				{
					var results = GetEntitiesToModifyFileSizes(service, userId, expression, entityName);
					foreach (var result in results)
					{
						if (!entities.ContainsKey(result.Key))
							entities.Add(result.Key, result.Value);
					}
				}
				foreach (var condition in expression.Conditions)
				{
					if (condition.AttributeName == CrmConstants.FileSizeKey)
					{
						DataCollection<Entity> changeEntities = null;
						if (UserHasPrivilege(service, userId, string.Format(GenericConstants.AttachmentExtensionPrivilegeTemplate, "Read")))
						{
							try
							{
								QueryExpression query = new QueryExpression(GenericConstants.AttachementExtensionEntityName);
								query.ColumnSet = new ColumnSet(true);
								query.Criteria = new FilterExpression(LogicalOperator.And);
								query.Criteria.AddCondition(GenericConstants.AttachementExtensionEntityNameKey, ConditionOperator.Equal, entityName);
								query.Criteria.AddCondition(GenericConstants.AttachementExtensionFileSizeKey, condition.Operator, condition.Values.ToArray());

								changeEntities = service.RetrieveMultiple(query).Entities;
							}
							catch { }
						}
						if (changeEntities != null)
						{
							foreach (var entity in changeEntities)
							{
								Guid g = new Guid((string)entity.Attributes[GenericConstants.AttachementExtensionEntityIdKey]);
								if (!entities.ContainsKey(g))
								{
									entities.Add(g, (int)entity.Attributes[GenericConstants.AttachementExtensionFileSizeKey]);
								}
							}
						}
					}
				}
			}
			return entities;
		}

		private void HandleEntity(IOrganizationService service, Guid userId, Providers.IBinaryStorageProvider storageProvider, Entity entity, string documentBodyAttributeKey, string fileNameAttributeKey)
		{
			try
			{
				if (!(entity.Attributes.ContainsKey(CrmConstants.FileSizeKey) || entity.Attributes.ContainsKey(documentBodyAttributeKey)))
				{
					//If none of these columns are queried, we don't have to do anything, as it will just return whatever is in base tables.
					return;
				}

				Entity attachementExtensionEntity = null;
				if (UserHasPrivilege(service, userId, string.Format(GenericConstants.AttachmentExtensionPrivilegeTemplate, "Read")))
				{
					try
					{
						//If the file size is requested it's currently equal to the "empty body" length, need to redirect the value
						//Retrieve the attachment extension record
						var query = new QueryExpression(GenericConstants.AttachementExtensionEntityName);
						query.ColumnSet = new ColumnSet(GenericConstants.AttachementExtensionFileSizeKey);
						query.Criteria = new FilterExpression(LogicalOperator.And);
						query.Criteria.AddCondition(GenericConstants.AttachementExtensionEntityNameKey, ConditionOperator.Equal, entity.LogicalName);
						query.Criteria.AddCondition(GenericConstants.AttachementExtensionEntityIdKey, ConditionOperator.Equal, entity.Id.ToString());
						attachementExtensionEntity = service.RetrieveMultiple(query).Entities.SingleOrDefault();
					}
					catch
					{
					}
				}

				if (entity.Attributes.ContainsKey(CrmConstants.FileSizeKey) && ((int)entity.Attributes[CrmConstants.FileSizeKey] == GenericConstants.EmptyBodyContentDataLength))
				{
					if (attachementExtensionEntity != null)
					{
						//Return the filesize from the attachments table, since crm automatically sets it to 3 when setting "empty" body contents
						entity.Attributes[CrmConstants.FileSizeKey] = attachementExtensionEntity.Attributes[GenericConstants.AttachementExtensionFileSizeKey];
					}
					else
					{
						try
						{
							entity.Attributes[CrmConstants.FileSizeKey] = storageProvider.GetFileSize(entity.Id, (string)entity.Attributes[fileNameAttributeKey]);
						}
						catch { }
					}
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
				throw new InvalidPluginExecutionException(OperationStatus.Failed, ex.ToString());
			}
		}
	}
}
