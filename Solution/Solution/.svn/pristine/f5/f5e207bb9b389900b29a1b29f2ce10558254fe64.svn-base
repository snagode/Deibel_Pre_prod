using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Workflow;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.ClientControls.Browse;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;
using Thermo.SampleManager.Server.Workflow;
using Thermo.SampleManager.Server.Workflow.Nodes;

namespace Thermo.SampleManager.Tasks
{
    /// <summary>
    /// Lot Details Task
    /// BSmock, 3-Jan-2018;  Fixed Lot details string/phrase mistype
    /// </summary>
    [SampleManagerTask("LotDetailsTask", "GENERAL", "LOT_DETAILS")]
    public class LotDetailsTask : GenericLabtableTask
    {
        #region Member Variables

        private FormLotDetails m_Form;
        private LotDetails m_LotDetails;
        private LotReservation m_LotReservation;
        private IEntity m_SelectedTreeItem;
        private WorkflowBase m_DefaultWorkflow;
        private Dictionary<EntityBrowse, IEntityCollection> m_CriteriaBrowseLookup;
        private IQuery m_CriteriaQuery;
        private bool m_InitialisingCriteria;
        private string m_Title;

        private FormLotReservation m_FormLotReservation;
        private ToolBarButton m_ToolBarButtonReserve;
        private ToolBarButton m_ToolBarButtonTake;
        private ToolBarButton m_ToolBarButtonComplete;
        private ToolBarButton m_ToolBarButtonCancel;
        private string m_Units;

        private ContextMenuItem m_RmbModify;
        private ContextMenuItem m_RmbDisplay;
        private ContextMenuItem m_RmbReview;
        private bool m_RefreshBrowse;
        private bool m_LotCreatedByWorkflow;
        private ToolBarButton m_RemoveToolBarButton;
        private ToolBarButton m_AddToolBarButton;
        private List<string> m_PropertiesUsedInForm;
        private bool m_Saved;
        private bool m_Closed;

        private IWorkflowEventService m_WorkflowEventService;

        private BackgroundWorker m_BackgroundWorker;

        private enum InventoryMode
        {
            Reserve,
            Take,
            None
        }

        private InventoryMode m_CurrentMode = InventoryMode.None;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this is currently creating/logging a new Lot
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is logging in; otherwise, <c>false</c>.
        /// </value>
        public bool IsLogin
        {
            get { return ((Context.LaunchMode == "ADD") || (m_LotCreatedByWorkflow)) && !m_Saved; }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Called when the <see cref="P:Thermo.SampleManager.Tasks.GenericLabtableTask.MainForm" /> has been loaded.
        /// </summary>
        protected override void MainFormLoaded()
        {
            m_BackgroundWorker = new BackgroundWorker();

            m_BackgroundWorker.DoWork += m_BackgroundWorker_DoWork;
            m_BackgroundWorker.RunWorkerCompleted += m_BackgroundWorker_RunWorkerCompleted;
            m_Form = (FormLotDetails)MainForm;
            m_LotDetails = (LotDetails)MainForm.Entity;
            m_CriteriaBrowseLookup = new Dictionary<EntityBrowse, IEntityCollection>();
            m_RefreshBrowse = true;

            FilterTemplateFields();
            RefreshPropertiesGrid();
            m_LotDetails.CalculateQuantities();

            if (IsLogin)
            {
                m_Form.Title = m_Title;
                m_Form.ServerValidator.Validate += ServerValidator_Validate;

                CheckforSyntax();
                Library.Task.StateModified();
            }
            else
            {
                BuildTrees();
                PopulateRmb();
                ToggleInventories();

                m_Form.ExplorerGridJobs.SelectionChanged += ExplorerGridJobs_SelectionChanged;
            }

            m_Form.GridParentLots.ValidateCell += GridParentLotsOnValidateCell;
            m_Form.GridParentLots.FocusedRowChanged += GridParentLots_FocusedRowChanged;
            m_LotDetails.ParentLots.ItemChanged += ParentLots_ItemChanged;
            m_LotDetails.ParentLots.ItemAdded += ParentLots_ItemAdded;
            m_Form.GridLotProperties.CellValueChanged += GridLotProperties_CellValueChanged;

            m_Form.Selected += Form_Selected;
            m_Form.Closed += m_Form_Closed;

            // Inventory

            ReservationSetup();

            // General

            ManageTabs();
            UpdateIcon();

            // Toolbars

            m_RemoveToolBarButton = m_Form.ToolBar.FindButton("ButtonRemove");
            m_AddToolBarButton = m_Form.ToolBar.FindButton("ButtonAdd");
            m_RemoveToolBarButton.Click += RemoveToolBarButton_Click;
            m_AddToolBarButton.Click += AddToolBarButton_Click;
            UpdateToolbar(null);

            if (!m_LotDetails.Template.IsNull())
            {
                Library.Utils.ShowAlert(m_Form.StringTableMessage.TemplateWarning);
            }
            base.MainFormLoaded();
        }

        /// <summary>
        /// Handles the Closed event of the m_Form control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void m_Form_Closed(object sender, EventArgs e)
        {
            m_Closed = true;
        }

        /// <summary>
        /// Once the worker is complete update the job tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!m_Closed)
            {
                IQuery jobs = EntityManager.CreateQuery(JobHeaderBase.EntityName);
                jobs.AddEquals(JobHeaderPropertyNames.LotId, m_LotDetails);
                m_Form.EntityBrowseRelatedJobs.Republish(jobs);
                m_Form.DataQueryRelatedJobs.ResultData.Refresh();
                if (m_Form.DataQueryRelatedJobs.ResultData.Count > 0)
                {
                    UpdateSampleBrowse((JobHeader)m_Form.DataQueryRelatedJobs.ResultData[0]);
                }
            }
        }

        /// <summary>
        /// Background worker to process post login triggers.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // The lot is the highest level so trigger the post login normally

            IWorkflowPropertyBag bag = m_LotDetails.TriggerPostLogin();

            if (bag.Errors.Count == 0)
            {
                EntityManager.Commit();
            }

            // If any jobs/samples are created after the lot is created
            // trigger their post login
            bag = m_WorkflowEventService.ProcessDeferredTriggers("POST_LOGIN");

            if (bag.Errors.Count == 0)
            {
                EntityManager.Commit();
            }
        }

        /// <summary>
        /// Override setup task to allow lot login.
        /// </summary>
        protected override void SetupTask()
        {
            m_WorkflowEventService = Library.GetService<IWorkflowEventService>();

            // If this is add/login mode prompt for a workflow

            if (IsLogin && Context.Workflow == null)
            {
                IEntity workflowEntity;
                IQuery workflowQuery = EntityManager.CreateQuery(WorkflowBase.EntityName);
                workflowQuery.AddEquals(WorkflowPropertyNames.TableName, LotDetailsBase.EntityName);
                workflowQuery.AddEquals(WorkflowPropertyNames.WorkflowType, PhraseWflowType.PhraseIdLOT);

                string caption = Library.Message.GetMessage("WorkflowMessages", "LotWorkflowRun");
                string title = Library.Message.GetMessage("WorkflowMessages", "NodeLotLoginNameFormat");

                if (Library.Utils.PromptForEntity(caption, title, workflowQuery, out workflowEntity, TriState.Default, 123) == FormResult.OK)
                {
                    m_DefaultWorkflow = (WorkflowBase)workflowEntity;
                }
                else
                {
                    Exit();
                    return;
                }
            }

            if (Context.Workflow != null)
            {
                m_DefaultWorkflow = (WorkflowBase)Context.Workflow;
            }

            base.SetupTask();
        }

        /// <summary>
        /// Create the entity through workflow
        /// </summary>
        /// <returns></returns>
        protected override IEntity CreateNewEntity()
        {
            m_Title = Library.Message.GetMessage("WorkflowMessages", "NodeLotLoginNameFormat");
            IList<IEntity> newEntities = RunWorkflowForEntity(null, (Workflow)m_DefaultWorkflow, 1);
            m_LotCreatedByWorkflow = true;
            return newEntities[0];
        }

        /// <summary>
        /// Called before the property sheet or wizard is saved.
        /// </summary>
        /// <returns>
        /// true to allow the save to continue, false to abort the save.
        ///             Please also ensure that you call the base.OnPreSave when continuing
        ///             successfully.
        /// </returns>
        protected override bool OnPreSave()
        {
            bool continueSave = true;
            StoreGridDataInEntities(m_Form.GridLotProperties);
            if ((m_LotDetails.Quantity <= 0 && m_LotDetails.TrackInventory) && Context.LaunchMode == AddOption)
            {
                continueSave = Library.Utils.FlashMessageYesNo(m_Form.StringTableMessage.QuantityError, m_Form.StringTableMessage.QuantityTitle);
            }

            if (continueSave)
            {
                m_LotDetails.TriggerPostEdit();
                continueSave = base.OnPreSave();

                // If the inventory is being tracked disable quantity and unit so they can not be changed.

                if (m_LotDetails.TrackInventory)
                {
                    UnboundGridColumn columnQuantity = m_Form.GridLotProperties.GetColumnByName(LotDetailsPropertyNames.Quantity);
                    UnboundGridColumn columnUnits = m_Form.GridLotProperties.GetColumnByName(LotDetailsPropertyNames.Units);
                    columnQuantity.DisableCell(m_Form.GridLotProperties.Rows[0]);
                    columnUnits.DisableCell(m_Form.GridLotProperties.Rows[0]);
                }
            }
            return continueSave;
        }

        /// <summary>
        /// Called after the property sheet or wizard is saved.
        /// </summary>
        protected override void OnPostSave()
        {
            base.OnPostSave();
            m_BackgroundWorker.RunWorkerAsync();
            UpdateLotDetails();
        }

        /// <summary>
        /// UpdateLotDetails
        /// </summary>
        private void UpdateLotDetails()
        {
            ReseravtionToolbarButtonCheck();
            if (IsLogin)
            {
                m_Saved = true;

                ManageTabs();
                BuildTrees();
                PopulateRmb();

                m_Form.LotIdSyntax.Visible = false;
                m_Form.LotId.Visible = true;
                m_Form.NameSyntax.Visible = false;
                m_Form.Name.Visible = true;
            }
        }

        #endregion

        #region Trees

        /// <summary>
        /// Builds the trees.
        /// </summary>
        private void BuildTrees()
        {
            LotRelation top = (LotRelation)EntityManager.CreateEntity(LotRelationBase.EntityName);
            top.FromLot = m_LotDetails;
            top.ToLot = m_LotDetails;

            // Populate FROM

            m_Form.FromTree.ClearNodes();
            string nodeName = String.Format(m_Form.StringTable.TreeMadeFrom, m_LotDetails.Name);
            BuildFromNode(top, nodeName);

            // Populate TO

            m_Form.IntoTree.ClearNodes();
            nodeName = String.Format(m_Form.StringTable.TreeMadeInto, m_LotDetails.Name);
            BuildToNode(top, nodeName);
        }

        /// <summary>
        /// Builds to node.
        /// </summary>
        /// <param name="relation">The relation.</param>
        /// <param name="nodeName">Name of the node.</param>
        /// <param name="root">The root.</param>
        /// <param name="depth">The depth.</param>
        private void BuildToNode(LotRelationBase relation, string nodeName, SimpleTreeListNodeProxy root = null, int depth = 1)
        {
            if (depth > 10)
            {
                return;
            }

            LotDetails toLot = (LotDetails)relation.ToLot;
            root = m_Form.IntoTree.AddNode(root, nodeName, toLot.Icon, relation);

            foreach (LotRelation to in toLot.ChildLots)
            {
                BuildToNode(to, to.ToLot.Name, root, depth + 1);
            }
        }

        /// <summary>
        /// Builds from node.
        /// </summary>
        /// <param name="relation">The relation.</param>
        /// <param name="nodeName">Name of the node.</param>
        /// <param name="root">The root.</param>
        /// <param name="depth">The depth.</param>
        private void BuildFromNode(LotRelationBase relation, string nodeName, SimpleTreeListNodeProxy root = null, int depth = 1)
        {
            if (depth > 10)
            {
                return;
            }

            LotDetails fromLot = (LotDetails)relation.FromLot;
            root = m_Form.FromTree.AddNode(root, nodeName, fromLot.Icon, relation);

            foreach (LotRelation to in fromLot.ParentLots)
            {
                BuildFromNode(to, to.FromLot.Name, root, depth + 1);
            }
        }

        /// <summary>
        /// Populates the RMB.
        /// </summary>
        private void PopulateRmb()
        {
            // Relations

            m_Form.IntoTree.ContextMenu.BeforePopup += ContextChild_BeforePopup;
            m_Form.FromTree.ContextMenu.BeforePopup += ContextParent_BeforePopup;

            m_Form.IntoTree.FocusedNodeChanged += SimpleTreeListChildFocusedNodeChanged;
            m_Form.FromTree.FocusedNodeChanged += SimpleTreeListParentFocusedNodeChanged;

            m_RmbModify = m_Form.FromTree.ContextMenu.AddItem("Modify", String.Empty);
            m_Form.IntoTree.ContextMenu.AddItem(m_RmbModify);

            m_RmbDisplay = m_Form.FromTree.ContextMenu.AddItem("Display", String.Empty);
            m_Form.IntoTree.ContextMenu.AddItem(m_RmbDisplay);

            m_RmbReview = m_Form.FromTree.ContextMenu.AddItem("Review", String.Empty);
            m_Form.IntoTree.ContextMenu.AddItem(m_RmbReview);

            m_Form.FromTree.ContextMenu.RegisterContextMenu();

            m_RmbModify.ItemClicked += RmbModify_ItemClicked;
            m_RmbDisplay.ItemClicked += RmbDisplay_ItemClicked;
            m_RmbReview.ItemClicked += RmbReview_ItemClicked;
        }

        /// <summary>
        /// Handles the BeforePopup event of the Context menu on the Parent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ContextMenuBeforePopupEventArgs"/> instance containing the event data.</param>
        private void ContextParent_BeforePopup(object sender, ContextMenuBeforePopupEventArgs e)
        {
            LotRelation relation = e.Entity as LotRelation;

            if (relation != null)
            {
                LotDetailsBase lotDetail = relation.FromLot;
                m_SelectedTreeItem = lotDetail;
            }
            else
            {
                LotDetailsBase lotDetail = (LotDetailsBase)e.Entity;
                m_SelectedTreeItem = lotDetail;
            }
        }

        /// <summary>
        /// Handles the BeforePopup event of the Context menu on the Child control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ContextMenuBeforePopupEventArgs"/> instance containing the event data.</param>
        private void ContextChild_BeforePopup(object sender, ContextMenuBeforePopupEventArgs e)
        {
            LotRelation relation = e.Entity as LotRelation;

            if (relation != null)
            {
                LotDetailsBase lotDetail = relation.ToLot;
                m_SelectedTreeItem = lotDetail;
            }
            else
            {
                LotDetailsBase lotDetail = (LotDetailsBase)e.Entity;
                m_SelectedTreeItem = lotDetail;
            }
        }

        /// <summary>
        /// Handles the FocusedNodeChanged event of the SimpleTreeListUsed control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.SimpleFocusedNodeChangedEventArgs"/> instance containing the event data.</param>
        private void SimpleTreeListParentFocusedNodeChanged(object sender, SimpleFocusedNodeChangedEventArgs e)
        {
            LotRelation relation = (LotRelation)e.NewNode.Data;
            m_Form.DataEntityParent.Data = relation;
        }

        /// <summary>
        /// Handles the FocusedNodeChanged event of the SimpleTreeListUsed control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.SimpleFocusedNodeChangedEventArgs"/> instance containing the event data.</param>
        private void SimpleTreeListChildFocusedNodeChanged(object sender, SimpleFocusedNodeChangedEventArgs e)
        {
            LotRelation relation = (LotRelation)e.NewNode.Data;
            m_Form.DataEntityChild.Data = relation;
        }

        /// <summary>
        /// Handles the ItemClicked event of the m_RmbReview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ContextMenuItemEventArgs"/> instance containing the event data.</param>
        private void RmbReview_ItemClicked(object sender, ContextMenuItemEventArgs e)
        {
            Library.Task.CreateTask(35272, m_SelectedTreeItem);
        }

        /// <summary>
        /// Handles the ItemClicked event of the m_RmbModify control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ContextMenuItemEventArgs"/> instance containing the event data.</param>
        private void RmbModify_ItemClicked(object sender, ContextMenuItemEventArgs e)
        {
            Library.Task.CreateTask(35303, m_SelectedTreeItem);
        }

        /// <summary>
        /// Handles the ItemClicked event of the m_RmbDisplay control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ContextMenuItemEventArgs"/> instance containing the event data.</param>
        private void RmbDisplay_ItemClicked(object sender, ContextMenuItemEventArgs e)
        {
            Library.Task.CreateTask(35302, m_SelectedTreeItem);
        }

        #endregion

        #region Jobs

        /// <summary>
        /// Handles the SelectionChanged event of the ExplorerGridJobs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ExplorerGridSelectionChangedEventArgs"/> instance containing the event data.</param>
        private void ExplorerGridJobs_SelectionChanged(object sender, ExplorerGridSelectionChangedEventArgs e)
        {
            if (e.Selection.Count > 0)
            {
                var job = e.Selection[0] as JobHeader;
                UpdateSampleBrowse(job);
            }
        }

        #endregion

        #region Workflow

        /// <summary>
        /// Runs the workflow for entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="selectedWorkflow">The selected workflow.</param>
        /// <param name="count">The count.</param>
        /// <returns>List of newly created entities.</returns>
        private IList<IEntity> RunWorkflowForEntity(IEntity entity, Workflow selectedWorkflow, int count)
        {
            List<IEntity> newEntities = new List<IEntity>();

            for (int j = 0; j < count; j++)
            {
                // Run the workflow with a property bag for results - used passed in Parameters if available.

                IWorkflowPropertyBag propertyBag;

                if (selectedWorkflow.Properties == null)
                {
                    propertyBag = GeneratePropertyBag(entity);
                }
                else
                {
                    propertyBag = selectedWorkflow.Properties;
                }

                // Counters

                propertyBag.Set("$WORKFLOW_MAX", count);
                propertyBag.Set("$WORKFLOW_COUNT", j + 1);

                // Perform

                PerformWorkflow(selectedWorkflow, propertyBag);

                // Keep track of the newly created entities.

                IList<IEntity> entities = propertyBag.GetEntities(selectedWorkflow.TableName);
                newEntities.AddRange(entities);
            }

            return newEntities;
        }

        /// <summary>
        /// Generates the property bag for the passed entity.
        /// </summary>
        /// <returns></returns>
        private IWorkflowPropertyBag GeneratePropertyBag(IEntity entity)
        {
            // Generate context for the workflow

            IWorkflowPropertyBag propertyBag = new WorkflowPropertyBag();

            if (entity != null)
            {
                // Pass in the selected parent
                propertyBag.Add(entity.EntityType, entity);
            }

            return propertyBag;
        }

        /// <summary>
        /// Performs the workflow.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <param name="propertyBag">The property bag.</param>
        /// <returns></returns>
        private bool PerformWorkflow(Workflow workflow, IWorkflowPropertyBag propertyBag)
        {
            if (propertyBag == null)
            {
                // Perform the workflow & validate it's output

                propertyBag = workflow.Perform();
            }
            else
            {
                workflow.Perform(propertyBag);
            }

            // Make sure the workflow generated something

            if (propertyBag.Count == 0)
            {
                // Un-supported entity type
                string title = Library.Message.GetMessage("WorkflowMessages", "NodeLotLoginName");
                string message = Library.Message.GetMessage("GeneralMessages", "EmptyWorkflowOutput");
                Library.Utils.FlashMessage(message, title, MessageButtons.OK, MessageIcon.Error, MessageDefaultButton.Button1);

                return false;
            }

            // Exit if there are errors

            if (propertyBag.Errors.Count > 0)
            {
                throw new SampleManagerException(propertyBag.Errors[0].Message, propertyBag.Errors[0]);
            }

            return true;
        }

        #endregion

        #region Lot Properties

        /// <summary>
        /// Refresh Properties Grid
        /// </summary>
        private void RefreshPropertiesGrid()
        {
            m_Form.GridLotProperties.BeginUpdate();
            m_Form.GridLotProperties.ClearGrid();
            BuildPropertyColumns(m_LotDetails, m_Form.GridLotProperties);
            BuildRows(m_LotDetails, m_Form.GridLotProperties);
            m_Form.GridLotProperties.EndUpdate();
        }

        /// <summary>
        /// Build Property Columns
        /// </summary>
        /// <param name="lotDetailsBase">The lot details base.</param>
        /// <param name="grid">The grid.</param>
        private void BuildPropertyColumns(LotDetailsBase lotDetailsBase, UnboundGrid grid)
        {
            // Get the entity template from the entity

            var template = (EntityTemplateInternal)lotDetailsBase.EntityTemplate;

            foreach (EntityTemplateProperty property in template.EntityTemplateProperties)
            {
                // Add a column for this entity template property

                if (property.PromptType.IsPhrase(PhraseEntTmpPt.PhraseIdHIDDEN))
                {
                    continue;
                }

                // If the property is used in the form don't add it to the template list

                if (m_PropertiesUsedInForm.Contains(property.PropertyName))
                {
                    continue;
                }

                // Retrieve or create column

                UnboundGridColumn gridcolumn = grid.GetColumnByName(property.PropertyName);

                if (gridcolumn == null)
                {
                    gridcolumn = grid.AddColumn(property.PropertyName, property.LocalTitle, "Properties", 100);
                    gridcolumn.SetColumnEditorFromObjectModel(template.TableName, property.PropertyName);
                }
            }
        }

        /// <summary>
        /// Build Rows
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="grid">The grid.</param>
        private void BuildRows(IEntity entity, UnboundGrid grid)
        {
            m_CriteriaBrowseLookup.Clear();

            EntityTemplateInternal template = (EntityTemplateInternal)((LotDetailsBase)entity).EntityTemplate;

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

                EntityTemplatePropertyInternal templateProperty = template.GetProperty(column.Name);

                if (templateProperty == null)
                {
                    newRow[column] = entity.Get(column.Name);
                }
                else
                {
                    if (templateProperty.IsHidden)
                    {
                        // Disable this cell

                        column.DisableCell(newRow, DisabledCellDisplayMode.GreyHideContents);
                    }

                    // Set value

                    newRow[column] = entity.Get(templateProperty.PropertyName);

                    // Stop the Units from being changed once the lot has been created if it's tracking inventory.

                    if ((templateProperty.PropertyName == LotDetailsPropertyNames.Units || templateProperty.PropertyName == LotDetailsPropertyNames.Quantity)
                        && Context.LaunchMode == "MODIFY" && ((LotDetailsBase)entity).TrackInventory)
                    {
                        column.DisableCell(newRow);
                    }

                    // This is an active cell

                    if (templateProperty.IsMandatory)
                    {
                        // Make the cell appear yellow

                        column.SetCellMandatory(newRow);
                    }

                    if (!String.IsNullOrEmpty(templateProperty.FilterBy))
                    {
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
                    else if (!String.IsNullOrEmpty(templateProperty.Criteria))
                    {
                        // A criteria has been specified for this column, setup the browse

                        ICriteriaTaskService criteriaTaskService = (ICriteriaTaskService)Library.GetService(typeof(ICriteriaTaskService));

                        // Once the query is populated the Query Populated Event is raised. This is because the criteria
                        // could prompt for VGL values or C# values.
                        // Prompted Criteria is ignored

                        string linkedType = EntityType.GetLinkedEntityType(template.TableName, templateProperty.PropertyName);
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

                                m_CriteriaQuery.HideRemoved();
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
        }

        /// <summary>
        /// Setup the filter by.
        /// </summary>
        /// <param name="templateProperty">The template property.</param>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <param name="filterValue">The filter value.</param>
        private void SetupFilterBy(EntityTemplatePropertyInternal templateProperty, UnboundGridRow row, UnboundGridColumn column, object filterValue)
        {
            // Setup entity browse filtering

            IQuery filteredQuery = templateProperty.CreateFilterByQuery(filterValue);

            // Setup the property browse for the collection column to browse collection properties for the table.

            IEntityBrowse browse = BrowseFactory.CreateEntityOrHierarchyBrowse(filteredQuery.TableName, filteredQuery);

            column.SetCellEntityBrowse(row, browse);
        }

        /// <summary>
        /// Stores the grid data in entities.
        /// </summary>
        /// <param name="grid">The grid.</param>
        private void StoreGridDataInEntities(UnboundGrid grid)
        {
            // Store grid data in object model

            foreach (UnboundGridRow row in grid.Rows)
            {
                IEntity rowEntity = (IEntity)row.Tag;

                // Adjust for the "Assign" field

                int startPos = grid.FixedColumns;
                if (rowEntity.EntityType == TestBase.EntityName)
                {
                    startPos = startPos - 1;
                }

                // Spin through and update

                for (int i = startPos; i < grid.Columns.Count; i++)
                {
                    UnboundGridColumn column = grid.Columns[i];
                    if (column.IsCellReadOnly(row))
                    {
                        continue; // Handled elsewhere
                    }

                    object value = row[column];

                    if (value is DateTime)
                    {
                        value = new NullableDateTime((DateTime)value);
                    }

                    rowEntity.Set(column.Name, value);
                }
            }
        }

        /// <summary>
        /// Handles the QueryPopulated event of the CriteriaTaskService control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CriteriaTaskQueryPopulatedEventArgs"/> instance containing the event data.</param>
        private void CriteriaTaskService_QueryPopulated(object sender, CriteriaTaskQueryPopulatedEventArgs e)
        {
            if (m_InitialisingCriteria)
            {
                m_CriteriaQuery = e.PopulatedQuery;
            }
        }

        #endregion

        #region Syntax

        /// <summary>
        /// Check for Syntax
        /// </summary>
        private void CheckforSyntax()
        {
            if (!String.IsNullOrEmpty(m_LotDetails.LotIdFormula))
            {
                m_Form.LotId.Visible = false;
                m_Form.LotIdSyntax.Visible = true;
            }

            if (!String.IsNullOrEmpty(m_LotDetails.LotNameFormula))
            {
                m_Form.Name.Visible = false;
                m_Form.Name.ReadOnly = true;
                m_Form.NameSyntax.Visible = true;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Manages the tabs.
        /// </summary>
        private void ManageTabs()
        {
            m_Form.GridChildLots.Visible = !IsLogin;
            m_Form.Page_RelationTrees.Visible = !IsLogin;
            m_Form.Page_JobSamples.Visible = !IsLogin;
            m_Form.Page_InventoryLog.Visible = m_LotDetails.TrackInventory && !IsLogin;
        }

        /// <summary>
        /// Build Child List
        /// </summary>
        /// <param name="childList">The child list.</param>
        /// <param name="currentLot">The current lot.</param>
        private void BuildChildList(List<string> childList, LotDetails currentLot)
        {
            foreach (LotRelation childLot in currentLot.ChildLots)
            {
                childList.Add(childLot.ToLot.LotId);
                BuildChildList(childList, (LotDetails)childLot.ToLot);
            }
        }

        /// <summary>
        /// Build Parent List
        /// </summary>
        /// <param name="parentList">The parent list.</param>
        /// <param name="currentLot">The current lot.</param>
        private void BuildParentList(List<string> parentList, LotDetails currentLot)
        {
            foreach (LotRelation parentLot in currentLot.ParentLots)
            {
                if (!parentLot.FromLot.IsNull())
                {
                    parentList.Add(parentLot.FromLot.LotId);
                    BuildParentList(parentList, (LotDetails)parentLot.FromLot);
                }
            }
        }

        /// <summary>
        /// Build Parent Browse
        /// </summary>
        private void BuildRelationsBrowse(IQuery parentQuery)
        {
            List<string> list = new List<string>();
            if (!String.IsNullOrEmpty(m_LotDetails.LotId))
            {
                list.Add(m_LotDetails.LotId);
            }

            BuildParentList(list, m_LotDetails);
            BuildChildList(list, m_LotDetails);

            foreach (string relatedLot in list)
            {
                parentQuery.AddNotEquals(LotDetailsPropertyNames.LotId, relatedLot);
            }
        }

        /// <summary>
        /// Update icon based on type
        /// </summary>
        private void UpdateIcon()
        {
            m_Form.IconImage.SetImageByIconName(m_LotDetails.Icon);
        }


        /// <summary>
        /// Filter template fields based on the property sheet.
        /// </summary>
        private void FilterTemplateFields()
        {
            m_PropertiesUsedInForm = new List<string>();
            foreach (ClientProxyControl control in m_Form.Controls)
            {
                if (control is Prompt)
                {
                    Prompt prompt = (Prompt)control;
                    if (!String.IsNullOrEmpty(prompt.Property) && !prompt.ReadOnly && prompt.EntityType == LotDetailsBase.EntityName && prompt.Visible && prompt.Enabled)
                    {
                        m_PropertiesUsedInForm.Add(((Prompt)control).Property);
                    }
                }
            }
        }

        /// <summary>
        /// Update sample browse based on the selected job.
        /// </summary>
        /// <param name="job"></param>
        private void UpdateSampleBrowse(JobHeader job)
        {
            if (job != null)
            {
                IQuery query = EntityManager.CreateQuery(SampleBase.EntityName);
                query.AddEquals(SamplePropertyNames.JobName, job.JobName);
                m_Form.EntityBrowseSample.Republish(query);
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the FocusedRowChanged event of the GridParentLots control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridFocusedRowChangedEventArgs"/> instance containing the event data.</param>
        private void GridParentLots_FocusedRowChanged(object sender, DataGridFocusedRowChangedEventArgs e)
        {
            if (e.Row is LotRelation)
            {
                IQuery browseQuery = null;

                WorkflowNode createByNode = e.Row.GetWorkflowNode() as WorkflowNode;

                if (createByNode != null)
                {
                    LinkedParentLotNode linkLotNode = createByNode.Node as LinkedParentLotNode;

                    if (linkLotNode != null)
                    {
                        browseQuery = linkLotNode.LinkBrowseQuery;
                    }
                }

                if (browseQuery == null)
                {
                    browseQuery = EntityManager.CreateQuery(LotDetailsBase.EntityName);
                }

                BuildRelationsBrowse(browseQuery);
                m_Form.EntityBrowseParent.Republish(browseQuery);

                UpdateToolbar((LotRelation)e.Row);
            }
        }

        /// <summary>
        /// Update toolbar depending on nodes.
        /// </summary>
        /// <param name="lot"></param>
        private void UpdateToolbar(LotRelation lot)
        {
            if (lot == null)
            {
                m_AddToolBarButton.Enabled = false;
                m_RemoveToolBarButton.Enabled = false;
            }

            WorkflowNode parentWorkflowNode = m_LotDetails.GetWorkflowNode() as WorkflowNode;
            if (parentWorkflowNode != null)
            {
                CreateLotNode createLotNode = parentWorkflowNode.Node as CreateLotNode;
                if (createLotNode != null)
                {
                    m_AddToolBarButton.Enabled = createLotNode.ModifyParentLots;
                    m_RemoveToolBarButton.Enabled = createLotNode.ModifyParentLots;
                }
            }

            if (m_AddToolBarButton.Enabled && lot != null)
            {
                WorkflowNode childWorkflowNode = lot.GetWorkflowNode() as WorkflowNode;
                if (childWorkflowNode != null)
                {
                    LinkedParentLotNode linkLotNode = childWorkflowNode.Node as LinkedParentLotNode;
                    if (linkLotNode != null)
                    {
                        m_RemoveToolBarButton.Enabled = !linkLotNode.MandatoryLinkLot;
                    }
                }
            }
        }

        /// <summary>
        /// Grids parent lots on validate cell.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DataGridValidateCellEventArgs"/> instance containing the event data.</param>
        private void GridParentLotsOnValidateCell(object sender, DataGridValidateCellEventArgs e)
        {
            if (e.Value != null)
            {
                double newQuantity;
                Double.TryParse(e.Value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out newQuantity);
                LotRelationBase lot = e.Entity as LotRelationBase;

                if (lot != null)
                {
                    double quantity = lot.FromLot.Quantity;

                    if (newQuantity.Equals(0) && quantity.Equals(0))
                    {
                        return;
                    }

                    if (newQuantity > quantity)
                    {
                        string errorMessage = Library.Message.GetMessage("WorkflowMessages", "ParentLotQuantityExceeded");
                        m_Form.GridParentLots.SetCellError(e.Entity, e.Column, errorMessage);
                    }
                    else
                    {
                        m_Form.GridParentLots.ClearCellError(e.Entity, e.Column);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Selected event of the Form control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectedEventArgs"/> instance containing the event data.</param>
        private void Form_Selected(object sender, SelectedEventArgs e)
        {
            if (e.FormName == m_Form.Page_Relations.Name && m_RefreshBrowse)
            {
                m_RefreshBrowse = false;
            }

            if (e.FormName == m_Form.Page_JobSamples.Name)
            {
                // Select first job
                if (m_Form.DataQueryRelatedJobs.ResultData.Count > 0)
                {
                    UpdateSampleBrowse((JobHeader)m_Form.DataQueryRelatedJobs.ResultData[0]);
                }
            }
        }

        /// <summary>
        /// Add Toolbar Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddToolBarButton_Click(object sender, EventArgs e)
        {
            m_Form.GridParentLots.GridData.Add(EntityManager.CreateEntity(LotRelationBase.EntityName));
        }

        /// <summary>
        /// Remove Toolbar Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveToolBarButton_Click(object sender, EventArgs e)
        {
            m_Form.GridParentLots.GridData.Remove(m_Form.GridParentLots.FocusedEntity);
        }

        /// <summary>
        /// Update the lot id for lot login.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerValidator_Validate(object sender, Library.ClientControls.Validation.ServerValidatorEventArgs e)
        {
            if (IsLogin)
            {
                m_LotDetails.UpdatedLotId();
            }
        }

        /// <summary>
        /// Cell Value Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridLotProperties_CellValueChanged(object sender, UnboundGridValueChangedEventArgs e)
        {
            object value = e.Value;

            if (value is DateTime)
            {
                value = new NullableDateTime((DateTime)value);
            }

            object entityValue = ((IEntity)m_LotDetails).Get(e.Column.Name);

            if (entityValue != value)
            {
                Library.Task.StateModified();
            }
        }

        #endregion

        #region Inventories

        /// <summary>
        /// Toggles the parent inventories.
        /// </summary>
        private void ToggleInventories()
        {
            DataGridColumn col = m_Form.GridChildLots.GetColumnByProperty("DisplayQuantityConsumed");
            col.Visible = m_LotDetails.TrackInventory;

            foreach (LotRelation relation in m_LotDetails.ParentLots)
            {
                ToggleParentInventory(relation);
            }
        }

        /// <summary>
        /// Handles the ItemChanged event of the ParentLots control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EntityCollectionEventArgs"/> instance containing the event data.</param>
        private void ParentLots_ItemChanged(object sender, EntityCollectionEventArgs e)
        {
            LotRelation relation = (LotRelation)e.Entity;
            ToggleParentInventory(relation);
        }

        /// <summary>
        /// Handles the ItemAdded event of the ParentLots control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EntityCollectionEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void ParentLots_ItemAdded(object sender, EntityCollectionEventArgs e)
        {
            LotRelation relation = (LotRelation)e.Entity;
            ToggleParentInventory(relation);
        }

        /// <summary>
        /// Toggles the parent inventory.
        /// </summary>
        /// <param name="relation">The relation.</param>
        private void ToggleParentInventory(LotRelation relation)
        {
            bool enabled = false;

            if (BaseEntity.IsValid(relation.ToLot))
            {
                enabled = relation.ToLot.TrackInventory;
            }

            DataGridColumn quantity = m_Form.GridParentLots.GetColumnByProperty(LotRelationPropertyNames.QuantityConsumed);
            DataGridColumn units = m_Form.GridParentLots.GetColumnByProperty(LotRelationPropertyNames.Units);
            DataGridColumn avail = m_Form.GridParentLots.GetColumnByProperty("FromLot.DisplayQuantityAvailable");

            if (enabled)
            {
                quantity.EnableCell(relation);
                units.EnableCell(relation);
                avail.EnableCell(relation);
            }
            else
            {
                quantity.DisableCell(relation, DisabledCellDisplayMode.GreyHideContents);
                units.DisableCell(relation, DisabledCellDisplayMode.GreyHideContents);
                avail.DisableCell(relation, DisabledCellDisplayMode.GreyHideContents);
            }
        }

        #endregion

        #region Inventory - reserveration setup & events

        private void ReservationSetup()
        {
            if (Context.LaunchMode == "MODIFY" && !m_LotDetails.Status.IsPhrase(PhraseLotStat.PhraseIdX))
            {
                m_ToolBarButtonReserve = m_Form.ReservationToolBar.FindButton("Reserve");
                m_ToolBarButtonReserve.ToolTip = Library.Message.GetMessage("WorkflowMessages", "ReservationTooltipReserve");
                m_ToolBarButtonReserve.Click += ToolBarButtonReserveOnClick;

                m_ToolBarButtonTake = m_Form.ReservationToolBar.FindButton("Take");
                m_ToolBarButtonTake.ToolTip = Library.Message.GetMessage("WorkflowMessages", "ReservationTooltipTake");
                m_ToolBarButtonTake.Click += ToolBarButtonTakeOnClick;

                m_ToolBarButtonComplete = m_Form.ReservationToolBar.FindButton("Complete");
                m_ToolBarButtonComplete.ToolTip = Library.Message.GetMessage("WorkflowMessages", "ReservationTooltipComplete");
                m_ToolBarButtonComplete.Click += ToolBarButtonCompleteOnClick;

                m_ToolBarButtonCancel = m_Form.ReservationToolBar.FindButton("Cancel");
                m_ToolBarButtonCancel.ToolTip = Library.Message.GetMessage("WorkflowMessages", "ReservationTooltipCancel");
                m_ToolBarButtonCancel.Click += ToolBarButtonCancelOnClick;

                m_Form.ReservationLog.FocusedRowChanged += ReservationLogOnFocusedRowChanged;

                if (m_Form.ReservationLog.GridData.Count > 0)
                {
                    m_ToolBarButtonTake.Enabled = true;
                    m_ToolBarButtonComplete.Enabled = true;
                    m_ToolBarButtonCancel.Enabled = true;
                }
            }
            else
            {
                m_Form.ReservationToolBar.FindButton("Reserve").Enabled = false;
            }
        }

        /// <summary>
        /// Reservations the toolbar button check.
        /// </summary>
        private void ReseravtionToolbarButtonCheck()
        {
            if (m_Form.ReservationLog.GridData.Count > 0)
            {
                bool state = m_Form.ReservationLog.GridData.Count > 0;

                m_ToolBarButtonTake.Enabled = state;
                m_ToolBarButtonComplete.Enabled = state;
                m_ToolBarButtonCancel.Enabled = state;
            }
        }

        /// <summary>
        /// Tools the bar button reserve on click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void ToolBarButtonReserveOnClick(object sender, EventArgs e)
        {

            if (m_FormLotReservation == null || m_CurrentMode != InventoryMode.Reserve)
            {
                m_CurrentMode = InventoryMode.Reserve;

                m_FormLotReservation = (FormLotReservation)FormFactory.CreateForm("LotReservation");
                m_FormLotReservation.Created += FormResOnCreatedReserve;
                m_FormLotReservation.Loaded += FormLotReservationOnLoaded;
                m_FormLotReservation.Closing += FormLotReservationOnClosing;
                m_FormLotReservation.Closed += FormLotReservationOnClosed;
            }

            m_LotDetails.InventoryDisplayQuantityReserved = m_LotDetails.DisplayQuantityReserved;
            m_LotDetails.InventoryComment = string.Empty;

            m_FormLotReservation.ShowDialog();
        }

        /// <summary>
        /// Tools the bar button take on click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ToolBarButtonTakeOnClick(object sender, EventArgs e)
        {

            if (m_LotReservation != null && (m_LotReservation.Status.IsPhrase(PhraseLotresstat.PhraseIdV) ||
                                             m_LotReservation.Status.IsPhrase(PhraseLotresstat.PhraseIdP)) && !m_LotReservation.IsNew())
            {
                if (m_FormLotReservation == null || m_CurrentMode != InventoryMode.Take)
                {
                    m_CurrentMode = InventoryMode.Take;
                    m_FormLotReservation = (FormLotReservation)FormFactory.CreateForm("LotReservation");
                    m_FormLotReservation.Created += FormResOnCreatedTake;
                    m_FormLotReservation.Loaded += FormLotReservationOnLoaded;
                    m_FormLotReservation.Closing += FormLotReservationOnClosing;
                    m_FormLotReservation.Closed += FormLotReservationOnClosed;
                }

                m_LotDetails.InventoryComment = m_LotReservation.Comments;
                m_LotDetails.InventoryDisplayQuantityReserved = m_LotReservation.DisplayQuantityReserved;
                m_FormLotReservation.ShowDialog();
            }
            else
            {
                Library.Utils.FlashMessage(
                    Library.Message.GetMessage("WorkflowMessages", "ReservationTakeMsg"),
                    Library.Message.GetMessage("WorkflowMessages", "ReservationTakeCaption"));
            }
        }

        /// <summary>
        /// Tools the bar button complete on click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ToolBarButtonCompleteOnClick(object sender, EventArgs e)
        {
            if (!m_LotReservation.CompleteReservation())
            {
                Library.Utils.FlashMessage(
                    Library.Message.GetMessage("WorkflowMessages", "ReservationStatusChangeMsg"),
                    Library.Message.GetMessage("WorkflowMessages", "ReservationStatusChangeCaption"));
            }
        }

        /// <summary>
        /// Tools the bar button cancel on click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ToolBarButtonCancelOnClick(object sender, EventArgs e)
        {
            if (!m_LotReservation.CancelReservation())
            {
                Library.Utils.FlashMessage(
                    Library.Message.GetMessage("WorkflowMessages", "ReservationStatusChangeMsg"),
                    Library.Message.GetMessage("WorkflowMessages", "ReservationStatusChangeCaption"));
            }
        }

        /// <summary>
        /// Reservations the log on focused row changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DataGridFocusedRowChangedEventArgs"/> instance containing the event data.</param>
        private void ReservationLogOnFocusedRowChanged(object sender, DataGridFocusedRowChangedEventArgs e)
        {
            // Sets the currently selected lot reservation

            m_LotReservation = (LotReservation)e.Row;
        }

        /// <summary>
        /// Reservation form created.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void FormResOnCreatedReserve(object sender, EventArgs e)
        {
            m_FormLotReservation.Title = m_FormLotReservation.StringTable.ReserveInventoryTitle;
        }

        /// <summary>
        /// Reservation form created.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void FormResOnCreatedTake(object sender, EventArgs e)
        {
            m_FormLotReservation.Title = String.Format(m_FormLotReservation.StringTable.TakeInventoryTitle, m_LotReservation.OrderNum);
        }

        /// <summary>
        /// Forms the lot reservation on loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void FormLotReservationOnLoaded(object sender, EventArgs eventArgs)
        {
            m_FormLotReservation.RadioButtonPct.CheckedChanged += RadioButtonPctOnCheckedChanged;
            m_FormLotReservation.RadioButtonAbs.CheckedChanged += RadioButtonAbsOnCheckedChanged;
            m_FormLotReservation.Quantity.NumberChanged += QuantityOnNumberChanged;
            m_FormLotReservation.QuantityPercentage.NumberChanged += QuantityPercentageOnNumberChanged;
            m_FormLotReservation.PromptEntityBrowseUnits.EntityChanged += PromptEntityBrowseUnitsOnEntityChanged;
        }

        /// <summary>
        /// Forms the lot reservation on closing.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        private void FormLotReservationOnClosing(object sender, CancelEventArgs e)
        {
            if (m_FormLotReservation.FormResult == FormResult.OK)
            {
                string errorMsg = String.Empty;

                if (ValidateReservationFormData(out errorMsg))
                {
                    // Standard
                    //double quantity = m_LotDetails.QuantityConversionCalculation(m_FormLotReservation.Quantity.Number, m_Units, m_LotDetails.Units);

                    // BSmock - change phrase to string
                    double quantity = m_LotDetails.QuantityConversionCalculation(m_FormLotReservation.Quantity.Number, m_Units, m_LotDetails.Units.PhraseId);

                    string comments = m_FormLotReservation.Comments.Text;

                    if (m_CurrentMode == InventoryMode.Reserve)
                    {
                        m_LotDetails.AddLotReservation(quantity, PhraseLotresstat.PhraseIdV, comments);
                    }
                    else
                    {
                        m_LotDetails.TakeReservation(m_LotReservation.LotReservationGuid, quantity, comments);
                    }

                    return;
                }

                if (m_FormLotReservation.RadioButtonAbs.Checked)
                {
                    m_FormLotReservation.Quantity.ShowError(errorMsg);
                }
                else
                {
                    m_FormLotReservation.QuantityPercentage.ShowError(errorMsg);
                }

                e.Cancel = true;
            }
        }

        /// <summary>
        /// Forms the lot reservation on closed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void FormLotReservationOnClosed(object sender, EventArgs eventArgs)
        {
            m_FormLotReservation.RadioButtonPct.CheckedChanged -= RadioButtonPctOnCheckedChanged;
            m_FormLotReservation.RadioButtonAbs.CheckedChanged -= RadioButtonAbsOnCheckedChanged;
            m_FormLotReservation.Quantity.NumberChanged -= QuantityOnNumberChanged;
            m_FormLotReservation.QuantityPercentage.NumberChanged -= QuantityPercentageOnNumberChanged;
            m_FormLotReservation.PromptEntityBrowseUnits.EntityChanged -= PromptEntityBrowseUnitsOnEntityChanged;
        }

        #endregion

        #region Inventory - reservation radio toggle

        /// <summary>
        /// Toggle Controls
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="checkedChangedEventArgs">The <see cref="CheckedChangedEventArgs"/> instance containing the event data.</param>
        private void RadioButtonAbsOnCheckedChanged(object sender, CheckedChangedEventArgs checkedChangedEventArgs)
        {
            if (checkedChangedEventArgs.Checked)
            {
                ToggleAbs(true);
            }
        }

        /// <summary>
        /// Toggle Controls
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="checkedChangedEventArgs">The <see cref="CheckedChangedEventArgs"/> instance containing the event data.</param>
        private void RadioButtonPctOnCheckedChanged(object sender, CheckedChangedEventArgs checkedChangedEventArgs)
        {
            if (checkedChangedEventArgs.Checked)
            {
                ToggleAbs(false);
            }
        }

        /// <summary>
        /// Toggles the abs.
        /// </summary>
        /// <param name="abs">if set to <c>true</c> [abs].</param>
        private void ToggleAbs(bool abs)
        {
            m_FormLotReservation.QuantityPercentage.Enabled = !abs;
            m_FormLotReservation.QuantityPercentage.Mandatory = !abs;
            m_FormLotReservation.Quantity.Enabled = abs;
            m_FormLotReservation.Quantity.Mandatory = abs;
        }

        #endregion

        #region Inventory - reservation validation & calculations

        /// <summary>
        /// Validates the specified error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private bool ValidateReservationFormData(out string errorMessage)
        {
            if (m_FormLotReservation.Quantity.Number <= 0)
            {
                errorMessage = m_FormLotReservation.StringTable.QuantityNotZero;
                return false;
            }

            // Standard
            //double result = m_CurrentMode == InventoryMode.Reserve
            //    ? m_LotDetails.QuantityAvailable -
            //      m_LotDetails.QuantityConversionCalculation(m_FormLotReservation.Quantity.Number, m_Units, m_LotDetails.Units)
            //    : (m_LotReservation.QuantityReserved - m_LotReservation.QuantityTaken) -
            //      m_LotDetails.QuantityConversionCalculation(m_FormLotReservation.Quantity.Number, m_Units, m_LotDetails.Units);

            // BSmock - change phrase to string
            double result = m_CurrentMode == InventoryMode.Reserve
                ? m_LotDetails.QuantityAvailable -
                  m_LotDetails.QuantityConversionCalculation(m_FormLotReservation.Quantity.Number, m_Units, m_LotDetails.Units.PhraseId)
                : (m_LotReservation.QuantityReserved - m_LotReservation.QuantityTaken) -
                  m_LotDetails.QuantityConversionCalculation(m_FormLotReservation.Quantity.Number, m_Units, m_LotDetails.Units.PhraseId);

            errorMessage = m_CurrentMode == InventoryMode.Reserve
                ? m_FormLotReservation.StringTable.QuantityAvailableExceeded
                : m_FormLotReservation.StringTable.QuantityReservedExceeded;
            return result >= 0;
        }

        /// <summary>
        /// Quantities the percentage on number changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RealChangedEventArgs"/> instance containing the event data.</param>
        private void QuantityPercentageOnNumberChanged(object sender, RealChangedEventArgs e)
        {
            // Prevent recursive calling

            if (m_FormLotReservation.RadioButtonPct.Checked)
            {
                m_FormLotReservation.Quantity.Number = m_CurrentMode == InventoryMode.Reserve
                    ? m_LotDetails.ReservationQuantityAvailableFromPercentage(e.Number, m_Units)
                    : m_LotReservation.ReservationQuantityReservedFromPercentage(e.Number, m_Units);
            }
        }

        /// <summary>
        /// Quantities the on number changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RealChangedEventArgs"/> instance containing the event data.</param>
        private void QuantityOnNumberChanged(object sender, RealChangedEventArgs e)
        {
            // Prevent recursive calling

            if (m_FormLotReservation.RadioButtonAbs.Checked)
            {
                m_FormLotReservation.QuantityPercentage.Number = m_CurrentMode == InventoryMode.Reserve
                    ? m_LotDetails.ReservationPercentageFromQuantityAvailable(e.Number, m_Units)
                    : m_LotReservation.ReservationPercentageFromQuantityReserved(e.Number, m_Units);
            }
        }

        /// <summary>
        /// Prompts the entity browse units on entity changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EntityChangedEventArgs"/> instance containing the event data.</param>
        private void PromptEntityBrowseUnitsOnEntityChanged(object sender, EntityChangedEventArgs e)
        {
            if (e.Entity != null)
            {
                m_Units = ((UnitHeaderBase)e.Entity).Identity;

                if (m_FormLotReservation.RadioButtonPct.Checked)
                {
                    m_FormLotReservation.Quantity.Number = m_CurrentMode == InventoryMode.Reserve
                        ? m_LotDetails.ReservationQuantityAvailableFromPercentage(m_FormLotReservation.QuantityPercentage.Number, m_Units)
                        : m_LotReservation.ReservationQuantityReservedFromPercentage(m_FormLotReservation.QuantityPercentage.Number, m_Units);
                }
                else
                {
                    m_FormLotReservation.QuantityPercentage.Number = m_CurrentMode == InventoryMode.Reserve
                        ? m_LotDetails.ReservationPercentageFromQuantityAvailable(m_FormLotReservation.Quantity.Number, m_Units)
                        : m_LotReservation.ReservationPercentageFromQuantityReserved(m_FormLotReservation.Quantity.Number, m_Units);
                }
            }
        }

        #endregion
    }
}