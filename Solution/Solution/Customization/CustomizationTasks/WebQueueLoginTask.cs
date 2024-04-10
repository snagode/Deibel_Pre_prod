﻿using System;
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
    /// <summary>
    /// Sample login grid.  This is assigned to an explorer folder and used to log
    /// samples in using data in table web_sample_queue.  
    /// </summary>
    [SampleManagerTask(nameof(WebQueueLoginTask), "LABTABLE", "WEB_SAMPLE_QUEUE")]
    public class WebQueueLoginTask : DefaultFormTask
    {
        FormWebQueueLogin _form;
        Color _red = Color.LightCoral;
        Color _blue = Color.LightBlue;
        UnboundGridColumn _customer;
        UnboundGridColumn _analysis;
        UnboundGridColumn _samplePoint;
        UnboundGridColumn _sampleId;
        UnboundGridColumn _description;
        UnboundGridColumn _descriptionB;
        UnboundGridColumn _component;
        UnboundGridColumn _technician;
        UnboundGridColumn _date;
        UnboundGridColumn _include;
        UnboundGridColumn _BatchNumber;
        Workflow _jobWorkflow;
        Workflow _sampleWorkflow;

        protected override void SetupTask()
        {
            _form = FormFactory.CreateForm<FormWebQueueLogin>();
            _form.Loaded += _form_Loaded;
            _form.Show();
        }

        private void _form_Loaded(object sender, EventArgs e)
        {
            _form.btnLoad.Click += BtnLoad_Click;
            _form.btnLogin.Click += BtnLogin_Click;
            _form.btnCheckAll.Click += BtnCheckAll_Click;
            _form.btnUncheckAll.Click += BtnUncheckAll_Click;
            //_form.txtSampleIds.Text = "461612";

            _jobWorkflow = EntityManager.SelectLatestVersion(Workflow.EntityName, "5C034090-7C3A-46F0-9F35-A41729EAC02E") as Workflow;      // name = Deibel Job
            _sampleWorkflow = EntityManager.SelectLatestVersion(Workflow.EntityName, "580E9809-ACF3-46A3-896D-94D1FFD44A11") as Workflow;   // name = Web Sample
        }

        private void BtnUncheckAll_Click(object sender, EventArgs e)
        {
            RefreshChecks(false);
        }

        private void BtnCheckAll_Click(object sender, EventArgs e)
        {
            RefreshChecks(true);
        }

        void RefreshChecks(bool setting)
        {
            var grid = _form.gridQueueSamples;
            foreach (var row in grid.Rows)
                row.SetValue(_include, setting);
        }

        #region Events

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            var order = _form.txtSampleIds.Text.Trim();

            var q = EntityManager.CreateQuery(WebSampleQueueBase.EntityName);
            q.AddEquals(WebSampleQueuePropertyNames.WebJobOrder, order);
            q.AddOrder(WebSampleQueuePropertyNames.SampleOrderId, true);
            var col = EntityManager.Select(q);
            if (col.Count == 0)
            {
                Library.Utils.FlashMessage("No samples found.", "");
                return;
            }

            bool logged = col.Cast<WebSampleQueueBase>().First().LoggedIn;
            if (logged && !Library.Utils.FlashMessageYesNo($"Web queue order {order} has been logged in previously.  Continue?", "Confirm"))
                return;

            _form.txtSampleIds.ReadOnly = true;
            _form.SetBusy();

            // Start building grid - it's an "Unbound Grid"
            var grid = _form.gridQueueSamples;
            grid.BeginUpdate();
            grid.ClearRows();
            BuildColumns();

            // Loop through queue items and add to grid
            bool blueHue = false;
            var id = "";
            foreach (WebSampleQueueBase sample in col)
            {
                // Color logic
                var thisId = sample.SampleOrderId;
                if (id != thisId)
                {
                    id = thisId;
                    blueHue = !blueHue;
                }

                var qSample = new QueueSample();

                var row = grid.AddRow();
                if (blueHue)
                    row.SetBackgroundColor(_blue);
                row.Tag = qSample;

                row.SetValue(_include, true);
                _include.ReadOnly = false;

                // If customer isn't in LIMS, default to not included and set customer field red
                qSample.Customer = GetCustomer(sample.CustomerId);
                RefreshCell(row, _customer, qSample.Customer, sample.CustomerId);

                // Order id
                row.SetValue(_sampleId, sample.SampleOrderId);
                qSample.SampleId = sample.SampleOrderId;

                // Removed Mar 10 2021.  Keeping here in case change reversion
                // Sample point
                //qSample.SamplePoint = GetSamplePoint(sample.SamplePoint);
                //RefreshCell(row, _samplePoint, qSample.SamplePoint, sample.SamplePoint);

                row.SetValue(_description, sample.SampleDescription);
                qSample.Description = sample.SampleDescription;

                //To Set the descriptionB value
                row.SetValue(_descriptionB, sample.DescriptionB);
                qSample.DescriptionB = sample.DescriptionB;

                row.SetValue(_BatchNumber, sample.BatchNumber);
                qSample.BatchNumber = sample.BatchNumber;



                // If Analysis isn't in LIMS, default to not included and set analysis field red
                qSample.Analysis = GetAnalysis(sample.AnalysisId);
                RefreshCell(row, _analysis, qSample.Analysis, sample.AnalysisId);

                //Added on 22/10/21 -Start
                if (BaseEntity.IsValid(qSample.Analysis))
                {
                    var cl = qSample.Analysis.CLHeaders.Cast<VersionedCLHeader>().Where(h => h.CompList == sample.ComponentList).FirstOrDefault();
                    if (BaseEntity.IsValid(cl))
                        qSample.ComponentList = sample.ComponentList;
                }

                //End
                // Removed Mar 10 2021.  Keeping here in case change reversion
                // If Component isn't in LIMS, default to not included and set analysis field red
                //qSample.Component = GetComponent(sample.AnalysisId, sample.ComponentName);
                //RefreshCell(row, _component, qSample.Component, sample.ComponentName);

                qSample.Technician = sample.SampleSubmitter;
                row.SetValue(_technician, sample.SampleSubmitter);

                qSample.DateImported = sample.DateImported;
                row.SetValue(_date, sample.DateImported);

                qSample.JobOrderId = sample.WebJobOrder;
                qSample.SampleOrderId = sample.SampleOrderId;
                qSample.TestOrderId = sample.TestOrderId;

                //Modified by Avinash
                //qSample.GroupId = sample.GroupId;
                qSample.DescriptionB = sample.DescriptionB;
                qSample.DescriptionC = sample.DescriptionC;
                qSample.Comments = sample.Comments;
                qSample.ProductMatrix = sample.ProductMatrix;
                qSample.BatchNumber = sample.BatchNumber;
                qSample.PoNumber = sample.PoNumber;

                // qSample.ImContactRole = sample.


            }
            grid.EndUpdate();

            _form.ClearBusy();
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            var JobOrderId = _form.txtSampleIds.Text.Trim();
            var query = EntityManager.CreateQuery(TableNames.JobHeader);
            query.AddEquals(JobHeaderPropertyNames.JobOrderId, JobOrderId);
            var result = EntityManager.Select(query).Cast<JobHeader>().ToList();
            
            if (result.Count == 0 )
            {
                //if (JobOrderId != null)
                //{
                //var query1 = EntityManager.CreateQuery(TableNames.JobHeader);
                //query.AddEquals(JobHeaderPropertyNames.JobOrderId, JobOrderId);
                //var results = EntityManager.Select(query1).ActiveCount;
                //var results1 = EntityManager.Select(query1).Cast<JobHeader>().ToList();
                //if (results > 0)
                //{

                //if (results.Any(X => X.JobStatus.ToString() == PhraseJobStat.PhraseIdX.ToString()))
                //{
                //    //continue;
                //    NewMethod();
                //}
                //else
                //{
                //    Library.Utils.FlashMessage("The Job is already logged in with this Job Order ID.To Log in a new Job Please cancel the previous Job", "Job Already logged in with this Job Order");
                //    Exit();
                //}
                if (result.Any(X => X.JobStatus.ToString() == PhraseJobStat.PhraseIdV.ToString()))
                {
                    //continue;
                    Library.Utils.FlashMessage("The Job is already logged in with this Job Order ID.To Log in a new Job Please cancel the previous Job", "Job Already logged in with this Job Order");
                    Exit();

                }
                else
                {
                    NewMethod();
                }


            }

            else if((result.Any(X => X.JobStatus.ToString() == PhraseJobStat.PhraseIdX.ToString())) && !((result.Any(X => X.JobStatus.ToString() == PhraseJobStat.PhraseIdV.ToString()))))
            {
                NewMethod();
            }

            else
            {
                Library.Utils.FlashMessage("This Job order is already Logged in", " This Job order is already logged in");
            }

        }

        public void NewMethod()
        {
            _form.SetBusy();
            // Web samples with include checked
            var qSamples = _form.gridQueueSamples.Rows.Where(r => ((bool)r.GetValue(_include.Name)) && ((QueueSample)r.Tag).Valid).Select(r => r.Tag).Cast<QueueSample>().ToList();
            if (qSamples.Count == 0)
            {
                Library.Utils.FlashMessage("No valid queue samples found.", "");
                return;
            }

            // Make parent job
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

            job.JobOrderId = _form.txtSampleIds.Text.Trim();
            var product = job.ProductId;
            var DeibelLabId = job.GroupId;





            // Group on sample ID from the file and loop through tests
            var samples = EntityManager.CreateEntityCollection(Sample.EntityName);
            foreach (var sampleGroup in qSamples.GroupBy(s => s.SampleId))
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
                job.CustomerId = qItem.Customer;
                //Modified by Avinash - Start
                job.PoNumber = qItem.PoNumber;



                //End
                sample.CustomerId = qItem.Customer;
                //New Changes Avinash Start
                var customer = sample.CustomerId;
                SetValues(customer.ToString(), job, sample);
                //New Changes Avinash End
                // sample.ImSampleRefId = qItem.SampleId;
                sample.GroupId = job.GroupId;
                sample.SamplingPoint = qItem.SamplePoint;
                sample.Description = qItem.Description;
                sample.DescriptionB = qItem.Technician;
                sample.SampledDate = qItem.DateImported;
                sample.SampleOrderId = qItem.SampleOrderId;
                //Modified by Avinash - 31st May 2021
                sample.DescriptionB = qItem.DescriptionB;
                sample.DescriptionC = qItem.DescriptionC;
                sample.Comments = qItem.Comments;
                sample.ProductMatrix = qItem.ProductMatrix;
                sample.SampBatchNumber = qItem.BatchNumber;
                sample.ImContactRole = qItem.ImContactRole;







                // Group on the analysis, add tests to sample
                foreach (var testGroup in sampleGroup.GroupBy(s => s.Analysis))
                {
                    //22/10/21-Deployed in Prod from Dev qitem-->row
                    // BSmock 11-Oct-21 - replace qItem reference with single test item reference (row)
                    var row = testGroup.First();

                    var test = sample.AddTest(testGroup.Key, false)[0];
                    if(! (String.IsNullOrEmpty(row.ComponentList)))
                    {
                        test.ComponentList = row.ComponentList;
                    }
                    test.HasResultList = !string.IsNullOrWhiteSpace(row.ComponentList) ? true : false;
                    test.TestOrderId = row.TestOrderId;

                    // Removed Mar 10 2021 - no more single result assignments
                    //if(!string.IsNullOrWhiteSpace(qItem.ComponentList))
                    //    test.HasResultList = true;

                    //// Add result component
                    //foreach (var qResult in testGroup.OrderBy(r => r.OrderNumber).GroupBy(t => t.Component))
                    //{
                    //    var result = test.AddResult(qResult.First().Component);
                    //    result.ImResultRefId = qItem.SampleId;
                    //    result.SetStatus(PhraseReslStat.PhraseIdU);
                    //}
                }
                EntityManager.Transaction.Add(job);
                samples.Add(sample);
                EntityManager.Transaction.Add(sample);
            }
            EntityManager.Commit();
            _form.ClearBusy();

            if (samples.Count == 0)
                Library.Utils.FlashMessage($"Workflow {_sampleWorkflow.WorkflowName} failed.", "Error");
            else
                // Open login form
                Library.Task.CreateTask(35127, job);
        }
        #endregion

        #region Utility
        void SetValues(string customerId, JobHeader job, Sample sample)
        {
            var user = (Personnel)this.Library.Environment.CurrentUser;
            var DefaultGoupID = user.DefaultGroup;

            var query = EntityManager.CreateQuery(TableNames.Customer);
            query.AddEquals(CustomerPropertyNames.Identity, customerId);
            var results = (CustomerBase)EntityManager.Select(query).ActiveItems[0];
            var CustomerContacts = results.CustomerContacts.ActiveItems;
            if (CustomerContacts != null)
            {
                foreach (CustomerContactsBase contact in CustomerContacts)
                {
                    if (contact.Type.ToString() == PhraseContctTyp.PhraseIdCONTACT && contact.LoginFlag == true)
                    {
                        job.ReportToName = contact.ContactName.ToString();
                        job.OfficePhone = contact.OfficePhone;
                    }
                }
            }

            if (job.ReportToName == "")
            {
                var CustomerContacts1 = results.CustomerContacts.ActiveItems;
                if (CustomerContacts1 != null)
                {
                    foreach (CustomerContactsBase contact in CustomerContacts1)
                    {
                        job.ReportToName = contact.ContactName.ToString();
                        job.OfficePhone = contact.OfficePhone;
                        break;
                    }
                }

            }

            var product = results.Product.Identity;
            sample.Product = product;
            var salesperson = results.SalespersonId;
            var CustomerName = results.CustomerName;
            var DeibelDivivion = results.DeibelDivision;
            //var invoiceToName = results.CustomerContacts.ActiveItems[0].Identity.Fields[1].ToString();
            var DeibelLabID = results.GroupId;
            job.SalespersonId = salesperson;
            job.ProductId = product;
            job.CustomerName = CustomerName;
           // job.InvoiceToName = invoiceToName;
            job.GroupId = DefaultGoupID;

        }
        void BuildColumns()
        {
            var grid = _form.gridQueueSamples;

            _include = grid.AddColumn("Include", "Include", GridColumnType.Boolean);

            _customer = grid.AddColumn("Customer", "Customer");
            _customer.ReadOnly = true;

            _sampleId = grid.AddColumn("SampleId", "Sample Id");
            _sampleId.ReadOnly = true;


            _BatchNumber = grid.AddColumn("Batch Number", "Batch Number");
            _BatchNumber.ReadOnly = true;
            //_samplePoint = grid.AddColumn("SamplePoint", "Sample Point");
            //_samplePoint.ReadOnly = true;

            _description = grid.AddColumn("Description", "Description");
            _descriptionB = grid.AddColumn("DescriptionB", "DescriptionB");

            _analysis = grid.AddColumn("Test", "Test");
            _analysis.ReadOnly = true;

            //_component = grid.AddColumn("Component", "Component");
            //_component.ReadOnly = true;

            _technician = grid.AddColumn("Technician", "Technician");
            _technician.ReadOnly = true;

            _date = grid.AddColumn("Date Imported", "Date Imported");
            _date.ReadOnly = true;

        }

        SamplePoint GetSamplePoint(string pointDescription)
        {
            var q = EntityManager.CreateQuery(SamplePoint.EntityName);
            q.AddEquals(SamplePointPropertyNames.Description, pointDescription.Trim());
            return EntityManager.Select(q).GetFirst() as SamplePoint;
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

        Customer GetCustomer(string customer)
        {
            return EntityManager.Select(CustomerBase.EntityName, new Identity(customer)) as Customer;
        }

        void RefreshCell(UnboundGridRow row, UnboundGridColumn col, IEntity entity, string queueText)
        {
            var valid = true;
            if (!BaseEntity.IsValid(entity))
            {
                col.SetCellBackgroundColor(row, _red);
                row.SetValue(_include, false);
                valid = false;
                ((QueueSample)row.Tag).Valid = false;
            }
            row.SetValue(col, GetCellText(queueText, valid));
        }

        string GetCellText(string value, bool valid)
        {
            if (valid)
                return value;

            if (string.IsNullOrWhiteSpace(value))
                return "{EMPTY}";
            return "{" + value + "}";
        }

        #endregion






    }

    public class QueueSample
    {
        public string SampleId = "";
        public VersionedAnalysis Analysis;
        public VersionedComponent Component;
        public string ComponentList = "";
        public int OrderNumber;
        public Customer Customer;
        public SamplePoint SamplePoint;
        public string Technician = "";
        public NullableDateTime DateImported;
        public string Description = "";
        public bool Valid = true;
        public string SampleOrderId = "";
        public string TestOrderId = "";
        public string JobOrderId = "";
        //Modified by AVinash
        public string DescriptionB = "";
        public string DescriptionC = "";
        public string Comments = "";
        public string ProductMatrix = "";
        public string BatchNumber = "";
        public string ImContactRole = "";
        public string PoNumber = "";
        public string Product = "";
       
        public GroupHeader GroupId ;
    }
}