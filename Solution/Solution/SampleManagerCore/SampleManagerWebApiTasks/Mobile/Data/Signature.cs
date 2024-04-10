using System;
using System.Runtime.Serialization;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Signature
	/// </summary>
	[DataContract(Name="signature")]
	public class Signature : MobileObject
	{
		#region Properties

		/// <summary>
		/// Gets or sets the procedure number.
		/// </summary>
		/// <value>
		/// The procedure number.
		/// </value>
		[DataMember(Name = "function")]
		public int Function { get; set; }

		/// <summary>
		/// Gets or sets the reason.
		/// </summary>
		/// <value>
		/// The reason.
		/// </value>
		[DataMember(Name = "reason")]
		public string Reason { get; set; }

		/// <summary>
		/// Gets or sets the comments.
		/// </summary>
		/// <value>
		/// The comments.
		/// </value>
		[DataMember(Name = "comments")]
		public string Comments { get; set; }

		/// <summary>
		/// Gets or sets the name of the client.
		/// </summary>
		/// <value>
		/// The name of the client.
		/// </value>
		[DataMember(Name = "clientName")]
		public string ClientName { get; set; }

		/// <summary>
		/// Gets or sets the client address.
		/// </summary>
		/// <value>
		/// The client address.
		/// </value>
		[DataMember(Name = "clientAddress")]
		public string ClientAddress { get; set; }

		/// <summary>
		/// Gets or sets the client date.
		/// </summary>
		/// <value>
		/// The client date.
		/// </value>
		[DataMember(Name = "clientDate")]
		public DateTime ClientDate { get; set; }

		#endregion
	}
}
