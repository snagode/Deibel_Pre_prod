using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of substance LTE.
	/// </summary>
	[SampleManagerTask("SubstanceTask", "LABTABLE", "SUBSTANCE")]
	public class SubstanceTask : GenericLabtableTask
	{
		#region Member Variables

		private FormSubstance m_Form;
		private Substance m_Substance;

		private const string DefinitionTypeBoth = "BOTH";
		private const string DefinitionTypeGhs = "GHS";
		private const string DefinitionTypeChip = "CHIP";

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the task has created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormSubstance) MainForm;
			m_Substance = (Substance) MainForm.Entity;

			m_Substance.PropertyChanged += new PropertyEventHandler(SubstancePropertyChanged);
		}

		/// <summary>
		/// Called when the task has loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			HideTabs();

			m_Form.LabelFlammability.Caption = m_Substance.Flammability.PhraseId;
			m_Form.LabelHealth.Caption = m_Substance.Health.PhraseId;
			m_Form.LabelInstability.Caption = m_Substance.Instability.PhraseId;
			m_Form.LabelSpecial.Caption = m_Substance.Special.PhraseId;
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Handles the PropertyChanged event of the m_Substance control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.PropertyEventArgs"/> instance containing the event data.</param>
		private void SubstancePropertyChanged(object sender, PropertyEventArgs e)
		{
			switch (e.PropertyName)
			{
				case SubstancePropertyNames.Flammability:
					m_Form.LabelFlammability.Caption = m_Substance.Flammability.PhraseId;
					break;

				case SubstancePropertyNames.Health:
					m_Form.LabelHealth.Caption = m_Substance.Health.PhraseId;
					break;

				case SubstancePropertyNames.Instability:
					m_Form.LabelInstability.Caption = m_Substance.Instability.PhraseId;
					break;

				case SubstancePropertyNames.Special:
					m_Form.LabelSpecial.Caption = m_Substance.Special.PhraseId;
					break;

				case SubstancePropertyNames.DefinitionType:
					HideTabs();
					break;
			}
		}

		private void HideTabs ()
		{
			if (m_Substance.DefinitionType.PhraseId==DefinitionTypeChip)
			{
				m_Form.PageGhsHazard.Visible = false;
				m_Form.PageGhsOverview.Visible = false;
				m_Form.PageGhsPictogram.Visible = false;
				m_Form.PageGhsPrecaution.Visible = false;

				m_Form.PageHandling.Visible = true;
				m_Form.PageHazards.Visible = true;
				m_Form.PageRisks.Visible = true;
				m_Form.PageSafety.Visible = true;
			}
			else if (m_Substance.DefinitionType.PhraseId==DefinitionTypeGhs)
			{
				m_Form.PageGhsHazard.Visible = true;
				m_Form.PageGhsOverview.Visible = true;
				m_Form.PageGhsPictogram.Visible = true;
				m_Form.PageGhsPrecaution.Visible = true;

				m_Form.PageHandling.Visible = false;
				m_Form.PageHazards.Visible = false;
				m_Form.PageRisks.Visible = false;
				m_Form.PageSafety.Visible = false;
			}
			else
			{
				m_Form.PageGhsHazard.Visible = true;
				m_Form.PageGhsOverview.Visible = true;
				m_Form.PageGhsPictogram.Visible = true;
				m_Form.PageGhsPrecaution.Visible = true;

				m_Form.PageHandling.Visible = true;
				m_Form.PageHazards.Visible = true;
				m_Form.PageRisks.Visible = true;
				m_Form.PageSafety.Visible = true;
			}
		}

		#endregion
	}
}