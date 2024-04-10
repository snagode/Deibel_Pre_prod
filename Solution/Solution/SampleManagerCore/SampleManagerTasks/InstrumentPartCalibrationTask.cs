using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Instrument Part Calibration Task
	/// </summary>
	[SampleManagerTask("InstrumentPartCalibrationTask", "GENERAL", "INSTRUMENT_PART")]
	public class InstrumentPartCalibrationTask : DefaultSingleEntityTask
	{
		#region Member Variables

		private FormInstrumentPartCalibration m_Form;
		private InstrumentPart m_InstPart;

		#endregion

		#region Overrides

		/// <summary>
		/// Called to allow the consumer to extend the query
		/// </summary>
		/// <param name="query"></param>
		protected override void FindSingleEntityQuery(IQuery query)
		{
			query.AddEquals("REQUIRES_CALIBRATION", true);

			query.AddAnd();

			query.AddEquals("RETIRED", false);
		}

		/// <summary>
		/// Called to validate the select entity
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		protected override bool FindSingleEntityValidate(IEntity entity)
		{
			InstrumentPart instPart = (InstrumentPart) entity;

			// Return true if instrument doesnt required a sample or 
			// if it does require a sample and there isnt one

			if (instPart.Retired)
			{
				FormInstrumentPartCalibration form =
					(FormInstrumentPartCalibration) FormFactory.CreateForm(Context.TaskParameters[0]);
				Library.Utils.FlashMessage(form.StringTable.Retired, form.StringTable.RetiredHeader);

				return false;
			}

			if (!instPart.RequiresCalibration)
			{
				FormInstrumentPartCalibration form =
					(FormInstrumentPartCalibration) FormFactory.CreateForm(Context.TaskParameters[0]);
				Library.Utils.FlashMessage(form.StringTable.CalibrationNotReq, form.StringTable.CalibrationNotReqHeader);

				return false;
			}

			return true;
		}

		#endregion

		#region Main Form Load/Creation

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormInstrumentPartCalibration) MainForm;
			m_InstPart = (InstrumentPart) MainForm.Entity;
		}

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			m_InstPart.LastCalibDate = Library.Environment.ClientNow;
		}

		#endregion

		#region Save

		/// <summary>
		/// Add a history comment to the instrument part before its saved
		/// </summary>
		/// <returns></returns>
		protected override bool OnPreSave()
		{
			EntityManager.Transaction.Add(m_InstPart);

			m_InstPart.CreateEvent(PhraseInstpEvnt.PhraseIdCALIB,
			                       m_Form.CalibrationComment.Text,
			                       m_InstPart.LastCalibDate);

			return true;
		}

		#endregion
	}
}