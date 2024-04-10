using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Instrument Part Available Task
	/// </summary>
	[SampleManagerTask("InstrumentPartAvailableTask", "GENERAL", "INSTRUMENT")]
	public class InstrumentPartAvailableTask : DefaultSingleEntityTask
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
			InstrumentPart instPart = (InstrumentPart) entity;

			if (instPart.Retired)
			{
				FormInstrumentPartAvailable form = (FormInstrumentPartAvailable) FormFactory.CreateForm(Context.TaskParameters[0]);
				Library.Utils.FlashMessage(form.StringTable.Retired, form.StringTable.RetiredHeader);

				return false;
			}

			if (instPart.Available)
			{
				FormInstrumentPartAvailable form = (FormInstrumentPartAvailable) FormFactory.CreateForm(Context.TaskParameters[0]);
				Library.Utils.FlashMessage(form.StringTable.AlreadyAvailable, form.StringTable.AlreadyAvailableHeader);

				return false;
			}

			return true;
		}

		#endregion

		#region Save

		/// <summary>
		/// Add a history comment to the instrument part before its saved
		/// </summary>
		/// <returns></returns>
		protected override bool OnPreSave()
		{
			InstrumentPart entity = (InstrumentPart) MainForm.Entity;

			EntityManager.Transaction.Add(entity);

			entity.HistoryComment = ((FormInstrumentPartAvailable) MainForm).Comment.Text;
			entity.Available = true;

			return true;
		}

		#endregion
	}
}