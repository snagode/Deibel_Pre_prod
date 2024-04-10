using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Workflow;
using Thermo.SampleManager.Core.Definition;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.ClientControls.Browse;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;
using Thermo.SampleManager.Server.Workflow;
using Thermo.SampleManager.Server.Workflow.Nodes;
using TabPage = Thermo.SampleManager.Library.ClientControls.TabPage;
using ToolBarButton = Thermo.SampleManager.Library.ClientControls.ToolBarButton;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Base class for all sample administration tasks.
    /// Modifications:  
    ///     B.Smock.01 - Default assignments to avoid increment retrieval error
    ///                  Lock check before locking entities
    ///                  
    ///     B.Smock.02 - Add 'on hold' to valid for edit sample status list
	/// </summary>
	public abstract class SampleAdminBaseTask : DefaultFormTask
	{
		#region Constants

		private const string TabPageJobs = "TabPageJobData";
		private const string TabPageSamples = "TabPageSampleData";
		private const string TabPageTests = "TabPageTestData";
		private const string TabPageTestAssignment = "TabPageTestAssignment";

		private const string TaskReadOnlyMode = "DISPLAY";

		private const string TestAssignSampleColumnName = "SampleColumn";

		// Grid Layout Configuration
		private const string GridModeHorizontal = "Horizontal";
		private const string GridModeVertical = "Vertical";
		private const string SampleLoginSettingType = "SampleLogin";
		private const string SampleLoginSettingProperty = "Layout";

		// Format used to alter the appearance of an entity when it has no entity template (i.e. created in VGL)
		private const string BlankEntityTemplateIndicatorFormat = "[{0}]";


		private const string SampleIdColumn = "SampleId";
		private const string JobNameColumn = "JobName";
		private const string TestIdColumn = "TestId";
		private const string AssignColumn = "Assign";

		#endregion

		#region Member Variables


		/// <summary>
		/// The node to set focus on after tree is loaded
		/// </summary>
		private SimpleTreeListNodeProxy m_FocusOnLoadNode = null;

		private bool m_Loaded;

		/// <summary>
		/// Focused entity in the tree
		/// </summary>
		protected IEntity m_FocusedTreeEntity;

		/// <summary>
		/// Root node is selected
		/// </summary>
		protected bool m_RootNodeSelected;

		/// <summary>
		/// TreeList root node
		/// </summary>
		protected SimpleTreeListNodeProxy m_RootNode;

		/// <summary>
		/// Sample Admin Form
		/// </summary>
		protected FormSampleAdmin m_Form;

		/// <summary>
		/// m_TabPageJobs
		/// </summary>
		protected TabPage m_TabPageJobs;

		/// <summary>
		/// m_TabPageSamples
		/// </summary>
		protected TabPage m_TabPageSamples;

		/// <summary>
		/// m_TabPageTests
		/// </summary>
		protected TabPage m_TabPageTests;

		private ToolBarButton m_RemoveToolBarButton;
		private ToolBarButton m_AddJobToolBarButton;
		private ToolBarButton m_AddSampleToolBarButton;
		private ToolBarButton m_AddSubSampleToolBarButton;
		private ToolBarButton m_AddAnalysisToolBarButton;
		private ToolBarButton m_AddTestScheduleToolBarButton;

		private ContextMenuItem m_JobContextMenuItem;
		private ContextMenuItem m_SampleContextMenuItem;
		private ContextMenuItem m_SubSampleContextMenuItem;
		private ContextMenuItem m_AddTestScheduleContextMenuItem;
		private ContextMenuItem m_AddTestContextMenuItem;

		/// <summary>
		/// m_ToolBarButtonTransposer
		/// </summary>
		protected ToolBarButton m_ToolBarButtonTranspose;

		private ToolBarButton m_ToolBarButtonFillUp;
		private ToolBarButton m_ToolBarButtonFillLeft;
		private ToolBarButton m_ToolBarButtonFillDown;
		private ToolBarButton m_ToolBarButtonFillRight;
		private ToolBarButton m_ToolBarButtonFillAll;

		private IEntityCollection m_TopLevelEntities;
		private UnboundGridColumn m_AssignColumn;

		private string m_AddWorkflowType;

		private bool m_ApplyClicked;
		private bool m_OkClicked;

		/// <summary>
		/// m_VisibleGrid
		/// </summary>
		protected UnboundGrid m_VisibleGrid;

		private SimpleTreeListNodeProxy m_ContextMenuNode;
		private Dictionary<string, UnboundGrid> m_TabGridLookup;

		private IEntityCollection m_NewJobs;
		private IEntityCollection m_NewSamples;
		private IEntityCollection m_NewTests;

		/// <summary>
		/// m_TopLevelTableName
		/// </summary>
		protected string m_TopLevelTableName;

		/// <summary>
		/// Title
		/// </summary>
		protected string m_Title;

		/// <summary>
		/// Flag to indicate is we are within a sub sampling operation.
		/// </summary>
		protected bool m_SubSampling;

		private bool m_TestAssignmentInitialised;
		private bool m_Saving;
		private Dictionary<UnboundGrid, bool> m_GridModeLookup;

		private IQuery m_CriteriaQuery;
		private bool m_InitialisingCriteria;

		private bool m_FatalErrorOccured;
		private bool m_ExitOnCancel;

		private Dictionary<EntityBrowse, IEntityCollection> m_CriteriaBrowseLookup;

		private IWorkflowEventService m_WorkflowEventService;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the default workflow.
		/// </summary>
		/// <value>
		/// The default workflow.
		/// </value>
		protected WorkflowBase DefaultWorkflow { get; set; }

		/// <summary>
		/// Gets a value indicating whether this is operating in One Shot mode
		/// </summary>
		/// <value>
		///   <c>true</c> if one shot mode; otherwise, <c>false</c>.
		/// </value>
		public virtual bool OneShotMode
		{
			get { return Context.UsingSurrogateEntityManager; }
		}

		#endregion

		#region Setup Task

		/// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			m_WorkflowEventService = Library.GetService<IWorkflowEventService>();

			// Make sure the task has sufficient data to operate

			bool isValid = ValidateContext();

			if (!isValid)
			{
				Exit();
				return;
			}

			m_CriteriaBrowseLookup = new Dictionary<EntityBrowse, IEntityCollection>();

			base.SetupTask();
		}

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			m_TopLevelTableName = GetTopLevelTableName();

			// Setup Initial Data - useful if modifying data

			bool initialised = InitialiseTopLevelEntities(out m_TopLevelEntities);

			if (!initialised)
			{
				Exit();
				return;
			}

			// Initialise Form

			m_Form = (FormSampleAdmin) MainForm;

			m_Title = GetTitle();

			m_Form.Title = m_Title;

			// Lock committed data

			if (Context.LaunchMode != GenericLabtableTask.DisplayOption)
			{
				string errorText;
				bool lockSucceeded = LockAllEntities(out errorText);
				if (!lockSucceeded)
				{
					// Cannot lock all data, tell the user and exit

					string message = Library.Message.GetMessage("GeneralMessages", "SampleLoginLockMessage");
					message = string.Format(message, errorText);
					Library.Utils.FlashMessage(message, m_Title, MessageButtons.OK, MessageIcon.Error, MessageDefaultButton.Button1);

					ReleaseLocks();
					Exit();
					return;
				}
			}

			// Store a lookup list of grids and tab pages for ease of use when changing tabs

			m_TabGridLookup = new Dictionary<string, UnboundGrid>();
			m_TabGridLookup.Add(TabPageJobs, m_Form.GridJobProperties);
			m_TabGridLookup.Add(TabPageSamples, m_Form.GridSampleProperties);
			m_TabGridLookup.Add(TabPageTests, m_Form.GridTestProperties);

			// Get TabPage references

			m_TabPageJobs = m_Form.MainTabControl.FindTabPage(TabPageJobs);
			m_TabPageSamples = m_Form.MainTabControl.FindTabPage(TabPageSamples);
			m_TabPageTests = m_Form.MainTabControl.FindTabPage(TabPageTests);

			// Assign control events

			m_Form.MainTabControl.SelectedPageChanged += TabControl1_SelectedPageChanged;

			m_Form.TreeListItems.Loaded += TreeListItems_Loaded;
			m_Form.TreeListItems.FocusedNodeChanged += TreeListItems_FocusedNodeChanged;
			m_Form.TreeListItems.NodeAdded += TreeListItems_NodeAdded;

			// Add a root node

			const string rootIconName = "INT_SAMPLES_MULTI";
			string rootText = IsJobWorkflow ? m_Form.StringTable.RootTextJobs : m_Form.StringTable.RootTextSamples;
			m_RootNode = m_Form.TreeListItems.AddNode(null, rootText, new IconName(rootIconName));
			m_Form.TreeListItems.Caption = m_Title;

			// Add initial data to the tree

			InitialiseTree();

			// Setup Toolbar buttons

			m_RemoveToolBarButton = m_Form.ToolBarTree.FindButton("ButtonRemove");
			m_AddJobToolBarButton = m_Form.ToolBarTree.FindButton("ButtonAddJobWorkflow");
			m_AddSampleToolBarButton = m_Form.ToolBarTree.FindButton("ButtonAddSampleWorkflow");
			m_AddAnalysisToolBarButton = m_Form.ToolBarTree.FindButton("ButtonAddTest");
			m_AddTestScheduleToolBarButton = m_Form.ToolBarTree.FindButton("ButtonAddTestSchedule");
			m_AddSubSampleToolBarButton = m_Form.ToolBarTree.FindButton("ButtonAddSubSample");

			// Enable/Disable Workflow Options

			m_AddJobToolBarButton.Visible = IsJobWorkflow;

			m_AddJobToolBarButton.Enabled = !IsTaskReadReadonly;
			m_AddSampleToolBarButton.Visible = !OneShotMode;

			m_AddSampleToolBarButton.Enabled = !IsTaskReadReadonly;
			m_AddSampleToolBarButton.Visible = (!OneShotMode || IsJobWorkflow);

			m_AddSubSampleToolBarButton.Enabled = !IsTaskReadReadonly;
			m_RemoveToolBarButton.Enabled = !IsTaskReadReadonly;
			m_AddAnalysisToolBarButton.Enabled = !IsTaskReadReadonly;
			m_AddTestScheduleToolBarButton.Enabled = !IsTaskReadReadonly;

			// Show/Hide ActionButtons based on edit or display mode

			m_Form.ActionButtonOK.Visible = !IsTaskReadReadonly;
			m_Form.ActionButtonCancel.Visible = !IsTaskReadReadonly;
			m_Form.ActionButtonApply.Visible = !IsTaskReadReadonly;
			m_Form.ActionButtonClose.Visible = IsTaskReadReadonly;

			// Setup ToolBar

			m_ToolBarButtonTranspose = m_Form.ToolBarTree.FindButton("ButtonTranspose");
			m_ToolBarButtonFillUp = m_Form.ToolBarTree.FindButton("ButtonFillUp");
			m_ToolBarButtonFillLeft = m_Form.ToolBarTree.FindButton("ButtonFillLeft");
			m_ToolBarButtonFillDown = m_Form.ToolBarTree.FindButton("ButtonFillDown");
			m_ToolBarButtonFillRight = m_Form.ToolBarTree.FindButton("ButtonFillRight");
			m_ToolBarButtonFillAll = m_Form.ToolBarTree.FindButton("ButtonFillAll");
			m_ToolBarButtonTranspose.Click += ToolBarButtonTranspose_Click;
			m_ToolBarButtonFillUp.Click += ToolBarButtonFillUp_Click;
			m_ToolBarButtonFillLeft.Click += ToolBarButtonFillLeft_Click;
			m_ToolBarButtonFillDown.Click += ToolBarButtonFillDown_Click;
			m_ToolBarButtonFillRight.Click += ToolBarButtonFillRight_Click;
			m_ToolBarButtonFillAll.Click += ToolBarButtonFillAll_Click;

			// Assign ToolBar button click events

			m_RemoveToolBarButton.Click += DeleteButton_Click;
			m_AddSampleToolBarButton.Click += AddSampleToolBarButton_Click;
			m_AddSubSampleToolBarButton.Click += AddSubSampleToolBarButton_Click;
			m_AddAnalysisToolBarButton.Click += AddAnalysisToolBarButton_Click;
			m_AddTestScheduleToolBarButton.Click += AddTestScheduleToolBarButton_Click;

			m_TabPageJobs.Visible = IsJobWorkflow;

			if (IsJobWorkflow)
			{
				m_AddJobToolBarButton.Click += AddJobToolBarButton_Click;

				// Set Fixed Columns

				m_Form.GridSampleProperties.FixedColumns = m_Form.GridSampleProperties.FixedColumns + 1;
			}

			if (!IsTaskReadReadonly)
			{
				// Setup the Tree ContextMenu

				m_Form.TreeListItems.ContextMenu.BeforePopup += ContextMenu_BeforePopup;

				m_JobContextMenuItem = m_Form.TreeListItems.ContextMenu.AddItem(m_Form.StringTable.AddJobWorkflow, "INT_JOB_LOGIN");
				m_JobContextMenuItem.ItemClicked += JobContextMenuItem_ItemClicked;
				m_JobContextMenuItem.Enabled = m_AddJobToolBarButton.Visible;

				m_SampleContextMenuItem = m_Form.TreeListItems.ContextMenu.AddItem(m_Form.StringTable.AddSampleWorkflow, "INT_SAMPLE_NEW");
				m_SampleContextMenuItem.ItemClicked += SampleContextMenuItem_ItemClicked;
				m_SampleContextMenuItem.Enabled = m_AddSampleToolBarButton.Visible;

				m_SubSampleContextMenuItem = m_Form.TreeListItems.ContextMenu.AddItem(m_Form.StringTable.AddSubSample, "INT_SAMPLE_PLAN_LOGIN");
				m_SubSampleContextMenuItem.ItemClicked += SubSampleContextMenuItem_ItemClicked;

				m_AddTestContextMenuItem = m_Form.TreeListItems.ContextMenu.AddItem(m_Form.StringTable.AddTestSchedule, "INT_TEST_SCHEDULE");
				m_AddTestContextMenuItem.ItemClicked += AddTestScheduleContextMenuItem_ItemClicked;

				m_AddTestScheduleContextMenuItem = m_Form.TreeListItems.ContextMenu.AddItem(m_Form.StringTable.AddTest, "TEST_ADD");
				m_AddTestScheduleContextMenuItem.ItemClicked += AddTestContextMenuItem_ItemClicked;

				// FormResult is not returning Apply when the apply button has been clicked, hence the need to do this manually

				m_Form.ActionButtonApply.BeforeAction += ActionButtonApply_BeforeAction;
				m_Form.ActionButtonOK.BeforeAction += ActionButtonOK_BeforeAction;

				// Apply is not really that useful in one shot mode

				m_Form.RightButtonPanel.Visible = !OneShotMode;
			}

			m_Form.Closing += Form_Closing;

			if (Context.LaunchMode != "MODIFY" && Context.LaunchMode != "DISPLAY")
			{
				Library.Task.StateModified();
			}

			// Enable / Disable ToolBar Buttons

			UpdateWorkflowToolbarButtons();

			m_VisibleGrid = IsJobWorkflow ? m_Form.GridJobProperties : m_Form.GridSampleProperties;

			InitialiseGridOrientation();

			// If We have no Data - run workflow or prompt for some

			if (m_TopLevelEntities.ActiveCount == 0)
			{
				if (OneShotMode)
				{
					ThreadPool.QueueUserWorkItem(state =>
					{
						try
						{
							if (Library.Environment.GetGlobalInt("CLIENT_TYPE") == 1)
							{
								// In web client, a delay is needed to avoid execution of the workflow happening
								// before initialization of controls on client side.

								Thread.Sleep(500);
							}

							RunWorkflowOnce(DefaultWorkflow);
						}
						catch (Exception ex)
						{
							OnException(ex);
						}
					});
				}
				else
				{
					AddNewWorkflow(GetTopLevelTableName(), true);
				}
			}

			// Assign Grid Events

			m_Form.GridJobProperties.CellValueChanged += OnCellValueChanged;
			m_Form.GridSampleProperties.CellValueChanged += OnCellValueChanged;
			m_Form.GridTestProperties.CellValueChanged += OnCellValueChanged;
			m_Form.GridSampleProperties.ValidateCell += GridSampleProperties_ValidateCell;
			m_Form.GridJobProperties.ServerFillingValue += OnServerFillingValue;
			m_Form.GridSampleProperties.ServerFillingValue += OnServerFillingValue;
			m_Form.GridTestProperties.ServerFillingValue += OnServerFillingValue;
		}

		/// <summary>
		/// Enable ToolBar Fill Buttons
		/// </summary>
		/// <param name="enable"></param>
		private void EnableToolBarFillButtons(bool enable)
		{
			m_ToolBarButtonFillUp.Enabled = enable;
			m_ToolBarButtonFillDown.Enabled = enable;
			m_ToolBarButtonFillAll.Enabled = enable;
			m_ToolBarButtonFillLeft.Enabled = enable;
			m_ToolBarButtonFillRight.Enabled = enable;
		}

		/// <summary>
		/// Handles the Loaded event of the TreeListItems control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void TreeListItems_Loaded(object sender, EventArgs e)
		{
			m_Loaded = true;

			m_Form.ActionButtonOK.Enabled = true;
			m_Form.ActionButtonCancel.Enabled = true;

			if (m_FocusOnLoadNode != null)
			{
				m_Form.TreeListItems.FocusNode(m_FocusOnLoadNode);
				m_FocusOnLoadNode = null;
			}
		}

		/// <summary>
		/// Initialises the data.
		/// </summary>
		/// <param name="topLevelEntities">The top level entities.</param>
		/// <returns></returns>
		protected abstract bool InitialiseTopLevelEntities(out IEntityCollection topLevelEntities);

		/// <summary>
		/// Sets the title.
		/// </summary>
		protected abstract string GetTitle();

		/// <summary>
		/// Validates the context.
		/// </summary>
		/// <returns></returns>
		protected virtual bool ValidateContext()
		{
			if (!Library.Environment.GetGlobalBoolean("WORKFLOW_STATUS_CHANGE") && Context.LaunchMode != "DISPLAY")
			{
				Library.Utils.ShowAlert(Library.Message.GetMessage("WorkflowMessages", "WorkflowStatusChangeNo"));
				return false;
			}
			return true;
		}

		/// <summary>
		/// Gets the name of the top level table.
		/// </summary>
		/// <returns></returns>
		protected abstract string GetTopLevelTableName();

		#endregion

		#region UI Initialisation

		/// <summary>
		/// Enables the disable cell.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="entities">The entities.</param>
		protected void RefreshGrid(UnboundGrid grid, IEntityCollection entities)
		{
			try
			{
				// Suspend comms with client

				grid.BeginUpdate();

				// Store grid values in object model before clearing the grid
				// If apply has been click the object model code is has already be modifed
				// by Assign After Edit so don't update.

				if (!m_ApplyClicked)
				{
					StoreGridDataInEntities(grid);
				}

				// Clear the grid

				grid.ClearGrid();

				// Add default columns for this grid

				BuildDefaultColumns(grid);

				// Build up the grid columns

				BuildColumns(entities, grid);

				// Add data rows to the grid

				BuildRows(entities, grid);
			}
			finally
			{
				// Do a full client refresh

				grid.EndUpdate();
			}
		}

		/// <summary>
		/// Populates the grid.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="grid">The grid.</param>
		private void BuildRows(IEntityCollection entities, UnboundGrid grid)
		{
			m_CriteriaBrowseLookup.Clear();

			// Go through each entity to determine which columns are needed in the grid (based on entity template)

			foreach (IEntity entity in entities)
			{
				EntityTemplateInternal template = GetTemplate(entity);
				if (template == null) continue;

				// Add a row to the grid

				UnboundGridRow newRow = grid.AddRow();
				newRow.Tag = entity;

				if (entity is Test)
				{
					newRow[m_AssignColumn] = ((Test) entity).Assign;
				}

				// Set the row icon

				newRow.SetIcon(new IconName(entity.Icon));

				// Populate default column values

				PopulateDefaultColumns(grid, newRow, entity);

				// Set cell values and enable/disable redundant cells based on the entity template

				for (int i = grid.FixedColumns; i < grid.Columns.Count; i++)
				{
					UnboundGridColumn column = grid.Columns[i];

					// Try getting the template property

					EntityTemplatePropertyInternal templateProperty = template.GetProperty(column.Name);

					if (templateProperty == null || templateProperty.IsHidden)
					{
						// Disable this cell

						column.DisableCell(newRow, DisabledCellDisplayMode.GreyHideContents);
						continue;
					}

					// Set value

					newRow[column] = entity.Get(templateProperty.PropertyName);

					// This is an active cell

					if (templateProperty.IsMandatory)
					{
						// Make the cell appear yellow

						column.SetCellMandatory(newRow);
					}

					if (!string.IsNullOrEmpty(templateProperty.FilterBy))
					{
						// Setup this column for filtering

						// Mark the column that is used for filtering

						UnboundGridColumn filterBySourceColumn = grid.GetColumnByName(templateProperty.FilterBy);
						if (filterBySourceColumn != null)
						{
							filterBySourceColumn.Tag = true;
						}

						// Setup filter

						object filterValue = entity.Get(templateProperty.FilterBy);

						if (filterValue != null)
						{
							IEntity filterValueEntity = filterValue as IEntity;
							bool isValid = filterValueEntity == null || BaseEntity.IsValid(filterValueEntity);
							if (isValid)
							{
								SetupFilterBy(templateProperty, newRow, column, filterValue);
							}
						}
					}
					else if (!string.IsNullOrEmpty(templateProperty.Criteria))
					{
						// A criteria has been specified for this column, setup the browse

						ICriteriaTaskService criteriaTaskService = (ICriteriaTaskService) Library.GetService(typeof (ICriteriaTaskService));

						// Once the query is populated the Query Populated Event is raised. This is beacause the criteria
						// could prompt for VGL values or C# values.
						// Prompted Criteria is ignored

						string linkedType = EntityType.GetLinkedEntityType(template.TableName, templateProperty.PropertyName);
						CriteriaSaved criteria = (CriteriaSaved) EntityManager.Select(TableNames.CriteriaSaved, new Identity(linkedType, templateProperty.Criteria));

						if (BaseEntity.IsValid(criteria))
						{

							// Generate a query based on the criteria

							criteriaTaskService.QueryPopulated += CriteriaTaskService_QueryPopulated;
							m_CriteriaQuery = null;
							m_InitialisingCriteria = true;
							criteriaTaskService.GetPopulatedCriteriaQuery(criteria);
							m_InitialisingCriteria = false;

							if (m_CriteriaQuery != null)
							{
								// Assign the browse to the column

								IEntityCollection browseEntities = EntityManager.Select(m_CriteriaQuery.TableName, m_CriteriaQuery);
								EntityBrowse criteriaBrowse = BrowseFactory.CreateEntityBrowse(browseEntities);
								column.SetCellBrowse(newRow, criteriaBrowse);
								m_CriteriaBrowseLookup[criteriaBrowse] = browseEntities;

								// Make sure the cell's value is present within the browse

								IEntity defaultValueEntity = entity.GetEntity(templateProperty.PropertyName);
								if (BaseEntity.IsValid(defaultValueEntity) && !browseEntities.Contains(defaultValueEntity))
								{
									// The default value is not within the specified criteria, null out this cell

									newRow[templateProperty.PropertyName] = null;
								}
							}
						}
					}

					if (templateProperty.IsReadOnly || !ValidStatusForModify(entity))
					{
						// Disable the cell but display it's contents

						column.DisableCell(newRow, DisabledCellDisplayMode.ShowContents);
					}

					// Do specific column stuff 

					SetupGridColumn(entity, templateProperty, newRow, column);
				}

			}
		}

		/// <summary>
		/// Setups the filter by.
		/// </summary>
		/// <param name="templateProperty">The template property.</param>
		/// <param name="row">The new row.</param>
		/// <param name="column">The column.</param>
		/// <param name="filterValue">The filter value.</param>
		private void SetupFilterBy(EntityTemplatePropertyInternal templateProperty, UnboundGridRow row, UnboundGridColumn column, object filterValue)
		{
			// Setup entity browse filtering

			IQuery filteredQuery = templateProperty.CreateFilterByQuery(filterValue);

			// Setup the property browse for the collection column to browse collection properties for the table.

			IEntityBrowse browse = BrowseFactory.CreateEntityOrHierarchyBrowse(filteredQuery.TableName, filteredQuery);

			column.SetCellEntityBrowse(row, browse);
		}

		/// <summary>
		/// Gets the template.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		private static EntityTemplateInternal GetTemplate(IEntity entity)
		{
			var template = (EntityTemplateInternal) entity.GetEntity("EntityTemplate");
			return template;
		}

		/// <summary>
		/// Adds the default columns.
		/// </summary>
		/// <param name="grid">The grid.</param>
		private void BuildDefaultColumns(UnboundGrid grid)
		{
			if (grid == m_Form.GridJobProperties)
			{
				// Add Job Name column to the Job Grid

				grid.AddColumn(JobNameColumn, "Job", GridColumnType.Text, "Job", false, false, true, true, false, true, false, HorizontalAlignment.Left, 135);
				return;
			}

			if (grid == m_Form.GridSampleProperties)
			{
				if (IsJobWorkflow)
				{
					// Add Job Name column to the Samples Grid

					grid.AddColumn(JobNameColumn, "Job", GridColumnType.Text, "Sample", false, false, true, true, false, true, false, HorizontalAlignment.Left, 135);
				}

				// Add Sample ID column to the Samples Grid

				grid.AddColumn(SampleIdColumn, "Sample", GridColumnType.Text, "Sample", false, false, true, true, false, true, false, HorizontalAlignment.Left, 135);
				return;
			}

			// This is the test grid

			grid.AddColumn(SampleIdColumn, "Sample", GridColumnType.Text, "Test", false, false, true, true, false, true, false, HorizontalAlignment.Left, 135);
			grid.AddColumn(TestIdColumn, "Test", GridColumnType.Text, "Test", false, false, true, true, false, true, false, HorizontalAlignment.Left, 135);

			m_AssignColumn = grid.AddColumn(AssignColumn, "Assign", GridColumnType.Boolean, "Test", false, false, false, true, true, false, true, HorizontalAlignment.Center, 50);
			m_AssignColumn.ValueChanged += AssignColumn_ValueChanged;
		}

		/// <summary>
		/// Populates the default columns.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="row">The row.</param>
		/// <param name="entity">The entity.</param>
		private void PopulateDefaultColumns(UnboundGrid grid, UnboundGridRow row, IEntity entity)
		{
			if (grid == m_Form.GridJobProperties)
			{
				// Set Job Name

				JobHeader jobHeader = (JobHeader) entity;
				row[JobNameColumn] = jobHeader.JobName;
				return;
			}

			if (grid == m_Form.GridSampleProperties)
			{
				Sample sample = (Sample) entity;

				if (IsJobWorkflow)
				{
					// Set Job Name

					row[JobNameColumn] = sample.JobName.JobName;
				}

				// Set Sample ID

				row[SampleIdColumn] = sample.IdText;
				return;
			}

			// This is the test grid

			Test test = (Test) entity;

			row[SampleIdColumn] = test.Sample.IdText;
			row[TestIdColumn] = test.TestCount == 1 ? test.Analysis.VersionedAnalysisName : string.Format("{0}/{1}", test.Analysis.VersionedAnalysisName, test.TestCount);
			row[AssignColumn] = test.Assign;
		}

		/// <summary>
		/// Builds the columns.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="grid">The grid.</param>
		private void BuildColumns(IEntityCollection entities, UnboundGrid grid)
		{
			// Go through each entity to determine which columns are needed in the grid (based on entity template)

			foreach (IEntity entity in entities)
			{
				// Get the entity template from the entity

				EntityTemplateInternal template = GetTemplate(entity);
				if (template == null) continue;

				foreach (EntityTemplateProperty property in template.EntityTemplateProperties)
				{
					// Add a column for this entity template property

					if (property.PromptType.IsPhrase(PhraseEntTmpPt.PhraseIdHIDDEN)) continue;

					// Retrieve or create column

					UnboundGridColumn gridcolumn = grid.GetColumnByName(property.PropertyName);

					if (gridcolumn == null)
					{
						gridcolumn = grid.AddColumn(property.PropertyName, property.LocalTitle, "Properties", 100);

						if (template.TableName == TestInternal.EntityName && property.PropertyName == TestPropertyNames.Instrument)
						{
							// Instruments must be available and not retired
							IQuery query = EntityManager.CreateQuery(InstrumentBase.EntityName);
							query.AddEquals(InstrumentPropertyNames.Available, true);
							query.AddEquals(InstrumentPropertyNames.Retired, false);
							EntityBrowse instrumentBrowse = BrowseFactory.CreateEntityBrowse(query);
							gridcolumn.SetColumnBrowse(instrumentBrowse);
						}
						else
						{
							gridcolumn.SetColumnEditorFromObjectModel(template.TableName, property.PropertyName);
						}
					}
				}
			}
		}

		/// <summary>
		/// Handles the QueryPopulated event of the criteriaTaskService control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Server.CriteriaTaskQueryPopulatedEventArgs"/> instance containing the event data.</param>
		private void CriteriaTaskService_QueryPopulated(object sender, CriteriaTaskQueryPopulatedEventArgs e)
		{
			if (m_InitialisingCriteria)
				m_CriteriaQuery = e.PopulatedQuery;
		}

		/// <summary>
		/// Updates the toolbar based on the current selection.
		/// </summary>
		private void UpdateWorkflowToolbarButtons()
		{
			if (IsTaskReadReadonly)
			{
				// All buttons are invisible
				return;
			}

			if (!BaseEntity.IsValid(m_FocusedTreeEntity))
			{
				// Disable all toolbar buttons

				if (IsJobWorkflow)
				{
					m_AddJobToolBarButton.Enabled = true;
				}

				m_AddSampleToolBarButton.Enabled = !IsJobWorkflow;
				if (m_RootNodeSelected)
				{
					m_AddTestScheduleToolBarButton.Enabled = CanAddTestsToRootNode(m_TopLevelEntities);
					m_AddAnalysisToolBarButton.Enabled = m_AddTestScheduleToolBarButton.Enabled;
				}
				else
				{
					m_AddTestScheduleToolBarButton.Enabled = false;
					m_AddAnalysisToolBarButton.Enabled = false;
				}

				m_AddSubSampleToolBarButton.Enabled = false;

				m_RemoveToolBarButton.Enabled = false;

				return;
			}

			// Something is selected, if its new it can be removed

			m_RemoveToolBarButton.Enabled = CanRemoveNode();

			// Update buttons

			switch (m_FocusedTreeEntity.EntityType)
			{
				case JobHeader.EntityName:

					OnJobSelected();
					break;

				case Sample.EntityName:

					OnSampleSelected();
					break;
			}
		}

		/// <summary>
		/// Determines whether this instance [can remove node].
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if this instance [can remove node]; otherwise, <c>false</c>.
		/// </returns>
		private bool CanRemoveNode()
		{
			if (!m_FocusedTreeEntity.IsNew())
			{
				// Only un-committed data can be removed

				return false;
			}

			if (m_FocusedTreeEntity is JobHeader)
			{
				// This is a New Job, can it be removed

				CreateJobNode createJobNode = ((WorkflowNode) m_FocusedTreeEntity.GetWorkflowNode()).Node as CreateJobNode;

				if (createJobNode != null)
				{
					return createJobNode.AllowRemove;
				}

				// Default to true

				return true;
			}

			// This is a New Sample, can it be removed

			CreateSampleNode createSampleNode = ((WorkflowNode) m_FocusedTreeEntity.GetWorkflowNode()).Node as CreateSampleNode;

			if (createSampleNode != null)
			{
				return createSampleNode.AllowRemove;
			}

			// Default to true

			return true;
		}

		/// <summary>
		/// Called when a job is selected
		/// </summary>
		private void OnJobSelected()
		{
			if (IsJobWorkflow)
			{
				m_AddJobToolBarButton.Enabled = false;
			}

			m_AddSampleToolBarButton.Enabled = CanAddSamplesToJob(m_FocusedTreeEntity);
			m_AddTestScheduleToolBarButton.Enabled = CanAddTestsToJob(m_FocusedTreeEntity);
			m_AddAnalysisToolBarButton.Enabled = m_AddTestScheduleToolBarButton.Enabled;
			m_AddSubSampleToolBarButton.Enabled = false;
		}

		/// <summary>
		/// Updates toolbar when a sample has been selected.
		/// </summary>
		private void OnSampleSelected()
		{
			if (IsJobWorkflow)
			{
				m_AddJobToolBarButton.Enabled = false;
			}

			m_AddSampleToolBarButton.Enabled = false;
			m_AddSubSampleToolBarButton.Enabled = CanSplitSample(m_FocusedTreeEntity);
			m_AddTestScheduleToolBarButton.Enabled = CanAddTests(m_FocusedTreeEntity);
			m_AddAnalysisToolBarButton.Enabled = m_AddTestScheduleToolBarButton.Enabled;
		}

		/// <summary>
		/// Determines whether this instance can add samples.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns>
		/// 	<c>true</c> if this instance can add samples; otherwise, <c>false</c>.
		/// </returns>
		private static bool CanAddSamplesToJob(IEntity entity)
		{
			bool addSample = false;
			JobHeader focusedJob = entity as JobHeader;

			if (focusedJob == null)
			{
				// Irrelevant unless a Job is selected

				return false;
			}

			// This is a Job, can samples be added to it?

			CreateJobNode createJobNode = ((WorkflowNode) entity.GetWorkflowNode()).Node as CreateJobNode;

			if (createJobNode != null)
			{
				addSample = createJobNode.AllowNewSamples;
			}

			if (addSample)
			{
				addSample = ValidStatusForModify(entity);
			}

			return addSample;
		}

		/// <summary>
		/// Determines whether this instance can split samples.
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if this instance can split samples; otherwise, <c>false</c>.
		/// </returns>
		private bool CanSplitSample(IEntity entity)
		{
			bool addSample = false;
			Sample focusedSample = entity as Sample;

			if (focusedSample == null || !HasEntityTemplate(entity))
			{
				// Irrelevant unless a Sample is selected and has Entity Template

				return false;
			}

			// This is a Sample, can it be split?

			CreateSampleNode createSampleNode = ((WorkflowNode) entity.GetWorkflowNode()).Node as CreateSampleNode;

			if (createSampleNode != null)
			{
				addSample = createSampleNode.AllowSplit;
			}

			if (addSample)
			{
				addSample = ValidStatusForModify(entity);
			}
			return addSample;
		}

		/// <summary>
		/// Determines whether this instance can add tests.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns>
		/// 	<c>true</c> if this instance can add tests; otherwise, <c>false</c>.
		/// </returns>
		private static bool CanAddTests(IEntity entity)
		{
			bool addTests = false;
			Sample focusedSample = entity as Sample;

			if (focusedSample == null)
			{
				// Irrelevant unless a Sample is selected

				return false;
			}

			// This is a Sample, can test be added to it?

			CreateSampleNode createSampleNode = ((WorkflowNode) entity.GetWorkflowNode()).Node as CreateSampleNode;

			if (createSampleNode != null)
			{
				addTests = createSampleNode.AllowNewTests;
			}

			if (addTests)
			{
				addTests = ValidStatusForModify(entity);
			}

			return addTests;
		}

		/// <summary>
		/// If we can add tests to one sample.
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		private static bool CanAddTestsToJob(IEntity entity)
		{
			JobHeader focusedJob = entity as JobHeader;

			if (focusedJob != null)
			{
				foreach (Sample sample in focusedJob.RootSamples)
				{
					if (CanAddTests(sample))
					{
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Add tests to a collection of entities.
		/// </summary>
		/// <param name="collection"></param>
		/// <returns></returns>
		private static bool CanAddTestsToRootNode(IEntityCollection collection)
		{
			bool addTestsToGridData = false;

			foreach (IEntity entity in collection)
			{
				if (entity is JobHeader)
				{
					addTestsToGridData = CanAddTestsToJob(entity);
					if (addTestsToGridData)
					{
						return addTestsToGridData;
					}
				}

				if (entity is Sample)
				{
					addTestsToGridData = CanAddTests(entity);
					if (addTestsToGridData)
					{
						return addTestsToGridData;
					}
				}
			}

			return addTestsToGridData;
		}

		/// <summary>
		/// Stores the grid data.
		/// </summary>
		/// <param name="grid">The grid.</param>
		private void StoreGridDataInEntities(UnboundGrid grid)
		{
			if (grid == m_Form.TestAssignmentGrid)
			{
				return;
			}

			// Store grid data in object model

			foreach (UnboundGridRow row in grid.Rows)
			{
				IEntity rowEntity = (IEntity) row.Tag;

				// Adjust for the "Assign" field

				int startPos = grid.FixedColumns;
				if (rowEntity.EntityType == TestBase.EntityName)
				{
					startPos = startPos - 1;
				}

				// Spin through and update

				for (int i = startPos; i < grid.Columns.Count; i++)
				{
					UnboundGridColumn column = grid.Columns[i];
					if ((column.Name == AssignColumn && rowEntity.EntityType == TestBase.EntityName) || column.IsCellReadOnly(row)) continue; // Handled elsewhere

					object value = row[column];

					if (value is DateTime)
					{
						value = new NullableDateTime((DateTime) value);
					}

					rowEntity.Set(column.Name, value);
				}
			}
		}

		/// <summary>
		/// Initialises the grid orientation.
		/// </summary>
		private void InitialiseGridOrientation()
		{
			// GridModeLookup keeps track of which grids are currently vertical on the client

			m_GridModeLookup = new Dictionary<UnboundGrid, bool>();

			// Read jobs grid mode config settings

			SetupGridLayout(m_Form.GridJobProperties, GridModeVertical);

			// Read samples grid mode config settings

			SetupGridLayout(m_Form.GridSampleProperties, GridModeVertical);

			// Read tests grid mode config settings

			SetupGridLayout(m_Form.GridTestProperties, GridModeHorizontal);

			RefreshPropagateButtons();
		}

		/// <summary>
		/// Setups the grid layout.
		/// </summary>
		/// <param name="unboundGrid">The unbound grid.</param>
		/// <param name="defaultMode">The default mode.</param>
		private void SetupGridLayout(UnboundGrid unboundGrid, string defaultMode)
		{
			// Layouts are remembered			
			string layoutSetting = Library.Environment.ReadLocalSetting(SampleLoginSettingType, unboundGrid.Name, SampleLoginSettingProperty);

			if (!string.IsNullOrEmpty(layoutSetting))
			{
				// Store the layout setting
				bool isVertical = layoutSetting == GridModeVertical;
				m_GridModeLookup[unboundGrid] = isVertical;

				if (layoutSetting != defaultMode)
				{
					// The mode differs from the layout defined in the form definition
					unboundGrid.Transpose();
				}
			}
			else
			{
				// Set default mode
				m_GridModeLookup[unboundGrid] = defaultMode == GridModeVertical;
			}
		}

		/// <summary>
		/// Saves the grid layout.
		/// </summary>
		/// <param name="unboundGrid">The unbound grid.</param>
		private void SaveGridLayout(UnboundGrid unboundGrid)
		{
			// save grid layouts
			string layoutSetting = m_GridModeLookup[unboundGrid] ? GridModeVertical : GridModeHorizontal;
			Library.Environment.WriteLocalSetting(SampleLoginSettingType, unboundGrid.Name, SampleLoginSettingProperty, layoutSetting);
		}

		#endregion

		#region Events

		/// <summary>
		/// Handles the NodeAdded event of the TreeListItems control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.SimpleTreeListNodeEventArgs"/> instance containing the event data.</param>
		private void TreeListItems_NodeAdded(object sender, SimpleTreeListNodeEventArgs e)
		{
			IEntity newEntity = e.Node.Data as IEntity;

			if (!BaseEntity.IsValid(newEntity))
			{
				return;
			}

			if (newEntity != null)
			{
				if (newEntity.EntityType == Sample.EntityName)
				{
					// Add all child samples for this sample
					Sample sample = (Sample) newEntity;
					foreach (Sample subSample in sample.ChildSamples)
					{
						IconName icon = new IconName(((IEntity) subSample).Icon);
						string nodeText = GetEntityDisplayText(subSample);
						m_Form.TreeListItems.AddNode(e.Node, nodeText, icon, subSample);
					}
				}
				else if (newEntity.EntityType == JobHeader.EntityName)
				{
					// Add all samples for this job
					JobHeader job = (JobHeader) newEntity;

					foreach (Sample sample in job.RootSamples)
					{
						IconName icon = new IconName(((IEntity) sample).Icon);
						string nodeText = GetEntityDisplayText(sample);
						m_Form.TreeListItems.AddNode(e.Node, nodeText, icon, sample);
					}
				}
			}

			m_TestAssignmentInitialised = false;
		}

		/// <summary>
		/// Handles the FocusedNodeChanged event of the TreeListItems control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.SimpleFocusedNodeChangedEventArgs"/> instance containing the event data.</param>
		private void TreeListItems_FocusedNodeChanged(object sender, SimpleFocusedNodeChangedEventArgs e)
		{
			if (!m_Loaded) return;

			if (m_VisibleGrid != null)
			{
				StoreGridDataInEntities(m_VisibleGrid);
			}

			// Get the selected entity from the tree

			IEntity oldFocusedNode = m_FocusedTreeEntity;
			m_FocusedTreeEntity = e.NewNode == null ? null : e.NewNode.Data as IEntity;

			if (e.NewNode != null)
				m_RootNodeSelected = e.NewNode.IsRootNode;
			else
				m_RootNodeSelected = false;

			// Work out if we need to change tabs.

			if (oldFocusedNode is JobHeader && m_FocusedTreeEntity is Sample)
			{
				// Job -> Sample

				if (m_Form.MainTabControl.SelectedPage.Name == TabPageJobs)
				{
					m_TabPageSamples.Show();
				}
				else
				{
					UpdateTabPage();
				}
			}
			else if (oldFocusedNode is Sample && m_FocusedTreeEntity is JobHeader)
			{
				// Sample -> Job

				if (m_Form.MainTabControl.SelectedPage.Name == TabPageSamples)
				{
					m_TabPageJobs.Show();
				}
				else
				{
					UpdateTabPage();
				}
			}
			else
			{
				UpdateTabPage();
			}

			// The toolbar buttons are dependant on the node type

			UpdateWorkflowToolbarButtons();
			ClearBusy();
		}

		/// <summary>
		/// Handles the SelectedPageChanging event of the TabControl1 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.TabControlSelectedPageChangedEventArgs"/> instance containing the event data.</param>
		private void TabControl1_SelectedPageChanged(object sender, TabControlSelectedPageChangedEventArgs e)
		{
			if (!m_Loaded) return;

			SetSelectedTabPage(e.NewPage.Name);
		}

		/// <summary>
		/// Updates the tab page.
		/// </summary>
		private void UpdateTabPage()
		{
			if (m_Form.MainTabControl.SelectedPage == null) return;
			SetSelectedTabPage(m_Form.MainTabControl.SelectedPage.Name);
		}

		/// <summary>
		/// Sets the selected tab page internal.
		/// </summary>
		/// <param name="tabPage">The tab page.</param>
		private void SetSelectedTabPage(string tabPage)
		{
			switch (tabPage)
			{
				case TabPageJobs:

					m_VisibleGrid = m_Form.GridJobProperties;

					RefreshJobsGrid();

					m_ToolBarButtonTranspose.Enabled = true;
					RefreshPropagateButtons();

					break;

				case TabPageSamples:

					m_VisibleGrid = m_Form.GridSampleProperties;

					RefreshSamplesGrid();

					m_ToolBarButtonTranspose.Enabled = true;
					RefreshPropagateButtons();

					break;

				case TabPageTests:

					m_VisibleGrid = m_Form.GridTestProperties;

					RefreshTestsGrid();

					m_ToolBarButtonTranspose.Enabled = true;
					RefreshPropagateButtons();
					EnableToolBarFillButtons(false);

					break;

				case TabPageTestAssignment:

					m_VisibleGrid = m_Form.TestAssignmentGrid;

					if (!m_TestAssignmentInitialised)
					{
						RebuildTestAssignmentGrid();
					}

					m_ToolBarButtonTranspose.Enabled = false;
					RefreshPropagateButtons();
					EnableToolBarFillButtons(!IsTaskReadReadonly);

					break;
			}
		}

		/// <summary>
		/// Refreshes the jobs grid.
		/// </summary>
		private void RefreshJobsGrid()
		{
			JobHeader focusedJob = m_FocusedTreeEntity as JobHeader;
			Sample focusedSample = m_FocusedTreeEntity as Sample;

			if (BaseEntity.IsValid(focusedJob))
			{
				IEntityCollection gridJobs = EntityManager.CreateEntityCollection(JobHeader.EntityName);
				gridJobs.Add(focusedJob);
				RefreshGrid(m_Form.GridJobProperties, gridJobs);
			}
			else if (focusedSample != null && BaseEntity.IsValid(focusedSample))
			{
				IEntityCollection gridJobs = EntityManager.CreateEntityCollection(JobHeader.EntityName);
				gridJobs.Add(focusedSample.JobName);
				RefreshGrid(m_Form.GridJobProperties, gridJobs);
			}
			else
			{
				RefreshGrid(m_Form.GridJobProperties, m_TopLevelEntities);
			}
		}

		/// <summary>
		/// Refreshes the samples grid.
		/// </summary>
		protected void RefreshSamplesGrid()
		{
			IEntityCollection gridSamples = EntityManager.CreateEntityCollection(Sample.EntityName);

			// This is a job workflow, determine if the root or a job is selected and populate the grid

			JobHeader focusedJob = m_FocusedTreeEntity as JobHeader;
			Sample focusedSample = m_FocusedTreeEntity as Sample;

			if (focusedJob != null && BaseEntity.IsValid(focusedJob))
			{
				foreach (Sample sample in focusedJob.Samples)
				{
					AddSampleToGridData(sample, gridSamples);
				}
			}
			else if (focusedSample != null && BaseEntity.IsValid(focusedSample))
			{
				// Sample is selected in the Tree

				AddSampleToGridData(focusedSample, gridSamples);

				foreach (Sample subSample in focusedSample.ChildSamples)
				{
					AddSampleToGridData(subSample, gridSamples);
				}
			}
			else
			{
				// Root Node is selected in the Tree

				if (IsJobWorkflow)
				{
					// Root node is selected, add all samples for all jobs

					foreach (JobHeader job in m_TopLevelEntities.ActiveItems)
					{
						// Add all samples for this job

						foreach (Sample sample in job.Samples)
						{
							AddSampleToGridData(sample, gridSamples);
						}
					}
				}
				else
				{
					// This is a Sample Workflow

					foreach (Sample childSample in m_TopLevelEntities.ActiveItems)
					{
						AddSampleToGridData(childSample, gridSamples);
					}
				}
			}

			RefreshGrid(m_Form.GridSampleProperties, gridSamples);
		}

		/// <summary>
		/// Adds the sample and formulations.
		/// </summary>
		/// <param name="sample">The sample.</param>
		/// <param name="collection">The collection.</param>
		private static void AddSampleToGridData(Sample sample, IEntityCollection collection)
		{
			collection.Add(sample);

			foreach (Sample subSample in sample.ChildSamples)
			{
				AddSampleToGridData(subSample, collection);
			}
		}

		/// <summary>
		/// Refreshes the tests grid.
		/// </summary>
		protected virtual void RefreshTestsGrid()
		{
			IEntityCollection gridTests = EntityManager.CreateEntityCollection(Test.EntityName);

			m_Form.GridTestProperties.BeginUpdate();

			JobHeader focusedJob = m_FocusedTreeEntity as JobHeader;
			Sample focusedSample = m_FocusedTreeEntity as Sample;

			if (focusedJob != null && BaseEntity.IsValid(focusedJob))
			{
				// Add all tests from all samples for the selected job

				foreach (Sample sample in focusedJob.Samples)
				{
					AddTestsToGridData(sample, gridTests);
				}
			}
			else if (BaseEntity.IsValid(focusedSample))
			{
				AddTestsToGridData(focusedSample, gridTests);
			}
			else
			{
				// Root node is selected

				if (IsJobWorkflow)
				{
					// Add all tests for all samples in this job

					foreach (JobHeader job in m_TopLevelEntities.ActiveItems)
					{
						foreach (Sample sample in job.Samples)
						{
							AddTestsToGridData(sample, gridTests);
						}
					}
				}
				else
				{
					// Add all tests for all samples

					foreach (Sample sample in m_TopLevelEntities.ActiveItems)
					{
						AddTestsToGridData(sample, gridTests);
					}
				}
			}

			RefreshGrid(m_Form.GridTestProperties, gridTests);

			EnableDisableAssignCells();

			m_Form.GridTestProperties.EndUpdate();
		}

		/// <summary>
		/// Adds the tests to grid data.
		/// </summary>
		/// <param name="sample">The sample.</param>
		/// <param name="gridTests">The grid tests.</param>
		private static void AddTestsToGridData(Sample sample, IEntityCollection gridTests)
		{
			foreach (Test test in sample.Tests)
			{
				gridTests.Add(test);
			}

			foreach (Sample subSample in sample.ChildSamples)
			{
				AddTestsToGridData(subSample, gridTests);
			}
		}

		/// <summary>
		/// Refreshes the visible grid.
		/// </summary>
		private void RefreshVisibleGrid()
		{
			if (m_Loaded)
			{
				if (m_VisibleGrid == m_Form.GridJobProperties)
				{
					RefreshJobsGrid();
				}
				else if (m_VisibleGrid == m_Form.GridSampleProperties)
				{
					RefreshSamplesGrid();
				}
				else if (m_VisibleGrid == m_Form.GridTestProperties)
				{
					RefreshTestsGrid();
				}
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets a value indicating whether this instance is task read readonly.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is task read readonly; otherwise, <c>false</c>.
		/// </value>
		private bool IsTaskReadReadonly
		{
			get { return Context.LaunchMode == TaskReadOnlyMode; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is job workflow.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is job workflow; otherwise, <c>false</c>.
		/// </value>
		protected abstract bool IsJobWorkflow { get; }

		#endregion

		#region Workflow

		/// <summary>
		/// Performs the workflow.
		/// </summary>
		/// <param name="workflow">The workflow.</param>
		/// <param name="propertyBag">The property bag.</param>
		/// <returns></returns>
		protected bool PerformWorkflow(Workflow workflow, IWorkflowPropertyBag propertyBag)
		{
			if (propertyBag == null)
			{
				// Perform the workflow & validate it's output

				propertyBag = workflow.Perform();
			}
			else
			{
				workflow.Perform(propertyBag);
			}

			// Make sure the workflow generated something

			if (propertyBag.Count == 0)
			{
				// Un-supported entity type

				string message = Library.Message.GetMessage("GeneralMessages", "EmptyWorkflowOutput");
				Library.Utils.FlashMessage(message, m_Title, MessageButtons.OK, MessageIcon.Error, MessageDefaultButton.Button1);

				return false;
			}

			// Exit if there are errors

			if (propertyBag.Errors.Count > 0)
			{
				Library.Utils.FlashMessage(propertyBag.Errors[0].Message, m_Title, MessageButtons.OK, MessageIcon.Error, MessageDefaultButton.Button1);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Adds the new workflow.
		/// </summary>
		/// <param name="workflowType">Name of the table.</param>
		/// <param name="exitOnCancel">if set to <c>true</c> the sample login task should exit if the dialog is cancelled.</param>
		protected void AddNewWorkflow(string workflowType, bool exitOnCancel = false)
		{
			m_AddWorkflowType = workflowType;
			m_ExitOnCancel = exitOnCancel;

			// Create popup form and set header label text

			FormSampleAdminAddWorkflow form = (FormSampleAdminAddWorkflow) FormFactory.CreateForm(typeof (FormSampleAdminAddWorkflow));
			form.Loaded += AddWorkflowForm_Loaded;
			form.ShowDialog();
		}

		/// <summary>
		/// Handles the Loaded event of the form control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void AddWorkflowForm_Loaded(object sender, EventArgs e)
		{
			FormSampleAdminAddWorkflow form = ConfigureAddWorkflowForm((FormSampleAdminAddWorkflow) sender);
			form.Closed += AddWorkflowFormClosed;
		}

		/// <summary>
		/// Configures the add workflow form.
		/// </summary>
		/// <param name="form">The form.</param>
		/// <returns></returns>
		protected FormSampleAdminAddWorkflow ConfigureAddWorkflowForm(FormSampleAdminAddWorkflow form)
		{
			switch (m_AddWorkflowType)
			{
				case PhraseWflowType.PhraseIdJOB_HEADER:

					form.LabelAddNewWorkflow.Caption = form.StringTable.JobLabelText;
					form.Title = form.StringTable.JobTitle;
					break;

				case PhraseWflowType.PhraseIdSAMPLE:

					form.LabelAddNewWorkflow.Caption = (IsJobWorkflow) ? form.StringTable.SampleToJobLabelText : form.StringTable.SampleLabelText;
					form.Title = form.StringTable.SampleTitle;
					break;

				case PhraseWflowType.PhraseIdSUBSAMPLE:

					form.LabelAddNewWorkflow.Caption = form.StringTable.SubSampleLabelText;
					form.Title = form.StringTable.SubSampleTitle;
					break;
			}

			// Setup browse and show the dialog

			IQuery query = EntityManager.CreateQuery(Workflow.EntityName);

			query.AddEquals(WorkflowPropertyNames.WorkflowType, m_AddWorkflowType);
			form.PromptEntityBrowseWorkflow.Browse = BrowseFactory.CreateEntityBrowse(query);

			// We have a default workflow to use

			if (DefaultWorkflow != null && DefaultWorkflow.TableName == m_AddWorkflowType)
			{
				form.PromptEntityBrowseWorkflow.Entity = DefaultWorkflow;
				form.SpinEditNewCount.Focus();
			}

			return form;
		}

		/// <summary>
		/// Handles the Closing event of the AddWorkflowForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void AddWorkflowFormClosed(object sender, EventArgs e)
		{
			FormSampleAdminAddWorkflow form = (FormSampleAdminAddWorkflow) sender;

			if (form.FormResult == FormResult.OK)
			{
				// Execute the selected workflow for the specified number of times. Push it onto a separate thread.

				ThreadPool.QueueUserWorkItem(state =>
				{
					try
					{
						RunWorkflow(form);
					}
					catch (Exception ex)
					{
						OnException(ex);
					}
				});

			}
			else if (form.FormResult == FormResult.Cancel && m_ExitOnCancel)
			{
				// Take the same route out as a fatal error.

				m_FatalErrorOccured = true;
				Exit();
			}

			m_SubSampling = false;
			m_ExitOnCancel = false;
		}

		/// <summary>
		/// Runs the workflow.
		/// </summary>
		/// <param name="form">The form.</param>
		private void RunWorkflow(object form)
		{
			FormSampleAdminAddWorkflow addForm = (FormSampleAdminAddWorkflow) form;
			Workflow workflow = (Workflow) addForm.PromptEntityBrowseWorkflow.Entity;
			int count = addForm.SpinEditNewCount.Number;
			RunWorkflow(workflow, count);
		}

		/// <summary>
		/// Runs the workflow (once)
		/// </summary>
		/// <param name="workflow">The workflow.</param>
		protected void RunWorkflowOnce(object workflow)
		{
			Workflow flow = (Workflow) workflow;
			RunWorkflow(flow, 1);
		}

		/// <summary>
		/// Runs the workflow.
		/// </summary>
		/// <param name="workflow">The workflow.</param>
		/// <param name="count">The count.</param>
		private void RunWorkflow(Workflow workflow, int count)
		{
			List<IEntity> newEntities = new List<IEntity>();

			SetBusy("SampleLoginRunWorkflowDesc", "SampleLoginRunWorkflowCaption");

			try
			{
				// We can end up back in here

				m_FatalErrorOccured = false;

				// Don't update the tree as we go along.

				m_Form.TreeListItems.SuppressAddEvents = true;
				m_Form.TreeListItems.FocusedNodeChanged -= TreeListItems_FocusedNodeChanged;

				// Determine the targets

				IEnumerable targets = GetWorkflowTargets();

				foreach (IEntity entity in targets)
				{
					RunWorkflowForEntity(entity, workflow, count, newEntities);
				}

				UpdateTestAssignmentGrid(workflow.TableName, newEntities);
			}
			catch (FatalWorkflowError fatalError)
			{
				// A Fatal Error has occured whilst processing this Workflow, 
				// Display the error to the user and exit the task

				string errorMessage = Library.Message.GetMessage("GeneralMessages", "SampleLoginFatalWorkflowErrorMessage");
				errorMessage = string.Format(errorMessage, fatalError.Message);
				Library.Utils.FlashMessage(errorMessage, m_Title, MessageButtons.OK, MessageIcon.Error, MessageDefaultButton.Button1);

				m_FatalErrorOccured = true;
			}
			finally
			{
				if (m_FatalErrorOccured)
				{
					// Fatal error occured, exit this session

					Exit();
				}
				else
				{
					Library.Task.StateModified();
					m_Form.TreeListItems.SuppressAddEvents = false;

					m_Form.TreeListItems.FocusedNodeChanged += TreeListItems_FocusedNodeChanged;

					// Update Set Position

					if (newEntities.Count > 0)
					{
						var node = m_Form.TreeListItems.FindNodeByData(newEntities[0]);
						if (node != null)
						{
							if (m_Loaded)
								m_Form.TreeListItems.FocusNode(node);
							else
								m_FocusOnLoadNode = node;
						}
					}
				}

				ClearBusy();
			}
		}

		/// <summary>
		/// Runs the workflow for entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="selectedWorkflow">The selected workflow.</param>
		/// <param name="count">The count.</param>
		/// <param name="allNewEntities">Updated list of entities</param>
		private void RunWorkflowForEntity(IEntity entity, Workflow selectedWorkflow, int count, List<IEntity> allNewEntities)
		{
			// Run the Workflows

			var newEntities = RunWorkflowForEntity(entity, selectedWorkflow, count);

			// Once we've got our list of new entities - show them

			SimpleTreeListNodeProxy targetNode;

			bool subSampling = selectedWorkflow.WorkflowType.IsPhrase(PhraseWflowType.PhraseIdSUBSAMPLE);

			if (selectedWorkflow.TableName == m_TopLevelTableName && !subSampling)
			{
				targetNode = m_RootNode;

				// Add new top level entities

				foreach (IEntity newEntity in newEntities)
				{
					m_TopLevelEntities.Add(newEntity);
				}
			}
			else
			{
				targetNode = m_Form.TreeListItems.FindNodeByData(entity);
			}

			// Put the data on the tree...

			if (subSampling)
			{
				newEntities = AddSubSamplesToTree(targetNode);
			}
			else
			{
				AddDataToTree(newEntities, targetNode);
			}

			allNewEntities.AddRange(newEntities);

			// Refresh the Tree - if we did some work.

			targetNode.RefreshList();
			RefreshVisibleGrid();
		}

		/// <summary>
		/// Runs the workflow for entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="selectedWorkflow">The selected workflow.</param>
		/// <param name="count">The count.</param>
		/// <returns>List of newly created entities.</returns>
		private IList<IEntity> RunWorkflowForEntity(IEntity entity, Workflow selectedWorkflow, int count)
		{
			List<IEntity> newEntities = new List<IEntity>();

			for (int j = 0; j < count; j++)
			{
				// Run the workflow with a property bag for results - used passed in Parameters if available.

				IWorkflowPropertyBag propertyBag;

				if (selectedWorkflow.Properties == null)
				{
					propertyBag = GeneratePropertyBag(entity);
				}
				else
				{
					propertyBag = selectedWorkflow.Properties;
				}

				// Counters

				propertyBag.Set("$WORKFLOW_MAX", count);
				propertyBag.Set("$WORKFLOW_COUNT", j + 1);

				// Perform

				PerformWorkflow(selectedWorkflow, propertyBag);

				// Keep track of the newly created entities.

				IList<IEntity> entities = propertyBag.GetEntities(selectedWorkflow.TableName);
				newEntities.AddRange(entities);
			}

			return newEntities;
		}

		/// <summary>
		/// Generates the property bag for the passed entity.
		/// </summary>
		/// <returns></returns>
		private static IWorkflowPropertyBag GeneratePropertyBag(IEntity entity)
		{
			// Generate context for the workflow

			IWorkflowPropertyBag propertyBag = new WorkflowPropertyBag();

			if (entity != null)
			{
				// Pass in the selected parent
				propertyBag.Add(entity.EntityType, entity);
			}

			return propertyBag;
		}

		/// <summary>
		/// Sets the busy status of the form
		/// </summary>
		/// <param name="messageMessage">The message message.</param>
		/// <param name="captionMessage">The caption message.</param>
		private void SetBusy(string messageMessage, string captionMessage)
		{
			string caption = Library.Message.GetMessage("GeneralMessages", captionMessage);
			string message = Library.Message.GetMessage("GeneralMessages", messageMessage);
			m_Form.SetBusy(message, caption);

			Logger.DebugFormat("Wait Status {0} - {1}", caption, message);
		}

		/// <summary>
		/// Clears the busy.
		/// </summary>
		private void ClearBusy()
		{
			m_Form.ClearBusy();

			Logger.Debug("Wait Status Cleared");
		}

		/// <summary>
		/// Gets the parent sample.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable GetWorkflowTargets()
		{
			SimpleTreeListNodeProxy targetNode = m_ContextMenuNode ?? m_Form.TreeListItems.FocusedNode;

			IList<IEntity> targets = new List<IEntity>();
			targets.Add(targetNode.Data as IEntity);

			return targets;
		}

		/// <summary>
		/// Adds the data to tree.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="targetNode">The target node.</param>
		private void AddDataToTree(IList<IEntity> entities, SimpleTreeListNodeProxy targetNode)
		{
			if (entities.Count == 0) return;

			foreach (IEntity entity in entities)
			{
				IconName icon = new IconName(entity.Icon);
				string displayText = GetEntityDisplayText(entity);
				m_Form.TreeListItems.AddNode(targetNode, displayText, icon, entity);
			}
		}

		/// <summary>
		/// Add sub samples to the sample node
		/// </summary>
		/// <param name="targetNode">The tree node representing the sample.</param>
		private List<IEntity> AddSubSamplesToTree(SimpleTreeListNodeProxy targetNode)
		{
			List<IEntity> newEntities = new List<IEntity>();
			SampleInternal parentNode = (SampleInternal) targetNode.Data;

			foreach (IEntity entity in parentNode.ChildSamples)
			{
				SimpleTreeListNodeProxy childNode = m_Form.TreeListItems.FindNodeByData(entity);

				if (childNode == null)
				{
					IconName icon = new IconName(entity.Icon);
					string displayText = GetEntityDisplayText(entity);
					childNode = m_Form.TreeListItems.AddNode(targetNode, displayText, icon, entity);
					newEntities.Add(entity);
				}

				// Recurse down

				var childNodes = AddSubSamplesToTree(childNode);
				newEntities.AddRange(childNodes);
			}

			return newEntities;
		}

		/// <summary>
		/// Initialises the tree.
		/// </summary>
		private void InitialiseTree()
		{
			m_Form.TreeListItems.SuppressAddEvents = true;

			foreach (IEntity entity in m_TopLevelEntities.ActiveItems)
			{
				AddTopLevelTreeNode(entity);
			}

			m_Form.TreeListItems.SuppressAddEvents = false;
			m_RootNode.RefreshList();

			IEntity first = m_TopLevelEntities.GetFirst();
			if (first != null)
			{
				var node = m_Form.TreeListItems.FindNodeByData(first);

				if (m_Loaded)
					m_Form.TreeListItems.FocusNode(node);
				else
					m_FocusOnLoadNode = node;
			}
		}

		/// <summary>
		/// Gets the entity display text.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		protected string GetEntityDisplayText(IEntity entity)
		{
			string displayText = string.Empty;

			switch (entity.EntityType)
			{
				case JobHeader.EntityName:

					displayText = ((JobHeader) entity).JobName;
					break;

				case Sample.EntityName:

					if (entity.IsNew())
						displayText = ((Sample) entity).IdText;
					else if (Library.Environment.GetGlobalString("SAMP_BROWSE") == "T")
						displayText = ((Sample) entity).IdText.Trim() + "(" + ((Sample) entity).IdNumeric.String.Trim() + ")";
					else
						displayText = ((Sample) entity).IdNumeric.String.Trim() + "(" + ((Sample) entity).IdText.Trim() + ")";

					break;

				case Test.EntityName:

					displayText = ((Test) entity).Analysis.VersionedAnalysisName;
					break;
			}

			if (!HasEntityTemplate(entity))
			{
				// This entity doesn't have an Entity Template, adjust it's display

				return string.Format(BlankEntityTemplateIndicatorFormat, displayText);
			}

			return displayText;
		}

		/// <summary>
		/// Adds the top level tree node.
		/// </summary>
		/// <param name="entity">The entity.</param>
		private void AddTopLevelTreeNode(IEntity entity)
		{
			IconName icon = new IconName(entity.Icon);
			string displayText = GetEntityDisplayText(entity);
			m_Form.TreeListItems.AddNode(m_RootNode, displayText, icon, entity);
		}

		#endregion

		#region Tree ContextMenu

		/// <summary>
		/// Handles the BeforePopup event of the ContextMenu control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void ContextMenu_BeforePopup(object sender, ContextMenuBeforePopupEventArgs e)
		{
			m_ContextMenuNode = m_Form.TreeListItems.FindNodeByData(e.Entity);

			IEntity selectedItem = e.Entity;

			if (!BaseEntity.IsValid(selectedItem))
			{
				// Nothing is selected, just allow top level items to be added

				m_SubSampleContextMenuItem.Visible = false;
				m_AddTestContextMenuItem.Visible = false;
				m_AddTestScheduleContextMenuItem.Visible = false;

				m_JobContextMenuItem.Visible = IsJobWorkflow;
				m_SampleContextMenuItem.Visible = !IsJobWorkflow;

				return;
			}

			// Show/hide popup items based on selection

			m_JobContextMenuItem.Visible = false;
			m_SampleContextMenuItem.Visible = CanAddSamplesToJob(selectedItem);

			m_SubSampleContextMenuItem.Visible = CanSplitSample(selectedItem);
			m_AddTestContextMenuItem.Visible = CanAddTests(selectedItem);
			m_AddTestScheduleContextMenuItem.Visible = CanAddTests(selectedItem);
		}

		/// <summary>
		/// Handles the ItemClicked event of the m_JobContextMenuItem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ContextMenuItemEventArgs"/> instance containing the event data.</param>
		private void JobContextMenuItem_ItemClicked(object sender, ContextMenuItemEventArgs e)
		{
			AddNewWorkflow(PhraseWflowType.PhraseIdJOB_HEADER);
		}

		/// <summary>
		/// Handles the ItemClicked event of the m_SampleContextMenuItem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ContextMenuItemEventArgs"/> instance containing the event data.</param>
		private void SampleContextMenuItem_ItemClicked(object sender, ContextMenuItemEventArgs e)
		{
			AddNewWorkflow(PhraseWflowType.PhraseIdSAMPLE);
		}

		/// <summary>
		/// Handles the ItemClicked event of the m_TestContextMenuItem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ContextMenuItemEventArgs"/> instance containing the event data.</param>
		private void SubSampleContextMenuItem_ItemClicked(object sender, ContextMenuItemEventArgs e)
		{
			m_SubSampling = true;
			AddNewWorkflow(PhraseWflowType.PhraseIdSUBSAMPLE);
		}

		/// <summary>
		/// Handles the ItemClicked event of the AddTestContextMenuItem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ContextMenuItemEventArgs"/> instance containing the event data.</param>
		private void AddTestContextMenuItem_ItemClicked(object sender, ContextMenuItemEventArgs e)
		{
			AddTest((Sample) m_ContextMenuNode.Data);
		}

		/// <summary>
		/// Handles the ItemClicked event of the AddTestScheduleContextMenuItem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ContextMenuItemEventArgs"/> instance containing the event data.</param>
		private void AddTestScheduleContextMenuItem_ItemClicked(object sender, ContextMenuItemEventArgs e)
		{
			AddTestSchedule((Sample) m_ContextMenuNode.Data);
		}

		#endregion

		#region Grid Toolbar Buttons

		/// <summary>
		/// Shows the hide grid tool bar buttons.
		/// </summary>
		private void RefreshPropagateButtons()
		{
			m_VisibleGrid.FocusedColumnChanged -= m_VisibleGrid_FocusedColumnChanged;
			m_VisibleGrid.FocusedColumnChanged += m_VisibleGrid_FocusedColumnChanged;
			if (m_VisibleGrid == m_Form.TestAssignmentGrid)
			{
				m_ToolBarButtonFillUp.Visible = true;
				m_ToolBarButtonFillDown.Visible = true;
				m_ToolBarButtonFillLeft.Visible = false;
				m_ToolBarButtonFillRight.Visible = false;
			}
			else
			{
				bool isVertical = m_GridModeLookup[m_VisibleGrid];
				m_ToolBarButtonFillUp.Visible = !isVertical;
				m_ToolBarButtonFillDown.Visible = !isVertical;
				m_ToolBarButtonFillLeft.Visible = isVertical;
				m_ToolBarButtonFillRight.Visible = isVertical;
			}
		}

		/// <summary>
		/// Handles the Click event of the m_ToolBarButtonTranspose control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void ToolBarButtonTranspose_Click(object sender, EventArgs e)
		{
			m_VisibleGrid.Transpose();

			bool isVertical = m_GridModeLookup[m_VisibleGrid];
			m_GridModeLookup[m_VisibleGrid] = !isVertical;

			RefreshPropagateButtons();
		}

		/// <summary>
		/// Handles the Click event of the m_ToolBarButtonFillUp control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void ToolBarButtonFillUp_Click(object sender, EventArgs e)
		{
			FillUpLeft();
		}

		/// <summary>
		/// Handles the Click event of the m_ToolBarButtonFillLeft control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void ToolBarButtonFillLeft_Click(object sender, EventArgs e)
		{
			FillUpLeft();
		}

		/// <summary>
		/// Handles the Click event of the m_ToolBarButtonFillDown control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void ToolBarButtonFillDown_Click(object sender, EventArgs e)
		{
			FillDownRight();
		}

		/// <summary>
		/// Handles the Click event of the m_ToolBarButtonFillRight control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void ToolBarButtonFillRight_Click(object sender, EventArgs e)
		{
			FillDownRight();
		}

		/// <summary>
		/// Handles the Click event of the m_ToolBarButtonFillAll control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void ToolBarButtonFillAll_Click(object sender, EventArgs e)
		{
			FillAll();
		}

		#endregion

		#region Validation

		/// <summary>
		/// Validates the entities.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <returns></returns>
		private bool ValidateEntities(IEntityCollection entities)
		{
			foreach (IEntity entity in entities)
			{
				// Get the entity template

				EntityTemplateBase template = GetTemplate(entity);

				// Validate values for this entity against its entity template

				foreach (EntityTemplateProperty templateProperty in template.EntityTemplateProperties)
				{
					object value = entity.Get(templateProperty.PropertyName);

					// Check all mandatory prompts have been entered

					bool isMandatory = (templateProperty.PromptType.PhraseId == PhraseEntTmpPt.PhraseIdMANDATORY);

					if (isMandatory && (value == null || value.ToString() == string.Empty ||
					                    (value is IEntity && !BaseEntity.IsValid((IEntity) value))))
					{
						string message = Library.Message.GetMessage("GeneralMessages", "SampleAdminEmptyMandatory");
						Library.Utils.FlashMessage(message, m_Title, MessageButtons.OK, MessageIcon.Error, MessageDefaultButton.Button1);
						return false;
					}
				}

				if (entity.EntityType != Test.EntityName)
				{
					// Validate children

					string childPropertyName = entity.EntityType == JobHeader.EntityName ? JobHeaderPropertyNames.Samples : SamplePropertyNames.Tests;
					IEntityCollection children = entity.GetEntityCollection(childPropertyName);

					if (!ValidateEntities(children))
					{
						return false;
					}

					if (entity.EntityType == SampleBase.EntityName)
					{
						if (!ValidateEntities(((Sample) entity).ChildSamples))
						{
							return false;
						}
					}

				}
			}

			return true;
		}

		/// <summary>
		/// Handles the ValidateCell event of the GridSampleProperties control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.UnboundGridValidateCellEventArgs"/> instance containing the event data.</param>
		private void GridSampleProperties_ValidateCell(object sender, UnboundGridValidateCellEventArgs e)
		{
			if (e.Column.Name == SamplePropertyNames.LocationId)
			{
				LocationBase location = (LocationBase) e.Value;

				if (location == null)
				{
					return;
				}

				if (!location.Assignable)
				{
					// Only assignable locations are allowed
					e.IsValid = false;
					e.ErrorText = Library.Message.GetMessage("GeneralMessages", "AssignableLocationMessage");
				}
			}
		}

		/// <summary>
		/// Called when a value is being filled.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.UnboundGridValueChangingEventArgs"/> instance containing the event data.</param>
		private void OnServerFillingValue(object sender, UnboundGridValueChangingEventArgs e)
		{
			// Make sure invalid values cannot be assigned into filtered cells
			IEntity targetEntity = (IEntity) e.Row.Tag;
			EntityTemplateInternal targetTemplate = GetTemplate(targetEntity);
			EntityTemplatePropertyInternal targetTemplateProperty = targetTemplate.GetProperty(e.Column.Name);

			// Make sure invalid Component Lists cannot be filled
			TestInternal test = targetEntity as TestInternal;
			if (test != null && e.SourceColumn.Name == TestPropertyNames.ComponentList)
			{
				// Make sure the target test has the component list defined
				bool componentListExists = ((VersionedAnalysis) test.Analysis).GetComponentList(e.Value as string) != null;
				if (!componentListExists)
				{
					// Component List is being copied but it is not defined on the target entity
					e.Cancel = true;
					return;
				}
			}

			if (targetTemplateProperty != null)
			{
				if (!string.IsNullOrEmpty(targetTemplateProperty.FilterBy))
				{
					// The target cell here is filtered by another value, make sure the source of 
					// the fill operation has the same filter value
					object filterValue = e.Row[targetTemplateProperty.FilterBy];
					object sourceFilterValue = e.SourceRow[targetTemplateProperty.FilterBy];

					// Values can only be filled into this cell when the FilterBy source has a value and it matches the source filter value
					e.Cancel = filterValue == null || !filterValue.Equals(sourceFilterValue);
				}
				else if (!string.IsNullOrEmpty(targetTemplateProperty.Criteria))
				{
					IEntity fillValue = e.Value as IEntity;
					if (BaseEntity.IsValid(fillValue))
					{
						// Make sure the fill value satisfies this cell's Criteria
						EntityBrowse criteriaBrowse = e.Column.GetCellBrowse(e.Row) as EntityBrowse;
						if (criteriaBrowse != null)
						{
							if (m_CriteriaBrowseLookup.ContainsKey(criteriaBrowse))
							{
								IEntityCollection criteriaData = m_CriteriaBrowseLookup[criteriaBrowse];
								e.Cancel = !criteriaData.Contains(fillValue);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// m_VisibleGrid_FocusedColumnChanged
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void m_VisibleGrid_FocusedColumnChanged(object sender, UnboundGridFocusedColumnChangedEventArgs e)
		{
			if (e.Column != null)
			{
				if (e.Column.Name == AssignColumn)
				{
					EnableToolBarFillButtons(false);
				}
				else
				{
					EnableToolBarFillButtons(!IsTaskReadReadonly);
				}
			}
		}

		/// <summary>
		/// Valids the status for modify.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		protected static bool ValidStatusForModify(IEntity entity)
		{
			if (entity is JobHeader)
			{
				if (!((JobHeader) entity).JobStatus.IsPhrase(PhraseJobStat.PhraseIdC) &&
				    !((JobHeader) entity).JobStatus.IsPhrase(PhraseJobStat.PhraseIdV) &&
				    !entity.IsNew())
				{
					return false;
				}
			}

			if (entity is Sample)
			{
				if (!((Sample) entity).Status.IsPhrase(PhraseSampStat.PhraseIdC) &&
				    !((Sample) entity).Status.IsPhrase(PhraseSampStat.PhraseIdW) &&
				    !((Sample) entity).Status.IsPhrase(PhraseSampStat.PhraseIdV) &&
				    !((Sample) entity).Status.IsPhrase(PhraseSampStat.PhraseIdU) &&
                    !((Sample) entity).Status.IsPhrase(PhraseSampStat.PhraseIdH) &&  // B.Smock.02
                    !entity.IsNew())
				{
					return false;
				}
			}

			if (entity is Test)
			{
				if (!((Test) entity).Status.IsPhrase(PhraseTestStat.PhraseIdC) &&
				    !((Test) entity).Status.IsPhrase(PhraseTestStat.PhraseIdW) &&
				    !((Test) entity).Status.IsPhrase(PhraseTestStat.PhraseIdV) &&
				    !((Test) entity).Status.IsPhrase(PhraseTestStat.PhraseIdU) &&
				    !((Test) entity).Status.IsPhrase(PhraseTestStat.PhraseIdP) &&
				    !entity.IsNew())
				{
					return false;
				}
			}

			return true;
		}

		#endregion

		#region Save & Refresh

		/// <summary>
		/// Called when an exception occurs during a save operation.
		/// </summary>
		/// <param name="e">The exception</param>
		/// <returns></returns>
		protected override bool OnSaveException(Exception e)
		{
			ClearBusy();
			return base.OnSaveException(e);
		}

		/// <summary>
		/// Handles the BeforeAction event of the ActionButtonApply control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ControlCancelEventArgs"/> instance containing the event data.</param>
		private void ActionButtonApply_BeforeAction(object sender, ControlCancelEventArgs e)
		{
			m_ApplyClicked = true;

			ThreadPool.QueueUserWorkItem(state =>
			{
				try
				{
					ProcessActionButton(null);
				}
				catch (Exception ex)
				{
					OnException(ex);
				}
			});

			e.Cancel = true;
		}

		/// <summary>
		/// Handles the BeforeAction event of the ActionButtonOK control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ControlCancelEventArgs"/> instance containing the event data.</param>
		private void ActionButtonOK_BeforeAction(object sender, ControlCancelEventArgs e)
		{
			m_OkClicked = true;

			ThreadPool.QueueUserWorkItem(state =>
			{
				try
				{
					ProcessActionButton(null);
				}
				catch (Exception ex)
				{
					OnException(ex);
				}
			});

			e.Cancel = true;
		}

		/// <summary>
		/// Processes the action button.
		/// </summary>
		/// <param name="state">The state.</param>
		private void ProcessActionButton(object state)
		{
			SetBusy("SampleLoginSaveDesc1", "SampleLoginSaveCaption");

			if (SaveAndValidateData())
			{
				bool ok = PreCommit();

				// Process the saved data

				if (ok)
				{
					if (Library.Task.Modified)
					{
						// Commit the Changes

						if (!Context.UsingSurrogateEntityManager)
						{
                            // B.Smock.01 - avoid increment error
                            foreach (JobHeader j in m_NewJobs)
                            {
                                j.AssignDefaultValues();
                            }
                            foreach (Sample s in m_NewSamples)
                            {
                                s.AssignDefaultValues();
                            }
                            foreach (Test t in m_NewTests)
                            {
                                t.AssignDefaultValues();
                            }
                            ReleaseLocks();
                            // End modification
                            
                            EntityManager.Commit();
							PostCommit();
						}
					}

					if (m_OkClicked)
					{
						ClearBusy();
						m_Form.Close();
						return;
					}
				}
			}

			ClearBusy();
		}

		/// <summary>
		/// Called before committing the data
		/// </summary>
		/// <returns></returns>
		private bool PreCommit()
		{
			m_Saving = true;

			// Update Test Assignment

			SetBusy("SampleLoginSaveDesc2", "SampleLoginSaveCaption");

			UpdateAssignedTests();

			// Running Workflow

			WorkflowErrorCollection errors = TriggerPostEditWorkfow();

			if (errors.Count > 0)
			{
				ClearBusy();
				WorkflowError error = errors[0];
				Library.Utils.FlashMessage(error.Message, m_Form.Title);
				return false;
			}

			// Everything is Valid, add modified stuff to transaction

			foreach (IEntity topLevelEntity in m_TopLevelEntities)
			{
				if (topLevelEntity.IsModified() || topLevelEntity.IsDeleted())
				{
					EntityManager.Transaction.Add(topLevelEntity);
					Library.Task.StateModified();
				}
			}

			// Keep track of new things - Post Commit will spin through the new items

			TrackNewItems();

			SetBusy("SampleLoginSaveDesc3", "SampleLoginSaveCaption");

			m_Saving = false;

			return true;
		}

		/// <summary>
		/// Saves the and validate data.
		/// </summary>
		/// <returns></returns>
		private bool SaveAndValidateData()
		{
			SaveClientData();
			return ValidateEntities(m_TopLevelEntities);
		}

		/// <summary>
		/// Called after the data is committed
		/// </summary>
		private void PostCommit()
		{
			SetBusy("SampleLoginSaveDesc4", "SampleLoginSaveCaption");
			PostCommit(null);
		}

		/// <summary>
		/// Post Save Workflow - Closes the form after completion if OK Pressed.
		/// </summary>
		/// <param name="state">The state.</param>
		private void PostCommit(object state)
		{
			// Trigger Workflow

			WorkflowErrorCollection errors = TriggerPostLoginWorkflow();

			// Handle Errors

			if (errors.Count > 0)
			{
				ClearBusy();
				WorkflowError error = errors[0];
				Library.Utils.FlashMessage(error.Message, m_Form.Title);
				return;
			}

			SetBusy("SampleLoginSaveDesc5", "SampleLoginSaveCaption");

			if (m_ApplyClicked)
			{
				// Re-lock the entities

				LockAllEntities();

				// Multiple trigger may have updated jobs or samples so refresh grids
				// Reset apply button so data from the grid does not update the entity

				if (IsJobWorkflow)
				{
					RefreshJobsGrid();
				}

				RefreshTestsGrid();
				RefreshSamplesGrid();
				RefreshTestAssignmentData();

				// Refresh after Apply

				m_RootNode.Nodes.Clear();
				InitialiseTree();

				m_ApplyClicked = false;
				ClearBusy();

				Library.Task.StateModified(false);

				return;
			}

			ClearBusy();
			m_Form.Close();
		}

		#endregion

		#region Tracking New Items

		/// <summary>
		/// Tracks the new items.
		/// </summary>
		private void TrackNewItems()
		{
			m_NewJobs = EntityManager.CreateEntityCollection(JobHeaderBase.EntityName);
			m_NewSamples = EntityManager.CreateEntityCollection(SampleBase.EntityName);
			m_NewTests = EntityManager.CreateEntityCollection(TestBase.EntityName);

			if (IsJobWorkflow)
			{
				foreach (JobHeaderBase job in m_TopLevelEntities)
				{
					TrackNewItems(job);
				}

				return;
			}

			foreach (Sample sample in m_TopLevelEntities)
			{
				TrackNewItems(sample);
			}
		}

		/// <summary>
		/// Tracks the new items.
		/// </summary>
		/// <param name="job">The job.</param>
		private void TrackNewItems(JobHeaderBase job)
		{
			if (job.IsNew()) m_NewJobs.Add(job);
			foreach (SampleBase sample in job.Samples)
			{
				TrackNewItems(sample);
			}
		}

		/// <summary>
		/// Tracks the new items.
		/// </summary>
		/// <param name="sample">The sample.</param>
		private void TrackNewItems(SampleBase sample)
		{
			if (sample.IsNew()) m_NewSamples.Add(sample);
			foreach (TestBase test in sample.Tests)
			{
				if (test.IsNew()) m_NewTests.Add(test);
			}
		}

		#endregion

		#region Test Update

		/// <summary>
		/// Updates the test assignment.
		/// </summary>
		private void UpdateAssignedTests()
		{
			// Pesky Job mode

			if (IsJobWorkflow)
			{
				foreach (JobHeaderBase job in m_TopLevelEntities)
				{
					UpdateAssignedTests(job.Samples);
				}

				return;
			}

			// Spin through the samples and remove any unwanted tests

			UpdateAssignedTests(m_TopLevelEntities);
		}

		/// <summary>
		/// Updates the test assignment - removes and that have been flagged as not removed.
		/// </summary>
		/// <param name="samples">The samples.</param>
		private void UpdateAssignedTests(IEntityCollection samples)
		{
			foreach (Sample sample in samples)
			{
				IList<Test> remove = new List<Test>();
				foreach (Test test in sample.Tests)
				{
					if (test.Assign) continue;
					remove.Add(test);
				}

				foreach (Test test in remove)
				{
					sample.Tests.Remove(test);

					if (!test.IsNew()) test.TriggerDeleted();

					EntityManager.Transaction.Remove(test);
				}

				// Remove redundant tests from formulations

				UpdateAssignedTests(sample.ChildSamples);
			}
		}

		#endregion

		#region Form Closing

		/// <summary>
		/// Handles the Closing event of the m_Form control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			bool cancelledOut = false;

			if (!IsTaskReadReadonly)
			{
				if (!m_FatalErrorOccured && m_Form.ActionButtonApply.Enabled && !m_OkClicked)
				{
					string message = Library.Message.GetMessage("GeneralMessages", "DataSaveConfirmation");
					cancelledOut = Library.Utils.FlashMessageYesNo(message, m_Title);
					e.Cancel = !cancelledOut;
				}

				// Release entity locks

				ReleaseLocks();
			}

			// Store grid Horz / Vert layouts

			SaveGridLayout(m_Form.GridJobProperties);
			SaveGridLayout(m_Form.GridSampleProperties);
			SaveGridLayout(m_Form.GridTestProperties);

			// Report back cancelled if appropriate.

			if (cancelledOut && OneShotMode)
			{
				var bag = ((IWorkflowDefinition) DefaultWorkflow).Properties;

				if (bag != null)
				{
					string message = Library.Message.GetMessage("WorkflowMessages", "ErrorLoginCancelled");
					WorkflowError error = new WorkflowError(message);
					bag.AddWorkflowError(error);
				}
			}
		}

		/// <summary>
		/// Save data.
		/// </summary>
		private void SaveClientData()
		{
			// Store client data into entities

			if (IsJobWorkflow)
			{
				StoreGridDataInEntities(m_Form.GridJobProperties);
			}

			StoreGridDataInEntities(m_Form.GridSampleProperties);
			StoreGridDataInEntities(m_Form.GridTestProperties);
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Handles the Click event of the m_DeleteButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void DeleteButton_Click(object sender, EventArgs e)
		{
			var focusedNode = m_Form.TreeListItems.FocusedNode;

			if (focusedNode == null || focusedNode == m_RootNode)
			{
				// Nothing is selected, exit

				return;
			}

			// Delete the focused tree node

			IEntity focusedEntity = (IEntity) focusedNode.Data;
			string messageProperty = focusedEntity.EntityType == JobHeader.EntityName ? JobHeaderPropertyNames.JobName : SamplePropertyNames.IdText;

			string message = Library.Message.GetMessage("GeneralMessages", "SampleAdminDeleteConfirmation");
			message = string.Format(message, focusedEntity.GetString(messageProperty));

			bool proceed = Library.Utils.FlashMessageYesNo(message, m_Title, MessageIcon.Question);

			if (!proceed)
			{
				// User has cancelled the deletion

				return;
			}

			// Get rid of the deleted node

			m_Form.TreeListItems.RemoveNode(focusedNode);

			m_TestAssignmentInitialised = false;

			// Refresh after delete

			if (m_Saving)
			{
				return;
			}

			// Make sure this is a valid existing entity

			if (!BaseEntity.IsValid(focusedEntity)) return;

			// Search for entity in children
			// This will call Delete on the entity

			if (!RemovedChild(m_TopLevelEntities, focusedEntity))
			{
				// Delete the Entity manually

				EntityManager.Delete(focusedEntity);
			}

			// Refresh UI

			RefreshVisibleGrid();

			// When deleting items it is easier to re-build the test assignment grid completely rather than try to work out which columns & rows to remove

			if (m_TestAssignmentInitialised)
			{
				RebuildTestAssignmentGrid();
			}
		}

		/// <summary>
		/// Removed child in tree
		/// </summary>
		/// <param name="entities"></param>
		/// <param name="entityToRemove"></param>
		/// <returns></returns>
		private bool RemovedChild(IEntityCollection entities, IEntity entityToRemove)
		{
			bool entityRemoved = false;

			if (entities.Contains(entityToRemove))
			{
				entities.Remove(entityToRemove);
			}

			foreach (IEntity entity in entities)
			{
				Sample sample = entity as Sample;

				if (sample != null)
				{
					entityRemoved = RemovedChild(sample.ChildSamples, entityToRemove);
					continue;
				}

				JobHeader job = entity as JobHeader;

				if (job != null)
				{
					entityRemoved = RemovedChild(job.Samples, entityToRemove);
				}
			}

			return entityRemoved;
		}

		/// <summary>
		/// Handles the Click event of the m_AddJobToolBarButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void AddJobToolBarButton_Click(object sender, EventArgs e)
		{
			m_ContextMenuNode = null;
			AddNewWorkflow(PhraseWflowType.PhraseIdJOB_HEADER);
		}

		/// <summary>
		/// Handles the Click event of the m_AddSampleToolBarButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void AddSampleToolBarButton_Click(object sender, EventArgs e)
		{
			m_ContextMenuNode = null;
			AddNewWorkflow(PhraseWflowType.PhraseIdSAMPLE);
		}

		/// <summary>
		/// Handles the Click event of the m_AddSampleToolBarButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void AddSubSampleToolBarButton_Click(object sender, EventArgs e)
		{
			m_SubSampling = true;
			m_ContextMenuNode = null;

			AddNewWorkflow(PhraseWflowType.PhraseIdSUBSAMPLE);
		}

		/// <summary>
		/// Handles the Click event of the m_AddTestScheduleToolBarButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void AddTestScheduleToolBarButton_Click(object sender, EventArgs e)
		{
			if (m_RootNodeSelected)
			{
				AddTestSchedule(m_TopLevelEntities);
			}
			else
			{
				IEntity focusedEntity = (IEntity) m_Form.TreeListItems.FocusedNode.Data;

				if (focusedEntity is JobHeader)
				{
					AddTestSchedule(((JobHeader) focusedEntity).RootSamples);
				}
				else if (focusedEntity is Sample)
				{
					AddTestSchedule((Sample) focusedEntity);
				}
			}
		}

		/// <summary>
		/// Adds the test schedule.
		/// </summary>
		/// <param name="sample">The sample.</param>
		private void AddTestSchedule(Sample sample)
		{
			IEntity schedule;
			if (!BaseEntity.IsValid(sample)) return;

			string caption = Library.Message.GetMessage("GeneralMessages", "SampleLoginAddTestScheduleCaption");
			string message = Library.Message.GetMessage("GeneralMessages", "SampleLoginAddTestScheduleMessage");

			if (Library.Utils.PromptForEntity(message, caption, TestSchedHeader.EntityName, out schedule) == FormResult.OK)
			{
				AddTestSchedule(sample, (TestSchedHeader) schedule);
			}
		}

		/// <summary>
		/// Adds the test schedule.
		/// </summary>
		/// <param name="sample">The sample.</param>
		/// <param name="schedule">The schedule.</param>
		private void AddTestSchedule(Sample sample, TestSchedHeader schedule)
		{
			// Add tests to sample

			sample.AddTests(schedule);

			// Update the screen.

			RebuildTestAssignmentGrid();
			RefreshVisibleGrid();

			Library.Task.StateModified();
		}

		/// <summary>
		/// Add test schedule to sample
		/// </summary>
		/// <param name="testschedule"></param>
		/// <param name="collection"></param>
		private void AddTestScheduleToSamples(IEntityCollection collection, TestSchedHeader testschedule)
		{
			// Only add schedule to relevant samples.

			foreach (Sample sample in collection)
			{
				if (CanAddTests(sample))
				{
					sample.AddTests(testschedule);
				}
			}

			// Update the screen.

			RebuildTestAssignmentGrid();
			RefreshVisibleGrid();

			Library.Task.StateModified();
		}

		/// <summary>
		/// Build a collection of samples then prompt for a test schedule.
		/// </summary>
		/// <param name="collection"></param>
		private void AddTestSchedule(IEntityCollection collection)
		{
			IEntity schedule;
			IEntityCollection allSamples = GetSamplesFromCollection(collection);
			if (allSamples != null && allSamples.Count > 0)
			{
				string caption = Library.Message.GetMessage("GeneralMessages", "SampleLoginAddTestScheduleCaption");
				string message = Library.Message.GetMessage("GeneralMessages", "SampleLoginAddTestScheduleMessage");

				if (Library.Utils.PromptForEntity(message, caption, TestSchedHeader.EntityName, out schedule) == FormResult.OK)
				{
					AddTestScheduleToSamples(allSamples, (TestSchedHeader) schedule);
				}
			}
		}

		/// <summary>
		/// Get samples from the passed in collection.
		/// </summary>
		/// <param name="collection"></param>
		/// <returns></returns>
		private IEntityCollection GetSamplesFromCollection(IEntityCollection collection)
		{
			IEntityCollection allSamples = EntityManager.CreateEntityCollection(Sample.EntityName);

			foreach (IEntity entity in collection)
			{
				if (entity is JobHeader)
				{
					foreach (Sample sample in ((JobHeader) entity).RootSamples)
					{
						allSamples.Add(sample);
					}
				}

				if (entity is Sample)
				{
					allSamples.Add(entity);
				}
			}
			return allSamples;
		}

		/// <summary>
		/// Handles the Click event of the m_AddAnalysisToolBarButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void AddAnalysisToolBarButton_Click(object sender, EventArgs e)
		{
			if (m_RootNodeSelected)
			{
				AddTest(m_TopLevelEntities);
			}
			else
			{
				IEntity focusedEntity = (IEntity) m_Form.TreeListItems.FocusedNode.Data;

				if (focusedEntity is JobHeader)
				{
					AddTest(((JobHeader) focusedEntity).RootSamples);
				}
				else if (focusedEntity is Sample)
				{
					AddTest((Sample) focusedEntity);
				}
			}
		}

		/// <summary>
		/// Adds the analysis.
		/// </summary>
		/// <param name="sample">The sample.</param>
		private void AddTest(Sample sample)
		{
			IEntity analysis;
			if (!BaseEntity.IsValid(sample)) return;

			string caption = Library.Message.GetMessage("GeneralMessages", "SampleLoginAddAnalysisCaption");
			string message = Library.Message.GetMessage("GeneralMessages", "SampleLoginAddAnalysisMessage");

			if (Library.Utils.PromptForEntity(message, caption, VersionedAnalysis.EntityName, out analysis) == FormResult.OK)
			{
				AddTest(sample, (VersionedAnalysis) analysis);
			}
		}

		/// <summary>
		/// Adds the test.
		/// </summary>
		/// <param name="sample">The sample.</param>
		/// <param name="analysis">The analysis.</param>
		private void AddTest(Sample sample, VersionedAnalysis analysis)
		{
			List<TestInternal> newTests = sample.AddTest(analysis);

			List<IEntity> newEntities = new List<IEntity>();
			foreach (TestInternal newTest in newTests)
			{
				newEntities.Add(newTest);
			}

			UpdateTestAssignmentGrid(Test.EntityName, newEntities);
			RefreshVisibleGrid();

			Library.Task.StateModified();
		}

		/// <summary>
		/// Adds the test.
		/// </summary>
		/// <param name="collection"></param>
		private void AddTest(IEntityCollection collection)
		{
			IEntity analysis;
			IEntityCollection allSamples = GetSamplesFromCollection(collection);

			if (allSamples != null && allSamples.Count > 0)
			{
				string caption = Library.Message.GetMessage("GeneralMessages", "SampleLoginAddAnalysisCaption");
				string message = Library.Message.GetMessage("GeneralMessages", "SampleLoginAddAnalysisMessage");
				if (Library.Utils.PromptForEntity(message, caption, VersionedAnalysis.EntityName, out analysis) == FormResult.OK)
				{
					List<IEntity> newEntities = new List<IEntity>();
					foreach (Sample sample in allSamples)
					{
						if (CanAddTests(sample))
						{
							List<TestInternal> newTests = sample.AddTest((VersionedAnalysis) analysis);
							foreach (TestInternal newTest in newTests)
							{
								newEntities.Add(newTest);
							}
						}
					}
					UpdateTestAssignmentGrid(Test.EntityName, newEntities);
					RefreshVisibleGrid();
					Library.Task.StateModified();
				}
			}
		}

		#endregion

		#region Fill Values

		/// <summary>
		/// Fills up left.
		/// </summary>
		private void FillUpLeft()
		{
			if (m_VisibleGrid.FocusedColumn.Name == AssignColumn)
			{

			}
			m_VisibleGrid.FillUp();
		}

		/// <summary>
		/// Fills down right.
		/// </summary>
		private void FillDownRight()
		{
			m_VisibleGrid.FillDown();
		}


		/// <summary>
		/// Fills all.
		/// </summary>
		private void FillAll()
		{
			m_VisibleGrid.FillAll();
		}

		#endregion

		#region Test Assignment

		/// <summary>
		/// Handles the ValueChanged event of the AssignColumn control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.UnboundGridValueChangedEventArgs"/> instance containing the event data.</param>
		private void AssignColumn_ValueChanged(object sender, UnboundGridValueChangedEventArgs e)
		{
			Test test = (Test) e.Row.Tag;
			test.Assign = (bool) e.Value;
			EnableDisableAssignCells();
			m_TestAssignmentInitialised = false;
			m_Form.GridTestProperties.SetFocusedRow(e.Row);
		}

		/// <summary>
		/// Enable and Disable Assigned Cells. Rules are as follows the last replicate is always shown and enabled.
		/// If you have multiple replicates you can only unassign in order so if you unassign the last 2 replicates
		/// the last replicate cell will be disabled and contents hidden so it can not be assigned.
		/// 
		/// This needs to work with multiple analysis and analysis can be inserted in any spots so to check all the
		/// grid when checking for unassigned and assigned replicates.
		/// </summary>
		private void EnableDisableAssignCells()
		{
			m_Form.GridTestProperties.BeginUpdate();

			for (int i = 0;
				i < m_Form.GridTestProperties.Rows.Count;
				i++)
			{
				// Get this and the next row
				UnboundGridRow row = m_Form.GridTestProperties.Rows[i];
				Test test = (Test) row.Tag;

				Sample sample = (Sample) test.Sample;

				// This is an unassigned test, disable the cell if it has a previous replicate that is not assigned or if the parent sample doesn't allow tests to be added
				TestInternal previousReplicate = sample.GetTest((VersionedAnalysisInternal) test.Analysis, test.TestCount - 1);

				if (test.TestCount > 1 && previousReplicate != null && !previousReplicate.Assign)
				{
					// Disable this cell and hide the tick box because it's previous replicate is not assigned
					m_AssignColumn.DisableCell(row, DisabledCellDisplayMode.GreyHideContents);
				}
				else
				{
					// Get the next test in the grid, if it is the same as this one and it's replicate number is greater than one, disable this cell
					Test nextTest = (Test) sample.GetTest((VersionedAnalysisInternal) test.Analysis, test.TestCount + 1);
					bool hasAssignedFollowers = nextTest != null && nextTest.Assign && nextTest.TestCount > 1;

					if (hasAssignedFollowers)
					{
						// This test has following replicates which are assigned so disable the cell
						m_AssignColumn.DisableCell(row, DisabledCellDisplayMode.GreyShowContents);
					}
					else
					{
						// This cell needs to be active if it's workflow node deems that it can be

						bool canRemoveTest = CanRemoveTest(test);

						if (canRemoveTest)
						{
							m_AssignColumn.EnableCell(row);
						}
						else
						{
							m_AssignColumn.DisableCell(row, DisabledCellDisplayMode.GreyShowContents);
						}
					}
				}
			}

			m_Form.GridTestProperties.EndUpdate();
		}


		/// <summary>
		/// Determines whether this instance [can remove test] the specified test.
		/// </summary>
		/// <param name="test">The test.</param>
		/// <returns>
		/// 	<c>true</c> if this instance [can remove test] the specified test; otherwise, <c>false</c>.
		/// </returns>
		private static bool CanRemoveTest(TestInternal test)
		{
			if (!BaseEntity.IsValid(test))
			{
				// Cannot remove committed tests

				return false;
			}

			// Make sure this test has not got more replicates that are assigned
			TestInternal nextTest = ((SampleInternal) test.Sample).GetTest((VersionedAnalysisInternal) test.Analysis, test.TestCount + 1);
			if (BaseEntity.IsValid(nextTest) && nextTest.Assign)
			{
				// This test has subsequent replicates that are assigned, it cannot be removed
				return false;
			}

			var allowRemoveTestStatuses = new List<string>() {PhraseTestStat.PhraseIdU, PhraseTestStat.PhraseIdV, PhraseTestStat.PhraseIdW};

			if (!string.IsNullOrWhiteSpace(test.Status.PhraseId) && !allowRemoveTestStatuses.Contains(test.Status.PhraseId))
			{
				return false;
			}

			if (test.IsNew())
			{
				if (test.CreatorNode == null)
				{
					return true;
				}

				CreateTestNode createTestNode = ((WorkflowNode) test.CreatorNode).Node as CreateTestNode;
				if (createTestNode != null)
				{
					return createTestNode.AllowRemove;
				}

				CreateTestsNode createTestsNode = ((WorkflowNode) test.CreatorNode).Node as CreateTestsNode;
				if (createTestsNode != null)
				{
					return createTestsNode.AllowRemove;
				}
			}
			else
			{
				if (test.WorkflowNode == null)
				{
					return true;
				}

				var createTestNode = ((WorkflowNode) test.WorkflowNode).Node as CreateTestNode;
				if (createTestNode != null)
				{
					return createTestNode.AllowRemove;
				}

				var createTestsNode = ((WorkflowNode) test.WorkflowNode).Node as CreateTestsNode;
				if (createTestsNode != null)
				{
					return createTestsNode.AllowRemove;
				}

			}

			return true;
		}

		/// <summary>
		/// Determines whether this instance [can add test] the specified sample.
		/// </summary>
		/// <param name="sample">The sample.</param>
		/// <param name="test">The test.</param>
		/// <returns>
		/// 	<c>true</c> if this instance [can add test] the specified sample; otherwise, <c>false</c>.
		/// </returns>
		private static bool CanAssignTest(SampleInternal sample, TestInternal test)
		{
			// Does this sample allow tests to be added?
			CreateSampleNode createSampleNode = ((WorkflowNode) sample.GetWorkflowNode()).Node as CreateSampleNode;
			if (createSampleNode != null)
			{
				if (!createSampleNode.AllowNewTests)
				{
					// Sample doesn't allow new tests to be added to it, check to see if this test was previously part of this sample?
					if (!sample.Tests.Contains(test))
					{
						// This test has never been on the sample so it cannot be added
						return false;
					}
				}
			}

			return ValidStatusForModify(sample);
		}

		/// <summary>
		/// Adds the test assignment jobs.
		/// </summary>
		/// <param name="jobs">The jobs.</param>
		private void AddTestAssignmentJobs(IList jobs)
		{
			foreach (JobHeader job in jobs)
			{
				AddTestAssignmentSamples(job.RootSamples.ActiveItems);
			}
		}

		/// <summary>
		/// Adds the test assignment samples.
		/// </summary>
		/// <param name="samples">The samples.</param>
		private void AddTestAssignmentSamples(IList samples)
		{
			foreach (Sample sample in samples)
			{
				AddTestAssignmentSample(sample);
			}
		}

		/// <summary>
		/// Adds the test assignment sample.
		/// </summary>
		/// <param name="sample">The sample.</param>
		private void AddTestAssignmentSample(Sample sample)
		{
			// Add tests

			AddTestAssignmentTests(sample.Tests.ActiveItems);

			// Add a new row for this sample

			UnboundGridRow newRow = m_Form.TestAssignmentGrid.AddRow();
			newRow.Tag = sample;

			// Add Test Assignment Data for Child Samples

			foreach (Sample subSample in sample.ChildSamples.ActiveItems)
			{
				AddTestAssignmentSample(subSample);
			}
		}

		/// <summary>
		/// Adds the test assignment tests.
		/// </summary>
		/// <param name="newTests">The new tests.</param>
		private void AddTestAssignmentTests(IList newTests)
		{
			// Check to see if this test is present in the grid

			foreach (Test test in newTests)
			{
				string columnName = GetTestAssignmentColumnName(test);

				if (m_Form.TestAssignmentGrid.GetColumnByName(columnName) == null)
				{
					// Get the replicate number for this test in the grid

					string columnCaption = test.TestCount == 1 ? test.Analysis.VersionedAnalysisName : string.Format("{0}/{1}", test.Analysis.VersionedAnalysisName, test.TestCount);

					// Add this test to the grid as a column

					UnboundGridColumn newColumn = m_Form.TestAssignmentGrid.AddColumn(columnName, columnCaption, GridColumnType.Boolean, "");
					newColumn.Tag = new object[] {test.Analysis, test.TestCount};
					newColumn.ValueChanged += TestAssignmentColumn_ValueChanged;
				}
			}

			// Analysis names are not unique, make the column name unique by appending the analysis ID to duplicates

			foreach (UnboundGridColumn column in m_Form.TestAssignmentGrid.Columns)
			{
				List<UnboundGridColumn> columns = GetTestAssignmentColumnsByCaption(column.Caption);

				if (columns.Count != 1)
				{
					// These are duplicates, append analysis id on all of them

					foreach (UnboundGridColumn duplicateColumn in columns)
					{
						VersionedAnalysis analysis = (VersionedAnalysis) ((object[]) duplicateColumn.Tag)[0];
						duplicateColumn.Caption = string.Format("{0} ({1})", duplicateColumn.Caption, analysis.Identity);
					}
				}
			}

			// Finally, sort the columns (i.e. the tests)

			//m_Form.TestAssignmentGrid.SortColumns();
		}

		/// <summary>
		/// Gets the test assignment columns by caption.
		/// </summary>
		/// <param name="caption">The caption.</param>
		/// <returns></returns>
		private List<UnboundGridColumn> GetTestAssignmentColumnsByCaption(string caption)
		{
			List<UnboundGridColumn> columns = new List<UnboundGridColumn>();

			foreach (UnboundGridColumn column in m_Form.TestAssignmentGrid.Columns)
			{
				if (column.Caption == caption)
				{
					columns.Add(column);
				}
			}

			return columns;
		}

		/// <summary>
		/// Gets the name of the test assignment column.
		/// </summary>
		/// <param name="test">The test.</param>
		/// <returns></returns>
		private static string GetTestAssignmentColumnName(TestInternal test)
		{
			return string.Format("{0}_{1}", test.Analysis.Identity, test.TestCount);
		}

		/// <summary>
		/// Initialises the test assignment grid.
		/// </summary>
		private void RefreshTestAssignmentData()
		{
			if (!m_TestAssignmentInitialised)
			{
				return;
			}

			m_Form.TestAssignmentGrid.BeginUpdate();
			foreach (UnboundGridRow row in m_Form.TestAssignmentGrid.Rows)
			{
				// Set fixed column values
				Sample sample = (Sample) row.Tag;
				row[TestAssignSampleColumnName] = sample.IdText;

				// Set Row Icon
				row.SetIcon(new IconName(((IEntity) sample).Icon));


				foreach (UnboundGridColumn column in m_Form.TestAssignmentGrid.Columns)
				{
					if (column.Name == TestAssignSampleColumnName)
					{
						// This is a static column displaying the sample id
						continue;
					}

					// Get analysis info for this column
					object[] columnTag = (object[]) column.Tag;
					VersionedAnalysisInternal analysis = (VersionedAnalysisInternal) columnTag[0];
					int testCount = (int) columnTag[1];

					// Is there a test on this sample for this analysis / replicate?
					TestInternal test = sample.GetTest(analysis, testCount);
					bool isSelected = BaseEntity.IsValid(test) && test.Assign;
					row[column] = isSelected;

					column.EnableCell(row);

					// Enable / disable the cell
					if (isSelected)
					{
						// If this test is committed, don't allow it to be removed

						bool canRemoveTest = CanRemoveTest(test);

						if (!test.IsNew() && canRemoveTest)
						{
							continue;
						}

						// This cell is enabled if it is un-committed, doesn't have a following replicate that is assigned and it's creator workflow allows it to be removed

						if (!canRemoveTest)
						{
							// This test cannot be removed
							column.DisableCell(row, DisabledCellDisplayMode.GreyShowContents);
							continue;
						}
					}
					else
					{
						// This is an unassigned test, disable the cell if it has a previous replicate that is not assigned or if the parent sample doesn't allow tests to be added
						TestInternal previousReplicate = sample.GetTest(analysis, testCount - 1);
						bool isReplicateWithoutPreviousAssigned = testCount > 1 ? !BaseEntity.IsValid(previousReplicate) || !previousReplicate.Assign : false;
						bool canAddTestToSample = CanAssignTest(sample, test);
						if (isReplicateWithoutPreviousAssigned || !canAddTestToSample)
						{
							// The test can't be assigned
							column.DisableCell(row, DisabledCellDisplayMode.GreyHideContents);
							continue;
						}
					}
				}


			}
			m_Form.TestAssignmentGrid.EndUpdate();
		}

		/// <summary>
		/// Updates the test assignment grid.
		/// </summary>
		/// <param name="entityType">Type of the entity.</param>
		/// <param name="newEntities">The new entities.</param>
		private void UpdateTestAssignmentGrid(string entityType, List<IEntity> newEntities)
		{
			if (!m_Loaded)
			{
				return;
			}

			try
			{
				m_Form.TestAssignmentGrid.BeginUpdate();

				switch (entityType)
				{
					case JobHeader.EntityName:

						AddTestAssignmentJobs(newEntities);
						break;

					case Sample.EntityName:

						AddTestAssignmentSamples(newEntities);
						break;

					case Test.EntityName:

						AddTestAssignmentTests(newEntities);
						break;
				}

				RefreshTestAssignmentData();
			}
			finally
			{
				m_Form.TestAssignmentGrid.EndUpdate();
			}
		}

		/// <summary>
		/// Handles the ValueChanged event of the newColumn control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.UnboundGridValueChangedEventArgs"/> instance containing the event data.</param>
		private void TestAssignmentColumn_ValueChanged(object sender, UnboundGridValueChangedEventArgs e)
		{
			bool isSelected = (bool) e.Value;

			// Get the sample from the row

			Sample sample = (Sample) e.Row.Tag;

			// Get the analysis from the Column

			object[] columnTag = (object[]) e.Column.Tag;
			VersionedAnalysis analysis = (VersionedAnalysis) columnTag[0];
			int testCount = (int) columnTag[1];

			if (isSelected)
			{
				// If the test exists just set the assign flag, otherwise create the new test

				TestInternal test = sample.GetTest(analysis, testCount);
				List<TestInternal> newTests = new List<TestInternal>();
				if (test == null)
				{
					// We may get more than one test based on the repeat count on the analysis

					newTests = sample.AddTest(analysis);
				}
				else
				{
					// Just process the existing test

					newTests.Add(test);
				}

				#region Comment

				try
				{
					m_Form.TestAssignmentGrid.BeginUpdate();

					foreach (TestInternal newTest in newTests)
					{
						newTest.Assign = true;

						// Make sure the cell is ticked

						string thisColumnName = string.Format("{0}_{1}", analysis.Identity, newTest.TestCount);
						UnboundGridColumn thisColumn = m_Form.TestAssignmentGrid.GetColumnByName(thisColumnName);
						e.Row[thisColumn] = true;

						// If there are replicates, the next cell could be disabled, check to see if this should be re-enabled

						string nextColumnName = string.Format("{0}_{1}", analysis.Identity, newTest.TestCount + 1);
						UnboundGridColumn nextColumn = m_Form.TestAssignmentGrid.GetColumnByName(nextColumnName);
						if (nextColumn != null)
						{
							// Re-enable the cell if the sample allows tests to be added to it
							TestInternal nextTest = sample.GetTest(analysis, newTest.TestCount + 1);
							bool canAssignTest = CanAssignTest(sample, nextTest);
							if (canAssignTest)
							{
								nextColumn.EnableCell(e.Row);
							}
						}

						// Make the previous column readonly if neccessary
						string prevColumnName = string.Format("{0}_{1}", analysis.Identity, newTest.TestCount - 1);
						UnboundGridColumn prevColumn = m_Form.TestAssignmentGrid.GetColumnByName(prevColumnName);
						if (prevColumn != null)
						{
							// Disable the previous cell

							prevColumn.DisableCell(e.Row, DisabledCellDisplayMode.GreyShowContents);
						}
					}
				}
				finally
				{
					m_Form.TestAssignmentGrid.EndUpdate();
				}

				#endregion
			}
			else
			{
				// Remove test from the sample

				TestInternal testToRemove = sample.GetTest(analysis, testCount);


				if (testToRemove != null)
				{
					testToRemove.Assign = false;

					// If there are replicates, the next cell might need to be disabled, check to see if this should be disabled

					string nextColumnName = string.Format("{0}_{1}", analysis.Identity, testToRemove.TestCount + 1);
					UnboundGridColumn nextColumn = m_Form.TestAssignmentGrid.GetColumnByName(nextColumnName);
					if (nextColumn != null)
					{
						// Disable the cell

						nextColumn.DisableCell(e.Row, DisabledCellDisplayMode.GreyHideContents);
					}

					// Enable the previous column if neccessary
					string prevColumnName = string.Format("{0}_{1}", analysis.Identity, testToRemove.TestCount - 1);
					UnboundGridColumn prevColumn = m_Form.TestAssignmentGrid.GetColumnByName(prevColumnName);
					if (prevColumn != null)
					{
						// Re-enable the previous cell only when it relates to an uncomitted test
						TestInternal previousTest = sample.GetTest(analysis, testCount - 1);

						bool canRemoveTest = CanRemoveTest(previousTest);
						if (canRemoveTest)
						{
							prevColumn.EnableCell(e.Row);
						}
					}
				}
			}
			Library.Task.StateModified();
		}

		/// <summary>
		/// Rebuilds the test assignment grid.
		/// </summary>
		private void RebuildTestAssignmentGrid()
		{
			try
			{
				m_Form.TestAssignmentGrid.BeginUpdate();
				m_Form.TestAssignmentGrid.ClearRows();

				// Remove all columns apart from the sample column

				while (m_Form.TestAssignmentGrid.Columns.Count > 1)
				{
					UnboundGridColumn columnToRemove = m_Form.TestAssignmentGrid.Columns[m_Form.TestAssignmentGrid.Columns.Count - 1];
					m_Form.TestAssignmentGrid.RemoveColumn(columnToRemove);
				}

				// Re-Build columns and rows

				if (IsJobWorkflow)
				{
					AddTestAssignmentJobs(m_TopLevelEntities);
				}
				else
				{
					AddTestAssignmentSamples(m_TopLevelEntities);
				}
			}
			finally
			{
				m_Form.TestAssignmentGrid.EndUpdate();
				m_TestAssignmentInitialised = true;

				RefreshTestAssignmentData();
			}
		}

		#endregion

		#region Workflow Events

		/// <summary>
		/// Triggers the post login workfow.
		/// </summary>
		/// <returns>Errors if found</returns>
		private WorkflowErrorCollection TriggerPostLoginWorkflow()
		{
			try
			{
				IWorkflowPropertyBag bag = m_WorkflowEventService.ProcessDeferredTriggers("POST_LOGIN");
				if (bag.Errors.Count == 0)
				{
					m_NewJobs = null;
					m_NewSamples = null;
					m_NewTests = null;
					EntityManager.Commit();
				}
				else
				{
					return bag.Errors;
				}
			}
			catch (Exception e)
			{
				var error = new WorkflowError(e.Message, e);
				var errors = new WorkflowErrorCollection();
				errors.Add(error);
				return errors;
			}

			return new WorkflowErrorCollection();
		}

		/// <summary>
		/// Triggers the post edit workfow.
		/// </summary>
		private WorkflowErrorCollection TriggerPostEditWorkfow()
		{
			try
			{
				foreach (IEntity top in m_TopLevelEntities)
				{
					Sample sample = top as Sample;

					if (sample != null)
					{
						var bag = SamplePostEditWorkflow(sample);

						if (sample.Tests != null)
						{
							foreach (Test test in sample.Tests)
							{
								if (test.IsNew())
								{
									m_WorkflowEventService.RegisterDeferredTrigger(test);
								}
							}
						}

						AllocChildSampleTriggers(sample);

						if (bag != null && bag.HasErrors) return bag.Errors;
						continue;
					}

					JobHeader job = top as JobHeader;

					if (job != null)
					{
						foreach (Sample jobSample in job.Samples)
						{
							foreach (Test test in jobSample.Tests)
							{
								if (test.IsNew())
								{
									m_WorkflowEventService.RegisterDeferredTrigger(test);
								}
							}
							AllocChildSampleTriggers(jobSample);
						}

						var bag = JobPostEditWorkflow(job);
						if (bag != null && bag.HasErrors) return bag.Errors;
					}
				}
			}
			catch (Exception e)
			{
				var error = new WorkflowError(e.Message, e);
				var errors = new WorkflowErrorCollection();
				errors.Add(error);
				return errors;
			}

			return new WorkflowErrorCollection();
		}

		/// <summary>
		/// Allocs the child sample triggers.
		/// </summary>
		/// <param name="sample">The sample.</param>
		private void AllocChildSampleTriggers(Sample sample)
		{
			if (sample.ChildSamples != null)
			{
				foreach (Sample childSample in sample.ChildSamples)
				{
					if (childSample.Tests != null)
					{
						foreach (Test test in childSample.Tests)
						{
							if (test.IsNew())
							{
								m_WorkflowEventService.RegisterDeferredTrigger(test);
							}
						}
					}
					if (childSample.ChildSamples != null) AllocChildSampleTriggers(childSample);
				}
			}
		}

		/// <summary>
		/// Post Edit Event - before commit
		/// </summary>
		/// <param name="job">The job.</param>
		private static IWorkflowPropertyBag JobPostEditWorkflow(JobHeader job)
		{
			foreach (Sample sample in job.Samples)
			{
				var bag = SamplePostEditWorkflow(sample);
				if (bag != null && bag.HasErrors) return bag;
			}

			return job.TriggerPostEdit();
		}

		/// <summary>
		/// Post Edit Event - before commit
		/// </summary>
		/// <param name="sample">The sample.</param>
		private static IWorkflowPropertyBag SamplePostEditWorkflow(Sample sample)
		{
			foreach (Test test in sample.Tests)
			{
				var bag = test.TriggerPostEdit();
				if (bag != null && bag.HasErrors) return bag;
			}

			foreach (Sample childSample in sample.ChildSamples)
			{
				var bag = childSample.TriggerPostEdit();
				if (bag != null && bag.HasErrors) return bag;
			}

			return sample.TriggerPostEdit();
		}

		#endregion

		#region Specific Grid Column Prompts

		/// <summary>
		/// Setup the grid column
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="templateProperty">The template property.</param>
		/// <param name="row">The row.</param>
		/// <param name="column">The column.</param>
		private void SetupGridColumn(IEntity entity, EntityTemplatePropertyInternal templateProperty, UnboundGridRow row, UnboundGridColumn column)
		{
			if (entity.EntityType == TestBase.EntityName)
			{
				SetupTestGridColumnInternal((Test) entity, templateProperty, row, column);
				SetupTestGridColumn((Test) entity, templateProperty, row, column);
				return;
			}

			if (entity.EntityType == JobHeaderBase.EntityName)
			{
				SetupJobGridColumn((JobHeader) entity, templateProperty, row, column);
				return;
			}

			if (entity.EntityType == SampleBase.EntityName)
			{
				SetupSampleGridColumnInternal(templateProperty, row, column);
				SetupSampleGridColumn((Sample) entity, templateProperty, row, column);
			}
		}

		/// <summary>
		/// Setup the sample grid column.
		/// </summary>
		/// <param name="templateProperty">The template property.</param>
		/// <param name="row">The row.</param>
		/// <param name="column">The column.</param>
		private static void SetupSampleGridColumnInternal(EntityTemplatePropertyInternal templateProperty, UnboundGridRow row, UnboundGridColumn column)
		{
			// Spreadsheet login does not allow you to change the test schedule during login.

			if (templateProperty.PropertyName == SamplePropertyNames.TestSchedule || templateProperty.PropertyName == SamplePropertyNames.JobName)
			{
				column.DisableCell(row, DisabledCellDisplayMode.GreyShowContents);
			}
		}

		/// <summary>
		/// Setup the test grid column.
		/// </summary>
		/// <param name="test">The test.</param>
		/// <param name="templateProperty">The template property.</param>
		/// <param name="row">The row.</param>
		/// <param name="column">The column.</param>
		private void SetupTestGridColumnInternal(Test test, EntityTemplatePropertyInternal templateProperty, UnboundGridRow row, UnboundGridColumn column)
		{
			// Instrument Type - keep track of it changing as it affect the Instrument Browse

			if (templateProperty.PropertyName == TestPropertyNames.InstrumentType)
			{
				//column.ValueChanged += InstrumentType_ValueChanged;
				return;
			}

			// Instruments - restrict to Instrument Type

			if (templateProperty.PropertyName == TestPropertyNames.Instrument)
			{
				if (test.InstrumentRequired)
				{
					column.SetCellMandatory(row);
				}

				return;
			}

			// Component List - Identities of the Available Component Lists

			if (templateProperty.PropertyName == TestPropertyNames.ComponentList)
			{
				if (test.IsNew())
				{
					StringBrowse browse = BrowseFactory.CreateStringBrowse(test.Analysis.CLHeaders, VersionedCLHeaderPropertyNames.CompList);
					column.SetCellBrowse(row, browse);
				}
				else
				{
					column.DisableCell(row, DisabledCellDisplayMode.GreyShowContents);
				}
			}
		}

		/// <summary>
		/// Setup the sample grid column.
		/// </summary>
		/// <param name="sample">The sample.</param>
		/// <param name="templateProperty">The template property.</param>
		/// <param name="row">The row.</param>
		/// <param name="column">The column.</param>
		protected virtual void SetupSampleGridColumn(Sample sample, EntityTemplatePropertyInternal templateProperty, UnboundGridRow row, UnboundGridColumn column)
		{
		}

		/// <summary>
		/// Setup the job grid column.
		/// </summary>
		/// <param name="sample">The sample.</param>
		/// <param name="templateProperty">The template property.</param>
		/// <param name="row">The row.</param>
		/// <param name="column">The column.</param>
		protected virtual void SetupJobGridColumn(JobHeader sample, EntityTemplatePropertyInternal templateProperty, UnboundGridRow row, UnboundGridColumn column)
		{
		}

		/// <summary>
		/// Setup the test grid column.
		/// </summary>
		/// <param name="test">The test.</param>
		/// <param name="templateProperty">The template property.</param>
		/// <param name="row">The row.</param>
		/// <param name="column">The column.</param>
		protected virtual void SetupTestGridColumn(Test test, EntityTemplatePropertyInternal templateProperty, UnboundGridRow row, UnboundGridColumn column)
		{
		}

		#endregion

		#region Grid Changed Events

		/// <summary>
		/// Called when [cell value changed].
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.UnboundGridValueChangedEventArgs"/> instance containing the event data.</param>
		private void OnCellValueChanged(object sender, UnboundGridValueChangedEventArgs e)
		{
			Library.Task.StateModified();

			if (e.Column.Tag == null)
			{
				return;
			}

			bool isfilterSource = (bool) e.Column.Tag;

			if (isfilterSource)
			{
				// This updated column's value is used to filter other cell browses in this row, update these browses

				IEntity rowEntity = (IEntity) e.Row.Tag;
				EntityTemplateBase template = GetTemplate(rowEntity);

				foreach (EntityTemplateProperty templateProperty in template.EntityTemplateProperties)
				{
					if (templateProperty.FilterBy == e.Column.Name)
					{
						UnboundGrid grid = (UnboundGrid) sender;

						UnboundGridColumn filteredColumn = grid.GetColumnByName(templateProperty.PropertyName);

						// Update the browse

						SetupFilterBy(templateProperty, e.Row, filteredColumn, e.Value);

						// This column is filtered by the updated column, reset it's value and browse

						e.Row[templateProperty.PropertyName] = null;
					}
				}
			}
		}

		#endregion

		#region Locking

		/// <summary>
		/// Locks all entities.
		/// </summary>
		/// <returns></returns>
		private void LockAllEntities()
		{
			string errorText;
			LockAllEntities(out errorText);
		}

		/// <summary>
		/// Locks the entities.
		/// </summary>
		private bool LockAllEntities(out string errorText)
		{
			StringBuilder lockMessages = new StringBuilder();

			if (IsJobWorkflow)
			{
				foreach (JobHeader job in m_TopLevelEntities.ActiveItems)
				{
					LockEntity(job, lockMessages);

					LockSamples(job.Samples, lockMessages);
				}
			}
			else
			{
				LockSamples(m_TopLevelEntities, lockMessages);
			}

			// Process locking errors

			if (lockMessages.Length != 0)
			{
				errorText = lockMessages.ToString();

				return false;
			}

			errorText = string.Empty;
			return true;
		}

		/// <summary>
		/// Locks the samples.
		/// </summary>
		/// <param name="samples">The samples.</param>
		/// <param name="lockMessages">The lock messages.</param>
		private void LockSamples(IEntityCollection samples, StringBuilder lockMessages)
		{
			foreach (Sample sample in samples)
			{
				LockEntity(sample, lockMessages);

				foreach (Test test in sample.Tests)
				{
					LockEntity(test, lockMessages);
				}

				LockSamples(sample.ChildSamples, lockMessages);
			}
		}

		/// <summary>
		/// Locks the entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="lockMessages">The lock messages.</param>
		private void LockEntity(IEntity entity, StringBuilder lockMessages)
		{
			if (entity.IsNew())
			{
				return;
			}

            // B.Smock.01 - Lock check
            if (entity.Locked)
            {
                return;
            }
            // End modification

			try
			{
				entity.LockOrThrow();

				EntityManager.Transaction.Add(entity);
			}
			catch (LockingEntityError e)
			{
				lockMessages.AppendLine(e.Message);
			}
		}

		/// <summary>
		/// Releases the locks.
		/// </summary>
		private void ReleaseLocks()
		{
			if (IsJobWorkflow)
			{
				foreach (JobHeader job in m_TopLevelEntities)
				{
					job.LockRelease();

					UnLockSamples(job.Samples);
				}
			}
			else
			{
				UnLockSamples(m_TopLevelEntities);
			}
		}

		/// <summary>
		/// Locks the samples.
		/// </summary>
		/// <param name="samples">The samples.</param>
		private static void UnLockSamples(IEntityCollection samples)
		{
			foreach (Sample sample in samples)
			{
				sample.LockRelease();

				foreach (Test test in sample.Tests)
				{
					test.LockRelease();
				}

				UnLockSamples(sample.ChildSamples);
			}
		}

		#endregion

		#region Display

		/// <summary>
		/// Determines whether the entity has an entity template.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns>
		/// 	<c>true</c> if has entity template; otherwise, <c>false</c>.
		/// </returns>
		protected bool HasEntityTemplate(IEntity entity)
		{
			EntityTemplate template = entity.GetEntity("ENTITY_TEMPLATE") as EntityTemplate;
			return BaseEntity.IsValid(template);
		}

		#endregion

		#region Thread Exception Handling

		/// <summary>
		/// Called when an exception is raised by a thread pool operation.
		/// </summary>
		/// <param name="e">The e.</param>
		protected void OnException(Exception e)
		{
			Logger.ErrorFormat("Unexpected exception from Sample Login Thread Pool - {0}", e.Message);
			Logger.Debug(e.Message, e);

			ClearBusy();

			string caption = Library.Message.GetMessage("LaboratoryMessages", "SampleLoginThreadExceptionCaption");
			Library.Utils.FlashMessage(e.Message, caption, MessageButtons.OK, MessageIcon.Error, MessageDefaultButton.Button1);
		}

		#endregion
	}
}
