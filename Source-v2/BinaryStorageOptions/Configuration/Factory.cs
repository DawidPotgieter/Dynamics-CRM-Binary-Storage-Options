using Microsoft.Xrm.Sdk;

namespace BinaryStorageOptions.Configuration
{
	public static class Factory
	{
		private const string SettingsEntityName = "bso_binarystorageoptionsconfigurationsettin";

		public static IConfigurationProvider GetConfigurationProvider(IOrganizationService organizationService, string entityName)
		{
			return new CrmEntityConfigurationProvider(organizationService, SettingsEntityName, entityName);
		}
	}
}
