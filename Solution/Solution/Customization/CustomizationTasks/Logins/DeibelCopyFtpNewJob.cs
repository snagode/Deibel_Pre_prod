using Customization.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(DeibelCopyFtpNewJob))]
    public class DeibelCopyFtpNewJob : ExtendedJobLoginBase
    {
        protected override string TopTableName => JobHeader.EntityName;
        protected override bool JobWorkflow => true;
        protected override string Title => "Add Samples New Job";

        List<JobHeader> _rootJobs = new List<JobHeader>();
        bool _exit;

        Workflow _sampleWorkflow;
        Workflow _initialWorkflow;
        protected override Workflow InitialWorkflow
        {
            get
            {
                if (_initialWorkflow == null)
                {
                    string wfName = "";
                    if (Context.TaskParameters.Count() > 1)
                        wfName = Context.TaskParameters[1];

                    _initialWorkflow = Utils.GetLoginWorkflow(TopTableName, true, wfName);
                    if (_initialWorkflow == null)
                    {
                        _exit = true;
                        Exit();
                    }
                }
                return _initialWorkflow;
            }
        }

        protected override void MainFormLoaded()
        {
            base.MainFormLoaded();

            if (_exit)
            {
                Exit();
                return;
            }

            // Fill customer from FTP sample
            foreach (var job in _rootJobs)
            {
                job.CustomerId = job.Samples[0].Get(SamplePropertyNames.CustomerId) as CustomerBase;
                Utils.Refresh(job, JobHeaderPropertyNames.CustomerId, null);
            }
        }

        protected override IEntityCollection TopEntities()
        {
            var col = EntityManager.CreateEntityCollection(TopTableName);
            
            // Get distinct customers
            var allRequests = Context.SelectedItems.Cast<FtpSampleBase>();
            var customerIds = allRequests.Select(c => c.CustomerId.Identity).Distinct().ToList();

            // One job per cusotmer
            string wfName = "";
            if (Context.TaskParameters.Count() > 2)
                wfName = Context.TaskParameters[2];
            _sampleWorkflow = Utils.GetLoginWorkflow(Sample.EntityName, true, wfName);
            if (_sampleWorkflow == null)
            {
                _exit = true;
                Exit();
                return null;
            }
            foreach (var customer in customerIds)
            {
                var requests = allRequests.Where(c => c.CustomerId.Identity == customer).ToList();
                var j = Utils.GetNewEntity(InitialWorkflow, TopTableName) as JobHeader;
                _rootJobs.Add(j);

                // Add selected samples to it
                var samples = FtpSamples(requests);
                if (_exit)
                    break;
                foreach (Sample sample in samples)
                {
                    j.Samples.Add(sample);
                }
                col.Add(j);
            }
            return col;
        }

        List<Sample> FtpSamples(List<FtpSampleBase> ftpSamples)
        {
            var items = ftpSamples;
            Customer customer = null;

            // Prompt for Mars customer selection if it's a Mars transaction

            if (items.Where(c => c.CustomerId.Identity == "MS_FTP").Count() > 0)
            {
                var qualifiers = new List<string>();
                var filter = Library.Environment.GetGlobalString("DEIBEL_FTP_MARS_FILTER");
                if (string.IsNullOrWhiteSpace(filter))
                {
                    qualifiers.Add("MS%");
                }
                else
                {
                    qualifiers = filter.Split(',').ToList();
                }

                var q = EntityManager.CreateQuery(Customer.EntityName);
                // Loop through qualifiers
                var count = 1;
                var size = qualifiers.Count;
                foreach (string s in qualifiers)
                {
                    var t = s.Trim();
                    q.AddLike(CustomerPropertyNames.Identity, t);
                    if (count < size)
                        q.AddOr();
                    count++;
                }
                IEntity e;
                Library.Utils.PromptForEntity("Select Mars Customer", "Select Mars Customer", q, out e);
                customer = e as Customer;
                if (customer == null || customer.IsNull())
                {
                    _exit = true;
                    return new List<Sample>(); ;
                }
            }

            // Order the samples on FTP sample id ascending
            var ftpSamps = ftpSamples.OrderBy(s => s.Identity).ToList();

            // If customer was selected from a prompt, assign selected customer to ftp_sample
            // Commit will occur on sample login
            if (customer != null)
            {
                foreach (var sample in ftpSamps)
                    sample.SelectedCustomer = customer;
            }
            return ConvertFtpSamples(ftpSamps, customer);
        }

        List<Sample> ConvertFtpSamples(List<FtpSampleBase> ftpSamples, Customer customer)
        {
            var samples = new List<Sample>();
            foreach (var ftp in ftpSamples)
            {
                var s = Utils.GetNewEntity(_sampleWorkflow, Sample.EntityName) as Sample;
                s.FtpTransaction = ftp;

                // If it's a Mars customer, customer is selected at prompt, otherwise comes from FTP
                s.CustomerId = customer ?? ftp.CustomerId;

                // Copy values from mapped fields
                var fieldMap = s.CustomerId.CustomerXmlInbounds.Cast<CustomerXmlInboundBase>().ToList();
                foreach (var field in fieldMap.Where(m => m.TableName == "SAMPLE" && m.TableFieldName != "CUSTOMER_ID" && !string.IsNullOrWhiteSpace(m.TableFieldName)))
                {
                    try
                    {
                        // Get value from Ftp_sample entity using ftp field name
                        var value = ((IEntity)ftp).Get(field.FtpFieldName);

                        // Set the value on sample entity using table field name
                        if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                            ((IEntity)s).Set(field.TableFieldName, value);
                    }
                    catch { }
                }

                var customerComponents = s.CustomerId.CustomerComponents.Cast<CustomerComponentsBase>().ToList();
                var analysesProcessed = new List<VersionedAnalysis>();

                var ftpTests = ftp.FtpTests.Cast<FtpTest>().ToList();
                foreach (var ftpTest in ftpTests)
                {
                    var analysis = ftpTest.Analysis;
                    if (analysis == null)
                    {
                        continue;
                    }

                    // Don't add analyses that aren't analysis_order == 1
                    if (!FirstOrderAnalysis(analysis, customerComponents))
                        continue;

                    // All copies of analysis are added first time we process it
                    if (analysesProcessed.Contains(analysis))
                        continue;
                    analysesProcessed.Add(analysis);

                    // List containing only this analysis
                    var copies = ftpTests.Where(a => a.AnalysisAlias == ftpTest.AnalysisAlias).ToList();

                    // Max copies of a component
                    var modeComponent = copies.GroupBy(a => a.ComponentAlias).OrderByDescending(g => g.Count()).First().Key;
                    var mode = copies.Where(c => c.ComponentAlias == modeComponent).Count();

                    // Add that many copies of the analysis                    
                    for (int i = 1; i <= mode; i++)
                    {
                        var tests = s.AddTest(analysis, EntityManager);
                        if (tests.Count == 0)
                            continue;

                        var t = tests[0] as Test;
                        t.Analysis = analysis;

                        // Copy values from mapped fields
                        var mappedFields = fieldMap
                            .Where(m => m.TableName == "TEST"
                            && m.TableFieldName != "ANALYSIS"
                            && !string.IsNullOrWhiteSpace(m.TableFieldName))
                            .ToList();

                        foreach (var field in mappedFields)
                        {
                            try
                            {
                                // Get value from Ftp_test entity using ftp field name
                                var value = ((IEntity)ftpTest).Get(field.FtpFieldName);

                                // Set the value on test entity using table field name
                                if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                                    ((IEntity)t).Set(field.TableFieldName, value);
                            }
                            catch { }
                        }

                        // Component list assignment from Customer
                        var componentList = s.CustomerId.CustomerComponents
                            .Cast<CustomerComponentsBase>()
                            .Where(c => c.Analysis == t.Analysis.Identity
                            && !string.IsNullOrWhiteSpace(c.ComponentList))
                            .Select(c => c.ComponentList)
                            .FirstOrDefault() ?? string.Empty;

                        t.ComponentList = componentList;
                    }
                }
                samples.Add(s);
            }
            return samples;
        }

        bool FirstOrderAnalysis(VersionedAnalysis analysis, List<CustomerComponentsBase> compMap)
        {
            var comp = compMap
                .Where(c => c.Analysis == analysis.Identity
                && c.AnalysisOrder == 1)
                .FirstOrDefault();

            // No mapped component for this analysis with order number = 1
            if (comp == null)
                return false;

            return true;
        }
    }
}
