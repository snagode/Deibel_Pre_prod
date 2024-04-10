using System;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Instrument Calibration Task
	/// </summary>
	[SampleManagerTask("InstrumentCalibrationTask", "GENERAL", "INSTRUMENT")]
	public class InstrumentCalibrationTask : DefaultSingleEntityTask
	{
		#region Member Variables

		private FormInstrumentCalibration m_Form;
		private Instrument m_Inst;

		#endregion

		#region Constants

		private const int DisplayInstrumentPartMenuNumber = 15880;

		#endregion

		#region Overrides

		/// <summary>
		/// Called to allow the consumer to extend the query
		/// </summary>
		/// <param name="query"></param>
		protected override void FindSingleEntityQuery(IQuery query)
		{
			query.PushBracket();

			query.AddEquals("REQUIRES_CALIBRATION", true);

			query.AddAnd();

			query.AddEquals("RETIRED", false);

			query.AddAnd();

			query.PushBracket();

			query.AddEquals("CALIB_SAMPLE_TEMPLATE", "");

			query.AddOr();

			query.AddEquals("CALIBRATION_SAMPLE", "");

			query.AddOr();

			query.AddEquals("CALIBRATION_SAMPLE", PackedDecimal.FromInt32(0));

			query.PopBracket();
			query.PopBracket();
		}

		/// <summary>
		/// Called to validate the select entity
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		protected override bool FindSingleEntityValidate(IEntity entity)
		{
			Instrument inst = (Instrument) entity;

			if (inst.Retired)
			{
				FormInstrumentCalibration form = (FormInstrumentCalibration) FormFactory.CreateForm(Context.TaskParameters[0]);
				Library.Utils.FlashMessage(form.StringTable.Retired, form.StringTable.RetiredHeader);

				return false;
			}

			if (!inst.RequiresCalibration)
			{
				FormInstrumentCalibration form =
					(FormInstrumentCalibration) FormFactory.CreateForm(Context.TaskParameters[0]);
				Library.Utils.FlashMessage(form.StringTable.CalibrationNotReq, form.StringTable.CalibrationNotReqHeader);

				return false;
			}

			if (!inst.CalibSampleTemplate.IsNull() && (inst.InCalibration))
			{
				FormInstrumentCalibration form = (FormInstrumentCalibration) FormFactory.CreateForm(Context.TaskParameters[0]);
				Library.Utils.FlashMessage(form.StringTable.InCalibration, form.StringTable.InCalibrationHeader);

				return false;
			}

			return true;
		}

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormInstrumentCalibration) MainForm;
			m_Inst = (Instrument) MainForm.Entity;
		}

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			if (!m_Inst.CalibSampleTemplate.IsNull())
			{
				m_Form.Message.Visible = true;
				m_Form.CalibDate.Visible = false;
				m_Form.CalibrationComment.Visible = false;
				m_Form.CalibrationComment.Enabled = false;

				m_Form.OKButton.Visible = false;
				m_Form.OKButton.Enabled = false;
				m_Form.OKSample.Visible = true;
				m_Form.OKSample.Enabled = true;

				m_Form.OKSample.Click += new EventHandler(OkSampleClick);
			}
			else
			{
				m_Inst.LastCalibDate = Library.Environment.ClientNow;

				m_Form.Message.Visible = false;
				m_Form.CalibDate.Visible = true;
				m_Form.CalibrationComment.Enabled = true;
				m_Form.CalibrationComment.Visible = true;

				m_Form.OKButton.Visible = true;
				m_Form.OKButton.Enabled = true;
				m_Form.OKSample.Visible = false;
				m_Form.OKSample.Enabled = false;
			}

			ContextMenuItem menuItem = new ContextMenuItem("Display", "INT_DISPLAY");
			menuItem.ItemClicked += new ContextMenuItemClickedEventHandler(MenuItemDisplayClicked);

			m_Form.PartsGrid.ContextMenu.AddItem(menuItem);
		}

		#endregion

		#region Click Events

		/// <summary>
		/// Menus the item display clicked.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ContextMenuItemEventArgs"/> instance containing the event data.</param>
		private void MenuItemDisplayClicked(object sender, ContextMenuItemEventArgs e)
		{
			InstrumentPartLink partLink = ((InstrumentPartLink) m_Form.PartsGrid.FocusedEntity);

			if ((partLink != null) && (!partLink.InstrumentPart.IsNull()))
				Library.Task.CreateTask(DisplayInstrumentPartMenuNumber, partLink.InstrumentPart);
			else
				Library.Utils.FlashMessage(m_Form.StringTable.NoDisplayPart, m_Form.StringTable.NoDisplayPartHeader);
		}

		/// <summary>
		/// Ok click.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void OkSampleClick(object sender, EventArgs e)
		{
			if (!m_Inst.CalibSampleTemplate.IsNull())
			{
				SampleBase sample = LoginSample();

				if (sample != null)
				{
					EntityManager.Transaction.Add(m_Inst);

					string eventComment = string.Format(m_Form.StringTable.SampleCreated, sample.IdText,
					                                    sample.IdNumeric.ToString().Trim());

					m_Inst.AssignCalibrationSample(sample, eventComment);

					EntityManager.Commit();

					m_Form.Close();
				}
			}
		}

		#endregion

		#region Save

		/// <summary>
		/// Add a history comment to the instrument before its saved
		/// </summary>
		/// <returns></returns>
		protected override bool OnPreSave()
		{
			if (m_Inst.CalibSampleTemplate.IsNull())
			{
				EntityManager.Transaction.Add(m_Inst);

				m_Inst.CreateEvent(PhraseInstEvnt.PhraseIdCALIB,
				                   m_Form.CalibrationComment.Text,
				                   m_Inst.CalibDate);
			}

			return true;
		}

		#endregion

		#region Calibration Sample Handling

		/// <summary>
		/// Login the sample for the calibration
		/// </summary>
		/// <returns></returns>
		private SampleBase LoginSample()
		{
			object loginReturn = Library.VGL.RunVGLRoutineInteractive("$LIB_INSTRUMENT", "LIB_INSTRUMENT_LOGIN_SAMPLE",
			                                                          m_Inst.CalibSampleTemplate.Identity);

			if (loginReturn is PackedDecimal)
				return (SampleBase) EntityManager.Select(TableNames.Sample, new Identity(loginReturn));

			return null;
		}

		#endregion
	}
}