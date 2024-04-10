﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.CommandLine;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common;
using Thermo.SampleManager.Common.Utilities;

using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;
using System.IO;
using System.Net;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(MASBillingBackgroundTask))]
    public class MASBillingBackgroundTask : SampleManagerTask, IBackgroundTask
    {
        string MAS_PATH;
        string SERVER_PATH;


        protected override void SetupTask() // this event executed whenever form task is called
        {

            // Check if this is run in background, i.e. executed by timer queue service
            //if (Library.Environment.IsBackground())
            //    return;
            //if (Library.Environment.IsInteractive())
            //{
            //    Launch();
            //}
            Launch();
            // If it's not from timer queue, the intention is to debug and this is called
            // from a menu item.  

        }


        public void Launch()
        {
            SERVER_PATH = Library.Environment.GetFolderList("smp$billing").ToString();
            MAS_PATH = Library.Environment.GetFolderList("smp$masdestination").ToString();

            var q = EntityManager.CreateQuery(MasBillingBase.EntityName);
            q.PushBracket();
            q.AddEquals(MasBillingPropertyNames.Status, "P");
            q.AddOr();
            q.AddEquals(MasBillingPropertyNames.Status, "B");
            q.AddOr();
            q.AddEquals(MasBillingPropertyNames.Status, "R");
            q.PopBracket();
            q.AddAnd();
            q.AddEquals(MasBillingPropertyNames.ApprovalStatus, "A");
            //q.AddAnd();
            //q.AddOrder(MasBillingPropertyNames.DateCreated, false);
            var data = EntityManager.Select(q);


            //var maspath = @MAS_PATH + "\\" + q.LabCode + "\\" + q.FileName;
            //var serverpath = @SERVER_PATH + "\\MAS\\" + q.FileName;

            foreach (var item in data)
            {
                var record = (MasBillingBase)item;

                //record.Status = "T";
                //EntityManager.Transaction.Add(record);
                //EntityManager.Commit();

                //var pq = EntityManager.CreateQuery(MasBillingBase.EntityName);
                //pq.AddEquals(MasBillingPropertyNames.RecordId, record.RecordId);
                //var enity = EntityManager.Select(pq).ActiveItems.Cast<MasBillingBase>().FirstOrDefault();

                MasBilling_Send(MAS_PATH + "//" + record.LabCode + "//" + record.FileName,
                     SERVER_PATH + "\\" + record.FileName,
                    record);

            }
        }


        private void MasBilling_Send(string maspath, string serverpath, MasBillingBase q)
        {
            string status = q.Status;
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

                    q.Status = "T";
                    q.StatusMessage = "Send complete";

                }
                else
                {
                    serverpathextn = q.XlFileName;
                    maspathextn = maspath + "\\" + prepareXLFiles(q.XlFileName);

                }

                //if (status.Trim() == "P" || status.Trim() == "T")
                //    UpdateToBilledStatus(q);
            }
            catch (Exception ex)
            {
                //q.Status = "F";
                //q.StatusMessage = "Send failed - exceeded max attempts";

                var tries = q.Tries;
                if (tries < 5)
                {
                    //IF tries < GLOBAL("MAX_MAS_ATTEMPTS") THEN
                    q.Status = "R";
                    q.StatusMessage = "Retrying - Previous send failed";
                    q.Tries = tries + 1;
                    q.ErrorDescription = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                }
                else
                {
                    q.Status = "F";
                    q.StatusMessage = "Send failed - exceeded max attempts";
                    q.ErrorDescription = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
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