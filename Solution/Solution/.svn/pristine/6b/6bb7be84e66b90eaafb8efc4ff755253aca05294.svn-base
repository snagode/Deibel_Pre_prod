using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.ClientControls.Browse;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of the Test Schedule LTE
	/// </summary>
	[SampleManagerTask("TestScheduleTask", "LABTABLE", TestSchedHeaderBase.EntityName)]
	public class TestScheduleTask : GenericLabtableTask
	{
		#region Member Variables

		private FormTestSchedule m_Form;
		private FormTestScheduleEntry m_GridRowForm;
		private TestSchedHeader m_TestSchedule;
		private DataGridColumn m_ComponentListColumn;
		private DataGridColumn m_InstrumentTypeColumn;
		private DataGridColumn m_InstrumentColumn;
		private List<string> m_CompListBrowseColumns;
		private EntityBrowse m_AllInstrumentsBrowse;

		#endregion

		#region Task Loaded

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			base.MainFormLoaded();

			m_Form = (FormTestSchedule) MainForm;
			m_TestSchedule = (TestSchedHeader) m_Form.Entity;

			// Setup Component List Browse Columns
			m_CompListBrowseColumns = new List<string>();
			m_CompListBrowseColumns.Add(VersionedCLHeaderPropertyNames.CompList);
			m_CompListBrowseColumns.Add(VersionedCLHeaderPropertyNames.Description);

			// Assign Control Events
			m_Form.GridTestSchedEntries.BeforeDisplayNewRowForm += OnBeforeAddNewEntry;
			m_TestSchedule.TestSchedEntries.ItemAdded += TestSchedEntriesItemAdded;
			m_TestSchedule.TestSchedEntries.ItemPropertyChanged += TestSchedEntriesItemPropertyChanged;

			// Get Column References
			m_ComponentListColumn = m_Form.GridTestSchedEntries.GetColumnByProperty(TestSchedEntryPropertyNames.ComponentList);
			m_InstrumentTypeColumn = m_Form.GridTestSchedEntries.GetColumnByProperty(TestSchedEntryPropertyNames.InstrumentType);
			m_InstrumentColumn = m_Form.GridTestSchedEntries.GetColumnByProperty(TestSchedEntryPropertyNames.InstrumentId);

			// Setup Grid Appearance / Behaviour
			SetupEntriesGrid();
		}

		#endregion

		#region Entries

		/// <summary>
		/// Setups the entries grid.
		/// </summary>
		private void SetupEntriesGrid()
		{
			// Setup Browse
			m_AllInstrumentsBrowse = BrowseFactory.CreateEntityBrowse(InstrumentBase.EntityName);

			foreach (TestSchedEntry entry in m_Form.GridTestSchedEntries.GridData)
			{
				RefreshComponentListCell(entry);
				RefreshInstrumentTypeCell(entry);
				RefreshInstrumentCell(entry);
			}
		}

		/// <summary>
		/// Handles the ItemAdded event of the TestSchedEntries control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void TestSchedEntriesItemAdded(object sender, EntityCollectionEventArgs e)
		{
			RefreshComponentListCell((TestSchedEntry) e.Entity);
			RefreshInstrumentTypeCell((TestSchedEntry) e.Entity);
			RefreshInstrumentCell((TestSchedEntry) e.Entity);
		}

		/// <summary>
		/// Handles the ItemPropertyChanged event of the TestSchedEntries control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void TestSchedEntriesItemPropertyChanged(object sender, EntityCollectionEventArgs e)
		{
			if (e.PropertyName == TestSchedEntryPropertyNames.InstrumentType)
			{
				RefreshInstrumentCell((TestSchedEntry) e.Entity);
			}
		}

		/// <summary>
		/// Refreshes the component list browse.
		/// </summary>
		/// <param name="testSchedEntry">The entry.</param>
		private void RefreshComponentListCell(TestSchedEntry testSchedEntry)
		{
			if (!testSchedEntry.IsAnalysis || !BaseEntity.IsValid(testSchedEntry.Analysis))
			{
				// Disable the Component List cell
				m_ComponentListColumn.DisableCell(testSchedEntry, DisabledCellDisplayMode.GreyHideContents);
				return;
			}

			// Component List Cell is Active
			m_ComponentListColumn.EnableCell(testSchedEntry);

			// Assign cell browse
			StringBrowse componentListBrowse = BrowseFactory.CreateStringBrowse(testSchedEntry.Analysis.CLHeaders,
			                                                                    VersionedCLHeaderPropertyNames.CompList);
			m_ComponentListColumn.SetCellBrowse(testSchedEntry, componentListBrowse);
		}

		/// <summary>
		/// Refreshes the instrument type cell.
		/// </summary>
		/// <param name="testSchedEntry">The test sched entry.</param>
		private void RefreshInstrumentTypeCell(TestSchedEntry testSchedEntry)
		{
			if (!testSchedEntry.IsAnalysis || !BaseEntity.IsValid(testSchedEntry.Analysis))
			{
				// Disable the Instrument Type cell
				m_InstrumentTypeColumn.DisableCell(testSchedEntry, DisabledCellDisplayMode.GreyHideContents);
				return;
			}

			// Instrument Type Cell is Active
			m_InstrumentTypeColumn.EnableCell(testSchedEntry);
		}

		/// <summary>
		/// Refreshes the instrument cell.
		/// </summary>
		/// <param name="testSchedEntry">The test sched entry.</param>
		private void RefreshInstrumentCell(TestSchedEntry testSchedEntry)
		{
			if (!testSchedEntry.IsAnalysis || !BaseEntity.IsValid(testSchedEntry.Analysis))
			{
				// Disable the Instrument cell
				m_InstrumentColumn.DisableCell(testSchedEntry, DisabledCellDisplayMode.GreyHideContents);
				return;
			}

			// Insteument Cell is Active
			m_InstrumentColumn.EnableCell(testSchedEntry);

			if (BaseEntity.IsValid(testSchedEntry.InstrumentType))
			{
				// Select data for cell browse based on the instrument type
				IQuery query = EntityManager.CreateQuery(InstrumentBase.EntityName);
				query.AddEquals(InstrumentPropertyNames.InstrumentTemplate, testSchedEntry.InstrumentType);

				// Assign cell browse
				EntityBrowse instrumentBrowse = BrowseFactory.CreateEntityBrowse(query);
				m_InstrumentColumn.SetCellBrowse(testSchedEntry, instrumentBrowse);
			}
			else
			{
				// No Instrument Type Assigned, just populate the cell browse with all instruments
				m_InstrumentColumn.SetCellBrowse(testSchedEntry, m_AllInstrumentsBrowse);
			}
		}

		/// <summary>
		/// Called when [before add new entry].
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.EntityCancelEventArgs"/> instance containing the event data.</param>
		private void OnBeforeAddNewEntry(object sender, EntityCancelEventArgs e)
		{
			if (m_GridRowForm == null)
			{
				m_GridRowForm = (FormTestScheduleEntry) m_Form.GridTestSchedEntries.RowForm;
				m_GridRowForm.Loaded += GridRowFormLoaded;

				m_GridRowForm.TestScheduleAddAnalValidator.Validate += TestScheduleAddAnalValidator;
				m_GridRowForm.TestScheduleAddSchedValidator.Validate += TestScheduleAddSchedValidator;
			}

			TestSchedEntry entry = (TestSchedEntry) e.Entity;
			if (entry != null)
			{
				m_GridRowForm.PromptAnalysis.Visible = entry.IsAnalysis;
				m_GridRowForm.PromptSchedule.Visible = !entry.IsAnalysis;
			}

			e.Entity.PropertyChanged -= EntryPropertyChanged;
			e.Entity.PropertyChanged += EntryPropertyChanged;
		}

		/// <summary>
		/// Validate the test schedule field.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Library.ClientControls.Validation.ServerValidatorEventArgs"/> instance containing the event data.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		private void TestScheduleAddSchedValidator(object sender, Library.ClientControls.Validation.ServerValidatorEventArgs e)
		{
			if (!m_GridRowForm.PromptSchedule.Visible) return;

			var sched = m_GridRowForm.Entity.Get(TestSchedEntryPropertyNames.TestSchedule);

			if (sched == null)
			{
				e.Valid = false;
			}
			else if ((sched is string) && (string.IsNullOrWhiteSpace((string) sched)))
			{
				e.Valid = false;
			}
			else
			{
				e.Valid = true;
			}
		}

		/// <summary>
		/// Validate the analysis field.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Library.ClientControls.Validation.ServerValidatorEventArgs"/> instance containing the event data.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		private void TestScheduleAddAnalValidator(object sender, Library.ClientControls.Validation.ServerValidatorEventArgs e)
		{
			if (!m_GridRowForm.PromptAnalysis.Visible) return;

			var anal = m_GridRowForm.Entity.Get(TestSchedEntryPropertyNames.AnalysisId);

			if (anal == null)
			{
				e.Valid = false;
			}
			else if ((anal is string) && (string.IsNullOrWhiteSpace((string) anal)))
			{
				e.Valid = false;
			}
			else
			{
				e.Valid = true;
			}
		}

		/// <summary>
		/// Handles the Loaded event of the m_GridRowForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void GridRowFormLoaded(object sender, System.EventArgs e)
		{
			if (m_TestSchedule.IsNew())
			{
				// Include all Test Schedules
				m_GridRowForm.PromptSchedule.Browse = BrowseFactory.CreateEntityBrowse(TestSchedHeaderBase.EntityName);
			}
			else
			{
				// Include all Test Schedules apart from this one in the schedule browse
				IQuery query = EntityManager.CreateQuery(TestSchedHeaderBase.EntityName);
				query.AddNotEquals(TestSchedHeaderPropertyNames.Identity, m_TestSchedule.Identity);

				m_GridRowForm.PromptSchedule.Browse = BrowseFactory.CreateEntityBrowse(query);
			}
		}

		/// <summary>
		/// Handles the PropertyChanged event of the entry control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.PropertyEventArgs"/> instance containing the event data.</param>
		private void EntryPropertyChanged(object sender, PropertyEventArgs e)
		{
			if (e.PropertyName == TestSchedEntryPropertyNames.IsAnalysis)
			{
				// Show / Hide the Analysis / Schedule prompt
				TestSchedEntry entry = (TestSchedEntry)sender;
				m_GridRowForm.PromptAnalysis.Visible = entry.IsAnalysis;
				m_GridRowForm.PromptSchedule.Visible = !entry.IsAnalysis;
			}
		}

		#endregion

	}
}