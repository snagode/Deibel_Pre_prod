
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.ClientControls.Browse;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;
using System;

namespace Thermo.SampleManager.Tasks
{
    /// <summary>
    /// Implementation of the Entity Template Prompts
    /// </summary>
    [SampleManagerTask("EntityTemplatePromptTask")]
    public class EntityTemplatePromptTask : DefaultPromptFormTask
    {
        #region Member Variables

        private FormEntityTemplatePrompt m_Form;
        private EntityTemplate m_Template;
        private readonly Dictionary<string, object> m_PromptValues = new Dictionary<string, object>();
        private string m_TemplateName;
        private IQuery m_CriteriaQuery;
        private Dictionary<EntityBrowse, IEntityCollection> m_CriteriaBrowseLookup;
        private bool m_InitialisingCriteria;
        private IEntity m_Entity;

        #endregion

        #region Overrides

        /// <summary>
        /// Called when the <see cref="DefaultFormTask.MainForm"/> has been created.
        /// </summary>
        protected override void MainFormCreated()
        {
            m_CriteriaBrowseLookup = new Dictionary<EntityBrowse, IEntityCollection>();
            m_TemplateName = Context.TaskParameters[1];
            m_Form = (FormEntityTemplatePrompt)MainForm;
            m_Template = (EntityTemplate)EntityManager.SelectLatestVersion(TableNames.EntityTemplate, m_TemplateName);
            ParseParameters();
        }

        /// <summary>
        /// Called when the <see cref="DefaultFormTask.MainForm"/> has been loaded.
        /// </summary>
        protected override void MainFormLoaded()
        {
            m_Form.PromptUnboundGrid.CellValueChanged += OnCellValueChanged;
            m_Form.PromptUnboundGrid.ValidateCell += PromptUnboundGrid_ValidateCell;

            // Setup the form properties

            m_Form.PromptIcon.SetImageByIconName(new IconName(Library.Utils.GetDefaultIcon(m_Template.TableName)));
            m_Form.PromptDescription.Caption = m_Template.Description;

            // Create Entity and fill default values

            m_Entity = EntityManager.CreateEntity(m_Template.TableName);
            foreach (KeyValuePair<string, object> parameterValue in m_PromptValues)
            {
                m_Entity.Set(parameterValue.Key, parameterValue.Value);
            }

            m_Form.PromptUnboundGrid.BeginUpdate();
            BuildColumns(m_Entity, m_Form.PromptUnboundGrid);
            BuildRows(m_Entity, m_Form.PromptUnboundGrid);
            m_Form.PromptUnboundGrid.EndUpdate();

            // Call the base class to setup the closed event
            base.MainFormLoaded();
        }

        /// <summary>
        /// Handles the Closing event of the form control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        protected override void MainFormClosing(object sender, CancelEventArgs e)
        {
            // Check the user pressed OK.
            if (MainForm.FormResult == FormResult.OK)
            {
                StoreGridDataInEntity(m_Form.PromptUnboundGrid);

                // Get the unbound property values
                foreach (EntityTemplateProperty property in m_Template.EntityTemplateProperties)
                {

                    if (property.PromptType.PhraseId == PhraseEntTmpPt.PhraseIdHIDDEN)
                        continue;

                    if (m_PromptValues.ContainsKey(property.Name))
                    {
                        m_PromptValues[property.Name] = m_Entity.Get(property.Name);
                    }

                    //// Check all mandatory prompts have been entered
                    bool isMandatory = (property.PromptType.PhraseId == PhraseEntTmpPt.PhraseIdMANDATORY);

                    if (isMandatory && (m_PromptValues[property.Name] == null || m_PromptValues[property.Name].ToString() == string.Empty))
                    {
                        m_Form.PromptUnboundGrid.SetCellError(m_Form.PromptUnboundGrid.Rows[0], property.Name, m_Form.ValidationMessages.Mandatory);
                        e.Cancel = true;
                        return;
                    }
                }

                // Check if the key is unique
                if (!CheckUniqueFields())
                {
                    m_Form.PromptUnboundGrid.SetCellError(m_Form.PromptUnboundGrid.Rows[0], m_Form.PromptUnboundGrid.Columns[0], m_Form.ValidationMessages.Unique);
                    e.Cancel = true;
                    return;
                }

                // Return the values list
                Context.ReturnValue = m_PromptValues;
            }
            else
            {
                // Return a null list
                Context.ReturnValue = null;
            }
        }

        #endregion

        #region Build Methods

        /// <summary>
        /// Parses the parameters.
        /// </summary>
        private void ParseParameters()
        {
            for (int i = 2; i <= Context.TaskParameters.GetUpperBound(0); i += 2)
            {
                string property = Context.TaskParameters[i];
                object value;

                EntityType.TryParseProperty(m_Template.TableName, property, Context.TaskParameters[i + 1], out value);

                m_PromptValues.Add(property, value);
            }
        }

        /// <summary>
        /// Build Columns
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="grid"></param>
        private void BuildColumns(IEntity entity, UnboundGrid grid)
        {
            foreach (EntityTemplateProperty property in m_Template.EntityTemplateProperties)
            {
                // Add a column for this entity template property

                if (property.PromptType.IsPhrase(PhraseEntTmpPt.PhraseIdHIDDEN)) continue;

                // Retrieve or create column

                UnboundGridColumn gridcolumn = grid.GetColumnByName(property.PropertyName);

                if (gridcolumn == null)
                {
                    gridcolumn = grid.AddColumn(property.PropertyName, property.LocalTitle, "Properties", 100);
                    gridcolumn.SetColumnEditorFromObjectModel(m_Template.TableName, property.PropertyName);
                }
            }
        }

        /// <summary>
        /// Build Rows
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="grid"></param>
        private void BuildRows(IEntity entity, UnboundGrid grid)
        {
            m_CriteriaBrowseLookup.Clear();

            // Add a row to the grid

            UnboundGridRow newRow = grid.AddRow();
            newRow.Tag = entity;

            // Set the row icon
            newRow.SetIcon(new IconName(entity.Icon));

            // Set cell values and enable/disable redundant cells based on the entity template

            for (int i = grid.FixedColumns; i < grid.Columns.Count; i++)
            {
                UnboundGridColumn column = grid.Columns[i];

                // Try getting the template property

                EntityTemplateProperty templateProperty = (EntityTemplateProperty)m_Template.GetProperty(column.Name);

                if (templateProperty == null || templateProperty.IsHidden)
                {
                    // Disable this cell

                    column.DisableCell(newRow, DisabledCellDisplayMode.GreyHideContents);
                    continue;
                }

                // Set value

                newRow[column] = entity.Get(templateProperty.PropertyName);

                // This is an active cell

                if (templateProperty.IsMandatory)
                {
                    // Make the cell appear yellow

                    column.SetCellMandatory(newRow);
                }

                if (!string.IsNullOrEmpty(templateProperty.FilterBy))
                {
                    // Setup this column for filtering

                    // Mark the column that is used for filtering

                    UnboundGridColumn filterBySourceColumn = grid.GetColumnByName(templateProperty.FilterBy);
                    if (filterBySourceColumn != null)
                    {
                        filterBySourceColumn.Tag = true;
                    }

                    // Setup filter

                    object filterValue = entity.Get(templateProperty.FilterBy);

                    if (filterValue != null)
                    {
                        IEntity filterValueEntity = filterValue as IEntity;
                        bool isValid = filterValueEntity == null || BaseEntity.IsValid(filterValueEntity);
                        if (isValid)
                        {
                            SetupFilterBy(templateProperty, newRow, column, filterValue);
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(templateProperty.Criteria))
                {
                    // A criteria has been specified for this column, setup the browse

                    ICriteriaTaskService criteriaTaskService = (ICriteriaTaskService)Library.GetService(typeof(ICriteriaTaskService));

                    // Once the query is populated the Query Populated Event is raised. This is beacause the criteria
                    // could prompt for VGL values or C# values.
                    // Prompted Criteria is ignored

                    string linkedType = EntityType.GetLinkedEntityType(m_Template.TableName, templateProperty.PropertyName);
                    CriteriaSaved criteria = (CriteriaSaved)EntityManager.Select(TableNames.CriteriaSaved, new Identity(linkedType, templateProperty.Criteria));

                    if (BaseEntity.IsValid(criteria))
                    {

                        // Generate a query based on the criteria

                        criteriaTaskService.QueryPopulated += CriteriaTaskService_QueryPopulated;
                        m_CriteriaQuery = null;
                        m_InitialisingCriteria = true;
                        criteriaTaskService.GetPopulatedCriteriaQuery(criteria);
                        m_InitialisingCriteria = false;

                        if (m_CriteriaQuery != null)
                        {
                            // Assign the browse to the column

                            IEntityCollection browseEntities = EntityManager.Select(m_CriteriaQuery.TableName, m_CriteriaQuery);
                            EntityBrowse criteriaBrowse = BrowseFactory.CreateEntityBrowse(browseEntities);
                            column.SetCellBrowse(newRow, criteriaBrowse);
                            m_CriteriaBrowseLookup[criteriaBrowse] = browseEntities;

                            // Make sure the cell's value is present within the browse

                            IEntity defaultValueEntity = entity.GetEntity(templateProperty.PropertyName);
                            if (BaseEntity.IsValid(defaultValueEntity) && !browseEntities.Contains(defaultValueEntity))
                            {
                                // The default value is not within the specified criteria, null out this cell

                                newRow[templateProperty.PropertyName] = null;
                            }
                        }
                    }
                }

                if (templateProperty.IsReadOnly)
                {
                    // Disable the cell but display it's contents

                    column.DisableCell(newRow, DisabledCellDisplayMode.ShowContents);
                }

            }

        }

        /// <summary>
        /// Handles the QueryPopulated event of the criteriaTaskService control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Thermo.SampleManager.Server.CriteriaTaskQueryPopulatedEventArgs"/> instance containing the event data.</param>
        private void CriteriaTaskService_QueryPopulated(object sender, CriteriaTaskQueryPopulatedEventArgs e)
        {
            if (m_InitialisingCriteria)
                m_CriteriaQuery = e.PopulatedQuery;
        }


        /// <summary>
        /// SetupFilterBy
        /// </summary>
        /// <param name="templateProperty"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="filterValue"></param>
        private void SetupFilterBy(EntityTemplateProperty templateProperty, UnboundGridRow row, UnboundGridColumn column, object filterValue)
        {
            // Setup entity browse filtering

            IQuery filteredQuery = templateProperty.CreateFilterByQuery(filterValue);

            // Setup the property browse for the collection column to browse collection properties for the table.

            IEntityBrowse browse = BrowseFactory.CreateEntityOrHierarchyBrowse(filteredQuery.TableName, filteredQuery);

            column.SetCellEntityBrowse(row, browse);
        }


        /// <summary>
        /// Checks a record doesn't already exist based on the unique key.
        /// </summary>
        /// <returns>True if the field values are ok. False if a unique key has been violated.</returns>
        private bool CheckUniqueFields()
        {
            ISchemaTable table = Library.Schema.Tables[m_Template.TableName];

            IQuery query = EntityManager.CreateQuery(m_Template.TableName);

            foreach (ISchemaField schemaField in table.KeyFields)
            {
                string propertyName = EntityType.DeducePropertyName(table.Name, schemaField.Name);

                if (m_PromptValues.ContainsKey(propertyName))
                {
                    query.AddEquals(schemaField.Name, m_PromptValues[propertyName]);
                }
                else
                {
                    // We don't have all the fields so can't check uniqueness at this point
                    return true;
                }
            }

            // If the code gets here then we should have all the values setup in the query
            IEntityCollection entities = EntityManager.Select(m_Template.TableName, query);

            return (entities.Count == 0);
        }

        /// <summary>
        /// Store Grid Data In Entity
        /// </summary>
        /// <param name="grid"></param>
        private void StoreGridDataInEntity(UnboundGrid grid)
        {
            // Store grid data in object model
            foreach (UnboundGridRow row in grid.Rows)
            {
                IEntity rowEntity = (IEntity)row.Tag;

                int startPos = grid.FixedColumns;

                // Spin through and update

                for (int i = startPos; i < grid.Columns.Count; i++)
                {
                    UnboundGridColumn column = grid.Columns[i];
                    if (column.IsCellReadOnly(row)) continue; // Handled elsewhere

                    object value = row[column];

                    if (value is DateTime)
                    {
                        value = new NullableDateTime((DateTime)value);
                    }

                    rowEntity.Set(column.Name, value);
                }
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// OnCellValueChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCellValueChanged(object sender, UnboundGridValueChangedEventArgs e)
        {
            Library.Task.StateModified();

            if (e.Column.Tag == null)
            {
                return;
            }

            bool isfilterSource = (bool)e.Column.Tag;

            if (isfilterSource)
            {
                // This updated column's value is used to filter other cell browses in this row, update these browses

                IEntity rowEntity = (IEntity)e.Row.Tag;

                foreach (EntityTemplateProperty templateProperty in m_Template.EntityTemplateProperties)
                {
                    if (templateProperty.FilterBy == e.Column.Name)
                    {
                        UnboundGrid grid = (UnboundGrid)sender;

                        UnboundGridColumn filteredColumn = grid.GetColumnByName(templateProperty.PropertyName);

                        // Update the browse

                        SetupFilterBy(templateProperty, e.Row, filteredColumn, e.Value);

                        // This column is filtered by the updated column, reset it's value and browse

                        e.Row[templateProperty.PropertyName] = null;
                    }
                }
            }
        }

        /// <summary>
        /// PromptUnboundGrid_ValidateCell
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PromptUnboundGrid_ValidateCell(object sender, UnboundGridValidateCellEventArgs e)
        {
            Location location = e.Value as Location;
            if (location == null)
            {
                return;
            }

            if (!location.Assignable)
            {
                // Only assignable locations are allowed
                e.IsValid = false;
                e.ErrorText = Library.Message.GetMessage("GeneralMessages", "AssignableLocationMessage");
            }
        }


        #endregion

    }
}