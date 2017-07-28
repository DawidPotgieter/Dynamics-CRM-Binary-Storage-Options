using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryStorageOptions
{
	public static class CrmConstants
	{
		public const string AnnotationEntityName = "annotation";
		public const string AttachmentEntityName = "activitymimeattachment";

		public const string TargetParameterKey = "Target";
		public const string RelatedEntitiesQueryKey = "RelatedEntitiesQuery";
		public const string BusinessEntityKey = "BusinessEntity";
		public const string BusinessEntityCollectionKey = "BusinessEntityCollection";
		public const string FileSizeKey = "filesize";
		public const string IsDocumentKey = "isdocument";
		public const string MimeTypeKey = "mimetype";
		public const string Query = "Query";

		internal const int PreOperationStateNumber = 20;
		internal const int PostOperationStateNumber = 40;
		internal const int AsyncOperationMode = 1;
	}
}
