using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of the Unit LTE
	/// </summary>
	[SampleManagerTask("UnitTask", "LABTABLE", "UNIT_HEADER")]
	public class UnitTask : GenericLabtableTask
	{
		#region Member Variables

		private UnitHeader m_Unit;
		private FormUnitHeader m_UnitForm;

		#endregion

		#region Task Loaded

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_UnitForm = (FormUnitHeader) MainForm;
			m_Unit = (UnitHeader) MainForm.Entity;

			m_Unit.PropertyChanged += new PropertyEventHandler(UnitPropertyChanged);

			m_UnitForm.BaseRadioButton.CheckedChanged += BaseRadioButtonCheckedChanged;
			m_UnitForm.DerivedRadioButton.CheckedChanged += DerivedRadioButtonCheckedChanged;
			m_UnitForm.CalculatedRadioButton.CheckedChanged += CalculatedRadioButtonCheckedChanged;
			m_UnitForm.SupplementaryRadioButton.CheckedChanged += SupplementaryRadioButtonCheckedChanged;

			SetClass();
		}

		#endregion

		#region Control Events

		/// <summary>
		/// Handles the PropertyChanged event of the m_Unit control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.PropertyEventArgs"/> instance containing the event data.</param>
		private void UnitPropertyChanged(object sender, PropertyEventArgs e)
		{
			if (e.PropertyName == UnitHeaderPropertyNames.Class)
			{
				SetClass();
			}
		}

		/// <summary>
		/// Handles the CheckedChanged event of the SupplementaryRadioButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.CheckedChangedEventArgs"/> instance containing the event data.</param>
		private void SupplementaryRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (e.Checked)
			{
				m_Unit.SetClass(PhraseUnitClass.PhraseIdS);
			}
		}

		/// <summary>
		/// Handles the CheckedChanged event of the CalculatedRadioButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.CheckedChangedEventArgs"/> instance containing the event data.</param>
		private void CalculatedRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (e.Checked)
			{
				m_Unit.SetClass(PhraseUnitClass.PhraseIdC);
			}
		}

		/// <summary>
		/// Handles the CheckedChanged event of the DerivedRadioButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.CheckedChangedEventArgs"/> instance containing the event data.</param>
		private void DerivedRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (e.Checked)
			{
				m_Unit.SetClass(PhraseUnitClass.PhraseIdD);
			}
		}

		/// <summary>
		/// Handles the CheckedChanged event of the baseRadioButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.CheckedChangedEventArgs"/> instance containing the event data.</param>
		private void BaseRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (e.Checked)
			{
				m_Unit.SetClass(PhraseUnitClass.PhraseIdB);
			}
		}

		#endregion

		# region Classy Methods

		/// <summary>
		/// Sets the prompts according to the class of unit
		/// </summary>
		protected void SetClass()
		{
			string classType = m_Unit.Class.PhraseId;

			m_UnitForm.BaseRadioButton.Checked = (classType == PhraseUnitClass.PhraseIdB);
			m_UnitForm.SupplementaryRadioButton.Checked = (classType == PhraseUnitClass.PhraseIdS);
			m_UnitForm.SupplementaryPromptPanel.Visible = (classType == PhraseUnitClass.PhraseIdS);
			m_UnitForm.CalculatedRadioButton.Checked = (classType == PhraseUnitClass.PhraseIdC);
			m_UnitForm.CalculatedPromptPanel.Visible = (classType == PhraseUnitClass.PhraseIdC);
			m_UnitForm.DerivedRadioButton.Checked = (classType == PhraseUnitClass.PhraseIdD);
			m_UnitForm.DerivedPromptPanel.Visible = (classType == PhraseUnitClass.PhraseIdD);
		}

		#endregion
	}
}