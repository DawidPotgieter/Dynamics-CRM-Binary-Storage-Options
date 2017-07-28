using BinaryStorageOptions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BinaryStorageOptions.Providers
{
	public abstract class AESEncryptionProvider
	{
		private int[] validKeysizes = new int[] { 128, 192, 256 };

		private IConfigurationProvider configurationProvider;
		private int keyBitLength;

		internal AESEncryptionProvider(IConfigurationProvider configurationProvider, int keyBitLength)
		{
			if (!validKeysizes.Contains(keyBitLength))
			{
				throw new ArgumentException("KeyBitLength is invalid.");
			}
			this.configurationProvider = configurationProvider;
			this.keyBitLength = keyBitLength;
		}

		protected abstract string EncryptionConfigurationKey { get; }
		protected abstract string EncryptionConfigurationSaltKey { get; }

		public byte[] Decrypt(byte[] data)
		{
			byte[] realData;
			byte[] IV;
			using (MemoryStream ms = new MemoryStream(data))
			using (BinaryReader br = new BinaryReader(ms))
			{
				int ivLength = br.ReadInt32();
				IV = br.ReadBytes(ivLength);
				realData = br.ReadBytes(data.Length - sizeof(Int32) - IV.Length);
			}
			using (AesManaged aes = new AesManaged { Key = EncryptionKey, IV = IV })
			{
				ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, IV);
				using (MemoryStream ms = new MemoryStream())
				using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
				{
					cs.Write(realData, 0, realData.Length);
					cs.FlushFinalBlock();
					return ms.ToArray();
				}
			}
		}

		public byte[] Encrypt(byte[] data)
		{
			using (AesManaged aes = new AesManaged { Key = EncryptionKey })
			{
				ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
				using (MemoryStream ms = new MemoryStream())
				using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
				{
					cs.Write(data, 0, data.Length);
					cs.FlushFinalBlock();
					return BitConverter.GetBytes(aes.IV.Length).Concat(aes.IV).Concat(ms.ToArray()).ToArray();
				}
			}
		}

		public string Key
		{
			get
			{
				return configurationProvider.GetSettingValue(EncryptionConfigurationKey);
			}
		}

		public string KeySalt
		{
			get
			{
				return configurationProvider.GetSettingValue(EncryptionConfigurationSaltKey);
			}
		}

		private byte[] EncryptionKey
		{
			get
			{
				using (Rfc2898DeriveBytes rfcKey = new Rfc2898DeriveBytes(Key, Salt) { IterationCount = 2048 })
				{
					return rfcKey.GetBytes(keyBitLength / 8);
				}
			}
		}

		private byte[] Salt
		{
			get
			{
				return Encoding.UTF8.GetBytes(KeySalt);
			}
		}
	}
}
