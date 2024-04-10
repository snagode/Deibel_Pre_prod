using System;
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
    [SampleManagerTask(nameof(ChangeBillingStatusTask))]
    public class ChangeBillingStatusTask : SampleManagerTask
    {
        protected override void SetupTask()
        {

            base.SetupTask();
            if (Context.SelectedItems.ActiveCount == 0)
            { Exit(false); return; }

            var jobs = Context.SelectedItems;
            var samples = Context.SelectedItems;
            var tests = Context.SelectedItems;

            PhraseBase phraseP = (PhraseBase)EntityManager.SelectPhrase(PhraseBillStat.Identity, PhraseBillStat.PhraseIdP);
            PhraseBase phraseB = (PhraseBase)EntityManager.SelectPhrase(PhraseBillStat.Identity, PhraseBillStat.PhraseIdB);
            PhraseBase phraseN = (PhraseBase)EntityManager.SelectPhrase(PhraseBillStat.Identity, PhraseBillStat.PhraseIdN);

            PhraseBase phrase= phraseN;

            string menuName = ((Thermo.SampleManager.Library.EntityDefinition.MasterMenuBase)this.Context.MenuItem).Description;

            if (menuName == "Change Billing Status to Billed")
                phrase = phraseB;
            else if (menuName == "Change Billing Status to Not Billed")
                phrase = phraseN;
            else
                return;

            if (jobs.ActiveItems.FirstOrDefault().EntityType == "JOB_HEADER")

                foreach (JobHeader job in jobs)
                {
                    if (job.BillingStatus.PhraseId == phraseB.PhraseId)
                    {
                        job.BillingStatus = phrase;

                        foreach (Sample j in job.Samples)
                        {
                            j.BillingStatus = phrase;
                            foreach (Test t in j.Tests)
                            {
                                t.BillingStatus = phrase;
                                EntityManager.Transaction.Add(t);
                            }
                            EntityManager.Transaction.Add(j);
                        }
                        EntityManager.Transaction.Add(job);
                    }
                }
            //else
            //{
            //    job.BillingStatus = phraseB;
            //    foreach (Sample j in job.Samples)
            //    {
            //        j.BillingStatus = phraseB;
            //        foreach (Test t in j.Tests)
            //        {
            //            t.BillingStatus = phraseB;
            //        }
            //    }

            //}

            else if (samples.ActiveItems.FirstOrDefault().EntityType == "SAMPLE")
            {
                foreach (Sample sample in samples)
                {
                    if (sample.BillingStatus.PhraseId == phraseB.PhraseId)
                    {
                        sample.BillingStatus = phrase;
                        foreach (Test j in sample.Tests)
                        {
                            j.BillingStatus = phrase;
                            EntityManager.Transaction.Add(j);
                        }
                        EntityManager.Transaction.Add(sample);
                    }
                    //else
                    //{
                    //    sample.BillingStatus = phraseB;
                    //    foreach (Test t in sample.Tests)
                    //    {
                    //        t.BillingStatus = phraseB;
                    //    }
                    //}

                }
            }
            else if (samples.ActiveItems.FirstOrDefault().EntityType == "TEST")
            {
                foreach (Test test in tests)
                {
                    if (test.BillingStatus.PhraseId == phraseB.PhraseId)
                    {
                        test.BillingStatus = phrase;
                        EntityManager.Transaction.Add(test);
                    }
                }
                //else
                //    test.BillingStatus = phraseB;

            }
            EntityManager.Commit();
        }
    }
}
