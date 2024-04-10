using System;
using System.ComponentModel;
using System.ServiceModel.Web;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.WebApiTasks.Data;

namespace Thermo.SampleManager.WebApiTasks
{
	/// <summary>
	/// Utilities
	/// </summary>
	[SampleManagerWebApi("utilities")]
	public class UtilityTask : SampleManagerWebApiTask
	{
		#region About

		/// <summary>
		/// About Information
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "about", Method = "GET")]
		[Description("Information regarding this SampleManager Web API")]
		public About About()
		{
			return new About(Library.Environment);
		}

		#endregion

		#region Ping

		/// <summary>
		/// Pings the specified response.
		/// </summary>
		/// <param name="response">The response.</param>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "ping/{response}", Method = "GET")]
		[Description("Ping the server with the specified response")]
		public string Ping(string response)
		{
			return response;
		}

		/// <summary>
		/// Pings the time.
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "ping", Method = "GET")]
		[Description("Ping the server and get the current server timestamp")]
		public DateTime PingTime()
		{
			return DateTime.Now;
		}

		#endregion
	}
}