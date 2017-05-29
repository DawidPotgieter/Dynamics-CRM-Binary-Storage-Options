using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryStorageOptions
{
	public static class GenericConstants
	{
		public const string EmptyBodyContent = "IA==";
		public const int EmptyBodyContentDataLength = 3;
		public const string DocumentBodyAttributeKey = "DocumentBodyAttributeKey";
		public const string FileNameAttributeKey = "FileNameAttributeKey";
		public const string AttachementExtensionEntityName = "bso_attachmentextension";
		public const string AttachementExtensionEntityNameKey = "bso_entityname";
		public const string AttachementExtensionEntityIdKey = "bso_entityid";
		public const string AttachementExtensionFileSizeKey = "bso_filesize";
		public static string AttachmentExtensionPrivilegeTemplate = "prv{0}bso_attachmentextension";

		private static Dictionary<string, string> annotationConstants = new Dictionary<string, string>
		{
			{ DocumentBodyAttributeKey, "documentbody" },
			{ FileNameAttributeKey, "filename" },
		};
		private static Dictionary<string, string> attachmentConstants = new Dictionary<string, string>
		{
			{ DocumentBodyAttributeKey, "body" },
			{ FileNameAttributeKey, "filename" },
		};
		public static ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>> Constants = new ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>>(
			new Dictionary<string, ReadOnlyDictionary<string, string>>
			{
				{ CrmConstants.AnnotationEntityName, new ReadOnlyDictionary<string, string>(annotationConstants) },
				{ CrmConstants.AttachmentEntityName, new ReadOnlyDictionary<string, string>(attachmentConstants) },
			}
		);

	}
}
