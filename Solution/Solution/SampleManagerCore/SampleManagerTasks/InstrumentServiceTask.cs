using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Instrument Service Task
	/// </summary>
	[SampleManagerTask("InstrumentServiceTask", "GENERAL", "INSTRUMENT")]
	internal class InstrumentServiceTask : DefaultSingleEntityTask
	{
		#region Member Variables

		private FormInstrumentService m_Form;
		private Instrument m_Inst;

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
		/// Return true if instrument requires servicing
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		protected override bool FindSingleEntityValidate(IEntity entity)
		{
			Instrument inst = (Instrument) entity;

			if (inst.Retired)
			{
				FormInstrumentService form = (FormInstrumentService) FormFactory.CreateForm(Context.TaskParameters[0]);
				Library.Utils.FlashMessage(form.StringTable.Retired, form.StringTable.RetiredHeader);

				return false;
			}

			if (!inst.RequiresServicing)
			{
				FormInstrumentService form =
					(FormInstrumentService) FormFactory.CreateForm(Context.TaskParameters[0]);
				Library.Utils.FlashMessage(form.StringTable.NoService, form.StringTable.NoServiceHeader);

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
			m_Form = (FormInstrumentService) MainForm;
			m_Inst = (Instrument) MainForm.Entity;
		}

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			m_Inst.LastServiceDate = Library.Environment.ClientNow;
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
			EntityManager.Transaction.Add(m_Inst);

			m_Inst.CreateEvent(PhraseInstEvnt.PhraseIdSERVICE,
			                   m_Form.ServiceComment.Text,
			                   m_Inst.LastServiceDate);

			return true;
		}

		#endregion
	}
}