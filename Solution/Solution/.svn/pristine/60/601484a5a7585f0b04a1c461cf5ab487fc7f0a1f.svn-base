using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Instrument Template server side task
	/// </summary>
	[SampleManagerTask("InstrumentTemplateTask", "GENERAL", "INSTRUMENT_TEMPLATE")]
	public class InstrumentTemplateTask : GenericLabtableTask
	{
		#region Member Variables

		private FormInstrumentTemplate m_Form;
		private InstrumentTemplate m_InstrumentTemplate;

		#endregion

		#region Overridden Methods

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			//Get the Form
			m_Form = (FormInstrumentTemplate) MainForm;

			//Get the Instrument Part
			m_InstrumentTemplate = (InstrumentTemplate) m_Form.Entity;

			//Assign Property Changed Event
			m_InstrumentTemplate.PropertyChanged += new PropertyEventHandler(InstrumentTemplatePropertyChanged);
		}

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			//Set initial state of Calibration and Servicing Controls
			EnableCalibControls();
			EnableServiceControls();
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Handles the PropertyChanged event of the m_InstrumentTemplate Entity.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.PropertyEventArgs"/> instance containing the event data.</param>
		private void InstrumentTemplatePropertyChanged(object sender, PropertyEventArgs e)
		{
			switch (e.PropertyName)
			{
				case InstrumentPartTemplatePropertyNames.RequiresCalibration:
					EnableCalibControls();
					break;
				case InstrumentPartTemplatePropertyNames.RequiresServicing:
					EnableServiceControls();
					break;
			}
		}

		#endregion

		#region Enable Controls

		/// <summary>
		/// Enables / disables the calibration controls.
		/// </summary>
		private void EnableCalibControls()
		{
			m_Form.CalibrationPlan.Enabled = m_InstrumentTemplate.RequiresCalibration;
			m_Form.CalibContractor.Enabled = m_InstrumentTemplate.RequiresCalibration;
			m_Form.CalibLeadTime.Enabled = m_InstrumentTemplate.RequiresCalibration;
			m_Form.CalibSampleTemplate.Enabled = m_InstrumentTemplate.RequiresCalibration;
		}

		/// <summary>
		/// Enables / disables the servicing controls.
		/// </summary>
		private void EnableServiceControls()
		{
			m_Form.ServiceIntv.Enabled = m_InstrumentTemplate.RequiresServicing;
			m_Form.ServiceContractor.Enabled = m_InstrumentTemplate.RequiresServicing;
			m_Form.ServiceLeadTime.Enabled = m_InstrumentTemplate.RequiresServicing;
		}

		#endregion
	}
}