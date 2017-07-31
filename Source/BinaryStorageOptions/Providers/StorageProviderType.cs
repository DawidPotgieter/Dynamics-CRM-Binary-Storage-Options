using System.ComponentModel;

namespace BinaryStorageOptions.Providers
{
	public enum StorageProviderType
	{
		CrmDefault = 0,
		AzureBlob = 1,
		AzureFile = 2,
		Proxy = 3,
		[Description("This value can only be used on the Proxy side, won't work in CRM plugin")]
		FileSystem = 4,
	}
}
