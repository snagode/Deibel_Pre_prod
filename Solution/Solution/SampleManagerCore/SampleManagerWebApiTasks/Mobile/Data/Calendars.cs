using System.Globalization;
using System.Runtime.Serialization;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Login Localize Calendars
	/// </summary>
	[DataContract(Name="calendars")]
	public class Calendars : MobileObject
	{
		#region Properties

		/// <summary>
		/// Gets or sets the standard calendar
		/// </summary>
		/// <value>
		/// The standard.
		/// </value>
		[DataMember(Name = "standard")]
		public Calendar Standard { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public Calendars() : this (CultureInfo.CurrentCulture.Calendar, CultureInfo.CurrentCulture.DateTimeFormat)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object" /> class.
		/// </summary>
		/// <param name="calendar">The calendar.</param>
		/// <param name="dt">The dt.</param>
		public Calendars(System.Globalization.Calendar calendar, DateTimeFormatInfo dt)
		{
			Standard = new Calendar(calendar, dt);
		}

		#endregion
	}
}
