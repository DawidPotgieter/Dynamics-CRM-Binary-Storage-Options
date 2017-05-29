using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryStorageOptions.Providers
{
	public class PassThroughEncryptionProvider : IEncryptionProvider
	{
		public string Key
		{
			get
			{
				return null;
			}
		}

		public string KeySalt
		{
			get
			{
				return null;
			}
		}

		public byte[] Decrypt(byte[] data)
		{
			return data;
		}

		public byte[] Encrypt(byte[] data)
		{
			return data;
		}
	}
}
