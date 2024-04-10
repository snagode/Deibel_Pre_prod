using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
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
    /// Sample Admin Task - This is involved with Sample/Job Modification.
    /// </summary>
    [SampleManagerTask(nameof(DeibelSampleAdminTask))]
    public class DeibelSampleAdminTask : DeibelSampleAdminBaseTask
    {
        #region Member Variables

        /// <summary>
        /// First sample in the selection that was used to launch this task
        /// </summary>
        protected Sample m_ContextSample;

        #endregion

        #region Setup

        /// <summary>
        /// Called when the <see cref="DefaultFormTask.MainForm"/> has been loaded.
        /// </summary>
        protected override void MainFormLoaded()
        {
            base.SetDefaultWorkflows();
            _selectedSamples = EntityManager.CreateEntityCollection(Sample.EntityName).Cast<Sample>().ToList();
            base.MainFormLoaded();
            
            if (HasExited) return;

            if (m_ContextSample != null)
            {
                // Set Tree Focused node to the node passed in.
                SimpleTreeListNodeProxy contextTreeNode = m_Form.TreeListItems.FindNodeByData(m_ContextSample);
                if (contextTreeNode == null)
                    return;

                m_FocusedTreeEntity = m_ContextSample;
                m_Form.TreeListItems.FocusNode(contextTreeNode);

                //Refresh the tests grid to avoid a delay in the grid loading
                RefreshTestsGrid();
                m_VisibleGrid = m_Form.GridTestProperties;
                m_TabPageTests.Show();
                m_ToolBarButtonTranspose.Enabled = true;
            }
            else
            {
                // Job is selected
                JobHeader selectedJob = (JobHeader)Context.SelectedItems[0];
                _jobType = DeibelJobType.ExistingJob;

                // Set Tree Focused node to the node passed in.
                SimpleTreeListNodeProxy contextTreeNode = m_Form.TreeListItems.FindNodeByData(selectedJob);
                if (contextTreeNode == null)
                    return;
                m_FocusedTreeEntity = selectedJob;
                m_Form.TreeListItems.FocusNode(contextTreeNode);

                //Refresh the tests grid to avoid a delay in the grid loading
                RefreshSamplesGrid();
                m_VisibleGrid = m_Form.GridSampleProperties;
                m_TabPageSamples.Show();
                m_ToolBarButtonTranspose.Enabled = true;
            }
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
            {
                IEntity entity;
                bool sampleSelected = PromptForData(out entity);

                if (!sampleSelected)
                {
                    return false;
                }

                Context.SelectedItems.Add(entity);
            }

            // Check for items that don't have Entity Templates and warn the user
            List<string> invalidItems = new List<string>();

            CheckEntityTemplate(Context.SelectedItems, invalidItems);

            if (invalidItems.Count != 0)
            {
                // Popup an alert
                invalidItems.Sort();
                StringBuilder sb = new StringBuilder();
                foreach (string invalidItem in invalidItems)
                {
                    sb.AppendLine(invalidItem);
                }

                string alertMessage = Library.Message.GetMessage("GeneralMessages", "SampleLoginNullTemplateAlertMessage");
                string alertText = string.Format(alertMessage, sb);
                string alertTitle = Library.Message.GetMessage("GeneralMessages", "SampleLoginNullTemplateAlertTitle");
                Library.Utils.ShowAlert(alertTitle, alertText);
            }

            return CheckValidStatus(Context.SelectedItems);
        }

        /// <summary>
        /// Checks the entity template.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="invalidItems">The invalid items.</param>
        private void CheckEntityTemplate(IEntityCollection items, List<string> invalidItems)
        {
            foreach (IEntity entity in items)
            {
                if (!HasEntityTemplate(entity))
                {
                    string displayText = GetEntityDisplayText(entity);
                    displayText = string.Format("{0} {1}", entity.EntityType, displayText);
                    invalidItems.Add(displayText);
                }

                JobHeaderInternal job = entity as JobHeaderInternal;
                if (job != null)
                {
                    // Check samples
                    CheckEntityTemplate(job.Samples, invalidItems);
                    continue;
                }
            }
        }

        /// <summary>
        /// Checks the valid status.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        protected virtual bool CheckValidStatus(IEntityCollection items)
        {
            foreach (IEntity entity in items)
            {
                if (Context.LaunchMode != GenericLabtableTask.DisplayOption && !ValidStatusForModify(entity))
                {
                    string errorTitle = Library.Message.GetMessage("GeneralMessages", "SampleLoginOpenErrorTitle");
                    string error = string.Empty;
                    if (entity is Sample)
                    {
                        error = Library.Message.GetMessage("GeneralMessages", "SampleLoginOpenError", ((Sample)entity).IdText,
                            ((Sample)entity).Status.PhraseId);
                    }

                    if (entity is JobHeader)
                    {
                        error = Library.Message.GetMessage("GeneralMessages", "SampleLoginOpenError", ((JobHeader)entity).JobName,
                            ((JobHeader)entity).JobStatus.PhraseId);
                    }
                    Library.Utils.ShowAlert(errorTitle, error);

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Initialises the data.
        /// </summary>
        /// <returns></returns>
        protected override bool InitialiseTopLevelEntities(out IEntityCollection topLevelEntities)
        {
            if (Context.SelectedItems.Count == 0 || Context.SelectedItems[0].EntityType == JobHeader.EntityName)
            {
                // No context or this is a job session, just return the context;
                topLevelEntities = Context.SelectedItems;
                return true;
            }

            // Try placing the context sample within it's hierarchy
            // We cannot workout the parent of a composite sample so as soon as we reach a composite sample then this forms the root of this session.			
            m_ContextSample = (Sample)Context.SelectedItems[0];

            topLevelEntities = EntityManager.CreateEntityCollection(Sample.EntityName);

            foreach (Sample sample in Context.SelectedItems)
            {
                // Walk back up the hierarchy from the context sample to the master / root
                Sample rootSample = sample;
                while (rootSample.SplitParent != null)
                {
                    rootSample = (Sample)rootSample.SplitParent;
                }

                if (!topLevelEntities.Contains(rootSample))
                {
                    // This sample has not yet been added
                    topLevelEntities.Add(rootSample);
                }
            }

            return true;
        }

        /// <summary>
        /// Prompts for existing data t modify / display.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        protected virtual bool PromptForData(out IEntity entity)
        {
            // Prompt the user for a sample or Job when sample admin is initiated from the top menu

            string message = Library.Message.GetMessage("GeneralMessages", "SampleAdminPromptForData");
            string entityTypeDisplay = Context.EntityType == Sample.EntityName ? TextUtils.GetDisplayText(Context.EntityType) : "Job";
            message = string.Format(message, entityTypeDisplay);

            IQuery query = EntityManager.CreateQuery(Context.EntityType);

            if (Context.EntityType == JobHeaderBase.EntityName && Context.LaunchMode == GenericLabtableTask.ModifyOption)
            {
                query.AddEquals(JobHeaderPropertyNames.JobStatus, PhraseJobStat.PhraseIdV);
                query.AddOr();
                query.AddEquals(JobHeaderPropertyNames.JobStatus, PhraseJobStat.PhraseIdC);
            }

            if (Context.EntityType == SampleBase.EntityName && Context.LaunchMode == GenericLabtableTask.ModifyOption)
            {
                query.AddEquals(SamplePropertyNames.Status, PhraseSampStat.PhraseIdV);
                query.AddOr();
                query.AddEquals(SamplePropertyNames.Status, PhraseSampStat.PhraseIdC);
            }

            FormResult result = Library.Utils.PromptForEntity(message, Context.MenuItem.Description, query, out entity, TriState.No, Context.MenuProcedureNumber);

            return result == FormResult.OK;
        }

        #endregion

        #region General Overrides

        /// <summary>
        /// Gets the name of the top level table.
        /// </summary>
        /// <returns></returns>
        protected override string GetTopLevelTableName()
        {
            return Context.EntityType;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is job workflow.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is job workflow; otherwise, <c>false</c>.
        /// </value>
        protected override bool IsJobWorkflow
        {
            get { return Context.EntityType == JobHeader.EntityName; }
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
        }

        #endregion
    }
}
