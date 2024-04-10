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
    [SampleManagerTask(nameof(DeibelCopySamples))]
    public class DeibelCopySamples : ExtendedSampleLoginBase
    {
        protected override string TopTableName => Sample.EntityName;
        protected override bool JobWorkflow => false;
        protected override string Title => "Sample Copier";

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

            foreach (Sample sample in Context.SelectedItems)
            {
                var wf = InitialWorkflow == null ? Utils.GetLinkedLoginWorkflow(sample) : InitialWorkflow;
                _initialWorkflow = wf;

                var e = Utils.GetNewEntity(wf, TopTableName);
                Utils.CopyEntity(sample, e);
                col.Add(e);
            }
            return col;
        }
    }
}
