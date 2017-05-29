using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using BinaryStorageOptions.Providers;
using System.Collections.Generic;

namespace BinaryStorageOptions.Configuration
{
	public class CrmEntityConfigurationProvider : IConfigurationProvider
	{
		private const string KeyColumnName = "bso_key";
		private const string ValueColumnName = "bso_value";
		private const string ProviderTypeKey = "Provider Type";
		private const int DefaultMaxFileSize = 1024 * 1024 * 5; //Default CRM value

		private IOrganizationService organizationService;
		private string settingsEntityName;
		private string configurationForEntityType;

		public CrmEntityConfigurationProvider(IOrganizationService organizationService, string settingsEntityName, string configurationForEntityType)
		{
			this.organizationService = organizationService;
			this.settingsEntityName = settingsEntityName;
			this.configurationForEntityType = configurationForEntityType;
		}

		public IConfiguration Configuration
		{
			get
			{
				switch (StorageProviderType)
				{
					case BinaryStorageProviderType.AzureBlob:
						return new AzureBlobStorageConfiguration(this, configurationForEntityType);
					case BinaryStorageProviderType.AzureFile:
						return new AzureFileStorageConfiguration(this, configurationForEntityType);
				}
				return null;
			}
		}

		public BinaryStorageProviderType StorageProviderType
		{
			get
			{
				BinaryStorageProviderType providerType = BinaryStorageProviderType.CrmDefault;
				try
				{
					providerType = (BinaryStorageProviderType)Enum.Parse(typeof(BinaryStorageProviderType), GetSettingValue(ProviderTypeKey));
				}
				catch
				{
				}
				return providerType;
			}
		}

		private int? maxFileSize;
		public int MaxFileSize
		{
			get
			{
				if (!maxFileSize.HasValue)
				{
					try
					{
						maxFileSize = GetAttachmentSizeLimit();
					}
					catch
					{
						maxFileSize = DefaultMaxFileSize;
					}
				}
				return maxFileSize.Value;
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

		public int GetAttachmentSizeLimit()
		{
			var size = -1;
			var query = new QueryExpression("organization")
			{
				ColumnSet = new ColumnSet("maxuploadfilesize")
			};

			var results = organizationService.RetrieveMultiple(query);
			if (results.Entities.Count == 0)
			{
				return size;
			}
			return results[0].GetAttributeValue<int>("maxuploadfilesize");
		}
	}
}
