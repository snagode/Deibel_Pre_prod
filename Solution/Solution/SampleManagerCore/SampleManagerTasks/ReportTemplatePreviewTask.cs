using System;
using System.Globalization;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// ReportTemplatePreviewTask
	/// </summary>
	[SampleManagerTask("ReportTemplatePreviewTask", "GENERAL", "REPORT_TEMPLATE")]
	public class ReportTemplatePreviewTask : DefaultSingleEntityTask
	{
		#region Member Variables

		/// <summary>
		/// Main Form
		/// </summary>
		/// 
		private FormReportTemplatePreview m_Form;

		/// <summary>
		/// Selected entity
		/// </summary>
		private ReportTemplate m_ReportTemplate;

		#endregion

		#region Overrides

		/// <summary>
		/// Called to validate the select entity
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		protected override bool FindSingleEntityValidate(IEntity entity)
		{
			if (!ApprovedReport((ReportTemplate)entity))
			{
				Library.Utils.FlashMessage(Library.Message.GetMessage("ReportTemplateMessages", "ReportRunApprovalError"),
					Library.Message.GetMessage("ReportTemplateMessages", "DefaultLTEReportTitle",
						entity.Name));
				return false;
			}
			return true;
		}

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			m_ReportTemplate = (ReportTemplate) MainForm.Entity;
			m_Form = (FormReportTemplatePreview)MainForm;

			PopulateBrowse(m_ReportTemplate.EntityType.Name);
			m_Form.ButtonRun.Click += ButtonRun_Click;
			base.MainFormLoaded();
		}

		#endregion

		#region Events

		/// <summary>
		/// Handles the Click event of the ButtonRun control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		void ButtonRun_Click(object sender, EventArgs e)
		{
			if (m_ReportTemplate.Criteriasearch)
			{
				CriteriaSaved criteria = (CriteriaSaved) m_Form.PromptCriteria.Entity;
				ICriteriaTaskService criteriaTaskService = (ICriteriaTaskService) Library.GetService(typeof (ICriteriaTaskService));
				if (criteria != null)
				{
					criteriaTaskService.QueryPopulated += criteriaTaskService_QueryPopulated;
					criteriaTaskService.GetPopulatedCriteriaQuery(criteria);
				}
			}
			else
			{
				IEntity data = m_Form.PromptCriteria.Entity;
				ReportTemplate masterTemplate = (ReportTemplate)m_Form.PromptMaster.Entity;
				Library.Reporting.PreviewReport(m_ReportTemplate, data, new ReportOptions(), masterTemplate);
			}
		}

		/// <summary>
		/// Handles the QueryPopulated event of the criteriaTaskService control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Server.CriteriaTaskQueryPopulatedEventArgs"/> instance containing the event data.</param>
		private void criteriaTaskService_QueryPopulated(object sender, CriteriaTaskQueryPopulatedEventArgs e)
		{
			ReportTemplate masterTemplate = (ReportTemplate)m_Form.PromptMaster.Entity;

			if (e.PopulatedQuery != null)
			{
				IEntityCollection data = EntityManager.Select(e.PopulatedQuery);
				Library.Reporting.PreviewReport(m_ReportTemplate, data, new ReportOptions(), masterTemplate);
			}

			m_Form.Close();
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Populates the browse.
		/// </summary>
		/// <param name="entityType">Type of the entity.</param>
		private void PopulateBrowse(string entityType)
		{
			m_Form.IconName= m_ReportTemplate.EntityType.DefaultIcon;
			IconName iconName = new IconName( m_ReportTemplate.EntityType.DefaultIcon);
			m_Form.PictureBox1.SetImageByIconName(iconName);

			if (m_ReportTemplate.Criteriasearch)
			{
				IQuery reportCriteriaQuery = EntityManager.CreateQuery(CriteriaSavedBase.EntityName);
				reportCriteriaQuery.AddEquals(CriteriaSavedPropertyNames.TableName, entityType);
				m_Form.EntityBrowseCriteria.Republish(reportCriteriaQuery);
			}
			else
			{
				m_Form.EntityBrowseCriteria.Republish(entityType);
				TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
				m_Form.PromptCriteria.Caption = textInfo.ToTitleCase(entityType.Replace("_"," ").ToLower());
			}

			m_Form.PromptMaster.Entity = m_ReportTemplate.MasterTemplateLink;
			
			if (m_ReportTemplate.UseMasterTemplateConfig)
			{
				m_Form.PromptMaster.Entity = m_ReportTemplate.GlobalMasterTemplate;
			}
			m_Form.EntityBrowseMaster.Republish(m_Form.DataQueryMasters.ResultData);
		}


		/// <summary>
		/// Approveds the report.
		/// </summary>
		/// <param name="reportReport">The report report.</param>
		/// <returns></returns>
		private bool ApprovedReport(ReportTemplate reportReport)
		{
			if (!reportReport.ApprovalStatus.IsPhrase(PhraseApprStat.PhraseIdA))
			{
				return false;
			}

			if (!reportReport.Active)
			{
				return false;
			}

			return true;
		}


		#endregion
	}
}
