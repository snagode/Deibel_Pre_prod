using System;
using System.Linq;
using System.Threading;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;


namespace Customization.Tasks
{
    public abstract class ExtendedSampleLoginBase : SampleAdminBaseTask
    {
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
            else
                return true;
        }

        CopyTracker _csTrack;
        ContextMenuItem _copySampleBtn;

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

            // There are also events subscribed in SampleAdminBaseTask.  These should execute after those.
            m_Form.TreeListItems.NodeAdded += ExtendedTreeListItems_NodeAdded;
            m_Form.TreeListItems.ContextMenu.BeforePopup += ContextMenu_BeforePopup;

            _csTrack = new CopyTracker(Sample.EntityName, EntityManager, Utils);
            _copySampleBtn = Utils.CopyButton(DeibelMenuItem.CopySample);
            _copySampleBtn.ItemClicked += CopySampleClicked;

            m_Form.GridSampleProperties.CellValueChanged += GridSampleProperties_CellValueChanged;
            m_Form.GridSampleProperties.ServerFillingValue += GridSampleProperties_ServerFillingValue;
        }

        /// <summary>
        /// Capture grid autofill events
        /// </summary>
        private void GridSampleProperties_ServerFillingValue(object sender, UnboundGridValueChangingEventArgs e)
        {
            Utils.Refresh(e);
        }

        private void GridSampleProperties_CellValueChanged(object sender, UnboundGridValueChangedEventArgs e)
        {
            Utils.Refresh(e);
        }

        private void ContextMenu_BeforePopup(object sender, ContextMenuBeforePopupEventArgs e)
        {
            _copySampleBtn.Visible = false;

            if (e.Entity != null && !string.IsNullOrWhiteSpace(e.Entity.ToString()))
            {
                _copySampleBtn.Visible = true;
                _csTrack.CopyEntity = e.Entity;
            }
        }

        private void CopySampleClicked(object sender, ContextMenuItemEventArgs e)
        {
            int? count = Library.Utils.PromptForInteger("Number of Samples", "Samples", defaultValue: 0);
            if (count == null || count < 1)
                return;

            _csTrack.DoCopy = true;
            _csTrack.CopyCount = (int)count;
            _csTrack.RootNode = m_RootNode;

            var wf = Utils.GetLinkedLoginWorkflow(_csTrack.CopyEntity);
            var ct = _csTrack.CopyCount;
            PerformCopyEntity(wf, ct);
        }

        /// <summary>
        /// Using this to track the new stuff
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExtendedTreeListItems_NodeAdded(object sender, SimpleTreeListNodeEventArgs e)
        {
            var entity = e.Node.Data as IEntity;
            if (!BaseEntity.IsValid(entity) || !entity.IsNew())
                return;

            if (_csTrack.CopyCompleted(entity))
            {
                if (m_VisibleGrid == m_Form.GridSampleProperties)
                    RefreshGrid(m_VisibleGrid, _csTrack.NewEntities);
                _csTrack.Reset();
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
    }
}
