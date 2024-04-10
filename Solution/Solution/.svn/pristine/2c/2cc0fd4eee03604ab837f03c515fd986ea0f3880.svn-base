using System.Collections.Generic;
using Thermo.SampleManager.Common.CommandLine;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;
using Thermo.SampleManager.Tasks.BusinessObjects;

namespace Thermo.SampleManager.Tasks.BackgroundTasks
{
	/// <summary>
	/// Background task which processes all Schedules
	/// </summary>
	[SampleManagerTask("Scheduler")]
	public class SchedulerTask : SampleManagerTask, IBackgroundTask
	{
		#region Member Variables

		private bool m_UpdateTimerQueue;
		private string m_Criteria;
		private string m_ScheduleIdentity;

		#endregion

		#region Properties

		/// <summary>
		/// Modify the TimerQueue to run when next samples due
		/// </summary>
		/// <value><c>true</c> adjust timerqueue to run at <c>false</c>.</value>
		[CommandLineSwitch("updatetq", "Update the TimerQueue Entry with the Next Time", false)]
		public bool UpdateTimerQueue
		{
			get { return m_UpdateTimerQueue; }
			set { m_UpdateTimerQueue = value; }
		}

		/// <summary>
		/// Modify the TimerQueue to run when next samples due
		/// </summary>
		/// <value><c>true</c> adjust timerqueue to run at <c>false</c>.</value>
		[CommandLineSwitch("criteria", "Optional criteria for schedule", false)]
		public string ScheduleCriteria
		{
			get { return m_Criteria; }
			set { m_Criteria = value; }
		}

		/// <summary>
		/// Modify the TimerQueue to run when next samples due
		/// </summary>
		/// <value><c>true</c> adjust timerqueue to run at <c>false</c>.</value>
		[CommandLineSwitch("schedule", "Optional identity for schedule", false)]
		public string ScheduleIdentity
		{
			get { return m_ScheduleIdentity; }
			set { m_ScheduleIdentity = value; }
		}

		#endregion

		#region IBackgroundTask Members

		/// <summary>
		/// Launches this background task.
		/// </summary>
		public void Launch()
		{
			Logger.Debug("Starting Scheduler Task...");

			// Select all Schedules

			IEntityCollection schedules;
			if (string.IsNullOrEmpty(m_Criteria) || string.IsNullOrWhiteSpace(m_Criteria))
			{
				if (string.IsNullOrEmpty(m_ScheduleIdentity) || string.IsNullOrWhiteSpace(m_ScheduleIdentity))
				{
					schedules = EntityManager.Select(TableNames.Schedule);
				}
				else
				{
					var query = EntityManager.CreateQuery(TableNames.Schedule);
					query.AddEquals("IDENTITY", m_ScheduleIdentity);

					schedules = EntityManager.Select(TableNames.Schedule, query);
				}

			}
			else
			{
				var query = EntityManager.CreateQuery(TableNames.CriteriaSaved);
				query.AddEquals("IDENTITY", m_Criteria);
				var criteriaCollection = EntityManager.Select(query);

				if (criteriaCollection != null && criteriaCollection.Count > 0)
				{
					var criteria = criteriaCollection[0] as CriteriaSavedBase;
					var criteriaTaskService = (ICriteriaTaskService)Library.GetService(typeof(ICriteriaTaskService));
					var dquery = criteriaTaskService.GetDefaultQueryByCriteria(criteria);
					schedules = EntityManager.Select(dquery);
				}
				else
				{
					throw new SampleManagerError(string.Format(Library.Message.GetMessage("ReportTemplateMessages", "ErrorFindingCriteriaId"), m_Criteria));
				}
			}

			// Create a Scheduler

			Logger.Debug("Work out when to schedule the Samples");
			Scheduler scheduler = new Scheduler();

			// Process Schedules

			IList<SchedulePointEvent> scheduledSamples = scheduler.ProcessSchedules(schedules);

			// Login Samples

			if (scheduledSamples.Count > 0)
			{
				Logger.DebugFormat("Need to login {0} Samples", scheduledSamples.Count);
				ScheduleLoginManager loginManager = new ScheduleLoginManager(Library);
				loginManager.LoginSamples(scheduledSamples);
			}
			else
				Logger.Debug("No Samples to Login");

			// Update the Timerqueue with the next event

			Logger.DebugFormat("Next predicted event after these is {0}", scheduler.Next);

			if (m_UpdateTimerQueue)
			{
				if (Schedule.UpdateTimerQueue(EntityManager, scheduler.Next))
					EntityManager.Commit();
			}
		}

		#endregion
	}
}