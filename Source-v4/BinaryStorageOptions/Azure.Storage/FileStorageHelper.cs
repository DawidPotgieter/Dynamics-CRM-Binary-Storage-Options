using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace Azure.Storage
{
	public class FileStorageHelper : StorageHelper
	{
		private const string fileStorageUriTemplate = "https://{0}.file.{1}/";
		private const string fileStorageSecondaryUriTemplate = "https://{0}-secondary.file.{1}/";
		private const int bufferSize = 1024 * 1024 * 4; //File storage max buffer size is 4mb

		public FileStorageHelper(string account, string endpointSuffix, string key) :
			base(fileStorageUriTemplate, fileStorageSecondaryUriTemplate, account, endpointSuffix, key)
		{
		}

		/*
		<?xml version="1.0" encoding="utf-8"?>
		<EnumerationResults AccountName="https://myaccount.file.core.windows.net">
			<Prefix>string-value</Prefix>
			<Marker>string-value</Marker>
			<MaxResults>int-value</MaxResults>
			<Shares>
				<Share>
					<Name>share-name</Name>
					<Properties>
						<Last-Modified>date/time-value</Last-Modified>
						<Etag>etag</Etag>
						<Quota>max-share-size</Quota>
					</Properties>
					<Metadata>
						<metadata-name>value</metadata-name>
					</Metadata>
				</Share>
			</Shares>
			<NextMarker>marker-value</NextMarker>
		</EnumerationResults>
		 */
		public List<string> ListShares()
		{
			List<string> shares = new List<string>();
			var response = ExecuteRestRequestWithFailover(HtmlVerb.GET, "?comp=list");
			XDocument xDocument = XDocument.Load(response.Content.ReadAsStreamAsync().Result);
			foreach (var container in xDocument.Descendants("Share"))
			{
				shares.Add(container.Element("Name").Value);
			}
			return shares;
		}

		public bool CreateShare(string name)
		{
			Microsoft.WindowsAzure.Storage.NameValidator.ValidateShareName(name);

			var response = ExecuteRestRequest(HtmlVerb.PUT, string.Format("{0}?restype=share", name));
			return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Conflict; //Conflict = exits
		}

		/*
			<?xml version="1.0" encoding="utf-8"?>
			<EnumerationResults ServiceEndpoint="https://myaccount.file.core.windows.net/” ShareName="myshare" DirectoryPath="directory-path">
				<Marker>string-value</Marker>
				<MaxResults>int-value</MaxResults>
				<Entries>
					<File>
						<Name>file-name</Name>
						<Properties>
							<Content-Length>size-in-bytes</Content-Length>
						</Properties>
					</File>
					<Directory>
						<Name>directory-name</Name>
					</Directory>
				</Entries>
				<NextMarker />
			</EnumerationResults> 
		*/
		public List<string> ListDirectories(string share, string parentDirectory)
		{
			List<string> directories = new List<string>();
			var response = ExecuteRestRequestWithFailover(HtmlVerb.GET, string.Format("{0}/{1}?restype=directory&comp=list", share.ToLower(), parentDirectory.ToLower()).Replace("/?", "?"));
			XDocument xDocument = XDocument.Load(response.Content.ReadAsStreamAsync().Result);
			foreach (var container in xDocument.Descendants("Directory"))
			{
				directories.Add(container.Element("Name").Value);
			}
			return directories;
		}

		public List<string> ListFilenames(string share, string parentDirectory)
		{
			List<string> files = new List<string>();
			string nextMarker;
			files.AddRange(ListFilenamesPaged(share, parentDirectory, null, out nextMarker));
			while (!string.IsNullOrWhiteSpace(nextMarker))
			{
				files.AddRange(ListFilenamesPaged(share, parentDirectory, nextMarker, out nextMarker));
			}
			return files;
		}

		public List<string> ListFilenamesPaged(string share, string parentDirectory, string marker, out string nextMarker)
		{
			nextMarker = null;
			List<string> files = new List<string>();
			var response = ExecuteRestRequestWithFailover(HtmlVerb.GET, string.Format("{0}/{1}?restype=directory&comp=list&maxresults=5000{2}", share.ToLower(), parentDirectory.ToLower(), (!string.IsNullOrWhiteSpace(marker) ? "&marker=" + marker : "")).Replace("/?", "?"));
			XDocument xDocument = XDocument.Load(response.Content.ReadAsStreamAsync().Result);
			foreach (var container in xDocument.Descendants("File"))
			{
				files.Add(container.Element("Name").Value);
			}
			if (xDocument.Descendants("NextMarker").Count() == 1)
				nextMarker = xDocument.Descendants("NextMarker").First().Value;
			return files;
		}
		
		public bool CreateDirectory(string share, string parentDirectory, string name)
		{
			Microsoft.WindowsAzure.Storage.NameValidator.ValidateShareName(share);
			Microsoft.WindowsAzure.Storage.NameValidator.ValidateDirectoryName(parentDirectory);
			Microsoft.WindowsAzure.Storage.NameValidator.ValidateFileName(name);

			var response = ExecuteRestRequest(HtmlVerb.PUT, string.Format("{0}/{1}/{2}?restype=directory", share.ToLower(), parentDirectory.ToLower(), name.ToLower()).Replace("//", "/"));
			return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Conflict; //Conflict = exits
		}

		public int GetFileSize(string share, string directory, string name)
		{
			var response = ExecuteRestRequestWithFailover(HtmlVerb.HEAD, string.Format("{0}/{1}/{2}", share.ToLower(), directory.ToLower(), name.ToLower()).Replace("//", "/"));
			if (response.IsSuccessStatusCode)
			{
				return int.Parse(response.Content.Headers.First(h => h.Key == "Content-Length").Value.First());
			}
			return -1;
		}

		public byte[] GetFile(string share, string directory, string name, out Dictionary<string, string> metaData)
		{
			string uri = string.Format("{0}/{1}/{2}", share.ToLower(), directory.ToLower(), name.ToLower()).Replace("//", "/");
			metaData = GetMetaData(share, directory, name);
			var response = ExecuteRestRequestWithFailover(HtmlVerb.GET, uri);
			if (!response.IsSuccessStatusCode)
			{
				throw new HttpRequestException(string.Format("Could not get file '{0}'. {1} : {2}", uri, response.StatusCode, response.ReasonPhrase));
			}
			return response.Content.ReadAsByteArrayAsync().Result;
		}

		public Dictionary<string, string> GetMetaData(string share, string directory, string name, bool customOnly = true)
		{
			string uri = string.Format("{0}/{1}/{2}?comp=metadata", share.ToLower(), directory.ToLower(), name.ToLower()).Replace("//", "/");
			var response = ExecuteRestRequestWithFailover(HtmlVerb.GET, uri);
			if (!response.IsSuccessStatusCode)
			{
				throw new HttpRequestException(string.Format("Could not get file metaData '{0}'. {1} : {2}", uri, response.StatusCode, response.ReasonPhrase));
			}
			Dictionary<string, string> metadata = response.Headers.Where(h => h.Key.StartsWith("x-ms-meta-", StringComparison.OrdinalIgnoreCase)).ToDictionary(h => h.Key.Replace("x-ms-meta-", ""), h => h.Value.Single());
			if (!customOnly)
			{
				var nonCustomHeaders = response.Headers.Where(h => !h.Key.StartsWith("x-ms-meta-", StringComparison.OrdinalIgnoreCase)).ToDictionary(h => h.Key, h => string.Join(";", h.Value));
				foreach (var nonCustomHeader in nonCustomHeaders)
				{
					metadata.Add(nonCustomHeader.Key, nonCustomHeader.Value);
				}
			}
			return metadata;
		}

		public bool DeleteFile(string share, string directory, string name)
		{
			var response = ExecuteRestRequest(HtmlVerb.DELETE, string.Format("{0}/{1}/{2}", share.ToLower(), directory.ToLower(), name.ToLower()).Replace("//", "/"));
			return response.IsSuccessStatusCode;
		}

		public bool DeleteFilePrefixed(string share, string directory, string namePrefix)
		{
			//Unfortunately, File storage doesn't allow searching.  Have to page through all the names to find the prefixed value
			string nextMarker;
			var fileNamesPage = ListFilenamesPaged(share, directory, null, out nextMarker);
			while (!string.IsNullOrWhiteSpace(nextMarker) && !fileNamesPage.Any(fn => fn.StartsWith(namePrefix)))
			{
				fileNamesPage = ListFilenamesPaged(share, directory, nextMarker, out nextMarker);
			}

			string fullFileName = fileNamesPage.First(fn => fn.StartsWith(namePrefix));
			var response = ExecuteRestRequest(HtmlVerb.DELETE, string.Format("{0}/{1}/{2}", share.ToLower(), directory.ToLower(), fullFileName.ToLower()).Replace("//", "/"));
			return response.IsSuccessStatusCode;
		}

		public bool PutFile(string share, string directory, string name, byte[] content, Dictionary<string, string> metaData = null)
		{
			Microsoft.WindowsAzure.Storage.NameValidator.ValidateShareName(share);
			Microsoft.WindowsAzure.Storage.NameValidator.ValidateDirectoryName(directory);
			Microsoft.WindowsAzure.Storage.NameValidator.ValidateFileName(name);

			MD5 hasher = System.Security.Cryptography.MD5.Create();
			string md5 = Convert.ToBase64String(hasher.ComputeHash(content));
			Dictionary<string, string> additionalHeaders = new Dictionary<string, string>
			{
				{ "x-ms-type", "file" },
				{ "x-ms-content-md5", md5 }
			};
			if (metaData != null)
			{
				foreach (var item in metaData)
				{
					additionalHeaders.Add(string.Format("x-ms-meta-{0}", item.Key), item.Value);
				}
			}
			HttpContent httpContent = new StringContent("");
			httpContent.Headers.Add("x-ms-content-length", content.Length.ToString());
			httpContent.Headers.Remove("Content-Type");
			var response = ExecuteRestRequest(HtmlVerb.PUT, string.Format("{0}/{1}/{2}", share.ToLower(), directory.ToLower(), name.ToLower()).Replace("//", "/"), httpContent, additionalHeaders);
			if (response.IsSuccessStatusCode)
			{
				int currentBufferPage = 0;
				int startBytes = currentBufferPage * bufferSize;
				int endBytes;
				int currentBufferSize;
				while (startBytes < content.Length)
				{
					endBytes = Math.Min(bufferSize + startBytes - 1, content.Length - 1);
					currentBufferSize = endBytes - startBytes + 1;

					md5 = Convert.ToBase64String(hasher.ComputeHash(content, startBytes, currentBufferSize));

					additionalHeaders = new Dictionary<string, string>
					{
						{ "x-ms-range", string.Format("bytes={0}-{1}", startBytes, endBytes) },
						{ "x-ms-write", "update" }
					};

					httpContent = new ByteArrayContent(content, startBytes, currentBufferSize);
					httpContent.Headers.Add("Content-Length", currentBufferSize.ToString());
					httpContent.Headers.Add("Content-MD5", md5);
					response = ExecuteRestRequest(HtmlVerb.PUT, string.Format("{0}/{1}/{2}?comp=range", share.ToLower(), directory.ToLower(), name.ToLower()).Replace("//", "/"), httpContent, additionalHeaders);
					Guid? responseId = null;
					if (response.IsSuccessStatusCode)
					{
						responseId = Guid.Parse(response.Headers.First(h => h.Key == "x-ms-request-id").Value.First());
						startBytes = ++currentBufferPage * bufferSize;
					}
					else
					{
						try
						{
							DeleteFile(share, directory, name);
						}
						catch { }
						return false;
					}
				}
				return true;
			}
			return false;
		}
	}
}
