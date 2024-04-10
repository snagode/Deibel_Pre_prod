//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.Mail;
//using System.Text;
//using System.Threading.Tasks;
//using Thermo.Framework.Core;
//using Thermo.SampleManager.Common;
//using Thermo.SampleManager.Common.CommandLine;
//using Thermo.SampleManager.Common.Data;
//using Thermo.SampleManager.Common.Utilities;
//using Thermo.SampleManager.Core.Definition;
//using Thermo.SampleManager.Internal.ObjectModel;
//using Thermo.SampleManager.Library;
//using Thermo.SampleManager.Library.ClientControls;
//using Thermo.SampleManager.Library.DesignerRuntime;
//using Thermo.SampleManager.Library.EntityDefinition;
//using Thermo.SampleManager.Library.FormDefinition;
//using Thermo.SampleManager.ObjectModel;
//using Thermo.SampleManager.Tasks;

//namespace Customization.Tasks
//{
//    [SampleManagerTask(nameof(WebQueueLoginTask), "LABTABLE", "WEB_SAMPLE_QUEUE")]
//    public class WebQueueLoginTask : DefaultFormTask
//    {
//        FormWebQueueLogin _form;
//        Color _red = Color.LightCoral;
//        UnboundGridColumn _customer;
//        UnboundGridColumn _analysis;
//        UnboundGridColumn _samplePoint;
//        UnboundGridColumn _sampleId;
//        UnboundGridColumn _description;
//        UnboundGridColumn _component;
//        UnboundGridColumn _technician;
//        UnboundGridColumn _date;
//        UnboundGridColumn _include;
//        Workflow _jobWorkflow;
//        Workflow _sampleWorkflow;

//        protected override void SetupTask()
//        {
//            _form = FormFactory.CreateForm<FormWebQueueLogin>();
//            _form.Loaded += _form_Loaded;
//            _form.Show();
//        }

//        private void _form_Loaded(object sender, EventArgs e)
//        {
//            _form.btnLoad.Click += BtnLoad_Click;
//            _form.btnLogin.Click += BtnLogin_Click;
//            _form.txtSampleIds.Text = "461612";

//            _jobWorkflow = EntityManager.SelectLatestVersion(Workflow.EntityName, "5C034090-7C3A-46F0-9F35-A41729EAC02E") as Workflow;      // Deibel Job
//            _sampleWorkflow = EntityManager.SelectLatestVersion(Workflow.EntityName, "580E9809-ACF3-46A3-896D-94D1FFD44A11") as Workflow;   // Web Sample
//        }

//        void BuildColumns()
//        {
//            var grid = _form.gridQueueSamples;

//            _include = grid.AddColumn("Include", "Include", GridColumnType.Boolean);

//            _customer = grid.AddColumn("Customer", "Customer");
//            _customer.ReadOnly = true;

//            _sampleId = grid.AddColumn("SampleId", "Sample Id");
//            _sampleId.ReadOnly = true;

//            _samplePoint = grid.AddColumn("SamplePoint", "Sample Point");
//            _samplePoint.ReadOnly = true;

//            _description = grid.AddColumn("Description", "Description");

//            _analysis = grid.AddColumn("Test", "Test");
//            _analysis.ReadOnly = true;

//            _component = grid.AddColumn("Component", "Component");
//            _component.ReadOnly = true;

//            _technician = grid.AddColumn("Technician", "Technician");
//            _technician.ReadOnly = true;

//            _date = grid.AddColumn("SampleDate", "Sample Date");
//            _date.ReadOnly = true;
//        }

//        SamplePoint GetSamplePoint(string pointDescription)
//        {
//            var q = EntityManager.CreateQuery(SamplePoint.EntityName);
//            q.AddEquals(SamplePointPropertyNames.Description, pointDescription.Trim());
//            return EntityManager.Select(q).GetFirst() as SamplePoint;
//        }

//        VersionedAnalysis GetAnalysis(string analysis)
//        {
//            var q = EntityManager.CreateQuery(VersionedAnalysis.EntityName);
//            q.AddEquals(VersionedAnalysisPropertyNames.Identity, analysis.Trim());
//            q.AddOrder(VersionedAnalysisPropertyNames.AnalysisVersion, false);
//            return EntityManager.Select(q).GetFirst() as VersionedAnalysis;
//        }

//        VersionedComponent GetComponent(string analysis, string component)
//        {
//            var q = EntityManager.CreateQuery(VersionedComponent.EntityName);
//            q.AddEquals(VersionedComponentPropertyNames.Analysis, analysis);
//            q.AddEquals(VersionedComponentPropertyNames.VersionedComponentName, component.Trim());
//            q.AddOrder(VersionedComponentPropertyNames.AnalysisVersion, false);
//            return EntityManager.Select(q).GetFirst() as VersionedComponent;
//        }

//        Customer GetCustomer(string customer)
//        {
//            return EntityManager.Select(CustomerBase.EntityName, new Identity(customer)) as Customer;
//        }

//        private void BtnLoad_Click(object sender, EventArgs e)
//        {
//            var samples = _form.txtSampleIds.Text.Split(';').ToList<object>();

//            var q = EntityManager.CreateQuery(WebSampleQueueBase.EntityName);
//            q.AddIn(WebSampleQueuePropertyNames.SampleCode, samples);
//            var col = EntityManager.Select(q);
//            if (col.Count == 0)
//            {
//                Library.Utils.FlashMessage("No samples found.", "");
//                return;
//            }

//            var grid = _form.gridQueueSamples;
//            grid.BeginUpdate();
//            grid.ClearRows();
//            BuildColumns();

//            foreach (WebSampleQueueBase sample in col)
//            {
//                var qSample = new QueueSample();

//                var row = grid.AddRow();
//                row.Tag = qSample;

//                row.SetValue(_include, true);
//                _include.ReadOnly = false;

//                row.SetValue(_customer, sample.Customer);
//                qSample.Customer = GetCustomer(sample.Customer);
//                if (qSample.Customer == null)
//                {
//                    _customer.SetCellBackgroundColor(row, _red);
//                    row.SetValue("Include", false);
//                    _include.ReadOnly = true;
//                }

//                row.SetValue(_sampleId, sample.SampleCode);
//                qSample.SampleId = sample.SampleCode;

//                row.SetValue(_samplePoint, sample.SamplePoint);
//                qSample.SamplePoint = GetSamplePoint(sample.SamplePoint);
//                if (qSample.SamplePoint == null) _samplePoint.SetCellBackgroundColor(row, _red);

//                row.SetValue(_description, sample.SampleDescription);
//                qSample.Description = sample.SampleDescription;

//                row.SetValue(_analysis, sample.TestCode);
//                qSample.Analysis = GetAnalysis(sample.TestCode);
//                if (qSample.Analysis == null)
//                {
//                    _analysis.SetCellBackgroundColor(row, _red);
//                    row.SetValue("Include", false);
//                    _include.ReadOnly = true;
//                }

//                row.SetValue(_component, sample.ComponentName);
//                qSample.Component = GetComponent(sample.TestCode, sample.ComponentName);
//                qSample.OrderNumber = (int)qSample.Component.OrderNumber;
//                if (qSample.Component == null)
//                {
//                    _component.SetCellBackgroundColor(row, _red);
//                    row.SetValue("Include", false);
//                    _include.ReadOnly = true;
//                }

//                row.SetValue(_technician, sample.SamplingTech);
//                qSample.Technician = sample.SamplingTech;

//                row.SetValue(_date, sample.SamplingDatetime);
//                qSample.SampleDate = sample.SamplingDatetime;
//            }

//            grid.EndUpdate();
//        }

//        private void BtnLogin_Click(object sender, EventArgs e)
//        {
//            // Web samples with include checked
//            var qSamples = _form.gridQueueSamples.Rows.Where(r => ((bool)r.GetValue(_include.Name))).Select(r => r.Tag).Cast<QueueSample>().ToList();
//            if (qSamples.Count == 0)
//                return;

//            // Make parent job
//            var bag = _jobWorkflow.Perform();
//            JobHeader job;
//            var entities = bag.GetEntities(JobHeader.EntityName);
//            if(entities.Count == 0)
//            {
//                Library.Utils.FlashMessage($"Workflow {_jobWorkflow.WorkflowName} failed.", "Error");
//                return;
//            }
//            job = entities[0] as JobHeader;
//            if(job == null)
//            {
//                Library.Utils.FlashMessage($"Workflow {_jobWorkflow.WorkflowName} failed.", "Error");
//                return;
//            }

//            // Group on sample ID from the file and loop through tests
//            var samples = EntityManager.CreateEntityCollection(Sample.EntityName);
//            foreach (var sampleGroup in qSamples.GroupBy(s => s.SampleId))
//            {
//                // Make the sample
//                bag = _sampleWorkflow.Perform();
//                Sample sample;
//                entities = bag.GetEntities(Sample.EntityName);
//                if (entities.Count == 0)                
//                    continue;                
//                sample = entities[0] as Sample;
//                if (sample == null)
//                    continue;

//                var qItem = sampleGroup.First();
//                sample.JobName = job;
//                job.CustomerId = qItem.Customer;
//                sample.CustomerId = qItem.Customer;
//                sample.ImSampleRefId = qItem.SampleId;
//                sample.SamplingPoint = qItem.SamplePoint;
//                sample.Description = qItem.Description;
//                sample.DescriptionB = qItem.Technician;
//                sample.SampledDate = qItem.SampleDate;

//                // Group on the analysis, add tests to sample
//                foreach (var testGroup in sampleGroup.GroupBy(s => s.Analysis))
//                {
//                    // TODO - only add analysis, technician selects component list
//                    var test = sample.AddTest(testGroup.Key)[0];
//                    test.ComponentList = string.Empty;
//                    test.HasResultList = true;

//                    // Add result component
//                    foreach(var qResult in testGroup.OrderBy(r => r.OrderNumber).GroupBy(t => t.Component))
//                    {
//                        var result = test.AddResult(qResult.First().Component);
//                        result.ImResultRefId = qItem.SampleId;
//                        result.SetStatus(PhraseReslStat.PhraseIdU);
//                    }
//                }
//                samples.Add(sample);
//                EntityManager.Transaction.Add(sample);
//            }
//            EntityManager.Commit();

//            if (samples.Count == 0)
//                Library.Utils.FlashMessage($"Workflow {_sampleWorkflow.WorkflowName} failed.", "Error");
//            else
//                // Open login form
//                Library.Task.CreateTask(35127, job);
//        }
//    }

//    public class QueueSample
//    {
//        public string SampleId = "";
//        public VersionedAnalysis Analysis;
//        public VersionedComponent Component;
//        public int OrderNumber;
//        public Customer Customer;
//        public SamplePoint SamplePoint;
//        public string Technician = "";
//        public NullableDateTime SampleDate;
//        public string Description = "";
//    }
//}
