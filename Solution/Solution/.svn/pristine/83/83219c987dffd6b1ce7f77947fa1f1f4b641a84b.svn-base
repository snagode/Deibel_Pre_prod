using System;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Instrument Part server side task.
	/// </summary>
	[SampleManagerTask("InstrumentPartTask", "GENERAL", "INSTRUMENT_PART")]
	public class InstrumentPartTask : GenericLabtableTask
	{
		#region Member Variables

		private FormInstrumentPart m_Form;
		private InstrumentPart m_Part;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Part = (InstrumentPart) MainForm.Entity;
			m_Form = (FormInstrumentPart) MainForm;

			// Assign Property Changed and Status Changed Events
			m_Part.PropertyChanged += new PropertyEventHandler(PartPropertyChanged);

			// Control the setting of the instrument type property
			m_Part.InstrumentPartTemplateBeforeChange += new EventHandler(PartInstrumentPartTemplateBeforeChange);
		}

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			// Set initial state of Calibration and Servicing Controls
			EnableCalibControls();
			EnableServiceControls();
			EnableAvailablePrompt();
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Handles the PropertyChanged event of the InstrumentPart Entity.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.PropertyEventArgs"/> instance containing the event data.</param>
		private void PartPropertyChanged(object sender, PropertyEventArgs e)
		{
			switch (e.PropertyName)
			{
				case InstrumentPartPropertyNames.RequiresCalibration:
					EnableCalibControls();
					break;
				case InstrumentPartPropertyNames.RequiresServicing:
					EnableServiceControls();
					break;
				case InstrumentPartPropertyNames.Retired:
					EnableAvailablePrompt();
					break;
			}
		}

		/// <summary>
		/// Instrument part template before change.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void PartInstrumentPartTemplateBeforeChange(object sender, EventArgs e)
		{
			if (!Library.Utils.FlashMessageYesNo(m_Form.StringTable.ChangeType, m_Form.StringTable.TypeWarning))
			{
				BeforePropertyChangedEventArgs realE = (BeforePropertyChangedEventArgs) e;

				realE.Cancel = true;
			}
		}

		#endregion

		#region Enable Controls

		/// <summary>
		/// Enables the available prompt.
		/// </summary>
		private void EnableAvailablePrompt()
		{
			m_Form.Available.Enabled = !m_Part.Retired;
		}

		/// <summary>
		/// Enables / disables the calibration controls.
		/// </summary>
		private void EnableCalibControls()
		{
			m_Form.CalibrationPlan.Enabled = m_Part.RequiresCalibration;
			m_Form.CalibContractor.Enabled = m_Part.RequiresCalibration;
			m_Form.CalibLeadTime.Enabled = m_Part.RequiresCalibration;
			m_Form.NextCalibDate.Enabled = m_Part.RequiresCalibration;
		}

		/// <summary>
		/// Enables / disables the servicing controls.
		/// </summary>
		private void EnableServiceControls()
		{
			m_Form.ServiceInterval.Enabled = m_Part.RequiresServicing;
			m_Form.ServiceContractor.Enabled = m_Part.RequiresServicing;
			m_Form.ServiceLeadTime.Enabled = m_Part.RequiresServicing;
			m_Form.NextServiceDate.Enabled = m_Part.RequiresServicing;
		}

		#endregion
	}
}