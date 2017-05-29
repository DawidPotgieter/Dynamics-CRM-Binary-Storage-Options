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
	public class AES256EncryptionProvider : AESEncryptionProvider, IEncryptionProvider
	{
		internal AES256EncryptionProvider(IConfigurationProvider configurationProvider)
			: base(configurationProvider, 256)
		{
		}

		protected override string EncryptionConfigurationKey
		{
			get
			{
				return "AES256EncryptionKey";
			}
		}

		protected override string EncryptionConfigurationSaltKey
		{
			get
			{
				return "AES256EncryptionKeySalt";
			}
		}
	}
}
