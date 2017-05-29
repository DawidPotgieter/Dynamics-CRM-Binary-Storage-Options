using BinaryStorageOptions.Providers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryStorageOptions.Configuration
{
	public class BaseConfigurationProvider
	{
		private const int DefaultMaxFileSize = 1024 * 1024 * 5; //Default CRM value
		protected const string ProviderTypeKey = "Provider Type";
		protected IOrganizationService organizationService;
		protected string configurationForEntityType;

		public BaseConfigurationProvider(IOrganizationService organizationService, string configurationForEntityType)
		{
			this.organizationService = organizationService;
			this.configurationForEntityType = configurationForEntityType;
		}

		protected IConfiguration GetConfiguration(BinaryStorageProviderType storageProviderType, IConfigurationProvider configProvider)
		{
			switch (storageProviderType)
			{
				case BinaryStorageProviderType.AzureBlob:
					return new AzureBlobStorageConfiguration(configProvider, configurationForEntityType);
				case BinaryStorageProviderType.AzureFile:
					return new AzureFileStorageConfiguration(configProvider, configurationForEntityType);
			}
			return null;
		}

		protected BinaryStorageProviderType GetStorageProviderType(string storageProviderValue)
		{
			BinaryStorageProviderType providerType = BinaryStorageProviderType.CrmDefault;
			try
			{
				providerType = (BinaryStorageProviderType)Enum.Parse(typeof(BinaryStorageProviderType), storageProviderValue);
			}
			catch
			{
			}
			return providerType;
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

		private int GetAttachmentSizeLimit()
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
