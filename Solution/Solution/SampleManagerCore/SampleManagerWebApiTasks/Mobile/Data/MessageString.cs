using System.Runtime.Serialization;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Message String
	/// </summary>
	[DataContract(Name="messageString")]
	public class MessageString : MobileObject
	{
		#region Properties

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		[DataMember(Name = "name")]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>
		/// The value.
		/// </value>
		[DataMember(Name = "value")]
		public string Value { get; set; }

		#endregion
	}
}
