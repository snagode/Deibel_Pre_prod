using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.ClientControls.Browse;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;
using Thermo.SampleManager.Tasks;


namespace Customization.Tasks
{
    public abstract class ExtendedJobLoginBase : SampleAdminBaseTask
    {
        #region Implementation stuff

        protected abstract string TopTableName { get; }
        protected abstract bool JobWorkflow { get; }
        protected abstract string Title { get; }
        protected abstract IEntityCollection TopEntities();
        protected abstract Workflow InitialWorkflow { get; }

        protected override bool IsJobWorkflow => JobWorkflow;
        protected override string GetTitle() => Title;
        protected override string GetTopLevelTableName() => TopTableName;
        protected override bool InitialiseTopLevelEntities(out IEntityCollection topLevelEntities)
        {
            topLevelEntities = TopEntities();
            if (topLevelEntities == null)
                return false;

            return true;
        }

        #endregion

        LoginUtils _utils;
        protected LoginUtils Utils
        {
            get
            {
                if (_utils != null)
                    return _utils;

                _utils = new LoginUtils(EntityManager, Library);
                return _utils;
            }
        }
        List<JobHeader> Jobs
        {
            get
            {
                return m_RootNode.Nodes.Select(n => n.Data).Cast<JobHeader>().ToList();
            }
        }

        CopyTracker _csTrack;
        CopyTracker _cjTrack;

        ContextMenuItem _copyJobBtn;
        ContextMenuItem _copyAnySampleBtn;
        ContextMenuItem _copyJobSampleBtn;
        ContextMenuItem _copyThisSampleBtn;


        protected override void SetupTask()
        {
            DefaultWorkflow = InitialWorkflow;

            base.SetupTask();
        }

        protected override void MainFormLoaded()
        {
            base.MainFormLoaded();

            if (m_Form == null)
                return;

            Utils.SetForm(m_Form);

            // Sample copy menu items
            _csTrack = new CopyTracker(Sample.EntityName, EntityManager, Utils);
            _copyAnySampleBtn = Utils.CopyButton(DeibelMenuItem.CopyAnySample);  // Same functionality, different browse contents
            _copyJobSampleBtn = Utils.CopyButton(DeibelMenuItem.CopyJobSample);
            _copyThisSampleBtn = Utils.CopyButton(DeibelMenuItem.CopySample);
            _copyAnySampleBtn.ItemClicked += CopySampleClicked;
            _copyJobSampleBtn.ItemClicked += CopySampleClicked;
            _copyThisSampleBtn.ItemClicked += CopySampleClicked;

            // Job copy menu item
            _cjTrack = new CopyTracker(JobHeader.EntityName, EntityManager, Utils);
            _copyJobBtn = Utils.CopyButton(DeibelMenuItem.CopyJob);
            _copyJobBtn.ItemClicked += CopyJobClicked;

            AddIncrementButtons();

            // Some of these fire after events subscribed in SampleAdminBaseTask
            m_Form.TreeListItems.NodeAdded += ExtendedTreeListItems_NodeAdded;
            m_Form.TreeListItems.ContextMenu.BeforePopup += ContextMenu_BeforePopup;
            m_Form.GridSampleProperties.CellValueChanged += GridSampleProperties_CellValueChanged;
            m_Form.GridSampleProperties.ServerFillingValue += GridSampleProperties_ServerFillingValue;
            m_Form.GridJobProperties.CellValueChanged += GridJobProperties_CellValueChanged;
            m_Form.GridJobProperties.ServerFillingValue += GridJobProperties_ServerFillingValue;
        }


        void AddIncrementButtons()
        {
            int size = 16;
            var iconService = Library.GetService<IIconService>();

            Image img;
            ToolBarButton incDec;

            // Increment Left
            img = iconService.LoadImage(new IconName("INT_INFORMATION"), size);
            incDec = m_Form.ToolBarTree.AddButton("SetValue", img, ToolBarButtonAlignment.Right);
            incDec.ToolTip = $"Increment value = {_incVal}";
            incDec.BeginGroup = true;
            incDec.Click += InrementDecrement_Click;

            // Decrement left
            img = iconService.LoadImage(new IconName("MEDIA_FAST_FORWARD"), size);
            incDec = m_Form.ToolBarTree.AddButton("Increment", img, ToolBarButtonAlignment.Right);
            incDec.ToolTip = "Increment values from selected cell to rightmost cell.";
            incDec.Click += InrementDecrement_Click;
        }

        int _incVal = 1;
        private void InrementDecrement_Click(object sender, EventArgs e)
        {
            var s = sender as ToolBarButton;
            if (s == null)
                return;

            switch (s.Name)
            {
                case "Increment":
                    Utils.AutofillButtonClicked(m_Form.GridSampleProperties, _incVal);
                    break;
                case "SetValue":
                    PromptForIncrementValue(s);
                    break;
            }
        }

        void PromptForIncrementValue(ToolBarButton btn)
        {
            int? i = Library.Utils.PromptForInteger(@"Set value used by increment/decrement", "Set Value", 1, 100, _incVal);
            if (i == null)
                _incVal = 1;
            else
                _incVal = (int)i;

            btn.ToolTip = $"Increment value = {_incVal}";
        }

        protected void GridRowAdded(UnboundGridRow row, UnboundGridColumn column)
        {
            if (row == null || column == null || row.Tag == null || string.IsNullOrWhiteSpace(column.Name))
                return;

            var e = row.Tag as IEntity;

            if (column.Name == "CustomerId")
            {
                if (e.EntityType == JobHeader.EntityName)
                {
                    EntityBrowse criteriaBrowse = BrowseFactory.CreateEntityBrowse(Utils.CustomerQuery());
                    column.ValueChanged += CustomerColumn_ValueChanged;
                    column.SetCellBrowse(row, criteriaBrowse);
                    UpdateBrowses(e.Get("CustomerId") as Customer, row);
                }
            }
        }

        void UpdateBrowses(Customer customer, UnboundGridRow row)
        {
            if (customer == null || customer.IsNull())
                return;

            var col1 = m_Form.GridJobProperties.GetColumnByName("ReportToName");
            var col2 = m_Form.GridJobProperties.GetColumnByName("InvoiceToName");

            if (col1 != null)
            {
                var browse = BrowseFactory.CreateEntityBrowse(CustomerContactsBase.EntityName);
                browse.ReturnProperty = CustomerContactsPropertyNames.ContactName;
                col1.SetCellBrowse(row, browse);
                browse.Republish(customer.CustomerContacts);
            }
            if (col2 != null)
            {
                var browse = BrowseFactory.CreateEntityBrowse(CustomerContactsBase.EntityName);
                browse.ReturnProperty = CustomerContactsPropertyNames.ContactName;
                col2.SetCellBrowse(row, browse);
                browse.Republish(customer.CustomerContacts);
            }
        }

        void CustomerColumn_ValueChanged(object sender, UnboundGridValueChangedEventArgs e)
        {
            if (e?.Row?.Tag == null)
                return;

            UpdateBrowses(e.Value as Customer, e.Row);
        }

        private void GridJobProperties_ServerFillingValue(object sender, UnboundGridValueChangingEventArgs e)
        {
            Utils.Refresh(e);
        }
        private void GridJobProperties_CellValueChanged(object sender, UnboundGridValueChangedEventArgs e)
        {
            Utils.Refresh(e);
        }
        private void GridSampleProperties_CellValueChanged(object sender, UnboundGridValueChangedEventArgs e)
        {
            Utils.Refresh(e);
        }
        private void GridSampleProperties_ServerFillingValue(object sender, UnboundGridValueChangingEventArgs e)
        {
            Utils.Refresh(e);
        }

        private void ContextMenu_BeforePopup(object sender, ContextMenuBeforePopupEventArgs e)
        {
            _copyJobBtn.Visible = false;
            _copyThisSampleBtn.Visible = false;
            _copyJobSampleBtn.Visible = false;
            _copyAnySampleBtn.Visible = false;

            // We're at form's root node
            if (!BaseEntity.IsValid(e.Entity))
                return;

            if (e.Entity.EntityType == JobHeader.EntityName)
            {
                var j = e.Entity as JobHeader;
                _csTrack.SelectedJob = j;
                _cjTrack.SelectedJob = j;
                _copyJobSampleBtn.Visible = true;
                _copyAnySampleBtn.Visible = true;
            }
            if (e.Entity.EntityType == Sample.EntityName)
            {
                _csTrack.CopyEntity = e.Entity;
                _copyThisSampleBtn.Visible = true;
            }
        }

        /// <summary>
        /// Use node added to track new entities produced in SampleAdminBaseTask
        /// </summary>
        private void ExtendedTreeListItems_NodeAdded(object sender, SimpleTreeListNodeEventArgs e)
        {
            var entity = e.Node.Data as IEntity;
            if (!BaseEntity.IsValid(entity) || !entity.IsNew())
                return;

            // New empty sample, still want the job info on it
            if (!_csTrack.DoCopy && entity.IsNew() && entity.EntityType == Sample.EntityName)
            {
                var job = m_FocusedTreeEntity as JobHeader;
                if (job != null && !job.IsNull())
                {
                    Utils.PropagateJob(job, entity as Sample);
                }
            }

            if (_csTrack.CopyCompleted(entity))
            {
                // These samples came from a different job, update them
                if (_csTrack.ShowAllSamples)
                {
                    entity.Set("JobName", _csTrack.SelectedJob);
                    foreach (Sample sample in _csTrack.NewEntities)
                    {
                        Utils.PropagateJob(_csTrack.SelectedJob, sample);
                    }
                }
                if (m_VisibleGrid == m_Form.GridSampleProperties)
                    RefreshGrid(m_VisibleGrid, _csTrack.NewEntities);
                _csTrack.Reset();
            }

            if (_cjTrack.CopyCompleted(entity))
            {
                if (m_VisibleGrid == m_Form.GridJobProperties)
                    RefreshGrid(m_VisibleGrid, _cjTrack.NewEntities);
                RefreshTree(_cjTrack.NewEntities);
                _cjTrack.Reset();
            }

            // Need event handler to assign values to custom test fields on test added
            if (entity.EntityType == Sample.EntityName)
                ((Sample)entity).ChildItemAdded += SampleChildItemAdded;
        }

        private void SampleChildItemAdded(object sender, ChildObjectEventArgs e)
        {
            var test = e.ChildObject as Test;
            if (test == null)
                return;

            Utils.AssignTestCustomFields(e.ChildObject as Test);
        }

        /// <summary>
        /// Because job's copied samples are not produced by SampleAdminBaseTask we need to refresh
        /// tree manually.
        /// </summary>
        void RefreshTree(IEntityCollection newEntities)
        {
            foreach (JobHeader job in newEntities)
            {
                var targetNode = m_Form.TreeListItems.FindNodeByData(job);
                foreach (IEntity entity in job.Samples)
                {
                    IconName icon = new IconName(entity.Icon);
                    string displayText = GetEntityDisplayText(entity);
                    m_Form.TreeListItems.AddNode(targetNode, displayText, icon, entity);
                }
            }
        }

        /// <summary>
        /// In SampleAdminBaseTask, a new thread is allocated when using the RunWorkflow routine called here.
        /// However, the thread is queued before this routine is called, instead of within it.
        /// </summary>
        void PerformCopyEntity(Workflow workflow, int count)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    if (Library.Environment.GetGlobalInt("CLIENT_TYPE") == 1)
                    {
                        // In web client, a delay is needed to avoid execution of the workflow happening
                        // before initialization of controls on client side.

                        Thread.Sleep(500);
                    }

                    //RunWorkflow(workflow, count);
                }
                catch (Exception ex)
                {
                    OnException(ex);
                }
            });
        }

        /// <summary>
        /// Valids the status for modify.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        protected bool ValidStatusForModify(IEntity entity)
        {
            return Utils.ValidStatusForModify(entity);
        }

        #region Job Copying

        void CopyJobClicked(object sender, ContextMenuItemEventArgs e)
        {
            _cjTrack.DoCopy = true;
            _cjTrack.RootNode = m_Form.TreeListItems.FocusedNode;

            // Use workflow form to choose job
            FormSampleAdminAddWorkflow form = (FormSampleAdminAddWorkflow)FormFactory.CreateForm(typeof(FormSampleAdminAddWorkflow));
            form.Loaded += CopyJobFormLoaded;
            form.ShowDialog();
        }

        private void CopyJobFormLoaded(object sender, EventArgs e)
        {
            var form = sender as FormSampleAdminAddWorkflow;

            form.Closed += CopyJobFormClosed;
            form.LabelAddNewWorkflow.Caption = "Select job";
            form.Title = "Select job";

            // Load browse with jobs
            var col = EntityManager.CreateEntityCollection(JobHeader.EntityName);
            foreach (IEntity job in Jobs)
            {
                col.Add(job);
            }
            var browse = BrowseFactory.CreateEntityBrowse(col);
            browse.AddColumnsFromTableDefaults();
            form.PromptEntityBrowseWorkflow.Browse = browse;
            form.PromptEntityBrowseWorkflow.Caption = "Choose Job";
        }

        private void CopyJobFormClosed(object sender, EventArgs e)
        {
            FormSampleAdminAddWorkflow form = (FormSampleAdminAddWorkflow)sender;

            if (form.FormResult == FormResult.OK)
            {
                var job = (JobHeader)form.PromptEntityBrowseWorkflow.Entity;
                var workflow = Utils.GetLinkedLoginWorkflow(job);
                int count = form.SpinEditNewCount.Number;
                _cjTrack.CopyCount = count;
                _cjTrack.CopyEntity = job;

                PerformCopyEntity(workflow, count);
            }
        }

        #endregion

        #region Sample Copying

        private void CopySampleClicked(object sender, ContextMenuItemEventArgs e)
        {
            _csTrack.DoCopy = true;
            _csTrack.RootNode = m_Form.TreeListItems.FocusedNode;
            _csTrack.ShowAllSamples = e.Item.Caption == LoginUtils.CopyAnySampleCaption ? true : false;

            var btn = sender as ContextMenuItem;
            if (btn.Caption == LoginUtils.CopySampleCaption)
            {
                var workflow = Utils.GetLinkedLoginWorkflow(_csTrack.CopyEntity);
                var count = Library.Utils.PromptForInteger("Number of samples", "", 1);
                if (count == null)
                    return;

                _csTrack.CopyCount = (int)count;
                PerformCopyEntity(workflow, (int)count);
            }
            else
            {
                // Use workflow form for sample choice
                FormSampleAdminAddWorkflow form = (FormSampleAdminAddWorkflow)FormFactory.CreateForm(typeof(FormSampleAdminAddWorkflow));
                form.Loaded += CopySampleFormLoaded;
                form.ShowDialog();
            }
        }

        private void CopySampleFormLoaded(object sender, EventArgs e)
        {
            var form = sender as FormSampleAdminAddWorkflow;
            form.Closed += CopySampleFormClosed;
            form.LabelAddNewWorkflow.Caption = "Select sample";
            form.Title = "Select Sample";

            // Build sample browse based on menu item choice
            if (_csTrack.ShowAllSamples)
            {
                var col = EntityManager.CreateEntityCollection(Sample.EntityName);
                foreach (var job in Jobs)
                {
                    foreach (IEntity sample in job.Samples)
                    {
                        col.Add(sample);
                    }
                }
                var browse = BrowseFactory.CreateEntityBrowse(col);
                browse.AddColumnsFromTableDefaults();
                form.PromptEntityBrowseWorkflow.Browse = browse;
            }
            else
            {
                var browse = BrowseFactory.CreateEntityBrowse(_cjTrack.SelectedJob.Samples);
                browse.AddColumnsFromTableDefaults();
                form.PromptEntityBrowseWorkflow.Browse = browse;
            }
            form.PromptEntityBrowseWorkflow.Caption = "Choose Sample";
        }

        private void CopySampleFormClosed(object sender, EventArgs e)
        {
            FormSampleAdminAddWorkflow form = (FormSampleAdminAddWorkflow)sender;

            if (form.FormResult == FormResult.OK)
            {
                var sample = (Sample)form.PromptEntityBrowseWorkflow.Entity;
                var workflow = Utils.GetLinkedLoginWorkflow(sample);
                int count = form.SpinEditNewCount.Number;
                _csTrack.CopyCount = count;
                _csTrack.CopyEntity = sample;

                PerformCopyEntity(workflow, count);
            }
        }

        #endregion

    }


}
