using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Utilities;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.ClientControls.Browse;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks
{
    /// <summary>
    /// Implementation of the EntityTemplate LTE
    /// 
    /// BSmock 25-Dec-2017 - Added ability to specify function calling for custom browse tasks
    /// 
    /// </summary>
    [SampleManagerTask("EntityTemplateTask", "LABTABLE", EntityTemplate.EntityName)]
    public class EntityTemplateTask : GenericLabtableTask
    {
        #region Member Variables

        private FormEntityTemplate m_Form;
        private EntityTemplate m_EntityTemplate;

        // Grid Columns

        private DataGridColumn m_TitleColumn;
        private DataGridColumn m_DefaultValueColumn;
        private DataGridColumn m_PropertyColumn;
        private DataGridColumn m_FilterByColumn;
        private DataGridColumn m_PromptTypeColumn;
        private DataGridColumn m_DefaultTypeColumn;
        private DataGridColumn m_CriteriaColumn;

        private IQuery m_CriteriaQuery;
        private bool m_InitialisingCriteria;

        #endregion

        #region Overridden Methods

        /// <summary>
        /// Called when the <see cref="GenericLabtableTask.MainForm"/> has been loaded.
        /// </summary>
        protected override void MainFormLoaded()
        {
            m_Form = (FormEntityTemplate)MainForm;
            m_EntityTemplate = (EntityTemplate)m_Form.Entity;

            // Get Columns

            m_DefaultValueColumn = m_Form.GridEntityTemplateProperties.GetColumnByProperty(EntityTemplatePropertyInternal.DefaultValueObjectPropertyName);
            m_PropertyColumn = m_Form.GridEntityTemplateProperties.GetColumnByProperty(EntityTemplatePropertyPropertyNames.PropertyName);
            m_FilterByColumn = m_Form.GridEntityTemplateProperties.GetColumnByProperty(EntityTemplatePropertyPropertyNames.FilterBy);
            m_TitleColumn = m_Form.GridEntityTemplateProperties.GetColumnByProperty(EntityTemplatePropertyPropertyNames.Title);
            m_PromptTypeColumn = m_Form.GridEntityTemplateProperties.GetColumnByProperty(EntityTemplatePropertyPropertyNames.PromptType);
            m_CriteriaColumn = m_Form.GridEntityTemplateProperties.GetColumnByProperty(EntityTemplatePropertyPropertyNames.Criteria);
            m_DefaultTypeColumn =
                m_Form.GridEntityTemplateProperties.GetColumnByProperty(EntityTemplatePropertyPropertyNames.DefaultType);

            // Assign Grid Events

            m_Form.GridEntityTemplateProperties.DataLoaded += new System.EventHandler(GridEntityTemplateProperties_DataLoaded);
            m_Form.GridEntityTemplateProperties.FocusedRowChanged += new System.EventHandler<DataGridFocusedRowChangedEventArgs>(GridEntityTemplateProperties_FocusedRowChanged);

            // Show or Hide the Properties Grid (only visible after an entity type has been selected on the first page)

            ShowHidePropertiesGrid();

            // Assign ObjectModel events

            m_EntityTemplate.PropertyChanged += new PropertyEventHandler(m_EntityTemplate_PropertyChanged);
            m_EntityTemplate.EntityTemplateProperties.ItemPropertyChanged += new EntityCollectionEventHandler(EntityTemplateProperties_ItemPropertyChanged);
            m_EntityTemplate.EntityTemplateProperties.ItemAdded += new EntityCollectionEventHandler(EntityTemplateProperties_ItemAdded);

            // Disable the Entity Type prompt when opening an existing Template

            EnableDisableEntityType();

            // Set UI Display Text

            m_Form.TemplatePropertiesMessageLabel.Caption = GetMessage("EntityTemplateEmptyEntityType");
        }


        #endregion

        #region UI Updates

        /// <summary>
        /// Shows the hide properties grid.
        /// </summary>
        private void ShowHidePropertiesGrid()
        {
            // The properties grid is only visible when the entity type is set

            bool isVisible = !string.IsNullOrEmpty(m_EntityTemplate.TableName);
            m_Form.GridEntityTemplateProperties.Visible = isVisible;

            if (isVisible)
            {
                // Setup Property Browse for the new entity type

                StringBrowse propertyBrowse = CreatePropertyBrowse();
                m_PropertyColumn.SetColumnBrowse(propertyBrowse);

                // Set the grid caption

                m_Form.GridEntityTemplateProperties.Caption = string.Format(GetMessage("EntityTemplatePropertiesGridCaption"), TextUtils.GetDisplayText(m_EntityTemplate.TableName));
            }

            // Show / hide information panel.

            m_Form.TemplatePropertiesPlaceHolder.Visible = !isVisible;
        }

        /// <summary>
        /// Refreshes the filter by browse.
        /// </summary>
        private void RefreshFilterByBrowse(string currentProperty)
        {
            // FilterBy column browse should only contain reference fields that are already defined within the grid
            List<string> filterByFields = new List<string>();

            if (EntityType.IsLinkedEntityType(m_EntityTemplate.TableName, currentProperty))
            {

                foreach (EntityTemplateProperty property in m_EntityTemplate.EntityTemplateProperties.ActiveItems)
                {
                    bool isLink = EntityType.IsLinkedEntityType(m_EntityTemplate.TableName, property.PropertyName);

                    if (isLink && !filterByFields.Contains(property.PropertyName) &&
                        (currentProperty != property.PropertyName))
                    {
                        // Add this property to the browse because it is a link

                        if (ValidFilterProperty(m_EntityTemplate.TableName, currentProperty, property.PropertyName))
                        {
                            filterByFields.Add(property.PropertyName);
                        }
                    }

                    if (currentProperty == property.PropertyName)
                    {
                        //browse should only show properties before current property.
                        break;
                    }
                }
            }

            // Assign the filter by column browse
            StringBrowse filterByBrowse = BrowseFactory.CreateStringBrowse(filterByFields);
            m_FilterByColumn.SetColumnBrowse(filterByBrowse);
        }

        /// <summary>
        /// Valid the current property against browse property
        /// </summary>
        /// <param name="entityTemplateTableName"></param>
        /// <param name="currentProperty"></param>
        /// <param name="checkProperty"></param>
        /// <returns></returns>
        private static bool ValidFilterProperty(string entityTemplateTableName, string currentProperty, string checkProperty)
        {
            string linkType = EntityType.GetLinkedEntityType(entityTemplateTableName, checkProperty);
            string propertyType = EntityType.GetLinkedEntityType(entityTemplateTableName, currentProperty);

            IList<string> properties = EntityType.GetReflectedPropertyNames(propertyType);

            // Loop through each property to find a link 

            foreach (var property in properties)
            {
                PromptAttribute attribute = EntityType.GetPromptAttribute(propertyType, property);

                if (attribute.IsLink && EntityType.GetLinkedEntityType(propertyType, property) == linkType)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Grid_CellButtonClicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Grid_CellButtonClicked(object sender, DataGridCellButtonClickedEventArgs e)
        {
            EntityTemplateProperty entityTemplateProperty = (EntityTemplateProperty)e.Entity;

            var editorValue = Library.Task.CreateTaskAndWait("FormulaEditorTask", entityTemplateProperty.DefaultValue, string.Empty, m_EntityTemplate.TableName);

            if (editorValue != entityTemplateProperty.DefaultValueObject)
            {
                ((DataGrid)sender).WriteValue(e.Entity, e.Property, editorValue);
            }
        }

        /// <summary>
        /// Creates the property browse.
        /// </summary>
        /// <returns></returns>
        private StringBrowse CreatePropertyBrowse()
        {
            IQuery query = EntityManager.CreateQuery(EntityTemplateField.EntityName);
            query.AddEquals(EntityTemplateFieldPropertyNames.TableName, m_EntityTemplate.TableName);

            IEntityCollection fields = EntityManager.Select(EntityTemplateField.EntityName, query);

            return BrowseFactory.CreateStringBrowse(fields, EntityTemplateFieldPropertyNames.FieldName);
        }

        /// <summary>
        /// Enables or disables the Entity Type prompt.
        /// </summary>
        private void EnableDisableEntityType()
        {
            bool enabled = m_EntityTemplate.IsNew();
            m_Form.PromptEntityType.ReadOnly = !enabled;
        }

        /// <summary>
        /// Refreshes the default value cell editor.
        /// </summary>
        /// <param name="templateProperty">The template property.</param>
        private void RefreshGridCells(EntityTemplateProperty templateProperty)
        {
            try
            {
                m_Form.GridEntityTemplateProperties.BeginUpdate();

                if (string.IsNullOrEmpty(templateProperty.PropertyName))
                {
                    // Disable cells because there is no property assigned

                    m_DefaultValueColumn.DisableCell(templateProperty, DisabledCellDisplayMode.GreyHideContents);
                    m_DefaultTypeColumn.DisableCell(templateProperty, DisabledCellDisplayMode.GreyHideContents);
                    m_FilterByColumn.DisableCell(templateProperty, DisabledCellDisplayMode.GreyHideContents);
                    m_CriteriaColumn.DisableCell(templateProperty, DisabledCellDisplayMode.GreyHideContents);
                    m_TitleColumn.DisableCell(templateProperty, DisabledCellDisplayMode.GreyHideContents);
                    m_PromptTypeColumn.DisableCell(templateProperty, DisabledCellDisplayMode.GreyHideContents);

                    return;
                }

                // Enable all cells

                m_DefaultValueColumn.EnableCell(templateProperty);
                m_DefaultTypeColumn.EnableCell(templateProperty);
                m_TitleColumn.EnableCell(templateProperty);
                m_PromptTypeColumn.EnableCell(templateProperty);

                // Refresh default type and default value cells

                RefreshDefaultValueCell(templateProperty);
                RefreshFilterByAndCriteriaCells(templateProperty);
            }
            finally
            {
                m_Form.GridEntityTemplateProperties.EndUpdate();
            }
        }

        /// <summary>
        /// Refreshes the default value cell.
        /// </summary>
        /// <param name="templateProperty">The template property.</param>
        private void RefreshDefaultValueCell(EntityTemplateProperty templateProperty)
        {
            PromptAttribute promptAttribute = EntityType.GetPromptAttribute(m_EntityTemplate.TableName, templateProperty.PropertyName);

            if (promptAttribute == null)
            {
                return;
            }

            // Set cell mandatory if Default Type is "Syntax Before" of "Before Edit"

            if (templateProperty.DefaultType.PhraseId == PhraseEntTmpDt.PhraseIdASSIGN_B ||
                templateProperty.DefaultType.PhraseId == PhraseEntTmpDt.PhraseIdASSIGN_A)
            {
                // Cell is Mandatory as it's value is assigned "Syntax / Before Edit"

                m_DefaultValueColumn.SetCellMandatory(templateProperty);
            }
            else
            {
                // Cell is not Mandatory

                m_DefaultValueColumn.ClearCellMandatory(templateProperty);
            }

            if (templateProperty.DefaultType.PhraseId == PhraseEntTmpDt.PhraseIdVALUE)
            {
                // Dates are set using Intervals

                if (promptAttribute.DataType != null && promptAttribute.DataType.SMType == SMDataType.DateTime)
                {
                    m_DefaultValueColumn.SetCellDataType(templateProperty, SMDataType.Interval);

                    return;
                }

                // If there is a FilterBy assigned, setup the browse taking into account this FilterBy value

                if (!string.IsNullOrEmpty(templateProperty.FilterBy))
                {
                    // This property has a FilterBy, filter the Browse

                    EntityTemplatePropertyInternal filterSource = m_EntityTemplate.GetProperty(templateProperty.FilterBy);

                    IQuery filteredQuery = templateProperty.CreateFilterByQuery(filterSource.DefaultValue);
                    EntityBrowse entityBrowse = BrowseFactory.CreateEntityBrowse(filteredQuery.TableName, filteredQuery);
                    m_DefaultValueColumn.SetCellBrowse(templateProperty, entityBrowse);

                    return;
                }

                // If there is a Criteria assigned, setup the browse based on the Criteria

                if (!string.IsNullOrEmpty(templateProperty.Criteria))
                {
                    if (templateProperty.DefaultType.PhraseId == PhraseEntTmpDt.PhraseIdVALUE)
                    {
                        // This property has a Criteria, filter the Browse

                        // A criteria has been specified for this column, setup the browse
                        ICriteriaTaskService criteriaTaskService = (ICriteriaTaskService)Library.GetService(typeof(ICriteriaTaskService));

                        // Once the query is populated the Query Populated Event is raised. This is beacause the criteria
                        // could prompt for VGL values or C# values.
                        // Prompted Criteria is ignored
                        string linkedType = EntityType.GetLinkedEntityType(m_EntityTemplate.TableName, templateProperty.PropertyName);
                        CriteriaSaved criteria = (CriteriaSaved)EntityManager.Select(TableNames.CriteriaSaved, new Identity(linkedType, templateProperty.Criteria));
                        if (BaseEntity.IsValid(criteria))
                        {
                            // Generate a query based on the criteria
                            criteriaTaskService.QueryPopulated += new CriteriaTaskQueryPopulatedEventHandler(criteriaTaskService_QueryPopulated);
                            m_CriteriaQuery = null;
                            m_InitialisingCriteria = true;
                            criteriaTaskService.GetPopulatedCriteriaQuery(criteria);
                            m_InitialisingCriteria = false;

                            if (m_CriteriaQuery != null)
                            {
                                // Assign the browse to the column
                                IEntityCollection browseEntities = EntityManager.Select(m_CriteriaQuery.TableName, m_CriteriaQuery);
                                EntityBrowse criteriaBrowse = BrowseFactory.CreateEntityBrowse(browseEntities);
                                m_DefaultValueColumn.SetCellBrowse(templateProperty, criteriaBrowse);

                                // Make sure the cell's value is present within the browse
                                IEntity defaultValueEntity = templateProperty.DefaultValueObject as IEntity;
                                if (BaseEntity.IsValid(defaultValueEntity) && !browseEntities.Contains(defaultValueEntity))
                                {
                                    // The default value is not within the specified criteria, null out this cell
                                    templateProperty.DefaultValueObject = null;
                                }

                                return;
                            }
                        }
                    }
                }

                // Set cell editor based on the property name
                if (m_EntityTemplate.TableName == TestInternal.EntityName && templateProperty.PropertyName == TestPropertyNames.Instrument)
                {
                    // Instruments must be available and not retired
                    IQuery query = EntityManager.CreateQuery(Instrument.EntityName);
                    query.AddEquals(InstrumentPropertyNames.Available, true);
                    query.AddEquals(InstrumentPropertyNames.Retired, false);
                    EntityBrowse instrumentBrowse = BrowseFactory.CreateEntityBrowse(query);
                    m_DefaultValueColumn.SetColumnBrowse(instrumentBrowse);
                }
                else
                {
                    m_DefaultValueColumn.SetCellEditorFromObjectModel(templateProperty, m_EntityTemplate.TableName, templateProperty.PropertyName);
                }

                return;
            }

            // BSmock - Add Function phrase browse
            if (templateProperty.DefaultType.PhraseId == PhraseEntTmpDt.PhraseIdFUNCTION)
            {
                var pb = BrowseFactory.CreatePhraseBrowse(PhraseEntFunc.Identity);
                m_DefaultValueColumn.SetCellBrowse(templateProperty, pb, false, true, false);
            }

            // Default value type is 'AFTER' or 'BEFORE' so this field is free text

            m_DefaultValueColumn.ShowCellButton(templateProperty);
            m_DefaultValueColumn.SetCellDataType(templateProperty, SMDataType.Text);

            m_DefaultValueColumn.Grid.CellButtonClicked -= Grid_CellButtonClicked;
            m_DefaultValueColumn.Grid.CellButtonClicked += Grid_CellButtonClicked;
        }

        /// <summary>
        /// Handles the QueryPopulated event of the criteriaTaskService control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Thermo.SampleManager.Server.CriteriaTaskQueryPopulatedEventArgs"/> instance containing the event data.</param>
        private void criteriaTaskService_QueryPopulated(object sender, CriteriaTaskQueryPopulatedEventArgs e)
        {
            if (m_InitialisingCriteria)
                m_CriteriaQuery = e.PopulatedQuery;
        }

        /// <summary>
        /// Refreshes the filter by and criteria cells.
        /// </summary>
        /// <param name="templateProperty">The template property.</param>
        private void RefreshFilterByAndCriteriaCells(EntityTemplateProperty templateProperty)
        {
            PromptAttribute promptAttribute = EntityType.GetPromptAttribute(m_EntityTemplate.TableName, templateProperty.PropertyName);

            if (promptAttribute != null && promptAttribute.IsLink)
            {
                // This is a link to another entity, enable the filter by cell

                m_FilterByColumn.EnableCell(templateProperty);
                m_CriteriaColumn.EnableCell(templateProperty);

                // Setup Criteria browse for this property
                string linkedType = EntityType.GetLinkedEntityType(m_EntityTemplate.TableName, templateProperty.PropertyName);
                IQuery query = EntityManager.CreateQuery(CriteriaSaved.EntityName);
                query.AddEquals(CriteriaSavedPropertyNames.TableName, linkedType);
                query.AddEquals(CriteriaSavedPropertyNames.PublicCriteria, true);
                EntityBrowse criteriaBrowse = BrowseFactory.CreateEntityBrowse(query);
                criteriaBrowse.ReturnProperty = CriteriaSavedPropertyNames.Identity;
                m_CriteriaColumn.SetCellBrowse(templateProperty, criteriaBrowse);
            }
            else
            {
                // Disable the cell for properties that are not links

                m_FilterByColumn.DisableCell(templateProperty, DisabledCellDisplayMode.GreyHideContents);
                m_CriteriaColumn.DisableCell(templateProperty, DisabledCellDisplayMode.GreyHideContents);
            }
        }

        /// <summary>
        /// Handles the DataLoaded event of the GridEntityTemplateProperties control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void GridEntityTemplateProperties_DataLoaded(object sender, System.EventArgs e)
        {
            foreach (EntityTemplateProperty property in m_Form.GridEntityTemplateProperties.GridData)
            {
                RefreshGridCells(property);
            }
        }

        /// <summary>
        /// Update the filter browse when the focused row changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GridEntityTemplateProperties_FocusedRowChanged(object sender, DataGridFocusedRowChangedEventArgs e)
        {
            EntityTemplateProperty entityTemplateProperty = (EntityTemplateProperty)e.Row;
            if (entityTemplateProperty != null)
            {
                RefreshFilterByBrowse(entityTemplateProperty.Name);
            }
        }


        #endregion

        #region Object Model Event Handlers

        /// <summary>
        /// Handles the PropertyChanged event of the m_EntityTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.PropertyEventArgs"/> instance containing the event data.</param>
        private void m_EntityTemplate_PropertyChanged(object sender, PropertyEventArgs e)
        {
            if (e.PropertyName == EntityTemplatePropertyNames.TableName)
            {
                // Show or hide the properties grid

                ShowHidePropertiesGrid();
            }
        }

        /// <summary>
        /// Called after the property sheet or wizard is saved.
        /// </summary>
        protected override void OnPostSave()
        {
            base.OnPostSave();

            // Disable the EntityType prompt

            EnableDisableEntityType();
        }

        /// <summary>
        /// Handles the ItemAdded event of the EntityTemplateProperties control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
        private void EntityTemplateProperties_ItemAdded(object sender, EntityCollectionEventArgs e)
        {
            RefreshGridCells((EntityTemplateProperty)e.Entity);
        }

        /// <summary>
        /// Handles the ItemPropertyChanged event of the EntityTemplateProperties control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
        private void EntityTemplateProperties_ItemPropertyChanged(object sender, EntityCollectionEventArgs e)
        {
            switch (e.PropertyName)
            {
                case EntityTemplatePropertyPropertyNames.PropertyName:
                case EntityTemplatePropertyPropertyNames.DefaultType:

                    // Refresh the default value cell appearance

                    RefreshGridCells((EntityTemplateProperty)e.Entity);

                    // Refresh the available properties in the FilterBy browse

                    RefreshFilterByBrowse(((EntityTemplateProperty)e.Entity).Name);

                    break;

                case EntityTemplatePropertyPropertyNames.FilterBy:
                case EntityTemplatePropertyPropertyNames.Criteria:

                    // Update the DefaultValue browse

                    RefreshDefaultValueCell((EntityTemplateProperty)e.Entity);

                    break;

                case EntityTemplatePropertyPropertyNames.DefaultValue:

                    EntityTemplateProperty changedProperty = (EntityTemplateProperty)e.Entity;
                    if (!string.IsNullOrEmpty(changedProperty.PropertyName))
                    {
                        // Look for properties that are filtered by this value and refresh their DefaultValue browse

                        foreach (EntityTemplateProperty property in m_EntityTemplate.EntityTemplateProperties)
                        {
                            if (property.FilterBy == changedProperty.PropertyName)
                            {
                                // This property is filtered by the property that has just been changed, refresh it's browse

                                RefreshDefaultValueCell(property);
                            }
                        }
                    }

                    break;
            }
        }

        #endregion

    }
}
