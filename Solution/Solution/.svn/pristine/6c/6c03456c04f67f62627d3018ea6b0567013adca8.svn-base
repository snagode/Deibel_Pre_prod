using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of VersionedAnalysis Training History.
	/// </summary>
	[SampleManagerTask("VersionedAnalysisTrainingHistoryTask", "GENERAL")]
	public class VersionedAnalysisTrainingHistoryTask : DefaultSingleEntityTask
	{
		#region Member Variables

		private FormTrainingHistory m_Form;
		private VersionedAnalysis m_VersionedAnalysis;

		#endregion

		#region Overrides

		/// <summary>
		/// Add the VersionedAnalysis name to the title
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormTrainingHistory) MainForm;
			m_VersionedAnalysis = (VersionedAnalysis) MainForm.Entity;

			m_Form.Title += " - ";
			m_Form.Title += m_VersionedAnalysis.Name;
		}

		/// <summary>
		/// Set data dependent prompt browse - load the grid with summary information
		/// </summary>
		protected override void MainFormLoaded()
		{
			ISchemaField analField = m_VersionedAnalysis.FindSchemaField(VersionedAnalysisPropertyNames.Identity);

			string identity = m_VersionedAnalysis.Identity.PadRight(analField.TextualLength);
			IEntityCollection trainingHistory = TrainingHistory.BuildHistory(EntityManager, TableNames.VersionedAnalysisTraining,
			                                                                 identity);

			m_Form.IdentityString.Text = m_VersionedAnalysis.Identity;
			m_Form.DataEntityCollectionDesign1.Publish(trainingHistory);
		}

		#endregion
	}
}