using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryStorageOptions
{
	public class AttachmentDelete : BinaryDataDelete, IPlugin
	{
		public void Execute(IServiceProvider serviceProvider)
		{
			Delete(serviceProvider, AttachmentDetails.EntityName);
		}
	}
}
