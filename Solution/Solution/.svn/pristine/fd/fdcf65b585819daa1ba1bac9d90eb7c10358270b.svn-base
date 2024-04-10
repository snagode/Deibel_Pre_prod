using System;
using System.Threading;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Workflow;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server.Workflow;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Workflow Run Task
	/// </summary>
	[SampleManagerTask("WorkflowRunTask", "Execute", Workflow.EntityName)]
	public class WorkflowRunTask : SampleManagerTask
	{
		#region Member Variables

		private IEntityCollection m_WorkflowsToRun;

		#endregion

		#region Properties

		private IWorkflowService WorkflowService { get; set; }
		 
		#endregion

		#region Setup Task

		/// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{

			WorkflowService = Library.GetService<IWorkflowService>();

			// We need one or more workflow to run

			if (!ProcessParameters())
			{
				Exit();
				return;
			}

			// Make Sure we have some workflow to run

			if (m_WorkflowsToRun.Count == 0 && Library.Environment.IsInteractive())
			{
				PromptForItem();
			}

			// Drop out if there is nothing to do

			if (m_WorkflowsToRun.Count == 0)
			{
				 Exit();
				return;
			}

			// Spawn the Workflow on another thread, this will stop the workflow blocking setup.

			ThreadPool.QueueUserWorkItem(RunWorkflow);
		}

		/// <summary>
		/// Runs the workflow
		/// </summary>
		/// <param name="parameter">The parameter.</param>
		private void RunWorkflow(object parameter)
		{
			// Process each workflow

			foreach (Workflow workflow in m_WorkflowsToRun)
			{
				if (workflow.WorkflowType.IsPhrase(PhraseWflowType.PhraseIdGENERAL))
				{
					RunWorkflow(workflow);
				}
				else if (Library.Environment.IsBackground())
				{
					RunWorkflow(workflow);
				}
				else
				{
					SpawnWorkflow(workflow);
				}
			}

			// All Done

			Exit();
		}

		/// <summary>
		/// Spawns the workflow.
		/// </summary>
		/// <param name="wf">The wf.</param>
		private void SpawnWorkflow(Workflow wf)
		{
			try
			{
				WorkflowService.PerformTask(((IEntity)wf).Identity);
			}
			catch (Exception e)
			{
				if (Library.Environment.IsInteractive())
				{
					Library.Utils.FlashMessage(e.Message, wf.Name);
				}
			}
		}

		/// <summary>
		/// Processes the parameters.
		/// </summary>
		private bool ProcessParameters()
		{
			m_WorkflowsToRun = EntityManager.CreateEntityCollection(TableNames.Workflow);

			// Get it as a task parameter

			if (BaseEntity.IsValid(Context.Workflow))
			{
				m_WorkflowsToRun.Add(Context.Workflow);
			}

			// Allow Task Parameters with the name of the Workflow to Run

			if (!string.IsNullOrEmpty(Context.TaskParameterString))
			{
				IQuery namedWorkflow = EntityManager.CreateQuery(TableNames.Workflow);
				namedWorkflow.AddEquals(WorkflowPropertyNames.WorkflowName, Context.TaskParameterString);
				namedWorkflow.AddEquals(WorkflowPropertyNames.Active, true);

				IEntityCollection workflow = EntityManager.Select(TableNames.Workflow, namedWorkflow);

				if (workflow.ActiveCount == 1)
				{
					m_WorkflowsToRun.Add(workflow[0]);
					return true;
				}
			}

			// Check if we RMBd on a workflow
	
			if (Context.SelectedItems.Count != 0)
			{
				if (Context.SelectedItems[0].EntityType == TableNames.Workflow)
				{
					foreach (Workflow selectedWorkflow in Context.SelectedItems)
					{
						if (selectedWorkflow.Active)
							m_WorkflowsToRun.Add(selectedWorkflow);
					}

					if (m_WorkflowsToRun.Count == 0)
					{
						string message = Library.Message.GetMessage("WorkflowMessages", "MustBeActive");
						Library.Utils.FlashMessage(message, Context.MenuItem.Description, MessageButtons.OK, MessageIcon.Error, MessageDefaultButton.Button1);
						return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Prompts for item.
		/// </summary>
		private void PromptForItem()
		{
			IQuery query = EntityManager.CreateQuery(Context.EntityType);

			query.AddEquals(WorkflowPropertyNames.WorkflowType, PhraseWflowType.PhraseIdGENERAL);
			query.AddOr();
			query.AddEquals(WorkflowPropertyNames.WorkflowType, PhraseWflowType.PhraseIdENTITY);
			query.AddOr();
			query.AddEquals(WorkflowPropertyNames.WorkflowType, PhraseWflowType.PhraseIdSAMPLE);
			query.AddOr();
			query.AddEquals(WorkflowPropertyNames.WorkflowType, PhraseWflowType.PhraseIdJOB_HEADER);

			IEntity entity;

			string message = Library.Message.GetMessage("WorkflowMessages", "ChooseWorkflow");
			FormResult result = Library.Utils.PromptForEntity(message, Context.MenuItem.Description, query, out entity);

			if (result == FormResult.OK)
			{
				m_WorkflowsToRun.Add(entity);
			}
		}

		#endregion

		#region Run the workflow

		/// <summary>
		/// Runs the workflow.
		/// </summary>
		/// <param name="wf">The wf.</param>
		private IWorkflowPropertyBag RunWorkflow(Workflow wf)
		{
			IWorkflowPropertyBag bag = wf.Perform();

			// If it's not a General Workflow report the errors

			if (! wf.WorkflowType.IsPhrase(PhraseWflowType.PhraseIdGENERAL)) return null;

			// Handle any errors

			if (bag.Errors.Count == 0)
			{
				try
				{
					EntityManager.Commit();

					// Don't report success, if you want a message then add it to the workflow.
				}
				catch (Exception exception)
				{
					string message = Library.Message.GetMessage("WorkflowMessages", "ExecuteWorkflowFailure", wf.Name, exception.Message);
					Library.Utils.FlashMessage(message, Context.MenuItem.Description, MessageButtons.OK, MessageIcon.Error, MessageDefaultButton.Button1);
				}
			}
			else
			{
				string error = bag.Errors[0].Message;
				string message = Library.Message.GetMessage("WorkflowMessages", "ExecuteWorkflowFailure", wf.Name, error);

				Library.Utils.FlashMessage(message, Context.MenuItem.Description, MessageButtons.OK, MessageIcon.Error, MessageDefaultButton.Button1);
			}

			return bag;
		}

		#endregion
	}
}