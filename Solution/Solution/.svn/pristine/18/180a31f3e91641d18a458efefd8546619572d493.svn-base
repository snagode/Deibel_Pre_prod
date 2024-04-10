using System;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Library.ObjectModel;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Instrument Part Unavailable Task
	/// </summary>
	[SampleManagerTask("InstrumentPartUnavailableTask", "GENERAL", "INSTRUMENT")]
	public class InstrumentPartUnavailableTask : DefaultSingleEntityTask
	{
		#region Member Variables

		private FormInstrumentPartUnavailable m_Form;
		private InstrumentPart m_InstPart;

		#endregion

		#region Overrides

		/// <summary>
		/// Called to allow the consumer to extend the query
		/// </summary>
		/// <param name="query"></param>
		protected override void FindSingleEntityQuery(IQuery query)
		{
			query.AddEquals("AVAILABLE", true);

			query.AddAnd();

			query.AddEquals("RETIRED", false);
		}

		/// <summary>
		/// Return true if instrument requires servicing
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		protected override bool FindSingleEntityValidate(IEntity entity)
		{
			InstrumentPart instPart = (InstrumentPart) entity;

			if (instPart.Retired)
			{
				FormInstrumentPartUnavailable form =
					(FormInstrumentPartUnavailable) FormFactory.CreateForm(Context.TaskParameters[0]);
				Library.Utils.FlashMessage(form.StringTable.Retired, form.StringTable.RetiredHeader);

				return false;
			}

			if (!instPart.Available)
			{
				FormInstrumentPartUnavailable form =
					(FormInstrumentPartUnavailable) FormFactory.CreateForm(Context.TaskParameters[0]);
				Library.Utils.FlashMessage(form.StringTable.AlreadyUnavailable, form.StringTable.AlreadyUnavailableHeader);

				return false;
			}

			return true;
		}

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormInstrumentPartUnavailable) MainForm;
			m_InstPart = (InstrumentPart) MainForm.Entity;

			m_Form.PrintLabel.CheckedChanged += new EventHandler<CheckEventArgs>(PrintLabelCheckedChanged);
		}

		/// <summary>
		/// Called before the property sheet or wizard is saved.
		/// </summary>
		/// <returns>
		/// true to allow the save to continue, false to abort the save
		/// </returns>
		protected override bool OnPreSave()
		{
			EntityManager.Transaction.Add(m_InstPart);

			m_InstPart.HistoryComment = m_Form.Comment.Text;

			return true;
		}

		/// <summary>
		/// Called after the property sheet or wizard is saved.
		/// </summary>
		protected override void OnPostSave()
		{
			if (m_Form.PrintLabel.Checked)
			{
				LabelTemplateInternal labelTemplate = (LabelTemplateInternal) m_Form.Label.Entity;

				if (labelTemplate.IsNull())
				{
					Library.Utils.FlashMessage(m_Form.StringTable.NoLabel, m_Form.StringTable.NoLabelHeader);
				}
				else if (labelTemplate.LabelEntity != TableNames.InstrumentPartEvent)
				{
					Library.Utils.FlashMessage(m_Form.StringTable.WrongLabel, m_Form.StringTable.WrongLabelHeader);
				}
				else
				{
					Library.Utils.PrintLabel((LabelTemplateInternal) m_Form.Label.Entity,
					                         m_InstPart.InstrumentPartEvents.GetLast());
				}
			}
		}

		#endregion

		#region Print Label Support

		/// <summary>
		/// Print Label checked changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.CheckEventArgs"/> instance containing the event data.</param>
		private void PrintLabelCheckedChanged(object sender, CheckEventArgs e)
		{
			m_Form.Label.Enabled = e.Checked;
		}

		#endregion
	}
}