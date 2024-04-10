using System;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Utilities;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.ClientControls.Validation;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;
using Thermo.SampleManager.Tasks.BusinessObjects;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Schedule Task
	/// </summary>
	[SampleManagerTask("ScheduleTask", "GENERAL", "SCHEDULE")]
	public class ScheduleTask : GenericLabtableTask
	{
		#region Member Variables

		private IDetailGrid m_AnalysisGrid;
		private string m_DefaultTitle;
		private FormSchedule m_Form;
		private Schedule m_Schedule;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormSchedule) MainForm;
			m_Schedule = (Schedule) m_Form.Entity;

			m_Schedule.PropertyChanged += SampleSchedulePropertyChanged;
			m_Form.EndDateValidator.Validate += EndDateValidatorValidate;
			m_Form.Loaded += FormLoaded;
			m_Form.PredictButton.Click += PredictButtonClick;

			m_AnalysisGrid = m_Form.SchedulePointGrid.FindDetailGrid("SchedulePointAnalysisGrid");
			m_AnalysisGrid.FocusedRowChanged += AnalysisGridFocusedRowChanged;

			m_Form.SchedulePointGrid.FocusedViewChanged += SchedulePointGridOnFocusedViewChanged;
			m_Form.SchedulePointGrid.FocusedRowChanged += SchedulePointGridOnFocusedRowChanged;

			m_DefaultTitle = m_Form.Title;
		}

		/// <summary>
		/// Form Loaded Event Handler
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void FormLoaded(object sender, EventArgs e)
		{
			SetStatus();

			// Set some appropriate start end dates.

			m_Form.PredictStart.Date = DateTime.Today.Subtract(TimeSpan.FromDays(1));
			m_Form.PredictStart.ReadOnly = false;

			m_Form.PredictEnd.Date = DateTime.Today.AddDays(6);
			m_Form.PredictEnd.ReadOnly = false;

			// Add Columns to the Grids

			AddSchedulePointColumns();
			AddSchedulePointAnalysisColumns();
		}

		#endregion

		#region Browses

		/// <summary>
		/// Schedules the point grid on focused view changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="FocusedViewChangedEventArgs"/> instance containing the event data.</param>
		private void SchedulePointGridOnFocusedViewChanged(object sender, FocusedViewChangedEventArgs e)
		{
			SchedulePointAnalysis spa = e.Entity as SchedulePointAnalysis;

			if (spa != null)
			{
				RepublishComponentListBrowse(spa.Analysis);
			}
		}

		/// <summary>
		/// Schedules the point grid on focused row changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="FocusedRowChangedEventArgs"/> instance containing the event data.</param>
		private void SchedulePointGridOnFocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
		{
			SchedulePointAnalysis spa = e.Entity as SchedulePointAnalysis;

			if (spa != null)
			{
				RepublishComponentListBrowse(spa.Analysis);
			}
		}

		/// <summary>
		/// Analyses the grid focused row changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.FocusedRowChangedEventArgs"/> instance containing the event data.</param>
		private void AnalysisGridFocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
		{
			SchedulePointAnalysis spa = e.Entity as SchedulePointAnalysis;

			if (spa != null)
			{
				spa.PropertyChanged -= AnalysisPropertyChanged;
				spa.PropertyChanged += AnalysisPropertyChanged;
				RepublishComponentListBrowse(spa.Analysis);
			}
		}

		/// <summary>
		/// Analyses the property changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.PropertyEventArgs"/> instance containing the event data.</param>
		private void AnalysisPropertyChanged(object sender, PropertyEventArgs e)
		{
			if (e.PropertyName == SchedulePointAnalysisPropertyNames.Analysis)
			{
				RepublishComponentListBrowse(((SchedulePointAnalysis) sender).Analysis);
			}
		}

		/// <summary>
		/// Republishes the component list browse.
		/// </summary>
		/// <param name="analysis">The analysis.</param>
		private void RepublishComponentListBrowse(string analysis)
		{
			IQuery lists = EntityManager.CreateQuery(ComplistHeaderViewBase.EntityName);
			lists.AddEquals(ComplistHeaderViewPropertyNames.Analysis, analysis);
			m_Form.ComponentListBrowse.Republish(lists);
		}

		#endregion

		#region Column Addition

		/// <summary>
		/// Adds the schedule point columns.
		/// </summary>
		private void AddSchedulePointColumns()
		{
			ISchemaTable schedulePoint = Library.Schema.Tables[SchedulePointBase.StructureTableName];
			ISchemaTable sample = Library.Schema.Tables[SampleBase.StructureTableName];

			AddMatchingColumns(m_Form.SchedulePointGrid, schedulePoint, sample);
		}

		/// <summary>
		/// Adds the schedule point columns.
		/// </summary>
		private void AddSchedulePointAnalysisColumns()
		{
			ISchemaTable schedulePointAnalysis = Library.Schema.Tables[SchedulePointAnalysisBase.StructureTableName];
			ISchemaTable test = Library.Schema.Tables[TestBase.StructureTableName];

			IDetailGrid grid = m_Form.SchedulePointGrid.FindDetailGrid("SchedulePointAnalysisGrid");
			AddMatchingColumns(grid, schedulePointAnalysis, test);
		}

		/// <summary>
		/// Adds the matching columns.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="master">The master.</param>
		/// <param name="slave">The slave.</param>
		private static void AddMatchingColumns(IGridView grid, ISchemaTable master, ISchemaTable slave)
		{
			foreach (ISchemaField field in master.Fields)
			{
				if (!slave.Fields.Contains(field.Name)) continue;
				if (field.Name.Contains("_VERSION")) continue;

				string propertyName = EntityType.DeducePropertyName(field.Table.Name, field.Name);
				string displayName = TextUtils.GetDisplayText(propertyName);
				string controlName = string.Concat(grid.Name, "Column", propertyName);

				GridColumn column = new GridColumn(controlName, propertyName, displayName);
				InsertPropertyColumn(grid, column, grid.FixedColumns);
			}
		}

		/// <summary>
		/// Adds the property column.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="newColumn">The new column.</param>
		/// <param name="pos">The pos.</param>
		private static void InsertPropertyColumn(IGridView grid, GridColumn newColumn, int pos)
		{
			foreach (GridColumn column in grid.Columns)
			{
				if (newColumn.Property == column.Property) return;
				if (newColumn.Name == column.Name) return;
			}

			grid.Columns.Insert(pos, newColumn);
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Handles the PropertyChanged event of the m_SamplePointSchedule control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.PropertyEventArgs"/> instance containing the event data.</param>
		private void SampleSchedulePropertyChanged(object sender, PropertyEventArgs e)
		{
			if (e.PropertyName == Schedule.PropertyStatus)
				SetStatus();
		}

		/// <summary>
		/// Handles the Validate event of the endDateValidator control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.Validation.ServerValidatorEventArgs"/> instance containing the event data.</param>
		private void EndDateValidatorValidate(object sender, ServerValidatorEventArgs e)
		{
			if (!m_Schedule.EndDate.IsNull)
			{
				if (m_Schedule.EndDate.Value <= m_Schedule.StartDate.Value)
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
			IconName icon = new IconName(m_Schedule.Status.Icon.Identity);
			m_Form.StatusImage.SetImageByIconName(icon);

			m_Form.Title = string.Format("{0} - {1}", m_DefaultTitle, m_Schedule.Status.PhraseText);
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

				ScheduleToCalendar.Generate(scheduler, m_Schedule, start, end, m_Form.PredictionCalendar);
			}
			catch (Exception e)
			{
				Library.Utils.FlashMessage(e.Message, m_Form.StringTable.PredictionError);
			}
		}

		#endregion
	}
}