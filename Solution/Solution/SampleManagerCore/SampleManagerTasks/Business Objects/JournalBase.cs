using System;
using System.Collections.Generic;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks.BusinessObjects
{
	/// <summary>
	/// JournalBase class
	/// </summary>
	public abstract class JournalBase : IJournal
	{
		#region Member Variables

		private IEntityCollection m_JournalCollection;
		private IList<JournalInterval> m_JournalIntervals;

		#endregion

		#region IJournal

		/// <summary>
		/// Entity Type for journal entries
		/// </summary>
		public string EntityType { get; protected set; }

		/// <summary>
		/// Entity for journal
		/// </summary>
		public IEntity Entity { get; protected set; }

		/// <summary>
		/// Title for Journal Form
		/// </summary>
		public string FormTitle { get; protected set; }

		/// <summary>
		/// Journal Start Date
		/// </summary>
		public NullableDateTime Start { get; private set; }

		/// <summary>
		/// Journal End Date
		/// </summary>
		public NullableDateTime End { get; private set; }

		/// <summary>
		/// Journal Entry collection
		/// </summary>
		public IEntityCollection JournalCollection
		{
			get { return m_JournalCollection; }
		}

		/// <summary>
		/// Journal Interval list
		/// </summary>
		public IList<JournalInterval> JournalIntervals
		{
			get { return m_JournalIntervals; }
		}

		/// <summary>
		/// Get the Journal entries as calendar events
		/// </summary>
		/// <returns></returns>
		public virtual IList<UnboundCalendarEvent> GetJournalAsCalendarEvents()
		{
			List<UnboundCalendarEvent> listOfEvents = new List<UnboundCalendarEvent>();

			foreach (JournalInterval interval in JournalIntervals)
			{
				UnboundCalendarEvent intervalEvent = IntervalToEvent(interval);
				listOfEvents.Add(intervalEvent);
			}

			return listOfEvents;
		}

		/// <summary>
		/// Fill the journal
		/// </summary>
		public void Fill()
		{
			LoadEntries();
			GetJournalIntervals();
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="entity">The entity.</param>
		protected JournalBase(IEntity entity)
		{
			EntityType = null;
			Entity = entity;
			Start = new NullableDateTime();
			End = new NullableDateTime();
		}

		#endregion

		#region Load Entries

		/// <summary>
		/// Load the journal collection
		/// </summary>
		private void LoadEntries()
		{
			IQuery query = Entity.EntityManager.CreateQuery(EntityType);

			query.AddEquals(WorkflowJournalPropertyNames.TableName, Entity.EntityType);
			query.AddEquals(WorkflowJournalPropertyNames.EntityRecord, Entity.IdentityString);
			query.AddOrder(WorkflowJournalPropertyNames.PerformedOn, true);

			m_JournalCollection = Entity.EntityManager.Select(EntityType, query);

			if (m_JournalCollection.Count > 0)
			{
				Start = GetEntryPerformedOn(m_JournalCollection[0]);
				End = GetEntryPerformedOn(m_JournalCollection[m_JournalCollection.Count - 1]);
			}
		}

		/// <summary>
		/// Get the journal intervals
		/// </summary>
		private void GetJournalIntervals()
		{
			m_JournalIntervals = new List<JournalInterval>();

			if (JournalCollection.Count > 0)
			{
				Dictionary<string, DateTime> openPeriods = new Dictionary<string, DateTime>();
				Dictionary<string, DateTime> tempPeriods = new Dictionary<string, DateTime>();

				// Work with the first entry

				DateTime startDate = GetEntryPerformedOn(m_JournalCollection[0]).Value;
				string states = GetExitValue(m_JournalCollection[0]);
				string[] stateList = states.Split(new [] {','});

				foreach (string t in stateList)
					openPeriods.Add(t, startDate);

				// Work with the rest of entries

				for (int j = 1; j < m_JournalCollection.Count; j++)
				{
					DateTime date = GetEntryPerformedOn(m_JournalCollection[j]).Value;
					states = GetExitValue(m_JournalCollection[j]);

					stateList = states.Split(new [] {','});
					foreach (string t in stateList)
					{
						if (openPeriods.ContainsKey(t))
						{
							// Period doesn't end yet

							tempPeriods.Add(t, openPeriods[t]);
							openPeriods.Remove(t);
						}
						else
						{
							// Period starts

							tempPeriods.Add(t, date);
						}
					}

					// Remaining periods in temp are ended

					foreach (string key in openPeriods.Keys)
					{
						JournalInterval newPeriod = new JournalInterval(key, new NullableDateTime(openPeriods[key]), new NullableDateTime(date));
						m_JournalIntervals.Add(newPeriod);
					}

					// Copy from temporal dictionary to open period

					openPeriods.Clear();

					foreach (string key in tempPeriods.Keys)
						openPeriods.Add(key, tempPeriods[key]);

					tempPeriods.Clear();
				}

				// We need to close the last interval

				if (openPeriods.Count > 0)
				{
					// There are still states that are opened date

					foreach (string key in openPeriods.Keys)
					{
						JournalInterval newPeriod = new JournalInterval(key, new NullableDateTime(openPeriods[key]));
						m_JournalIntervals.Add(newPeriod);
					}
				}
			}
		}

		#endregion

		#region Abstract Property Get Functions

		/// <summary>
		/// Get the entry name
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		protected abstract string GetEntryName(IEntity entity);

		/// <summary>
		/// Get the enter values
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		protected abstract string GetEnterValue(IEntity entity);

		/// <summary>
		/// Get the exit values
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		protected abstract string GetExitValue(IEntity entity);

		/// <summary>
		/// Get the performed date
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		protected abstract NullableDateTime GetEntryPerformedOn(IEntity entity);

		/// <summary>
		/// Get the entry type
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		protected abstract string GetEntryType(IEntity entity);

		/// <summary>
		/// Get the performed by
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		protected abstract string GetEntryPerformedBy(IEntity entity);

		#endregion

		#region Utility Methods

		/// <summary>
		/// Convert an Journal Interval to an unboundcalendar event
		/// </summary>
		/// <param name="interval">The Journal Interval</param>
		/// <returns></returns>
		protected virtual UnboundCalendarEvent IntervalToEvent(JournalInterval interval)
		{
			UnboundCalendarEvent calendarEvent = new UnboundCalendarEvent();

			calendarEvent.AllDay = false;
			calendarEvent.Start = interval.StartOn.Value;

			bool openPeriod = interval.EndOn.IsNull;
			calendarEvent.End = (openPeriod) ? DateTime.Now : interval.EndOn.Value;
			calendarEvent.Subject = interval.Name;

			string description;

			if (openPeriod)
			{
				string format = ServerMessageManager.Current.GetMessage("WorkflowMessages", "WorkflowJournalOpenDescription");
				description = string.Format(format, calendarEvent.Start);
			}
			else
			{
				string format = ServerMessageManager.Current.GetMessage("WorkflowMessages", "WorkflowJournalPeriodDescription");
				description = string.Format(format, calendarEvent.Start, calendarEvent.End);
			}

			calendarEvent.Description = description;

			return calendarEvent;
		}

		#endregion
	}
}