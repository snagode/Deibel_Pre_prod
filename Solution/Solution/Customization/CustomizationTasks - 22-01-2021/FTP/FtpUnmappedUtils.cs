using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.Framework.Core;
using Customization.ObjectModel;
using Thermo.SampleManager.Server;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Common.CommandLine;
using Thermo.SampleManager.Library.DesignerRuntime;

namespace Customization.Tasks
{
    public class FtpUnmappedUtils
    {
        StandardLibrary Library;
        IEntityManager EntityManager;

        int _arbitraryId = 1;

        public ReportTemplate DefaultTemplate { get; }

        public IEntityCollection ReportEntities { get; set; }
        public PrinterInternal DefaultPrinter { get; }
        public CriteriaSaved DefaultCriteria { get; }

        ReportManager _rptManager;

        public FtpUnmappedUtils(StandardLibrary library, IEntityManager entityManager)
        {
            Library = library;
            EntityManager = entityManager;

            // Set the default values
            var reportId = Library.Environment.GetGlobalString("FTP_NOT_MAPPED_REPORT");
            var report = EntityManager.SelectLatestVersion(ReportTemplate.EntityName, reportId) as ReportTemplate;
            DefaultTemplate = report;

            var printerId = Library.Environment.GetGlobalString("FTP_NOT_MAPPED_PRINTER");
            var printer = EntityManager.Select(PrinterBase.EntityName, new Identity(printerId)) as PrinterInternal;
            DefaultPrinter = printer;

            var criteriaId = Library.Environment.GetGlobalString("FTP_NOT_MAPPED_CRITERIA");
            var criteria = EntityManager.Select(CriteriaSaved.EntityName, new Identity("FTP_SAMPLE", criteriaId)) as CriteriaSaved;
            DefaultCriteria = criteria;

            // Start with "default" set of entities
            ReportEntities = EntityManager.CreateEntityCollection(FtpSampleBase.EntityName);
            RefreshEntities(DefaultCriteria);
        }

        void SetDefaults()
        {
            _rptManager = new ReportManager(Library);
        }
  
        public void RefreshEntities(CriteriaSaved criteria)
        {
            // Use the saved criteria from report template to gather ftp entities
            var CriteriaService = Library.GetService<ICriteriaTaskService>();
            var q = CriteriaService.GetCriteriaQuery(DefaultCriteria);
            var samples = EntityManager.Select(FtpSampleBase.EntityName, q);
            if (samples.Count == 0)
                return;
            var ftpList = samples.Cast<FtpSampleBase>();

            // Selected_customer field has priority over customer_id field
            var sel = ftpList
                .Where(s => !s.SelectedCustomer.IsNull());
            AddFtpSamples(sel, true);

            // Ignore MS_FTP, only concerned with the selected customer
            var def = ftpList
                .Where(s => s.SelectedCustomer.IsNull()
                && s.CustomerId.Identity != "MS_FTP");
            AddFtpSamples(def, false);
        }


        void AddFtpSamples(IEnumerable<FtpSampleBase> samples, bool useSelectedCustomer)
        {
            // Distinct customers
            var ids = new List<CustomerBase>();
            if (useSelectedCustomer)
                ids = samples.Select(c => c.SelectedCustomer).Distinct().ToList();
            else
                ids = samples.Select(c => c.CustomerId).Distinct().ToList();

            // Loop through customers gathering all tests per customer
            foreach (var customer in ids)
            {
                // Each customer corresponds to 1 new FTP_SAMPLE
                var newFtp = EntityManager.CreateEntity(FtpSampleBase.EntityName, new Identity(_arbitraryId)) as FtpSampleBase;
                newFtp.CustomerId = customer;

                // All tests from samples of this customer
                var tests = new List<FtpTest>();
                if (useSelectedCustomer)
                    tests = samples
                        .Where(s => s.SelectedCustomer == customer)
                        .SelectMany(t => t.FtpTests.Cast<FtpTest>())
                        .Where(t => t.Component == null).ToList();
                else
                    tests = samples
                        .Where(s => s.CustomerId == customer)
                        .SelectMany(t => t.FtpTests.Cast<FtpTest>())
                        .Where(t => t.Component == null).ToList();

                // Remove entries with both analysis and component aliases = blank
                tests = tests.Where(t => !(string.IsNullOrWhiteSpace(t.AnalysisAlias) && string.IsNullOrWhiteSpace(t.ComponentAlias))).ToList();
                if (tests.Count == 0)
                    continue;

                // We want distinct tests
                var distinctTests = tests.GroupBy(t => new { t.AnalysisAlias, t.ComponentAlias }).Select(t => t.First()).ToList();

                // Add the tests to the new ftp sample
                foreach (var t in distinctTests)
                {
                    var newTest = EntityManager.CreateEntity(FtpTestBase.EntityName, new Identity(newFtp.Identity, _arbitraryId)) as FtpTestBase;
                    newTest.AnalysisAlias = t.AnalysisAlias;
                    newTest.ComponentAlias = t.ComponentAlias;
                    newFtp.FtpTests.Add(newTest);

                    _arbitraryId++;
                }

                // Add new sample to entity collection that will be passed to the report
                ReportEntities.Add(newFtp);
            }
        }
    }
}
