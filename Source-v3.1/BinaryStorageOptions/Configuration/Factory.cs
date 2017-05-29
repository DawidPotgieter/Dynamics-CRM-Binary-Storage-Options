using Microsoft.Xrm.Sdk;
using System.Xml.Linq;
using System.Linq;
using System;

namespace BinaryStorageOptions.Configuration
{
	public static class Factory
	{
		private const string SettingsEntityName = "bso_binarystorageoptionsconfigurationsettin";

		public static IConfigurationProvider GetConfigurationProvider(IOrganizationService organizationService, string entityName, string unsecurePluginStepConfiguration, string securePluginStepConfiguration)
		{
			try
			{
				if (unsecurePluginStepConfiguration != null)
				{
					XDocument xDocument = XDocument.Parse(unsecurePluginStepConfiguration);
					var configProviderElement = xDocument.Descendants("Setting").Where(s => s.Attribute("key").Value == "ConfigurationProviderType").SingleOrDefault();
					string value = (string)configProviderElement.Attribute("value");
					if (!string.IsNullOrWhiteSpace(value))
					{
						switch (value)
						{
							case "PluginStepConfigurationProvider":
								return new PluginStepConfigurationProvider(organizationService, entityName, unsecurePluginStepConfiguration, securePluginStepConfiguration);
						}
					}
				}
			}
			catch
			{
			}
			return new CrmEntityConfigurationProvider(organizationService, entityName, SettingsEntityName);
		}
	}
}
