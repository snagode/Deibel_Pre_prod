using System;
using System.Globalization;
using Thermo.SampleManager.Library.DesignerRuntime;

namespace Thermo.SampleManager.WebApiTasks.Data
{
	/// <summary>
	/// API Data Object
	/// </summary>
	[Serializable]
	public class Object : MarshalByRefObject
	{
		#region Localization Support

		/// <summary>
		/// Gets the localized string.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <param name="message">The message.</param>
		/// <returns></returns>
		public static string GetLocalizedString(StandardLibrary library, string message)
		{
			try
			{
				string text = library.VGL.GetMessage(message);
				if (text != null && ! text.Equals(message)) return text;
				return library.Message.ConvertTaggedField(message);
			}
			catch (Exception)
			{
				return message;
			}
		}

		#endregion

		#region Timezone Support

		/// <summary>
		/// Gets the server date using a text date in client form
		/// 
		/// The purpose of this function is to take a string from the API and make it into
		/// something that's appropriate for the database. This is used in cases where we
		/// are not using native samplemanager dates - so we need to convert manually.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <param name="text">The text.</param>
		/// <returns></returns>
		public static DateTime? GetServerDate(StandardLibrary library, string text)
		{
			if (string.IsNullOrEmpty(text)) return null;

			// Explicitly remove the UTC marker at the end. We want TryParse to just make a date.
			// If we don't do this daylight saving starts to give problems. So if you must change
			// this make sure you try two dates spanning across the DST boundary.

			DateTime val;
			text = text.TrimEnd('Z');

			if (DateTime.TryParse(text, out val))
			{

				return GetServerDate(library, val);
			}

			return null;
		}

		/// <summary>
		/// Gets the server date text.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <param name="text">The text.</param>
		/// <returns></returns>
		public static string GetServerDateText(StandardLibrary library, string text)
		{
			var serverDate = GetServerDate(library, text);
			if (serverDate == null) return text;
			return serverDate.Value.ToString("s", CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Gets the server date.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <param name="dateTime">The date time.</param>
		/// <returns></returns>
		public static DateTime? GetServerDate(StandardLibrary library, DateTime? dateTime)
		{
			if (dateTime == null) return null;

			var clientTimezone = library.Environment.TimeZoneInfoClient;
			var serverTimezone = library.Environment.TimeZoneInfoServer;

			if (serverTimezone == null) return dateTime;

			// Ensure the date is unspecified - allows dumb conversion.

			var date = DateTime.SpecifyKind((DateTime)dateTime, DateTimeKind.Unspecified);
			return TimeZoneInfo.ConvertTime(date, clientTimezone, serverTimezone);
		}

		/// <summary>
		/// Gets the client date using a text date in server form
		/// 
		/// In general we're going to be turning a server database textual date into UTC
		/// This is used in cases where we are not using a SampleManager date field and
		/// where we have to do date conversion manually.
		///  </summary>
		/// <param name="library">The library.</param>
		/// <param name="text">The text.</param>
		/// <returns></returns>
		public static DateTime? GetClientDate(StandardLibrary library, string text)
		{
			DateTime val;

			if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out val))
			{
				return GetClientDate(library, val);
			}

			return null;
		}

		/// <summary>
		/// Gets the client date text.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <param name="text">The text.</param>
		/// <returns></returns>
		public static string GetClientDateText(StandardLibrary library, string text)
		{
			var clientDate = GetClientDate(library, text);
			if (clientDate == null) return text;
			return clientDate.Value.ToString("s", CultureInfo.InvariantCulture);
		}
		/// <summary>
		/// Gets the client date.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <param name="dateTime">The date time.</param>
		/// <returns></returns>
		public static DateTime? GetClientDate(StandardLibrary library, DateTime? dateTime)
		{
			if (dateTime == null) return null;

			var clientTimezone = library.Environment.TimeZoneInfoClient;
			var serverTimezone = library.Environment.TimeZoneInfoServer;

			if (serverTimezone == null) return dateTime;
			if (clientTimezone == null) return dateTime;

			// Ensure the date is unspecified - allows dumb conversion.

			var date = DateTime.SpecifyKind((DateTime)dateTime, DateTimeKind.Unspecified);
			return TimeZoneInfo.ConvertTime(date, serverTimezone, clientTimezone);
		}

		#endregion
	}
}
