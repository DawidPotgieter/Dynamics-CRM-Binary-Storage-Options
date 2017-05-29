using System;
using Microsoft.Xrm.Sdk;

namespace BinaryStorageOptions
{
	public class AttachmentPreCreate : BinaryDataCreate, IPlugin
	{
		public virtual void Execute(IServiceProvider serviceProvider)
		{
			Create(serviceProvider, false, AttachmentDetails.EntityName, AttachmentDetails.DocumentBodyAttributeKey, AttachmentDetails.FileNameAttributeKey);
		}
	}
}
