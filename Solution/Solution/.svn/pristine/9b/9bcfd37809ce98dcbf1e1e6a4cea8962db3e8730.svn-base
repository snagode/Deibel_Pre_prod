using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	///  Run Modular Report
	/// </summary>
	[SampleManagerTask("ModularReportRunTask")]
	public class ModularReportRun : BaseReportingTask
	{
		/// <summary>
		/// Selected entity
		/// </summary>
		private ModularReport m_ModularReport;

		/// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			m_ModularReport = (ModularReport)Context.SelectedItems.GetFirst();

			ICriteriaTaskService criteriaTaskService = (ICriteriaTaskService)Library.GetService(typeof(ICriteriaTaskService));
			ISimplePromptService promptService =
				(ISimplePromptService)Library.GetService(typeof(ISimplePromptService));
			m_ReportOptions = new ReportOptions(ReportOutput.Preview);
			if (m_ModularReport == null)
			{
				IEntity reportOut;
				promptService.PromptForEntity(
						Library.Message.GetMessage("ReportTemplateMessages", "SelectMasterText"),
						Library.Message.GetMessage("ReportTemplateMessages", "SelectMasterHead"),
						ModularReport.EntityName,
						out reportOut);
				if (reportOut == null)
				{
					Exit();
					return;
				}
				m_ModularReport = (ModularReport)reportOut;
			}
			if (m_ModularReport == null)
			{
				Exit();
				return;
			}

			if (!ApprovedReport(m_ModularReport) && !Library.Environment.GetGlobalBoolean("SHOW_VERSIONS"))
			{
				Library.Utils.FlashMessage(Library.Message.GetMessage("ReportTemplateMessages", "ReportRunApprovalError"),
					Library.Message.GetMessage("ReportTemplateMessages", "DefaultLTEReportTitle", m_ModularReport.Name));
				return;
			}


			if (string.IsNullOrEmpty(m_ModularReport.Criteria)) //no default, prompt
			{
				IQuery criteriaQuery = EntityManager.CreateQuery(TableNames.CriteriaSaved);
				IEntity criteria;
				criteriaQuery.AddEquals(CriteriaSavedPropertyNames.TableName, m_ModularReport.EntityType);
				promptService.PromptForEntity(
					Library.Message.GetMessage("ReportTemplateMessages", "SelectCriteriaText"),
					Library.Message.GetMessage("ReportTemplateMessages", "SelectCriteriaHead"),
					criteriaQuery,
					out criteria);
				if (criteria == null)
				{
					Exit();
					return;
				}

				criteriaTaskService.QueryPopulated += criteriaTaskService_QueryPopulated;
				criteriaTaskService.GetPopulatedCriteriaQuery(criteria);
			}
			else
			{
				criteriaTaskService.QueryPopulated += criteriaTaskService_QueryPopulated;
				criteriaTaskService.GetPopulatedCriteriaQuery(m_ModularReport.Criteria, m_ModularReport.EntityType);
			}
		}

		/// <summary>
		/// Handles the QueryPopulated event of the criteriaTaskService control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Server.CriteriaTaskQueryPopulatedEventArgs"/> instance containing the event data.</param>
		private void criteriaTaskService_QueryPopulated(object sender, CriteriaTaskQueryPopulatedEventArgs e)
		{
			m_MasterTemplate = m_ModularReport.MasterTemplate;
			if (e.PopulatedQuery != null)
			{
				IEntity[] collection = m_ModularReport.ReportTemplates;
				m_ReportTemplateCollection = new ReportTemplate[collection.Length];
				int i = 0;
				foreach (var entity in collection)
				{
					var reportTemplate = (ReportTemplate)entity;
					m_ReportTemplateCollection[i] = reportTemplate;
					i++;
				}

				m_ReportData = EntityManager.Select(e.PopulatedQuery);
				ProduceReport();
			}
		}

		/// <summary>
		/// Approveds the report.
		/// </summary>
		/// <param name="modularReport">The modular report.</param>
		/// <returns></returns>
		private bool ApprovedReport(ModularReport modularReport)
		{
			if (!m_ModularReport.ApprovalStatus.IsPhrase(PhraseApprStat.PhraseIdA))
			{
				return false;
			}

			if (!m_ModularReport.Active)
			{
				return false;
			}

			foreach (IEntity reportTemplate in modularReport.ReportTemplates)
			{
				if (reportTemplate.GetString(ReportTemplatePropertyNames.ApprovalStatus) != PhraseApprStat.PhraseIdA)
				{
					return false;
				}

				if (!reportTemplate.GetBoolean(ReportTemplatePropertyNames.Active))
				{
					return false;
				}
			}

			return true;
		}
	}
}
