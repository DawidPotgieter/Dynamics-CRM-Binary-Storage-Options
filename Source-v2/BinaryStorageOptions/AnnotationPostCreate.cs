using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryStorageOptions
{
	public class AnnotationPostCreate : BinaryDataCreate, IPlugin
	{
		public void Execute(IServiceProvider serviceProvider)
		{
			Create(serviceProvider, true, AnnotationDetails.EntityName, AnnotationDetails.DocumentBodyAttributeKey, AnnotationDetails.FileNameAttributeKey);
		}
	}
}
