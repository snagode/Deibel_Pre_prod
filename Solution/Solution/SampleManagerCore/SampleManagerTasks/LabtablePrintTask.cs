using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Labtable Print Task
	/// </summary>
	[SampleManagerTask("LabtablePrintTask")]
	public class LabtablePrintTask : BaseListPrintReportingTask
	{
		#region Overrides

		/// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			base.SetupTask();

			m_IsPrint = true;
			m_LanguageSpecific = true;
			m_EntityType = Context.EntityType;

			var promptService = (ISimplePromptService)Library.GetService(typeof(ISimplePromptService));
			if (Context.SelectedItems.Count == 0)
			{
				IEntity entity;
				promptService.PromptForEntity(
					Library.Message.GetMessage("ReportTemplateMessages", "CriteriaPrintSelectEntityHead"),
					Library.Message.GetMessage("ReportTemplateMessages", "CriteriaPrintSelectEntityBody"), m_EntityType,
					out entity);
				m_ReportData = EntityManager.CreateEntityCollection(m_EntityType);
				if (entity != null)
				{
					m_ReportData.Add(entity);
				}
			}
			else
			{
				m_ReportData = Context.SelectedItems;
			}

			m_ReportOptions = new ReportOptions();
			ProduceReport();
			Exit();
		}

		#endregion
	}
}
