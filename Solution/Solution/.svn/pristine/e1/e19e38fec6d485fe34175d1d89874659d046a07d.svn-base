using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Workflow Document Task
	/// </summary>
	[SampleManagerTask("WorkflowDocumentTask", "Document", Workflow.EntityName)]
	public class WorkflowDocumentTask : SampleManagerTask
	{
		#region Setup Task

		/// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			// Make Sure we have some workflow to document

			if (Context.SelectedItems.Count != 0) return;

			// An entity has not been selected then prompt for one

			IQuery query = EntityManager.CreateQuery(Context.EntityType);
			IEntity entity;

			string message = Library.Message.GetMessage("WorkflowMessages", "ChooseWorkflow");
			FormResult result = Library.Utils.PromptForEntity(message, Context.MenuItem.Description, query, out entity);

			if (result == FormResult.OK)
			{
				Context.SelectedItems.Add(entity);
				return;
			}

			Exit();
		}

		#endregion

		#region Task Execution

		/// <summary>
		/// Task is ready for execution
		/// </summary>
		protected override void TaskReady()
		{
			// Document the Workflows

			foreach (Workflow workflow in Context.SelectedItems)
			{
				DocumentWorkflow(workflow);
			}

			// Exit the task as the workflows have been executed.

			Exit();
		}

		/// <summary>
		/// Runs the workflow.
		/// </summary>
		/// <param name="wf">The wf.</param>
		private void DocumentWorkflow(Workflow wf)
		{
			// Generate excessive amounts of documentation if the task has an EXTREME parameter.

			bool extreme = (Context.TaskParameters.GetUpperBound(0) == 0 && Context.TaskParameters[0] == "EXTREME");

			// Make the document and open it on the client.

			string message = Library.Message.GetMessage("WorkflowMessages", "DocWorkflowStart", wf.WorkflowName);
			if (extreme) message = Library.Message.GetMessage("WorkflowMessages", "DocWorkflowStartExtreme", wf.WorkflowName);
			Library.Utils.SetStatusBar(message);

			string fileName = wf.GenerateDocument(extreme);
			Library.File.TransferToClientTemp(fileName, fileName, true);

			message = Library.Message.GetMessage("WorkflowMessages", "DocWorkflowEnd", wf.WorkflowName);
			Library.Utils.SetStatusBar(message);
		}

		#endregion
	}
}