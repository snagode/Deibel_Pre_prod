using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Thermo.Framework.Core;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library.DesignerRuntime;

namespace Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel.WebApi
{
	/// <summary>
	/// Web API Client
	/// </summary>
	public class WebApiClient : WebClient
	{
		#region Constants

		private const string JsonContentType = "application/json";
		private const string NullErrorMessage = "null";
		private const string PostMethod = "POST";
		private const string PutMethod = "PUT";

		#endregion

		#region Properties

		/// <summary>
		/// Gets the logger.
		/// </summary>
		/// <value>
		/// The logger.
		/// </value>
		protected Logger Logger { get; private set; }

		/// <summary>
		/// Time in milliseconds
		/// </summary>
		/// <value>
		/// The timeout.
		/// </value>
		public int Timeout { get; set; }

		/// <summary>
		/// Gets or sets the standard library.
		/// </summary>
		/// <value>
		/// The library.
		/// </value>
		public StandardLibrary Library { get; private set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="WebApiClient" /> class.
		/// </summary>
		/// <param name="library">The standard library for messages etc.</param>
		/// <param name="baseAddress">The base address.</param>
		/// <param name="timeout">The timeout.</param>
		/// <exception cref="SampleManagerError"></exception>
		public WebApiClient(StandardLibrary library, string baseAddress, int timeout = 60000)
		{
			Library = library;
			Logger = Logger.GetInstance(GetType());

			Logger.DebugFormat("Creating Web Client. Base Address {0}, Timeout {1}", baseAddress, timeout);

			try
			{
				BaseAddress = baseAddress;

				Headers[HttpRequestHeader.ContentType] = JsonContentType;
				Proxy = null;
				Timeout = timeout;

				// Ignore the trust status our the self-signed certificate.

				ServicePointManager.ServerCertificateValidationCallback = delegate
				{
					return true;
				};
			}
			catch (ArgumentException e)
			{
				string message = GetMessage("WebApiInvalidAddressMessage", baseAddress);
				string caption = GetMessage("WebApiInvalidAddressCaption");

				throw new SampleManagerError(caption, message, e);
			}

			Logger.DebugFormat("Created Web Client. Base Address {0}, Timeout {1}", BaseAddress, Timeout);
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Returns a <see cref="T:System.Net.WebRequest" /> object for the specified resource.
		/// </summary>
		/// <param name="address">A <see cref="T:System.Uri" /> that identifies the resource to request.</param>
		/// <returns>
		/// A new <see cref="T:System.Net.WebRequest" /> object for the specified resource.
		/// </returns>
		protected override WebRequest GetWebRequest(Uri address)
		{
			var request = base.GetWebRequest(address);

			if (request != null)
			{
				request.Timeout = Timeout;
			}

			return request;
		}

		#endregion

		#region Utility Methods

		/// <summary>
		/// Generates the query string.
		/// </summary>
		/// <param name="queryArgs">The query arguments.</param>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		protected string GenerateQueryString(List<KeyValuePair<string, string>> queryArgs, string path = null)
		{
			UriBuilder uriBuilder;

			// Get a URI Builder

			try
			{
				uriBuilder = new UriBuilder(BaseAddress);
				if (!string.IsNullOrEmpty(path))
				{
					path = path.Trim('/');
					uriBuilder.Path += path;
				}
			}
			catch (UriFormatException ex)
			{
				string message = GetMessage("WebApiInvalidWebAddressMessage", BaseAddress, ex.Message);
				string caption = GetMessage("WebApiInvalidWebAddressCaption");

				var error = new SampleManagerError(caption, message, ex);
				Logger.Error(message, error);
				throw error;
			}

			// Process the Query Parameters

			var query = HttpUtility.ParseQueryString(uriBuilder.Query);

			if (queryArgs != null)
			{
				foreach (var pair in queryArgs)
				{
					query.Add(pair.Key, pair.Value);
				}
			}

			uriBuilder.Query = query.ToString();
			return uriBuilder.ToString();
		}

		/// <summary>
		/// Sends an HTTP PUT request to the server to create a resource(s) specified by the resource path.  
		/// The caller should know the type of data that will be sent to the server, specified by the template parameter.
		/// </summary>
		/// <typeparam name="T">The template type of the data that will be sent to the server.</typeparam>
		/// <param name="data">The resource data to be sent to the server to be created.</param>
		/// <param name="resourcePath">Specifies the path of the resource(s) to be created on the server.</param>
		/// <param name="queryArgs">The query string parameters, if applicable, for the request.</param>
		/// <returns></returns>
		public string Put<T>(T data, string resourcePath, List<KeyValuePair<string, string>> queryArgs)
		{
			string response;
			string request = GenerateQueryString(queryArgs, resourcePath);
			string stringifiedData = JsonHelper.ToJson(data);

			try
			{
				Logger.DebugFormat("PUT {0} - full request {1}, data {2}", resourcePath, request, stringifiedData);
				response = UploadString(request, PutMethod, stringifiedData);
				Logger.DebugFormat("PUT response {0}", response);
			}
			catch (WebException ex)
			{
				throw GetExceptionDetail(ex);
			}

			return response;
		}

		/// <summary>
		/// Sends an HTTP POST request to the server to create a resource(s) specified by the resource path.  
		/// The caller should know the type of data that will be sent to the server, specified by the template parameter.
		/// </summary>
		/// <typeparam name="T">The template type of the data that will be sent to the server.</typeparam>
		/// <param name="data">The resource data to be sent to the server to be created.</param>
		/// <param name="resourcePath">Specifies the path of the resource(s) to be created on the server.</param>
		/// <param name="queryArgs">The query string parameters, if applicable, for the request.</param>
		/// <returns></returns>
		public string Post<T>(T data, string resourcePath, List<KeyValuePair<string, string>> queryArgs)
		{
			string response;
			string request = GenerateQueryString(queryArgs, resourcePath);
			string stringifiedData = JsonHelper.ToJson(data);

			try
			{
				Logger.DebugFormat("POST {0} - full request {1}, data {2}", resourcePath, request, stringifiedData);
				response = UploadString(request, PostMethod, stringifiedData);
				Logger.DebugFormat("POST response {0}", response);
			}
			catch (WebException ex)
			{
				throw GetExceptionDetail(ex);
			}

			return response;
		}

		/// <summary>
		/// Sends an HTTP GET request to the server and returns the data.  The caller should know the type of data that will 
		/// be returned by the server and specify it via the template type.  
		/// </summary>
		/// <typeparam name="T">The template type that will be returned.</typeparam>
		/// <param name="resourcePath">Specifies the path of the resource(s) that the client will attempt to return.</param>
		/// <param name="queryArgs">The query string parameters, if applicable, for the request.</param>
		/// <returns>Returns an instance of the object represented by the JSON data returned by the server.</returns>
		public T Get<T>(string resourcePath, List<KeyValuePair<string, string>> queryArgs)
		{
			string request = GenerateQueryString(queryArgs, resourcePath);

			try
			{
				Logger.DebugFormat("GET {0} - full request {1}", resourcePath, request);
				string stringifiedData = DownloadString(request);
				Logger.DebugFormat("GET response {0}", stringifiedData);

				return JsonHelper.FromJson<T>(stringifiedData);
			}
			catch (WebException ex)
			{
				throw GetExceptionDetail(ex);
			}
		}

		#endregion

		#region Messages

		/// <summary>
		/// Gets the message.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="args">The arguments.</param>
		/// <returns></returns>
		private string GetMessage(string message, params string[] args)
		{
			return Library.Message.GetMessage("ChromeleonLinkMessages", message, args);
		}

		#endregion

		#region Exception Handling

		/// <summary>
		/// Gets the exception detail.
		/// </summary>
		/// <returns></returns>
		public SampleManagerError GetExceptionDetail(WebException exception)
		{
			string title = GetMessage("WebApiErrorCaption");
			string message = exception.Message;

			Logger.Debug(exception.Message, exception);

			try
			{
				if (exception.Status == WebExceptionStatus.ProtocolError)
				{
					var response = exception.Response.GetResponseStream();
					if (response != null)
					{
						var reader = new StreamReader(response);
						var fullMessage = reader.ReadToEnd();

						if (!string.IsNullOrEmpty(fullMessage) && fullMessage != NullErrorMessage)
						{
							var apiError = GetWebApiError(fullMessage);

							if (apiError == null)
							{
								// Web Page Error from API

								Logger.Debug("Exception could not be deserialized");
								message = GetFirstParagraph(fullMessage);
							}
							else if (string.IsNullOrWhiteSpace(apiError.ExceptionMessage))
							{
								// Partial Exception Information

								Logger.Debug("Exception partially deserialized");
								message = apiError.Message;
							}
							else
							{
								// Full Exception information from the API

								Logger.Debug("Exception deserialized");
								Logger.Debug(apiError.Message, apiError);
								var fullError = GetLowestError(title, apiError);
								return fullError;
							}
						}
					}
				}
				else
				{
					Logger.Debug("Exception handled without processing");

					// Full Exception information from the API

					Logger.Debug(exception.Message, exception);
					var fullError = GetLowestError(title, exception);
					return fullError;
				}
			}
			catch (Exception ex)
			{
				Logger.Debug("Exception during Exception Message reading code");
				Logger.Debug(ex.Message, ex);
			}

			// Return whatever information we've managed to glean.

			var error = new SampleManagerError(title, message, exception);
			Logger.Error(error.Title, error);
			return error;
		}

		/// <summary>
		/// Gets the lowest error via InnerException
		/// </summary>
		/// <param name="title">The title.</param>
		/// <param name="exception">The exception.</param>
		/// <returns></returns>
		private SampleManagerError GetLowestError(string title, Exception exception)
		{
			var webApiError = exception as WebApiError;

			if (webApiError != null)
			{
				if (webApiError.InnerException == null)
				{
					return new SampleManagerError(title, webApiError.ExceptionMessage, webApiError);
				}

				return GetLowestError(title, webApiError.InnerException);
			}

			if (exception.InnerException == null)
			{ 
				return new SampleManagerError(title, exception.Message, exception);
			}

			return GetLowestError(title, exception.InnerException);
		}

		/// <summary>
		/// Gets the web API error.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <returns></returns>
		private WebApiError GetWebApiError(string message)
		{
			try
			{
				var error = JsonHelper.FromJson<WebApiError>(message);
				return error;
			}
			catch (Exception)
			{
				return null;
			}
		}

		/// <summary>
		/// Gets the first paragraph.
		/// </summary>
		/// <param name="htmltext">The htmltext.</param>
		/// <returns></returns>
		private string GetFirstParagraph(string htmltext)
		{
			Match m = Regex.Match(htmltext, @"<p>\s*(.+?)\s*</p>");
			if (m.Success) return m.Groups[1].Value;
			return htmltext;
		}

		#endregion
	}
}
