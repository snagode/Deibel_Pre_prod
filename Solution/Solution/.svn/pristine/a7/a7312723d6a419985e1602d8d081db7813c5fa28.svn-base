using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Server.Workflow;
using Thermo.SampleManager.Server.Workflow.Nodes;


using Thermo.Framework.Core;
using Thermo.SampleManager.Core.Definition;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Library.ClientControls;
using ToolBarButton = Thermo.SampleManager.Library.ClientControls.ToolBarButton;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(AuthorizeCustomerConfirmation), "WorkflowCallback")]
    public class AuthorizeCustomerConfirmation : SampleManagerTask
    {
        protected override void SetupTask()
        {
            if (Context.SelectedItems.ActiveCount == 0)
            { Exit(false); return; }

            var tests = new List<Test>();

                var job = Context.SelectedItems[0] as JobHeader;
                var sample = Context.SelectedItems[0] as Sample;
                var test = Context.SelectedItems[0] as Test;

           

            if (job != null)
            {
                if (job.JobStatus.PhraseId != PhraseSampStat.PhraseIdA)
                {
                    job.JobStatus.PhraseId = PhraseSampStat.PhraseIdA;
                }
                if (job.Samples.Cast<Sample>().SelectMany(s => s.Tests.Cast<Test>()).Any(s => s.Status.PhraseId != PhraseSampStat.PhraseIdA))
                {

                    job.Samples.Cast<Sample>().Where(s => s.Status.PhraseId == PhraseSampStat.PhraseIdA).ToList().ForEach(c => c.Status.PhraseId = PhraseSampStat.PhraseIdA);

                }
                else if (job.Samples.Cast<Sample>().Any(s => s.Status.PhraseId != PhraseSampStat.PhraseIdA))
                {
                    job.Samples.Cast<Sample>().Where(s => s.Status.PhraseId == PhraseSampStat.PhraseIdA).ToList().ForEach(c => c.Status.PhraseId = PhraseSampStat.PhraseIdA);

                }
                EntityManager.Transaction.Add(job);
            }
            else if (sample != null)
            {
                //tests = job.Samples.Cast<Sample>().SelectMany(s => s.Tests.Cast<Test>()).ToList();


                if (sample.Tests.Cast<Test>().Any(s => s.Status.PhraseId != PhraseSampStat.PhraseIdA))
                {
                    job.Samples.Cast<Sample>().Where(s => s.Status.PhraseId == PhraseSampStat.PhraseIdA).ToList().ForEach(c => c.Status.PhraseId = PhraseSampStat.PhraseIdA);
                }
                else if (sample.Status.PhraseId != PhraseSampStat.PhraseIdA)
                {
                    //sample.SetStatus(PhraseSampStat.PhraseIdA);
                }
                
                //sample.TriggerAuthorized();
                EntityManager.Transaction.Add(sample);
                var workflow = sample.WorkflowNode.Workflow as Workflow;
                var actionType = workflow.WorkflowNodes.Cast<WorkflowNode>()
                    .Where(s => s.ActionTypeId.Contains("Job Status Trigger A")).Select(x => x.ActionType).FirstOrDefault();
                // var firstBag = new WorkflowPropertyBag();
                //// firstBag.Add("ParentSamples", castedCompositeParents);
                //Library.Workflow.PerformAction(sample, actionType);
                //CreateSampleNode
                //ValidStatusForModify();
                //WorkflowNodePropertyNames.ActionTypeId


            }
            else if (test.Status.PhraseId != PhraseSampStat.PhraseIdA)
            {
                test.Status.PhraseId = PhraseSampStat.PhraseIdA;
                EntityManager.Transaction.Add(test);
            }
            else
            {
                Exit(true);
                return;
            }

            EntityManager.Commit();

            Exit(true);
        }


    }
}
