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
    /// This class is used for adding new samples to an existing job.  The data 
    /// are copied from selected samples to the new samples
    /// </summary>
    [SampleManagerTask(nameof(DeibelJobAddSamplesTask))]
    public class DeibelJobAddSamplesTask : DeibelSampleAdminBaseTask
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

            IEntity e = null;
            JobHeader selectedJob = null;

            // Only available jobs can be saved after modification
            var q = EntityManager.CreateQuery(JobHeader.EntityName);
            var status = new List<object> { "V", "C" };
            q.AddIn(JobHeaderPropertyNames.JobStatus, status);

            // Include jobs from all user groups, or all if it's system
            var user = Library.Environment.CurrentUser as PersonnelBase;
            if (!user.IsSystemUser())
            {
                var groups = user.UserGroupIds().Cast<object>().ToList();
                q.AddIn(JobHeaderPropertyNames.GroupId, groups);
            }

            q.AddOrder(JobHeaderPropertyNames.DateCreated, false);
            Library.Utils.PromptForEntity("Select job header", "Choose header", q, out e);
            selectedJob = e as JobHeader;
            if (selectedJob == null)
                Exit();

            _jobHeader = selectedJob;
            _jobType = DeibelJobType.ExistingJob;
            _addSampleContext = AddSampleMode.LoadingSelected;

            var items = Context.SelectedItems.ActiveItems;
            if (Context.SelectedItems[0].EntityType == Sample.EntityName)
                _selectedSamples = items.Cast<Sample>().ToList();
            else if (Context.SelectedItems[0].EntityType == FtpSampleBase.EntityName)
                _selectedSamples = base.ConvertFtpSamples(items.Cast<FtpSampleBase>().OrderBy(s => s.Identity).ToList());

 // Start
            foreach(var selectedsample in _selectedSamples)
            {
              var Customer  = selectedsample.CustomerId;
                if(Customer!= null)
                {
                    var fieldMap = Customer.CustomerXmlInbounds.Cast<CustomerXmlInboundBase>().ToList();
                    foreach (var field in fieldMap.Where(m => m.TableName == "SAMPLE" && m.TableFieldName != "CUSTOMER_ID" && !string.IsNullOrWhiteSpace(m.TableFieldName)))
                    {
                        try
                        {
                            // Get value from sample entity using table field name
                            var value = ((IEntity)selectedsample).Get(field.TableFieldName);


                            // Set the value on sample entity using table field name
                            if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                                ((IEntity)selectedsample).Set(field.TableFieldName, value);
                        }
                        catch { }
                    }
                }
                //New Changes-Avinash Start
                var identity = selectedsample.FtpTransaction;
                
                    var query = EntityManager.CreateQuery(TableNames.FtpSample);
                    query.AddEquals(FtpSamplePropertyNames.Identity, identity);
                    var results = (FtpSampleBase)EntityManager.Select(query).ActiveItems[0];

                   // var ImSampleRefId = results.AuxInfoB;
                   // var  ImSampleRefIdAlt = results.AuxInfoC;
                
                //New Changes-Avinash End
                var Jobproductname = selectedJob.ProductId;
                var JobCustomerId = selectedJob.CustomerId;
                var JobGroupId = selectedJob.GroupId;
                selectedsample.Product = Jobproductname;
               // selectedsample.CustomerId = JobCustomerId;
                selectedsample.GroupId = JobGroupId;
                //New Changes Avinash Start
               // selectedsample.ImSampleRefId = ImSampleRefId;
               // selectedsample.ImSampleRefIdAlt = ImSampleRefIdAlt;
                //New Changes Avinash End
                if (selectedJob.Samples != null)
                {
                 var sample = (SampleBase)selectedJob.Samples.GetFirst();
                if (sample != null)
                {
                var productversion = sample.ProductVersion;
                selectedsample.ProductVersion = productversion;
                }
                else
                {
                    selectedsample.ProductVersion = selectedJob.ProductVersion;
                }
               if (sample != null)
                {
                var tests = sample.Tests;
                foreach (Test test in tests)
                {
                    Addtestsd(test,selectedsample, Customer);
                    
                }
               // EntityManager.Transaction.Add(selectedsample);
               }
             }
           }
// End

            base.SetupTask();
        }

        public void Addtestsd(Test test, Sample selectedsample1,CustomerBase customer)
        {

            if (customer != null)
            {

                var fieldMap = customer.CustomerXmlInbounds.Cast<CustomerXmlInboundBase>().ToList();

                // Copy values from mapped fields
                foreach (var field in fieldMap.Where(m => m.TableName == "TEST" && m.TableFieldName != "ANALYSIS" && !string.IsNullOrWhiteSpace(m.TableFieldName)))
                {
                    try
                    {
                        var value = ((IEntity)selectedsample1).Get(field.TableFieldName);
                   
                       


                        if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                            ((IEntity)selectedsample1).Set(field.TableFieldName, value);
                      
                    }
                    catch { }
                }
            }
            //var newTests = selectedsample1.Tests;
            //foreach (Test test1 in newTests)
            //{
            //    if (test.Analysis.Equals(test1.Analysis))
            //    {
            //        test1.ImTestRefId = test.ImTestRefId;
            //        test1.ImTestRefIdAlt = test.ImTestRefIdAlt;
            //       // EntityManager.Transaction.Add(test1);
            //    }
            //}
         }

        /// <summary>
        /// Called when the <see cref="DefaultFormTask.MainForm"/> has been loaded.
        /// </summary>
        protected override void MainFormLoaded()
        {
            base.MainFormLoaded();

            AddSelectedSamplesToJob();

            if (HasExited) return;

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

            return true;  // CheckValidStatus(Context.SelectedItems);
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

        /// <summary>
        /// Prompts for existing data to modify / display.
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
