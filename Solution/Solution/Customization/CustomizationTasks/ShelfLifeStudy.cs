using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Tasks;
using Thermo.SampleManager.Internal.ObjectModel;
using System.Collections;
using Thermo.SampleManager.Common.Workflow;
using Thermo.SampleManager.Server.Workflow;
using Thermo.SampleManager.Library.ClientControls;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(ShelfLifeStudy), "WorkflowCallback")]

    public class ShelfLifeStudy : SampleManagerTask//SampleAdminTask
    {
        protected IEntity _jobHeader2;
        JobHeader job;
        Workflow _jobWorkflow;
        Workflow _sampleWorkflow;
        FormSampleAdmin _sampleAdmin;
        protected IEntity m_FocusedTreeEntity;
        List<IEntity> newEntities = new List<IEntity>();
        protected WorkflowBase _defaultJobWorkflow;
        protected Workflow _defaultSampleWorkflow;
        IWorkflowPropertyBag propertyBag;
        IEntityCollection m_TopLevelEntities;
        protected FormSampleAdmin m_Form;
        Boolean flag = false;
        protected override void SetupTask()
        {
            base.SetupTask();
            if (Context.SelectedItems.ActiveCount == 0)
            { Exit(false); return; }

            job = (JobHeader)Context.SelectedItems[0];
            if (job.Slstudy == true)
            {
                if (Library.Utils.FlashMessageYesNo("Is this the first product for the Shelf Life Study ?", ""))
                {
                    flag = true;
                    //**************************Working As DeibelSampleAdminTask*************************************************************
                    callFunction();

                }
                else
                {
                    flag = false;
                    callFunction();
                }


            }
            Exit(true);
        }

        private void callFunction()
        {
            try
            {

                SetDefaultWorkflows();
                var newEntities = new List<IEntity>();
                m_Form = (FormSampleAdmin)FormFactory.CreateForm<FormSampleAdmin>();
                RunWorkflowForEntity(job, _defaultSampleWorkflow, 1, newEntities);

            }
            catch (Exception)
            {

                throw;
            }
        }

        protected void SetDefaultWorkflows()//bool doOrdering = false
        {
            string jobGUID = "";
            string sampleGUID = "";

            if (Context.TaskParameters.Count() != 2 || false)
            {
                // Workflow name: Deibel Job
                jobGUID = "5C034090-7C3A-46F0-9F35-A41729EAC02E";

                // Workflow name: Deibel Sample
                sampleGUID = "D9EC80B8-783B-4728-84D9-8A4F82F7C6AC";
            }
            else
            {
                jobGUID = Context.TaskParameters[0];
                sampleGUID = Context.TaskParameters[1];
            }

            _defaultJobWorkflow = EntityManager.Select(WorkflowBase.EntityName, new Identity(jobGUID, 1)) as WorkflowBase;
            _defaultSampleWorkflow = EntityManager.Select(WorkflowBase.EntityName, new Identity(sampleGUID, 1)) as Workflow;
        }

        protected void RunWorkflowForEntity(IEntity entity, Workflow selectedWorkflow, int count, List<IEntity> allNewEntities)
        {
            var newEntities = RunWorkflowForEntity(entity, selectedWorkflow, count).ToList();
            UpdateTestAssignmentGrid(_defaultSampleWorkflow.TableName, newEntities);
        }
        IList<IEntity> RunWorkflowForEntity(IEntity entity, Workflow selectedWorkflow, int count)
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

            // ApplyInformation(newEntities);

            return newEntities;
        }
        protected bool PerformWorkflow(Workflow workflow, IWorkflowPropertyBag propertyBag)
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

                string message = Library.Message.GetMessage("GeneralMessages", "EmptyWorkflowOutput");
                // Library.Utils.FlashMessage(message, m_Title, MessageButtons.OK, MessageIcon.Error, MessageDefaultButton.Button1);

                return false;
            }

            // Exit if there are errors

            if (propertyBag.Errors.Count > 0)
            {
                // Library.Utils.FlashMessage(propertyBag.Errors[0].Message, m_Title, MessageButtons.OK, MessageIcon.Error, MessageDefaultButton.Button1);
                return false;
            }

            return true;
        }

        static IWorkflowPropertyBag GeneratePropertyBag(IEntity entity)
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
       
        protected void UpdateTestAssignmentGrid(string entityType, List<IEntity> newEntities)
        {
            AddTestAssignmentSamples(newEntities);
        }
        void AddTestAssignmentSamples(IList samples)
        {
            foreach (Sample sample in samples)
            {
                AddTestAssignmentSample(sample);
                // AssignAfterEdit(sample, job, "JobName");
                if (job.CustomerId != null)
                {
                    sample.CustomerId = job.CustomerId;
                }

                // EntityManager.Transaction.Add(sample);

            }
            EntityManager.Commit();
        }
        void AddTestAssignmentSample(Sample sample)
        {
            try
            {

                var analysis = "SHELF_LIFE_STUDY";
                var q = EntityManager.CreateQuery(TableNames.VersionedAnalysis);
                q.AddEquals(VersionedAnalysisPropertyNames.VersionedAnalysisName, analysis);
                var col = EntityManager.Select(q).ActiveItems[0];
                var vAnalysis = col as VersionedAnalysis;
               

                var analysis1 = "SHELF_LIFE_PROD";
                var q1 = EntityManager.CreateQuery(TableNames.VersionedAnalysis);
                q1.AddEquals(VersionedAnalysisPropertyNames.VersionedAnalysisName, analysis1);
                var col1 = EntityManager.Select(q1).ActiveItems[0];
                var vAnalysis2 = col1 as VersionedAnalysis;
               

                if (flag)
                {
                    var addTest1 = sample.AddTest(vAnalysis);
                    var addTest2 = sample.AddTest(vAnalysis2);

                }
                else
                {
                    var addTest2 = sample.AddTest(vAnalysis2);
                }

            }
            catch (Exception)
            {

                throw;
            }
            //samples.Add(sample);
            //EntityManager.Transaction.Add(sample);
            //  AddTest(sample, ana, 1);

        }

        


    }
}
