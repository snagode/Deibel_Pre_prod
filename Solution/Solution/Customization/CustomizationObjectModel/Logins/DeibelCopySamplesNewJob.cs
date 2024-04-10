using System;
using System.Linq;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(DeibelCopySamplesNewJob))]
    public class DeibelCopySamplesNewJob : ExtendedJobLoginBase
    {
        protected override string TopTableName => JobHeader.EntityName;
        protected override bool JobWorkflow => true;
        protected override string Title => "Add Samples New Job";

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
                    _initialWorkflow = Utils.GetLoginWorkflow(TopTableName, true, wfName);
                    if (_initialWorkflow == null)
                        Exit();
                }
                return _initialWorkflow;
            }
        }

        protected override IEntityCollection TopEntities()
        {
            var col = EntityManager.CreateEntityCollection(TopTableName);

            // Make a new job
            var j = Utils.GetNewEntity(InitialWorkflow, TopTableName) as JobHeader;

            // check if sample WF is specified in menu item
            Workflow wf = null;
            if (Context.TaskParameters.Count() > 2)
            {
                wf = Utils.GetLoginWorkflow(Sample.EntityName, false, Context.TaskParameters[2]);
            }

            // Add selected samples to it
            foreach (Sample sample in Context.SelectedItems)
            {
                wf = wf == null ? Utils.GetLinkedLoginWorkflow(sample) : wf;

                var e = Utils.GetNewEntity(wf, Sample.EntityName);
                Utils.CopyEntity(sample, e);

                j.Samples.Add(e);                
            }

            col.Add(j);
            return col;
        }
    }
}
