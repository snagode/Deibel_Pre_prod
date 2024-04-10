using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common;
using Thermo.SampleManager.Common.CommandLine;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Utilities;
using Thermo.SampleManager.Core.Definition;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(FtpLoginQueue), "LABTABLE", "FTP_Login_QUeue")]
    public class FtpLoginQueue : DefaultFormTask
    {

        String alertText = "";
        int val = 1;
        Color _red = Color.LightCoral;
        Color _blue = Color.LightBlue;
        FormFtpLoginQueue _form;
        UnboundGridColumn _auxinfoe;
        UnboundGridColumn _auxinfod;
        UnboundGridColumn _auxinfoc;
        UnboundGridColumn _auxinfob;
        UnboundGridColumn _transactionId;
        UnboundGridColumn _customerID;
        UnboundGridColumn _dateimported;
        UnboundGridColumn _identity;
        UnboundGridColumn _keyfield;
        UnboundGridColumn _include;
        Workflow _jobWorkflow;
        Workflow _sampleWorkflow;
        String ftpcontacts = "";
        int temp = 0;
        string emailIds = "";
        IEntityCollection collectionFtpTests;

        protected override void SetupTask()
        {
            _form = FormFactory.CreateForm<FormFtpLoginQueue>();
            _form.Loaded += _form_Loaded;
            _form.Show();
        }

        private void _form_Loaded(object sender, EventArgs e)
        {
            _form.btnLoad.Click += BtnLoad_Click;
            _form.TxtCustomerID.Enabled = false;
            _form.btnLogin.Click += BtnLogin_Click;
            // _form.TxtCustomerID.TextChanged += TxtCustomerID_TextChanged;
            // _form.TxtCustomerID.EditValueChanged += TxtCustomerID_EditValueChanged;
            // _form.TxtCustomerID.Leave += TxtCustomerID_Leave;
            _form.TxtCustomerID.LostFocus += TxtCustomerID_LostFocus;
            _jobWorkflow = EntityManager.SelectLatestVersion(Workflow.EntityName, "5C034090-7C3A-46F0-9F35-A41729EAC02E") as Workflow;      // name = Deibel Job
            _sampleWorkflow = EntityManager.SelectLatestVersion(Workflow.EntityName, "580E9809-ACF3-46A3-896D-94D1FFD44A11") as Workflow;   // name = Web Sample
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            // Web samples with include checked
            var qSamples = _form.gridLoginSamples.Rows.Where(r => ((bool)r.GetValue(_include.Name)) && ((LoginSample)r.Tag).Valid).Select(r => r.Tag).Cast<LoginSample>().ToList();
            if (qSamples.Count == 0)
            {
                Library.Utils.FlashMessage("No valid queue samples found.", "");
                return;
            }
            var bag = _jobWorkflow.Perform();
            JobHeader job;
            var entities = bag.GetEntities(JobHeader.EntityName);
            if (entities.Count == 0)
            {
                Library.Utils.FlashMessage($"Workflow {_jobWorkflow.WorkflowName} failed.", "Error");
                return;
            }
            job = entities[0] as JobHeader;
            if (job == null)
            {
                Library.Utils.FlashMessage($"Workflow {_jobWorkflow.WorkflowName} failed.", "Error");
                return;
            }

            // Group on sample ID from the file and loop through tests
            var samples = EntityManager.CreateEntityCollection(Sample.EntityName);
            foreach (var sampleGroup in qSamples.GroupBy(s => s.Keyfield))
            {
                // Make the sample
                bag = _sampleWorkflow.Perform();
                Sample sample;
                entities = bag.GetEntities(Sample.EntityName);
                if (entities.Count == 0)
                    continue;
                sample = entities[0] as Sample;
                if (sample == null)
                    continue;
                var qItem = sampleGroup.First();
                sample.JobName = job;
                foreach (FtpTestBase test in collectionFtpTests)
                {

                    // Test test2 = new Test();
                    // var analysis = new VersionedAnalysisInternal();
                    // test2.Analysis = new VersionedAnalysisInternal() ;
                    //  analysis.Identity = "3_MCPD_20";
                    //  analysis.AnalysisVersion = "1";
                    //var val = test.Analysis as VersionedAnalysisInternal;
                    // var test1 = sample.AddTest(analysis);
                    // test.Analysis.cas = 





                    var Alias = "3_MCPD_20";
                    var q = EntityManager.CreateQuery(TableNames.VersionedAnalysis);
                    q.AddEquals(VersionedAnalysisPropertyNames.VersionedAnalysisName, Alias);
                    var col = EntityManager.Select(q).ActiveItems[0];
                    var ana = col as VersionedAnalysisInternal;
                    var test5 = sample.AddTest(ana);

                    //// VersionedAnalysis Analysis =  test.Name
                    //var test1 = sample.AddTest(Analysis);
                }

                //job.CustomerId = qItem.Customer;
                //foreach (var testGroup in sampleGroup.GroupBy(s => s.Analysis))
                //{
                //    var test = sample.AddTest(testGroup.Key)[0];
                //    test.ComponentList = qItem.ComponentList;
                //    test.TestOrderId = qItem.TestOrderId;

                //}

                EntityManager.Transaction.Add(job);
                samples.Add(sample);
                EntityManager.Transaction.Add(sample);
            }
            EntityManager.Commit();

            if (samples.Count == 0)
                Library.Utils.FlashMessage($"Workflow {_sampleWorkflow.WorkflowName} failed.", "Error");
            else
                // Open login form
                Library.Task.CreateTask(35127, job);

        }
        VersionedAnalysis GetAnalysis(string analysis)
        {
            var q = EntityManager.CreateQuery(VersionedAnalysis.EntityName);
            q.AddEquals(VersionedAnalysisPropertyNames.Identity, analysis.Trim());
            q.AddOrder(VersionedAnalysisPropertyNames.AnalysisVersion, false);
            return EntityManager.Select(q).GetFirst() as VersionedAnalysis;
        }

        VersionedComponent GetComponent(string analysis, string component)
        {
            var q = EntityManager.CreateQuery(VersionedComponent.EntityName);
            q.AddEquals(VersionedComponentPropertyNames.Analysis, analysis);
            q.AddEquals(VersionedComponentPropertyNames.VersionedComponentName, component.Trim());
            q.AddOrder(VersionedComponentPropertyNames.AnalysisVersion, false);
            return EntityManager.Select(q).GetFirst() as VersionedComponent;
        }
        private void TxtCustomerID_LostFocus(object sender, EventArgs e)
        {
            var customerID = _form.TxtCustomerID.RawText;
            var query1 = EntityManager.CreateQuery(TableNames.Customer);
            query1.AddEquals(CustomerPropertyNames.CustomerName, customerID);
            Customer result = (Customer)EntityManager.Select(query1).ActiveItems[0];
            emailIds = result.FtpContact;
            SendEmail();
        }

        private void TxtCustomerID_Leave(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }



        private void TxtCustomerID_TextChanged(object sender, TextChangedEventArgs e)
        {
            var customerID = _form.TxtCustomerID.Text;
            var query1 = EntityManager.CreateQuery(TableNames.Customer);
            query1.AddEquals(CustomerPropertyNames.CustomerName, customerID);
            Customer result = (Customer)EntityManager.Select(query1).ActiveItems[0];
            emailIds = result.FtpContact;
            SendEmail();

        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            var samplevalues = _form.txtSampleIds.Text;
            // string [] values = samplevalues.Trim(',');
            //       var values = samplevalues.Split(',')
            //.Select(x => x.Trim())
            //.Where(x => !string.IsNullOrWhiteSpace(x))
            //.ToArray().ToList<String>();

            var values = samplevalues.Split(',')
       .Select(x => x.Trim())
       .Where(x => !string.IsNullOrWhiteSpace(x))
       .ToArray();
            if (values.Length >= 1)
            {
                var firstSample = values[0];

                var query = EntityManager.CreateQuery(TableNames.FtpSample);
                query.AddEquals(FtpSamplePropertyNames.KeyField, firstSample);
                var result1 = EntityManager.Select(query);
                if (result1.Count >= 1)
                {
                    FtpSampleBase ftpsamples = (FtpSampleBase)EntityManager.Select(query).ActiveItems[0];
                    var customerId = ftpsamples.CustomerId;

                    ftpcontacts = customerId.FtpContact;
                }

            }

            //  var val2 = values.Cast<Object>().ToList();
            var count = values.Count();
            int t = 1;

            var q = EntityManager.CreateQuery(TableNames.FtpSample);



            q.AddEquals(FtpSamplePropertyNames.KeyField, values[0]);
            t++;


            if ((t == 2) && (t <= count))
            {
                q.AddOr();
                q.AddEquals(FtpSamplePropertyNames.KeyField, values[1]);
                t++;
            }
            // q.AddIn(FtpSamplePropertyNames.KeyField, val2);

            if ((t == 3) && (t <= count))
            {
                q.AddOr();
                q.AddEquals(FtpSamplePropertyNames.KeyField, values[2]);
                t++;
            }
            if ((t == 4) && (t <= count))
            {
                q.AddOr();
                q.AddEquals(FtpSamplePropertyNames.KeyField, values[3]);
                t++;
            }
            if ((t == 5) && (t <= count))
            {
                q.AddOr();
                q.AddEquals(FtpSamplePropertyNames.KeyField, values[4]);
                t++;
            }
            if ((t == 6) && (t <= count))
            {
                q.AddOr();
                q.AddEquals(FtpSamplePropertyNames.KeyField, values[5]);
                t++;
            }
            if ((t == 7) && (t <= count))
            {
                q.AddOr();
                q.AddEquals(FtpSamplePropertyNames.KeyField, values[6]);
                t++;
            }
            if ((t == 8) && (t <= count))
            {
                q.AddOr();
                q.AddEquals(FtpSamplePropertyNames.KeyField, values[7]);
                t++;
            }
            if ((t == 9) && (t <= count))
            {
                q.AddOr();
                q.AddEquals(FtpSamplePropertyNames.KeyField, values[8]);
                t++;
            }
            if ((t == 10) && (t <= count))
            {
                q.AddOr();
                q.AddEquals(FtpSamplePropertyNames.KeyField, values[9]);
                t++;
            }
            if ((t == 11) && (t <= count))
            {
                q.AddOr();
                q.AddEquals(FtpSamplePropertyNames.KeyField, values[10]);
                t++;
            }
            if ((t == 12) && (t <= count))
            {
                q.AddOr();
                q.AddEquals(FtpSamplePropertyNames.KeyField, values[11]);
                t++;
            }
            if ((t == 13) && (t <= count))
            {
                q.AddOr();
                q.AddEquals(FtpSamplePropertyNames.KeyField, values[12]);
                t++;
            }
            if ((t == 14) && (t <= count))
            {
                q.AddOr();
                q.AddEquals(FtpSamplePropertyNames.KeyField, values[13]);
                t++;
            }
            if ((t == 15) && (t <= count))
            {
                q.AddOr();
                q.AddEquals(FtpSamplePropertyNames.KeyField, values[14]);

            }

            var col = EntityManager.Select(q);


            //var col1 = (FtpSampleBase)EntityManager.Select(q).ActiveItems[0];
            //foreach(FtpSampleBase item in col.ActiveItems)
            //{
            //    if(item.KeyField==values[0])
            //}





            //foreach (FtpSampleBase item in col.ActiveItems)
            //{
            for (int i = 0; i < values.Length; i++)
            {
                if (col.ActiveItems.Cast<FtpSampleBase>().ToList().Any(m => values[i].Contains(m.KeyField)))
                {

                }
                else
                {
                    alertText = alertText + "  " + " " + values[i];
                }
            }

            //    if (col.Contains(values[i]))
            //     {

            //    }
            //    else
            //    {
            //        alertText = alertText + "  " + " " +  values[i];
            //    }

            //}



            if (col.Count == 0)
            {
                temp = 1;
                Library.Utils.FlashMessage(" What Customer ID are the missing samples for?", "What Customer ID are the missing samples for?");
                _form.TxtCustomerID.Enabled = true;
            }

            if (temp != 1)
            {
                if (col.Count == 0 || samplevalues == "" || alertText != "")
                {
                    if (Library.Utils.FlashMessageYesNo("The Sample Keyfiled" + " " + alertText + " " + "is missing.Do you want to send an email to concerned person?", "Do you want to send an email"))
                    {
                        SendEmail();
                    }
                    alertText = "";
                    return;
                }
            }

            if (_form.TxtCustomerID.Text != null)
            {

            }
            var grid = _form.gridLoginSamples;
            grid.BeginUpdate();
            grid.ClearRows();
            BuildColumns();
            bool blueHue = false;
            var id = "";
            foreach (FtpSampleBase sample in col)
            {
                var thisId = sample.KeyField;
                if (id != thisId)
                {
                    id = thisId;
                    blueHue = !blueHue;
                }

                var loginSample = new LoginSample();
                var row = grid.AddRow();
                if (blueHue)
                    row.SetBackgroundColor(_blue);
                row.Tag = sample;

                row.SetValue(_include, true);
                _include.ReadOnly = false;
                row.Tag = loginSample;
                //To check  loginSample.Analysis = GetAnalysis(sample.AnalysisId);
                collectionFtpTests = sample.FtpTests;
                row.SetValue(_identity, sample.Identity);
                row.SetValue(_keyfield, sample.KeyField);
                row.SetValue(_dateimported, sample.DateImported);
                row.SetValue(_customerID, sample.CustomerId);
                row.SetValue(_transactionId, sample.TransactionId);
                row.SetValue(_auxinfob, sample.AuxInfoB);
                row.SetValue(_auxinfoc, sample.AuxInfoC);
                row.SetValue(_auxinfod, sample.AuxInfoD);
                row.SetValue(_auxinfoe, sample.AuxInfoE);
            }
            grid.EndUpdate();
        }

        void SendEmail()
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("10.3.1.4");
            mail.From = new MailAddress("Results@deibellabs.com");
            // mail.To.Add("EGlynn@lindt.com");
            if (temp == 1)
            {
                mail.To.Add(emailIds);
            }
            else
            {
                mail.To.Add(ftpcontacts);
            }

            mail.Subject = " Missing FTP Samples ";
            //string htmlString = @"<html>
            //          <body>
            //          <p>Dear Ms. Susan,</p>
            //          <p>Thank you for your letter of yesterday inviting me to come for an interview on Friday afternoon, 5th July, at 2:30.
            //                  I shall be happy to be there as requested and will bring my diploma and other papers with me.</p>
            //          <p>Sincerely,<br>-Jack</br></p>
            //          </body>
            //          </html>

            mail.Body = "The following FTP Samples are missing:" + Environment.NewLine + Environment.NewLine + " " +

                " " + alertText + Environment.NewLine + Environment.NewLine +

                "These samples will be placed on hold until the FTP files are received.  Please advise at your earliest convenience."


                + Environment.NewLine + Environment.NewLine + Environment.NewLine + "Note: This is an auto generated message.    "
                ;

            SmtpServer.UseDefaultCredentials = false;
            SmtpServer.Credentials = new System.Net.NetworkCredential("results", "3SFS@7120!");
            SmtpServer.EnableSsl = false;

            SmtpServer.Port = 25;
            SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;


            SmtpServer.Send(mail);
            Library.Utils.FlashMessage("Mail is successfully Sent", "Mail is Successfully Sent");
        }

        void BuildColumns()
        {
            var grid1 = _form.gridLoginSamples;
            _include = grid1.AddColumn("Include", "Include", GridColumnType.Boolean);

            _identity = grid1.AddColumn("Identity", "Identity");
            _keyfield = grid1.AddColumn("KeyField", "Keyfield");

            _dateimported = grid1.AddColumn("DateImported", "DateImported");


            _customerID = grid1.AddColumn("CustomerId", "CustomerId");
            _customerID.ReadOnly = true;

            //_samplePoint = grid.AddColumn("SamplePoint", "Sample Point");
            //_samplePoint.ReadOnly = true;

            _transactionId = grid1.AddColumn("TransactionId", "TransactionId");

            _auxinfob = grid1.AddColumn("AuxInfoB", "AuxInfoB");
            _auxinfob.ReadOnly = true;

            //_component = grid.AddColumn("Component", "Component");
            //_component.ReadOnly = true;

            _auxinfoc = grid1.AddColumn("AuxInfoC", "AuxInfoC");
            _auxinfoc.ReadOnly = true;

            _auxinfod = grid1.AddColumn("AuxInfoD", "AuxInfoD");
            _auxinfod.ReadOnly = true;

            _auxinfoe = grid1.AddColumn("AuxInfoE", "AuxInfoE");
            _auxinfoe.ReadOnly = true;
        }
    }

    public class LoginSample
    {
        public string ComponentList = "";
        public string TestOrderId = "";
        public VersionedAnalysis Analysis;
        public string Keyfield = "";
        public string AuxInfoB = "";
        public string AuxInfoC = "";
        public string AuxInfoD = "";
        public string AuxInfoE = "";
        public bool Valid = true;

    }
}

