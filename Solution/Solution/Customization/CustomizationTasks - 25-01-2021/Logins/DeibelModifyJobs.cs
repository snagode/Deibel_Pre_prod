using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Thermo.Framework.Utilities;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(DeibelModifyJobs))]
    public class DeibelModifyJobs : ExtendedJobLoginBase
    {
        protected override string TopTableName => JobHeader.EntityName;
        protected override bool JobWorkflow => true;
        protected override string Title => "Modify Job";

        protected override Workflow InitialWorkflow
        {
            get
            {
                return null;
            }
        }

        protected override IEntityCollection TopEntities()
        {
            return Context.SelectedItems;
        }


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
            base.MainFormLoaded();

            if (HasExited) return;

            // Job is selected
            JobHeader selectedJob = (JobHeader)Context.SelectedItems[0];

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

            FormResult result = Library.Utils.PromptForEntity(message, Context.MenuItem.Description, query, out entity, TriState.No, Context.MenuProcedureNumber);

            return result == FormResult.OK;
        }

        #endregion

    }
}
