using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryStorageOptions
{
	public partial class Plugin : IPlugin
	{
		private readonly string unsecurePluginStepConfiguration;
		private readonly string securePluginStepConfiguration;

		public Plugin(string unsecurePluginStepConfiguration, string securePluginStepConfiguration)
		{
			this.unsecurePluginStepConfiguration = unsecurePluginStepConfiguration;
			this.securePluginStepConfiguration = securePluginStepConfiguration;
		}

		public void Execute(IServiceProvider serviceProvider)
		{
			IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

			switch (context.MessageName)
			{
				case MessageName.Create:
					Create(serviceProvider);
					break;
				case MessageName.Delete:
					Delete(serviceProvider);
					break;
				case MessageName.Retrieve:
				case MessageName.RetrieveMultiple:
					Retrieve(serviceProvider);
					break;
			}
		}

		private bool UserHasPrivilege(IOrganizationService service, Guid userId, string privilegeName)
		{
			bool userHasPrivilege = false;

			try
			{
				QueryExpression privilegeQuery = new QueryExpression("privilege");
				privilegeQuery.ColumnSet = new ColumnSet(true);
				LinkEntity privilegeLink1 = new LinkEntity("privilege", "roleprivileges", "privilegeid", "privilegeid", JoinOperator.Inner);
				LinkEntity privilegeLink2 = new LinkEntity("roleprivileges", "role", "roleid", "roleid", JoinOperator.Inner);
				LinkEntity privilegeLink3 = new LinkEntity("role", "systemuserroles", "roleid", "roleid", JoinOperator.Inner);
				LinkEntity privilegeLink4 = new LinkEntity("systemuserroles", "systemuser", "systemuserid", "systemuserid", JoinOperator.Inner);

				ConditionExpression userCondition = new ConditionExpression("systemuserid", ConditionOperator.Equal, userId); // // Id of the User
				ConditionExpression privilegeCondition = new ConditionExpression("name", ConditionOperator.Equal, privilegeName); // name of the privilege

				privilegeLink4.LinkCriteria.AddCondition(userCondition);
				FilterExpression privilegeFilter = new FilterExpression(LogicalOperator.And);
				privilegeFilter.Conditions.Add(privilegeCondition);
				privilegeQuery.Criteria = privilegeFilter;

				privilegeLink3.LinkEntities.Add(privilegeLink4);
				privilegeLink2.LinkEntities.Add(privilegeLink3);
				privilegeLink1.LinkEntities.Add(privilegeLink2);
				privilegeQuery.LinkEntities.Add(privilegeLink1);

				EntityCollection retrievedPrivileges = service.RetrieveMultiple(privilegeQuery);
				if (retrievedPrivileges.Entities.Count > 0) { userHasPrivilege = true; }
			}
			catch { }

			return userHasPrivilege;
		}
	}
}
