using System;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Workflow;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server.Workflow;
using Thermo.SampleManager.Server.Workflow.Nodes;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Merge Sample Task
	/// </summary>
	[SampleManagerTask("SampleMergeTask", "Execute", WorkflowActionType.EntityName)]
	public class SampleMergeTask : SampleManagerTask
	{
		#region Member Variables

		private FormSampleComposite m_Form;
		private IEntityCollection m_Samples;
		private WorkflowActionType m_ActionType;

		#endregion

		#region Setup

		/// <summary>
		/// Setup the SampleManager task
		/// </summary>
		protected override void SetupTask()
		{
			// Make sure setup of task is correct

			if (Context.SelectedItems.ActiveCount == 0)
			{
				throw new SampleManagerError(Library.Message.GetMessage("LaboratoryMessages", "SampleMergeError"),
											 Library.Message.GetMessage("LaboratoryMessages", "SampleMergeSelectSamplesError"));
			}

			m_Samples = Context.SelectedItems;

			// Get the Sample Workflow

			Sample sample = (Sample)m_Samples[0];
			WorkflowNode sampleNode = (WorkflowNode)sample.GetWorkflowNode();
			if (!BaseEntity.IsValid(sampleNode))
			{
				throw new SampleManagerError(Library.Message.GetMessage("LaboratoryMessages", "SampleMergeError"),
											 Library.Message.GetMessage("LaboratoryMessages", "SampleMergeNoWorkflowError"));
			}

			// Make sure we have the same sample types

			foreach (SampleInternal item in m_Samples)
			{
				if (item.WorkflowNode.WorkflowId == sampleNode.WorkflowId) continue;
				throw new SampleManagerError(Library.Message.GetMessage("LaboratoryMessages", "SampleMergeError"),
											 Library.Message.GetMessage("LaboratoryMessages", "SampleMergeWrongWorkflowError"));
			}

			// Get the Action Type

			var id = new Identity(Context.EntityType, Context.TaskParameters[0]);

			m_ActionType = (WorkflowActionType)EntityManager.Select(WorkflowActionType.EntityName, id);

			if (!BaseEntity.IsValid(m_ActionType))
			{
				throw new SampleManagerError(Library.Message.GetMessage("LaboratoryMessages", "SampleMergeError"),
											 Library.Message.GetMessage("LaboratoryMessages", "SampleMergeMissingActionError"));
			}

			// Bring up a confirm window

			m_Form = (FormSampleComposite)FormFactory.CreateForm(FormSampleComposite.GetInterfaceName());
			m_Form.Created += FormCreated;
			m_Form.Loaded += FormLoaded;

			// Off we go...

			m_Form.Show();
		}

		/// <summary>
		/// Handles the Created event of the m_Form control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void FormCreated(object sender, EventArgs e)
		{
			// Populate the data

			m_Form.MergeSamples.Republish(m_Samples);
		}

		/// <summary>
		/// Form Loaded Event
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void FormLoaded(object sender, EventArgs e)
		{
			m_Form.SubmitButton.Click += SubmitButton_Click;
			m_Form.AddButton.Click += AddButton_Click;
		}

		/// <summary>
		/// Handles the Click event of the SubmitButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		void SubmitButton_Click(object sender, EventArgs e)
		{
			// Lets run the composite action

			bool firstTime = true;
			var firstBag = new WorkflowPropertyBag();
			firstBag.Add(CompositeNode.ExpectedSamplesProperty, m_Samples);

			foreach (Sample sample in m_Samples)
			{
				// Run the composite nodes on only the first workflow sample, but pass 
				// in the other samples to be parents.

				if (firstTime)
				{
					firstTime = false;
					Library.Workflow.PerformAction(sample, m_ActionType, firstBag);
					continue;
				}

				// Run the workflow for the other samples, but skip composite nodes.

				var bag = new WorkflowPropertyBag();
				bag.Add(CompositeNode.SkipCompositeProperty, true);
				Library.Workflow.PerformAction(sample, m_ActionType, bag);
			}

			// Let's see what happened

			if (firstBag.HasErrors)
			{
				WorkflowError error = firstBag.Errors[0];
				Library.Utils.FlashMessage(error.Message, error.Title);
		
				return;
			}

			// Save 

			EntityManager.Commit();

			// Tell the user we did something

			string caption = m_Form.StringTable.ShowSampleTitle;
			string message = m_Form.StringTable.ShowSampleMessage;

			if (Library.Utils.FlashMessageYesNo(message, caption))
			{
				DisplayNewSamples(firstBag);
			}

			m_Form.OKButton.PerformClick();	//put here to avoid thread lockup
		}

		/// <summary>
		/// Handles the Click event of the AddButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void AddButton_Click(object sender, EventArgs e)
		{
			IEntity sample;

			// Get hold of similar samples.

			IQuery query = GetBaseQuery();

			// Remove existing Samples

			foreach (Sample existing in m_Samples)
			{
				query.AddNotEquals(SamplePropertyNames.IdNumeric, existing.IdNumeric);
			}

			// Prompt

			string message = m_Form.StringTable.AddSampleMessage;
			string title = m_Form.StringTable.AddSampleTitle;

			if (Library.Utils.PromptForEntity(message, title, query, out sample) == FormResult.OK)
			{
				if (m_Samples.Contains(sample))
				{
					message = m_Form.StringTable.AddSampleErrorMessage;
					title = m_Form.StringTable.AddSampleErrorTitle;
					Library.Utils.FlashMessage(message, title, MessageButtons.OK, MessageIcon.Hand, MessageDefaultButton.Button1);

					return;
				}

				m_Samples.Add(sample);
				m_Form.MergeSamples.Republish(m_Samples);
			}
		}

		/// <summary>
		/// Gets the base query.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="SampleManagerError">Unable to find selected sample</exception>
		private IQuery GetBaseQuery()
		{
			IQuery query;

			if (m_Samples.Count == 0)
			{
				throw new SampleManagerError(Library.Message.GetMessage("LaboratoryMessages", "SampleMergeFindSampleError"));
			}

			Sample firstSample = (Sample)m_Samples[0];

			// Find the State

			WorkflowNode workflowNode = (WorkflowNode)firstSample.WorkflowNode;
			WorkflowState workflowState = null;

			foreach (var action in workflowNode.GetActionNodes())
			{
				if (action.ActionType.Identity == m_ActionType.Identity)
				{
					workflowState = (WorkflowState)action.RequiredState;
					break;
				}
			}

			// Use the State if available to drive the query

			if (workflowState != null && BaseEntity.IsValid(workflowState))
			{
				query = workflowState.GetQuery();
			}
			else
			{
				query = EntityManager.CreateQuery(Sample.EntityName);
			}

			query.AddEquals(SamplePropertyNames.WorkflowNode, workflowNode);
			return query;
		}

		#endregion

		#region Merging

		/// <summary>
		/// Displays the new samples.
		/// </summary>
		/// <param name="propertyBag">The property bag.</param>
		private void DisplayNewSamples(IWorkflowPropertyBag propertyBag)
		{
			IEntityCollection samples = EntityManager.CreateEntityCollection(Sample.EntityName);

			foreach (Sample newSample in propertyBag.GetEntities(Sample.EntityName))
			{
				samples.Add(newSample);
			}

			Library.Task.CreateTask(35103, samples);
		}

		#endregion
	}
}