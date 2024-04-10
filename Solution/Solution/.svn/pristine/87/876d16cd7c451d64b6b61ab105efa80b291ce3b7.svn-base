using System;
using System.Globalization;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the SCHEDULE_GROUP entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class ScheduleGroup : ScheduleGroupBase
	{
		#region Constants

		/// <summary>
		/// Schedule Status Property Name
		/// </summary>
		public const string PropertyStatus = "Status";

		#endregion

		#region Member Variables

		private IEntityCollection m_AssignedSchedules;
		private PhraseBase m_Status;

		#endregion

		#region Property Changed Events

		/// <summary>
		/// Called when when a property changes.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged(PropertyEventArgs e)
		{
			if (e.PropertyName == ScheduleGroupPropertyNames.StartDate ||
			    e.PropertyName == ScheduleGroupPropertyNames.EndDate ||
			    e.PropertyName == ScheduleGroupPropertyNames.Active ||
			    e.PropertyName == ScheduleGroupPropertyNames.ForceScheduleControl)
			{
				RefreshStatus();
				NotifyPropertyChanged(PropertyStatus);
			}
		}

		/// <summary>
		/// Updated
		/// </summary>
		protected override void OnPreCommit()
		{
			Schedule.UpdateTimerQueue(EntityManager, Library.Environment.ClientNow.ToDateTime(CultureInfo.CurrentCulture));
		}

		#endregion

		#region Assigned Schedules

		/// <summary>
		/// Gets the schedules which are assigned to this group.
		/// </summary>
		/// <value>The assigned schedules.</value>
		[PromptCollection(TableNames.Schedule)]
		public IEntityCollection AssignedSchedules
		{
			get
			{
				if (m_AssignedSchedules == null)
				{
					if (IsNew())
						m_AssignedSchedules = EntityManager.CreateEntityCollection(TableNames.Schedule);
					else
					{
						IQuery query = EntityManager.CreateQuery(TableNames.Schedule);
						query.AddEquals(SchedulePropertyNames.ScheduleGroup, this);

						m_AssignedSchedules = EntityManager.Select(TableNames.Schedule, query);
					}

					m_AssignedSchedules.ItemAdded += new EntityCollectionEventHandler(AssignedSchedulesItemAdded);
				}

				return m_AssignedSchedules;
			}
		}

		/// <summary>
		/// Added Schedules
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void AssignedSchedulesItemAdded(object sender, EntityCollectionEventArgs e)
		{
			Schedule added = (Schedule) e.Entity;
			added.ScheduleGroup = this;
		}

		#endregion

		#region Status Indication

		/// <summary>
		/// Gets the current status of the schedule based on Start and End Dates.
		/// </summary>
		/// <value>The schedule status.</value>
		[PromptValidPhrase(PhraseSchedStat.Identity)]
		public PhraseBase Status
		{
			get
			{
				if (m_Status == null)
					RefreshStatus();

				return m_Status;
			}
		}

		/// <summary>
		/// Refreshes the status.
		/// </summary>
		private void RefreshStatus()
		{
			if (!ForceScheduleControl)
			{
				SetStatus(PhraseSchedStat.PhraseIdU);
				return;
			}

			if (!Active)
			{
				SetStatus(PhraseSchedStat.PhraseIdI);
				return;
			}

			if (Running)
			{
				SetStatus(PhraseSchedStat.PhraseIdR);
				return;
			}

			if (Ended)
			{
				SetStatus(PhraseSchedStat.PhraseIdC);
				return;
			}

			SetStatus(PhraseSchedStat.PhraseIdP);
		}

		/// <summary>
		/// Sets the status.
		/// </summary>
		/// <param name="phraseId">The phrase id.</param>
		private void SetStatus(string phraseId)
		{
			m_Status = (Phrase) EntityManager.SelectPhrase(PhraseSchedStat.Identity, phraseId);
		}

		#endregion

		#region Activity Periods

		/// <summary>
		/// Gets a value indicating whether this <see cref="Schedule"/> is started.
		/// </summary>
		/// <value><c>true</c> if started; otherwise, <c>false</c>.</value>
		public bool Started
		{
			get { return StartsBefore(DateTime.Now); }
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="Schedule"/> is ended.
		/// </summary>
		/// <value><c>true</c> if ended; otherwise, <c>false</c>.</value>
		public bool Ended
		{
			get { return !EndsAfter(DateTime.Now); }
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="Schedule"/> is running.
		/// </summary>
		/// <value><c>true</c> if running; otherwise, <c>false</c>.</value>
		public bool Running
		{
			get { return Started && !Ended; }
		}

		/// <summary>
		/// Does the schedule start on or before the passed in time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
		public bool StartsBefore(DateTime time)
		{
			if (StartDate.IsNull) return false;
			return (StartDate.Value <= time);
		}

		/// <summary>
		/// Does the schedule end on or after the passed in time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
		public bool EndsAfter(DateTime time)
		{
			if (EndDate.IsNull) return true;
			return (EndDate.Value >= time);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Links to Type LocationBase
		/// </summary>
		/// <value></value>
		[PromptHierarchyLink(LocationBase.EntityName, true, ObjectModel.Location.HierarchyPropertyName)]
		public override LocationBase Location
		{
			get
			{
				return base.Location;
			}
			set
			{
				base.Location = value;
			}
		}

		#endregion
	}
}