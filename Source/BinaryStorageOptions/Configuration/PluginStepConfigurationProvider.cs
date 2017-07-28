using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinaryStorageOptions.Providers;
using Microsoft.Xrm.Sdk;
using System.Xml.Linq;

namespace BinaryStorageOptions.Configuration
{
	public class PluginStepConfigurationProvider : BaseConfigurationProvider, IConfigurationProvider
	{
		private readonly XDocument unsecureDocument;
		private readonly XDocument secureDocument;

		public PluginStepConfigurationProvider(IOrganizationService organizationService, string configurationForEntityType, string unsecurePluginStepConfiguration, string securePluginStepConfiguration) :
			base(organizationService, configurationForEntityType)
		{
			unsecureDocument = XDocument.Parse(unsecurePluginStepConfiguration);
			secureDocument = XDocument.Parse(securePluginStepConfiguration);
		}

		public IConfiguration Configuration
		{
			get
			{
				return GetConfiguration(StorageProviderType, this);
			}
		}

		public StorageProviderType StorageProviderType
		{
			get
			{
				return GetStorageProviderType(GetSettingValue(ProviderTypeKey));
			}
		}

		public CompressionProviderType CompressionProviderType
		{
			get
			{
				return GetCompressionProviderType(GetSettingValue(CompressionTypeKey));
			}
		}

		public EncryptionProviderType EncryptionProviderType
		{
			get
			{
				return GetEncryptionProviderType(GetSettingValue(EncryptionTypeKey));
			}
		}

		public string GetSettingValue(string key)
		{
			XElement setting = unsecureDocument.Descendants("Setting").Where(n => n.Attribute("key").Value == key).FirstOrDefault();
			if (setting == null)
			{
				setting = secureDocument.Descendants("Setting").Where(n => n.Attribute("key").Value == key).FirstOrDefault();
			}
			if (setting == null)
			{
				return null;
			}
			return setting.Attribute("value").Value;
		}
	}
}
