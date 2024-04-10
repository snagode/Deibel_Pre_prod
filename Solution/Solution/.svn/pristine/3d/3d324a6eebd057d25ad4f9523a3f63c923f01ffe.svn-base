using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Instrument Available Task
	/// </summary>
	[SampleManagerTask("InstrumentAvailableTask", "GENERAL", "INSTRUMENT")]
	public class InstrumentAvailableTask : DefaultSingleEntityTask
	{
		#region Overrides

		/// <summary>
		/// Called to allow the consumer to extend the query
		/// </summary>
		/// <param name="query"></param>
		protected override void FindSingleEntityQuery(IQuery query)
		{
			query.AddEquals("AVAILABLE", false);

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
				FormInstrumentAvailable form = (FormInstrumentAvailable) FormFactory.CreateForm(Context.TaskParameters[0]);
				Library.Utils.FlashMessage(form.StringTable.Retired, form.StringTable.RetiredHeader);

				return false;
			}

			if (inst.Available)
			{
				FormInstrumentAvailable form = (FormInstrumentAvailable) FormFactory.CreateForm(Context.TaskParameters[0]);
				Library.Utils.FlashMessage(form.StringTable.AlreadyAvailable, form.StringTable.AlreadyAvailableHeader);

				return false;
			}

			return true;
		}

		#endregion

		#region Save

		/// <summary>
		/// Add a history comment to the instrument before its saved
		/// </summary>
		/// <returns></returns>
		protected override bool OnPreSave()
		{
			Instrument inst = (Instrument) MainForm.Entity;

			EntityManager.Transaction.Add(inst);

			inst.HistoryComment = ((FormInstrumentAvailable) MainForm).Comment.Text;
			inst.Available = true;

			return true;
		}

		#endregion
	}
}