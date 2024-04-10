using System;
using System.Collections;
using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Utilities;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks.BusinessObjects
{
	/// <summary>
	/// Processes Schedule(s) in order to generate a list of items from which samples will be logged into the system.
	/// </summary>
	public class Scheduler : LogMessaging
	{
		#region Member Variables

		private DateTime m_End;
		private Exception m_LastException;
		private DateTime? m_Next;
		private TimeSpan m_PreSchedule;
		private IList<SchedulePointEvent> m_SampleEvents;
		private DateTime m_Start;
		private bool m_UseActive;
		private bool m_UseLastLogin;
		private bool m_UsePreSchedule;
		private bool m_UseRunWindow;

		private Schedule m_Schedule;
		private SchedulePoint m_SchedulePoint;
		private ScheduleCalendar m_Calendar;

		#endregion

		#region Properties

		/// <summary>
		/// The time of the event AFTER the last scheduled event
		/// </summary>
		/// <value>The next event.</value>
		public DateTime? Next
		{
			get { return m_Next; }
		}

		/// <summary>
		/// Gets the sample events.
		/// </summary>
		/// <value>The sample events.</value>
		public IList<SchedulePointEvent> SampleEvents
		{
			get { return m_SampleEvents; }
		}

		/// <summary>
		/// Gets the start.
		/// </summary>
		/// <value>The start.</value>
		public DateTime Start
		{
			get { return m_Start; }
		}

		/// <summary>
		/// Gets the end.
		/// </summary>
		/// <value>The end.</value>
		public DateTime End
		{
			get { return m_End; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to pre schedule.
		/// </summary>
		/// <value><c>true</c> if pre schedule; otherwise, <c>false</c>.</value>
		public bool UsePreSchedule
		{
			get { return m_UsePreSchedule; }
			set { m_UsePreSchedule = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether use the last login date
		/// </summary>
		/// <value><c>true</c> if use last login; otherwise, <c>false</c>.</value>
		public bool UseLastLogin
		{
			get { return m_UseLastLogin; }
			set { m_UseLastLogin = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to use active only
		/// </summary>
		/// <value><c>true</c> if use active; otherwise, <c>false</c>.</value>
		public bool UseActive
		{
			get { return m_UseActive; }
			set { m_UseActive = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to use the run window.
		/// </summary>
		/// <value><c>true</c> if using run window; otherwise, <c>false</c>.</value>
		public bool UseRunWindow
		{
			get { return m_UseRunWindow; }
			set { m_UseRunWindow = value; }
		}

		/// <summary>
		/// Gets the last exception.
		/// </summary>
		/// <value>The last exception.</value>
		public Exception LastException
		{
			get { return m_LastException; }
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="Scheduler"/> class.
		/// </summary>
		public Scheduler()
		{
			m_SampleEvents = new List<SchedulePointEvent>();

			m_UsePreSchedule = true;
			m_UseActive = true;
			m_UseLastLogin = true;
			m_UseRunWindow = true;
		}

		#endregion

		#region Main Process

		/// <summary>
		/// Checks all Schedules and returns a list of SamplePointAnalysis objects based on their Frequency.
		/// </summary>
		/// <param name="schedules">The schedules.</param>
		/// <returns>
		/// A list of SamplePointAnalysis objects whose frequency matches startTime and nestTime.
		/// </returns>
		public IList<SchedulePointEvent> ProcessSchedules(IEntityCollection schedules)
		{
			TimeSpan window = new TimeSpan(0, 1, 0);
			return ProcessSchedules(schedules, DateTime.Now.Subtract(window), DateTime.Now.Add(window));
		}

		/// <summary>
		/// Checks all Schedules and returns a list of SamplePointAnalysis objects based on their Frequency.
		/// </summary>
		/// <param name="schedules">The schedules.</param>
		/// <param name="startWindow">The time from which this all frequencies will be checked.</param>
		/// <param name="endWindow">The next scheduled time at which all frequencies will be checked.</param>
		/// <returns>
		/// A list of SamplePointAnalysis objects whose frequency matches startTime and nestTime.
		/// </returns>
		public IList<SchedulePointEvent> ProcessSchedules(IEnumerable schedules, DateTime startWindow, DateTime endWindow)
		{
			m_Start = startWindow;
			m_End = endWindow;
			m_Next = null;
			m_LastException = null;
			m_SampleEvents = new List<SchedulePointEvent>();

			// Turn on Logging

			StartLogging();
			ClearLogging();

			// Output the parameters

			Logger.InfoFormat("Starting to Schedule from : {0} to {1}", startWindow, endWindow);
			Logger.InfoFormat("Preschedule {0}, Active Only {1}, Using Last Login {2}, Using Run Window {3}",
			                  m_UsePreSchedule, m_UseActive, m_UseLastLogin, m_UseRunWindow);

			// Do the Scheduling

			foreach (Schedule schedule in schedules)
			{
				// Discard Inactive

				if (m_UseActive && !schedule.Active)
				{
					Logger.InfoFormat("Schedule Inactive {0}", schedule.Name);
					continue;
				}

				// Discard Pending

				if (!schedule.StartsBefore(End))
				{
					Logger.InfoFormat("Schedule Pending {0} - Starts at {1}", schedule.Name, schedule.StartDate);
					continue;
				}

				// Discard Completed

				if (!schedule.EndsAfter(Start))
				{
					Logger.InfoFormat("Schedule Completed {0} - Ended {1}", schedule.Name, schedule.EndDate);
					continue;
				}

				// Process the schedule, catching and logging issues if they arise.

				try
				{
					ProcessSchedule(schedule);
				}
				catch (Exception e)
				{
					Logger.ErrorFormat("Error processing schedule {0}. {1}", schedule.Name, e.Message);
					Logger.Debug("Exception Details", e);
					m_LastException = e;
				}
			}

			// Stop Logging

			Logger.InfoFormat("Finished Scheduling - Found {0} Events", m_SampleEvents.Count);
			StopLogging();

			return m_SampleEvents;
		}

		#endregion

		#region Processing Methods

		/// <summary>
		/// Generate Events for the given schedule
		/// </summary>
		/// <param name="schedule">The schedule.</param>
		private void ProcessSchedule(Schedule schedule)
		{
			m_Calendar = (ScheduleCalendar) schedule.Calendar;
			m_Schedule = schedule;

			ProcessSchedule();
		}

		/// <summary>
		/// Processes the schedule.
		/// </summary>
		private void ProcessSchedule()
		{
			Logger.InfoFormat("Processing Schedule '{0}'", m_Schedule.Name);

			// Adjust Window

			DateTime start = Start;
			DateTime end = End;

			if (m_Schedule.StartsAfter(start))
			{
				start = m_Schedule.StartDate.Value;
				Logger.InfoFormat("Schedule Start '{0}' adjusted to the start of the Schedule {1}", m_Schedule.Name, Start, start);
			}

			if (m_Schedule.EndsBefore(end))
			{
				end = m_Schedule.EndDate.Value;
				Logger.InfoFormat("Schedule End '{0}' adjusted to the end of the Schedule {1}", m_Schedule.Name, End, end);
			}

			// Process all Schedule Points

			foreach (SchedulePoint point in m_Schedule.SchedulePoints)
			{
				// Discard Inactive

				ProcessSchedulePoint(start, end, point);
			}
		}

		/// <summary>
		/// Processes the schedule point.
		/// </summary>
		/// <param name="start">The start.</param>
		/// <param name="end">The end.</param>
		/// <param name="point">The point.</param>
		private void ProcessSchedulePoint(DateTime start, DateTime end, SchedulePoint point)
		{
			m_SchedulePoint = point;
			ProcessSchedulePoint(start, end);
		}

		/// <summary>
		/// Processes the schedule point.
		/// </summary>
		/// <param name="start">The start.</param>
		/// <param name="end">The end.</param>
		private void ProcessSchedulePoint(DateTime start, DateTime end)
		{
			if (m_UseActive && ! m_SchedulePoint.Active)
			{
				Logger.InfoFormat("Schedule Point Inactive - '{0}'", m_SchedulePoint.Name);
				return;
			}

			// Discard Null

			if (m_SchedulePoint.SamplePoint.IsNull())
			{
				Logger.WarnFormat("Sample Point Null - '{0}'", m_SchedulePoint.Name);
				return;
			}

			// Discard Removed

			if (m_SchedulePoint.SamplePoint.Removeflag)
			{
				Logger.WarnFormat("Sample Point Removed '{0}' - '{1}'", m_SchedulePoint.SamplePoint.Name, m_SchedulePoint.Name);
				return;
			}

			// Tell the world what we are up to

			Logger.InfoFormat("Processing Schedule Point '{0}'", m_SchedulePoint.Name);

			// If there is a pre schedule then we need to act as if the current time is in the future

			DateTime scheduleEndWindow = GetScheduleEndWindow(end);

			// Get the Start of the Window - this will look up the last login date

			DateTime scheduleStartWindow = GetScheduleStartWindow(start, scheduleEndWindow);
			if (scheduleStartWindow > scheduleEndWindow) return;

			// Schedule the Items

			ProcessAnalyses(scheduleStartWindow, scheduleEndWindow);
			ProcessTestSchedules(scheduleStartWindow, scheduleEndWindow);
		}

		/// <summary>
		/// Gets the schedule start window.
		/// </summary>
		/// <param name="start">The start.</param>
		/// <param name="scheduleEndWindow">The schedule end window.</param>
		/// <returns></returns>
		private DateTime GetScheduleStartWindow(DateTime start, DateTime scheduleEndWindow)
		{
			DateTime scheduleStartWindow = start;

			if (m_UseLastLogin && !m_SchedulePoint.LastLogin.IsNull)
			{
				DateTime lastLogin = m_SchedulePoint.LastLogin.Value;
				Logger.DebugFormat("Last Login for '{0}' was {1}", m_SchedulePoint, lastLogin);

				if (lastLogin > scheduleEndWindow)
				{
					scheduleStartWindow = lastLogin;
					Logger.DebugFormat("Previous Sample Logged in at {0}, which is after the End Window {1} - Nothing to do.", lastLogin,
					                  scheduleEndWindow);
				}
				if (lastLogin >= scheduleStartWindow)
				{
					scheduleStartWindow = lastLogin.AddSeconds(1);
					Logger.DebugFormat("Changing start window to be just after last login - {0}", scheduleStartWindow);
				}
				else
				{
					Logger.DebugFormat("Note that Sample Events for '{0}' were missed between {1} and {2}.",
					                  m_SchedulePoint, lastLogin, scheduleStartWindow);
				}
			}

			return scheduleStartWindow;
		}

		/// <summary>
		/// Gets the schedule end window - honoring the preschedule
		/// </summary>
		/// <param name="end">The end.</param>
		/// <returns></returns>
		private DateTime GetScheduleEndWindow(DateTime end)
		{
			DateTime scheduleEndWindow = end;
			m_PreSchedule = m_SchedulePoint.PreSchedule;

			if (m_UsePreSchedule && m_PreSchedule != TimeSpan.Zero)
			{
				scheduleEndWindow = end.Add(m_PreSchedule);
				Logger.DebugFormat("End Period extended by Preshedule {0} to {1}", m_PreSchedule, scheduleEndWindow);
			}

			return scheduleEndWindow;
		}

		/// <summary>
		/// Processes the analyses.
		/// </summary>
		/// <param name="startWindow">The start window.</param>
		/// <param name="endWindow">The end window.</param>
		private void ProcessAnalyses(DateTime startWindow, DateTime endWindow)
		{
			foreach (SchedulePointAnalysis item in m_SchedulePoint.SchedulePointAnalysis)
			{
				// Discard Null

				if (string.IsNullOrEmpty(item.Analysis))
				{
					Logger.WarnFormat("Analysis Blank - '{0}'", item.Name);
					continue;
				}

				Logger.DebugFormat("Processing Schedule Point Analysis '{0}'", item.Name);
				ProcessItem(item, startWindow, endWindow);
			}
		}

		/// <summary>
		/// Processes the test schedules
		/// </summary>
		/// <param name="startWindow">The start window.</param>
		/// <param name="endWindow">The end window.</param>
		private void ProcessTestSchedules(DateTime startWindow, DateTime endWindow)
		{
			foreach (SchedulePointTestSchedule item in m_SchedulePoint.SchedulePointTestSchedules)
			{
				// Discard Null

				if (item.TestSchedule.IsNull())
				{
					Logger.WarnFormat("Test Schedule Null - '{0}'", item.Name);
					continue;
				}

				// Discard Removed

				if (item.TestSchedule.Removeflag)
				{
					Logger.WarnFormat("Test Schedule Removed '{0}' - '{1}'", item.TestSchedule.Name, item.Name);
					continue;
				}

				Logger.DebugFormat("Processing Schedule Point Test Schedule '{0}'", item.Name);
				ProcessItem(item, startWindow, endWindow);
			}
		}

		/// <summary>
		/// Processes the item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="startWindow">The start window.</param>
		/// <param name="endWindow">The end window.</param>
		private void ProcessItem(ISchedulePointItem item, DateTime startWindow, DateTime endWindow)
		{
			// Work out how far back we need to check for missed items

			DateTime scheduleStartWindow = startWindow;

			if (m_UseRunWindow && item.RunWindow != TimeSpan.Zero)
			{
				scheduleStartWindow = startWindow.Subtract(item.RunWindow);
				Logger.DebugFormat("Start Period extended by Run Window {0} to {1}", item.RunWindow, scheduleStartWindow);
			}

			// When did we last do something?

			if (m_UseLastLogin && !m_SchedulePoint.LastLogin.IsNull)
			{
				DateTime lastLogin = m_SchedulePoint.LastLogin.Value;
				Logger.DebugFormat("Last Login for '{0}' was {1}", m_SchedulePoint, lastLogin);

				if (lastLogin > endWindow)
				{
					Logger.DebugFormat("Previous Sample Logged in at {0}, which is after the End Window {1}. Nothing to do.", lastLogin,
					                   endWindow);
					return;
				}
				if (lastLogin >= scheduleStartWindow)
				{
					scheduleStartWindow = lastLogin.AddSeconds(1);
					Logger.DebugFormat("Changing start window to be just after last login - {0}", scheduleStartWindow);
				}
				else
				{
					Logger.DebugFormat("Sample Events for '{0}' missed between {1} and {2}. Fell outside of run window",
					                   m_SchedulePoint, lastLogin, scheduleStartWindow);
				}
			}

			// Walk from the begining of the login window to the next time the scheduler will run

			DateTime day = scheduleStartWindow.Date;

			while (day <= endWindow.Date)
			{
				ProcessDay(item, day, scheduleStartWindow, endWindow);
				day = day.AddDays(1);
			}
		}

		/// <summary>
		/// Processes the day.
		/// </summary>
		/// <param name="pointAnalysis">The sp analysis.</param>
		/// <param name="day">The day.</param>
		/// <param name="startWindow">The login window start.</param>
		/// <param name="nextTime">The next time.</param>
		private void ProcessDay(ISchedulePointItem pointAnalysis, DateTime day, DateTime startWindow, DateTime nextTime)
		{
			// If this day is not valid then times are irrelevant

			if (!pointAnalysis.FrequencyObject.IsDateInFrequency(day))
			{
				Logger.DebugFormat("Date {0} does not fall within the frequency", day.Date);
				return;
			}

			Logger.DebugFormat("Date {0} is relevant and falls within the frequency", day.Date);

			if (pointAnalysis.FrequencyObject.TimeMode == TimeMode.Range)
				ScheduleRange(pointAnalysis, day, startWindow, nextTime);
			else
				ScheduleTimes(pointAnalysis, day, startWindow, nextTime);
		}

		/// <summary>
		/// Schedules a range of times in a given dat
		/// </summary>
		/// <param name="item">The point analysis.</param>
		/// <param name="day">The day.</param>
		/// <param name="startWindow">The start window.</param>
		/// <param name="endWindow">The next time.</param>
		private void ScheduleRange(ISchedulePointItem item, DateTime day, DateTime startWindow, DateTime endWindow)
		{
			// Generate a datetime from which to start processing

			DateTime scheduledTime = day.Add(item.FrequencyObject.StartTime);
			DateTime dayEnd = day.Add(item.FrequencyObject.EndTime);

			// Walk from the start of the login window the next scheduled run time or the end of the time range for this day

			while ((scheduledTime < endWindow) && (scheduledTime <= dayEnd))
			{
				ScheduleEvent(item, scheduledTime, startWindow, endWindow);
				scheduledTime = scheduledTime.Add(item.FrequencyObject.TimeFrequency);
			}

			// Keep track of the next event time after this window

			if (scheduledTime > endWindow)
				SetNextTime(scheduledTime);

			Logger.DebugFormat("Last Time Considered {0} - End Window {1}, End Day {2}", scheduledTime, endWindow, dayEnd);
		}

		/// <summary>
		/// Schedules the times.
		/// </summary>
		/// <param name="item">The sp analysis.</param>
		/// <param name="day">The day.</param>
		/// <param name="startWindow">The login window start.</param>
		/// <param name="endWindow">The next time.</param>
		private void ScheduleTimes(ISchedulePointItem item, DateTime day, DateTime startWindow, DateTime endWindow)
		{
			foreach (TimeSpan specificTime in item.FrequencyObject.SpecificTimes)
			{
				DateTime scheduledTime = day.Add(specificTime);
				ScheduleEvent(item, scheduledTime, startWindow, endWindow);
			}
		}

		/// <summary>
		/// Schedules the event.
		/// </summary>
		/// <param name="item">The point analysis.</param>
		/// <param name="scheduledTime">The scheduled time.</param>
		/// <param name="startWindow">The start window.</param>
		/// <param name="endWindow">The end window.</param>
		private void ScheduleEvent(ISchedulePointItem item, DateTime scheduledTime, DateTime startWindow, DateTime endWindow)
		{
			// Check it is appropriate

			if (scheduledTime < startWindow)
			{
				Logger.DebugFormat("Skipped Event at {0} it is before the start {1}", scheduledTime, startWindow);
				return;
			}

			if (scheduledTime > endWindow)
			{
				Logger.DebugFormat("Skipped Event at {0} it is after the end {1}", scheduledTime, endWindow);
				SetNextTime(scheduledTime);
				return;
			}

			// Create the Event

			AddScheduledItem(item, scheduledTime);
		}

		#endregion

		#region Add Events

		/// <summary>
		/// Adds the scheduled item.
		/// </summary>
		/// <param name="item">The sp analysis.</param>
		/// <param name="visit">The event date time.</param>
		private void AddScheduledItem(ISchedulePointItem item, DateTime visit)
		{
			// Check for invalid events

			if (!ValidTime(visit)) return;

			// Is there an analysis already present for this Sample Point at this time?

			SchedulePointEvent pointEvent = FindSamplePointEvent(visit);

			if (pointEvent == null)
			{
				Logger.InfoFormat("Created event for SamplePoint '{0}' at time {1}", m_SchedulePoint.SamplePoint.Name, visit);

				// This is the first analysis for this Sampling Point so create a Sampling event

				DateTime loginTime = visit;
				if (m_UsePreSchedule) loginTime = visit.Subtract(m_PreSchedule);

				pointEvent = new SchedulePointEvent(m_SchedulePoint, loginTime, visit);
				m_SampleEvents.Add(pointEvent);
			}

			Logger.DebugFormat("Adding SamplePointAnalysis '{0}' to SamplePoint '{1}' at time {2}", item.Name,
			                   m_SchedulePoint.SamplePoint.Name, visit.ToLongDateString());

			pointEvent.Items.Add(item);
		}

		/// <summary>
		/// Finds a sample point event.
		/// </summary>
		/// <param name="eventTime">The event time.</param>
		/// <returns></returns>
		private SchedulePointEvent FindSamplePointEvent(DateTime eventTime)
		{
			foreach (SchedulePointEvent pointEvent in m_SampleEvents)
			{
				if (pointEvent.SchedulePoint.Equals(m_SchedulePoint) && (pointEvent.SamplingEvent == eventTime))
					return pointEvent;
			}

			return null;
		}

		/// <summary>
		/// Is this a valid time
		/// </summary>
		/// <param name="dateTime">The date time.</param>
		/// <returns></returns>
		private bool ValidTime(DateTime dateTime)
		{
			return ValidTime(dateTime, m_Calendar, m_SchedulePoint);
		}

		/// <summary>
		/// Is this a valid time
		/// </summary>
		/// <param name="dateTime">The date time.</param>
		/// <param name="scheduleCalendar">The calendar.</param>
		/// <param name="pointEntry">The sp entry.</param>
		/// <returns></returns>
		private bool ValidTime(DateTime dateTime, ScheduleCalendar scheduleCalendar, SchedulePointBase pointEntry)
		{
			if (!scheduleCalendar.IsNull())
			{
				bool holiday, downtime;

				Logger.DebugFormat("Checking '{0}' Calender for Event Date {1}", scheduleCalendar.Name, dateTime);
				scheduleCalendar.CheckDate(dateTime, out holiday, out downtime);

				// Public Holidays

				if (holiday)
				{
					Logger.DebugFormat("Event '{0}' occurs on a holiday {1}", pointEntry, dateTime);
					if (!pointEntry.CollectPublicHoliday)
					{
						Logger.InfoFormat("Event '{0}' Skipped - does not occur on holiday {1}", pointEntry, dateTime);
						return false;
					}
				}

				// Scheduled Downtime

				if (downtime)
				{
					Logger.DebugFormat("Event '{0}' occurs during Downtime {1}", pointEntry, dateTime);
					if (!pointEntry.CollectDowntime)
					{
						Logger.InfoFormat("Event '{0}' Skipped - does not occur during downtime {1}", pointEntry, dateTime);
						return false;
					}
				}
			}

			return true;
		}

		#endregion

		#region Next Time

		/// <summary>
		/// Sets the next time.
		/// </summary>
		/// <param name="nextTime">The next time.</param>
		private void SetNextTime(DateTime nextTime)
		{
			nextTime = nextTime.Subtract(m_PreSchedule);

			if (m_Next == null)
			{
				m_Next = nextTime;
				return;
			}

			if (nextTime < (DateTime) m_Next)
				m_Next = nextTime;
		}

		#endregion
	}
}