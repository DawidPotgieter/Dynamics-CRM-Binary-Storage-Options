using BinaryStorageOptions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryStorageOptions.Providers
{
	public class ZipCompressionProvider : ICompressionProvider
	{
		private const string CompressionLevelKey = "ZipCompressionLevel";

		private IConfigurationProvider configurationProvider;

		internal ZipCompressionProvider(IConfigurationProvider configurationProvider)
		{
			this.configurationProvider = configurationProvider;
		}

		public byte[] Compress(byte[] data)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				using (ZipArchive archive = new ZipArchive(ms, ZipArchiveMode.Create))
				{
					ZipArchiveEntry entry = archive.CreateEntry("data", CompressionLevel);
					using (var streamWriter = new BinaryWriter(entry.Open()))
					{
						streamWriter.Write(data);
					}
				}
				return ms.ToArray();
			}
		}

		public byte[] Decompress(byte[] data)
		{
			using (MemoryStream ms = new MemoryStream(data))
			using (ZipArchive archive = new ZipArchive(ms, ZipArchiveMode.Read))
			{
				ZipArchiveEntry entry = archive.Entries.Single();
				using (var streamReader = new BinaryReader(entry.Open()))
				{
					return streamReader.ReadBytes((int)entry.Length);
				}
			}
		}

		public CompressionLevel CompressionLevel
		{
			get
			{
				CompressionLevel compressionLevel = CompressionLevel.Optimal;
				try
				{
					compressionLevel = (CompressionLevel)Enum.Parse(typeof(CompressionLevel), configurationProvider.GetSettingValue(CompressionLevelKey));
				}
				catch { }
				return compressionLevel;
			}
		}
	}
}
