using System;
using Microsoft.Xrm.Sdk;

namespace BinaryStorageOptions
{
	public class AnnotationPreCreate : BinaryDataCreate, IPlugin
	{
		public virtual void Execute(IServiceProvider serviceProvider)
		{
			Create(serviceProvider, false, AnnotationDetails.EntityName, AnnotationDetails.DocumentBodyAttributeKey, AnnotationDetails.FileNameAttributeKey);
		}
	}
}
