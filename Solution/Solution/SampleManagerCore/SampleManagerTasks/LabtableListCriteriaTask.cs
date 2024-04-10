
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Labtable List Task - with Criteria
	/// </summary>
	[SampleManagerTask("LabtableListCriteriaTask")]
	public class LabtableListCriteriaTask : BaseListPrintReportingTask
	{
		#region Member Variables

		private CriteriaSaved m_Criteria;

		#endregion

		#region Overrides

		/// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			base.SetupTask();

			ICriteriaTaskService criteriaTaskService = (ICriteriaTaskService) Library.GetService(typeof (ICriteriaTaskService));

			m_ReportLayout = (ReportLayoutHeader) EntityManager.Select(ReportLayoutHeaderBase.EntityName, Context.TaskParameters[0]);
			m_ReportOptions = new ReportOptions();

			m_IsPrint = false;
			m_LanguageSpecific = true;
			m_EntityType = Context.EntityType;

			if (Context.TaskParameters.Length == 2)
			{
				m_Criteria =
					(CriteriaSaved)
						EntityManager.Select(TableNames.CriteriaSaved,
							new Identity(m_ReportLayout.TableName, Context.TaskParameters[1]));
				if (m_Criteria != null)
				{
					criteriaTaskService.QueryPopulated += criteriaTaskService_QueryPopulated;
					criteriaTaskService.GetPopulatedCriteriaQuery(m_Criteria);
				}
			}
			else
			{
				IEntityCollection collection = null;

				if (Context.SelectedItems.Count == 0)
				{
					if (!string.IsNullOrEmpty(m_EntityType))
					{
						IQuery query = EntityManager.CreateQuery(Context.SelectedItems.EntityType);

						if (!Library.Environment.GetGlobalBoolean("OLEIMPRINT_LIST_REMOVED"))
						{
							query.HideRemoved();
						}
						collection = EntityManager.Select(query);
					}
				}
				else
				{
					collection = Context.SelectedItems;
				}

				if(collection!=null && collection.Count>0)
				{
					m_ReportData = collection;
					ProduceReport();
				}
				else
				{
					Library.Utils.FlashMessage(Library.Message.GetMessage("ReportTemplateMessages", "NoDataSelectedBody"),
						Library.Message.GetMessage("ReportTemplateMessages", "NoDataSelectedHead"));
				}

				Exit();
			}
		}

		#endregion

		#region Query Callbacks

		/// <summary>
		/// Handles the QueryPopulated event of the criteriaTaskService control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Server.CriteriaTaskQueryPopulatedEventArgs"/> instance containing the event data.</param>
		private void criteriaTaskService_QueryPopulated(object sender, CriteriaTaskQueryPopulatedEventArgs e)
		{
			IQuery populatedQuery = e.PopulatedQuery;

			if (populatedQuery != null) //cancelled
			{
				m_ReportData = EntityManager.Select(populatedQuery.TableName, populatedQuery);

				m_ReportOptions = new ReportOptions();
				ProduceReport();
			}

			Exit();
		}

		#endregion
	}
}

