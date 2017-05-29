using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryStorageOptions
{
	public class AttachmentPostCreate : BinaryDataCreate, IPlugin
	{
		public void Execute(IServiceProvider serviceProvider)
		{
			Create(serviceProvider, true, AttachmentDetails.EntityName, AttachmentDetails.DocumentBodyAttributeKey, AttachmentDetails.FileNameAttributeKey);
		}
	}
}
