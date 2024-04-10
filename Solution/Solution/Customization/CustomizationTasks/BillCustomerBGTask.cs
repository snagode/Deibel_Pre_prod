﻿using System;
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
    [SampleManagerTask(nameof(BillCustomerBGTask))]
    public class BillCustomerBGTask : SampleManagerTask, IBackgroundTask
    {
        private string masdestination = "";
        private string destination = "";

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

        /* This will update all the job/sample/tests which are in previewed state and also billed state
         * This will also update all the job/sample/Tests which are in approved state */
        public void Launch()
        {
            string SERVER_PATH = Library.Environment.GetFolderList("smp$billing").ToString();
            destination = SERVER_PATH + "\\";

            PhraseBase phraseB = (PhraseBase)EntityManager.SelectPhrase(PhraseBillStat.Identity, PhraseBillStat.PhraseIdB);
            PhraseBase phraseP = (PhraseBase)EntityManager.SelectPhrase(PhraseBillStat.Identity, PhraseBillStat.PhraseIdP);
            PhraseBase phrase = null;

            var q = this.EntityManager.CreateQuery(MasBillingDetailsBase.EntityName);
            q.PushBracket();
            q.AddEquals(MasBillingDetailsPropertyNames.StatusMessage, "P");
            q.AddOr();
            q.AddEquals(MasBillingDetailsPropertyNames.StatusMessage, "B");
            q.PopBracket();
            q.AddNotEquals(MasBillingDetailsPropertyNames.StatusMessage, "F");
            var data = EntityManager.Select(q).ActiveItems.Cast<MasBillingDetailsBase>().GroupBy(z => z.MasId);

            var uniqueMasId = data.Select(m => m.Select(n => n.MasId)).Distinct();

            MasBillingBase mas = (MasBillingBase)EntityManager.CreateEntity(MasBillingBase.EntityName);



            foreach (var sdata in data)
            {
                var masid = sdata.FirstOrDefault().MasId;
                var masq = this.EntityManager.CreateQuery(MasBillingBase.EntityName);
                masq.AddEquals(MasBillingPropertyNames.RecordId, masid);
                mas = EntityManager.Select(masq).ActiveItems.Cast<MasBillingBase>().FirstOrDefault();

                try
                {
                    // update mas
                    if (mas.StatusMessage == "Approve Previewed LSR")
                    {
                        GenerteCSV(mas);
                        mas.Status = "P"; //"Previewed";
                                          //mas.StatusMessage = "Queue for Transfer";
                        phrase = phraseP;
                    }
                    else if (mas.StatusMessage == "Queued for Transfer")
                    {
                        mas.Status = "B";
                        phrase = phraseB;


                    }
                    EntityManager.Transaction.Add(mas);

                    // update mas details
                    foreach (var record in sdata)
                    {
                        record.StatusMessage = "C";
                        EntityManager.Transaction.Add(record);
                    }

                    // update jobids
                    var jobs = sdata.Select(m => (object)m.JobName).Distinct().ToList();

                    var q1 = EntityManager.CreateQuery(JobHeader.EntityName);
                    q1.AddIn(JobHeaderPropertyNames.JobName, jobs);
                    var jobids = EntityManager.Select(q1).ActiveItems.Cast<JobHeader>();
                    foreach (var jobid in jobids)
                    {
                        jobid.BillingStatus = phrase;
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
                        sample.BillingStatus = phrase;
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
                        test.BillingStatus = phrase;
                        EntityManager.Transaction.Add(test);
                    }

                }
                catch (Exception e)
                {


                    // update mas details
                    //foreach (var record in sdata)
                    //{
                    mas.StatusMessage = "Failed";
                    if (e.InnerException != null)

                        mas.ErrorDescription = "Trasient state error: " + e.InnerException;
                    else
                        mas.ErrorDescription = "Trasient state error: " + e.Message;
                    EntityManager.Transaction.Add(mas);

                    //}



                }

                EntityManager.Commit();
            }



            //var jobs = data.Select(m => (object)m.JobName).Distinct().ToList();
            //var q1 = EntityManager.CreateQuery(JobHeader.EntityName);
            //q1.AddIn(JobHeaderPropertyNames.JobName, jobs);
            //var jobids = EntityManager.Select(q1).ActiveItems.Cast<JobHeader>();
            //foreach (var jobid in jobids)
            //{
            //    jobid.BillingStatus = phrase;
            //    EntityManager.Transaction.Add(jobid);
            //}

            // update samples
            //var samples = data.Select(m => (object)m.IdText).Distinct().ToList();
            //var q2 = EntityManager.CreateQuery(Sample.EntityName);
            //q2.AddIn(SamplePropertyNames.IdText, samples);h
            //var sampleids = EntityManager.Select(q2).ActiveItems.Cast<Sample>();
            //foreach (var sample in sampleids)
            //{
            //    sample.BillingStatus = phrase;
            //    EntityManager.Transaction.Add(sample);
            //}
            //update tests
            //var tests = data.Select(m => (object)m.TestNumber).Distinct().ToList();
            //var q3 = EntityManager.CreateQuery(Test.EntityName);
            //q3.AddIn(TestPropertyNames.TestNumber, tests);
            //var testids = EntityManager.Select(q3).ActiveItems.Cast<Test>();
            //PackedDecimal mas_number = new PackedDecimal();
            //mas_number = id;
            //foreach (var test in testids)
            //{
            //    test.BillingStatus = phrase;
            //    test.MasNumber = mas_number.String;
            //    EntityManager.Transaction.Add(test);
            //}

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


        public class masrow
        {
            public string InvDate;
            public string Division;
            public string CustNo;
            public string PO;
            public string ColName;
            public string Totals;
            public string BusinessLine;
            public string LocationId;
            public string SIDepositNum;
            public string SIDepositAmt;
        }

        private void GenerteCSV(MasBillingBase mas)
        {
            var res = false;
            var billtype = mas.Mode;
            var customerid = mas.CustomerId.CustomerName;
            var filename = Path.GetFileNameWithoutExtension(mas.XlFileName);
            List<masrow> MASRows = new List<masrow>();
            // set path
            var path = destination + filename;

            var query = EntityManager.CreateQuery(DeibelLabCustomerBase.EntityName);
            query.AddEquals(DeibelLabCustomerPropertyNames.CustomerId, customerid);
            query.AddEquals(DeibelLabCustomerPropertyNames.GroupId, mas.GroupId);
            var DeibelLabCustomer = EntityManager.Select(query).ActiveItems.Cast<DeibelLabCustomerBase>().FirstOrDefault();

            var q = this.EntityManager.CreateQuery(MasBillingdetailsViewBase.EntityName);
            q.AddEquals(MasBillingdetailsViewPropertyNames.MasId, mas.RecordId);
            //q.AddOr();sta
            //q.AddEquals(MasBillingDetailsPropertyNames.MasId, "B");
            var data = EntityManager.Select(q).ActiveItems.Cast<MasBillingdetailsViewBase>()
            //.GroupBy(c => new { c.JobName,c.CustomerId, c.LabId, c.PoNumber, c.IdText, c.TestCount, c.Analysis, c.Description, c.TestNumber });
            .Select(n => new { n.PoNumber, n.Analysis, n.TestCount, n.TestNumber, n.CustomerId, n.SiBlId, n.LabId.SiELocation, n.SldepositNum, n.SldepositAmt });
            string PONumber = string.Empty;
            switch (billtype)
            {

                case "SINGLE":
                    //var PONumber = data.FirstOrDefault().PoNumber;
                    if (data.GroupBy(j => j.PoNumber).Count() > 1)
                        PONumber = "See Lab Report Summary";
                    else
                        PONumber = data.FirstOrDefault().PoNumber;
                    MASRows = data.GroupBy(p => new { p.Analysis, p.CustomerId, p.TestCount, p.SiELocation, p.SiBlId, p.SldepositNum, p.SldepositAmt }).   // p.PoNumber,
                    Select(j => new masrow
                    {
                        CustNo = j.Key.CustomerId.CustomerName,
                        ColName = j.Key.Analysis,
                        PO = PONumber,
                        BusinessLine = j.Key.SiBlId.ToString(),
                        LocationId = j.Key.SiELocation.ToString(),
                        SIDepositNum = j.Key.SldepositNum.ToString(),
                        SIDepositAmt = j.Key.SldepositAmt.ToString(),
                        Totals = j.Sum(t => (int)t.TestCount).ToString()
                    }).OrderBy(o => o.ColName).ToList();

                    if (MASRows.Count <= 0)
                        return;

                    res = DownloadReport(MASRows, path, mas, DeibelLabCustomer, 0);

                    break;

                case "JOB":
                    //var PONumber1 = data.FirstOrDefault().PoNumber;
                    if (data.GroupBy(j => j.PoNumber).Count() > 1)
                        PONumber = "See Lab Report Summary";
                    else
                        PONumber = data.FirstOrDefault().PoNumber;
                    MASRows = data.GroupBy(p => new { p.Analysis, p.CustomerId, p.TestCount, p.SiELocation, p.SiBlId }).   //p.PoNumber,
                    Select(b => new masrow
                    {
                        CustNo = customerid,
                        Division = customerid,
                        ColName = b.Key.Analysis,
                        PO = PONumber,
                        BusinessLine = b.Key.SiBlId.ToString(),
                        LocationId = b.Key.SiELocation.ToString(),
                        Totals = b.Sum(t => (int)t.TestCount).ToString() //b.Count().ToString()
                    }).OrderBy(o => o.ColName).ToList();

                    if (MASRows.Count <= 0)
                        return;

                    res = DownloadReport(MASRows, path, mas, DeibelLabCustomer, 0);

                    break;

                case "PO":

                    MASRows = data.GroupBy(p => new { p.PoNumber, p.Analysis, p.CustomerId, p.TestCount, p.SiELocation, p.SiBlId, }).
                    Select(b => new masrow
                    {
                        CustNo = customerid,
                        Division = customerid,
                        ColName = b.Key.Analysis,
                        //, InvDate = b.Key.DateReceived.Value.ToShortDateString(),
                        PO = b.Key.PoNumber,
                        BusinessLine = b.Key.SiBlId.ToString(),
                        LocationId = b.Key.SiELocation.ToString(),
                        Totals = b.Count().ToString()
                    }).OrderBy(o => o.ColName).ToList();

                    int counter = 0;
                    foreach (var x in MASRows.GroupBy(x => x.PO))
                    {
                        res = DownloadReport(MASRows, path, mas, DeibelLabCustomer, counter);
                        counter++;
                    }

                    //if (MASRows.Count <= 0)
                    //    return;

                    //res = DownloadReport(MASRows, path, mas, DeibelLabCustomer, 0);

                    break;

            }





            //create csv file based on bill type
            //if (billtype == "POS")
            //{
            //    var POs = MASRows.GroupBy(x => x.PO).Where(g => g.Count() > 0);
            //    int counter = 1;

            //    foreach (var i in POs)
            //    {
            //        res = DownloadReport(i.ToList(), path + "-" + counter, mas, DeibelLabCustomer);
            //        //System.Diagnostics.Process.Start(path + ".csv");
            //        counter++;

            //        if (res == false)
            //            return;
            //    }
            //}
            //else
            //{

            //}

            //if (res)     //commented after move to prod
            //{
            //    //update mas_billing_details
            //    foreach (var item in data)
            //    {
            //        var id1 = new PackedDecimal(Library.Increment.GetIncrement(TableNames.MasBillingDetails, MasBillingDetailsPropertyNames.RecordId).ToString());
            //        MasBillingDetailsBase h = (MasBillingDetailsBase)EntityManager.CreateEntity(MasBillingDetailsBase.EntityName, new Identity(id1));
            //        h.StatusMessage = "C";
            //        EntityManager.Transaction.Add(h);
            //    }
            //    //EntityManager.Commit();
            //}

            //Library.Utils.FlashMessage("File Generated succefully and Data send to queue for processing", "");
        }

        public bool DownloadReport(List<masrow> lstData, string path, MasBillingBase mas, DeibelLabCustomerBase DeibelLabCustomer, int counter)
        {
            //MAS - DMN - AGR_05 - Jul - 20210731055919
            //var _endate  = DateTime.ParseExact(mas.EndDate.Value, "M/d/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
            //path.Substring(path.LastIndexOf('-'), path.Length);

            string diebelMasCustomer1 = "";
            string deibelMasCustomer2 = "";

            //if (DeibelLabCustomer.MasCustomer != null)
            //    if (DeibelLabCustomer.MasCustomer.Split('-')[0] != null)
            //    {
            //        diebelMasCustomer1 = DeibelLabCustomer.MasCustomer.Split('-')[0];

            //        if (DeibelLabCustomer.MasCustomer.Split('-')[1] != null)
            //            deibelMasCustomer2 = DeibelLabCustomer.MasCustomer.Split('-')[1];
            //    }


            var sb = new StringBuilder();

            foreach (var data in lstData.OrderBy(x => x.ColName))
            {
                sb.AppendLine(
                    mas.EndDate.Value.ToString("yyyyMMdd") + "," +
                   // diebelMasCustomer1 + "," +
                   "" + "," +
                    //deibelMasCustomer2 + "," +
                    DeibelLabCustomer.CustomerId + "," +
                    data.PO + "," +
                    data.BusinessLine + "," +
                    data.LocationId + "," +
                    DeibelLabCustomer.Deibellabcustomer.SiCustomerid + "," +
                    ((Personnel)Library.Environment.CurrentUser).SiDeptId + "," +
                    data.ColName + "," +
                    data.Totals +
                     DeibelLabCustomer.Deibellabcustomer.Currency.PhraseText != "USD" ?
                      DeibelLabCustomer.Deibellabcustomer.Currency.PhraseText + "," +
                    DeibelLabCustomer.Deibellabcustomer.CurrencyExchange + "," +
                     DeibelLabCustomer.Deibellabcustomer.CurrencyExchType : "" +
                     data.SIDepositNum + "," +
                     data.SIDepositAmt

                    );
            }

            if (counter > 0)
                File.WriteAllText(path + "-" + counter + ".csv", sb.ToString(), Encoding.UTF8);
            else
                File.WriteAllText(path + ".csv", sb.ToString(), Encoding.UTF8);

            return true;
        }

    }

    public static class ChunkDataSize
    {
        public static IEnumerable<IEnumerable<TSource>> ChunkData<TSource>(this IEnumerable<TSource> source, int chunkSize)
        {
            for (int i = 0; i < source.Count(); i += chunkSize)
                yield return source.Skip(i).Take(chunkSize);
        }


    }
}
