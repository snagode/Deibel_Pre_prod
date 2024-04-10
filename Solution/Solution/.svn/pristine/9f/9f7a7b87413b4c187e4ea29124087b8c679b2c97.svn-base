using System.Collections.Generic;
using System.Linq;
using System.Text;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Workflow;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server.Workflow.Definition;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Workflow Test Harness Task
	/// </summary>
	[SampleManagerTask("WorkflowTestHarnessTask", "GENERAL", "WORKFLOW")]
	public class WorkflowTestHarnessTask : DefaultFormTask
	{
		#region Constants

		private const string EventGroup = "EventGroup";
		private const string StateGroup = "StateGroup";
		private const string ActionGroup = "ActionGroup";
		private const string WorkflowGroup = "WorkflowGroup";

		#endregion

		#region Member Variables

		private Workflow m_Workflow;
		private FormWorkflowTestHarness m_SpyForm;
		private IEntity m_CurrentEntity;
		private IEntityCollection m_WorkflowEntities;
		private static Logger m_Logger;
		private string m_Title;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_SpyForm = (FormWorkflowTestHarness)MainForm;
			m_Title = m_SpyForm.Title;
			m_Workflow = (Workflow)MainForm.Entity;

			SelectedWorkflowEntities(m_Workflow);
		}

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
		    if (m_Workflow != null)//if workflow is selected
		    {
		        m_SpyForm.Title = m_Title + " - " + m_Workflow.WorkflowName;

		        FillWorkflowGroup(m_Workflow);

		        StartLogger();

		        m_SpyForm.WorkflowToolBox.ItemClicked += WorkflowToolBoxItemClicked;

		        m_SpyForm.DebugError.CheckedChanged += LogLevelChanged;
		        m_SpyForm.DebugInfo.CheckedChanged += LogLevelChanged;
		        m_SpyForm.DebugWarn.CheckedChanged += LogLevelChanged;
		        m_SpyForm.DebugDebug.CheckedChanged += LogLevelChanged;

		        m_SpyForm.ButtonClear.Click += ButtonClearClick;
		        m_SpyForm.ButtonRefresh.Click += ButtonRefreshClick;

		        UpdateToolBox(m_Workflow, m_CurrentEntity);
		    }
		    else // if cancelled
		    {
		        this.Exit();
		    }
		}

		/// <summary>
		/// Called when the designer task exits.
		/// </summary>
		/// <remarks>
		/// Override this method to perform any tidy-up actions before the task exits.
		/// </remarks>
		protected override void TaskExited()
		{
			base.TaskExited();

			if (m_Workflow != null)
			{
				Library.EntityWatcher.StopWatcher(m_Workflow.TableName, WatchHandler);
			}

			if (m_Logger != null)
			{
				StopLogger();
			}
		}

		/// <summary>
		/// Selecteds the workflow entities.
		/// </summary>
		/// <param name="workflow">The workflow.</param>
		private void SelectedWorkflowEntities(Workflow workflow)
		{
			if (m_Workflow == null) return;

			// Drop out if this is a general/no table name workflow.

			if (string.IsNullOrEmpty(workflow.TableName))
			{
				m_WorkflowEntities = EntityManager.CreateEntityCollection(WorkflowBase.EntityName);
				m_SpyForm.WorkflowEntityBrowse.Republish(m_WorkflowEntities);
				return;
			}

			m_WorkflowEntities = EntityManager.CreateEntityCollection(workflow.TableName);

			// Add the current entity

			if (m_CurrentEntity != null)
			{
				m_WorkflowEntities.Add(m_CurrentEntity);
			}

			// Publish and Watch for Changes.

			m_SpyForm.WorkflowEntityBrowse.Republish(m_WorkflowEntities);
			m_SpyForm.WorkflowEntityGrid.DataLoaded += WorkflowEntityGridDataLoaded;
			m_SpyForm.WorkflowEntityGrid.SelectionChanged += WorkflowEntityGridSelectionChanged;
			Library.EntityWatcher.StartWatcher(workflow.TableName, WatchHandler);
		}

		/// <summary>
		/// Get hold of the entity from the context
		/// </summary>
		/// <returns></returns>
		protected override IEntity GetEntity()
		{
			IEntity entity;

			if (Context.SelectedItems.Count > 0)
			{
				entity = Context.SelectedItems[0];

				if (entity.EntityType != WorkflowBase.EntityName)
				{
					m_CurrentEntity = entity;
					var node = (WorkflowNodeInternal)entity.GetWorkflowNode();
					entity = node.Workflow;
				}
			}
			else
			{
				// An entity has not been selected then prompt for a workflow

				FormResult result = Library.Utils.PromptForEntity(Library.Message.GetMessage("GeneralMessages", "FindEntity"),
				                                                  Context.MenuItem.Description, WorkflowBase.EntityName, out entity);

				if (result == FormResult.OK)
				{
					Context.SelectedItems.Add(entity);
				}
				else
				{
					entity = null;
				}
			}

			return entity;
		}

		#endregion

		#region Entity Event Handler

		private void WatchHandler(object sender, EntityWatcherEventArgs e)
		{
			EntityManager.SetEntityCacheAsOutOfDate();

			foreach (var identity in e.Inserted)
			{
				m_CurrentEntity = EntityManager.Select(m_Workflow.TableName, identity);
				m_WorkflowEntities.Insert(0, m_CurrentEntity);			
			}

			foreach (var identity in e.Modified)
			{
				m_CurrentEntity = EntityManager.Select(m_Workflow.TableName, identity);
			}

			m_SpyForm.WorkflowEntityBrowse.Republish(m_WorkflowEntities);
			AddTraceMessages();
		}

		void WorkflowEntityGridDataLoaded(object sender, System.EventArgs e)
		{
			if (m_CurrentEntity == null) return;
			m_SpyForm.WorkflowEntityGrid.SetSelectedItem(m_CurrentEntity);
			UpdateToolBox(m_Workflow, m_CurrentEntity);
		}

		private void WorkflowEntityGridSelectionChanged(object sender, ExplorerGridSelectionChangedEventArgs e)
		{
			if (e.Selection.Count != 1) return;
			if (m_CurrentEntity.Equals(e.Selection[0])) return;

			m_CurrentEntity = e.Selection[0];

			UpdateToolBox(m_Workflow, m_CurrentEntity);
		}

		#endregion

		#region Update ToolBox Actions, Events and States

		/// <summary>
		/// Handles the ToolBoxItemClicked event of the ToolBoxRun control
		/// </summary>
		/// <param name="sender">The sender</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ToolBoxItemClickedEventArgs"/> instance containing the event data.</param>
		private void WorkflowToolBoxItemClicked(object sender, ToolBoxItemClickedEventArgs e)
		{
			if (e.Item == null) return;
			if (e.Item.Group == null) return;

			if (e.Item.Group.Name == WorkflowGroup)
			{
				Library.Workflow.PerformTask(m_Workflow);
				AddTraceMessages();
				return;
			}

			if (e.Item.Group.Name == ActionGroup)
			{
				WorkflowActionType action = (WorkflowActionType)e.Item.Data;
				Library.Workflow.PerformActionTask(m_CurrentEntity, action);
				AddTraceMessages(); 
				return;
			}

			if (e.Item.Group.Name == EventGroup)
			{
				WorkflowEventType eventType = (WorkflowEventType)e.Item.Data;
				IWorkflowPropertyBag bag = Library.Workflow.TriggerEvent(m_CurrentEntity, eventType);
				CommitWorkflowUpdates(bag);
				return;
			}
		}

		/// <summary>
		/// Commits the workflow updates.
		/// </summary>
		/// <param name="bag">The bag.</param>
		private void CommitWorkflowUpdates(IWorkflowPropertyBag bag)
		{
			if (bag != null && bag.Errors.Count == 0)
			{
				EntityManager.Commit();
			}

			AddTraceMessages();
		}

		/// <summary>
		/// Updates the tool box.
		/// </summary>
		/// <param name="workflow">The workflow.</param>
		/// <param name="entity">The entity.</param>
		private void UpdateToolBox(Workflow workflow, IEntity entity)
		{
			m_SpyForm.WorkflowToolBox.BeginUpdate();
			m_SpyForm.StateToolBox.BeginUpdate();

			SetValidActions(workflow, entity);
			SetValidStates(workflow, entity);
			SetValidEvents(workflow, entity);

			m_SpyForm.WorkflowToolBox.EndUpdate();
			m_SpyForm.StateToolBox.EndUpdate();
		}

		/// <summary>
		/// Sets the valid events.
		/// </summary>
		/// <param name="workflow">The workflow.</param>
		/// <param name="entity">The entity.</param>
		private void SetValidEvents(Workflow workflow, IEntity entity)
		{
			ToolBoxGroup eventsGroup = m_SpyForm.WorkflowToolBox.FindGroupByName(EventGroup);
			m_SpyForm.WorkflowToolBox.ClearGroup(eventsGroup);

			IList<IEntity> allEvents = workflow.GetAllWorkflowEvents();

			if (entity != null)
			{
				IWorkflowNodeDefinition node = entity.GetWorkflowNode();
				allEvents = ((WorkflowNode)node).GetAllWorkflowEvents();
			}

			foreach (WorkflowEventType eventType in allEvents)
			{
				if (string.IsNullOrEmpty(eventType.Identity)) continue;
				string caption = eventType.WorkflowEventTypeName;
				ToolBoxItem item = m_SpyForm.WorkflowToolBox.AddItem(eventsGroup, eventType.Identity, caption, new IconName("FLASH"), null, eventType);
				item.Enabled = (entity != null);
			}
		}

		/// <summary>
		/// Sets the valid states.
		/// </summary>
		/// <param name="workflow">The workflow.</param>
		/// <param name="entity">The entity.</param>
		private void SetValidStates(Workflow workflow, IEntity entity)
		{
			ToolBoxGroup stateGroup = m_SpyForm.StateToolBox.FindGroupByName(StateGroup);
			m_SpyForm.StateToolBox.ClearGroup(stateGroup);

			IQuery query = EntityManager.CreateQuery(TableNames.WorkflowState);
			query.AddEquals(WorkflowStatePropertyNames.TableName, (entity == null) ? workflow.TableName : entity.EntityType);

			IEntityCollection states = EntityManager.Select(TableNames.WorkflowState, query);

			foreach (WorkflowState state in states)
			{
				ToolBoxItem item = m_SpyForm.StateToolBox.AddItem(stateGroup, state.Identity, state.WorkflowStateName, new IconName(state.IconId), null, state);
				item.Enabled = entity != null && state.Matches(entity);
			}
		}

		/// <summary>
		/// Sets the valid actions.
		/// </summary>
		/// <param name="workflow">The workflow.</param>
		/// <param name="entity">The entity.</param>
		private void SetValidActions(Workflow workflow, IEntity entity)
		{
			ToolBoxGroup actionGroup = m_SpyForm.WorkflowToolBox.FindGroupByName(ActionGroup);
			m_SpyForm.WorkflowToolBox.ClearGroup(actionGroup);

			IList<IEntity> allActions = workflow.GetAllWorkflowActions();
			IEnumerable<IEntity> actions = new List<IEntity>();

			if (entity != null)
			{
				IWorkflowNodeDefinition node = entity.GetWorkflowNode();
				allActions = ((WorkflowNode)node).GetAllWorkflowActions();
				actions = entity.GetActions();
			}

			foreach (WorkflowActionType action in allActions)
			{
				// Discard the empty workflow action types 
				if (string.IsNullOrEmpty(action.Identity)) continue;

				string caption = string.IsNullOrWhiteSpace(action.MenuText) ? action.WorkflowActionTypeName : action.MenuText;
				string iconName = string.IsNullOrWhiteSpace(action.IconId) ? action.Icon.Identity : action.IconId;

				ToolBoxItem item = m_SpyForm.WorkflowToolBox.AddItem(actionGroup, action.Identity, caption, new IconName(iconName), null, action);
				item.Enabled = (entity != null) && actions.Contains(action);
			}
		}

		/// <summary>
		/// Fill the Workflow Toolbox Group
		/// </summary>
		/// <param name="workflow"></param>
		private void FillWorkflowGroup(Workflow workflow)
		{
			if (workflow == null) return;

			const string defaultName = "Run Workflow";
			const string defaultIcon = "TEXT_TREE";

			ToolBoxGroup group = m_SpyForm.WorkflowToolBox.FindGroupByName(WorkflowGroup);

			string iconName = string.IsNullOrWhiteSpace(workflow.MenuIconId) ? defaultIcon : workflow.MenuIconId;
			string caption = string.IsNullOrWhiteSpace(workflow.MenuText) ? defaultName : workflow.MenuText;

			ToolBoxItem item = m_SpyForm.WorkflowToolBox.AddItem(group, defaultName, caption, new IconName(iconName), null, workflow);

			item.Enabled = (m_Workflow.WorkflowType.PhraseId == PhraseWflowType.PhraseIdENTITY) ||
						   (m_Workflow.WorkflowType.PhraseId == PhraseWflowType.PhraseIdGENERAL) ||
						   (m_Workflow.WorkflowType.PhraseId == PhraseWflowType.PhraseIdSAMPLE) ||
						   (m_Workflow.WorkflowType.PhraseId == PhraseWflowType.PhraseIdJOB_HEADER);
		}

		#endregion

		#region Trace Messages

		/// <summary>
		/// Starts the logger.
		/// </summary>
		private static void StartLogger()
		{
			m_Logger = Logger.GetInstance(typeof(Server.Workflow.Nodes.Node));
			m_Logger.MemoryStart(LoggerLevel.All);
			m_Logger.MemoryClear();
		}

		/// <summary>
		/// Stops the logger.
		/// </summary>
		private static void StopLogger()
		{
			m_Logger.MemoryStop();
		}

		/// <summary>
		/// Add trace messages
		/// </summary>
		private void AddTraceMessages()
		{
			StringBuilder builder = new StringBuilder();

			LoggerLevel logLevel = GetLogLevel();

			foreach (LoggerMessage message in m_Logger.MemoryCache.Where(m => m.Level <= logLevel))
			{
				builder.AppendLine(string.Format("{0:HH:mm:ss}\t{1}\t{2}", message.TimeStamp, message.Level, message.Message));
			}

			m_SpyForm.TraceComments.Text = builder.ToString();
		}

		/// <summary>
		/// Get the LoggerLevel from Buttons
		/// </summary>
		/// <returns></returns>
		private LoggerLevel GetLogLevel()
		{
			if (m_SpyForm.DebugInfo.Checked)
				return LoggerLevel.Info;
			if (m_SpyForm.DebugWarn.Checked)
				return LoggerLevel.Warn;
			if (m_SpyForm.DebugError.Checked)
				return LoggerLevel.Error;

			return LoggerLevel.Debug;
		}

		/// <summary>
		/// Buttons the clear click.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
		void ButtonClearClick(object sender, System.EventArgs e)
		{
			m_Logger.MemoryClear();
			m_SpyForm.TraceComments.Text = string.Empty;
		}

		/// <summary>
		/// Buttons the refresh click.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
		void ButtonRefreshClick(object sender, System.EventArgs e)
		{
			AddTraceMessages();
		}

		/// <summary>
		/// Logs the level changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="CheckedChangedEventArgs" /> instance containing the event data.</param>
		void LogLevelChanged(object sender, CheckedChangedEventArgs e)
		{
			AddTraceMessages();
		}

		#endregion
	}
}
