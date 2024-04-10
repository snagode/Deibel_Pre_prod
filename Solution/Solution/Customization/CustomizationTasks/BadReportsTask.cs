using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(BadReportsTask))]
    public class BadReportsTask : SampleManagerTask
    {
        FormBadReports _form;

        protected override void SetupTask()
        {
            _form = FormFactory.CreateForm<FormBadReports>();
            _form.Loaded += _form_Loaded;
            _form.Show();
        }

        private void _form_Loaded(object sender, EventArgs e)
        {
            var now = (DateTime)Library.Environment.ClientNow;

            _form.dateEnd.Date = now;
            var start = now.Subtract(new TimeSpan(7, 0, 0, 0));
            _form.dateStart.Date = start;


            _form.btnGetJobs.Click += BtnGetJobs_Click;
        }

        private void BtnGetJobs_Click(object sender, EventArgs e)
        {
            _form.SetBusy("", "Executing query...");
            
            // Get bad report results
            var q = EntityManager.CreateQuery(SampTestResult.EntityName);
            q.AddEquals(SampTestResultPropertyNames.ReportId, "");
            q.AddEquals(SampTestResultPropertyNames.ResultStatus, "A");
            q.AddLike(SampTestResultPropertyNames.RepControl, "%R%");
            q.AddGreaterThan(SampTestResultPropertyNames.LoginDate, _form.dateStart.Date);
            q.AddLessThan(SampTestResultPropertyNames.LoginDate, _form.dateEnd.Date);
            q.AddOrder(SampTestResultPropertyNames.IdNumeric, false);
            var strs = EntityManager.Select(SampTestResult.EntityName, q);

            // Group the jobs
            var jobs = strs.Cast<SampTestResult>().GroupBy(r => r.JobName).Select(j => j.First()).ToList();

            // Add each job to collection
            var badJobs = EntityManager.CreateEntityCollection(JobHeader.EntityName);
            foreach (var job in jobs)
            {
                if(job.JobName.JobStatus.PhraseId == PhraseJobStat.PhraseIdA)
                    badJobs.Add(job.JobName);
            }

            _form.ClearBusy();

            _form.ebJobs.Republish(badJobs);
        }
        
    }
}
