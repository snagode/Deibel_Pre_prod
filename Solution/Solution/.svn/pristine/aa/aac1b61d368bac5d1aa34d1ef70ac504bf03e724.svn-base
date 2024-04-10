using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Sample Admin Resample Task - This is involved with Resampling.
	/// </summary>
	[SampleManagerTask("SampleAdminResampleTask")]
	public class SampleAdminResampleTask : SampleAdminBaseTask
	{
		#region Member Variables

		/// <summary>
		/// First sample in the selection that was used to launch this task
		/// </summary>
		private Sample m_ContextSample;

		#endregion

		#region Setup

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			var bag = Context.Workflow.Properties;
			m_ContextSample = bag.Get<Sample>(SampleBase.EntityName);

			base.MainFormLoaded();

			SimpleTreeListNodeProxy contextTreeNode = m_Form.TreeListItems.FindNodeByData(m_ContextSample);

			m_FocusedTreeEntity = m_ContextSample;
			m_Form.TreeListItems.FocusNode(contextTreeNode);

			RefreshTestsGrid();
			m_VisibleGrid = m_Form.GridTestProperties;
			m_TabPageTests.Show();
			m_ToolBarButtonTranspose.Enabled = true;
		}

		#endregion

		#region Data Initialisation

		/// <summary>
		/// Validates the context.
		/// </summary>
		/// <returns></returns>
		protected override bool ValidateContext()
		{
			return true;
		}

		/// <summary>
		/// Initialises the data.
		/// </summary>
		/// <returns></returns>
		protected override bool InitialiseTopLevelEntities(out IEntityCollection topLevelEntities)
		{
			topLevelEntities = EntityManager.CreateEntityCollection(Sample.EntityName, false);
			topLevelEntities.Add(m_ContextSample);

			return true;
		}

		#endregion

		#region General Overrides

		/// <summary>
		/// Gets the name of the top level table.
		/// </summary>
		/// <returns></returns>
		protected override string GetTopLevelTableName()
		{
			return SampleBase.EntityName;
		}

		/// <summary>
		/// Gets a value indicating whether this instance is job workflow.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is job workflow; otherwise, <c>false</c>.
		/// </value>
		protected override bool IsJobWorkflow
		{
			get { return false; }
		}

		/// <summary>
		/// Sets the title.
		/// </summary>
		/// <returns></returns>
		protected override string GetTitle()
		{
			return string.Format(m_Form.StringTable.TitleResample, m_ContextSample.OriginalSample.IdText);
		}

		#endregion
	}
}
