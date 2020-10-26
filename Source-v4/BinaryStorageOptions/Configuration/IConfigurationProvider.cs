using BinaryStorageOptions.Providers;

namespace BinaryStorageOptions.Configuration
{
	public interface IConfigurationProvider
	{
		IConfiguration Configuration { get; }
		StorageProviderType StorageProviderType { get; }
		string StorageProviderEndpointSuffix { get; }
		CompressionProviderType CompressionProviderType { get; }
		EncryptionProviderType EncryptionProviderType { get; }
		string GetSettingValue(string key);
		int MaxFileSize { get; }
	}
}
