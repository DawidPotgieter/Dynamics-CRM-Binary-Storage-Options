using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryStorageOptions
{
	public class AnnotationPostRetrieve : BinaryDataRetrieve, IPlugin
	{
		public void Execute(IServiceProvider serviceProvider)
		{
			Retrieve(serviceProvider, AnnotationDetails.EntityName, AnnotationDetails.DocumentBodyAttributeKey, AnnotationDetails.FileNameAttributeKey);
		}
	}
}
