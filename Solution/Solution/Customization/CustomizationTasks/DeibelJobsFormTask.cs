using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Common.Data;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(DeibelJobsFormTask))]
    public class DeibelJobsFormTask : SampleManagerTask
    {
        FormDeibelJobs _form;
        
        protected override void SetupTask()
        {
            if (Context.SelectedItems.Count == 0 || Context.SelectedItems.EntityType != JobHeader.EntityName)
                return;

            _form = FormFactory.CreateForm<FormDeibelJobs>();
            _form.Loaded += _form_Loaded;
            _form.Show();
        }

        private void _form_Loaded(object sender, EventArgs e)
        {
            _form.gridJobs.SelectionChanged += GridJobs_SelectionChanged;

            _form.ebJobs.Republish(Context.SelectedItems);            
        }

        private void GridJobs_SelectionChanged(object sender, ExplorerGridSelectionChangedEventArgs e)
        {
            if (e == null || e.Selection.Count == 0)
                return;

            // Fill samples grid
            var list = e.Selection.Cast<JobHeader>().ToList();
            var jobs = list.Select(j => j.JobName).Cast<object>().ToList();
            
            var q = EntityManager.CreateQuery(Sample.EntityName);
            q.AddIn(SamplePropertyNames.JobName, jobs);

            _form.ebSamples.Republish(q);
        }
    }
}
