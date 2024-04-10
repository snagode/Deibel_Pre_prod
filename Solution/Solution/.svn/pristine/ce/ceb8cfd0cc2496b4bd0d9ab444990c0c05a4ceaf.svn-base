using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Labtable List Task
	/// </summary>
	[SampleManagerTask("LabtableListTask")]
	public class LabtableListTask : BaseListPrintReportingTask
	{
		#region Overrides

		/// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			base.SetupTask();

			m_IsPrint = false;
			m_LanguageSpecific = true;

			m_EntityType = Context.EntityType;
			IQuery query = EntityManager.CreateQuery(Context.SelectedItems.EntityType);

			if (!Library.Environment.GetGlobalBoolean("OLEIMPRINT_LIST_REMOVED"))
			{
				query.HideRemoved();
			}

			m_ReportData = EntityManager.Select(query);

			m_ReportOptions = new ReportOptions();

			ProduceReport();
			Exit();
		}

		#endregion
	}
}
