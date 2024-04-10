using System;
using Thermo.SampleManager.Common.CommandLine;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Workflow;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server.Workflow;

namespace Thermo.SampleManager.Tasks.BackgroundTasks
{
	/// <summary>
	/// Run a workflow non-interactively
	/// </summary>
	[SampleManagerTask("RunWorkflowBackground")]
	public class RunWorkflowBackgroundTask : SampleManagerTask, IBackgroundTask
	{
		#region Properties

		/// <summary>
		/// Gets or sets the name of the report.
		/// </summary>
		/// <value>The name of the report.</value>
		[CommandLineSwitch("workflow", "Name or GUID of the workflow to be run.", true)]
		public string WorkflowName { get; set; }

		/// <summary>
		/// Gets or sets the workflow version.
		/// </summary>
		/// <value>
		/// The workflow version.
		/// </value>
		[CommandLineSwitch("version", "Version of the workflow to be run.", false)]
		public string WorkflowVersion { get; set; }
		#endregion

		#region IBackgroundTask Implementation

		/// <summary>
		/// Launches this background task.
		/// </summary>
		public void Launch()
		{
			Workflow workflow = null;

			try
			{
				Guid workflowGuid;

				if (Guid.TryParse(WorkflowName, out workflowGuid))
				{
					if (string.IsNullOrEmpty(WorkflowVersion))
					{
						workflow = (Workflow) EntityManager.SelectLatestVersion(TableNames.Workflow, new Identity(WorkflowName));
					}
					else
					{
						IQuery query = EntityManager.CreateQuery(TableNames.Workflow);

						query.AddEquals(WorkflowPropertyNames.WorkflowGuid, WorkflowName);
						query.AddEquals(WorkflowPropertyNames.WorkflowVersion, WorkflowVersion);
						query.AddEquals(WorkflowPropertyNames.ApprovalStatus, PhraseApprStat.PhraseIdA);

						IEntityCollection workflowCollection = EntityManager.Select(TableNames.Workflow, query);

						if (workflowCollection.Count > 0)
						{
							workflow = (Workflow)workflowCollection[0];
						}
					}
				}
				else
				{
					IQuery query = EntityManager.CreateQuery(TableNames.Workflow);
					query.AddEquals(WorkflowPropertyNames.WorkflowName, WorkflowName);

					if (!string.IsNullOrEmpty(WorkflowVersion))
						query.AddEquals(WorkflowPropertyNames.WorkflowVersion, WorkflowVersion);


					query.AddOrder(WorkflowPropertyNames.WorkflowVersion,false);
					query.AddEquals(WorkflowPropertyNames.ApprovalStatus, PhraseApprStat.PhraseIdA);

					IEntityCollection workflowCollection = EntityManager.Select(TableNames.Workflow, query);

					if (workflowCollection.Count > 0)
					{
						workflow = (Workflow) workflowCollection[0];
					}
				}

				if ((workflow != null) && (!workflow.IsNull()))
				{
					Logger.InfoFormat("Running workflow {0}", WorkflowName);
	
					IWorkflowPropertyBag bag = workflow.Perform();

					if (bag.HasErrors)
					{
						Logger.Info("Failed.");

						foreach (WorkflowError error in bag.Errors)
						{
							Logger.Info(error.Message);
						}
					}
					else
					{
						EntityManager.Commit();
						TriggerPostCommitWorkflow();
						Logger.Info("Complete.");
					}
				}
				else
				{
					Logger.InfoFormat("Unable to locate workflow {0}", WorkflowName);
				}
			}
			catch (Exception e)
			{
				Logger.InfoFormat("Error running the workflow {0} : {1}", WorkflowName, e.Message);
				Logger.Debug(e.InnerException.Message);
			}
		}

		/// <summary>
		/// Triggers the post commit workflow.
		/// </summary>
		private void TriggerPostCommitWorkflow()
		{
			var workflowEventService = Library.GetService<IWorkflowEventService>();
			try
			{
				IWorkflowPropertyBag bag = workflowEventService.ProcessDeferredTriggers("POST_LOGIN");

				if (bag.Errors.Count != 0)
				{
					Logger.Error(bag.Errors[0].Message);
				}
				else
				{
					EntityManager.Commit();
				}
			}
			catch (Exception performException)
			{
				Logger.Error(performException);
			}
		}
		#endregion
	}
}