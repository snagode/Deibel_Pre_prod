using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Instrument History Task
	/// </summary>
	[SampleManagerTask("InstrumentHistoryTask", "GENERAL", "INSTRUMENT")]
	public class InstrumentHistoryTask : DefaultSingleEntityTask
	{
		#region Member Variables

		private FormInstrumentHistory m_Form;
		private Instrument m_Inst;

		#endregion

		#region Main Form Creation/Load

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormInstrumentHistory) MainForm;
			m_Inst = (Instrument) MainForm.Entity;
		}

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			IEntityCollection partsHistory = InstrumentPartLinkHistory.BuildHistory(EntityManager, TableNames.InstrumentPartLink,
			                                                                        m_Inst.Identity);
			m_Form.InstrumentPartLinkHistoryCollection.Publish(partsHistory);

			IEntityCollection instHistory = InstrumentHistory.BuildHistory(EntityManager, TableNames.Instrument, m_Inst.Identity);
			m_Form.InstrumentHistoryCollection.Publish(instHistory);
		}

		#endregion
	}
}