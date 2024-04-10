using System.Collections.Generic;
using System.Drawing;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks.BusinessObjects
{
	/// <summary>
	/// Workflow Journal
	/// </summary>
	public class JournalWorkflow : JournalBase
	{
		#region Constants

		private const string JournalFormTitle = "WorkflowJournalFormTitle";

		#endregion

		#region Member Variables

		private readonly Dictionary<string, WorkflowState> m_States = new Dictionary<string, WorkflowState>();
		private bool m_CachesLoaded;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="entity">The entity.</param>
		public JournalWorkflow(IEntity entity) : base(entity)
		{
			EntityType = TableNames.WorkflowJournal;
			FormTitle = JournalFormTitle;
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Get the entry name
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		protected override string GetEntryName(IEntity entity)
		{
			return entity.GetString(WorkflowJournalPropertyNames.EntryName);
		}

		/// <summary>
		/// Get the performed date
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		protected override NullableDateTime GetEntryPerformedOn(IEntity entity)
		{
			return entity.GetNullableDateTime(WorkflowJournalPropertyNames.PerformedOn);
		}

		/// <summary>
		/// Get the exit values
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		protected override string GetExitValue(IEntity entity)
		{
			return entity.GetString(WorkflowJournalPropertyNames.ExitStates);
		}

		/// <summary>
		/// Get the enter values
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		protected override string GetEnterValue(IEntity entity)
		{
			return entity.GetString(WorkflowJournalPropertyNames.EnterStates);
		}

		/// <summary>
		/// Get the performed by
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		protected override string GetEntryPerformedBy(IEntity entity)
		{
			return entity.GetString(WorkflowJournalPropertyNames.PerformedBy);
		}

		/// <summary>
		/// Get the entry type
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		protected override string GetEntryType(IEntity entity)
		{
			return entity.GetString(WorkflowJournalPropertyNames.EntryType);
		}

		/// <summary>
		/// Convert an Journal Interval to an unboundcalendar event
		/// </summary>
		/// <param name="interval">The Journal Interval</param>
		/// <returns></returns>
		protected override UnboundCalendarEvent IntervalToEvent(JournalInterval interval)
		{
			UnboundCalendarEvent calendarEvent = base.IntervalToEvent(interval);

			calendarEvent.Color = Color.LightBlue;

			if (m_States.ContainsKey(interval.Name))
			{
				WorkflowState state = m_States[interval.Name];
				calendarEvent.Subject = state.WorkflowStateName;
				calendarEvent.Icon = state.IconId;
			}
			else
				calendarEvent.Subject = interval.Name;

			return calendarEvent;
		}

		/// <summary>
		/// Get the Journal entries as calendar events
		/// </summary>
		/// <returns></returns>
		public override IList<UnboundCalendarEvent> GetJournalAsCalendarEvents()
		{
			if (!m_CachesLoaded)
			{
				LoadStates();
				m_CachesLoaded = true;
			}

			return base.GetJournalAsCalendarEvents();
		}

		#endregion

		#region Private Members

		/// <summary>
		/// Load workflow states defined for the type of entity
		/// </summary>
		private void LoadStates()
		{
			IQuery query = Entity.EntityManager.CreateQuery(TableNames.WorkflowState);
			query.AddEquals(WorkflowStatePropertyNames.TableName, Entity.EntityType);
			IEntityCollection states = Entity.EntityManager.Select(TableNames.WorkflowState, query);

			foreach (WorkflowState state in states)
				m_States.Add(state.Identity, state);
		}

		#endregion
	}
}