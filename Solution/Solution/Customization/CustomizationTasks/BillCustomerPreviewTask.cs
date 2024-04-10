﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Server;
using Thermo.SampleManager.Tasks;
//using DocumentFormat.OpenXml;
//using DocumentFormat.OpenXml.Packaging;
//using DocumentFormat.OpenXml.Spreadsheet;
using System.IO;
using System.Globalization;
using Thermo.SampleManager.ObjectModel;
using OfficeOpenXml;
using System.Drawing;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(BillCustomerPreviewTask))]
    public class BillCustomerPreviewTask : SampleManagerTask
    {
        private FormBillCustomerPreview _form;
        UnboundGridColumn _jobName;
        UnboundGridColumn _labId;
        UnboundGridColumn _PONumber;
        UnboundGridColumn _billingMonth;
        UnboundGridColumn _dateRecieved;
        UnboundGridColumn _customerId;
        UnboundGridColumn _customerName;
        UnboundGridColumn _MAScustomer;
        UnboundGridColumn _analysis;

        private string customerid;
        private static IEntityManager _entityManager;
        private IEntityCollection _MAScustomerCollection;
        private IEnumerable<data> _MASbillCollection;
        private DeibelLabCustomerBase _deibelLabCustomer;
        private List<row> _rows;
        private List<column> Headers;
        DateTime _startdate;
        DateTime _enddate;
        private string _billType;
        private static string filename;
        private string masdestination = "";//@"C://test_folder//";
        private string destination = "";// @"C://test_folder//";
        private string ImageFile = @"C:\Thermo\SampleManager\Server\SMUAT\Imprint\excel_logo.jpg";
        private string templateFilePath = @"C:\Thermo\SampleManager\Server\SMUAT\Imprint\monthlyBilling2.xlsx";
        private string _hideGrid;

        private int _MAScustomerCollectionCount;

        public IEntityCollection MAScustomerCollection
        {
            get
            {
                return _MAScustomerCollection;
            }

            set
            {
                _MAScustomerCollection = value;
            }
        }

        public int MAScustomerCollectionCount
        {
            get
            {
                return _MAScustomerCollectionCount;
            }

            set
            {
                _MAScustomerCollectionCount = value;
            }
        }


        public IEnumerable<data> MASbillCollection
        {
            get
            {
                return _MASbillCollection;
            }

            set
            {
                _MASbillCollection = value;
            }
        }

        public DeibelLabCustomerBase DeibelLabCustomer
        {
            get
            {
                return _deibelLabCustomer;
            }

            set
            {
                _deibelLabCustomer = value;
            }
        }

        public List<row> Rows
        {
            get
            {
                return _rows;
            }

            set
            {
                _rows = value;
            }
        }

        public string HideGrid
        {
            get
            {
                return _hideGrid;
            }

            set
            {
                _hideGrid = value;
            }
        }

        protected override void SetupTask()
        {
            _form = (FormBillCustomerPreview)FormFactory.CreateForm(typeof(FormBillCustomerPreview));
            _form.Closed += _form_Closed;
            _form.Loaded += _form_Loaded;
            _form.ButtonEdit1.Click += ButtonEdit1_Click;
            _form.ButtonEdit2.Click += ButtonEdit2_Click;
            _form.btnDownLoadLSR.Click += btnDownLoadLSR_Click;
            _form.Show();
        }

        private void btnDownLoadLSR_Click(object sender, EventArgs e)
        {
            // create excel file 
            bool res = false;

            _form.SetBusy();
            if (_form.UnboundGridDesign1.FocusedRow != null)
            {
                string excelFile = getCopyExcelTemplateFile(0);
                res = WriteExcel(Rows, Headers, excelFile);
                _form.ClearBusy();
                if (res)
                {
                    // Library.Utils.FlashMessage("File Generated succefully", "");
                    _form.SetBusy();
                    Library.File.TransferToClient(excelFile, Path.GetFileName(excelFile), true);
                    //UpdateInvoicePreviewed("", excelFile);
                    _form.ClearBusy();
                }
                else
                {
                    _form.ClearBusy();
                    //Library.Utils.FlashMessage("Error Gernerating file", "");
                    return;
                }
            }
            else
            {
                Library.Utils.FlashMessage("Select the row", "");
            }


        }

        private void _form_Closed(object sender, EventArgs e)
        {
            if (_form != null)
            {
                _form.Close();
            }
        }
        public class column
        {
            public int id;
            public string name;

        }
        public class row
        {
            public int id;
            public string PO;
            public string Lab;
            public string Description;
            public string BusinessLine;
            public string LocationId;
            public string SIDepositNum;
            public string SIDepositAmt;
            public string netterms;
            public List<item> Items;
        }

        public class item
        {
            public string column;
            public int val;
        }

        public class masrow
        {
            public string InvDate;
            public string Division;
            public string CustNo;
            public string PO;
            public string ColName;
            public string BusinessLine;
            public string LocationId;
            public string SISIDepositNum;
            public string SIDepositAmt;
            public string Netterms;
            public string Totals;
        }

        public class data
        {
            public string JobName;
            public string IdText;
            public string LabId;
            public string PoNumber;
            public string Analysis;
            public int TestCount;
            public string TestNumber;
            public string CustomerId;
        }

        private void _form_Loaded(object sender, EventArgs e)
        {
            var i = this.Context.TaskParameters;
            customerid = this.Context.TaskParameters[0]; //((CustomerToBilledVwBase)this.Context.SelectedItems.ActiveItems.First()).CustomerId;
            var startdate = this.Context.TaskParameters[1];
            var enddate = this.Context.TaskParameters[2];
            _billType = this.Context.TaskParameters[3];
            HideGrid = this.Context.TaskParameters[5];

            _startdate = DateTime.ParseExact(startdate, "M/d/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
            _enddate = DateTime.ParseExact(enddate, "M/d/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
            string SERVER_PATH = Library.Environment.GetFolderList("smp$billing").ToString();
            destination = SERVER_PATH + "\\";
            var currentUser = ((Personnel)Library.Environment.CurrentUser);
            var labid = currentUser.DefaultGroup.GroupId;

            var query = EntityManager.CreateQuery(DeibelLabCustomerBase.EntityName);
            query.AddEquals(DeibelLabCustomerPropertyNames.CustomerId, customerid);
            query.AddEquals(DeibelLabCustomerPropertyNames.GroupId, labid);                                        //group id enable 
            DeibelLabCustomer = EntityManager.Select(query).ActiveItems.Cast<DeibelLabCustomerBase>().FirstOrDefault();

            if (DeibelLabCustomer == null)
                Library.Utils.FlashMessage("Cusomer not available.", "");

            filename = GetFileName(DeibelLabCustomer);
            _form.lblCaption.Caption = string.Format(" {0} - {1}-{2}", DeibelLabCustomer.Deibellabcustomer.CompanyName,
                !string.IsNullOrEmpty(DeibelLabCustomer.MasCustomer) ? DeibelLabCustomer.MasCustomer.Split('-')[0].ToString() : "",
                !string.IsNullOrEmpty(DeibelLabCustomer.MasCustomer) ? DeibelLabCustomer.MasCustomer.Split('-').Length == 1 ? DeibelLabCustomer.MasCustomer.Split('-')[1] : "" : "");

            if (HideGrid == "false")
                BindGrid2();
        }

        private void BindGrid()
        {
            _form.SetBusy();
            var grid = _form.UnboundGridDesign1;
            grid.BeginUpdate();
            grid.ClearRows();
            BuildColumns();

            var q = EntityManager.CreateQuery(MonthlyBillingViewBase.EntityName);
            q.AddEquals(MonthlyBillingViewPropertyNames.CustomerId, customerid);
            //q.AddGreaterThan(MonthlyBillingViewPropertyNames.DateReceived, _startdate.ToUniversalTime());
            q.AddLessThan(MonthlyBillingViewPropertyNames.DateAuthorised, _enddate.ToUniversalTime());
            MAScustomerCollection = EntityManager.Select(q);


            var id = "";
            foreach (MonthlyBillingViewBase item in MAScustomerCollection.ActiveItems.ToList())
            {
                UnboundGridRow row = grid.AddRow();
                row.Tag = item;
                row.SetValue(_jobName, item.JobName);
                row.SetValue(_labId, item.LabId);
                ////row.SetValue(_PONumber, item.PoNumber);
                row.SetValue(_dateRecieved, item.DateAuthorised);
                row.SetValue(_customerId, item.CustomerId);
                row.SetValue(_customerName, item.CustomerName);
                row.SetValue(_MAScustomer, item.MasCustomer);
                row.SetValue(_analysis, item.Analysis);
            }
            grid.EndUpdate();
            _form.ClearBusy();
        }


        private void BindGrid2()
        {
            _form.SetBusy();
            var grid = _form.UnboundGridDesign1;

            var currentUser = ((Personnel)Library.Environment.CurrentUser);
            var labid = currentUser.DefaultGroup.GroupId;
            var billtype = _billType;

            grid.BeginUpdate();
            grid.ClearRows();


            var q = EntityManager.CreateQuery(MonthlyBillingViewBase.EntityName);
            q.AddEquals(MonthlyBillingViewPropertyNames.CustomerId, customerid);
            // q.AddGreaterThan(MonthlyBillingViewPropertyNames.DateReceived, _startdate.ToUniversalTime());
            q.AddEquals(MonthlyBillingViewPropertyNames.LabId, labid);                                      //group id enable 
            q.AddLessThan(MonthlyBillingViewPropertyNames.DateAuthorised, _enddate.ToUniversalTime());
            MAScustomerCollection = EntityManager.Select(q);
            MAScustomerCollectionCount = EntityManager.Select(q).ActiveCount;
            if (_billType == "Preview By Sample" || _billType == "Preview By PO")
            {
                //var q = EntityManager.CreateQuery(MonthlyBillingViewBase.EntityName);
                //q.AddEquals(MonthlyBillingViewPropertyNames.CustomerId, customerid);
                ////q.AddGreaterThan(MonthlyBillingViewPropertyNames.DateReceived, _startdate.ToUniversalTime());
                //// q.AddEquals(MonthlyBillingViewPropertyNames.LabId, labid);
                //q.AddLessThan(MonthlyBillingViewPropertyNames.DateReceived, _enddate.ToUniversalTime());
                //MAScustomerCollection = EntityManager.Select(q);

                // prepare data for the grid
                CreateMasCustomerHeadersAndRows();
            }
            else if (_billType == "Preview By Job")
            {
                //var q = EntityManager.CreateQuery(MonthlyBillingViewBase.EntityName);
                //q.AddEquals(MonthlyBillingViewPropertyNames.CustomerId, customerid);
                ////q.AddGreaterThan(MonthlyBillingViewPropertyNames.DateReceived, _startdate.ToUniversalTime());
                //q.AddEquals(MonthlyBillingJobVPropertyNames.LabId, labid);
                //q.AddLessThan(MonthlyBillingViewPropertyNames.DateReceived, _enddate.ToUniversalTime());
                //MAScustomerCollection = EntityManager.Select(q);

                // prepare data for the grid
                CreateMasCustomerHeadersAndRowsJob();
            }


            // build grid columns
            BuildColumns2();
            // build grid rows 
            foreach (var item in Rows)
            {
                UnboundGridRow row = grid.AddRow();
                //row.Tag = item;
                foreach (var item2 in grid.Columns)
                {
                    if (item2.Caption == "PO")
                        row.SetValue(item2, item.PO);
                    else if (item2.Caption == "LAB NO")
                        row.SetValue(item2, item.Lab);
                    else if (item2.Caption == "Description")
                        row.SetValue(item2, item.Description);
                    else
                    {
                        var val = item.Items.Where(m => m.column == item2.Name).Select(n => n.val).FirstOrDefault();
                        row.SetValue(item2, val);
                    }
                }
            }

            //build grid footer
            UnboundGridRow rowfooter = grid.AddRow();
            foreach (var item2 in grid.Columns)
            {
                var val = Rows.SelectMany(n => n.Items.Where(m => m.column == item2.Caption).Select(c => c.val)).Sum();
                rowfooter.SetValue(item2, val);
            }

            var coldescription = grid.Columns.Where(m => m.Caption == "Description").FirstOrDefault();
            var colLABNO = grid.Columns.Where(m => m.Caption == "LAB NO").FirstOrDefault();
            var colPO = grid.Columns.Where(m => m.Caption == "PO").FirstOrDefault();
            rowfooter.SetValue(coldescription, "Totals");
            rowfooter.SetValue(colLABNO, "");
            rowfooter.SetValue(colPO, "");

            grid.EndUpdate();
            _form.ClearBusy();
        }


        void BuildColumns2()
        {
            var grid = _form.UnboundGridDesign1;
            _jobName = grid.AddColumn(MonthlyBillingViewPropertyNames.JobName, "PO");
            _labId = grid.AddColumn(MonthlyBillingViewPropertyNames.LabId, "LAB NO");
            _PONumber = grid.AddColumn(MonthlyBillingViewPropertyNames.PoNumber, "Description");
            foreach (var item in Headers)
            {
                var maxLength = Headers.Max(x => x.name.Length);
                var columnWidth = (maxLength * 4) < 80 ? 80 : maxLength * 4;
                grid.AddColumn(item.name, item.name, width: columnWidth);
            }
        }

        void BuildColumns()
        {
            var grid = _form.UnboundGridDesign1;
            _jobName = grid.AddColumn(MonthlyBillingViewPropertyNames.JobName, "Job Name");
            _labId = grid.AddColumn(MonthlyBillingViewPropertyNames.LabId, "Lab Id");
            _PONumber = grid.AddColumn(MonthlyBillingViewPropertyNames.PoNumber, "PO Number");
            _billingMonth = grid.AddColumn(MonthlyBillingViewPropertyNames.BillingMonth, "Billing Month");
            _dateRecieved = grid.AddColumn(MonthlyBillingViewPropertyNames.DateAuthorised, "Date Authorised");
            _customerId = grid.AddColumn(MonthlyBillingViewPropertyNames.CustomerId, "Customer Id");
            _customerName = grid.AddColumn(MonthlyBillingViewPropertyNames.CustomerName, "Customer Name");
            _MAScustomer = grid.AddColumn(MonthlyBillingViewPropertyNames.MasCustomer, "MAS Customer");
            _analysis = grid.AddColumn(MonthlyBillingViewPropertyNames.Analysis, "Aanalysis");
        }

        private string GetFileName(DeibelLabCustomerBase deibelLabCustomerBase)
        {
            DateTime dt = DateTime.Now;
            return "MAS-" + deibelLabCustomerBase.GroupId + "-" + deibelLabCustomerBase.CustomerId + "-" + _enddate.ToString("MMM") + "-" + dt.ToString("yyyyMMddHHmmss");
        }

        private void ButtonEdit1_Click(object sender, EventArgs e)
        {
            //var stroo= Library.Environment.GetGlobalBoolean("EXPLORER_DEFAULT_PATH");

            //CreateMasCustomerHeadersAndRows();
            //ExportData(Rows, templateFilePath);
            var path = destination + filename;
            // get excel(xlsx) file name 

            var billtype = _billType;

            // create excel file 
            bool res = false;

            if (Rows.Count <= 0)
                Library.Utils.FlashMessage("No Data to generate LSR ", "");

            ////check existing mas id's and disabled it 
            //if (!CheckExistingInvoiceAny())
            //{
            //    Library.Utils.FlashMessage("Please wait as there is a LSR approved and is under process", "");
            //    return;
            //}

            CheckExistingInvoiceAny();

            if (_billType == "Preview By PO")
            {
                var POs = Rows.GroupBy(c => c.PO).Where(g => g.Count() > 0);
                int counter = 0;
                if (POs.Count() > 0)
                {
                    foreach (var i in POs)
                    {
                        _form.SetBusy();
                        var n = i.ToList().Select(m => m.id);
                        string excelFile = getCopyExcelTemplateFile(counter);
                        res = WriteExcel(i.ToList(), Headers, excelFile);
                        //System.Diagnostics.Process.Start(path + ".csv");
                        var PO = i.FirstOrDefault().PO;
                        counter++;
                        _form.ClearBusy();

                        if (res)
                        {
                            Library.Utils.FlashMessage("File Generated succefully for PO " + PO, "");
                            _form.SetBusy();
                            Library.File.TransferToClient(excelFile, Path.GetFileName(excelFile), true);
                            UpdateInvoicePreviewed(i.FirstOrDefault().PO, excelFile);
                            _form.ClearBusy();
                        }
                        else
                        {
                            _form.ClearBusy();
                            Library.Utils.FlashMessage("Error Gernerating file", "");
                            return;
                        }
                    }
                }
            }
            else
            {
                _form.SetBusy();
                string excelFile = getCopyExcelTemplateFile(0);
                res = WriteExcel(Rows, Headers, excelFile);
                _form.ClearBusy();

                if (res)
                {
                    Library.Utils.FlashMessage("File Generated succefully", "");
                    _form.SetBusy();
                    Library.File.TransferToClient(excelFile, Path.GetFileName(excelFile), true);
                    UpdateInvoicePreviewed("", excelFile);
                    _form.ClearBusy();
                }
                else
                {
                    _form.ClearBusy();
                    Library.Utils.FlashMessage("Error Gernerating file", "");
                    return;
                }


            }

            //if (res)
            //{



            //File.Copy(excelFile, "C:\\SampleManager\\PDF Files\\"+Path.GetFileName(excelFile));
            //ProcessStartInfo processInfo = new ProcessStartInfo();
            //processInfo.FileName = excelFile;
            //processInfo.ErrorDialog = true;
            //processInfo.UseShellExecute = false;
            //processInfo.RedirectStandardOutput = true;
            //processInfo.RedirectStandardError = true;

            //processInfo.WorkingDirectory = Path.GetDirectoryName(excelFile);
            //Process proc = Process.Start(processInfo);

            //Process proc = Process.Start(processInfo);
            //System.Diagnostics.Process.Start(excelFile);
            //var p = new Process();
            //p.StartInfo = new ProcessStartInfo(excelFile)
            //{
            //    UseShellExecute = true
            //};
            //p.Start();

            //UpdateInvoicePreviewed();

            //var backgroundWorker = new System.ComponentModel.BackgroundWorker();
            //BillCustomerBGTaskPreviewed task = new BillCustomerBGTaskPreviewed();
            //string[] _args = { customerid, _startdate.ToString(), _enddate.ToString(), _billType, filename, path };
            //object _o = (object)_args;
            //task.Launch();
            // backgroundWorker.DoWork += (o, args) => task.UpdateInvoicePreviewed(_o, args);
            // backgroundWorker.RunWorkerAsync();
            // backgroundWorker.RunWorkerCompleted += (o, args) => _form.HighlightButton.Enabled = true;
            // Library.Utils.FlashMessage("Data send to queue for processing", "");
            //}
            //else
            //    Library.Utils.FlashMessage("File Could not be created", "");

            //if (_billType == "Preview By PO")
            //{

            //    var POs = Rows.GroupBy(c => c.PO).Where(g => g.Count() > 0);
            //    if (POs.Count() > 0)
            //    {
            //        foreach (var i in POs)
            //        {
            //            UpdateInvoicePreviewed(i.FirstOrDefault().PO);
            //        }
            //    }
            //    Library.Utils.FlashMessage("Data send to queue for processing", "");
            //}
            //else
            //{
            //    UpdateInvoicePreviewed("");
            //    Library.Utils.FlashMessage("Data send to queue for processing", "");
            //}
            Library.Utils.FlashMessage("Data send to queue for processing", "");
            _form.Close();
        }

        private void UpdateInvoicePreviewed(string PONumber, string excelpath)
        {
            // Create entry for  mas_blilling 

            //_form.SetBusy();

            var apprphrase = (PhraseBase)EntityManager.SelectPhrase(PhraseApprStat.Identity, PhraseApprStat.PhraseIdP);

            var path = destination + filename;
            var billtype = _billType;
            //List<masrow> MASRows = new List<masrow>();

            var data = MAScustomerCollection.ActiveItems.Cast<MonthlyBillingViewBase>().Select(n =>
                                new { n.JobName, n.IdText, n.LabId, n.PoNumber, n.Analysis, n.TestCount, n.TestNumber, n.Description, n.CustomerId }).Where(m => m.PoNumber == PONumber || string.IsNullOrEmpty(PONumber)).Distinct();

            //update mas_billing
            var id = new PackedDecimal(Library.Increment.GetIncrement(TableNames.MasBilling, MasBillingPropertyNames.RecordId).ToString());
            MasBillingBase record = (MasBillingBase)EntityManager.CreateEntity(MasBillingBase.EntityName, new Identity(id));
            var currentUser = (Personnel)Library.Environment.CurrentUser;
            //record.RecordId  = Library.Increment.GetIncrement(TableNames.MasBilling, MasBillingPropertyNames.RecordId);
            record.DateCreated = DateTime.Now;
            record.CreatedBy = currentUser;
            record.CustomerId = DeibelLabCustomer.Deibellabcustomer;
            record.LabCode = DeibelLabCustomer.LabCode;
            record.FileName = Path.GetFileNameWithoutExtension(excelpath);
            record.XlFileName = excelpath;// path + ".xlsx";
            record.StartDate = _startdate;
            record.EndDate = _enddate;

            if (_billType == "Preview By Sample")
            {
                record.Mode = "SINGLE";
            }
            else if (_billType == "Preview By Job")
            {
                record.Mode = "JOB";
            }
            else
            {
                record.Mode = "PO";
            }

            record.Status = "T";
            record.Tries = 1;
            record.StatusMessage = "Approve Previewed LSR";
            record.GroupId = ((Personnel)Library.Environment.CurrentUser).GroupId.GroupId;
            record.ApprovalStatus = apprphrase;
            EntityManager.Transaction.Add(record);

            foreach (var item in data)
            {
                var id1 = new PackedDecimal(Library.Increment.GetIncrement(TableNames.MasBillingDetails, MasBillingDetailsPropertyNames.RecordId).ToString());
                MasBillingDetailsBase q = (MasBillingDetailsBase)EntityManager.CreateEntity(MasBillingDetailsBase.EntityName, new Identity(id1));
                q.TestNumber = item.TestNumber;
                q.JobName = item.JobName;
                q.Sample = item.IdText;
                q.MasId = id;
                q.StatusMessage = "P";
                q.RecordId = id1;

                EntityManager.Transaction.Add(q);
            }

            //var phrase = (PhraseBase)EntityManager.SelectPhrase(PhraseBillStat.Identity, PhraseBillStat.PhraseIdP);
            //var jobs = data.Select(m => (object)m.JobName).Distinct().ToList();
            //var q1 = EntityManager.CreateQuery(JobHeader.EntityName);
            //q1.AddIn(JobHeaderPropertyNames.JobName, jobs);
            //var jobids = EntityManager.Select(q1).ActiveItems.Cast<JobHeader>();
            //foreach (var jobid in jobids)
            //{
            //    jobid.BillingStatus = phrase;
            //    EntityManager.Transaction.Add(jobid);
            //}
            //// update samples
            //var samples = data.Select(m => (object)m.IdText).Distinct().ToList();
            //var q2 = EntityManager.CreateQuery(Sample.EntityName);
            //q2.AddIn(SamplePropertyNames.IdText, samples);
            //var sampleids = EntityManager.Select(q2).ActiveItems.Cast<Sample>();
            //foreach (var sample in sampleids)
            //{
            //    sample.BillingStatus = phrase;
            //    EntityManager.Transaction.Add(sample);
            //}
            ////update tests
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

            EntityManager.Commit();
            //_form.ClearBusy();
            //_form.Close();
        }


        private void CreateMasCustomerHeadersAndRows()
        {
            List<MonthlyBillingViewBase> data = new List<MonthlyBillingViewBase>();

            //var data = MAScustomerCollection.ActiveItems.Cast<MonthlyBillingViewBase>().Select(n =>
            //new { n.JobName, n.LabId, n.PoNumber, n.Analysis, n.TestCount, n.IdText, n.Description, n.TestNumber })
            //    .GroupBy(c => new { c.JobName, c.LabId, c.PoNumber, c.IdText, c.TestCount ,c.Analysis,c.Description});
            //new  List<MonthlyBillingViewBase> data = new List<MonthlyBillingViewBase>();{ n.JobName, n.IdText, n.LabId, n.PoNumber, n.Analysis, n.TestCount, n.TestNumber, n.Description }).Distinct()
            //    .GroupBy(c => new { c.JobName, c.IdText, c.LabId, c.PoNumber, c.Analysis, c.TestCount, c.Description });


            if (MAScustomerCollectionCount > 3000)
            {
                int n = 0;
                n = MAScustomerCollectionCount / 3000;
                //MAScustomerCollection.
                for (int i = 0; i <= n; i++)
                {
                    data.AddRange(MAScustomerCollection.ActiveItems.Cast<MonthlyBillingViewBase>().OrderBy(m => m.TestNumber).Skip((n - i) * 3000).Take(3000).ToList());
                }
            }
            else
                data = MAScustomerCollection.ActiveItems.Cast<MonthlyBillingViewBase>().ToList();


            var data1 = data//.Select(n =>
            //new { n.JobName, n.LabId, n.PoNumber, n.Analysis, n.TestCount, n.IdText, n.Description, n.TestNumber })
            .OrderBy(o => o.IdText)
                .GroupBy(c => new { c.JobName, c.LabId, c.PoNumber, c.IdText, c.TestCount, c.Analysis, c.Description, c.TestNumber, c.SiBlId, c.SiELocation, c.SldepositNum, c.SldepositAmt, c.Netterms });

            Headers = new List<column>();

            //create header rows
            foreach (var i in data1.Select(m => new { analysis = m.Key.Analysis }).Distinct().OrderBy(x => x.analysis))
            {
                Headers.Add(new column { id = 0, name = i.analysis });
            }

            Rows = new List<row>();
            row newrow;

            // create data rows 
            foreach (var i in data1.GroupBy(o => new { o.Key.IdText }))
            {
                // var cnt = i.Count(m => m.Key.TestCount);
                //var c = data1.GroupBy(o => new { o.Key.IdText }).Count();
                //var c1 = data1.GroupBy(o => new { o.Key.IdText }).Where(m => m.Key.IdText == "BM-220131-031-001");

                if (i.Key.IdText == "BM-220131-031-001")
                { }

                newrow = new row();
                var KEY = i.FirstOrDefault().Key;
                newrow.Description = KEY.Description;
                newrow.Lab = KEY.IdText;
                newrow.Items = new List<item>();
                newrow.PO = KEY.PoNumber;
                newrow.BusinessLine = KEY.SiBlId.ToString();
                newrow.LocationId = KEY.SiELocation.ToString();
                newrow.SIDepositNum = KEY.SldepositNum.ToString();
                newrow.SIDepositAmt = KEY.SldepositAmt.ToString();
                newrow.netterms = !string.IsNullOrEmpty(KEY.Netterms) ? ((PhraseBase)EntityManager.SelectPhrase(PhraseNetterms.Identity, KEY.Netterms.ToString())).PhraseText:"";
                


                foreach (var column in Headers)
                {
                    var item = new item();

                    var col = i.Where(p => p.Key.Analysis == column.name);
                    // var count = i.Count(p => p.Key.Analysis.Contains(column.name));

                    if (col.Count() > 0)
                    {
                        item.val = col.Sum(n => n.Key.TestCount);//col.Count();//  count;// col.Sum(n => n.Key.TestCount);
                        item.column = col.FirstOrDefault().Key.Analysis;
                    }
                    else
                    {
                        item.column = column.name;
                    }
                    newrow.Items.Add(item);
                }
                if (!Rows.Any(n => n.Lab == KEY.IdText))
                    Rows.Add(newrow);
            }

            //// create data rows 
            //foreach (var i in data1)
            //{
            //    newrow = new row();
            //    newrow.Description = i.Key.Description;
            //    newrow.Lab = i.Key.IdText;
            //    newrow.Items = new List<item>();
            //    newrow.PO = i.Key.PoNumber;

            //    foreach (var column in Headers)
            //    {
            //        var rec = data1.Where(m => m.Key.Analysis == column.name && m.Key.IdText == i.Key.IdText).FirstOrDefault();
            //        var Item = new item();
            //        Item.column = column.name;
            //        if (rec != null)
            //        {
            //            Item.val = rec.Sum(b => b.TestCount); //rec.Sum(b => b.TestCount);
            //        }
            //        newrow.Items.Add(Item);
            //    }
            //    if (!Rows.Any(n => n.Lab == i.Key.IdText))
            //        Rows.Add(newrow);
            //}

        }

        private void CreateMasCustomerHeadersAndRowsJob()
        {
            var data = MAScustomerCollection.ActiveItems.Cast<MonthlyBillingViewBase>().Select(n =>
            new { n.JobName, n.LabId, n.PoNumber, n.Analysis, n.TestCount, n.TestNumber, n.SiELocation, n.SiBlId, n.SldepositNum, n.SldepositAmt,n.Netterms })
                .GroupBy(c => new { c.JobName, c.LabId, c.PoNumber, c.Analysis, c.TestCount, c.SiBlId, c.SiELocation, c.SldepositNum, c.SldepositAmt ,c.Netterms});
            // .Select(v => new { JobName = v.Key.JobName, LabId = v.Key.LabId, PoNumber = v.Key.PoNumber, Analysis = v.Key.Analysis, TestCount = v.Sum(b => b.TestCount) }).ToList();

            Headers = new List<column>();

            //create header rows
            foreach (var i in data.Select(m => new { analysis = m.Key.Analysis }).Distinct().OrderBy(x => x.analysis))
            {
                Headers.Add(new column { id = 0, name = i.analysis });
            }

            Rows = new List<row>();
            row newrow;

            // create data rows 
            foreach (var i in data)
            {

                newrow = new row();
                newrow.Description = i.Key.JobName;
                newrow.Lab = i.Key.LabId.ToString();
                newrow.PO = i.Key.PoNumber;
                newrow.BusinessLine = i.Key.SiBlId.ToString();
                newrow.LocationId = i.Key.SiELocation.ToString();
                newrow.SIDepositNum = i.Key.SldepositNum.ToString();
                newrow.SIDepositAmt = i.Key.SldepositAmt.ToString();
                newrow.netterms = !string.IsNullOrEmpty(i.Key.Netterms) ? ((PhraseBase)EntityManager.SelectPhrase(PhraseNetterms.Identity, i.Key.Netterms.ToString())).PhraseText : "";

                newrow.Items = new List<item>();

                foreach (var column in Headers)
                {
                    //var col = i.Where(p => p.Analysis == column.name);
                    var rec = data.Where(m => m.Key.Analysis == column.name && m.Key.JobName == i.Key.JobName).FirstOrDefault();
                    var Item = new item();
                    Item.column = column.name;

                    //if (col.Count() > 0)
                    //{
                    //    Item.val = col.Count();//  count;// col.Sum(n => n.Key.TestCount);
                    //    Item.column = col.FirstOrDefault().Analysis;
                    //}
                    //else
                    //{
                    //    Item.column = column.name;
                    //}
                    newrow.Items.Add(Item);
                    if (rec != null)
                    {
                        Item.val = rec.Sum(b => b.TestCount);
                    }
                    newrow.Items.Add(Item);
                }

                if (!Rows.Any(n => n.Description == i.Key.JobName))
                    Rows.Add(newrow);
            }


            //var data = MAScustomerCollection.ActiveItems.Cast<MonthlyBillingJobVBase>();
            //Headers = new List<column>();

            ////create header rows
            //foreach (var i in data.Select(m => new { analysis = m.Analysis }).Distinct())
            //{
            //    Headers.Add(new column { id = 0, name = i.analysis });
            //}

            //Rows = new List<row>();
            //row newrow;

            //// create data rows 
            //foreach (MonthlyBillingJobVBase i in data)
            //{
            //    newrow = new row();
            //    newrow.Description = i.JobName;
            //    newrow.Lab = i.LabId.ToString();
            //    newrow.PO = i.PoNumber;
            //    newrow.Items = new List<item>();


            //    foreach (var column in Headers)
            //    {
            //        var rec = data.Where(m => m.Analysis == column.name && m.JobName == i.JobName).FirstOrDefault();
            //        var Item = new item();
            //        Item.column = column.name;
            //        if (rec != null)
            //        {
            //            Item.val = rec.TestCount;
            //        }
            //        newrow.Items.Add(Item);
            //    }

            //    if (!Rows.Any(n => n.Description == i.JobName))
            //        Rows.Add(newrow);
            //}
        }


        private void ButtonEdit2_Click(object sender, EventArgs e)
        {
            //part 1 generation of XLS file 

            if (!Library.Utils.FlashMessageYesNo("This will approve the LSR and send directly to the 'Queued for Transfer' folder and transfer to finance. Do you want to continue?", ""))
                return;

            var path1 = destination + filename;

            if (Rows.Count <= 0)
                Library.Utils.FlashMessage("No Data to generate LSR ", "");

            ////check existing mas id's and disabled it 
            //if (!CheckExistingInvoiceAny())
            //{
            //    Library.Utils.FlashMessage("Please wait as there is a LSR approved and is under process", "");
            //    return;
            //}

            CheckExistingInvoiceAny();

            // create excel file 
            bool res = false;
            _form.SetBusy();
            if (_billType == "Preview By PO")
            {
                var POs = Rows.GroupBy(c => c.PO).Where(g => g.Count() > 0);
                int counter = 0;
                if (POs.Count() > 0)
                {
                    foreach (var i in POs)
                    {
                        string excelFile = getCopyExcelTemplateFile(counter);
                        res = WriteExcel(i.ToList(), Headers, excelFile);
                        //System.Diagnostics.Process.Start(path + ".csv");

                        if (res)
                        {
                            //Library.Utils.FlashMessage("File Generated succefully", "");
                            Library.File.TransferToClient(excelFile, Path.GetFileName(excelFile), false);
                        }
                        else
                        {
                            _form.ClearBusy();
                            Library.Utils.FlashMessage("Error Gernerating file", "");
                            return;
                        }
                        counter++;
                    }
                }
            }
            else
            {
                string excelFile = getCopyExcelTemplateFile(0);
                res = WriteExcel(Rows, Headers, excelFile);

                if (res)
                {
                    //Library.Utils.FlashMessage("File Generated succefully", "");
                    Library.File.TransferToClient(excelFile, Path.GetFileName(excelFile), false);
                }
                else
                {
                    _form.ClearBusy();
                    Library.Utils.FlashMessage("Error Gernerating file", "");
                    return;
                }
            }


            //*************end of part1*****************//

            //// prepare data based on bill type 
            //var MASRows = GetMASRows();
            var res1 = false;

            var billtype = _billType;
            List<masrow> MASRows = new List<masrow>();
            var data = MAScustomerCollection.ActiveItems.Cast<MonthlyBillingViewBase>().Select(n =>
                            new { n.JobName, n.IdText, n.LabId, n.PoNumber, n.Analysis, n.TestCount, n.TestNumber, n.Description, n.CustomerId, n.LabId.SiELocation, n.SiBlId, n.SldepositAmt, n.SldepositNum ,n.Netterms}).Distinct();
            // set path
            var path = destination + filename;
            string PONumber = string.Empty;
            switch (_billType)
            {
                case "Preview By Sample":
                    //var q = EntityManager.CreateQuery(MonthlyBillingViewBase.EntityName);
                    //q.AddEquals(MonthlyBillingViewPropertyNames.CustomerId, customerid);
                    ////q.AddGreaterThan(MonthlyBillingViewPropertyNames.DateReceived, _startdate.ToUniversalTime());
                    //q.AddLessThan(MonthlyBillingViewPropertyNames.DateReceived, _enddate.ToUniversalTime());
                    ////q.AddEquals(MasBillingViewPropertyNames.LabId, DeibelLabCustomer.LabCode);
                    //MASbillCollection = EntityManager.Select(q);

                    //var data = MAScustomerCollection.ActiveItems.Cast<MonthlyBillingViewBase>().Select(n =>
                    //         new { n.JobName, n.IdText, n.LabId, n.PoNumber, n.Analysis, n.TestCount, n.TestNumber, n.Description, n.CustomerId }).Distinct();
                    //MASbillCollection = data.GroupBy(c => new { c.JobName, c.IdText, c.LabId, c.PoNumber, c.Analysis, c.CustomerId })
                    //  .Select(m => new data { JobName = m.Key.JobName, IdText = m.Key.IdText, LabId = m.Key.LabId.Name, PoNumber = m.Key.PoNumber, Analysis = m.Key.Analysis, CustomerId = m.Key.CustomerId.Name});

                    //List<MonthlyBillingViewBase> MASbillingList = MAScustomerCollection.ActiveItems.Cast<MonthlyBillingViewBase>().ToList();


                    if (data.GroupBy(j => j.PoNumber).Count() > 1)
                        PONumber = "See Lab Report Summary";
                    else
                        PONumber = data.FirstOrDefault().PoNumber;
                    MASRows = data.GroupBy(p => new { p.Analysis, p.CustomerId, p.TestCount, p.SiBlId, p.SiELocation, p.SldepositAmt, p.SldepositNum ,p.Netterms}).
                    //MASRows = MASbillCollection.GroupBy(p => new { p.PoNumber, p.Analysis, p.CustomerId }).
                    Select(q => new masrow
                    {
                        CustNo = q.Key.CustomerId.CustomerName,
                        ColName = q.Key.Analysis,
                        PO = PONumber,
                        BusinessLine = q.Key.SiBlId.ToString(),
                        LocationId = q.Key.SiELocation.ToString(),
                        SIDepositAmt=q.Key.SldepositAmt.ToString(),
                        SISIDepositNum=q.Key.SldepositNum,
                        Netterms = !string.IsNullOrEmpty(q.Key.Netterms) ? ((PhraseBase)EntityManager.SelectPhrase(PhraseNetterms.Identity, q.Key.Netterms.ToString())).PhraseText : "",
                        
                        Totals = q.Sum(t => (int)t.TestCount).ToString()
                    }).ToList();

                    if (MASRows.Count <= 0)
                        return;
                    //res1 = DownloadReport(MASRows, path, 0);
                    int counter = 0;

                    if (MASRows.Any(p => string.IsNullOrEmpty(p.SISIDepositNum)))
                    {
                        res1 = DownloadReport(MASRows.Where(p => string.IsNullOrEmpty(p.SISIDepositNum)).ToList(), path, counter);
                        counter++;
                    }

                    if (MASRows.Any(p => !string.IsNullOrEmpty(p.SISIDepositNum)))
                    {
                        res1 = DownloadReport(MASRows.Where(p => !string.IsNullOrEmpty(p.SISIDepositNum)).ToList(), path, counter);
                    }
                    break;

                case "Preview By Job":

                    //var l = EntityManager.CreateQuery(MasBillingJobVBase.EntityName);
                    //l.AddEquals(MasBillingJobVPropertyNames.CustomerId, customerid);
                    //l.AddLessThan(MasBillingJobVPropertyNames.DateReceived, _enddate.ToUniversalTime());
                    //q.AddGreaterThan(MonthlyBillingViewPropertyNames.DateReceived, _startdate.ToUniversalTime());
                    //l.AddEquals(MasBillingJobVPropertyNames.LabId, DeibelLabCustomer.LabCode);
                    //MASbillCollection = EntityManager.Select(l);
                    //MASRows = MASbillCollection.ActiveItems.Cast<MasBillingJobVBase>().GroupBy(p => new { p.PoNumber, p.Analysis, p.CustomerId }).
                    //string PONumber = string.Empty;
                    if (data.Select(t => t.PoNumber).Count() > 1)
                        PONumber = "See Lab Report Summary";
                    else
                        PONumber = data.FirstOrDefault().PoNumber;
                    //var POnumber = data.FirstOrDefault().PoNumber;
                    MASRows = data.GroupBy(p => new { p.Analysis, p.CustomerId, p.TestCount, p.SiELocation, p.SiBlId, p.SldepositAmt, p.SldepositNum,p.Netterms }).
                    Select(b => new masrow
                    {
                        CustNo = customerid,
                        Division = customerid,
                        ColName = b.Key.Analysis,
                        PO = PONumber,
                        BusinessLine = b.Key.SiBlId.ToString(),
                        LocationId = b.Key.SiELocation.ToString(),
                        SISIDepositNum = b.Key.SldepositNum,
                        SIDepositAmt = b.Key.SldepositAmt.ToString(),
                        Netterms = b.Key.Netterms != null ? ((PhraseBase)EntityManager.SelectPhrase(PhraseNetterms.Identity, b.Key.Netterms.ToString())).PhraseText : "",
                        
                        Totals = b.Sum(t => (int)t.TestCount).ToString() //b.Count().ToString()
                    }).ToList();

                    if (MASRows.Count <= 0)
                        return;

                    int counter1 = 0;

                    if (MASRows.Any(p => string.IsNullOrEmpty(p.SISIDepositNum)))
                    {
                        res1 = DownloadReport(MASRows.Where(p => string.IsNullOrEmpty(p.SISIDepositNum)).ToList(), path, counter1);
                        counter1++;
                    }

                    if (MASRows.Any(p => !string.IsNullOrEmpty(p.SISIDepositNum)))
                    {
                        res1 = DownloadReport(MASRows.Where(p => !string.IsNullOrEmpty(p.SISIDepositNum)).ToList(), path, counter1);
                    }

                    break;

                case "Preview By PO":

                    //var k = EntityManager.CreateQuery(MasBillingViewBase.EntityName);
                    //k.AddEquals(MasBillingViewPropertyNames.CustomerId, customerid);
                    //k.AddLessThan(MasBillingViewPropertyNames.DateReceived, _enddate.ToUniversalTime());
                    //k.AddEquals(MasBillingViewPropertyNames.LabId, DeibelLabCustomer.LabCode
                    //q.AddGreaterThan(MonthlyBillingViewPropertyNames.DateReceived, _startdate.ToUniversalTime());
                    //MASbillCollection = EntityManager.Select(k);
                    //MASRows = MASbillCollection.ActiveItems.Cast<MasBillingJobVBase>().GroupBy(p => new { p.PoNumber, p.Analysis, p.CustomerId }).
                    MASRows = data.GroupBy(p => new { p.PoNumber, p.Analysis, p.CustomerId, p.TestCount, p.SiELocation, p.SiBlId, p.SldepositAmt,p.SldepositNum,p.Netterms }).
                    Select(b => new masrow
                    {
                        CustNo = customerid,
                        Division = customerid,
                        ColName = b.Key.Analysis,
                        //, InvDate = b.Key.DateReceived.Value.ToShortDateString(),
                        PO = b.Key.PoNumber,
                        BusinessLine = b.Key.SiBlId.ToString(),
                        LocationId = b.Key.SiELocation.ToString(),
                        SIDepositAmt=b.Key.SldepositAmt.ToString(),
                        SISIDepositNum=b.Key.SldepositNum,
                        Netterms = !string.IsNullOrEmpty(b.Key.Netterms) ? ((PhraseBase)EntityManager.SelectPhrase(PhraseNetterms.Identity, b.Key.Netterms.ToString())).PhraseText : "",
                        
                        Totals = b.Count().ToString()
                    }).ToList();

                    if (MASRows.Count <= 0) 
                        return;
                    int counter2 = 0;
                    //foreach (var x in MASRows.GroupBy(x => x.PO))
                    //{
                    //    res1 = DownloadReport(x.ToList(), path, counter);
                    //    counter++;
                    //}
                    if(MASRows.Any(p => string.IsNullOrEmpty(p.SISIDepositNum)))
                    foreach (var x in MASRows.Where(p => string.IsNullOrEmpty(p.SISIDepositNum)).GroupBy(x => x.PO))
                    {
                        res1 = DownloadReport(x.ToList(), path, counter2);
                        counter2++;
                    }

                    if (MASRows.Any(p => !string.IsNullOrEmpty(p.SISIDepositNum)))
                        foreach (var x in MASRows.Where(p => !string.IsNullOrEmpty(p.SISIDepositNum)).GroupBy(x => x.PO))
                    {
                        res1 = DownloadReport(x.ToList(), path, counter2);
                        counter2++;
                    }
                    break;
            }





            //create csv file based on bill type
            //if (_billType == "Preview By PO")
            //{
            //    var POs = MASRows.GroupBy(x => x.PO).Where(g => g.Count() > 0);
            //    int counter = 1;

            //    foreach (var i in POs)
            //    {
            //        res = DownloadReport(i.ToList(), path + "-" + counter);
            //        //System.Diagnostics.Process.Start(path + ".csv");
            //        counter++;

            //        if (res == false)
            //            return;
            //    }
            //}
            //else
            //{

            //System.Diagnostics.Process.Start(path + ".csv");
            //}
            //Library.Utils.FlashMessage("File Generated succefully", "");

            if (res1)
            {
                if (_billType == "Preview By PO")
                {
                    int counter = 0;
                    var POs = Rows.GroupBy(c => c.PO).Where(g => g.Count() > 0);
                    if (POs.Count() > 0)
                    {
                        foreach (var i in POs)
                        {
                            string excelFile = getCopyExcelTemplateFileNoCopy(counter);
                            UpdateInvoiceBilledPO(i.FirstOrDefault().PO, excelFile);
                            counter++;
                        }
                    }
                    //Library.Utils.FlashMessage("Data send to queue for processing", "");
                }
                else
                    UpdateInvoiceBilled();


                //var backgroundWorker = new System.ComponentModel.BackgroundWorker();
                //backgroundWorker.DoWork += (o, args) => UpdateInvoiceBilled();
                //backgroundWorker.RunWorkerAsync();
                //  backgroundWorker.RunWorkerCompleted += (o, args) => _form.HighlightButton.Enabled = true;
                _form.ClearBusy();
                //Library.Utils.FlashMessage("File Generated succefully and Data send to queue for processing", "");
            }
            _form.Close();
        }


        private void UpdateInvoiceBilledPO(string PONumber, string excelFile)
        {
            var path = destination + filename;
            var data = MAScustomerCollection.ActiveItems.Cast<MonthlyBillingViewBase>().Select(n =>
                           new { n.JobName, n.IdText, n.LabId, n.PoNumber, n.Analysis, n.TestCount, n.TestNumber, n.Description, n.CustomerId }).Where(m => m.PoNumber == PONumber || string.IsNullOrEmpty(PONumber)).Distinct();


            //update mas_billing
            var id = new PackedDecimal(Library.Increment.GetIncrement(TableNames.MasBilling, MasBillingPropertyNames.RecordId).ToString());
            MasBillingBase record = (MasBillingBase)EntityManager.CreateEntity(MasBillingBase.EntityName, new Identity(id));
            var currentUser = (Personnel)Library.Environment.CurrentUser;
            //record.RecordId  = Library.Increment.GetIncrement(TableNames.MasBilling, MasBillingPropertyNames.RecordId);
            record.DateCreated = DateTime.Now;
            record.CreatedBy = currentUser;
            record.CustomerId = DeibelLabCustomer.Deibellabcustomer;
            record.LabCode = DeibelLabCustomer.LabCode;
            record.FileName = Path.GetFileNameWithoutExtension(excelFile);
            record.XlFileName = excelFile;// path + ".xlsx";
            record.StartDate = _startdate;
            record.EndDate = _enddate;
            //record.ApprovalStatus = apprphrase;
            record.Mode = "PO";
            record.Status = "T";
            record.Tries = 1;
            record.StatusMessage = "Queued for Transfer";// "Transfer Pending";Queued for Tranfer
            record.GroupId = ((Personnel)Library.Environment.CurrentUser).GroupId.GroupId;
            EntityManager.Transaction.Add(record);

            foreach (var item in data)
            {
                var id1 = new PackedDecimal(Library.Increment.GetIncrement(TableNames.MasBillingDetails, MasBillingDetailsPropertyNames.RecordId).ToString());
                MasBillingDetailsBase q = (MasBillingDetailsBase)EntityManager.CreateEntity(MasBillingDetailsBase.EntityName, new Identity(id1));
                q.TestNumber = item.TestNumber;
                q.JobName = item.JobName;
                q.Sample = item.IdText;
                q.MasId = id;
                q.StatusMessage = "B";
                q.RecordId = id1;

                EntityManager.Transaction.Add(q);
            }

            EntityManager.Commit();
        }

        private void UpdateInvoiceBilled()
        {
            // Create entry for  mas_blilling 

            //_form.SetBusy();
            //var apprphrase = (PhraseBase)EntityManager.SelectPhrase(PhraseApprStat.Identity, PhraseApprStat.PhraseIdA);
            var path = destination + filename;
            var data = MAScustomerCollection.ActiveItems.Cast<MonthlyBillingViewBase>().Select(n =>
                           new { n.JobName, n.IdText, n.LabId, n.PoNumber, n.Analysis, n.TestCount, n.TestNumber, n.Description, n.CustomerId }).Distinct();


            //update mas_billing
            var id = new PackedDecimal(Library.Increment.GetIncrement(TableNames.MasBilling, MasBillingPropertyNames.RecordId).ToString());
            MasBillingBase record = (MasBillingBase)EntityManager.CreateEntity(MasBillingBase.EntityName, new Identity(id));
            var currentUser = (Personnel)Library.Environment.CurrentUser;
            //record.RecordId  = Library.Increment.GetIncrement(TableNames.MasBilling, MasBillingPropertyNames.RecordId);
            record.DateCreated = DateTime.Now;
            record.CreatedBy = currentUser;
            record.CustomerId = DeibelLabCustomer.Deibellabcustomer;
            record.LabCode = DeibelLabCustomer.LabCode;
            record.FileName = Path.GetFileNameWithoutExtension(path + ".xlsx");
            record.XlFileName = path + ".xlsx";
            record.StartDate = _startdate;
            record.EndDate = _enddate;
            //record.ApprovalStatus = apprphrase;

            if (_billType == "Preview By Sample")
            {
                record.Mode = "SINGLE";
            }
            else if (_billType == "Preview By Job")
            {
                record.Mode = "JOB";
            }
            else
            {
                record.Mode = "PO";
            }

            record.Status = "T";
            record.Tries = 1;
            record.StatusMessage = "Queued for Transfer";// "Transfer Pending";Queued for Tranfer
            record.GroupId = ((Personnel)Library.Environment.CurrentUser).GroupId.GroupId;
            EntityManager.Transaction.Add(record);

            foreach (var item in data)
            {
                var id1 = new PackedDecimal(Library.Increment.GetIncrement(TableNames.MasBillingDetails, MasBillingDetailsPropertyNames.RecordId).ToString());
                MasBillingDetailsBase q = (MasBillingDetailsBase)EntityManager.CreateEntity(MasBillingDetailsBase.EntityName, new Identity(id1));
                q.TestNumber = item.TestNumber;
                q.JobName = item.JobName;
                q.Sample = item.IdText;
                q.MasId = id;
                q.StatusMessage = "B";
                q.RecordId = id1;

                EntityManager.Transaction.Add(q);
            }




            //if (_billType == "Preview By Sample" || _billType == "Preview By PO")
            //{
            //List<MonthlyBillingViewBase> MASbillingList = MAScustomerCollection.ActiveItems.Cast<MonthlyBillingViewBase>().ToList();
            //update Jobs
            //var jobs = MASbillingList.Select(m => (object)m.JobName).Distinct().ToList();

            //var phrase = (PhraseBase)EntityManager.SelectPhrase(PhraseBillStat.Identity, PhraseBillStat.PhraseIdB);
            //var jobs = data.Select(m => (object)m.JobName).Distinct().ToList();
            //var q1 = EntityManager.CreateQuery(JobHeader.EntityName);
            //q1.AddIn(JobHeaderPropertyNames.JobName, jobs);
            //var jobids = EntityManager.Select(q1).ActiveItems.Cast<JobHeader>();
            //foreach (var jobid in jobids)
            //{
            //    jobid.BillingStatus = phrase;
            //    EntityManager.Transaction.Add(jobid);
            //}
            //// update samples
            //var samples = data.Select(m => (object)m.IdText).Distinct().ToList();
            //var q2 = EntityManager.CreateQuery(Sample.EntityName);
            //q2.AddIn(SamplePropertyNames.IdText, samples);
            //var sampleids = EntityManager.Select(q2).ActiveItems.Cast<Sample>();
            //foreach (var sample in sampleids)
            //{
            //    sample.BillingStatus = phrase;
            //    EntityManager.Transaction.Add(sample);
            //}
            ////update tests
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

            //foreach (Sample sample in job.SamplesToDo)
            //{
            //    for (int i = 0; i < JobAuditTempBase.)
            //        var q2 = EntityManager.CreateQuery(Sample.EntityName);
            //    q2.AddEquals(SamplePropertyNames.IdText, item.);
            //    JobHeader job = EntityManager.Select(q1).ActiveItems.Cast<JobHeader>().FirstOrDefault();
            //    job.BillingStatus = (PhraseBase)EntityManager.SelectPhrase(PhraseBillStat.Identity, PhraseBillStat.PhraseIdB);
            //    sample.BillingStatus = (PhraseBase)EntityManager.SelectPhrase(PhraseBillStat.Identity, PhraseBillStat.PhraseIdB);
            //    foreach (Test test in sample.Tests)
            //    {
            //        test.BillingStatus = (PhraseBase)EntityManager.SelectPhrase(PhraseBillStat.Identity, PhraseBillStat.PhraseIdB);
            //    }
            //}
            //EntityManager.Transaction.Add(job);

            //}
            //else if (_billType == "Preview By Job")
            //{
            //    List<MasBillingJobVBase> MASbillingList = MASbillCollection.ActiveItems.Cast<MasBillingJobVBase>().ToList();
            //    //update Jobs
            //    var jobs = MASbillingList.Select(m => (object)m.JobName).Distinct().ToList();
            //    var q1 = EntityManager.CreateQuery(JobHeader.EntityName);
            //    q1.AddIn(JobHeaderPropertyNames.JobName, jobs);
            //    var jobids = EntityManager.Select(q1).ActiveItems.Cast<JobHeader>();
            //    foreach (var jobid in jobids)
            //    {
            //        jobid.BillingStatus = (PhraseBase)EntityManager.SelectPhrase(PhraseBillStat.Identity, PhraseBillStat.PhraseIdB);
            //        EntityManager.Transaction.Add(jobid);
            //    }
            //    // update samples
            //    var samples = MASbillingList.Select(m => (object)m.IdText).Distinct().ToList();
            //    var q2 = EntityManager.CreateQuery(Sample.EntityName);
            //    q2.AddIn(SamplePropertyNames.IdText, samples);
            //    var sampleids = EntityManager.Select(q2).ActiveItems.Cast<Sample>();
            //    foreach (var sample in sampleids)
            //    {
            //        sample.BillingStatus = (PhraseBase)EntityManager.SelectPhrase(PhraseBillStat.Identity, PhraseBillStat.PhraseIdB);
            //        EntityManager.Transaction.Add(sample);
            //    }
            //    // update tests
            //    var tests = MASbillingList.Select(m => (object)m.TestNumber).Distinct().ToList();
            //    var q3 = EntityManager.CreateQuery(Test.EntityName);
            //    q3.AddIn(TestPropertyNames.TestNumber, tests);
            //    var testids = EntityManager.Select(q3).ActiveItems.Cast<Test>();
            //    foreach (var test in testids)
            //    {
            //        test.BillingStatus = (PhraseBase)EntityManager.SelectPhrase(PhraseBillStat.Identity, PhraseBillStat.PhraseIdB);
            //        EntityManager.Transaction.Add(test);
            //    }

            //    //    //foreach (var item in MAScustomerCollection.Cast<MonthlyBillingJobVBase>().Select(m => m.JobName).Distinct())
            //    //    //{
            //    //    //    var q1 = EntityManager.CreateQuery(JobHeader.EntityName);
            //    //    //    q1.AddEquals(JobHeaderPropertyNames.JobName, item);
            //    //    //    JobHeader job = EntityManager.Select(q1).ActiveItems.Cast<JobHeader>().FirstOrDefault();
            //    //    //    job.BillingStatus = (PhraseBase)EntityManager.SelectPhrase(PhraseBillStat.Identity, PhraseBillStat.PhraseIdB);
            //    //    //    foreach (Sample sample in job.Samples)
            //    //    //    {
            //    //    //        sample.BillingStatus = (PhraseBase)EntityManager.SelectPhrase(PhraseBillStat.Identity, PhraseBillStat.PhraseIdB);
            //    //    //        foreach (Test test in sample.Tests)
            //    //    //        {
            //    //    //            test.BillingStatus = (PhraseBase)EntityManager.SelectPhrase(PhraseBillStat.Identity, PhraseBillStat.PhraseIdB);
            //    //    //        }
            //    //    //    }
            //    //    //    EntityManager.Transaction.Add(job);
            //    //    //}
            //}



            EntityManager.Commit();
            //_form.ClearBusy();

        }


        private List<masrow> GetMASRows()
        {
            var billtype = _billType;
            List<masrow> MASRows = new List<masrow>();

            switch (_billType)
            {
                case "Preview By Sample":
                    //var q = EntityManager.CreateQuery(MonthlyBillingViewBase.EntityName);
                    //q.AddEquals(MonthlyBillingViewPropertyNames.CustomerId, customerid);
                    ////q.AddGreaterThan(MonthlyBillingViewPropertyNames.DateReceived, _startdate.ToUniversalTime());
                    //q.AddLessThan(MonthlyBillingViewPropertyNames.DateReceived, _enddate.ToUniversalTime());
                    ////q.AddEquals(MasBillingViewPropertyNames.LabId, DeibelLabCustomer.LabCode);
                    //MASbillCollection = EntityManager.Select(q);

                    var data = MAScustomerCollection.ActiveItems.Cast<MonthlyBillingViewBase>().Select(n =>
                    new { n.JobName, n.IdText, n.LabId, n.PoNumber, n.Analysis, n.TestCount, n.TestNumber }).Distinct()
                     .GroupBy(c => new { c.JobName, c.LabId, c.PoNumber, c.Analysis, c.TestCount });

                    //List<MonthlyBillingViewBase> MASbillingList = MAScustomerCollection.ActiveItems.Cast<MonthlyBillingViewBase>().ToList();
                    //MASRows = MASbillingList.GroupBy(p => new { p.PoNumber, p.Analysis, p.CustomerId, p.TestCount }).
                    data.
                    Select(q => new masrow
                    {
                        //CustNo = q.Key.CustomerId.CustomerName,
                        ColName = q.Key.Analysis,
                        PO = q.Key.PoNumber,
                        Totals = q.Sum(t => (int)t.TestCount).ToString()
                    }).ToList();

                    break;

                case "Preview By Job":

                    var l = EntityManager.CreateQuery(MasBillingJobVBase.EntityName);
                    l.AddEquals(MasBillingJobVPropertyNames.CustomerId, customerid);
                    //q.AddGreaterThan(MonthlyBillingViewPropertyNames.DateReceived, _startdate.ToUniversalTime());
                    l.AddLessThan(MasBillingJobVPropertyNames.DateReceived, _enddate.ToUniversalTime());
                    //l.AddEquals(MasBillingJobVPropertyNames.LabId, DeibelLabCustomer.LabCode);
                    //MASbillCollection = EntityManager.Select(l);
                    //MASRows = MASbillCollection.ActiveItems.Cast<MasBillingJobVBase>().GroupBy(p => new { p.PoNumber, p.Analysis, p.CustomerId }).
                    //Select(b => new masrow
                    //{
                    //    CustNo = customerid,
                    //    Division = customerid,
                    //    ColName = b.Key.Analysis,
                    //    PO = b.Key.PoNumber,
                    //    Totals = b.Count().ToString()
                    //}).ToList();
                    break;

                case "Preview By PO":

                    var k = EntityManager.CreateQuery(MasBillingViewBase.EntityName);
                    k.AddEquals(MasBillingViewPropertyNames.CustomerId, customerid);
                    //q.AddGreaterThan(MonthlyBillingViewPropertyNames.DateReceived, _startdate.ToUniversalTime());
                    k.AddLessThan(MasBillingViewPropertyNames.DateReceived, _enddate.ToUniversalTime());
                    //k.AddEquals(MasBillingViewPropertyNames.LabId, DeibelLabCustomer.LabCode);
                    //MASbillCollection = EntityManager.Select(k);
                    //MASRows = MASbillCollection.ActiveItems.Cast<MasBillingJobVBase>().GroupBy(p => new { p.PoNumber, p.Analysis, p.CustomerId }).
                    //Select(b => new masrow
                    //{
                    //    CustNo = customerid,
                    //    Division = customerid,
                    //    ColName = b.Key.Analysis,
                    //    //, InvDate = b.Key.DateReceived.Value.ToShortDateString(),
                    //    PO = b.Key.PoNumber,
                    //    Totals = b.Count().ToString()
                    //}).ToList();
                    break;

            }
            return MASRows;
        }

        public bool DownloadReport(List<masrow> lstData, string path, int counter)
        {



            var sb = new StringBuilder();
            foreach (var data in lstData.OrderBy(x => x.ColName))
            {
                var cust1 = !string.IsNullOrEmpty(DeibelLabCustomer.MasCustomer) ? DeibelLabCustomer.MasCustomer.Split('-')[0].ToString() : "";
                var cust2 = !string.IsNullOrEmpty(DeibelLabCustomer.MasCustomer) ? DeibelLabCustomer.MasCustomer.Split('-').Length == 1 ? DeibelLabCustomer.MasCustomer.Split('-')[1] : "" : "";

                sb.AppendLine(_enddate.ToString("yyyyMMdd") + "," +
                    //DeibelLabCustomer.MasCustomer.Split('-')[0] + "," +
                    //DeibelLabCustomer.MasCustomer.Split('-')[1] + "," +
                    cust1 + "," +
                    cust2 + "," +
                    data.PO + "," +
                    data.BusinessLine + "," +
                    data.LocationId + "," +
                    DeibelLabCustomer.Deibellabcustomer.SiCustomerid + "," +
                    ((Personnel)Library.Environment.CurrentUser).SiDeptId + "," +
                    data.ColName + "," +
                    data.Totals + "," +
                     DeibelLabCustomer.Deibellabcustomer.Currency.PhraseText + "," +
                    DeibelLabCustomer.Deibellabcustomer.CurrencyExchange + "," +
                     DeibelLabCustomer.Deibellabcustomer.CurrencyExchType + "," +
                     data.Netterms.ToString()+","+
                     data.SISIDepositNum + "," +
                     data.SIDepositAmt
                    );
            }
            if (counter > 0)
                File.WriteAllText(path + "-" + counter + ".csv", sb.ToString(), Encoding.UTF8);
            else
                File.WriteAllText(path + ".csv", sb.ToString(), Encoding.UTF8);
            return true;
        }


        private string getCopyExcelTemplateFile(int counter)
        {
            string
                newFile = filename,
                rootPath = destination,
                templateFile = templateFilePath;
            string tempFile = "";
            if (counter > 0)
                tempFile = destination + newFile + "-" + counter + ".xlsx";
            else
                tempFile = destination + newFile + ".xlsx";

            File.Copy(templateFile, tempFile, true);
            return tempFile;
        }

        private string getCopyExcelTemplateFileNoCopy(int counter)
        {
            string
                newFile = filename,
                rootPath = destination,
                templateFile = templateFilePath;
            string tempFile = "";
            if (counter > 0)
                tempFile = destination + newFile + "-" + counter + ".xlsx";
            else
                tempFile = destination + newFile + ".xlsx";

            //File.Copy(templateFile, tempFile, true);
            return tempFile;
        }


        private bool WriteExcel(List<row> Rows, List<column> Headers, string excelFile)
        {
            //using EP Plus library and set license context
            var template = new FileInfo(excelFile);
            FileInfo fileInfo = new FileInfo(excelFile);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage p = new ExcelPackage(fileInfo))
            {
                try
                {

                    //ExcelPackage p = new ExcelPackage(fileInfo);
                    ExcelWorksheet myWorksheet = p.Workbook.Worksheets["Report"];

                    // create headers rows
                    myWorksheet.Cells[2, 3].Value = "PO";
                    myWorksheet.Cells[2, 4].Value = "LAB NO";
                    myWorksheet.Cells[2, 5].Value = "Description";

                    for (int i = 0; i < Headers.Count; i++)
                    {
                        myWorksheet.Cells[2, i + 6].Value = Headers[i].name;
                    }
                    // create data rows 
                    for (int j = 0; j < Rows.Count; j++)
                    {
                        if (!string.IsNullOrEmpty(Rows[j].PO))
                            myWorksheet.Cells[j + 3, 3].Value = Rows[j].PO;
                        else
                            myWorksheet.Cells[j + 3, 3].Value = "'";
                        myWorksheet.Cells[j + 3, 4].Value = Rows[j].Lab;
                        myWorksheet.Cells[j + 3, 5].Value = "'" + Rows[j].Description;
                        for (int k = 0; k < Rows[j].Items.Count; k++)
                        {
                            if (Rows[j].Items[k].val != 0)
                                myWorksheet.Cells[j + 3, k + 6].Value = Rows[j].Items[k].val;
                            else
                                myWorksheet.Cells[j + 3, k + 6].Value = "";
                        }
                    }
                    //create footer rows 
                    myWorksheet.Cells[Rows.Count + 3, 5].Value = "Totals";
                    int q = 0;
                    for (int i = 0; i < Headers.Count; i++)
                    {
                        int sum = 0;
                        sum = Rows.Select(n => n.Items[i].val).Sum();

                        if (sum == 0)
                        {
                            myWorksheet.DeleteColumn((i + 6) - q);
                            q++;
                        }
                        else
                            myWorksheet.Cells[Rows.Count + 3, (i + 6) - q].Value = sum;

                    }

                    // bold the footer row
                    myWorksheet.Cells[Rows.Count + 3, 2, Rows.Count + 3, Headers.Count + 5].Style.Font.Bold = true;

                    //myWorksheet.PrinterSettings.FitToPage = true;
                    //string columnName = OfficeOpenXml.ExcelCellAddress.GetColumnLetter(myWorksheet.Dimension.End.Column);
                    //string printArea = string.Format("A1:{0}{1}", columnName, myWorksheet.Dimension.End.Row);

                    // set the print area for excel
                    string from = myWorksheet.Cells[1, 2].Address;
                    string to = myWorksheet.Cells[Rows.Count + 3, Headers.Count + 5].Address;
                    myWorksheet.PrinterSettings.PrintArea = myWorksheet.Cells[from + ":" + to];

                    // set the image in the header of excel
                    Image img = Image.FromFile(ImageFile);
                    myWorksheet.HeaderFooter.OddHeader.InsertPicture(img, PictureAlignment.Centered);
                    // set the header text of excel
                    // set the header text of excel
                    //if (string.IsNullOrEmpty(DeibelLabCustomer.MasCustomer.Split('-')[0]) || string.IsNullOrEmpty(DeibelLabCustomer.MasCustomer.Split('-')[1]))
                    //{
                    //    Library.Utils.FlashMessage("Diebel Customer data has some incomplete information", "");
                    //    var currentUser = ((Personnel)Library.Environment.CurrentUser);
                    //    email_send(currentUser.Email, DeibelLabCustomer.CustomerId, DeibelLabCustomer.GroupId.GroupId);
                    //    return false;
                    //}
                    if (string.IsNullOrEmpty(DeibelLabCustomer.Deibellabcustomer.SiCustomerid))
                    {
                        Library.Utils.FlashMessage("Customer data has some incomplete information", "");
                        var currentUser = ((Personnel)Library.Environment.CurrentUser);
                        email_send(currentUser.Email, DeibelLabCustomer.CustomerId, DeibelLabCustomer.GroupId.GroupId);
                        return false;
                    }

                    myWorksheet.HeaderFooter.OddHeader.CenteredText += string.Format("\n\n&10&\"Arial,Bold\" Lab Report Summary" +
                    // "\n&10&\"Arial,Regular\" {0} - {1}-{2}", DeibelLabCustomer.Deibellabcustomer.CompanyName, DeibelLabCustomer.MasCustomer.Split('-')[0], DeibelLabCustomer.CustomerId) ;         // DeibelLabCustomer.MasCustomer.Split('-')[1]);
                    "\n&10&\"Arial,Regular\" {0}  - {1}", DeibelLabCustomer.Deibellabcustomer.CompanyName, DeibelLabCustomer.CustomerId);         // DeibelLabCustomer.MasCustomer.Split('-')[1]);
                    myWorksheet.HeaderFooter.EvenHeader.InsertPicture(img, PictureAlignment.Centered);
                    myWorksheet.HeaderFooter.EvenHeader.CenteredText += string.Format("\n\n&10&\"Arial,Bold\" Lab Report Summary" +
                    //     "\n&10&\"Arial,Regular\" {0} - {1}-{2}", DeibelLabCustomer.Deibellabcustomer.CompanyName, DeibelLabCustomer.MasCustomer.Split('-')[0], DeibelLabCustomer.CustomerId ) ;                           //  DeibelLabCustomer.MasCustomer.Split('-')[1]);
                    "\n&10&\"Arial,Regular\" {0} - {1}", DeibelLabCustomer.Deibellabcustomer.CompanyName, DeibelLabCustomer.CustomerId);

                    // set the page number settings for all pages in print
                    myWorksheet.HeaderFooter.differentOddEven = false;

                    //    header_txt= "&" : ASCII(34) : "Arial,Bold" :
                    //           ASCII(34) : "Lab Report Summary&" : ASCII(34) :
                    //           "Arial,Regular" : ASCII(34)
                    //    full_header = "&G " : Chr(10) :  Chr(10) : header_txt: Chr(10) : 
                    //      self.customer_name : Chr(10) : bill_month

                    //&D Current date
                    //&T Current time
                    //&F Workbook name
                    //&A Worksheet name (from the worksheet tab)
                    //& P Current page number
                    //& P + x Current page number plus x
                    //& P - x Current page number minus x                
                    //& N Total pages in the workbook
                    //&& Ampersand character

                    //myWorksheet.PrinterSettings.PrintArea = myWorksheet.Cells["A:1,G:30"];

                    // save excel package
                    p.Save();

                }
                catch (Exception e)
                {
                    return false;
                }
            }

            //****** using open xml
            //using (var spreadSheetDocument = SpreadsheetDocument.Open(excelFile, true))
            //{
            //    //WorkbookPart workbookPart = spreadSheetDocument.WorkbookPart;
            //    //Sheet sheet = spreadSheetDocument.WorkbookPart.Workbook.Sheets.GetFirstChild<Sheet>();
            //    //DocumentFormat.OpenXml.Spreadsheet.Worksheet worksheet = (spreadSheetDocument.WorkbookPart.GetPartById(sheet.Id.Value) as WorksheetPart).Worksheet;
            //    //string relId = workbookPart.Workbook.Descendants<Sheet>().First(s => sheetName.Equals(s.Name)).Id;
            //    //return (WorksheetPart)workbookPart.GetPartById(relId);

            //    //var workSheet = ((WorksheetPart)workbookPart.GetPartById(sheetID)).Worksheet;

            //    WorkbookPart workbookPart1 = spreadSheetDocument.WorkbookPart;
            //    IEnumerable<Sheet> Sheets = spreadSheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>().Where(s => s.Name == "Report");
            //    if (Sheets.Count() == 0)
            //    {
            //        // The specified worksheet does not exist.
            //        return false;
            //    }

            //    string relationshipId = Sheets.First().Id.Value;
            //    WorksheetPart worksheetPart = (WorksheetPart)spreadSheetDocument.WorkbookPart.GetPartById(relationshipId);
            //    SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
            //    var sharedstringtablepart = workbookPart1.SharedStringTablePart;

            //    // SheetData sheetdata = worksheet.GetFirstChild<SheetData>();
            //    ExportData2(Rows, "", sheetData);
            //    // ExportData2(Rows, "", sheetData);
            //    spreadSheetDocument.WorkbookPart.Workbook.Save();
            //    spreadSheetDocument.Close();
            //}

            return true;
        }

        private bool CheckExistingInvoiceAny()
        {
            var billtype = _billType;
            //_deibelLabCustomer.
            var currentUser = ((Personnel)Library.Environment.CurrentUser);
            var labid = currentUser.DefaultGroup.GroupId;
            var apprphrase = (PhraseBase)EntityManager.SelectPhrase(PhraseApprStat.Identity, PhraseApprStat.PhraseIdR);

            var q = EntityManager.CreateQuery(MasBillingBase.EntityName);
            q.AddEquals(MasBillingPropertyNames.CustomerId, customerid);
            q.AddEquals(MasBillingPropertyNames.GroupId, labid);
            //q.AddLessThanOrEquals(MasBillingPropertyNames.GroupId, _enddate);
            String[] strAr1 = new String[] { "A", "P" };
            q.AddIn(MasBillingPropertyNames.ApprovalStatus, strAr1.Cast<object>().ToList());
            //q.AddEquals(MasBillingPropertyNames.ApprovalStatus,"P");
            q.AddGreaterThanOrEquals(MasBillingPropertyNames.DateCreated, DateTime.Now.AddDays(-1));
            q.PushBracket();
            q.AddEquals(MasBillingPropertyNames.StatusMessage, "Queued for Transfer");
            q.AddOr();
            q.AddEquals(MasBillingPropertyNames.StatusMessage, "Approve Previewed LSR");
            q.PopBracket();

            var res = EntityManager.Select(q).ActiveItems.Cast<MasBillingBase>();

            //if (res == null)
            //    return false;

            foreach (var i in res)
            {
                i.ApprovalStatus = apprphrase;
                i.Status = "F";
                i.StatusMessage = "Cancelled";

                //MasBillingDetailsBase q = (MasBillingDetailsBase)EntityManager.CreateEntity(MasBillingDetailsBase.EntityName, new Identity(id1));
                var q3 = EntityManager.CreateQuery(MasBillingDetailsBase.EntityName);
                q3.AddEquals(MasBillingDetailsPropertyNames.MasId, i.RecordId);
                var res3 = EntityManager.Select(q3).ActiveItems.Cast<MasBillingDetailsBase>();

                foreach (var k in res3)
                {
                    k.StatusMessage = "F";
                    EntityManager.Transaction.Add(k);
                }
                EntityManager.Transaction.Add(i);
            }
            return true;
        }

        public void email_send(string mailstring)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("10.3.1.106");
                mail.From = new MailAddress("samplemanager@deibellabs.com");
                //mail.To.Add("Helpdesk@DeibelLabs.com");
                mail.To.Add("eberrios@deibellabs.com");
                //mail.To.Add("bcochuyt@deibellabs.com");
                //mail.To.Add("DCohenour@DeibelLabs.com");
                //mail.To.Add("NMatson@DeibelLabs.com");
                mail.To.Add(mailstring);

                mail.Subject = "Sage Intaact Customer ID is missing in Sample Manager.";
                mail.Body = "Sage Intaact Customer ID is missing in Sample Manager for Customer_ID. Please update with the correct customer ID and remove flag in order to bill.";

                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Credentials = new System.Net.NetworkCredential("results", "3SFS@7120!");
                SmtpServer.EnableSsl = false;

                SmtpServer.Port = 25;
                SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;

                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
                //LogInfo(ex.Message);
            }

        }

        public void email_send(string mailstring, string customerid, string groupid)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("10.3.1.106");
                mail.From = new MailAddress("samplemanager@deibellabs.com");
                //mail.To.Add("Helpdesk@DeibelLabs.com");
                //mail.To.Add("eberrios@deibellabs.com");
                //mail.To.Add("bcochuyt@deibellabs.com");
                //mail.To.Add("DCohenour@DeibelLabs.com");
                //mail.To.Add("NMatson@DeibelLabs.com");
                // mail.To.Add("sgona@DeibelLabs.com");
                if (!string.IsNullOrEmpty(mailstring))
                    mail.To.Add(mailstring);

                mail.Subject = "Sage Intaact Customer ID is missing in Sample Manager.";

                mail.Body = "Sage Intaact Customer ID is missing in Sample Manager for Customer ID " + customerid + ". Please update Sage Customer ID and notify the lab " + groupid + " that they can continue to bill for Customer ID " + customerid + ".";

                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Credentials = new System.Net.NetworkCredential("results", "3SFS@7120!");
                SmtpServer.EnableSsl = false;

                SmtpServer.Port = 25;
                SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;

                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
                //LogInfo(ex.Message);
            }

        }

        // excel report using document open xml by creating new excel sheet
        //private void ExportData(List<row> Rows, string filename)
        //{
        //    using (var workbook = SpreadsheetDocument.Open(templateFilePath, false))
        //    {
        //        var workbookPart = workbook.AddWorkbookPart();
        //        workbook.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();
        //        workbook.WorkbookPart.Workbook.Sheets = new DocumentFormat.OpenXml.Spreadsheet.Sheets();

        //        var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
        //        var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
        //        sheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);

        //        DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
        //        string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

        //        uint sheetId = 1;
        //        if (sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() > 0)
        //        {
        //            sheetId =
        //                sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Select(s => s.SheetId.Value).Max() + 1;
        //        }

        //        DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = relationshipId, SheetId = sheetId, Name = "test" };
        //        sheets.Append(sheet);

        //        DocumentFormat.OpenXml.Spreadsheet.Row blankkrow = new DocumentFormat.OpenXml.Spreadsheet.Row();
        //        DocumentFormat.OpenXml.Spreadsheet.Row blankkrow1 = new DocumentFormat.OpenXml.Spreadsheet.Row();
        //        DocumentFormat.OpenXml.Spreadsheet.Row blankkrow2 = new DocumentFormat.OpenXml.Spreadsheet.Row();
        //        DocumentFormat.OpenXml.Spreadsheet.Row blankkrow3 = new DocumentFormat.OpenXml.Spreadsheet.Row();
        //        sheetData.Append(blankkrow);
        //        sheetData.Append(blankkrow1);
        //        sheetData.Append(blankkrow2);
        //        sheetData.Append(blankkrow3);

        //        //insert Image by specifying two range
        //        OpenXmlHelper.InsertImage(sheetPart, 1, 1, 3, 3, new FileStream(ImageFile, FileMode.Open));

        //        DocumentFormat.OpenXml.Spreadsheet.Row headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
        //        DocumentFormat.OpenXml.Spreadsheet.Cell cellh1 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //        cellh1.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //        cellh1.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue("PO"); //
        //        headerRow.AppendChild(cellh1);
        //        DocumentFormat.OpenXml.Spreadsheet.Cell cellh2 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //        cellh2.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //        cellh2.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue("LAB NO"); //
        //        headerRow.AppendChild(cellh2);
        //        DocumentFormat.OpenXml.Spreadsheet.Cell cellh3 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //        cellh3.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //        cellh3.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue("Description"); //
        //        headerRow.AppendChild(cellh3);
        //        List<String> columns = new List<string>();

        //        foreach (var column in Headers)
        //        {
        //            columns.Add(column.name);
        //            DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //            cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //            cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(column.name);
        //            double width = OpenXmlHelper.GetWidth("Calibri", 11, column.name);
        //            headerRow.AppendChild(cell);
        //        }


        //        sheetData.AppendChild(headerRow);
        //        foreach (var row in Rows)
        //        {
        //            DocumentFormat.OpenXml.Spreadsheet.Row newRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
        //            DocumentFormat.OpenXml.Spreadsheet.Cell cell1 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //            cell1.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //            cell1.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(row.PO); //
        //            newRow.AppendChild(cell1);
        //            DocumentFormat.OpenXml.Spreadsheet.Cell cell2 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //            cell2.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //            cell2.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(row.Lab); //
        //            newRow.AppendChild(cell2);

        //            DocumentFormat.OpenXml.Spreadsheet.Cell cell3 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //            cell3.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //            cell3.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(row.Description); //
        //            newRow.AppendChild(cell3);

        //            foreach (String col in columns)
        //            {

        //                var i = row.Items.Where(m => m.column == col).FirstOrDefault();
        //                DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //                cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //                cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(i.val.ToString()); //
        //                newRow.AppendChild(cell);
        //            }

        //            sheetData.AppendChild(newRow);
        //        }

        //        DocumentFormat.OpenXml.Spreadsheet.Row footerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
        //        DocumentFormat.OpenXml.Spreadsheet.Cell cellf1 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //        cellf1.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //        cellf1.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(); //
        //        footerRow.AppendChild(cellf1);
        //        DocumentFormat.OpenXml.Spreadsheet.Cell cellf2 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //        cellf2.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //        cellf2.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(); //
        //        footerRow.AppendChild(cellf2);
        //        DocumentFormat.OpenXml.Spreadsheet.Cell cellf3 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //        cellf3.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //        cellf3.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue("Totals"); //
        //        footerRow.AppendChild(cellf3);

        //        foreach (String col in columns)
        //        {
        //            var sum = Rows.SelectMany(n => n.Items.Where(m => m.column == col).Select(c => c.val)).Sum();
        //            DocumentFormat.OpenXml.Spreadsheet.Cell cellf = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //            cellf.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //            cellf.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(sum.ToString()); //
        //            footerRow.AppendChild(cellf);
        //        }

        //        sheetData.AppendChild(footerRow);
        //        var tete = sheetData.Elements<Column>();
        //        foreach (var column in tete.Cast<Column>().ToList())
        //        {
        //            double width2 = OpenXmlHelper.GetWidth("Calibri", 11, column.LocalName);
        //        }
        //    }
        //}

        // update excel file using excel template
        //private void ExportData2(List<row> Rows, string filename, DocumentFormat.OpenXml.Spreadsheet.SheetData sheetData)
        //{
        //    var cell = OpenXmlHelper.FindCell(sheetData, "C4");

        //    cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //    cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue("PO"); //
        //}
    }
}
