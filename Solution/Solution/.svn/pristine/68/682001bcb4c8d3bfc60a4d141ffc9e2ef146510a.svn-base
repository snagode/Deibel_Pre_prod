using System;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.ClientControls.Validation;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks.BusinessObjects;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Schedule Group Task
	/// </summary>
	[SampleManagerTask("ScheduleGroupTask", "GENERAL", "SCHEDULE_GROUP")]
	public class ScheduleGroupTask : GenericLabtableTask
	{
		#region Member Variables

		private string m_DefaultTitle;
		private FormScheduleGroup m_Form;
		private ScheduleGroup m_ScheduleGroup;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormScheduleGroup) MainForm;
			m_ScheduleGroup = (ScheduleGroup) m_Form.Entity;

			m_Form.EndDateValidator.Validate += new ServerValidatorEventHandler(EndDateValidatorValidate);
			m_ScheduleGroup.PropertyChanged += new PropertyEventHandler(ScheduleGroupPropertyChanged);

			m_Form.Loaded += new EventHandler(FormLoaded);
			m_Form.PredictButton.Click += new EventHandler(PredictButtonClick);
			m_Form.ScheduleGrid.BeforeRowAdd += new EventHandler<BeforeRowAddedEventArgs>(ScheduleGridBeforeRowAdd);

			m_DefaultTitle = m_Form.Title;
		}

		/// <summary>
		/// Schedule Add
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.BeforeRowAddedEventArgs"/> instance containing the event data.</param>
		private void ScheduleGridBeforeRowAdd(object sender, BeforeRowAddedEventArgs e)
		{
			IQuery query = EntityManager.CreateQuery(ScheduleBase.EntityName);
			query.AddEquals(SchedulePropertyNames.ScheduleGroup, string.Empty);

			IEntity chooseEntity;
			FormResult result = Library.Utils.PromptForEntity(m_Form.StringTable.PromptTitleSchedule,
			                                                  m_Form.StringTable.PromptMessageSchedule, query,
			                                                  out chooseEntity);

			if (result == FormResult.OK)
				m_ScheduleGroup.AssignedSchedules.Add(chooseEntity);

			e.Cancel = true;
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Form Loaded Event Handler
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void FormLoaded(object sender, EventArgs e)
		{
			SetStatus();

			m_Form.PredictStart.Date = DateTime.Today.Subtract(TimeSpan.FromDays(1));
			m_Form.PredictStart.ReadOnly = false;
			m_Form.PredictEnd.Date = DateTime.Today.AddDays(6);
			m_Form.PredictEnd.ReadOnly = false;

			m_Form.ScheduleGrid.BeforeRowDelete += new EventHandler<BeforeRowDeleteEventArgs>(ScheduleGridBeforeRowDelete);
		}

		/// <summary>
		/// Handles the BeforeRowDelete event of the ScheduleGrid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.BeforeRowDeleteEventArgs"/> instance containing the event data.</param>
		private void ScheduleGridBeforeRowDelete(object sender, BeforeRowDeleteEventArgs e)
		{
			IGrid grid = (IGrid) sender;
			Schedule schedule = (Schedule) e.Entity;

			grid.GridData.Release(schedule);
			schedule.ScheduleGroup = null;
			EntityManager.Transaction.Add(schedule);

			e.Cancel = true;
		}

		/// <summary>
		/// Handles the PropertyChanged event of the m_ScheduleGroup control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.PropertyEventArgs"/> instance containing the event data.</param>
		private void ScheduleGroupPropertyChanged(object sender, PropertyEventArgs e)
		{
			if (e.PropertyName == ScheduleGroup.PropertyStatus)
				SetStatus();
		}

		/// <summary>
		/// Handles the Validate event of the endDateValidator control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.Validation.ServerValidatorEventArgs"/> instance containing the event data.</param>
		private void EndDateValidatorValidate(object sender, ServerValidatorEventArgs e)
		{
			if (!m_ScheduleGroup.EndDate.IsNull)
			{
				if (m_ScheduleGroup.EndDate.Value <= m_ScheduleGroup.StartDate.Value)
				{
					e.Valid = false;
					e.ErrorMessage = m_Form.StringTable.EndDateBeforeStart;
				}
			}
		}

		#endregion

		#region Status

		/// <summary>
		/// Enables the start end date prompts and sets the form title based on the schedule status.
		/// </summary>
		private void SetStatus()
		{
			IconName icon = new IconName(m_ScheduleGroup.Status.Icon.Identity);
			m_Form.StatusImage.SetImageByIconName(icon);

			// Set Form title

			m_Form.Title = string.Format("{0} - {1}", m_DefaultTitle, m_ScheduleGroup.Status.PhraseText);
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
			try
			{
				DateTime start = m_Form.PredictStart.Date.Value;
				DateTime end = m_Form.PredictEnd.Date.Value;

				Scheduler scheduler = new Scheduler();

				scheduler.UsePreSchedule = true;
				scheduler.UseActive = true;
				scheduler.UseLastLogin = false;
				scheduler.UseRunWindow = false;

				ScheduleToCalendar.Generate(scheduler, m_ScheduleGroup.AssignedSchedules, start, end, m_Form.PredictionCalendar);
			}
			catch (Exception e)
			{
				Library.Utils.FlashMessage(e.Message, m_Form.StringTable.PredictionError);
			}
		}

		#endregion
	}
}