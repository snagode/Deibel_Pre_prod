using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Worksheet Print Task - with Criteria
	/// </summary>
	[SampleManagerTask("WorksheetPrintCriteriaTask")]
	public class WorksheetPrintCriteriaTask : LabtablePrintCriteriaTask
	{
		/// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			IEntityCollection collection;
			
			//select worksheet from collection or prompt
			var promptService = (ISimplePromptService)Library.GetService(typeof(ISimplePromptService));
			if (Context.SelectedItems.Count == 0)
			{
				IEntity entity;
				promptService.PromptForEntity(
					Library.Message.GetMessage("ReportTemplateMessages", "CriteriaPrintSelectEntityHead"),
					Library.Message.GetMessage("ReportTemplateMessages", "CriteriaPrintSelectEntityBody"), Context.EntityType,
					out entity);
				collection = EntityManager.CreateEntityCollection(Context.EntityType);

				if (entity == null)
					return;

				collection.Add(entity);
			}
			else
			{
				collection = Context.SelectedItems;
			}

			//convert worksheets to types
			if (collection.GetFirst().EntityType == Worksheet.EntityName)
			{
				foreach (Worksheet worksheet in collection)
				{
					IEntity data = null;
					switch (worksheet.WorksheetType.PhraseId)
					{
						case PhraseWksType.PhraseIdSMPREPWKS:
							if (m_ReportData == null)
								m_ReportData = EntityManager.CreateEntityCollection(SamplePrepWorksheet.EntityName);

							data  = EntityManager.Select(SamplePrepWorksheet.EntityName, worksheet.Identity);

							if(data!=null)
								m_ReportData.Add(data);
							break;

						case PhraseWksType.PhraseIdSMPWKS:
							if (m_ReportData == null)
								m_ReportData = EntityManager.CreateEntityCollection(SampleWorksheet.EntityName);

							 data = EntityManager.Select(SampleWorksheet.EntityName, worksheet.Identity);

							if(data!=null)
								m_ReportData.Add(data);
							break;

						case PhraseWksType.PhraseIdTESTPREP:
							if (m_ReportData == null)
								m_ReportData = EntityManager.CreateEntityCollection(TestPreparationWorksheet.EntityName);

							data = EntityManager.Select(TestPreparationWorksheet.EntityName, worksheet.Identity);

							if(data!=null)
								m_ReportData.Add(data);
							break;

						case PhraseWksType.PhraseIdTESTWKS:
							if (m_ReportData == null)
								m_ReportData = EntityManager.CreateEntityCollection(AnalysisWorksheet.EntityName);

							data = EntityManager.Select(AnalysisWorksheet.EntityName, worksheet.Identity);

							if(data!=null)
								m_ReportData.Add(data);
							break;

						case PhraseWksType.PhraseIdUDWKS:
							if (m_ReportData == null)
								m_ReportData = EntityManager.CreateEntityCollection(UserWorksheet.EntityName);

							data = EntityManager.Select(UserWorksheet.EntityName, worksheet.Identity);

							if(data!=null)
								m_ReportData.Add(data);
							break;
					}
				}
			}
			else
			{
				m_ReportData = collection;
			}

			base.SetupTask();
		}
	}
}
