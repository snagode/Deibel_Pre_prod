using System;
using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of the Master Menu LTE
	/// </summary>
	[SampleManagerTask("MasterMenuTask", "LABTABLE", "MASTER_MENU")]
	public class MasterMenuTask : GenericLabtableTask
	{
		#region Member Variables

		private SortedList<string, string> m_Contexts;
		private bool m_EntityNamesPublished;
		private MasterMenu m_MasterMenu;
		private FormMasterMenu m_MasterMenuForm;
		private RadioButton m_SelectedRadioButton;

		private const string WorkflowActionTaskString = "WorkflowActionTask";
		private const string WorkflowRunTaskString = "WorkflowRunTask";

		#endregion

		#region Overridden Methods

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_MasterMenuForm = (FormMasterMenu) MainForm;
			m_MasterMenu = (MasterMenu) MainForm.Entity;
		}

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{

			m_MasterMenu.PropertyChanged += MasterMenuPropertyChanged;

			m_MasterMenuForm.Vgl.CheckedChanged += ImplementationCheckedChanged;
			m_MasterMenuForm.DotNet.CheckedChanged += ImplementationCheckedChanged;
			m_MasterMenuForm.Designer.CheckedChanged += ImplementationCheckedChanged;
			m_MasterMenuForm.WorkflowAction.CheckedChanged += ImplementationCheckedChanged;
			m_MasterMenuForm.RunWorkflow.CheckedChanged += ImplementationCheckedChanged;

			m_MasterMenuForm.PromptActionType.EntityChanged += PromptWorkflowActionEntityChanged;
			m_MasterMenuForm.PromptState.EntityChanged += PromptWorkflowActionEntityChanged;
			m_MasterMenuForm.PromptWorkflowName.EntityChanged += PromptWorkflowEntityChanged;

			SetInitialImplementationType();
			SetMenuType();
			// Allow Generation

			m_MasterMenuForm.GenerateButton.Enabled = (Context.LaunchMode == AddOption || Context.LaunchMode == TestOption ||
			                                           Context.LaunchMode == CopyOption);
			m_MasterMenuForm.GenerateButton.Click += GenerateButtonClick;
			m_MasterMenuForm.WebEnabled.Enabled = m_MasterMenu.WebAvailable;
		}

		/// <summary>
		/// Called before the property sheet or wizard is saved.
		/// </summary>
		/// <returns>
		/// true to allow the save to continue, false to abort the save
		/// </returns>
		protected override bool OnPreSave()
		{
			UpdateImplementationType();

			if (m_MasterMenu.RoleEntries.ActiveCount == 0)
			{
				string message = Library.VGL.GetMessage("LTE_MASTER_MNU_NO_ROLES");

				if (!Library.Utils.FlashMessageYesNo(message, m_MasterMenuForm.Title, MessageIcon.Question))
				{
					// User elected to correct the record.

					return false;
				}
			}

			return base.OnPreSave();
		}

		#endregion

		#region Menu Number Generation

		/// <summary>
		/// Generate button click.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void GenerateButtonClick(object sender, EventArgs e)
		{
			GenerateProcedureNumber();
		}

		/// <summary>
		/// Generates the procedure number.
		/// </summary>
		private void GenerateProcedureNumber()
		{
			FormMasterMenuContext menuContext = FormFactory.CreateForm<FormMasterMenuContext>();

			menuContext.Closed += MenuContextClosed;
			menuContext.Loaded += MenuContextLoaded;

			menuContext.ShowDialog();
		}

		/// <summary>
		/// Menus the context loaded.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void MenuContextLoaded(object sender, EventArgs e)
		{
			FormMasterMenuContext menuContext = (FormMasterMenuContext) sender;
			GetContexts(menuContext);
		}

		/// <summary>
		/// Menu Context closed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void MenuContextClosed(object sender, EventArgs e)
		{
			FormMasterMenuContext menuContext = (FormMasterMenuContext) sender;

			if (menuContext.FormResult == FormResult.OK)
			{
				string context = m_Contexts[menuContext.Context.Text];
				GenerateProcedureNumber(context);
			}
		}

		/// <summary>
		/// Generates the procedure number.
		/// </summary>
		/// <param name="context">The context.</param>
		private void GenerateProcedureNumber(string context)
		{
			int nextProcedureNum = (int) Library.VGL.RunVGLRoutine("$lib_menu", "lib_menu_get_new_proc_num", context);
			m_MasterMenu.ProcedureNum = nextProcedureNum;
		}

		/// <summary>
		/// Loads the contexts.
		/// </summary>
		/// <param name="menuContext">The menu context.</param>
		private void LoadContexts(FormMasterMenuContext menuContext)
		{
			if (m_Contexts != null) return;

			m_Contexts = new SortedList<string, string>();

			m_Contexts.Add(menuContext.StringTable.CORE, "CORE");
			m_Contexts.Add(menuContext.StringTable.FORMS, "FORMS");
			m_Contexts.Add(menuContext.StringTable.CUSTOMISATION, "CUSTOMISATION");
			m_Contexts.Add(menuContext.StringTable.PRIVILEGE, "PRIVILEGE");
			m_Contexts.Add(menuContext.StringTable.EFMWATERMANAGEMENT, "EFM_WATER_MANAGEMENT");
			m_Contexts.Add(menuContext.StringTable.EFMBATCHTREES, "EFM_BATCH_TREES");
			m_Contexts.Add(menuContext.StringTable.EFMRESULTPIPE, "EFM_RESULT_PIPE");
			m_Contexts.Add(menuContext.StringTable.EFMSQC, "EFM_SQC");
			m_Contexts.Add(menuContext.StringTable.EFMSTABILITY, "EFM_STABILITY");
			m_Contexts.Add(menuContext.StringTable.EFMSMIDI, "EFM_SMIDI");
			m_Contexts.Add(menuContext.StringTable.IMPLEMENTATION, "IMPLEMENTATION");
		}

		/// <summary>
		/// Gets the contexts.
		/// </summary>
		/// <param name="menuContext">The menu context.</param>
		private void GetContexts(FormMasterMenuContext menuContext)
		{
			LoadContexts(menuContext);
			menuContext.Context.Browse = BrowseFactory.CreateStringBrowse(new List<string>());

			foreach (string label in m_Contexts.Keys)
				menuContext.Context.Browse.AddItem(label);

			menuContext.Context.Text = menuContext.StringTable.CUSTOMISATION;
		}

		#endregion

		#region Menu Type

		/// <summary>
		/// Sets the type of the menu.
		/// </summary>
		private void SetMenuType()
		{
			m_MasterMenuForm.AuxReport.Enabled = m_MasterMenu.Type.IsPhrase(PhraseMenuType.PhraseIdAUXILIARY);
			m_MasterMenuForm.TableName2.Enabled = m_MasterMenu.Type.IsPhrase(PhraseMenuType.PhraseIdAUXILIARY);

			m_MasterMenuForm.LimsmlEntity.Enabled = m_MasterMenu.Type.IsPhrase(PhraseMenuType.PhraseIdLIMSML);
			m_MasterMenuForm.LimsmlAction.Enabled = m_MasterMenu.Type.IsPhrase(PhraseMenuType.PhraseIdLIMSML);

			if (m_MasterMenu.Type.IsPhrase(PhraseMenuType.PhraseIdLIMSML))
				LoadEntityTypes();
		}

		/// <summary>
		/// Loads the entity types.
		/// </summary>
		private void LoadEntityTypes()
		{
			if (m_EntityNamesPublished) return;

			List<string> entities = new List<string>();
			IEntityCollection items = EntityManager.Select(LimsmlEntityActionBase.EntityName);

			foreach (LimsmlEntityAction item in items)
			{
				string entity = item.Entity;
				if (entities.Contains(entity)) continue;
				entities.Add(entity);
			}

			entities.Sort();
			m_MasterMenuForm.LimsmlEntity.Browse.Republish(entities);
			m_EntityNamesPublished = true;
		}

		/// <summary>
		/// Fires when a property changes on the underlying entity
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e">property event arguments</param>
		private void MasterMenuPropertyChanged(object sender, PropertyEventArgs e)
		{
			switch (e.PropertyName)
			{
				case MasterMenuPropertyNames.Type:
					SetMenuType();
					m_MasterMenu.LimsmlAction = string.Empty;
					m_MasterMenu.LimsmlEntity = string.Empty;
					m_MasterMenu.AuxReport = string.Empty;
					break;
			}
		}

		#endregion

		# region Implementation Type

		/// <summary>
		/// Implementation Changed
		/// </summary>
		private void SetInitialImplementationType()
		{
			if (m_MasterMenu.ImplementationType.PhraseId == PhraseImplType.PhraseIdDOTNET)
				m_MasterMenuForm.DotNet.Checked = true;

			else if (m_MasterMenu.ImplementationType.PhraseId == PhraseImplType.PhraseIdVGL)
				m_MasterMenuForm.Vgl.Checked = true;

			else
			{
				if (m_MasterMenu.TaskName == WorkflowActionTaskString)
				{
					m_MasterMenuForm.WorkflowAction.Checked = true;
					ActionPromptFromTaskParams();
				}

				else if (m_MasterMenu.TaskName == WorkflowRunTaskString)
				{
					m_MasterMenuForm.RunWorkflow.Checked = true;
					WorkflowPromptFromTaskParams();
				}

				else
					m_MasterMenuForm.Designer.Checked = true;
			}

			SetImplementationControls();
		}

		/// <summary>
		/// Implementation Changed
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.CheckedChangedEventArgs"/> instance containing the event data.</param>
		private void ImplementationCheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (sender is RadioButton)
			{
				if ((m_SelectedRadioButton != null) && (m_SelectedRadioButton.Name != ((RadioButton) sender).Name))
				{
					Library.Task.StateModified();
				}
				m_SelectedRadioButton = (RadioButton) sender;
			}
			SetImplementationControls();
		}

		/// <summary>
		/// Sets the implementation controls.
		/// </summary>
		private void SetImplementationControls()
		{
			m_MasterMenuForm.NetPanel.Enabled = m_MasterMenuForm.DotNet.Checked;
			m_MasterMenuForm.FormPanel.Enabled = m_MasterMenuForm.Designer.Checked;
			m_MasterMenuForm.VglPanel.Enabled = m_MasterMenuForm.Vgl.Checked;
			m_MasterMenuForm.WorkflowActionPanel.Enabled = m_MasterMenuForm.WorkflowAction.Checked;
			m_MasterMenuForm.RunWorkflowPanel.Enabled = m_MasterMenuForm.RunWorkflow.Checked;
		}

		/// <summary>
		/// PromptWorkflowEntityChanged
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PromptWorkflowEntityChanged(object sender, EntityChangedEventArgs e)
		{
			if (m_MasterMenuForm.RunWorkflow.Checked)
				TaskParamsFromWorkflowPrompt();
		}

		/// <summary>
		/// PromptWorkflowActionEntityChanged
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PromptWorkflowActionEntityChanged(object sender, EntityChangedEventArgs e)
		{
			if (m_MasterMenuForm.WorkflowAction.Checked)
				TaskParamsFromActionPrompt();
		}


		/// <summary>
		/// Update the taskname and implementation type according to the current radio button
		/// </summary>
		private void UpdateImplementationType()
		{
			if (m_MasterMenuForm.DotNet.Checked)
				m_MasterMenu.ImplementationType = (PhraseBase) EntityManager.SelectPhrase(PhraseImplType.Identity, PhraseImplType.PhraseIdDOTNET);

			else if (m_MasterMenuForm.Vgl.Checked)
				m_MasterMenu.ImplementationType = (PhraseBase) EntityManager.SelectPhrase(PhraseImplType.Identity, PhraseImplType.PhraseIdVGL);

			else
			{
				m_MasterMenu.ImplementationType = (PhraseBase) EntityManager.SelectPhrase(PhraseImplType.Identity, PhraseImplType.PhraseIdDESIGNER);

				if (m_MasterMenuForm.WorkflowAction.Checked)
				{
					m_MasterMenu.TaskName = WorkflowActionTaskString;

					TaskParamsFromActionPrompt();
				}
				else if (m_MasterMenuForm.RunWorkflow.Checked)
				{
					m_MasterMenu.TaskName = WorkflowRunTaskString;

					TaskParamsFromWorkflowPrompt();
				}
			}
		}

		/// <summary>
		/// Set the task parameters field from the specific workflow prompt
		/// </summary>
		private void TaskParamsFromWorkflowPrompt()
		{
			string taskParams = "";

			if ((m_MasterMenuForm.PromptWorkflowName.Entity != null) && (!m_MasterMenuForm.PromptWorkflowName.Entity.IsNull()))
			{
				taskParams = ((Workflow) m_MasterMenuForm.PromptWorkflowName.Entity).Name;
			}

			if (m_MasterMenu.TaskParameters != taskParams)
			{
				m_MasterMenu.TaskParameters = taskParams;
				Library.Task.StateModified();
			}
		}

		/// <summary>
		/// Set the workflow prompt from the task parameters field
		/// </summary>
		private void WorkflowPromptFromTaskParams()
		{
			if (!string.IsNullOrEmpty(m_MasterMenu.TaskParameters))
			{
				IQuery query = EntityManager.CreateQuery(TableNames.Workflow);

				query.AddEquals(WorkflowPropertyNames.WorkflowName, m_MasterMenu.TaskParameters);

				IEntityCollection workflows = EntityManager.Select(TableNames.Workflow, query);

				if (workflows.Count > 0)
					m_MasterMenuForm.PromptWorkflowName.Entity = workflows[0];
			}
		}

		/// <summary>
		/// Set the task parameter string from the currently entered values
		/// </summary>
		private void TaskParamsFromActionPrompt()
		{
			string taskParams = "";

			if ((m_MasterMenuForm.PromptActionType.Entity != null) && (!m_MasterMenuForm.PromptActionType.Entity.IsNull()))
			{
				taskParams = ((WorkflowActionType) m_MasterMenuForm.PromptActionType.Entity).Identity;
			}

			if ((m_MasterMenuForm.PromptState.Entity != null) && (!m_MasterMenuForm.PromptState.Entity.IsNull()))
			{
				taskParams += ",";
				taskParams += ((WorkflowState) m_MasterMenuForm.PromptState.Entity).Identity;
			}

			if (m_MasterMenu.TaskParameters != taskParams)
			{
				m_MasterMenu.TaskParameters = taskParams;
				Library.Task.StateModified();
			}
		}

		/// <summary>
		/// Set the workflow action prompts from the task parameter property
		/// </summary>
		private void ActionPromptFromTaskParams()
		{
			if (!string.IsNullOrEmpty(m_MasterMenu.TaskParameters))
			{
				string[] parameters = m_MasterMenu.TaskParameters.Split(new[] {','});

				// Locate the Action type

				IQuery query = EntityManager.CreateQuery(TableNames.WorkflowActionType);

				query.AddEquals(WorkflowActionTypePropertyNames.Identity, parameters[0]);

				IEntityCollection workflowActionTypes = EntityManager.Select(TableNames.WorkflowActionType, query);

				if (workflowActionTypes.Count > 0)
					m_MasterMenuForm.PromptActionType.Entity = workflowActionTypes[0];

				// Locate the Action type

				if (parameters.GetLength(0) > 1)
				{
					query = EntityManager.CreateQuery(TableNames.WorkflowState);

					query.AddEquals(WorkflowStatePropertyNames.Identity, parameters[1]);

					IEntityCollection workflowStates = EntityManager.Select(TableNames.WorkflowState, query);

					if (workflowStates.Count > 0)
						m_MasterMenuForm.PromptState.Entity = workflowStates[0];
				}
			}
		}

		#endregion
	}
}