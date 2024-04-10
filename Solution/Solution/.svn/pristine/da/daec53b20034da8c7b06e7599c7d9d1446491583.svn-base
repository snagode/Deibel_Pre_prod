using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Instrument Part Template server side task.
	/// </summary>
	[SampleManagerTask("InstrumentPartTemplateTask", "GENERAL", "INSTRUMENT_PART_TEMPLATE")]
	public class InstrumentPartTemplateTask : GenericLabtableTask
	{
		#region Member Variables

		private FormInstrumentPartTemplate m_Form;
		private InstrumentPartTemplate m_PartTemplate;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			//Get the Form
			m_Form = (FormInstrumentPartTemplate) MainForm;

			//Get the Instrument Part
			m_PartTemplate = (InstrumentPartTemplate) m_Form.Entity;

			//Assign Property Changed Event
			m_PartTemplate.PropertyChanged += new PropertyEventHandler(PartTemplatePropertyChanged);
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
		/// Handles the PropertyChanged event of the m_Part Entity.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.PropertyEventArgs"/> instance containing the event data.</param>
		private void PartTemplatePropertyChanged(object sender, PropertyEventArgs e)
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
			m_Form.CalibrationPlan.Enabled = m_PartTemplate.RequiresCalibration;
			m_Form.CalibContractor.Enabled = m_PartTemplate.RequiresCalibration;
			m_Form.CalibLeadTime.Enabled = m_PartTemplate.RequiresCalibration;
		}

		/// <summary>
		/// Enables / disables the servicing controls.
		/// </summary>
		private void EnableServiceControls()
		{
			m_Form.ServiceInterval.Enabled = m_PartTemplate.RequiresServicing;
			m_Form.ServiceContractor.Enabled = m_PartTemplate.RequiresServicing;
			m_Form.ServiceLeadTime.Enabled = m_PartTemplate.RequiresServicing;
		}

		#endregion
	}
}