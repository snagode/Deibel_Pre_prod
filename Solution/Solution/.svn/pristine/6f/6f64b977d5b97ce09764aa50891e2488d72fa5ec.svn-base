using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Server.Workflow.Definition;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Workflow Task Proxy
	/// </summary>
	[SampleManagerTask("WorkflowTaskProxy", "LABTABLE", "WORKFLOW")]
	public class WorkflowTaskProxy : SampleManagerTask
	{
		#region Task Setup

		/// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			// The idea of this task is to call the appropriate option for the specified workflow type

			if (Context.LaunchMode == GenericLabtableTask.AddOption)
			{
				// Add not supported as we cannot determine the workflow type

				string errorMessage = Library.Message.GetMessage("WorkflowMessages", "WorkflowProxyAddError");
				Library.Utils.FlashMessage(errorMessage, Context.MenuItem.Description, MessageButtons.OK, MessageIcon.Error, MessageDefaultButton.Button1);
			}
			else
			{
				// Call the appropriate display/modify option for a selected item so we have to have a 
				// selected workflow - otherwise its all a little pointless.

				if (Context.SelectedItems.Count == 0)
				{
					throw new SampleManagerError(Library.Message.GetMessage("LaboratoryMessages", "WorkflowProxySelectedWorkflowError"), Context.MenuItem.Description);
				}

				// Spin through starting appropriate tasks

				foreach (WorkflowInternal workflow in Context.SelectedItems)
				{
					ProxyWorkflowOption(workflow, Context.LaunchMode);
				}
			}

			// Drop out this task does nothing itself.

			Exit();
		}

		#endregion

		#region Workflow Proxy

		/// <summary>
		/// Proxies the workflow option.
		/// </summary>
		/// <param name="workflow">The workflow.</param>
		/// <param name="launchMode">The launch mode.</param>
		protected virtual void ProxyWorkflowOption(WorkflowInternal workflow, string launchMode)
		{
			switch (launchMode)
			{
				case GenericLabtableTask.DisplayOption:
					ProxyWorkflowDisplayOption(workflow);
					break;
				case GenericLabtableTask.ModifyOption:
					ProxyWorkflowModifyOption(workflow);
					break;
				case GenericLabtableTask.CopyOption:
					ProxyWorkflowCopyOption(workflow);
					break;
				case GenericLabtableTask.AddOption:
					ProxyWorkflowAddOption(workflow);
					break;
				default:
					throw new SampleManagerError(Library.Message.GetMessage("LaboratoryMessages", "WorkflowProxyDisplayModifyError"), Context.MenuItem.Description);
			}
		}

		/// <summary>
		/// Proxies the workflow modify option.
		/// </summary>
		/// <param name="workflow">The workflow.</param>
		protected virtual bool ProxyWorkflowModifyOption(WorkflowInternal workflow)
		{
			if (BaseEntity.IsValid(workflow.WorkflowTypeInformation))
			{
				int proc = workflow.WorkflowTypeInformation.ModifyOption;
				Library.Task.CreateTask(proc, workflow);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Proxies the workflow display option.
		/// </summary>
		/// <param name="workflow">The workflow.</param>
		protected virtual bool ProxyWorkflowDisplayOption(WorkflowInternal workflow)
		{
			if (BaseEntity.IsValid(workflow.WorkflowTypeInformation))
			{
				int proc = workflow.WorkflowTypeInformation.DisplayOption;
				Library.Task.CreateTask(proc, workflow);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Proxies the workflow copy option.
		/// </summary>
		/// <param name="workflow">The workflow.</param>
		protected virtual bool ProxyWorkflowCopyOption(WorkflowInternal workflow)
		{
			if (BaseEntity.IsValid(workflow.WorkflowTypeInformation))
			{
				int proc = workflow.WorkflowTypeInformation.CopyOption;
				Library.Task.CreateTask(proc, workflow);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Proxies the workflow add option.
		/// </summary>
		/// <param name="workflow">The workflow.</param>
		protected virtual bool ProxyWorkflowAddOption(WorkflowInternal workflow)
		{
			if (BaseEntity.IsValid(workflow.WorkflowTypeInformation))
			{
				int proc = workflow.WorkflowTypeInformation.ModifyOption;
				Library.Task.CreateTask(proc, workflow);
				return true;
			}

			return false;
		}

		#endregion
	}
}
