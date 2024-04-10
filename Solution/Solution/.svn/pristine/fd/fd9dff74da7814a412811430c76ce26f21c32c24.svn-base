using System;
using System.ComponentModel;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Workflow;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server.Workflow;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Workflow Action Task
	/// </summary>
	[SampleManagerTask("WorkflowActionTask", "Execute")]
	public class WorkflowActionTask : SampleManagerTask
	{
		/// <summary>
		/// The m_ uni bag
		/// </summary>
		private WorkflowPropertyBag m_UniBag;

		#region Properties

		private WorkflowActionType ActionType { get; set; }
		private WorkflowState State { get; set; }

		private BackgroundWorker m_ProcessWorker;

		#endregion

		#region Setup Task - Main work done here.

		/// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			
			// We need two parameters, the ACTION and optionally the STATE.

			ProcessParameters();

			// An entity has not been selected then prompt for one

			if (Context.SelectedItems.Count == 0)
			{
				PromptForItem();
			}

			// Process the Action on each of the items.

			if (Context.SelectedItems.Count != 0)
			{
				m_ProcessWorker = new BackgroundWorker();
				m_ProcessWorker.DoWork += (s, e) => { ProcessItems(); };
				m_ProcessWorker.RunWorkerCompleted += (s, e) => { Exit(); };
				m_ProcessWorker.RunWorkerAsync();

				return;
			}
			

			// All Done

			Exit();
		}

		/// <summary>
		/// Processes the parameters.
		/// </summary>
		/// <exception cref="SampleManagerError">Wrong number of params, specify ACTION and optionally Required STATE</exception>
		private void ProcessParameters()
		{
			if (Context.TaskParameters.Length == 0)
			{
				throw new SampleManagerError(Library.Message.GetMessage("LaboratoryMessages", "WorkflowActionParamsWrongNumber"));
			}

			// Check the Action Type

			string actionName = Context.TaskParameters[0].Trim();
			ActionType = (WorkflowActionType) EntityManager.Select(WorkflowActionType.EntityName, new Identity(Context.EntityType, actionName));

			if (!BaseEntity.IsValid(ActionType))
			{
				throw new SampleManagerError(Library.Message.GetMessage("LaboratoryMessages", "WorkflowActionParamsInvalidAction"));
			}

			// Check the State

			if (Context.TaskParameters.Length >= 2)
			{
				string stateName = Context.TaskParameters[1].Trim();
				State = (WorkflowState) EntityManager.Select(WorkflowState.EntityName, new Identity(Context.EntityType, stateName));

				if (!BaseEntity.IsValid(State))
				{
					throw new SampleManagerError(Library.Message.GetMessage("LaboratoryMessages", "WorkflowActionParamsInvalidState"));
				}
			}
		}

		/// <summary>
		/// Prompts for item.
		/// </summary>
		private void PromptForItem()
		{
			IEntity entity;
			IQuery query = EntityManager.CreateQuery(Context.EntityType);
			if (State != null)
			{
				query = State.GetQuery();
			}

			string message = Library.Message.GetMessage("WorkflowMessages", "ExecuteActionChooseEntity");
			FormResult result = Library.Utils.PromptForEntity(message, Context.MenuItem.Description, query, out entity);

			if (result == FormResult.OK)
			{
				Context.SelectedItems.Add(entity);
			}
		}

		/// <summary>
		/// Process Items
		/// </summary>
		private void ProcessItems()
		{
			bool allOk = false;

			// Execute the Action on each of the entities

			m_UniBag = new WorkflowPropertyBag();
			
			foreach (IEntity entity in Context.SelectedItems)
			{
				allOk = RunWorkflowAction(entity);
				if (!allOk) break;
			}

			// Only commit if everything was successful.

			if (allOk)
			{
				try
				{
					EntityManager.Commit();
                    TriggerPostCommitWorkflow();
				}
				catch (Exception exception)
				{
					string message = Library.Message.GetMessage("WorkflowMessages", "ExecuteActionFailure", ActionType.Name, exception.Message);
					Library.Utils.FlashMessage(message,  ActionType.Name, MessageButtons.OK, MessageIcon.Error, MessageDefaultButton.Button1);
				}
			}
		}

		/// <summary>
		/// Runs the workflow action
		/// </summary>
		/// <param name="entity">The entity.</param>
		private bool RunWorkflowAction(IEntity entity)
		{
			try
			{

				var bag = entity.PerformAction(ActionType.Identity, m_UniBag) as WorkflowPropertyBag;
				
				if (bag.Errors.Count != 0)
				{
					string error = bag.Errors[0].Message;
					string message = Library.Message.GetMessage("WorkflowMessages", "ExecuteActionFailure", ActionType.Name, error);

					Library.Utils.FlashMessage(message, ActionType.Name, MessageButtons.OK, MessageIcon.Error, MessageDefaultButton.Button1);
					return false;
				}


				//copy global variables in bag over
				foreach (var globalVariable in bag)
				{
					m_UniBag.SetGlobalVariable(globalVariable.Key,globalVariable.Value);
				}
			}
			catch (Exception performException)
			{
				string message = Library.Message.GetMessage("WorkflowMessages", "ExecuteActionFailure", ActionType.Name, performException.ToString());
				Library.Utils.FlashMessage(message, ActionType.Name, MessageButtons.OK, MessageIcon.Error, MessageDefaultButton.Button1);
				return false;
			}

			return true;
		}

        /// <summary>
        /// Trigger Post Commit Workflow
        /// </summary>
        private void TriggerPostCommitWorkflow()
        {
            IWorkflowEventService workflowEventService = Library.GetService<IWorkflowEventService>();
            try
            {
                IWorkflowPropertyBag bag = workflowEventService.ProcessDeferredTriggers("POST_LOGIN");

                if (bag.Errors.Count != 0)
                {
                    string error = bag.Errors[0].Message;
                    string message = Library.Message.GetMessage("WorkflowMessages", "ExecuteActionFailure", ActionType.Name, error);
                    Library.Utils.FlashMessage(message, ActionType.Name, MessageButtons.OK, MessageIcon.Error, MessageDefaultButton.Button1);                   
                }
                else
                {
                    EntityManager.Commit();
                }
            }
            catch (Exception performException)
            {
                string message = Library.Message.GetMessage("WorkflowMessages", "ExecuteActionFailure", ActionType.Name, performException.Message);
                Library.Utils.FlashMessage(message, ActionType.Name, MessageButtons.OK, MessageIcon.Error, MessageDefaultButton.Button1);               
            }
        }

		#endregion
	}
}