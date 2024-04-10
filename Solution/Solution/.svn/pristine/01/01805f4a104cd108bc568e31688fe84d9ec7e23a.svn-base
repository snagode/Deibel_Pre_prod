using Customization.ObjectModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Integration;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Customization.Tasks
{
    [SampleManagerTask("IMReportingTask", "WorkflowCallback")]
    public class IMReportingTask : SampleManagerTask
    {
        List<JobHeader> _jobs = new List<JobHeader>();
        List<Sample> _samples = new List<Sample>();
        List<Test> _tests = new List<Test>();
        bool _workflowCallback = false;
        string _selectedEntityType;

        protected override void SetupTask()
        { 
            if (Context == null || Context.SelectedItems == null ||Context.SelectedItems.Count == 0)
                return;

            _workflowCallback = Context.MenuItem == null;
            _selectedEntityType = Context.SelectedItems.EntityType;

            // Check if ftp sending is disabled
            bool sendResults = Library.Environment.GetGlobalBoolean("DEIBEL_FTP_EXPORT_ENABLED");
            if (!sendResults)
            {
                if (!_workflowCallback)
                {
                    Library.Utils.FlashMessage("FTP result export is currently disabled.", "");
                }
                return;
            }
            if (_selectedEntityType == JobHeader.EntityName)
            {
                _jobs = Context.SelectedItems.Cast<JobHeader>().ToList();
                _samples = _jobs.SelectMany(j => j.Samples.ActiveItems).Cast<Sample>().ToList();
                _tests = _samples.SelectMany(s => s.Tests.ActiveItems).Cast<Test>().ToList();

                if (!ValidJobs(_jobs) || !ValidSamples(_samples))
                    return;
            }
            else if (_selectedEntityType == Sample.EntityName)
            {
                _samples = Context.SelectedItems.Cast<Sample>().ToList();
                _tests = _samples.SelectMany(s => s.Tests.ActiveItems).Cast<Test>().ToList();

                if (!ValidSamples(_samples))
                    return;

                // List of distinct jobs
                var sampJobs = _samples.GroupBy(s => s.JobName).Select(s => s.First()).ToList();
                _jobs = sampJobs.Select(s => s.JobName).Cast<JobHeader>().ToList();
            }
            else if (_selectedEntityType == Test.EntityName)
            {
                _tests = Context.SelectedItems.Cast<Test>().ToList();

                // Make sure they're in a job
                foreach (var test in _tests)
                {
                    if (string.IsNullOrWhiteSpace(test.ParentJobName))
                    {
                        if (!_workflowCallback)
                            Library.Utils.FlashMessage("One or more selected tests is not part of a job.  Only samples in jobs can be sent via FTP.", "Invalid Selection");

                        return;
                    }
                }
                // Distinct samples
                var testSamps = _tests.GroupBy(t => t.Sample).Select(t => t.First()).ToList();
                _samples = testSamps.Select(t => t.Sample).Cast<Sample>().ToList();

                // Distinct jobs
                var sampJobs = _samples.GroupBy(s => s.JobName).Select(s => s.First()).ToList();
                _jobs = sampJobs.Select(s => s.JobName).Cast<JobHeader>().ToList();
            }
            
            // Write XML and then export data to IM
            foreach (var job in _jobs)
            {
                if(job.CustomerId == null || string.IsNullOrWhiteSpace(job.CustomerId.Identity) )
                {
                    if (!_workflowCallback)
                        Library.Utils.FlashMessage($"No customer is assigned to job {job.JobName}", "Invalid Selection");

                    continue;
                }
                var samples = _samples.Where(s => s.JobName == job).ToList();
                var tests = _tests.Where(t => t.ParentJobName == job.JobName).ToList();
                if (samples.Count == 0 || tests.Count == 0)
                    continue;

                var writer = new DeibelXmlWriter(EntityManager, job.CustomerId as Customer);
                var xml = writer.GetOutboundXML(job, samples, tests);
                if (xml == string.Empty)
                    continue;
                if (ExportToIM(xml))
                    UpdateEntities(writer.EntitiesSent);
            }
            if (_workflowCallback)
                Exit();
        }

        #region Create Report
        
        bool ExportToIM(string xml)
        {
            bool bSendOK = false;
            List<string> sCheckGlobals = new List<string>();

            DataItemIntegration im = new DataItemIntegration();
            DataItemList imdilist = new DataItemList("SampleManagerExport");
            DataItemXmlIgnore imdi = new DataItemXmlIgnore("Item");
            var m_imdii = new DataItemIntegration(imdi, new System.Xml.Serialization.XmlAnyElementAttribute().ToString());

            if (!Library.Environment.CheckGlobalExists("INTEGRATION_MANAGER_SERVER_URL")) sCheckGlobals.Add("INTEGRATION_MANAGER_SERVER_URL");
            if (!Library.Environment.CheckGlobalExists("INTEGRATION_MANAGER_INSTANCE")) sCheckGlobals.Add("INTEGRATION_MANAGER_INSTANCE");
            if (!Library.Environment.CheckGlobalExists("INTEGRATION_MANAGER_USERNAME")) sCheckGlobals.Add("INTEGRATION_MANAGER_USERNAME");
            if (!Library.Environment.CheckGlobalExists("INTEGRATION_MANAGER_PASSWORD")) sCheckGlobals.Add("INTEGRATION_MANAGER_PASSWORD");
            if (sCheckGlobals.Count > 0)
            {
                Library.Utils.FlashMessage("You are missing the following global configuration items: " + String.Join(", \n", sCheckGlobals), "Missing configuration items", MessageButtons.OK, MessageIcon.Error, MessageDefaultButton.Button1);
            }

            m_imdii.URL = Library.Environment.GetGlobalString("INTEGRATION_MANAGER_SERVER_URL");
            m_imdii.InstanceName = Library.Environment.GetGlobalString("INTEGRATION_MANAGER_INSTANCE");
            m_imdii.Username = Library.Environment.GetGlobalString("INTEGRATION_MANAGER_USERNAME");
            m_imdii.Password = Library.Environment.GetGlobalString("INTEGRATION_MANAGER_PASSWORD");
            m_imdii.ObjectType = "SampleManager Data";

            imdi.Set("SMExportDataXML", xml);

            imdilist.Set("Operator", ((Personnel)(Library.Environment.CurrentUser)).Identity.ToString());
            imdilist.ItemsName = "Items";
            imdilist.Items.Add(imdi);

            m_imdii.DataItem = imdilist;

            bSendOK = m_imdii.Send();

            if (bSendOK)
                return true;
            else
            {
                Library.Utils.FlashMessage("There was a problem sending to IntegrationManager: " + m_imdii.LastException.Message, "Failed to send to IM");
                return false;
            }
        }

        #endregion

        #region Utility Methods

        bool ValidJobs(List<JobHeader> jobs)
        {
            foreach (var job in jobs)
            {
                // Make sure it has a sample
                if (job.Samples.Count == 0)
                {
                    if (!_workflowCallback)
                        Library.Utils.FlashMessage($"Selected job {job.JobName} contains no samples.", "Invalid Selection");

                    return false;
                }

                if (!ValidSamples(job.Samples.Cast<Sample>().ToList()))
                    return false;

                // Make sure the jobs all have a customer assigned to them
                if (job.CustomerId == null)
                {
                    if (!_workflowCallback)
                        Library.Utils.FlashMessage("One or more selected jobs does not have a customer assigned to it.  Only jobs with a customer can be sent via FTP.", "Invalid data");

                    return false;
                }
                // Make sure all the samples' customer match the customer on this job
                else
                {
                    var samples = job.Samples.Cast<Sample>().ToList();
                    var countInvalidCustomers = samples.Where(c => c.CustomerId != job.CustomerId).Count();
                    if (countInvalidCustomers > 0)
                    {
                        if (!_workflowCallback)
                            Library.Utils.FlashMessage("One or more samples has a customer assigned that is different from the parent job's customer.", "Invalid data");

                        return false;
                    }
                }

                if(job.CustomerId.FtpResultConfig.PhraseId == PhraseFtpConfig.PhraseIdNONE 
                    || job.CustomerId.FtpReportingLevel.PhraseId == PhraseDlFtpLvl.PhraseIdNONE 
                    || string.IsNullOrWhiteSpace(job.CustomerId.FtpReportingLevel.PhraseId))
                {
                    if (!_workflowCallback)
                        Library.Utils.FlashMessage("One or more selected jobs has a customer with result export disabled.", "");

                    return false;
                }
            }
            return true;
        }

        bool ValidSamples(List<Sample> samples)
        {
            foreach (var sample in samples)
            {
                // Make sure all samples got a job assigned
                if (sample.JobName == null || string.IsNullOrWhiteSpace(sample.JobName.JobName))
                {
                    if (!_workflowCallback)
                        Library.Utils.FlashMessage("One or more selected samples is not part of a job.  Only samples in jobs can be sent via FTP.", "Invalid Selection");

                    return false;
                }

                // Check customer configuration
                var cust = sample.CustomerId;
                if (cust.FtpReportingLevel.PhraseId == PhraseDlFtpLvl.PhraseIdNONE 
                    || cust.FtpResultConfig.PhraseId == PhraseFtpConfig.PhraseIdNONE
                    || string.IsNullOrWhiteSpace(cust.FtpReportingLevel.PhraseId))
                {
                    if (!_workflowCallback)
                        Library.Utils.FlashMessage("One or more selected samples is assigned a customer that does not receive results from FTP server.", "Invalid Selection");

                    return false;
                }
            }
            return true;
        }

        void UpdateEntities(List<IEntity> entities)
        {
            var now = Library.Environment.ClientNow;

            string user;
            if (_workflowCallback)
                user = "WORKFLOW";
            else
                user = ((Personnel)Library.Environment.CurrentUser).Identity;

            foreach (var entity in entities)
            {
                if (entity == null)
                    continue;

                switch (entity.EntityType)
                {
                    case JobHeader.EntityName:
                        var jh = entity as JobHeader;
                        jh.FtpDateSent = now;
                        jh.FtpSentBy = user;
                        break;

                    case Sample.EntityName:
                        var s = entity as Sample;
                        s.FtpDateSent = now;
                        s.FtpSentBy = user;
                        break;

                    case Test.EntityName:
                        var t = entity as Test;
                        t.FtpDateSent = now;
                        t.FtpSentBy = user;
                        break;
                }
                EntityManager.Transaction.Add(entity);
            }

            if (!_workflowCallback)
            {
                EntityManager.Commit();
            }
        }

        #endregion

    }


}
