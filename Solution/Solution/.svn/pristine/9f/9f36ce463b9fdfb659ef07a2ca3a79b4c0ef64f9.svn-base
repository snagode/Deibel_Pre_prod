using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of the Sampling Procedure LTE
	/// </summary>
	[SampleManagerTask("SamplingProcedureTask", "LABTABLE", "SAMPLING_PROCEDURE")]
	public class SamplingProcedureTask : GenericLabtableTask
	{
		#region Member Variables

		private FormSamplingProcedure m_Form;
		private SamplingProcedure m_SamplingProcedure;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormSamplingProcedure) MainForm;
			m_SamplingProcedure = (SamplingProcedure) MainForm.Entity;
			m_SamplingProcedure.PropertyChanged += SamplingProcedurePropertyChanged;
		}

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			SetupPrompts();
		}

		#endregion

		#region Prompt Management

		/// <summary>
		/// Handles the PropertyChanged event of the m_SamplingProcedure control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.PropertyEventArgs"/> instance containing the event data.</param>
		private void SamplingProcedurePropertyChanged(object sender, PropertyEventArgs e)
		{
			if (e.PropertyName == SamplingProcedurePropertyNames.ReplicateRoutine)
				SetupPrompts();
			else if (e.PropertyName == SamplingProcedurePropertyNames.VglLibrary)
			{
				m_SamplingProcedure.ReplicateRoutine = null;
				m_SamplingProcedure.ProcedureRoutine = null;
			}
		}

		/// <summary>
		/// Makes the appropriate prompt enabled
		/// </summary>
		private void SetupPrompts()
		{
			m_Form.ReplicateNumber.Enabled = string.IsNullOrEmpty(m_SamplingProcedure.ReplicateRoutine);
		}

		#endregion
	}
}