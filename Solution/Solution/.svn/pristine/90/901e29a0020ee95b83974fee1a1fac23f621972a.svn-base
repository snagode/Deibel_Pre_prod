using System;
using System.Linq;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(DeibelCopyJobs))]
    public class DeibelCopyJobs : ExtendedJobLoginBase
    {
        protected override string TopTableName => JobHeader.EntityName;
        protected override bool JobWorkflow => true;
        protected override string Title => "Copy Job";

        protected override void MainFormCreated()
        {
            var jobs = Context.SelectedItems.Cast<JobHeader>().ToList();
            var jobct = jobs.Count.ToString();
            int samplect = 0;
            foreach(var j in jobs)
            {
                samplect += j.Samples.Count;
            }
            
            MainForm.SetBusy($"Copying {jobct} jobs, {samplect} samples...");
            
            base.MainFormCreated();
        }

        protected override void MainFormLoaded()
        {
            base.MainFormLoaded();

            MainForm.ClearBusy();
        }

        Workflow _initialWorkflow;
        protected override Workflow InitialWorkflow
        {
            get
            {
                if (_initialWorkflow == null)
                {
                    string wfName = "";
                    if (Context.TaskParameters.Count() > 1)
                        wfName = Context.TaskParameters[1];

                    _initialWorkflow = Utils.GetLoginWorkflow(TopTableName, false, wfName);
                }
                return _initialWorkflow;
            }
        }

        protected override IEntityCollection TopEntities()
        {
            var col = EntityManager.CreateEntityCollection(TopTableName);

            // Make a new job
            foreach (JobHeader job in Context.SelectedItems.Cast<JobHeader>().ToList())
            {
                var wf = InitialWorkflow == null ? Utils.GetLinkedLoginWorkflow(job) : _initialWorkflow;
                _initialWorkflow = wf;
                var j = Utils.GetNewEntity(wf, TopTableName);

                Utils.CopyEntity(job, j);
                col.Add(j);
            }
            return col;
        }
    }
}
