using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Instrument Part Service Task
	/// </summary>
	[SampleManagerTask("InstrumentPartServiceTask", "GENERAL", "INSTRUMENT_PART")]
	public class InstrumentPartServiceTask : DefaultSingleEntityTask
	{
		#region Member Variables

		private FormInstrumentPartService m_Form;
		private InstrumentPart m_InstPart;

		#endregion

		#region Overrides

		/// <summary>
		/// Called to allow the consumer to extend the query
		/// </summary>
		/// <param name="query"></param>
		protected override void FindSingleEntityQuery(IQuery query)
		{
			query.AddEquals("REQUIRES_SERVICING", true);

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

			if (instPart.Retired)
			{
				FormInstrumentPartService form = (FormInstrumentPartService) FormFactory.CreateForm(Context.TaskParameters[0]);
				Library.Utils.FlashMessage(form.StringTable.Retired, form.StringTable.RetiredHeader);

				return false;
			}

			if (!instPart.RequiresServicing)
			{
				FormInstrumentPartService form =
					(FormInstrumentPartService) FormFactory.CreateForm(Context.TaskParameters[0]);
				Library.Utils.FlashMessage(form.StringTable.ServiceNotReq, form.StringTable.ServiceNotReqHeader);

				return false;
			}

			return true;
		}

		#endregion

		#region Main Form Creation/Load

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormInstrumentPartService) MainForm;
			m_InstPart = (InstrumentPart) MainForm.Entity;
		}

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			m_InstPart.LastServiceDate = Library.Environment.ClientNow;
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
			EntityManager.Transaction.Add(m_InstPart);

			m_InstPart.CreateEvent(PhraseInstpEvnt.PhraseIdSERVICE,
			                       m_Form.ServiceComment.Text,
			                       m_InstPart.LastServiceDate);

			return true;
		}

		#endregion
	}
}