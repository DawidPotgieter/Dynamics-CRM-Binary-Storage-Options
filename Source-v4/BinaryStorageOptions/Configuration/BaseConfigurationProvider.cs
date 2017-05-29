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
		protected const string CompressionTypeKey = "Compression";
		protected const string EncryptionTypeKey = "Encryption";
		protected IOrganizationService organizationService;
		protected string configurationForEntityType;

		public BaseConfigurationProvider(IOrganizationService organizationService, string configurationForEntityType)
		{
			this.organizationService = organizationService;
			this.configurationForEntityType = configurationForEntityType;
		}

		protected IConfiguration GetConfiguration(StorageProviderType storageProviderType, IConfigurationProvider configProvider)
		{
			switch (storageProviderType)
			{
				case StorageProviderType.AzureBlob:
					return new AzureBlobStorageConfiguration(configProvider, configurationForEntityType);
				case StorageProviderType.AzureFile:
					return new AzureFileStorageConfiguration(configProvider, configurationForEntityType);
			}
			return null;
		}

		protected StorageProviderType GetStorageProviderType(string storageProviderValue)
		{
			StorageProviderType providerType = StorageProviderType.CrmDefault;
			try
			{
				providerType = (StorageProviderType)Enum.Parse(typeof(StorageProviderType), storageProviderValue);
			}
			catch
			{
			}
			return providerType;
		}

		protected CompressionProviderType GetCompressionProviderType(string compressionProviderValue)
		{
			CompressionProviderType providerType = CompressionProviderType.PassThrough;
			try
			{
				providerType = (CompressionProviderType)Enum.Parse(typeof(CompressionProviderType), compressionProviderValue);
			}
			catch
			{
			}
			return providerType;
		}

		protected EncryptionProviderType GetEncryptionProviderType(string encryptionProviderValue)
		{
			EncryptionProviderType providerType = EncryptionProviderType.PassThrough;
			try
			{
				providerType = (EncryptionProviderType)Enum.Parse(typeof(EncryptionProviderType), encryptionProviderValue);
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
