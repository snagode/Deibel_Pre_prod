using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Common.CommandLine;
using System.IO;


namespace Customization.Tasks
{
    [SampleManagerTask(nameof(BIllingStatusUpdateBGTask))]
    public class BIllingStatusUpdateBGTask : SampleManagerTask, IBackgroundTask
    {
        protected override void SetupTask()
        {
            //if (Library.Environment.IsBackground())
            //    return;
            //if (Library.Environment.IsInteractive())
            //{
            //    Launch();
            //}
            Launch();
        }

        public void Launch()
        {

            var q = this.EntityManager.CreateQuery(CustomerToBilledVwBase.EntityName);

            q.AddEquals(CustomerToBilledVwPropertyNames.BillingStatus, "R");

            PhraseBase phraseB = (PhraseBase)EntityManager.SelectPhrase(PhraseBillStat.Identity, PhraseBillStat.PhraseIdB);

            var res = EntityManager.Select(q);

            foreach (CustomerToBilledVwBase item in res.ActiveItems.Cast<CustomerToBilledVwBase>())
            {

                var j = this.EntityManager.CreateQuery(JobHeader.EntityName);
                j.AddEquals(JobHeaderPropertyNames.JobName, item.JobName);

                var job = (JobHeader)EntityManager.Select(j).ActiveItems.Cast<JobHeader>().FirstOrDefault();

                if ((job.Samples.ActiveItems.Cast<Sample>().SelectMany(n => n.Tests.ActiveItems.Cast<Test>())).All(m => m.BillingStatus.PhraseId == "B"))
                    job.BillingStatus = phraseB;

                EntityManager.Transaction.Add(job);


            }
            EntityManager.Commit();
        }
    }
}
