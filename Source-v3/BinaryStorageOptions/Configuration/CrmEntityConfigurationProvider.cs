using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using BinaryStorageOptions.Providers;
using System.Collections.Generic;

namespace BinaryStorageOptions.Configuration
{
	public class CrmEntityConfigurationProvider : BaseConfigurationProvider, IConfigurationProvider
	{
		private const string KeyColumnName = "bso_key";
		private const string ValueColumnName = "bso_value";

		private string settingsEntityName;

		public CrmEntityConfigurationProvider(IOrganizationService organizationService, string configurationForEntityType, string settingsEntityName) :
			base(organizationService, configurationForEntityType)
		{
			this.settingsEntityName = settingsEntityName;
		}

		public IConfiguration Configuration
		{
			get
			{
				return GetConfiguration(StorageProviderType, this);
			}
		}

		public BinaryStorageProviderType StorageProviderType
		{
			get
			{
				return GetStorageProviderType(GetSettingValue(ProviderTypeKey));
			}
		}

		public string GetSettingValue(string key)
		{
			if (settings == null)
				LoadAllSettings();
			return settings[key];
		}

		private Dictionary<string, string> settings = null;
		private void LoadAllSettings()
		{
			var crmSettings = organizationService.RetrieveMultiple(new QueryExpression
			{
				EntityName = settingsEntityName,
				ColumnSet = new ColumnSet(true),
			}).Entities;
			settings = new Dictionary<string, string>();
			foreach (var entity in crmSettings)
			{
				settings.Add((string)entity[KeyColumnName], (string)entity[ValueColumnName]);
			}
		}
	}
}
