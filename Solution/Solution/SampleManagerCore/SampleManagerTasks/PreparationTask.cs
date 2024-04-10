using System;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Preparation server side task
	/// </summary>
	[SampleManagerTask("PreparationTask", "LABTABLE", "PREPARATION")]
	public class PreparationTask : GenericLabtableTask
	{
		#region Member Variables

		private FormPreparation m_Form;
		private Preparation m_Prep;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Prep = (Preparation) MainForm.Entity;
			m_Form = (FormPreparation) MainForm;

			m_Prep.TrainedOperatorsChanged += new EventHandler(PrepTrainedOperatorsChanged);
		}

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			PublishTrainedOperators();
			SetTrainedOperatorsTitle();
		}

		/// <summary>
		/// Preps the trained operators changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void PrepTrainedOperatorsChanged(object sender, EventArgs e)
		{
			PublishTrainedOperators();
		}

		/// <summary>
		/// Publishes the trained operators.
		/// </summary>
		private void PublishTrainedOperators()
		{
			m_Form.TrainedOperBrowse.Republish(m_Prep.TrainedOperators);
			SetTrainedOperatorsTitle();
		}

		/// <summary>
		/// Sets the trained operators title.
		/// </summary>
		private void SetTrainedOperatorsTitle()
		{
			if ((m_Prep.TrainedOperators.Count == 0) && (m_Prep.PreparationTrainings.Count == 0))
				m_Form.TrainingRequired.Caption = m_Form.StringTable.NoTrainingRequired;
			else
				m_Form.TrainingRequired.Caption = m_Form.StringTable.TrainingRequired;
		}

		#endregion
	}
}