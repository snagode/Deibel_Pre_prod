using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Instrument Part History Task
	/// </summary>
	[SampleManagerTask("InstrumentPartHistoryTask", "GENERAL", "INSTRUMENT")]
	public class InstrumentPartHistoryTask : DefaultSingleEntityTask
	{
		#region Member Variables

		private FormInstrumentPartHistory m_Form;
		private InstrumentPart m_InstPart;

		#endregion

		#region Main Form Creation/Load

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormInstrumentPartHistory) MainForm;
			m_InstPart = (InstrumentPart) MainForm.Entity;
		}

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			IEntityCollection ipHistory = InstrumentPartHistory.BuildHistory(EntityManager, TableNames.InstrumentPart,
			                                                                 m_InstPart.Identity);
			m_Form.InstrumentPartHistoryCollection.Publish(ipHistory);
		}

		#endregion
	}
}