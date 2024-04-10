using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Instrument Comment Task
	/// </summary>
	[SampleManagerTask("InstrumentCommentTask", "GENERAL", "INSTRUMENT")]
	public class InstrumentCommentTask : DefaultSingleEntityTask
	{
		#region Member Variables

		private FormInstrumentComment m_Form;
		private Instrument m_Inst;

		#endregion

		#region Main Form Created

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormInstrumentComment) MainForm;
			m_Inst = (Instrument) MainForm.Entity;
		}

		#endregion

		#region Save

		/// <summary>
		/// Add a history comment to the instrument before its saved
		/// </summary>
		/// <returns></returns>
		protected override bool OnPreSave()
		{
			m_Inst.CreateEvent(PhraseInstEvnt.PhraseIdCOMMENT, m_Form.Comment.Text);

			return true;
		}

		#endregion
	}
}