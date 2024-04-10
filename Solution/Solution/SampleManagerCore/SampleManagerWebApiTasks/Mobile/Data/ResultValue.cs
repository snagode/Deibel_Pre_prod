using System.Runtime.Serialization;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Result Value
	/// </summary>
	[DataContract(Name="resultValue")]
	public class ResultValue : MobileObject
	{
		#region Properties

		/// <summary>
		/// Gets or sets the test number.
		/// </summary>
		/// <value>
		/// The test number.
		/// </value>
		[DataMember(Name = "testNumber")]
		public int TestNumber { get; set; }

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		[DataMember(Name = "name")]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the text.
		/// </summary>
		/// <value>
		/// The text.
		/// </value>
		[DataMember(Name = "text")]
		public string Text { get; set; }

		#endregion
	}
}
