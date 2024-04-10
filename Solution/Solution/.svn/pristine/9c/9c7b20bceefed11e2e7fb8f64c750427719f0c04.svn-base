using System;
using System.Collections.Generic;
using System.Text;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks.BusinessObjects;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Scheduled Sample Login - but for a given date range
	/// </summary>
	[SampleManagerTask("ScheduleLoginTask", "GENERAL")]
	public class ScheduleLoginTask : SampleManagerTask
	{
		#region Member Variables

		/// <summary>
		/// Schedule Login Form
		/// </summary>
		protected FormScheduleLogin m_Form;

		private string m_MessageText;
		private IEntityCollection m_Samples;

		/// <summary>
		/// Schedules to Login
		/// </summary>
		protected IEntityCollection m_Schedules;

		#endregion

		#region Overrides

		/// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			m_Samples = EntityManager.CreateEntityCollection(SampleBase.EntityName);

			// Create the Form

			m_Form = (FormScheduleLogin) FormFactory.CreateForm(FormScheduleLogin.GetInterfaceName());

			// Select the Schedules

			GetSchedules();

			if (m_Schedules.Count == 0)
			{
				Exit();
				return;
			}

			// Link up Events

			m_Form.ScheduleBrowse.Republish(m_Schedules);
			m_Form.LoggedInSampleBrowse.Republish(m_Samples);

			m_Form.Created += new EventHandler(FormCreated);
			m_Form.Loaded += new EventHandler(FormLoaded);

			// Off we go...

			m_Form.Show();
		}

		#endregion

		#region Load Events

		/// <summary>
		/// Handles the Created event of the m_Form control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void FormCreated(object sender, EventArgs e)
		{
			m_Form.ScheduleBrowse.Republish(m_Schedules);
			m_Form.LoggedInSampleBrowse.Republish(m_Samples);
		}

		/// <summary>
		/// Form Loaded Event
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void FormLoaded(object sender, EventArgs e)
		{
			m_Form.StartDate.Date = DateTime.Today;
			m_Form.EndDate.Date = DateTime.Today.AddDays(1).Subtract(new TimeSpan(0, 0, 1));
			m_Form.PredictButton.ClickAndWait += new EventHandler(PredictButtonClick);
			m_Form.LoginButton.ClickAndWait += new EventHandler(LoginButtonClick);
		}

		#endregion

		#region Get Schedules

		/// <summary>
		/// Gets the schedules.
		/// </summary>
		protected virtual void GetSchedules()
		{
			m_Schedules = EntityManager.CreateEntityCollection(ScheduleBase.EntityName);

			if (Context.SelectedItems.Count == 0)
			{
				Schedule schedule = GetSchedule();

				if (schedule != null)
				{
					m_Schedules.Add(schedule);
				}
			}
			else
			{
				foreach (Schedule schedule in Context.SelectedItems)
				{
					m_Schedules.Add(schedule);
				}
			}
		}

		/// <summary>
		/// Gets the schedule.
		/// </summary>
		/// <returns></returns>
		private Schedule GetSchedule()
		{
			IEntity schedule;
			FormResult result = Library.Utils.PromptForEntity(m_Form.StringTable.PromptTitleSchedule,
			                                                  m_Form.StringTable.PromptMessageSchedule,
			                                                  ScheduleBase.EntityName, out schedule);
			if (result == FormResult.OK)
			{
				return (Schedule) schedule;
			}

			return null;
		}

		#endregion

		#region Prediction

		/// <summary>
		/// Prediction Button Click Event
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void PredictButtonClick(object sender, EventArgs args)
		{
			IList<SchedulePointEvent> events = ScheduleEvents();
			ScheduleToCalendar.Populate(events, m_Form.PredictionCalendar);
		}

		#endregion

		#region Login

		/// <summary>
		/// Logins the button click.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void LoginButtonClick(object sender, EventArgs e)
		{
			IList<SchedulePointEvent> events = ScheduleEvents();
			m_Samples = EntityManager.CreateEntityCollection(SampleBase.EntityName);

			if (events != null)
			{
				ScheduleLoginManager loginManager = new ScheduleLoginManager(Library);
				loginManager.LogMessageLevel = GetLoggerLevel();

				IList<SampleBase> samples = loginManager.LoginAndSelectSamples(events);

				foreach (SampleBase sample in samples)
				{
					m_Samples.Add(sample);
				}

				ShowMessages(loginManager.LogMessages);
			}

			m_Form.LoggedInSampleBrowse.Republish(m_Samples);
			ScheduleToCalendar.Populate(events, m_Form.PredictionCalendar);
		}

		#endregion

		#region Scheduling

		/// <summary>
		/// Schedules this instance.
		/// </summary>
		/// <returns></returns>
		private IList<SchedulePointEvent> ScheduleEvents()
		{
			ClearMessages();

			Scheduler scheduler = new Scheduler();

			scheduler.LogMessageLevel = GetLoggerLevel();

			scheduler.UsePreSchedule = false;
			scheduler.UseRunWindow = false;

			scheduler.UseActive = m_Form.ActiveCheck.Checked;
			scheduler.UseLastLogin = m_Form.LastLoginCheck.Checked;

			try
			{
				DateTime start = m_Form.StartDate.Date.Value;
				DateTime end = m_Form.EndDate.Date.Value;

				IList<SchedulePointEvent> events = scheduler.ProcessSchedules(m_Schedules, start, end);
				ShowMessages(scheduler.LogMessages);
				return events;
			}
			catch (Exception e)
			{
				Library.Utils.FlashMessage(e.Message, m_Form.StringTable.SchedulingError);
			}

			return null;
		}

		#endregion

		#region Logging

		/// <summary>
		/// Gets the logger level.
		/// </summary>
		/// <returns></returns>
		private LoggerLevel GetLoggerLevel()
		{
			if (m_Form.DebugError.Checked) return LoggerLevel.Error;
			if (m_Form.DebugWarn.Checked) return LoggerLevel.Warn;
			if (m_Form.DebugInfo.Checked) return LoggerLevel.Info;
			return LoggerLevel.Debug;
		}

		/// <summary>
		/// Clears the messages.
		/// </summary>
		private void ClearMessages()
		{
			m_MessageText = string.Empty;
			m_Form.DebugComments.Text = m_MessageText;
		}

		/// <summary>
		/// Shows the messages.
		/// </summary>
		/// <param name="messages">The messages.</param>
		private void ShowMessages(IEnumerable<LoggerMessage> messages)
		{
			StringBuilder builder = new StringBuilder();

			foreach (LoggerMessage message in messages)
			{
				string line = string.Format("{0}\t{1}\t{2}", message.TimeStamp.ToString("HH:mm:ss"), message.Level, message.Message);
				builder.AppendLine(line);
			}

			m_MessageText = string.Concat(m_MessageText, builder.ToString());
			m_Form.DebugComments.Text = m_MessageText;
		}

		#endregion
	}
}