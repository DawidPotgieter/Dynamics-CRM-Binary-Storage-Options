using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;

namespace Azure.Storage
{
	public class StorageHelper
	{
		private const string apiVersion = "2015-02-21";
		private const string skaTemplate = "{0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}\n{8}\n{9}\n{10}\n{11}\n{12}\n{13}";
		private const string skaTableTemplate = "{0}\n{1}\n{2}\n{3}\n{4}";
		private const int maxRetries = 2;

		private string uriTemplate;
		private string secondaryUriTemplate;
		private string account;
		private string endpointSuffix;
		private string key;
		private int retryCounter;
		private bool isTableStorage;

		public enum HtmlVerb
		{
			GET,
			POST,
			PUT,
			DELETE,
			HEAD,
		}

		public StorageHelper(string uriTemplate, string secondaryUriTemplate, string account, string endpointSuffix, string key, bool isTableStorage = false) 
		{
			this.uriTemplate = uriTemplate;
			this.secondaryUriTemplate = secondaryUriTemplate;
			this.account = account;
			this.endpointSuffix = endpointSuffix;
			this.key = key;
			this.isTableStorage = isTableStorage;
		}

		public StorageHelper(string uriTemplate, string account, string endpointSuffix, string key, bool isTableStorage = false)
			: this(uriTemplate, null, account, endpointSuffix, key, isTableStorage)
		{
		}

		protected HttpResponseMessage ExecuteRestRequestWithFailover(HtmlVerb verb, string resourceUri, HttpContent content = null, Dictionary<string, string> additionalHeaders = null)
		{
			resourceUri = PreEncodeUrl(resourceUri);
			Uri fullUri = new Uri(new Uri(string.Format(uriTemplate, account, endpointSuffix), UriKind.Absolute), new Uri(resourceUri, UriKind.Relative));
			try
			{
				retryCounter = 0;
				return ExecuteRestRequestWithRetry(verb, fullUri, content, additionalHeaders);
			}
			catch
			{
				if (!string.IsNullOrWhiteSpace(secondaryUriTemplate))
				{
					fullUri = new Uri(new Uri(string.Format(secondaryUriTemplate, account, endpointSuffix), UriKind.Absolute), new Uri(resourceUri, UriKind.Relative));
					retryCounter = 0;
					return ExecuteRestRequestWithRetry(verb, fullUri, content, additionalHeaders);
				}
				throw;
			}
		}

		protected HttpResponseMessage ExecuteRestRequest(HtmlVerb verb, string resourceUri, HttpContent content = null, Dictionary<string, string> additionalHeaders = null)
		{
			Uri fullUri = new Uri(new Uri(string.Format(uriTemplate, account, endpointSuffix), UriKind.Absolute), new Uri(PreEncodeUrl(resourceUri), UriKind.Relative));
			retryCounter = 0;
			return ExecuteRestRequestWithRetry(verb, fullUri, content, additionalHeaders);
		}

		private HttpResponseMessage ExecuteRestRequestWithRetry(HtmlVerb verb, Uri fullUri, HttpContent content = null, Dictionary<string, string> additionalHeaders = null)
		{
			try
			{
				return ExecuteRestRequest(verb, fullUri, content, additionalHeaders);
			}
			catch
			{
				if (retryCounter < maxRetries)
				{
					retryCounter++;
					return ExecuteRestRequestWithRetry(verb, fullUri, content, additionalHeaders);
				}
				throw;
			}
		}

		private HttpResponseMessage ExecuteRestRequest(HtmlVerb verb, Uri fullUri, HttpContent content = null, Dictionary<string, string> additionalHeaders = null)
		{
			DateTime dt = DateTime.UtcNow;

			using (HttpClient client = new HttpClient())
			{
				client.DefaultRequestHeaders.Add("x-ms-date", dt.ToString("R"));
				client.DefaultRequestHeaders.Add("x-ms-version", apiVersion);
				if (additionalHeaders != null)
				{
					foreach (var header in additionalHeaders)
					{
						client.DefaultRequestHeaders.Add(header.Key, header.Value);
					}
				}

				string canonicalizedHeaders = GetCanonicalizedHeaders(client.DefaultRequestHeaders, (content != null ? content.Headers : null));
				string canonicalizedResource = GetCanonicalizedResource(fullUri);
				string contentLengthHeader = (content != null && content.Headers.Any(h => h.Key == "Content-Length") ? content.Headers.First(h => h.Key == "Content-Length").Value.First() : "");
				if (contentLengthHeader == "0") contentLengthHeader = "";
				string md5Header = (content != null && content.Headers.Any(h => h.Key == "Content-MD5") ? content.Headers.First(h => h.Key == "Content-MD5").Value.First() : "");
				string sharedKey;
				if (!isTableStorage)
				{
					sharedKey = string.Format(skaTemplate,
					verb.ToString(),
					"", "",
					contentLengthHeader,
					md5Header,
					"", "", "", "", "", "", "",
					canonicalizedHeaders,
					canonicalizedResource);
				}
				else
				{
					string contentTypeHeader = (content != null && content.Headers.Any(h => h.Key == "Content-Type") ? content.Headers.First(h => h.Key == "Content-Type").Value.First() : "");
					sharedKey = string.Format(skaTableTemplate,
						verb.ToString(),
						md5Header,
						contentTypeHeader,
						dt.ToString("R"),
						canonicalizedResource);
				}

				string signedSharedKey = SignSka(sharedKey, key, account);

				client.DefaultRequestHeaders.Add("Authorization", signedSharedKey);

				switch (verb)
				{
					case HtmlVerb.HEAD:
						return client.SendAsync(new HttpRequestMessage(HttpMethod.Head, fullUri)).Result;
					case HtmlVerb.GET:
						return client.GetAsync(fullUri).Result;
					case HtmlVerb.PUT:
						return client.PutAsync(fullUri, content).Result;
					case HtmlVerb.POST:
						return client.PostAsync(fullUri, content).Result;
					case HtmlVerb.DELETE:
						return client.DeleteAsync(fullUri).Result;
				}
			}
			return null;
		}

		private string GetCanonicalizedHeaders(System.Net.Http.Headers.HttpRequestHeaders requestHeaders, System.Net.Http.Headers.HttpContentHeaders contentHeaders = null)
		{
			string canonicalizedHeaders = "";
			//assuming orderby does the lexigraphical ordering required here?
			var headers = requestHeaders.Where(h => h.Key.StartsWith("x-ms-"));
			if (contentHeaders != null)
				headers = headers.Union(contentHeaders.Where(h => h.Key.StartsWith("x-ms-")));
			headers = headers.GroupBy(g => g.Key).Select(g => g.First()).OrderBy(h => h.Key);
			foreach (var header in headers)
			{
				canonicalizedHeaders += header.Key.ToLower() + ":";
				foreach (var headerValue in header.Value)
				{
					canonicalizedHeaders += headerValue + ",";
				}
				canonicalizedHeaders = canonicalizedHeaders.TrimEnd(',') + "\n";
			}
			return canonicalizedHeaders.TrimEnd('\n');
		}

		private string GetCanonicalizedResource(Uri resourceUri)
		{
			string canonicalizedResource = "";
			canonicalizedResource += string.Format("/{0}", account);
			canonicalizedResource += resourceUri.AbsolutePath + "\n";

			var pieces = resourceUri.Query.TrimStart('?').Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
			Dictionary<string, string> queryStringParams = new Dictionary<string, string>();
			foreach (string piece in pieces)
			{
				var split = piece.Split('=');
				if (!isTableStorage)
				{
					queryStringParams.Add(split[0], split[1]);
				}
				else
				{
					if (split[0] == "comp")
					{
						queryStringParams.Add("?comp=", split[1]);
					}
				}
			}
			//assuming orderby does the lexigraphical ordering required here?
			foreach (var queryParam in queryStringParams.OrderBy(p => p.Key))
			{
				canonicalizedResource += WebUtility.UrlDecode(queryParam.Key.ToLower()) + ":" + WebUtility.UrlDecode(queryParam.Value) + "\n";
			}
			return canonicalizedResource.TrimEnd('\n');
		}

		private string SignSka(string stringToSign, string key, string account)
		{
			String signature = string.Empty;
			byte[] unicodeKey = Convert.FromBase64String(key);
			using (HMACSHA256 hmacSha256 = new HMACSHA256(unicodeKey))
			{
				Byte[] dataToHmac = System.Text.Encoding.UTF8.GetBytes(stringToSign);
				signature = Convert.ToBase64String(hmacSha256.ComputeHash(dataToHmac));
			}

			String authorizationHeader = String.Format(
						CultureInfo.InvariantCulture,
						"{0} {1}:{2}",
						"SharedKey",
						account,
						signature);

			return authorizationHeader;
		}

		private string PreEncodeUrl(string url)
		{
			return url.Replace("#", "%23");
		}
	}
}
