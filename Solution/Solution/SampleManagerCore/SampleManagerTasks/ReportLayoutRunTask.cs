using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// task for handling the list/print form
	/// </summary>
	[SampleManagerTask("ReportLayoutRunTask")]
	public class ReportLayoutRunTask : BaseListPrintReportingTask
	{
		/// <summary>
		/// Produces the report.(Master,Report)
		/// </summary>
		protected override void SetupTask()
		{
			base.SetupTask();
			IEntity selectedTemplate = Context.SelectedItems.GetFirst();

			ISimplePromptService promptService =
				(ISimplePromptService)Library.GetService(typeof(ISimplePromptService));
			if (Context.SelectedItems.Count == 0)
			{
				promptService.PromptForEntity(
					Library.Message.GetMessage("ReportTemplateMessages", "ReportLayoutRunPromptLayoutBody"),
					Library.Message.GetMessage("ReportTemplateMessages", "ReportLayoutRunPromptDataHead"),
					ReportLayoutHeader.EntityName,
					out selectedTemplate);
				if (selectedTemplate == null)
				{
					Exit();
					return;
				}
			}

			m_IsPrint = ((ReportLayoutHeader)selectedTemplate).OutputType.PhraseId == PhraseListprint.PhraseIdPRINT;
			m_EntityType = ((ReportLayoutHeader)selectedTemplate).TableName;
			m_ReportLayout = (ReportLayoutHeader)selectedTemplate;

			if (m_IsPrint)
			{
				IEntity selectedData;
				promptService.PromptForEntity(
					Library.Message.GetMessage("ReportTemplateMessages", "ReportLayoutRunPromptDataBody"),
					Library.Message.GetMessage("ReportTemplateMessages", "ReportLayoutRunPromptDataHead"), m_EntityType,
					out selectedData);
				if (selectedData == null) return;
				IEntityCollection collection = EntityManager.CreateEntityCollection(m_EntityType);
				collection.Add(selectedData);
				m_ReportData = collection;
			}
			else
			{

				IQuery query = EntityManager.CreateQuery(((ReportLayoutHeader)selectedTemplate).TableName);
				m_ReportData = EntityManager.Select(query);
			}

			m_ReportOptions = new ReportOptions();
			ProduceReport();
			Exit();
		}
	}
}
