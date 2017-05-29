using BinaryStorageOptions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryStorageOptions.Providers
{
	public class BinaryStorageProvider : IBinaryStorageProvider
	{
		private const string OriginalFileSizeKey = "originalfilesize";
		private const string CompressionKey = "compression";
		private const string EncryptionKey = "encryption";
		private const string CompressionThresholdKey = "CompressionThreshold";

		IStorageProvider storageProvider;
		IConfigurationProvider configurationProvider;

		internal BinaryStorageProvider(IConfigurationProvider configurationProvider, IStorageProvider storageProvider)
		{
			this.configurationProvider = configurationProvider;
			this.storageProvider = storageProvider;
		}

		public bool Create(Guid id, string filename, byte[] data)
		{
			ICompressionProvider compressionProvider = Factory.GetCompressionProvider(configurationProvider.CompressionProviderType, configurationProvider);
			IEncryptionProvider encryptionProvider = Factory.GetEncryptionProvider(configurationProvider.EncryptionProviderType, configurationProvider);
			int originalSize = data.Length;
			Dictionary<string, string> metaData = new Dictionary<string, string>();
			byte[] compressedData = compressionProvider.Compress(data);
			if ((double)compressedData.Length / data.Length <= CompressionThreshold)
			{
				metaData.Add(CompressionKey, configurationProvider.CompressionProviderType.ToString());
				data = compressedData;
			}
			data = encryptionProvider.Encrypt(data);

			if (!(encryptionProvider is PassThroughEncryptionProvider))
			{
				metaData.Add(EncryptionKey, configurationProvider.EncryptionProviderType.ToString());
			}

			if (originalSize != data.Length)
			{
				metaData.Add(OriginalFileSizeKey, originalSize.ToString());
			}

			return storageProvider.Create(id, filename, data, metaData);
		}

		public bool Delete(Guid id, string filename)
		{
			return storageProvider.Delete(id, filename);
		}

		public List<string> GetFileNames()
		{
			return storageProvider.GetFileNames();
		}

		public int GetFileSize(Guid id, string filename)
		{
			var metaData = storageProvider.GetMetaData(id, filename, false);
			if (metaData.ContainsKey(OriginalFileSizeKey))
			{
				return int.Parse(metaData[OriginalFileSizeKey]);
			}
			else
			{
				try
				{
					return int.Parse(metaData["Content-Length"]);
				}
				catch { }
			}
			return storageProvider.GetFileSize(id, filename);
		}

		public byte[] Read(Guid id, string filename)
		{
			Dictionary<string, string> metaData;
			byte[] data = storageProvider.Read(id, filename, out metaData);
			if (metaData.ContainsKey(EncryptionKey))
			{
				IEncryptionProvider encryptionProvider = Factory.GetEncryptionProvider((EncryptionProviderType)Enum.Parse(typeof(EncryptionProviderType), metaData[EncryptionKey]), configurationProvider);
				data = encryptionProvider.Decrypt(data);
			}
			if (metaData.ContainsKey(CompressionKey))
			{
				ICompressionProvider compressionProvider = Factory.GetCompressionProvider((CompressionProviderType)Enum.Parse(typeof(CompressionProviderType), metaData[CompressionKey]), configurationProvider);
				data = compressionProvider.Decompress(data);
			}
			return data;
		}

		private double CompressionThreshold
		{
			get
			{
				double threshold = 1;
				try
				{
					threshold = double.Parse(configurationProvider.GetSettingValue(CompressionThresholdKey));
				}
				catch { }
				return threshold;
			}
		}
	}
}
