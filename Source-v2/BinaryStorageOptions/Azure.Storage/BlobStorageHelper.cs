using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Azure.Storage
{
	public class BlobStorageHelper : StorageHelper
	{
		private const string blobStorageUriTemplate = "https://{0}.blob.core.windows.net/";
		private const string blobStorageSecondaryUriTemplate = "https://{0}-secondary.blob.core.windows.net/";

		public BlobStorageHelper(string account, string key) :
			base(blobStorageUriTemplate, blobStorageSecondaryUriTemplate, account, key)
		{
		}

		/*<?xml version="1.0" encoding="utf-8"?>
		<EnumerationResults ServiceEndpoint="https://myaccount.blob.core.windows.net">
			<Prefix>string-value</Prefix>
			<Marker>string-value</Marker>
			<MaxResults>int-value</MaxResults>
			<Containers>
				<Container>
					<Name>container-name</Name>
					<Properties>
						<Last-Modified>date/time-value</Last-Modified>
						<Etag>etag</Etag>
						<LeaseStatus>locked | unlocked</LeaseStatus>
						<LeaseState>available | leased | expired | breaking | broken</LeaseState>
						<LeaseDuration>infinite | fixed</LeaseDuration>      
					</Properties>
					<Metadata>
						<metadata-name>value</metadata-name>
					</Metadata>
				</Container>
			</Containers>
			<NextMarker>marker-value</NextMarker>
		</EnumerationResults>*/
		public List<string> ListContainers()
		{
			List<string> containers = new List<string>();
			var response = ExecuteRestRequestWithFailover(HtmlVerb.GET, "?comp=list");
			XDocument xDocument = XDocument.Load(response.Content.ReadAsStreamAsync().Result);
			foreach (var container in xDocument.Descendants("Container"))
			{
				containers.Add(container.Element("Name").Value);
			}
			return containers;
		}

		/*
		<?xml version="1.0" encoding="utf-8"?>
		<EnumerationResults ServiceEndpoint="http://myaccount.blob.core.windows.net/"  ContainerName="mycontainer">
			<Prefix>string-value</Prefix>
			<Marker>string-value</Marker>
			<MaxResults>int-value</MaxResults>
			<Delimiter>string-value</Delimiter>
			<Blobs>
				<Blob>
					<Name>blob-name</name>
					<Snapshot>date-time-value</Snapshot>
					<Properties>
						<Last-Modified>date-time-value</Last-Modified>
						<Etag>etag</Etag>
						<Content-Length>size-in-bytes</Content-Length>
						<Content-Type>blob-content-type</Content-Type>
						<Content-Encoding />
						<Content-Language />
						<Content-MD5 />
						<Cache-Control />
						<x-ms-blob-sequence-number>sequence-number</x-ms-blob-sequence-number>
						<BlobType>BlockBlob|PageBlob|AppendBlob</BlobType>
						<LeaseStatus>locked|unlocked</LeaseStatus>
						<LeaseState>available | leased | expired | breaking | broken</LeaseState>
						<LeaseDuration>infinite | fixed</LeaseDuration>
						<CopyId>id</CopyId>
						<CopyStatus>pending | success | aborted | failed </CopyStatus>
						<CopySource>source url</CopySource>
						<CopyProgress>bytes copied/bytes total</CopyProgress>
						<CopyCompletionTime>datetime</CopyCompletionTime>
						<CopyStatusDescription>error string</CopyStatusDescription>
					</Properties>
					<Metadata>   
						<Name>value</Name>
					</Metadata>
				</Blob>
				<BlobPrefix>
					<Name>blob-prefix</Name>
				</BlobPrefix>
			</Blobs>
			<NextMarker />
		</EnumerationResults>		
		*/
		public List<string> ListBlobNames(string container)
		{
			List<string> blobs = new List<string>();
			string nextMarker;
			blobs.AddRange(ListBlobNamesPaged(container, null, out nextMarker));
			while (!string.IsNullOrWhiteSpace(nextMarker))
			{
				blobs.AddRange(ListBlobNamesPaged(container, nextMarker, out nextMarker));
			}
			return blobs;
		}

		private List<string> ListBlobNamesPaged(string container, string marker, out string nextMarker)
		{
			nextMarker = null;
			List<string> blobs = new List<string>();
			var response = ExecuteRestRequestWithFailover(HtmlVerb.GET, string.Format("{0}?restype=container&comp=list&maxresults=5000{1}", container, (!string.IsNullOrWhiteSpace(marker) ? "&marker=" + marker : "")));
			XDocument xDocument = XDocument.Load(response.Content.ReadAsStreamAsync().Result);
			foreach (var blob in xDocument.Descendants("Blob"))
			{
				blobs.Add(blob.Element("Name").Value);
			}
			if (xDocument.Descendants("NextMarker").Count() == 1)
				nextMarker = xDocument.Descendants("NextMarker").First().Value;
			return blobs;
		}

		public int GetBlobSize(string container, string name)
		{
			var response = ExecuteRestRequestWithFailover(HtmlVerb.HEAD, string.Format("{0}/{1}", container, name));
			if (response.IsSuccessStatusCode)
			{
				return int.Parse(response.Content.Headers.First(h => h.Key == "Content-Length").Value.First());
			}
			return -1;
		}

		public bool CreateContainer(string name)
		{
			Microsoft.WindowsAzure.Storage.NameValidator.ValidateContainerName(name);

			var response = ExecuteRestRequest(HtmlVerb.PUT, string.Format("{0}?restype=container", name));
			return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Conflict; //Conflict means it already exists
		}

		public byte[] GetBlob(string container, string name)
		{
			string uri = string.Format("{0}/{1}", container, name);
			var response = ExecuteRestRequestWithFailover(HtmlVerb.GET, uri);
			if (!response.IsSuccessStatusCode)
			{
				throw new HttpRequestException(string.Format("Could not get file '{0}'. {1} : {2}", uri, response.StatusCode, response.ReasonPhrase));
			}
			return response.Content.ReadAsByteArrayAsync().Result;
		}

		public bool DeleteBlob(string container, string name)
		{
			var response = ExecuteRestRequest(HtmlVerb.DELETE, string.Format("{0}/{1}", container, name));
			return response.IsSuccessStatusCode;
		}

		public bool DeleteBlobPrefixed(string container, string namePrefix)
		{
			var response = ExecuteRestRequest(HtmlVerb.GET, string.Format("{0}?restype=container&comp=list&prefix={1}", container, namePrefix));
			XDocument xDocument = XDocument.Load(response.Content.ReadAsStreamAsync().Result);
			string blobName = xDocument.Descendants("Blob").Single().Element("Name").Value;
			response = ExecuteRestRequest(HtmlVerb.DELETE, string.Format("{0}/{1}", container, blobName));
			return response.IsSuccessStatusCode;
		}

		public bool PutBlob(string container, string name, byte[] content)
		{
			Microsoft.WindowsAzure.Storage.NameValidator.ValidateContainerName(container);
			Microsoft.WindowsAzure.Storage.NameValidator.ValidateBlobName(name);

			string md5 = Convert.ToBase64String(System.Security.Cryptography.MD5.Create().ComputeHash(content));
			Dictionary<string, string> additionalHeaders = new Dictionary<string, string>
			{
				{ "x-ms-blob-type", "BlockBlob" }
			};
			HttpContent httpContent = new ByteArrayContent(content);
			httpContent.Headers.Add("Content-Length", content.Length.ToString());
			httpContent.Headers.Add("Content-MD5", md5);
			var response = ExecuteRestRequest(HtmlVerb.PUT, string.Format("{0}/{1}", container, name), httpContent, additionalHeaders);
			return response.IsSuccessStatusCode;
		}
	}
}
