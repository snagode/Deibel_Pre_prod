using System.Collections.Generic;
using System.Linq;
using Thermo.SampleManager.Common;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Utilities;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Report Layout Task
	/// </summary>
	[SampleManagerTask("ReportLayoutHeaderTask", "LABTABLE", ReportLayoutHeaderBase.EntityName)]
	public class ReportLayoutHeaderTask : GenericLabtableTask
	{
		#region Constants

		private const string ColumnInclude = "Include";
		private const string ColumnSubReport = "SubReport";
		private const string ColumnTableName = "TableName";
		private const string ColumnRelationship = "RelationshipName";

		#endregion

		#region Member Variables

		private FormReportLayout m_Form;
		private ReportLayoutHeader m_Entity;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm" /> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			m_Form = (FormReportLayout) MainForm;
			m_Entity = (ReportLayoutHeader) MainForm.Entity;

			ShowHideTabs();
			m_Form.MasterTemplate.Enabled = !m_Form.UseMasterTemplateConfig.Checked;

			m_Form.GridFields.Columns[0].SetCellBrowse(m_Entity, BrowseFactory.CreatePropertyNameBrowse(m_Entity.TableName, PropertyFilter.NonCollection, true));

			m_Form.TableNames.Republish(new List<string>(Schema.Current.EntityTypes.Keys.OrderBy(p => p.Substring(0))));

			LoadListPrintLinks();
			SetEntityName();

			m_Form.EntityName.StringChanged += EntityName_StringChanged;

			m_Form.GridLinks.FocusedColumnChanged += GridLinks_FocusedColumnChanged;
			m_Form.GridLinks.FocusedRowChanged += GridLinks_FocusedRowChanged;
			m_Form.GridLinks.CellValueChanged += GridLinks_CellValueChanged;
			m_Form.GridFields.ValidateCell += GridFields_ValidateCell;
			m_Form.UseMasterTemplateConfig.CheckedChanged += UseMasterTemplateConfig_CheckedChanged;
			m_Entity.PropertyChanged += m_Entity_PropertyChanged;
		}

		void UseMasterTemplateConfig_CheckedChanged(object sender, CheckEventArgs e)
		{
			m_Form.MasterTemplate.Enabled = !e.Checked;
		}

		#endregion

		#region Output Type Handling

		/// <summary>
		/// Handles the PropertyChanged event of the m_Entity control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PropertyEventArgs"/> instance containing the event data.</param>
		private void m_Entity_PropertyChanged(object sender, PropertyEventArgs e)
		{
			if (e.PropertyName == ReportLayoutHeaderPropertyNames.OutputType)
			{
				ShowHideTabs();
			}
		}

		/// <summary>
		/// Shows the hide tabs.
		/// </summary>
		private void ShowHideTabs()
		{
			if (m_Entity.OutputType.PhraseId == PhraseListprint.PhraseIdPRINT)
			{
				m_Form.Page_Links.Visible = true;
			}
			else
			{
				m_Form.Page_Links.Visible = false;
			}
		}

		#endregion

		#region Form Handling

		/// <summary>
		/// Loads the list print links.
		/// </summary>
		private void LoadListPrintLinks()
		{
			m_Form.GridLinks.ClearRows();

			foreach (ReportLayoutLink reportLayoutHeader in m_Entity.ReportLayoutLinks)
			{
				var row = m_Form.GridLinks.AddRow();

				row.SetValue(ColumnInclude, true);
				row.SetValue(ColumnTableName, reportLayoutHeader.TableName);
				row.SetValue(ColumnRelationship, reportLayoutHeader.RelationshipName);
				row.SetValue(ColumnSubReport, reportLayoutHeader.ReportLayoutHeaderLink);
			}
		}

		/// <summary>
		/// Handles the ValidateCell event of the GridFields control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="DataGridValidateCellEventArgs"/> instance containing the event data.</param>
		private void GridFields_ValidateCell(object sender, DataGridValidateCellEventArgs e)
		{
			if (e.Column == m_Form.GridFields.Columns[0])
			{
				var field = e.Entity as ReportLayoutFieldBase;
				if (field == null) return;

				if (string.IsNullOrWhiteSpace(field.Description))
				{
					field.Description = TextUtils.BreakUpPascalCase((string) e.Value);
				}
			}
		}

		/// <summary>
		/// Handles the CellValueChanged event of the GridListprintLinks control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="UnboundGridValueChangedEventArgs"/> instance containing the event data.</param>
		private void GridLinks_CellValueChanged(object sender, UnboundGridValueChangedEventArgs e)
		{
			Library.Task.StateModified();
		}

		/// <summary>
		/// Republishes the available links.
		/// </summary>
		private void RepublishAvailableLinks()
		{
			if (m_Form.GridLinks.FocusedRow == null) return;

			IQuery query = EntityManager.CreateQuery(ReportLayoutHeaderBase.EntityName);
			query.AddEquals(ReportLayoutHeaderPropertyNames.TableName, m_Form.GridLinks.FocusedRow.GetValue(ColumnTableName));
			query.AddEquals(ReportLayoutHeaderPropertyNames.OutputType, PhraseListprint.PhraseIdLIST);

			m_Form.EntityBrowseSubReports.Republish(query);
		}

		/// <summary>
		/// Handles the StringChanged event of the EntityName control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Library.ClientControls.TextChangedEventArgs" /> instance containing the event data.</param>
		private void EntityName_StringChanged(object sender, TextChangedEventArgs e)
		{
			m_Form.GridLinks.ClearRows();
			SetEntityName();
		}

		/// <summary>
		/// Sets the name of the entity.
		/// </summary>
		/// <exception cref="System.NotImplementedException"></exception>
		private void SetEntityName()
		{
			m_Form.GridLinks.FocusedColumnChanged -= GridLinks_FocusedColumnChanged;
			m_Form.GridLinks.FocusedRowChanged -= GridLinks_FocusedRowChanged;

			if (string.IsNullOrEmpty(m_Entity.TableName)) return;

			m_Form.GridFields.Columns[0].SetCellBrowse(m_Entity, BrowseFactory.CreatePropertyNameBrowse(m_Entity.TableName, PropertyFilter.NonCollection, true));

			IEntity exampleEntity = EntityManager.CreateEntity(m_Entity.TableName);

			foreach (var relationship in EntityType.GetReflectedPropertyNames(m_Entity.TableName, true, false, false))
			{
				bool present = false;
				foreach (UnboundGridRow row in m_Form.GridLinks.Rows)
				{
					if ((string) row[ColumnTableName] == exampleEntity.GetEntityCollection(relationship).EntityType &&
					    (string) row[ColumnRelationship] == relationship)
					{
						present = true;
						break;
					}
				}

				if (!present)
				{
					m_Form.GridLinks.AddRow(false, exampleEntity.GetEntityCollection(relationship).EntityType, relationship, string.Empty);
				}
			}

			EntityManager.Delete(exampleEntity);

			m_Form.GridLinks.FocusedColumnChanged += GridLinks_FocusedColumnChanged;
			m_Form.GridLinks.FocusedRowChanged += GridLinks_FocusedRowChanged;
		}

		/// <summary>
		/// Handles the FocusedRowChanged event of the GridListprintLinks control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="UnboundGridFocusedRowChangedEventArgs"/> instance containing the event data.</param>
		private void GridLinks_FocusedRowChanged(object sender, UnboundGridFocusedRowChangedEventArgs e)
		{
			RepublishAvailableLinks();
		}

		/// <summary>
		/// Handles the FocusedColumnChanged event of the GridListprintLinks control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="UnboundGridFocusedColumnChangedEventArgs"/> instance containing the event data.</param>
		private void GridLinks_FocusedColumnChanged(object sender, UnboundGridFocusedColumnChangedEventArgs e)
		{
			RepublishAvailableLinks();
		}

		#endregion

		#region Pre-save

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
			int i = 1;

			// Clear existing links

			m_Entity.ReportLayoutLinks.RemoveAll();

			// Process the unbound grid.

			foreach (UnboundGridRow linkRow in m_Form.GridLinks.Rows)
			{
				var include = (bool)linkRow[ColumnInclude];
				if (!include) continue;

				var relation = (string)linkRow[ColumnRelationship];
				var tableName = (string)linkRow[ColumnTableName];
				var subReport = linkRow[ColumnSubReport] as ReportLayoutHeader;

				ReportLayoutLink found = null;

				foreach (var existing in m_Entity.ReportLayoutLinks.ActiveItems.Cast<ReportLayoutLink>())
				{
					if (existing.RelationshipName == relation && existing.TableName == tableName)
					{
						found = existing;
						break;
					}
				}

				// Create/Reinstate it

				if (found == null)
				{
					found = (ReportLayoutLink) EntityManager.CreateEntity(ReportLayoutLinkBase.EntityName);
				}

				m_Entity.ReportLayoutLinks.Add(found);

				// Update fields

				found.RelationshipName = relation;
				found.TableName = tableName;
				found.ReportLayoutHeaderLink = subReport;
				found.OrderNumber = i;
				found.ReportLayoutHeader = m_Entity;

				i++;
			}

			return base.OnPreSave();
		}

		#endregion
	}
}
