using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Windows.Forms;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.ClientControls.Validation;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of the Report LTE
	/// </summary>
	[SampleManagerTask("ReportTemplateTask", "LABTABLE", "REPORT_TEMPLATE")]
	public class ReportTemplateTask : GenericLabtableTask
	{
		#region Member Variables

		private FormReportTemplate m_FormReportTemplate;
		private ReportTemplate m_ReportTemplate;
		private IQuery m_PreviewQuery;

		#endregion

		#region Overridden Methods

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_ReportTemplate = (ReportTemplate) MainForm.Entity;
			m_FormReportTemplate = (FormReportTemplate) MainForm;

			m_FormReportTemplate.PrintTestButton.Click += PrintTestButtonClick;
			m_FormReportTemplate.DesignButton.Click += DesignButtonClick;
			m_FormReportTemplate.PrintTestCriteria.Click += new EventHandler(PrintTestCriteria_Click);
			m_FormReportTemplate.PromptEntityBrowseCriteria.EntityChanged += PromptEntityBrowseCriteria_EntityChanged;

			m_FormReportTemplate.RadioButtonCriteria.CheckedChanged += new EventHandler<CheckedChangedEventArgs>(RadioButtonCriteria_CheckedChanged);
			m_FormReportTemplate.RadioButtonEntity.CheckedChanged += new EventHandler<CheckedChangedEventArgs>(RadioButtonEntity_CheckedChanged);

			m_FormReportTemplate.PromptEntityBrowseTest.EntityChanged += PromptEntityBrowseTestEntityChanged;
			m_ReportTemplate.PropertyChanged += new PropertyEventHandler(ReportTemplatePropertyChanged);
			m_FormReportTemplate.DesignValidator.Validate += new ServerValidatorEventHandler(DesignValidatorValidate);
			m_FormReportTemplate.UseMasterTemplateConfig.CheckedChanged += UseMasterTemplateConfig_CheckedChanged;

		}


		/// <summary>
		/// Handles the StringChanged event of the PromptStringBrowseTemplateType control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
		private void PromptStringBrowseTemplateType_StringChanged(object sender, TextChangedEventArgs e)
		{
			bool master = (m_FormReportTemplate.PromptStringBrowseTemplateType.Text == "Master Report Template");
			m_FormReportTemplate.PanelTesting.Visible = !master;
			m_FormReportTemplate.PanelEntityLink.Visible = !master;
			m_FormReportTemplate.PanelTemplate.Visible = !master;
			m_ReportTemplate.IsTemplate = master;
			EnableDesignButton();
		}

		/// <summary>
		/// Handles the CheckedChanged event of the RadioButtonCriteria control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.CheckedChangedEventArgs"/> instance containing the event data.</param>
		private void RadioButtonCriteria_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			m_FormReportTemplate.PanelEntity.Visible = false;
			m_FormReportTemplate.PanelCriteria.Visible = true;

		}

		/// <summary>
		/// Handles the CheckedChanged event of the RadioButtonEntity control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.CheckedChangedEventArgs"/> instance containing the event data.</param>
		private void RadioButtonEntity_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			m_FormReportTemplate.PanelEntity.Visible = true;
			m_FormReportTemplate.PanelCriteria.Visible = false;
		}

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			m_FormReportTemplate.PromptStringBrowseTemplateType.StringChanged += new EventHandler<TextChangedEventArgs>(PromptStringBrowseTemplateType_StringChanged);
			m_FormReportTemplate.BrowseStringCollectionType.AddItem(
				m_FormReportTemplate.StringTableTemplateTypes.StandardReportTemplate);
			m_FormReportTemplate.BrowseStringCollectionType.AddItem(
				m_FormReportTemplate.StringTableTemplateTypes.MasterReportTemplate);
			if (m_ReportTemplate.IsTemplate)
			{
				m_FormReportTemplate.PromptStringBrowseTemplateType.Text = "Master Report Template";
			}
			else
			{
				m_FormReportTemplate.PromptStringBrowseTemplateType.Text = "Standard Report Template";
			}

			EnableDesignButton();
			EnablePrintButton();
			EnableCriteriaPrintButton();
			SetupTestDataBrowse();
			PopulateEntityTypes();

			m_FormReportTemplate.PromptEntityBrowseTest.ReadOnly = string.IsNullOrEmpty(m_ReportTemplate.DataEntityDefinition);
			m_FormReportTemplate.promptTableName.ReadOnly = !(Context.LaunchMode == AddOption || Context.LaunchMode == TestOption);

			if (m_ReportTemplate.ReportDefinition != null)
			{
				//m_FormReportTemplate.PromptEntityBrowseTemplate.Enabled = false;
				m_FormReportTemplate.PromptStringBrowseTemplateType.Enabled = false;
			}
		}

		/// <summary>
		/// Called before the property sheet or wizard is saved.
		/// </summary>
		/// <returns>
		/// true to allow the save to continue, false to abort the save.
		/// Please also ensure that you call the base.OnPreSave when continuing
		/// successfully.
		/// </returns>
		protected override bool OnPreSave()
		{
			bool preSave =  base.OnPreSave();

			if (!m_ReportTemplate.ApprovalStatus.IsPhrase(PhraseApprStat.PhraseIdA))
			{
				IQuery existsOnModQuery = EntityManager.CreateQuery(ModularReportItemBase.EntityName);
				existsOnModQuery.AddEquals(ModularReportItemPropertyNames.ReportTemplate,m_ReportTemplate);
				
				IEntityCollection modularReportItems = EntityManager.Select(existsOnModQuery);

				if (modularReportItems.Count > 0)
				{
					string authorizedMessage = string.Empty;
					var modularReports = new List<string>();

					foreach (ModularReportItemBase modularReportItem in modularReportItems)
					{
						if (modularReportItem.ModularReport.ApprovalStatus.IsPhrase(PhraseApprStat.PhraseIdA))
						{
							if (!modularReports.Contains(((IEntity)modularReportItem.ModularReport).IdentityString))
							{
								authorizedMessage = string.Format("{0} '{1}'", authorizedMessage, modularReportItem.ModularReport);
								modularReports.Add(((IEntity)modularReportItem.ModularReport).IdentityString);
							}
						}
					}

					if (!string.IsNullOrEmpty(authorizedMessage))
					{
						string warning = Library.Message.GetMessage("ReportTemplateMessages", "ReportTaskApprovalError", authorizedMessage);
						Library.Utils.FlashMessage(warning, m_FormReportTemplate.Title);
					}
				}
			}

			return preSave;
		}

		#endregion

		#region Button Events

		/// <summary>
		/// Handles the Click event of the m_DesignButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void DesignButtonClick(object sender, EventArgs e)
		{
			DesignReport();
		}

		/// <summary>
		/// Handles the Click event of the testButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void PrintTestButtonClick(object sender, EventArgs e)
		{

			PreviewReport(m_ReportTemplate.MasterTemplateLink);
		}

		/// <summary>
		/// Handles the Click event of the PrintTestCriteria control.
		/// </summary>0
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void PrintTestCriteria_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(m_ReportTemplate.Criteria)) return;
			//criteria
			ICriteriaTaskService criteriaTaskService =
				(ICriteriaTaskService) Library.GetService(typeof (ICriteriaTaskService));
			criteriaTaskService.QueryPopulated += CriteriaTaskService_QueryPopulated;
			criteriaTaskService.GetPopulatedCriteriaQuery(m_ReportTemplate.Criteria, m_ReportTemplate.DataEntityDefinition);

			PreviewReport(m_PreviewQuery, m_ReportTemplate.MasterTemplateLink);
		}

		/// <summary>
		/// Previews the report.
		/// </summary>
		/// <param name="masterTemplateEntity">The master template entity.</param>
		private void PreviewReport(IEntity masterTemplateEntity)
		{
			if (m_FormReportTemplate.PromptEntityBrowseTest.Entity == null) return;

			IEntity reportEntity = m_FormReportTemplate.PromptEntityBrowseTest.Entity;
			Library.Reporting.PreviewReport(m_ReportTemplate, reportEntity, new ReportOptions(), masterTemplateEntity);
		}

		/// <summary>
		/// Prints the report with query.
		/// </summary>
		private void PreviewReport(IQuery query, IEntity masterTemplateEntity)
		{
			if (query == null) return;
			IEntityCollection results = EntityManager.Select(query);
			Library.Reporting.PreviewReport(m_ReportTemplate, results, new ReportOptions(), masterTemplateEntity);
		}

		/// <summary>
		/// Designs the report.
		/// </summary>
		private void DesignReport()
		{
			bool radioSelected =
				m_FormReportTemplate.RadioButtonCriteria.Checked ||
				m_FormReportTemplate.RadioButtonEntity.Checked;

			if (m_ReportTemplate == null || !radioSelected) return;

			string previewId = string.Empty;

			if (m_ReportTemplate.Criteriasearch && m_ReportTemplate.Criteria != null)
			{
				previewId = m_ReportTemplate.Criteria;
			}
			else if (m_FormReportTemplate.PromptEntityBrowseTest.Entity != null)
			{
				previewId = m_FormReportTemplate.PromptEntityBrowseTest.Entity.IdentityString;
			}

			Library.Reporting.DesignReport(m_ReportTemplate, previewId);
		}

		/// <summary>
		/// Make sure a design is present
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The <see cref="Thermo.SampleManager.Library.ClientControls.Validation.ServerValidatorEventArgs"/> instance containing the event data.</param>
		private void DesignValidatorValidate(Object sender, ServerValidatorEventArgs args)
		{
			args.Valid = !string.IsNullOrEmpty(m_ReportTemplate.ReportDefinition);

			if (!args.Valid)
			{
				args.ErrorMessage = m_FormReportTemplate.StringTable.MissingDesignMessage;

				Library.Utils.FlashMessage(args.ErrorMessage, m_FormReportTemplate.StringTable.MissingDesignCaption,
					MessageButtons.OK, MessageIcon.Exclamation, MessageDefaultButton.Button1);
			}
		}

		#endregion

		#region Value Changing Events

		/// <summary>
		/// Handles the QueryPopulated event of the criteriaTaskService control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Server.CriteriaTaskQueryPopulatedEventArgs"/> instance containing the event data.</param>
		private void CriteriaTaskService_QueryPopulated(object sender, CriteriaTaskQueryPopulatedEventArgs e)
		{
			m_PreviewQuery = e.PopulatedQuery;
		}

		/// <summary>
		/// Reports the template property changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.PropertyEventArgs"/> instance containing the event data.</param>
		private void ReportTemplatePropertyChanged(object sender, PropertyEventArgs e)
		{
			if (e.PropertyName == ReportTemplatePropertyNames.DataEntityDefinition ||
			    e.PropertyName == ReportTemplatePropertyNames.Identity)
			{
				DefinitionChanged();
			}
		}

		/// <summary>
		/// Definition Changed
		/// </summary>
		private void DefinitionChanged()
		{
			EnableDesignButton();
			SetupTestDataBrowse();

			m_FormReportTemplate.PromptEntityBrowseTest.ReadOnly = string.IsNullOrEmpty(m_ReportTemplate.DataEntityDefinition);

			m_FormReportTemplate.PromptEntityBrowseTest.Entity = null;
		}

		/// <summary>
		/// Test Entity Changed
		/// </summary>  
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.EntityChangedEventArgs"/> instance containing the event data.</param>
		private void PromptEntityBrowseTestEntityChanged(object sender, EntityChangedEventArgs e)
		{
			EnablePrintButton();
			EnableDesignButton();
		}

		/// <summary>
		/// Handles the EntityChanged event of the PromptEntityBrowseCriteria control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.EntityChangedEventArgs"/> instance containing the event data.</param>
		private void PromptEntityBrowseCriteria_EntityChanged(object sender, EntityChangedEventArgs e)
		{
			EnableCriteriaPrintButton();
		}

		/// <summary>
		/// Handles the CheckedChanged event of the UseMasterTemplateConfig control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CheckEventArgs"/> instance containing the event data.</param>
		void UseMasterTemplateConfig_CheckedChanged(object sender, CheckEventArgs e)
		{
			m_FormReportTemplate.PromptEntityBrowseTemplate.Enabled = !e.Checked;
		}

		#endregion

		#region Browses

		/// <summary>
		/// Sets the browse by report property.
		/// </summary>
		private void SetupTestDataBrowse()
		{
			if (m_ReportTemplate == null) return;
			if (string.IsNullOrEmpty(m_ReportTemplate.DataEntityDefinition)) return;

			ISchemaTable table;
			bool isPhysicalTable = Schema.Current.Tables.TryGetValue(m_ReportTemplate.DataEntityDefinition, out table);
			if (!isPhysicalTable)
			{
				m_FormReportTemplate.PromptEntityBrowseTest.Enabled = false;
				m_FormReportTemplate.PromptEntityBrowseCriteria.Enabled = false;
				return;
			}

			m_FormReportTemplate.PromptEntityBrowseTest.Enabled = true;
			m_FormReportTemplate.PromptEntityBrowseCriteria.Enabled = true;

			m_FormReportTemplate.EntityBrowseTest.Republish(m_ReportTemplate.DataEntityDefinition);
		}

		/// <summary>
		/// Populates the entity types.
		/// </summary>
		private void PopulateEntityTypes()
		{
			List<string> entityTypes = new List<string>();
			foreach (string entityType in Schema.Current.EntityTypes.Keys)
			{
				entityTypes.Add(entityType);
			}

			entityTypes.Sort();

			m_FormReportTemplate.ReportDataTableName.Republish(entityTypes);

		}

		#endregion

		#region Button Control

		/// <summary>
		/// Enables the design button.
		/// </summary>
		private void EnableDesignButton()
		{
			// Display mode - no designing
			if (Context.LaunchMode == DisplayOption)
			{
				m_FormReportTemplate.DesignButton.Enabled = false;
				return;
			}

			string identity = m_ReportTemplate.Identity;

			//designing template - allow design regardless
			if (m_ReportTemplate.IsTemplate && !string.IsNullOrEmpty(identity))
			{
				m_FormReportTemplate.DesignButton.Enabled = true;
				return;
			}

			// No Entity/Identity/template
			string entityType = m_ReportTemplate.DataEntityDefinition;
			if ((string.IsNullOrEmpty(entityType)) || string.IsNullOrEmpty(identity))
			{
				m_FormReportTemplate.DesignButton.Enabled = false;
				return;
			}
			// Everything is good, allow edit.
			m_FormReportTemplate.DesignButton.Enabled = true;
		}

		/// <summary>
		/// Enables the print button.
		/// </summary>
		private void EnablePrintButton()
		{
			bool entitySelected = m_FormReportTemplate.PromptEntityBrowseTest.Entity != null;
			bool enabled = entitySelected && !string.IsNullOrEmpty(m_ReportTemplate.ReportDefinition);
			m_FormReportTemplate.PrintTestButton.Enabled = enabled;
		}

		/// <summary>
		/// Enables the print button.
		/// </summary>
		private void EnableCriteriaPrintButton()
		{
			bool entitySelected = !string.IsNullOrEmpty(m_ReportTemplate.Criteria);
			bool enabled = entitySelected && !string.IsNullOrEmpty(m_ReportTemplate.ReportDefinition);
			m_FormReportTemplate.PrintTestCriteria.Enabled = enabled;
		}

		#endregion
	}
}