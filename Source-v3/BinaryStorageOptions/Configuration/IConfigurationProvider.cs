using BinaryStorageOptions.Providers;

namespace BinaryStorageOptions.Configuration
{
	public interface IConfigurationProvider
	{
		IConfiguration Configuration { get; }
		BinaryStorageProviderType StorageProviderType { get; }
		string GetSettingValue(string key);
		int MaxFileSize { get; }
	}
}
