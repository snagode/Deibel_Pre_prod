﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Common.Data;


namespace Customization.Tasks
{
    [SampleManagerTask(nameof(MASBillingTask))]
    public class MASBillingTask : SampleManagerTask
    {
        string MAS_PATH;
        string SERVER_PATH;
        // private FormMASBilling _form;

        protected override void SetupTask()
        {
            SERVER_PATH = Library.Environment.GetFolderList("smp$billing").ToString();
            MAS_PATH = Library.Environment.GetFolderList("smp$masdestination").ToString();
            // _form = (FormMASBilling)FormFactory.CreateForm(typeof(FormMASBilling));

            string menuName = ((Thermo.SampleManager.Library.EntityDefinition.MasterMenuBase)this.Context.MenuItem).ShortText;
            var q = (MasBillingBase)((Thermo.SampleManager.Server.EntityCollection)this.Context.SelectedItems).Items[0];

            var maspath = @MAS_PATH + "\\" + q.LabCode + "\\" + q.FileName;
            var serverpath = @SERVER_PATH + "\\" + q.FileName;

            switch (menuName)
            {

                case "LSR Transfer":
                    MasBilling_Send(maspath, serverpath, q);
                    break;

                case "LSR Resend":
                    MasBilling_ReSend(maspath, serverpath, q);
                    break;

                case "Cancel LSR":
                    MasBilling_Cancel(q);
                    break;

                case "Reset LSR Billing Status":
                    MasBilling_Transfer_Reset(q);
                    break;

                case "Cancel":
                    MasBilling_Transfer_Cancel(q);
                    break;

                case "Approve LSR":
                    MasBilling_Approve_LSR(q);
                    break;

                case "Reject LSR":
                    MasBilling_Reject_LSR(q);
                    break;

                case "View LSR":
                    MasBilling_View_LSR(q);
                    break;

                case "Dowload LSR":
                    Download_LSR(q);
                    break;

                case "Reset":
                    Reset(q);
                    break;
            }


        }

        private void Reset(MasBillingBase k)
        {

            var apprphrase = (PhraseBase)EntityManager.SelectPhrase(PhraseApprStat.Identity, PhraseApprStat.PhraseIdR);
            PhraseBase phraseP = (PhraseBase)EntityManager.SelectPhrase(PhraseBillStat.Identity, PhraseBillStat.PhraseIdP);

            //var currentUser = ((Personnel)Library.Environment.CurrentUser);
            //var labid = currentUser.DefaultGroup.GroupId;

            //var q = EntityManager.CreateQuery(MasBillingBase.EntityName);
            //q.AddEquals(MasBillingPropertyNames.CustomerId, k.CustomerId);
            //q.AddEquals(MasBillingPropertyNames.GroupId, labid);
            ////q.AddLessThanOrEquals(MasBillingPropertyNames.GroupId, _enddate);
            ////q.AddGreaterThanOrEquals(MasBillingPropertyNames.DateCreated, k.DateCreated);
            ////q.PushBracket();
            //q.AddEquals(MasBillingPropertyNames.Status, "C");
            ////q.AddOr();
            //q.AddEquals(MasBillingPropertyNames.StatusMessage, "Send complete");
            ////q.PopBracket();
            //var res2 = EntityManager.Select(q).ActiveItems.Cast<MasBillingBase>();

            //if ((res2.Count() > 0))
            //{
            //    Library.Utils.FlashMessage("Cannot Reset - Subsequent LSR already generated", "");
            //    return;
            //}

            //MasBillingDetailsBase q = (MasBillingDetailsBase)EntityManager.CreateEntity(MasBillingDetailsBase.EntityName, new Identity(id1));
            var query = EntityManager.CreateQuery(MasBillingDetailsBase.EntityName);
            query.AddEquals(MasBillingDetailsPropertyNames.MasId, k.RecordId);
            var sdata = EntityManager.Select(query).ActiveItems.Cast<MasBillingDetailsBase>();

            try
            {

                //k.ApprovalStatus = apprphrase;
                k.Status = "B";
                k.StatusMessage = "Queued for Transfer";
                k.DateProcessed = DateTime.Now;
                //foreach (var l in sdata)
                //{
                //    l.StatusMessage = "C";
                //    EntityManager.Transaction.Add(l);
                //}
                EntityManager.Transaction.Add(k);
            }
            catch (Exception e)
            {
                // update mas details
                foreach (var record in sdata)
                {
                    record.StatusMessage = "F";
                    EntityManager.Transaction.Add(record);
                }
            }
            EntityManager.Commit();
        }

        private void Download_LSR(MasBillingBase q)
        {

            Library.File.TransferToClient(q.XlFileName, Path.GetFileName(q.XlFileName), true);

        }

        private void MasBilling_View_LSR(MasBillingBase q)
        {
            throw new NotImplementedException();
        }

        private void MasBilling_Transfer_Reset(MasBillingBase k)
        {
            var apprphrase = (PhraseBase)EntityManager.SelectPhrase(PhraseApprStat.Identity, PhraseApprStat.PhraseIdR);
            PhraseBase phraseP = (PhraseBase)EntityManager.SelectPhrase(PhraseBillStat.Identity, PhraseBillStat.PhraseIdP);

            var currentUser = ((Personnel)Library.Environment.CurrentUser);
            var labid = currentUser.DefaultGroup.GroupId;

            var q = EntityManager.CreateQuery(MasBillingBase.EntityName);
            q.AddEquals(MasBillingPropertyNames.CustomerId, k.CustomerId);
            q.AddEquals(MasBillingPropertyNames.GroupId, labid);
            //q.AddLessThanOrEquals(MasBillingPropertyNames.GroupId, _enddate);
            q.AddGreaterThanOrEquals(MasBillingPropertyNames.DateCreated, k.DateCreated);
            q.PushBracket();
            q.AddEquals(MasBillingPropertyNames.StatusMessage, "Retrying - Previous send failed");
            q.AddOr();
            q.AddEquals(MasBillingPropertyNames.StatusMessage, "Send failed - exceeded max attempts");
            q.PopBracket();
            var res2 = EntityManager.Select(q).ActiveItems.Cast<MasBillingBase>();

            if ((res2.Count() > 0))
            {
                Library.Utils.FlashMessage("Cannot Reset - Subsequent LSR already generated", "");
                return;
            }

            //MasBillingDetailsBase q = (MasBillingDetailsBase)EntityManager.CreateEntity(MasBillingDetailsBase.EntityName, new Identity(id1));
            var query = EntityManager.CreateQuery(MasBillingDetailsBase.EntityName);
            query.AddEquals(MasBillingDetailsPropertyNames.MasId, k.RecordId);
            var sdata = EntityManager.Select(query).ActiveItems.Cast<MasBillingDetailsBase>();

            try
            {

                k.ApprovalStatus = apprphrase;
                k.Status = "F";
                k.StatusMessage = "Reset - Previous send failed";
                k.DateProcessed = DateTime.Now;
                foreach (var l in sdata)
                {
                    l.StatusMessage = "F";
                    EntityManager.Transaction.Add(l);
                }
                EntityManager.Transaction.Add(k);


                // update jobids
                var jobs = sdata.Select(m => (object)m.JobName).Distinct().ToList();

                var q1 = EntityManager.CreateQuery(JobHeader.EntityName);
                q1.AddIn(JobHeaderPropertyNames.JobName, jobs);
                var jobids = EntityManager.Select(q1).ActiveItems.Cast<JobHeader>();
                foreach (var jobid in jobids)
                {
                    jobid.BillingStatus = phraseP;
                    EntityManager.Transaction.Add(jobid);
                }

                // update samples
                var samples = sdata.Select(m => (object)m.Sample).Distinct().ToList();
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
                    sample.BillingStatus = phraseP;
                    EntityManager.Transaction.Add(sample);
                }

                //update tests
                var tests = sdata.Select(m => (object)m.TestNumber).Distinct().ToList();
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
                    test.BillingStatus = phraseP;
                    EntityManager.Transaction.Add(test);
                }
            }
            catch (Exception e)
            {
                // update mas details
                foreach (var record in sdata)
                {
                    record.StatusMessage = "F";
                    EntityManager.Transaction.Add(record);
                }
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
        private void MasBilling_Reject_LSR(MasBillingBase q)
        {
            var apprphrase = (PhraseBase)EntityManager.SelectPhrase(PhraseApprStat.Identity, PhraseApprStat.PhraseIdR);
            q.DateProcessed = DateTime.Now;
            q.ApprovalStatus = apprphrase;
            EntityManager.Transaction.Add(q);
            EntityManager.Commit();
        }

        private void MasBilling_Approve_LSR(MasBillingBase mas)
        {
            //********************commented since  done in background **************//
            // UpdateinvoicedBilled(mas);
            //****************************************************************************************//

            //update mas 
            var apprphrase = (PhraseBase)EntityManager.SelectPhrase(PhraseApprStat.Identity, PhraseApprStat.PhraseIdA);
            mas.StatusMessage = "Queued for Transfer";
            mas.DateProcessed = DateTime.Now;
            mas.ApprovalStatus = apprphrase;
            EntityManager.Transaction.Add(mas);
            EntityManager.Commit();
        }


        //private IEnumerable<IEnumerable<T>> BuildChunksWithLinqAndYield<T>(List<T> fullList, int batchSize)
        //{
        //    int total = 0;
        //    while (total < fullList.Count)
        //    {
        //        yield return fullList.Skip(total).Take(batchSize);
        //        total += batchSize;
        //    }
        //}



        //private void MasBilling_Transfer_Pending(MasBillingBase q)
        //{
        //    var q1 = EntityManager.CreateQuery(Test.EntityName);
        //    q1.AddEquals(TestPropertyNames.MasNumber, q.RecordId.String);
        //    var tests = EntityManager.Select(q1).ActiveItems.Cast<Test>();
        //    var phrase = (PhraseBase)EntityManager.SelectPhrase(PhraseBillStat.Identity, PhraseBillStat.PhraseIdB);

        //    foreach (Test test in tests)
        //    {
        //        test.BillingStatus = phrase;
        //        EntityManager.Transaction.Add(test);
        //    }

        //    var jobids = tests.GroupBy(x => x.Sample.JobName).Select(v => (object)v.Key).ToList();
        //    var q2 = EntityManager.CreateQuery(JobHeader.EntityName);
        //    q2.AddIn(JobHeaderPropertyNames.JobName, jobids);
        //    var jobs = EntityManager.Select(q2).ActiveItems.Cast<JobHeader>();
        //    foreach (var job in jobs)
        //    {
        //        job.BillingStatus = phrase;
        //        EntityManager.Transaction.Add(job);
        //    }

        //    var sampleids = tests.GroupBy(x => x.Sample).Select(v => (object)v.Key.IdText).ToList();
        //    var q3 = EntityManager.CreateQuery(Sample.EntityName);
        //    q3.AddIn(SamplePropertyNames.IdText, sampleids);
        //    var samples = EntityManager.Select(q3).ActiveItems.Cast<Sample>();
        //    foreach (var sample in samples)
        //    {
        //        sample.BillingStatus = phrase;
        //        EntityManager.Transaction.Add(sample);
        //    }

        //    //foreach (SampleBase sample in tests.GroupBy(x => x.Sample).Select(v=>v.Key))
        //    //{
        //    //    SampleBase s = sample;
        //    //    s.BillingStatus = phrase;
        //    //    EntityManager.Transaction.Add(s);
        //    //}
        //    //foreach (JobHeaderBase job in tests.GroupBy(x => x.Sample.JobName).Select(v=>v.Key))
        //    //{
        //    //    JobHeaderBase j = job;
        //    //    j.BillingStatus = phrase;
        //    //    EntityManager.Transaction.Add(j);
        //    //}

        //    q.StatusMessage = "Transfer Pending";
        //    EntityManager.Transaction.Add(q);
        //    EntityManager.Commit();

        //}

        private void MasBilling_Send(string maspath, string serverpath, MasBillingBase q)
        {
            bool status;
            //var q = EntityManager.CreateQuery(MasBillingBase.EntityName) as MasBillingBase;
            var serverpathextn = "";
            var maspathextn = "";
            try
            {
                serverpathextn = serverpath + ".csv";
                maspathextn = maspath + ".csv";

                using (new NetworkConnection(MAS_PATH, new System.Net.NetworkCredential("VMware", "Deibel123")))
                {
                    if (File.Exists(serverpathextn))
                    {
                        File.Copy(serverpathextn, maspathextn);
                    }
                }

                //if (File.Exists(serverpathextn))
                //{
                //    File.Copy(serverpathextn, maspathextn);
                //}

                if (q.Mode != "POS")
                {
                    serverpathextn = serverpath + ".xlsx";
                    maspathextn = maspath + ".xlsx";


                    using (new NetworkConnection(MAS_PATH, new System.Net.NetworkCredential("VMware", "Deibel123")))
                    {
                        if (File.Exists(serverpathextn))
                        {
                            File.Copy(serverpathextn, maspathextn);
                        }
                    }

                    //if (File.Exists(serverpathextn))
                    //{
                    //    File.Copy(serverpathextn, maspathextn);
                    //}

                    q.Status = "C";
                    q.StatusMessage = "Send complete";

                }
                else
                {
                    serverpathextn = q.XlFileName;
                    maspathextn = maspath + "\\" + prepareXLFiles(q.XlFileName);
                }

                //added to cater billing _status
                if (q.Status == "P")
                    UpdateToBilledStatus(q);
            }
            catch (Exception e)
            {
                //q.Status = "F";
                //q.StatusMessage = "Send failed - exceeded max attempts";

                var tries = q.Tries;
                if (tries < 3)
                {
                    //IF tries < GLOBAL("MAX_MAS_ATTEMPTS") THEN
                    q.Status = "R";
                    q.StatusMessage = "Retrying - Previous send failed";
                    q.Tries = tries + 1;
                }
                else
                {
                    q.Status = "F";
                    q.StatusMessage = "Send failed - exceeded max attempts";
                }
            }
            q.DateProcessed = DateTime.Now;
            EntityManager.Transaction.Add(q);
            EntityManager.Commit();

        }


        private string prepareXLFiles(string full_file)
        {
            int pos;
            string file_name;

            pos = full_file.IndexOf("MAS-");

            file_name = full_file.Substring(pos, 50); //STRIP(substring(full_file, pos, 50))

            return file_name;
        }

        private void MasBilling_ReSend(string maspath, string serverpath, MasBillingBase q)
        {
            bool status;
            //var q = EntityManager.CreateQuery(MasBillingBase.EntityName) as MasBillingBase;

            q.Status = "R";
            q.StatusMessage = "Billing transfer reset";

            q.DateProcessed = DateTime.Now;
            EntityManager.Transaction.Add(q);
            EntityManager.Commit();

        }

        private void MasBilling_Cancel(MasBillingBase q)
        {
            bool status;
            // var q = EntityManager.CreateQuery(MasBillingBase.EntityName) as MasBillingBase;


            q.Status = "F";
            q.StatusMessage = "Billing transfer cancelled";

            q.DateProcessed = DateTime.Now;
            EntityManager.Transaction.Add(q);
            EntityManager.Commit();

        }


        private void MasBilling_Transfer_Cancel(MasBillingBase q)
        {
            //q.Status = "R";
            q.RemoveFlag = true;
            q.DateProcessed = DateTime.Now;
            EntityManager.Transaction.Add(q);
            EntityManager.Commit();

            //throw new NotImplementedException();
        }

        private void UpdateToBilledStatus(MasBillingBase q)
        {
            var qt = this.EntityManager.CreateQuery(MasBillingDetailsBase.EntityName);
            qt.AddEquals(MasBillingDetailsPropertyNames.StatusMessage, "P");
            qt.AddEquals(MasBillingDetailsPropertyNames.MasId, q.RecordId);

            var data = EntityManager.Select(qt).ActiveItems.Cast<MasBillingDetailsBase>();
            PhraseBase phraseB = (PhraseBase)EntityManager.SelectPhrase(PhraseBillStat.Identity, PhraseBillStat.PhraseIdB);

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
            var q2 = EntityManager.CreateQuery(Sample.EntityName);
            q2.AddIn(SamplePropertyNames.IdText, samples);
            var sampleids = EntityManager.Select(q2).ActiveItems.Cast<Sample>();
            foreach (var sample in sampleids)
            {
                sample.BillingStatus = phraseB;
                EntityManager.Transaction.Add(sample);
            }

            //update tests
            var tests = data.Select(m => (object)m.TestNumber).Distinct().ToList();
            var q3 = EntityManager.CreateQuery(Test.EntityName);
            q3.AddIn(TestPropertyNames.TestNumber, tests);
            var testids = EntityManager.Select(q3).ActiveItems.Cast<Test>();
            foreach (var test in testids)
            {
                test.BillingStatus = phraseB;
                test.MasNumber = q.RecordId.ToString();
                EntityManager.Transaction.Add(test);
            }
        }

    }
}