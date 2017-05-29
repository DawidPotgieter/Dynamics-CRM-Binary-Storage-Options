using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryStorageOptions.Providers
{
	public interface IEncryptionProvider
	{
		byte[] Encrypt(byte[] data);
		byte[] Decrypt(byte[] data);
		string Key { get; }
		string KeySalt { get; }
	}
}
