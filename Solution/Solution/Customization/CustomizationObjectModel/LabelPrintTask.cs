using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;

namespace Customization.Tasks
{
    /// <summary>
    /// Print test labels for jobs and samples
    /// </summary>
    [SampleManagerTask(nameof(LabelPrintTask))]
    public class LabelPrintTask : SampleManagerTask
    {
        readonly string _sampleLabel = "JOB_SAMPLE";
        readonly string _testLabel = "TEST_LIST";

        readonly string _printerName = "printer";

        IEntityCollection _jobs;
        IEntityCollection _samples;

        protected override void SetupTask()
        {
            // Prompt for job or sample, depending on parameter
            if(Context.SelectedItems.Count == 0)
            {
                if (Context.TaskParameters.Count() == 0)
                    return;

                if (Context.TaskParameters[0] == "SAMPLE")
                {
                    _samples = EntityManager.CreateEntityCollection(Sample.EntityName);
                    IEntity entity;
                    Library.Utils.PromptForEntity("Choose Sample", "", GetQuery(true), out entity);
                    if (entity == null)
                        return;
                    else
                        _samples.Add(entity);
                }
                else if (Context.TaskParameters[0] == "JOB")
                {
                    _jobs = EntityManager.CreateEntityCollection(JobHeader.EntityName);
                    IEntity entity;
                    Library.Utils.PromptForEntity("Choose Job", "", GetQuery(false), out entity);
                    if (entity == null)
                        return;
                    else
                        _jobs.Add(entity);
                }
                else
                    return;
            }
            else
            {
                if (Context.SelectedItems.EntityType == Sample.EntityName)
                {
                    _samples = Context.SelectedItems;
                }
                else if (Context.SelectedItems.EntityType == JobHeader.EntityName)
                {
                    _jobs = Context.SelectedItems;
                }
                else
                    return;
            }

            var samplbl = EntityManager.Select(LabelTemplate.EntityName, new Identity(_sampleLabel)) as LabelTemplate;
            var testlbl = EntityManager.Select(LabelTemplate.EntityName, new Identity(_testLabel)) as LabelTemplate;

            PrintLabels(samplbl, testlbl);
        }

        void PrintLabels(LabelTemplate sampleLabel, LabelTemplate testLabel)
        {
            if(_jobs != null)
            {
                foreach(JobHeader job in _jobs)
                {
                    foreach(Sample sample in job.Samples)
                    {
                        Library.Utils.PrintLabelBackground(sampleLabel, sample, _printerName, 1);
                        Library.Utils.PrintLabelBackground(testLabel, sample.Tests, _printerName, 1);

                        //Library.Utils.PreviewLabel(sampleLabel, _sampleLabel, sample, 1);
                        //Library.Utils.PreviewLabel(testLabel, _testLabel, sample.Tests, 1);
                    }
                }
            }
            else
            {
                foreach(Sample sample in _samples)
                {
                    Library.Utils.PrintLabelBackground(sampleLabel, sample, _printerName, 1);
                    Library.Utils.PrintLabelBackground(testLabel, sample.Tests, _printerName, 1);

                    //Library.Utils.PreviewLabel(sampleLabel, _sampleLabel, sample, 1);
                    //Library.Utils.PreviewLabel(testLabel, _testLabel, sample.Tests, 1);
                }
            }
        }

        IQuery GetQuery(bool sample)
        {
            IQuery q;
            var groupId = ((Personnel)(Library.Environment.CurrentUser)).DefaultGroup.GroupId;

            if (sample)
            {
                q = EntityManager.CreateQuery(Sample.EntityName);
                q.AddEquals(SamplePropertyNames.GroupId, groupId);
                q.AddOrder(SamplePropertyNames.IdNumeric, false);
            }
            else
            {
                q = EntityManager.CreateQuery(JobHeader.EntityName);
                q.AddEquals(JobHeaderPropertyNames.GroupId, groupId);
                q.AddOrder(JobHeaderPropertyNames.DateCreated, false);
            }

            return q;
        }
    }
}
