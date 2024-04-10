using System;
using System.Collections;
using System.Collections.Generic;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks.BusinessObjects
{
	/// <summary>
	/// Schedule to a Calendar Control
	/// </summary>
	public class ScheduleToCalendar
	{
		#region Generate

		/// <summary>
		/// Generates the events.
		/// </summary>
		/// <param name="schedule">The schedule.</param>
		/// <param name="start">The start.</param>
		/// <param name="end">The end.</param>
		/// <param name="calendar">The calendar.</param>
		public static void Generate(Schedule schedule, DateTime start, DateTime end, Calendar calendar)
		{
			IList<Schedule> schedules = new List<Schedule>();
			schedules.Add(schedule);
			Generate(schedules, start, end, calendar);
		}

		/// <summary>
		/// Generates the events.
		/// </summary>
		/// <param name="scheduler">The scheduler.</param>
		/// <param name="schedule">The schedule.</param>
		/// <param name="start">The start.</param>
		/// <param name="end">The end.</param>
		/// <param name="calendar">The calendar.</param>
		public static void Generate(Scheduler scheduler, Schedule schedule, DateTime start, DateTime end, Calendar calendar)
		{
			IList<Schedule> schedules = new List<Schedule>();
			schedules.Add(schedule);
			Generate(scheduler, schedules, start, end, calendar);
		}

		/// <summary>
		/// Generate Events and add to the Calendar
		/// </summary>
		/// <param name="schedules">The schedules.</param>
		/// <param name="start">The start.</param>
		/// <param name="end">The end.</param>
		/// <param name="calendar">The calendar.</param>
		public static void Generate(IEnumerable schedules, DateTime start, DateTime end, Calendar calendar)
		{
			Scheduler scheduler = new Scheduler();

			scheduler.UsePreSchedule = true;
			scheduler.UseActive = false;
			scheduler.UseLastLogin = false;
			scheduler.UseRunWindow = false;

			Generate(scheduler, schedules, start, end, calendar);
		}

		/// <summary>
		/// Generate Events and add to the Calendar
		/// </summary>
		/// <param name="scheduler">The scheduler.</param>
		/// <param name="schedules">The schedules.</param>
		/// <param name="start">The start.</param>
		/// <param name="end">The end.</param>
		/// <param name="calendar">The calendar.</param>
		public static IList<SchedulePointEvent> Generate(Scheduler scheduler, IEnumerable schedules, DateTime start,
		                                                 DateTime end, Calendar calendar)
		{
			// Run the Schedule

			IList<SchedulePointEvent> samplingEvents = scheduler.ProcessSchedules(schedules, start, end);

			// Raise Errors

			if (scheduler.LastException != null)
			{
				throw (scheduler.LastException);
			}

			// Load in the new Events

			Populate(samplingEvents, calendar);

			return samplingEvents;
		}

		/// <summary>
		/// Populate the Calendar with events
		/// </summary>
		/// <param name="samplingEvents">The sampling events.</param>
		/// <param name="calendar">The calendar.</param>
		public static void Populate(IList<SchedulePointEvent> samplingEvents, Calendar calendar)
		{
			calendar.ClearUnboundEvents();
			if (samplingEvents == null || samplingEvents.Count == 0) return;
			List<UnboundCalendarEvent> unboundEvents = new List<UnboundCalendarEvent>();

			foreach (SchedulePointEvent samplingEvent in samplingEvents)
			{
				UnboundCalendarEvent schedulerEvent = new UnboundCalendarEvent();

				if (samplingEvent.LoginEvent > samplingEvent.SamplingEvent)
				{
					schedulerEvent.Start = samplingEvent.SamplingEvent;
					schedulerEvent.End = samplingEvent.LoginEvent;
				}
				else
				{
					schedulerEvent.Start = samplingEvent.LoginEvent;
					schedulerEvent.End = samplingEvent.SamplingEvent;
				}

				schedulerEvent.Subject = samplingEvent.GetSubjectText();
				schedulerEvent.Color = samplingEvent.SchedulePoint.Color;
				schedulerEvent.Description = samplingEvent.GetDescriptionText();

				unboundEvents.Add(schedulerEvent);
			}

			// Send the events to the client scheduler control

			calendar.AddUnboundEvents(unboundEvents);
		}

		#endregion
	}
}