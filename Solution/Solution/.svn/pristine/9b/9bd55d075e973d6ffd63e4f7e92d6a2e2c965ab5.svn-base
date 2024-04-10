using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Instrument Part Comment Task
	/// </summary>
	[SampleManagerTask("InstrumentPartCommentTask", "GENERAL", "INSTRUMENT")]
	public class InstrumentPartCommentTask : DefaultSingleEntityTask
	{
		#region Member Variables

		private FormInstrumentPartComment m_Form;
		private InstrumentPart m_InstPart;

		#endregion

		#region Main Form Created

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormInstrumentPartComment) MainForm;
			m_InstPart = (InstrumentPart) MainForm.Entity;
		}

		#endregion

		#region Save

		/// <summary>
		/// Called before the property sheet or wizard is saved.
		/// </summary>
		/// <returns>
		/// true to allow the save to continue, false to abort the save
		/// </returns>
		protected override bool OnPreSave()
		{
			m_InstPart.CreateEvent(PhraseInstpEvnt.PhraseIdCOMMENT, m_Form.Comment.Text);

			return true;
		}

		#endregion
	}
}