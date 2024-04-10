using System;
using Newtonsoft.Json;

namespace Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel.WebApi
{
	/// <summary>
	/// Web API Exception
	/// </summary>
	[JsonObject]
	public class WebApiError : Exception
	{
		/// <summary>
		/// Gets or sets the error message.
		/// </summary>
		/// <value>
		/// The error message.
		/// </value>
		[JsonProperty]
		public string ExceptionMessage { get; set; }

		/// <summary>
		/// Gets or sets the type of the exception.
		/// </summary>
		/// <value>
		/// The type of the exception.
		/// </value>
		[JsonProperty]
		public string ExceptionType { get; set; }

		/// <summary>
		/// Gets or sets the exception source.
		/// </summary>
		/// <value>
		/// The exception source.
		/// </value>
		[JsonProperty]
		public string ExceptionSource { get; set; }

		/// <summary>
		/// Gets or sets the exception source.
		/// </summary>
		/// <value>
		/// The exception source.
		/// </value>
		[JsonProperty]
		public new string Message { get; set; }

		/// <summary>
		/// Gets the <see cref="T:System.Exception" /> instance that caused the current exception.
		/// </summary>
		[JsonProperty]
		public new WebApiError InnerException { get; set; }
	}
}
