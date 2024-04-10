using System.Runtime.Serialization;

namespace Thermo.SampleManager.WebApiTasks.Data
{
	/// <summary>
	/// Reference
	/// </summary>
	[DataContract(Name="reference")]
	public class Reference : Object
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
		/// Gets or sets the path.
		/// </summary>
		/// <value>
		/// The path.
		/// </value>
		[DataMember(Name="location")]
		public string Location { get; set; }

		/// <summary>
		/// Gets or sets the version.
		/// </summary>
		/// <value>
		/// The version.
		/// </value>
		[DataMember(Name = "version")]
		public string Version { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this in the GAC
		/// </summary>
		/// <value>
		///   <c>true</c> if gac; otherwise, <c>false</c>.
		/// </value>
		[DataMember(Name = "gac")]
		public bool Gac { get; set; }

		#endregion
	}
}
