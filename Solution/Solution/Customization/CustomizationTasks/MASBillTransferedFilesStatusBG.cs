using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.CommandLine;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(MASBillTransferedFilesStatusBG))]
    public class MASBillTransferedFilesStatusBG : SampleManagerTask, IBackgroundTask
    {

        protected override void SetupTask() // this event executed whenever form task is called
        {
            Launch();

        }

        public void Launch()
        {
            var q = EntityManager.CreateQuery(MasBillingBase.EntityName);
            q.AddEquals(Thermo.SampleManager.Common.Data.MasBillingPropertyNames.ApprovalStatus, "A");
            q.AddEquals(Thermo.SampleManager.Common.Data.MasBillingPropertyNames.Status, "T");
            q.AddEquals(Thermo.SampleManager.Common.Data.MasBillingPropertyNames.StatusMessage, "Send complete");

            var data = EntityManager.Select(q);

            foreach (var item in data)
            {
                var record = (MasBillingBase)item;

                UpdateToBilledStatus(record);
            }
        }

        private void UpdateToBilledStatus(MasBillingBase q)
        {
            var qt = this.EntityManager.CreateQuery(MasBillingDetailsBase.EntityName);
            //qt.AddEquals(MasBillingDetailsPropertyNames.StatusMessage, "B");
            qt.AddEquals(MasBillingDetailsPropertyNames.MasId, q.RecordId);

            var data = EntityManager.Select(qt).ActiveItems.Cast<MasBillingDetailsBase>();
            PhraseBase phraseB = (PhraseBase)EntityManager.SelectPhrase(PhraseBillStat.Identity, PhraseBillStat.PhraseIdB);

            //update MasId
            q.Status = "C";
            EntityManager.Transaction.Add(q);

            //// update jobids
            //var jobs = data.Select(m => (object)m.JobName).Distinct().ToList();

            //var q1 = EntityManager.CreateQuery(JobHeader.EntityName);
            //q1.AddIn(JobHeaderPropertyNames.JobName, jobs);
            //var jobids = EntityManager.Select(q1).ActiveItems.Cast<JobHeader>();
            //foreach (var jobid in jobids)
            //{
            //    jobid.BillingStatus = phraseB;
            //    EntityManager.Transaction.Add(jobid);
            //}

            //// update samples
            //var samples = data.Select(m => (object)m.Sample).Distinct().ToList();
            //var q2 = EntityManager.CreateQuery(Sample.EntityName);
            //q2.AddIn(SamplePropertyNames.IdText, samples);
            //var sampleids = EntityManager.Select(q2).ActiveItems.Cast<Sample>();
            //foreach (var sample in sampleids)
            //{
            //    sample.BillingStatus = phraseB;
            //    EntityManager.Transaction.Add(sample);
            //}

            ////update tests
            //var tests = data.Select(m => (object)m.TestNumber).Distinct().ToList();
            //var q3 = EntityManager.CreateQuery(Test.EntityName);
            //q3.AddIn(TestPropertyNames.TestNumber, tests);
            //var testids = EntityManager.Select(q3).ActiveItems.Cast<Test>();
            //foreach (var test in testids)
            //{
            //    test.BillingStatus = phraseB;
            //    test.MasNumber = q.RecordId.ToString();
            //    EntityManager.Transaction.Add(test);
            //}


            // update jobids
            var jobs = data.Select(m => (object)m.JobName).Distinct().ToList();

            var q1 = EntityManager.CreateQuery(JobHeader.EntityName);
            q1.AddIn(JobHeaderPropertyNames.JobName, jobs);
            var jobids = EntityManager.Select(q1).ActiveItems.Cast<JobHeader>();
            foreach (var jobid in jobids)
            {
                jobid.BillingStatus = phraseB;
                EntityManager.Transaction.Add(jobid);
            }

            // update samples
            var samples = data.Select(m => (object)m.Sample).Distinct().ToList();
            List<Sample> samplelist = new List<Sample>();
            var datasample = BuildChunksWithLinqAndYield<object>(samples, 1000);
            foreach (var size in datasample)
            {
                var i = size.ToList();
                var q2 = EntityManager.CreateQuery(Sample.EntityName);
                q2.AddIn(SamplePropertyNames.IdText, i);
                var sampeids = EntityManager.Select(q2).ActiveItems.Cast<Sample>();
                samplelist.AddRange(sampeids);
            }
            foreach (var sample in samplelist)
            {
                sample.BillingStatus = phraseB;
                EntityManager.Transaction.Add(sample);
            }



            //update tests
            var tests = data.Select(m => (object)m.TestNumber).Distinct().ToList();
            List<Test> testlist = new List<Test>();
            var datatest = BuildChunksWithLinqAndYield<object>(tests, 1000);
            foreach (var size in datatest)
            {
                var i = size.ToList();
                var q3 = EntityManager.CreateQuery(Test.EntityName);
                q3.AddIn(TestPropertyNames.TestNumber, i);
                var testsids = EntityManager.Select(q3).ActiveItems.Cast<Test>();
                testlist.AddRange(testsids);
            }
            foreach (var test in testlist)
            {
                test.BillingStatus = phraseB;
                EntityManager.Transaction.Add(test);
            }

            EntityManager.Commit();
        }

        private IEnumerable<IEnumerable<T>> BuildChunksWithLinqAndYield<T>(List<T> fullList, int batchSize)
        {
            int total = 0;
            while (total < fullList.Count)
            {
                yield return fullList.Skip(total).Take(batchSize);
                total += batchSize;
            }
        }


    }
}
