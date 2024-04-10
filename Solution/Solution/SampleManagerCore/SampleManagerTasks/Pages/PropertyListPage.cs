using System;
using System.Collections.Generic;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Utilities;
using Thermo.SampleManager.Common.Workflow;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;
using Thermo.Informatics.Common.Forms.Core;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Page to display properties in a vertical grid.
	/// </summary>
	[SampleManagerPage("PropertyListPage")]
	public class PropertyListPage : PageBase
	{
		#region Member Variables

		private UnboundGrid m_FieldGrid;
		private string m_CategoryName;
		private string m_InternalCategoryName;
		private bool m_Loaded;

		#endregion

		#region Overrides

		/// <summary>
		/// Page Selected
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public override void PageSelected(object sender, RuntimeFormsEventArgs e)
		{
			base.PageSelected(sender, e);
			if (m_Loaded) return;

			m_Loaded = true;

			m_CategoryName = Library.Message.GetMessage("LaboratoryMessages", "TemplateFieldsCategory");
			m_InternalCategoryName = Library.Message.GetMessage("LaboratoryMessages", "InternalFieldsCategory");
			
			m_FieldGrid = (UnboundGrid)MainForm.Controls[FormPropertyListPage.FieldsUnboundGridControlName];
			RefreshGrid(m_FieldGrid, MainForm.Entity);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Refreshes the grid.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="entity">The entity.</param>
		private void RefreshGrid(UnboundGrid grid, IEntity entity)
		{
			try
			{
				grid.BeginUpdate();
				grid.ClearGrid();
				BuildGrid(grid, entity);
			}
			catch(Exception ex)
			{
				DisplayPropertyError(ex.Message);
			}
			finally
			{
				grid.EndUpdate();
			}
		}

		/// <summary>
		/// Displays the property error.
		/// </summary>
		/// <param name="message">The message.</param>
		private void DisplayPropertyError(string message)
		{
			var label =(Label)MainForm.Controls[FormPropertyListPage.PropertyErrorLabelControlName];
			label.Visible = true;
			label.Caption += message;
			((PictureBox) MainForm.Controls[FormPropertyListPage.UnknownPictureControlName]).Visible = true;

		}

		/// <summary>
		/// Builds the grid.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="entity">The entity.</param>
		private void BuildGrid(UnboundGrid grid, IEntity entity)
		{
			UnboundGridRow newRow = grid.AddRow();
			newRow.Tag = entity;
			newRow.SetIcon(new IconName(entity.Icon));

			// Get the Template Fields

			EntityTemplateInternal entityTemplate = null;
			string template = GetTemplateId(entity);

			// Get the entity template from the entity

			IWorkflowNodeDefinition wfNode = entity.GetWorkflowNode();

			if (wfNode != null)
			{
				entityTemplate = (EntityTemplateInternal) ((WorkflowNode) wfNode).EntityTemplate;
			}

			if (entityTemplate != null && (entityTemplate.IsDeleted() || entityTemplate.Removeflag))
			{
				throw new Exception(string.Format(Library.Message.GetMessage("LaboratoryMessages", "EntityTemplateDeleted"), entityTemplate.Identity));
			}
			
			// Determine the columns.

			if (!string.IsNullOrEmpty(template))
			{
				AddOldTemplateFields(grid, entity, template);
			}
			else if (entityTemplate != null)
			{
				AddEntityTemplateFields(grid, entity, entityTemplate);
			}

			AddInternalFields(grid, entity.EntityType);

			// Read the values

			foreach(var column in grid.Columns)
			{
				object value = entity.Get(column.Name);
				if (value is DateTime)
				{
					value = new NullableDateTime((DateTime) value);
				}

				newRow[column] = value;
			}
		}

		/// <summary>
		/// Adds the entity template fields.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="entityTemplate">The entity template.</param>
		/// <param name="entity">The entity.</param>
		private void AddEntityTemplateFields(UnboundGrid grid, IEntity entity, EntityTemplateInternal entityTemplate)
		{
			foreach (EntityTemplateProperty property in entityTemplate.EntityTemplateProperties)
			{
				// Check for Dodgy Data

				if (! entity.ContainsProperty(property.PropertyName)) continue;

				// Add a column for this entity template property

				if (property.PromptType.PhraseId == PhraseEntTmpPt.PhraseIdHIDDEN) continue;
                
				// See if it already exists

				if (grid.ContainsColumn(property.PropertyName)) continue;

				// Create the Column

				UnboundGridColumn gridcolumn = grid.AddColumn(property.PropertyName, property.Title, m_CategoryName, 250);
				gridcolumn.SetColumnEditorFromObjectModel(entityTemplate.TableName, property.PropertyName);
			}
		}

		/// <summary>
		/// Adds the old template fields.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="template_id">The template_id.</param>
		private void AddOldTemplateFields(UnboundGrid grid, IEntity entity, string template_id)
		{
			string entityType = entity.EntityType;

			IQuery query = EntityManager.CreateQuery(TemplateFieldsBase.EntityName);
			query.AddEquals(TemplateFieldsPropertyNames.TemplateId, template_id);
			query.AddEquals(TemplateFieldsPropertyNames.TableName, entityType);

			IEntityCollection fieldsCollection = EntityManager.Select(TemplateFieldsBase.EntityName, query);

			foreach (TemplateFieldsBase templateFields in fieldsCollection)
			{
				if (!(templateFields.DisplayFlag || templateFields.PromptFlag || templateFields.MandatoryFlag)) continue;

				// Get hold of the appropriate property

				string propertyName = EntityType.DeducePropertyName(entityType, templateFields.FieldName);
				if (string.IsNullOrEmpty(propertyName)) continue;
				if (!entity.ContainsProperty(propertyName)) continue;

				// Get the column, if it already exists, don't add it again.

				UnboundGridColumn gridcolumn = grid.GetColumnByName(propertyName);
				if (gridcolumn != null) continue;

				string caption = templateFields.TextPrompt;
				caption = caption.TrimEnd(new [] {' ', '.'});
				
				gridcolumn = grid.AddColumn(propertyName, caption, m_CategoryName, 250);

				// Determine the appropriate prompt based on the object model

				gridcolumn.SetColumnEditorFromObjectModel(entityType, propertyName);
			}
		}

		/// <summary>
		/// Adds Internal Fields
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="entityType">Type of the entity.</param>
		private void AddInternalFields(UnboundGrid grid, string entityType)
		{
            FormsEntityType formsEntityType = new FormsEntityType(entityType);
			IQuery query = EntityManager.CreateQuery(MtFieldsBase.EntityName);
			query.AddEquals(MtFieldsPropertyNames.TableName, entityType);

			// Alphabetic order for the remaining fields

			List<string> propertyNames = new List<string>(EntityType.GetProperties(entityType));
			propertyNames.Sort();

			foreach (string propertyName in propertyNames)
			{
				// Get the column, if it already exists, don't add it again.

				UnboundGridColumn gridcolumn = grid.GetColumnByName(propertyName);
				if (gridcolumn != null) continue;

				// Partial Key Fields - Skip

				ISchemaTable table = Library.Schema.Tables[entityType];
				string fieldName = EntityType.GetFieldnameFromProperty(entityType, propertyName);
				if (fieldName != null && table.Fields.Contains(fieldName))
				{
					ISchemaField field = table.Fields[fieldName];
					if (field.LinkTable != null && field.LinkTable.KeyFields.Count != 1) continue;
				}

                IFormsPropertyType propertyType = formsEntityType.GetProperty(propertyName);
                if (propertyType.IsLinkToMany) continue;

				// Add the Column to the Grid

				gridcolumn = grid.AddColumn(propertyName,
											TextUtils.GetPropertyDisplayText(propertyName),
											m_InternalCategoryName, 250);

				// Determine the appropriate prompt based on the object model

				gridcolumn.SetColumnEditorFromObjectModel(entityType, propertyName);
			}
		}

		#endregion

		#region Static Public Methods

		/// <summary>
		/// Get the template id based on the entity type.
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public static string GetTemplateId(IEntity entity)
		{
			if (entity is Sample || entity is JobHeader || entity is Incidents || entity is BatchHeader)
			{
				return entity.GetString("TEMPLATE_ID");
			}

			if (entity is LotDetails)
			{
				return entity.GetString("TEMPLATE");
			}

			return string.Empty;
		}

		#endregion
	}
}
