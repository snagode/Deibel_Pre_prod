using System;
using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Library.ObjectModel;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Modular Reporting Task
	/// </summary>
	[SampleManagerTask("ModularReportTask")]
	public class ModularReportTask : GenericLabtableTask
	{
		#region Member Variables

		/// <summary>
		/// m_ form
		/// </summary>
		public FormModularReport m_Form;

		/// <summary>
		/// m_ entity
		/// </summary>
		public ModularReport m_Entity;

		/// <summary>
		/// The m_ catch repeating events
		/// </summary>
		private bool m_CatchRepeatingEvents;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormModularReport) MainForm;
			m_Entity = (ModularReport) m_Form.Entity;
			m_Form.UseMasterTemplateConfig.CheckedChanged += UseMasterTemplateConfig_CheckedChanged;
		}

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			m_Form.EntityType.TableNameChanged += EntityType_TableNameChanged;
			m_Form.ButtonPreview.Click += ButtonPreview_Click;
			m_Form.ModularReportItem.Columns[0].ValueChanged += ModularReportTask_ValueChanged;

			PopulateReportTemplates();
			m_Form.ModularReportItem.CellValueChanged += ModularReportItem_CellValueChanged;
			m_Form.ModularReportItem.RowAdded += ModularReportItem_RowAdded;
			m_Form.ModularReportItem.RowMoved += ModularReportItem_RowMoved;
			m_Form.ModularReportItem.RowRemoved += ModularReportItem_RowRemoved;

			if (Context.LaunchMode == ModifyOption)
				m_Form.EntityType.Enabled = false;

			RepublishAvailReports();
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
			m_Entity.ModularReportItems.RemoveAll();

			int i = 1;
			foreach (UnboundGridRow linkRow in m_Form.ModularReportItem.Rows)
			{
				ModularReportItemBase link = (ModularReportItemBase) EntityManager.CreateEntity(ModularReportItemBase.EntityName);
				link.ModularReport = m_Entity;
				link.OrderNum = i;
				if (linkRow["ReportTemplate"] is IEntity)
				{
					link.ReportTemplateId = ((ReportTemplateBase) linkRow["ReportTemplate"]).Identity;
				}
				else if (linkRow["ReportTemplate"] is string)
				{
					link.ReportTemplateId = linkRow["ReportTemplate"] as string;
				}
				link.ReportTemplateVersion = linkRow["ReportTemplateVersion"] as PackedDecimal;
				m_Entity.ModularReportItems.Add(link);
				i++;
			}

			return base.OnPreSave();
		}

		/// <summary>
		/// Checks if the modular report is appropriate for mode.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="modeToCheck"></param>
		/// <returns></returns>
		protected override bool EntityIsAppropriateForMode(IEntity entity, string modeToCheck)
		{
			if (!base.EntityIsAppropriateForMode(entity, modeToCheck)) return false;

			if (modeToCheck == ApproveOption)
			{
				if (ReportTemplatesNeedApproval(entity))
				{
					EntityError = Library.Message.GetMessage("ReportTemplateMessages", "ModularReportErrorInactiveChildren", entity.Name);
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Submit for Approval
		/// </summary>
		override protected bool ShouldSubmitOnSave(IEntity entity)
		{
			if (ReportTemplatesNeedApproval(entity))
			{
				Library.Utils.FlashMessage(
					Library.Message.GetMessage("ReportTemplateMessages", "ModularReportErrorInactiveChildren", entity.Name),
					MainForm.Title);

				return false;
			}

			return base.ShouldSubmitOnSave(entity);
		}

		#endregion

		#region Events

		/// <summary>
		/// Handles the RowRemoved event of the ModularReportItem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.UnboundGridRowEventArgs"/> instance containing the event data.</param>
		private void ModularReportItem_RowRemoved(object sender, UnboundGridRowEventArgs e)
		{
			Library.Task.StateModified();
		}

		/// <summary>
		/// Handles the RowMoved event of the ModularReportItem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.UnboundGridRowMovedEventArgs"/> instance containing the event data.</param>
		private void ModularReportItem_RowMoved(object sender, UnboundGridRowMovedEventArgs e)
		{
			Library.Task.StateModified();
		}

		/// <summary>
		/// Handles the RowAdded event of the ModularReportItem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.UnboundGridRowAddedEventArgs"/> instance containing the event data.</param>
		private void ModularReportItem_RowAdded(object sender, UnboundGridRowAddedEventArgs e)
		{
			Library.Task.StateModified();
		}

		/// <summary>
		/// Handles the CellValueChanged event of the ModularReportItem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.UnboundGridValueChangedEventArgs"/> instance containing the event data.</param>
		private void ModularReportItem_CellValueChanged(object sender, UnboundGridValueChangedEventArgs e)
		{
			Library.Task.StateModified();
		}

		/// <summary>
		/// Populates the report templates.
		/// </summary>
		private void PopulateReportTemplates()
		{
			m_Form.ModularReportItem.ClearRows();
			foreach (ModularReportItem modularReportItem in m_Entity.ModularReportItems)
			{
				m_Form.ModularReportItem.AddRow(modularReportItem.ReportTemplate.Identity, modularReportItem.ReportTemplateVersion, modularReportItem.ReportTemplate.ApprovalStatus.PhraseText);
			}
		}

		/// <summary>
		/// Handles the ValueChanged event of the ModularReportTask control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.UnboundGridValueChangedEventArgs"/> instance containing the event data.</param>
		private void ModularReportTask_ValueChanged(object sender, Thermo.SampleManager.Library.ClientControls.UnboundGridValueChangedEventArgs e)
		{
			ReportTemplate report = e.Value as ReportTemplate;
			if (report != null)
			{
				e.Row.SetValue("ReportTemplateVersion", report.Version);
				e.Row.SetValue("ApprovalStatus", report.ApprovalStatus.PhraseText);
			}
		}

		/// <summary>
		/// Handles the CheckedChanged event of the UseMasterTemplateConfig control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.CheckEventArgs"/> instance containing the event data.</param>
		private void UseMasterTemplateConfig_CheckedChanged(object sender, Thermo.SampleManager.Library.ClientControls.CheckEventArgs e)
		{
			m_Form.MasterTemplate.Enabled = !e.Checked;
		}

		/// <summary>
		/// Handles the Click event of the ButtonPreview control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void ButtonPreview_Click(object sender, EventArgs e)
		{
			m_CatchRepeatingEvents = false;
			string criteria = m_Entity.Criteria;
			ICriteriaTaskService criteriaTaskService = (ICriteriaTaskService) Library.GetService(typeof (ICriteriaTaskService));
			if (!string.IsNullOrEmpty(criteria))
			{
				criteriaTaskService.QueryPopulated +=
					new CriteriaTaskQueryPopulatedEventHandler(CriteriaTaskService_QueryPopulated);
				criteriaTaskService.GetPopulatedCriteriaQuery(criteria, m_Form.EntityType.TableName);
			}
			else
			{
				Library.Utils.FlashMessage(Library.Message.GetMessage("ReportTemplateMessages", "NoDataSelectedBody"),
					Library.Message.GetMessage("ReportTemplateMessages", "NoDataSelectedHead"));
			}
		}

		/// <summary>
		/// Handles the QueryPopulated event of the criteriaTaskService control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Server.CriteriaTaskQueryPopulatedEventArgs"/> instance containing the event data.</param>
		private void CriteriaTaskService_QueryPopulated(object sender, CriteriaTaskQueryPopulatedEventArgs e)
		{
			if (!m_CatchRepeatingEvents)
				PreviewReport(e.PopulatedQuery);

			m_CatchRepeatingEvents = true;
		}

		/// <summary>
		/// Handles the TableNameChanged event of the EntityType control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.TextChangedEventArgs"/> instance containing the event data.</param>
		private void EntityType_TableNameChanged(object sender,
			Thermo.SampleManager.Library.ClientControls.TextChangedEventArgs e)
		{
			RepublishAvailReports();
		}

		/// <summary>
		/// Prints the report with query.
		/// </summary>
		private void PreviewReport(IQuery query)
		{
			if (query == null)
			{
				Library.Utils.FlashMessage(Library.Message.GetMessage("ReportTemplateMessages", "NoDataSelectedBody"),
					Library.Message.GetMessage("ReportTemplateMessages", "NoDataSelectedHead"));
			}
			IEntityCollection data = EntityManager.Select(query);
			List<ReportTemplateInternal> reports = new List<ReportTemplateInternal>();
			foreach (UnboundGridRow linkRow in m_Form.ModularReportItem.Rows)
			{
				string identity=string.Empty;

				if(linkRow["ReportTemplate"] is IEntity)
					identity = ((ReportTemplateBase)linkRow["ReportTemplate"]).Identity;
				if (linkRow["ReportTemplate"] is string)
					identity = linkRow["ReportTemplate"] as string;

				if(identity!=string.Empty)
					reports.Add( (ReportTemplate) EntityManager.Select(ReportTemplate.EntityName, new Identity(identity,
						linkRow["ReportTemplateVersion"] as PackedDecimal
						)));
			}
			
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add("paramReportName", m_Entity.Name);

			if(reports.Count!=0)
				Library.Reporting.PreviewReport(reports.ToArray(), data, new ReportOptions(parameters), m_Entity.MasterTemplate);
		}

		/// <summary>
		/// Republishes the avail reports.
		/// </summary>
		private void RepublishAvailReports()
		{
			if (string.IsNullOrEmpty(m_Form.EntityType.TableName)) return;

			m_Form.ModularReportItem.Enabled = true;

			IQuery queryAvailReports = EntityManager.CreateQuery(ReportTemplate.EntityName);
			queryAvailReports.AddEquals(ReportTemplatePropertyNames.DataEntityDefinition, m_Form.EntityType.TableName);
			queryAvailReports.AddEquals(ReportTemplatePropertyNames.IsTemplate, false);

			IEntityCollection collection = EntityManager.Select(queryAvailReports);

			m_Form.EntityBrowseAvailableReportTemplates.ShowVersions = TriState.Yes;
			m_Form.EntityBrowseAvailableReportTemplates.Republish(collection);
		}

		#endregion

		#region Report Templates

		private bool ReportTemplatesNeedApproval(IEntity entity)
		{
			foreach (var reportTemplateEntity in ((ModularReport)entity).ReportTemplates)
			{
				ReportTemplate reportTemplate = reportTemplateEntity as ReportTemplate;

				if ((reportTemplate == null) || (!IsApprovalAuthorised(reportTemplate)) || (!IsActive(reportTemplate)))
				{
					return true;
				}
			}

			return false;
		}

		#endregion
	}
}