using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Utilities;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;

namespace Customization.Tasks
{
    /// <summary>
    /// New job and samples from web queue selection
    /// </summary>
    [SampleManagerTask(nameof(WebQueueCreateJobTask))]
    public class WebQueueCreateJobTask : DeibelSampleAdminBaseTask
    {
        #region Member Variables

        /// <summary>
        /// First sample in the selection that was used to launch this task
        /// </summary>
        protected Sample m_ContextSample;

        #endregion

        #region Setup

        protected override void SetupTask()
        {
            if (Context.SelectedItems.Count == 0)
                return;

            base.SetDefaultWorkflows();

            _selectedSamples = Context.SelectedItems.Cast<Sample>().ToList();
            _jobHeader = _selectedSamples[0].JobName as JobHeader;
            if(_jobHeader == null)            
                return;
            
            base.SetupTask();
        }

        /// <summary>
        /// Called when the <see cref="DefaultFormTask.MainForm"/> has been loaded.
        /// </summary>
        protected override void MainFormLoaded()
        {
            base.MainFormLoaded();

            AddSelectedSamplesToJob();

            if (HasExited)
                return;
            
            // Set Tree Focused node to the node passed in.
            SimpleTreeListNodeProxy contextTreeNode = m_Form.TreeListItems.FindNodeByData(_jobHeader);
            if (contextTreeNode == null)
                return;
            m_FocusedTreeEntity = _jobHeader;
            m_Form.TreeListItems.FocusNode(contextTreeNode);

            //Refresh the tests grid to avoid a delay in the grid loading
            RefreshSamplesGrid();
            m_VisibleGrid = m_Form.GridSampleProperties;
            m_TabPageSamples.Show();
            m_ToolBarButtonTranspose.Enabled = true;
        }

        #endregion

        #region Data Initialisation

        /// <summary>
        /// Validates the context.
        /// </summary>
        /// <returns></returns>
        protected override bool ValidateContext()
        {
            // Make sure a sample has been selected

            if (Context.SelectedItems.ActiveCount == 0)            
                return false;

            return true;
        }

        /// <summary>
        /// Initialises the data.
        /// </summary>
        /// <returns></returns>
        protected override bool InitialiseTopLevelEntities(out IEntityCollection topLevelEntities)
        {
            topLevelEntities = EntityManager.CreateEntityCollection(JobHeader.EntityName);
            topLevelEntities.Add(_jobHeader);
            return true;
        }

        void AddSelectedSamplesToJob()
        {
            // Log in samples
            var newEntities = new List<IEntity>();
            RunWorkflowForEntity(_jobHeader, _defaultSampleWorkflow, Context.SelectedItems.ActiveCount, newEntities);
            UpdateTestAssignmentGrid(_defaultSampleWorkflow.TableName, newEntities);
        }

        #endregion

        #region General Overrides

        /// <summary>
        /// Gets the name of the top level table.
        /// </summary>
        /// <returns></returns>
        protected override string GetTopLevelTableName()
        {
            return JobHeader.EntityName;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is job workflow.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is job workflow; otherwise, <c>false</c>.
        /// </value>
        protected override bool IsJobWorkflow
        {
            get { return true; }
        }

        /// <summary>
        /// Sets the title.
        /// </summary>
        /// <returns></returns>
        protected override string GetTitle()
        {
            return m_Form.StringTable.TitleSampleAdmin;
        }

        protected override void RunDefaultWorkflow()
        {
            // For new job logins, this class is for modifying existing jobs
        }

        #endregion
    }
}
