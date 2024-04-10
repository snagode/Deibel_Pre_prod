using System;
using System.Runtime.Serialization;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Interval
	/// </summary>
	[DataContract(Name = "interval")]
	public class Interval : MobileObject
	{
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public Interval()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Interval"/> class.
		/// </summary>
		/// <param name="timeSpan">The time span.</param>
		public Interval(TimeSpan timeSpan)
		{
			Negative = timeSpan < TimeSpan.Zero;
			Days = timeSpan.Days;
			Hours = timeSpan.Hours;
			Minutes = timeSpan.Minutes;
			Seconds = timeSpan.Seconds;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Interval"/> is negative.
		/// </summary>
		/// <value>
		///   <c>true</c> if negative; otherwise, <c>false</c>.
		/// </value>
		[DataMember(Name = "negative")]
		public bool Negative { get; set; }

		/// <summary>
		/// Gets or sets the days.
		/// </summary>
		/// <value>
		/// The days.
		/// </value>
		[DataMember(Name = "days")]
		public int Days { get; set; }

		/// <summary>
		/// Gets or sets the hours.
		/// </summary>
		/// <value>
		/// The hours.
		/// </value>
		[DataMember(Name = "hours")]
		public int Hours { get; set; }

		/// <summary>
		/// Gets or sets the minutes.
		/// </summary>
		/// <value>
		/// The minutes.
		/// </value>
		[DataMember(Name = "minutes")]
		public int Minutes { get; set; }

		/// <summary>
		/// Gets or sets the seconds.
		/// </summary>
		/// <value>
		/// The seconds.
		/// </value>
		[DataMember(Name = "seconds")]
		public int Seconds { get; set; }

		/// <summary>
		/// Gets the time span.
		/// </summary>
		/// <value>
		/// The time span.
		/// </value>
		public TimeSpan TimeSpan
		{
			get
			{
				if (Negative) return new TimeSpan(Days, Hours, Minutes, Seconds).Negate();
				return new TimeSpan(Days, Hours, Minutes, Seconds);
			}
		}

		/// <summary>
		/// Gets the string in SM VGL Style Format
		/// </summary>
		/// <value>
		/// The string.
		/// </value>
		public string VglString
		{
			get { return Framework.Utilities.TextUtils.TimeSpanToString(TimeSpan); }
		}

		#endregion

		#region Static Utilities

		/// <summary>
		/// Attempt to convert json text to a VGL friendly string.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <returns>VGL string or the text if not convertible</returns>
		public static string ToVglString(string text)
		{
			try
			{
				var interval = Deserialize<Interval>(text);
				return interval.VglString;
			}
			catch (Exception)
			{
				return text;
			}
		}

		#endregion
	}
}
