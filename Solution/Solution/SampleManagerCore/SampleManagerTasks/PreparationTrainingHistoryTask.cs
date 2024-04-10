using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of Preparation Training History.
	/// </summary>
	[SampleManagerTask("PreparationTrainingHistoryTask", "GENERAL")]
	public class PreparationTrainingHistoryTask : DefaultSingleEntityTask
	{
		#region Member Variables

		private FormTrainingHistory m_Form;
		private Preparation m_Preparation;

		#endregion

		#region Overides

		/// <summary>
		/// Add the Preparation name to the title
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormTrainingHistory) MainForm;
			m_Preparation = (Preparation) MainForm.Entity;

			m_Form.Title += " - ";
			m_Form.Title += m_Preparation.Name;
		}

		/// <summary>
		/// Set data dependent prompt browse - load the grid with summary information
		/// </summary>
		protected override void MainFormLoaded()
		{
			ISchemaField prepField = m_Preparation.FindSchemaField(PreparationPropertyNames.Identity);

			string identity = m_Preparation.Identity.PadRight(prepField.TextualLength);
			IEntityCollection trainingHistory = TrainingHistory.BuildHistory(EntityManager, TableNames.PreparationTraining,
			                                                                 identity);

			m_Form.IdentityString.Text = m_Preparation.Identity;
			m_Form.DataEntityCollectionDesign1.Publish(trainingHistory);
		}

		#endregion
	}
}