using System;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the CALENDAR entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class ScheduleCalendar : ScheduleCalendarBase
	{
		#region Date Ranges

		/// <summary>
		/// Validates the periods.
		/// </summary>
		/// <returns></returns>
		public bool ValidatePeriods()
		{
			bool isValid = true;

			foreach (ScheduleCalendarPeriod period in ScheduleCalendarPeriods)
			{
				if (period.StartDate.IsNull || period.EndDate.IsNull)
				{
					isValid = false;
					break;
				}
			}

			return isValid;
		}

		/// <summary>
		/// Checks a date against all non working periods.
		/// </summary>
		/// <param name="dateTime">The date time.</param>
		/// <param name="isPublicHoliday">if set to <c>true</c> [is public holiday].</param>
		/// <param name="isDowntime">if set to <c>true</c> [is downtime].</param>
		public void CheckDate(DateTime dateTime, out bool isPublicHoliday, out bool isDowntime)
		{
			isPublicHoliday = false;
			isDowntime = false;

			foreach (ScheduleCalendarPeriod nonWorkingPeriod in ScheduleCalendarPeriods)
			{
				// Is the start date within a non working period

				DateTime start = nonWorkingPeriod.StartDate.Value;
				DateTime end = nonWorkingPeriod.EndDate.Value;
				string type = nonWorkingPeriod.Type.PhraseId;

				if (dateTime >= start && dateTime <= end)
				{
					// We are in a non working period so check what the reason is for this

					isPublicHoliday = (isPublicHoliday) ? true : type == PhraseNonWork.PhraseIdPUBLIC_HOL;
					isDowntime = (isDowntime) ? true : type == PhraseNonWork.PhraseIdPLANT;

					if (isPublicHoliday && isDowntime)
					{
						break;
					}
				}
			}
		}

		#endregion
	}
}