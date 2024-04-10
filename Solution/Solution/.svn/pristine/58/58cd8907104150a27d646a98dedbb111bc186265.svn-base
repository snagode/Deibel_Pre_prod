using System;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Instrument Calibration Complete Task
	/// </summary>
	[SampleManagerTask("InstrumentCalibrationCompleteTask", "GENERAL", "INSTRUMENT")]
	public class InstrumentCalibrationCompleteTask : DefaultSingleEntityTask
	{
		#region Member Variables

		private FormInstrumentCalibrationComplete m_Form;
		private Instrument m_Inst;

		#endregion

		#region Overrrides

		/// <summary>
		/// Called to allow the consumer to extend the query
		/// </summary>
		/// <param name="query"></param>
		protected override void FindSingleEntityQuery(IQuery query)
		{
			query.PushBracket();

			query.AddEquals("REQUIRES_CALIBRATION", true);

			query.AddAnd();

			query.AddNotEquals("CALIB_SAMPLE_TEMPLATE", "");

			query.AddAnd();

			query.AddNotEquals("CALIBRATION_SAMPLE", "");

			query.AddAnd();

			query.AddNotEquals("CALIBRATION_SAMPLE", PackedDecimal.FromInt32(0));

			query.PopBracket();
		}

		/// <summary>
		/// Return true if instrument has a calibration templated and a current sample
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		protected override bool FindSingleEntityValidate(IEntity entity)
		{
			Instrument inst = (Instrument) entity;

			if (!inst.RequiresCalibration)
			{
				FormInstrumentCalibrationComplete form =
					(FormInstrumentCalibrationComplete) FormFactory.CreateForm(Context.TaskParameters[0]);
				Library.Utils.FlashMessage(form.StringTable.NotInCalibrationNotReq, form.StringTable.NotInCalibrationHeader);

				return false;
			}

			if (inst.CalibSampleTemplate.IsNull())
			{
				FormInstrumentCalibrationComplete form =
					(FormInstrumentCalibrationComplete) FormFactory.CreateForm(Context.TaskParameters[0]);
				Library.Utils.FlashMessage(form.StringTable.NotInCalibrationTemp, form.StringTable.NotInCalibrationHeader);

				return false;
			}

			if (!inst.InCalibration)
			{
				FormInstrumentCalibrationComplete form =
					(FormInstrumentCalibrationComplete) FormFactory.CreateForm(Context.TaskParameters[0]);
				Library.Utils.FlashMessage(form.StringTable.NotInCalibrationSamp, form.StringTable.NotInCalibrationHeader);

				return false;
			}

			return true;
		}

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormInstrumentCalibrationComplete) MainForm;
			m_Inst = (Instrument) MainForm.Entity;

			m_Form.ViewSample.Click += new EventHandler(ViewSampleClick);
		}

		/// <summary>
		/// Set the prompt contents
		/// </summary>
		protected override void MainFormLoaded()
		{
			if (!m_Inst.CalibrationSample.IsNull())
			{
				m_Form.Sample.Text = string.Format("{0} ({1})", m_Inst.CalibrationSample.IdText,
				                                   m_Inst.CalibrationSample.IdNumeric.ToString().Trim());
			}
			else
				m_Form.ViewSample.Enabled = false;
		}

		#endregion

		#region Click Events

		/// <summary>
		/// Display the sample
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ViewSampleClick(object sender, EventArgs e)
		{
			Library.VGL.RunVGLRoutineInteractive("$LIB_INSTRUMENT", "LIB_INSTRUMENT_SHOW_SAMPLE",
			                                     m_Inst.CalibrationSample.IdNumeric);
		}

		#endregion

		#region Save

		/// <summary>
		/// Add a history comment to the instrument before its saved
		/// </summary>
		/// <returns></returns>
		protected override bool OnPreSave()
		{
			bool ok = true;

			if (m_Inst.CalibrationSample == null)
				ok = Library.Utils.FlashMessageYesNo(m_Form.StringTable.NoSample, m_Form.StringTable.NoSampleHeader);
			else if (m_Inst.CalibrationSample.Status.PhraseId != PhraseSampStat.PhraseIdA)
			{
				ok = Library.Utils.FlashMessageYesNo(m_Form.StringTable.SampNotAuthorised,
				                                     m_Form.StringTable.SampNotAuthorisedHeader);
			}

			if (ok)
			{
				EntityManager.Transaction.Add(m_Inst);

				m_Inst.LastCalibDate = Library.Environment.ClientNow;
				m_Inst.SetStatus(PhraseInstStat.PhraseIdV);

				m_Inst.CreateEvent(PhraseInstEvnt.PhraseIdCALIB, m_Form.CalibrationComment.Text);

				if ((m_Inst.CalibrationSample != null) && (!m_Inst.CalibrationSample.IsNull()))
				{
					m_Inst.SampleResultsToProperties(m_Inst.CalibrationSample);
					m_Inst.CalibrationSample = null;
				}

				return true;
			}

			return false;
		}

		#endregion
	}
}