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
    [SampleManagerTask(nameof(MatrixQueueLogin), "LABTABLE", "WEB_SAMPLE_QUEUE")]
    public class MatrixQueueLogin : DefaultFormTask
    {
        FormMatrixQueueLogin _form;
        Color _red = Color.LightCoral;
        Color _blue = Color.LightBlue;
        UnboundGridColumn _customer;
        UnboundGridColumn _analysis;
        UnboundGridColumn _samplePoint;
        UnboundGridColumn _sampleId;
        UnboundGridColumn _description;
        UnboundGridColumn _descriptionB;
        UnboundGridColumn _descriptionC;
        UnboundGridColumn _component;
        UnboundGridColumn _technician;
        UnboundGridColumn _date;
        UnboundGridColumn _include;
        Workflow _jobWorkflow;
        Workflow _sampleWorkflow;

        protected override void SetupTask()
        {
            _form = FormFactory.CreateForm<FormMatrixQueueLogin>();
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
            // var q;
            var order = _form.txtSampleIds.Text.Trim();
            IEntityCollection col;
           
      
                var q = EntityManager.CreateQuery(WebSampleQueueBase.EntityName);
                q.AddEquals(WebSampleQueuePropertyNames.EmpJobOrder, order);
                q.AddOrder(WebSampleQueuePropertyNames.EmpSampleOrderId, true);
                col = EntityManager.Select(q);
            

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
                var thisId = sample.EmpSampleOrderId;
                if (id != thisId)
                {
                    id = thisId;
                    blueHue = !blueHue;
                }

                var qSample = new MatrixQueueSample();

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
                row.SetValue(_sampleId, sample.EmpSampleOrderId);
                qSample.SampleId = sample.EmpSampleOrderId;


                row.SetValue(_description, sample.SampleDescription);
                qSample.Description = sample.SampleDescription;

                //To Set the descriptionB value
                row.SetValue(_descriptionB, sample.DescriptionB);
                qSample.DescriptionB = sample.DescriptionB;

                row.SetValue(_descriptionC, sample.DescriptionC);
                qSample.DescriptionC = sample.DescriptionC;

                // If Analysis isn't in LIMS, default to not included and set analysis field red
                qSample.Analysis = GetAnalysis(sample.AnalysisId);
                RefreshCell(row, _analysis, qSample.Analysis, sample.AnalysisId);


                qSample.Technician = sample.SampleSubmitter;
                row.SetValue(_technician, sample.SampleSubmitter);

                qSample.ImportedDate = sample.DateImported;
                row.SetValue(_date, sample.DateImported);

                //New Changes 25th May
                qSample.EmpSampleOrderId = sample.EmpSampleOrderId;
                qSample.EmpTestOrderId = sample.EmpTestOrderId;
                //End
                qSample.EmpJobOrderId = sample.WebJobOrder;
                qSample.EmpSampleOrderId = sample.EmpSampleOrderId;
               // qSample.EmpTestOrderId = sample.EmpTestOrderId;

                //Modified by Avinash
                qSample.DescriptionB = sample.DescriptionB;
                qSample.DescriptionC = sample.DescriptionC;
                qSample.Comments = sample.Comments;
                qSample.ProductMatrix = sample.ProductMatrix;
                qSample.BatchNumber = sample.BatchNumber;
                qSample.PoNumber = sample.PoNumber;
                qSample.LoggedIn = sample.LoggedIn;
                // qSample.ImContactRole = sample.


            }
            grid.EndUpdate();

            _form.ClearBusy();
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            var JobLoggedIn = false;
            var JobOrderId = _form.txtSampleIds.Text.Trim();
            if (JobOrderId != null)
            {
                var query = EntityManager.CreateQuery(TableNames.JobHeader);
                query.AddEquals(JobHeaderPropertyNames.JobOrderId, JobOrderId);
                var results = EntityManager.Select(query).Cast<JobHeaderBase>();
                var result1 = EntityManager.Select(query).Cast<JobHeaderBase>().FirstOrDefault();
                if (result1 != null)
                {
                    JobLoggedIn = result1.ImLoginFlag1;
                }
                // var JobLoggedin = results.ImLoginFlag1;
                if (results.Count() > 0)
                {
                    if (results.Any(X => X.JobStatus.ToString() == PhraseJobStat.PhraseIdX.ToString()) && JobLoggedIn)
                    {
                        //continue;
                        NewMethod();
                    }
                    else
                    {

                        NewMethod();
                        //Library.Utils.FlashMessage("The Job is already logged in with this Job Order ID.To Log in a new Job Please cancel the previous Job", "Job Already logged in with this Job Order");
                        //Exit();
                    }
                }
                else
                {
                    NewMethod();
                }
            }

        }
        void NewMethod()
        { // Web samples with include checked
            var qSamples = _form.gridQueueSamples.Rows.Where(r => ((bool)r.GetValue(_include.Name)) && ((MatrixQueueSample)r.Tag).Valid).Select(r => r.Tag).Cast<MatrixQueueSample>().ToList();
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
            job.EmpJobOrderId = _form.txtSampleIds.Text.Trim();
            job.JobOrderId = "";
            var product = job.ProductId;





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
               // job.EmpJobOrderId = JobOrderId;
                job.ImLoginFlag1 = true;



                //End
                sample.CustomerId = qItem.Customer;
                sample.LoginFlag = true;
                //New Changes Avinash Start
                var customer = sample.CustomerId;
                SetValues(customer.ToString(), job, sample);
                //New Changes Avinash End
                //sample.ImSampleRefId = qItem.SampleId;
                sample.SamplingPoint = qItem.SamplePoint;
                sample.Description = qItem.Description;
                sample.DescriptionB = qItem.Technician;
                sample.DescriptionC = qItem.DescriptionC;
                sample.SampledDate = qItem.ImportedDate;

                //New Cjhanges 25thMay

                if (qItem.EmpSampleOrderId != null)
                {
                    sample.EmpSampleOrderId = qItem.EmpSampleOrderId;
                }


                //End

                //sample.SampleOrderId = qItem.EmpSampleOrderId;
                //Modified by Avinash - 31st May 2021
                sample.DescriptionB = qItem.DescriptionB;
                sample.DescriptionC = qItem.DescriptionC;
                sample.Comments = qItem.Comments;
                sample.ProductMatrix = qItem.ProductMatrix;
                sample.SampBatchNumber = qItem.BatchNumber;
                sample.ImContactRole = qItem.ImContactRole;
                sample.ImLoginFlag = qItem.LoggedIn;







                // Group on the analysis, add tests to sample
                foreach (var testGroup in sampleGroup.GroupBy(s => s.Analysis))
                {
                    var row = testGroup.First();
                    var test = sample.AddTest(testGroup.Key)[0];
                    test.ComponentList = qItem.ComponentList;
                    test.EmpTestOrderId = row.EmpTestOrderId;
                    EntityManager.Transaction.Add(test);

                    //New Changes 25th May
                    //if (qItem.EmpTestOrderId != null)
                    //{
                    //    test.TestOrderId = qItem.EmpTestOrderId;
                    //}
                    //test.EmpTestOrderId = qItem.EmpTestOrderId;

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
            //job.InvoiceToName = invoiceToName;
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

            //_samplePoint = grid.AddColumn("SamplePoint", "Sample Point");
            //_samplePoint.ReadOnly = true;

            _description = grid.AddColumn("Description", "Description");

            _descriptionB = grid.AddColumn("DescriptionB", "DescriptionB");

            _descriptionC = grid.AddColumn("DescriptionC", "DescriptionC");

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

    public class MatrixQueueSample
    {
        public string SampleId = "";
        public VersionedAnalysis Analysis;
        public VersionedComponent Component;
        public string ComponentList = "";
        public int OrderNumber;
        public Customer Customer;
        public SamplePoint SamplePoint;
        public string Technician = "";
        public NullableDateTime ImportedDate;
        public string Description = "";
        public bool Valid = true;
        

        public string EmpJobOrderId = "";
        public string EmpSampleOrderId = "";
        public string EmpTestOrderId = "";

        //Modified by AVinash
        public string DescriptionB = "";
        public string DescriptionC = "";
        public string Comments = "";
        public string ProductMatrix = "";
        public string BatchNumber = "";
        public string ImContactRole = "";
        public string PoNumber = "";
        public string Product = "";
        public string LimsCode = "";
        public Boolean LoggedIn;
    }
}
