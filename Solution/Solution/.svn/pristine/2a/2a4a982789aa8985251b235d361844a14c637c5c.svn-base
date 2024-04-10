using System.Collections;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Utilities;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Sample Split Task
	/// </summary>
	[SampleManagerTask("SampleSplitTask")]
	public class SampleSplitTask : SampleAdminTask
	{
		#region Member Variables

		private bool m_Initialising;
		
		#endregion

		#region Setup

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			base.MainFormLoaded();

			if (HasExited) return;

			m_Initialising = true;

			DefaultWorkflow = Context.Workflow as WorkflowBase;

			if (DefaultWorkflow == null)
				AddNewWorkflow(PhraseWflowType.PhraseIdSUBSAMPLE);

			m_Form.TreeListItems.ExpandAll();

			if (DefaultWorkflow != null)
				RunWorkflowOnce(DefaultWorkflow);
		}

		#endregion

		#region Sub Sampling

		/// <summary>
		/// Gets the parent sample.
		/// </summary>
		/// <returns></returns>
		protected override IEnumerable GetWorkflowTargets()
		{
			if (m_Initialising)
			{
				return Context.SelectedItems;
			}

			return base.GetWorkflowTargets();
		}

		#endregion

		#region Load

		/// <summary>
		/// Prompts for existing data to modify / display.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		protected override bool PromptForData(out IEntity entity)
		{
			// Get a sample from the workflow if possible
	
			if ((Context.Workflow != null) && (Context.Workflow.Properties != null))
			{
				object contextSample;

				Context.Workflow.Properties.TryGetValue(SampleBase.EntityName, out contextSample);

				IEntity entitySample = contextSample as IEntity;

				if (contextSample != null)
				{
					entity = entitySample;
					return true;
				}
			}

			// Prompt the user for a sample of status V or C

			string message = Library.Message.GetMessage("GeneralMessages", "SampleAdminPromptForData");
			string entityTypeDisplay = TextUtils.GetDisplayText(SampleBase.EntityName);
			message = string.Format(message, entityTypeDisplay);

			IQuery query = EntityManager.CreateQuery(SampleBase.EntityName);
			query.AddEquals(SamplePropertyNames.Status, PhraseSampStat.PhraseIdV);
			query.AddOr();
			query.AddEquals(SamplePropertyNames.Status, PhraseSampStat.PhraseIdC);

			FormResult result = Library.Utils.PromptForEntity(message, Context.MenuItem.Description, query, out entity, TriState.No, Context.MenuProcedureNumber);
			return result == FormResult.OK;
		}

		#endregion
	}
}
