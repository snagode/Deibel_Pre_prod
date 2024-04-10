using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Calendar Days
	/// </summary>
	[DataContract(Name="calendardays")]
	public class CalendarDays : MobileObject
	{
		/// <summary>
		/// Gets or sets the names.
		/// </summary>
		/// <value>
		/// The names.
		/// </value>
		[DataMember(Name = "names")]
		public List<string> Names { get; set; }

		/// <summary>
		/// Gets or sets the names abbreviations.
		/// </summary>
		/// <value>
		/// The names abbreviations.
		/// </value>
		[DataMember(Name = "namesAbbr")]
		public List<string> Abbreviated { get; set; }

		/// <summary>
		/// Gets or sets the short names
		/// </summary>
		/// <value>
		/// The names (short)
		/// </value>
		[DataMember(Name = "namesShort")]
		public List<string> Shortest { get; set; }
	}
}
