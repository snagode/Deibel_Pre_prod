using System;
using Thermo.SampleManager.Common.CommandLine;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Workflow;
using Thermo.SampleManager.Library;

namespace Thermo.SampleManager.Tasks.BackgroundTasks
{
	/// <summary>
	/// Run a workflow action non-interactively
	/// </summary>
	[SampleManagerTask("RunWorkflowActionBackground")]
	public class RunWorkflowActionBackgroundTask : SampleManagerTask, IBackgroundTask
	{
		#region Properties

		/// <summary>
		/// Gets or sets the name of the report.
		/// </summary>
		/// <value>The name of the report.</value>
		[CommandLineSwitch("action", "Workflow Action to be run.", true)]
		public string WorkflowActionName { get; set; }

		#endregion

		#region IBackgroundTask Implementation

		/// <summary>
		/// Launches this background task.
		/// </summary>
		public void Launch()
		{
			Logger.DebugFormat("Started Processing Background Workflow Action {0}", WorkflowActionName);

			if (Context.SelectedItems.Count > 0)
			{
				IEntity entity = Context.SelectedItems[0];

				try
				{
					Logger.InfoFormat("Performing the action {0} on the {1} named ({2})", WorkflowActionName, entity.EntityType, entity.Name);

					IWorkflowPropertyBag bag = entity.PerformAction(WorkflowActionName,null);

					if (bag.HasErrors)
					{
						Logger.Info("Performing the workflow resulted in errors");

						foreach (WorkflowError error in bag.Errors)
						{
							Logger.Error(error.Message);
						}
					}
					else
					{
						EntityManager.Commit();
						Logger.Info("Workflow Action performed without error");
					}
				}
				catch (WorkflowError e)
				{
					Logger.ErrorFormat("Workflow Error : {0}", e.Message);
				}
				catch (Exception e)
				{
					string message = string.Format("Error : {0}", e.Message);
					Logger.Error(message, e);
				}
			}
			else
			{
				Logger.InfoFormat("No entity was specified to perform {0} on", WorkflowActionName);
			}

			Logger.Debug("Finished Processing Background Workflow Action");
		}

		#endregion
	}
}