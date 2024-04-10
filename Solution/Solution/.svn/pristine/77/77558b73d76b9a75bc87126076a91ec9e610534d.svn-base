using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Calendar
	/// </summary>
	[DataContract(Name = "calendar")]
	public class Calendar : MobileObject
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
		/// Gets or sets the month separator.
		/// </summary>
		/// <value>
		/// The month separator.
		/// </value>
		[DataMember(Name = "/")]
		public string DateSeparator { get; set; }

		/// <summary>
		/// Gets or sets the time separator.
		/// </summary>
		/// <value>
		/// The time separator.
		/// </value>
		[DataMember(Name = ":")]
		public string TimeSeparator { get; set; }

		/// <summary>
		/// Gets or sets the first day.
		/// </summary>
		/// <value>
		/// The first day.
		/// </value>
		[DataMember(Name = "firstDay")]
		public int FirstDay { get; set; }

		/// <summary>
		/// Gets or sets the days.
		/// </summary>
		/// <value>
		/// The days.
		/// </value>
		[DataMember(Name = "days")]
		public CalendarDays Days { get; set; }

		/// <summary>
		/// Gets or sets the months.
		/// </summary>
		/// <value>
		/// The months.
		/// </value>
		[DataMember(Name = "months")]
		public CalendarMonths Months { get; set; }

		/// <summary>
		/// Gets or sets the morning.
		/// </summary>
		/// <value>
		/// The morning.
		/// </value>
		[DataMember(Name = "AM")]
		public List<string> AMDesignator { get; set; }

		/// <summary>
		/// Gets or sets the afternoon.
		/// </summary>
		/// <value>
		/// The afternoon.
		/// </value>
		[DataMember(Name = "PM")]
		public List<string> PMDesignator { get; set; }

		/// <summary>
		/// Gets or sets the eras.
		/// </summary>
		/// <value>
		/// The eras.
		/// </value>
		[DataMember(Name = "eras")]
		public List<CalendarEra> Eras { get; set; }

		/// <summary>
		/// Gets or sets the two digit year maximum.
		/// </summary>
		/// <value>
		/// The two digit year maximum.
		/// </value>
		[DataMember(Name = "twoDigitYearMax")]
		public int TwoDigitYearMax { get; set; }

		/// <summary>
		/// Gets or sets the patterns.
		/// </summary>
		/// <value>
		/// The patterns.
		/// </value>
		[DataMember(Name = "patterns")]
		public CalendarPatterns Patterns { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public Calendar() : this(CultureInfo.CurrentCulture.Calendar, CultureInfo.CurrentCulture.DateTimeFormat)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public Calendar(System.Globalization.Calendar calendar, DateTimeFormatInfo dt)
		{
			// Eras just in case we have a Flintones tablet...

			Eras = new List<CalendarEra>();

			foreach (var eraNumber in calendar.Eras)
			{
				string name = dt.GetEraName(eraNumber);
				CalendarEra era = new CalendarEra();
				era.Name = name;

				Eras.Add(era);
			}

			Name = calendar.ToString();

			Days = new CalendarDays();
			Days.Names = new List<string>(dt.DayNames);
			Days.Abbreviated = new List<string>(dt.AbbreviatedDayNames);
			Days.Shortest = new List<string>(dt.ShortestDayNames);

			DateSeparator = dt.DateSeparator;
			TimeSeparator = dt.TimeSeparator;
			FirstDay = (int)dt.FirstDayOfWeek;
			
			Months = new CalendarMonths();
			Months.Names = new List<string>(dt.MonthNames);
			Months.Abbreviated = new List<string>(dt.AbbreviatedMonthNames);

			AMDesignator = new List<string>();
			AMDesignator.Add(dt.AMDesignator);

			PMDesignator = new List<string>();
			PMDesignator.Add(dt.PMDesignator);

			TwoDigitYearMax = calendar.TwoDigitYearMax;

			Patterns = new CalendarPatterns();
			Patterns.ShortTime = dt.ShortTimePattern;
			Patterns.LongTime = dt.LongTimePattern;
			Patterns.MonthDay = dt.MonthDayPattern;
			Patterns.Sortable = dt.SortableDateTimePattern;
			Patterns.LongDate = dt.LongDatePattern;
			Patterns.ShortDate = dt.ShortDatePattern;
			Patterns.YearMonth = dt.YearMonthPattern;
			Patterns.FullDateTime = dt.FullDateTimePattern;
		}

		#endregion
	}
}
