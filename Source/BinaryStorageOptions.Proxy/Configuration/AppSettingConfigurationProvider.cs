using BinaryStorageOptions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BinaryStorageOptions.Providers;
using System.Web.Configuration;

namespace BinaryStorageOptions.Proxy.Configuration
{
	public class AppSettingConfigurationProvider : IConfigurationProvider
	{
		private const int DefaultMaxFileSize = 1024 * 1024 * 5; //Default CRM value
		private const string ProviderTypeKey = "Provider Type";
		private const string CompressionTypeKey = "Compression";
		private const string EncryptionTypeKey = "Encryption";

		private string configurationForEntityType;

		public AppSettingConfigurationProvider(string configurationForEntityType)
		{
			this.configurationForEntityType = configurationForEntityType;
		}

		public IConfiguration Configuration
		{
			get
			{
				switch (StorageProviderType)
				{
					case StorageProviderType.AzureBlob:
						return new AzureBlobStorageConfiguration(this, configurationForEntityType);
					case StorageProviderType.AzureFile:
						return new AzureFileStorageConfiguration(this, configurationForEntityType);
					case StorageProviderType.FileSystem:
						return new FileSystemStorageConfiguration(this, configurationForEntityType);
				}
				return null;
			}
		}

		public StorageProviderType StorageProviderType
		{
			get
			{
				StorageProviderType providerType = StorageProviderType.FileSystem;
				try
				{
					providerType = (StorageProviderType)Enum.Parse(typeof(StorageProviderType), GetSettingValue(ProviderTypeKey));
				}
				catch
				{
				}
				return providerType;
			}
		}

		public CompressionProviderType CompressionProviderType
		{
			get
			{
				CompressionProviderType providerType = CompressionProviderType.PassThrough;
				try
				{
					providerType = (CompressionProviderType)Enum.Parse(typeof(CompressionProviderType), GetSettingValue(CompressionTypeKey));
				}
				catch
				{
				}
				return providerType;
			}
		}

		public EncryptionProviderType EncryptionProviderType
		{
			get
			{
				EncryptionProviderType providerType = EncryptionProviderType.PassThrough;
				try
				{
					providerType = (EncryptionProviderType)Enum.Parse(typeof(EncryptionProviderType), GetSettingValue(EncryptionTypeKey));
				}
				catch
				{
				}
				return providerType;
			}
		}

		public int MaxFileSize => DefaultMaxFileSize;

		public string GetSettingValue(string key)
		{
			return WebConfigurationManager.AppSettings[key];
		}
	}
}