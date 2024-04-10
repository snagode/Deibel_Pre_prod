using System;
using System.Collections.Generic;
using System.Linq;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(DeibelCopySamplesExistingJob))]
    public class DeibelCopySamplesExistingJob : ExtendedJobLoginBase
    {
        protected override string TopTableName => JobHeader.EntityName;
        protected override bool JobWorkflow => true;
        protected override string Title => "Copy samples to existing Job";

        Workflow _initialWorkflow;
        protected override Workflow InitialWorkflow
        {
            get
            {
                return _initialWorkflow;
            }
        }

        protected override IEntityCollection TopEntities()
        {
            var col = EntityManager.CreateEntityCollection(TopTableName);

            // Prompt for existing job
            var q = EntityManager.CreateQuery(JobHeader.EntityName);
            var status = new List<object>() { "V", "C" };
            q.AddIn(JobHeaderPropertyNames.JobStatus, status);
            q.AddOrder(JobHeaderPropertyNames.DateCreated, false);
            IEntity entity;
            Library.Utils.PromptForEntity("Select a job", "Job", q, out entity);
            if (entity == null)
            {
                return null;
            }
            else
            {
                var j = entity as JobHeader;
                _initialWorkflow = Utils.GetLinkedLoginWorkflow(entity);
                
                // check if sample WF is specified in menu item
                Workflow wf = null;
                if (Context.TaskParameters.Count() > 1)
                {
                    wf = Utils.GetLoginWorkflow(Sample.EntityName, false, Context.TaskParameters[1]);
                }

                // Add selected samples to it
                foreach (Sample sample in Context.SelectedItems.Cast<Sample>().ToList())
                {
                    if(wf == null)
                        wf = Utils.GetLinkedLoginWorkflow(sample);

                    var e = Utils.GetNewEntity(wf, Sample.EntityName);
                    Utils.CopyEntity(sample, e);

                    j.Samples.Add(e);
                    Utils.PropagateJob(j, sample);
                }
                col.Add(j);
                return col;
            }
        }
    }
}
