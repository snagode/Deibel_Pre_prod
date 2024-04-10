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

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(FtpUnmappedInteractiveTask))]
    public class FtpUnmappedInteractiveTask : SampleManagerTask
    {
        FormFtpUnmapped _form;
        ReportManager _rptManager;
        FtpUnmappedUtils _ftpUtils;

        protected override void SetupTask()
        {
            _form = FormFactory.CreateForm<FormFtpUnmapped>();
            _form.Loaded += _form_Loaded;
            _form.Show();
        }

        private void _form_Loaded(object sender, EventArgs e)
        {
            _form.gridFtpSample.SelectionChanged += ExplorerGrid1_SelectionChanged;
            _form.btnPreview.Click += BtnPreview_Click;
            _form.btnEmail.Click += BtnEmail_Click;

            _rptManager = new ReportManager(Library);
            _ftpUtils = new FtpUnmappedUtils(Library, EntityManager);

            _form.pebPrinter.Entity = _ftpUtils.DefaultPrinter;

            _form.ebFtpSample.Republish(_ftpUtils.ReportEntities);
        }

        private void BtnEmail_Click(object sender, EventArgs e)
        {
            if (_ftpUtils.DefaultPrinter == null)
            {
                Library.Utils.FlashMessage("Please enter a Printer ID into configuration item FTP_NOT_MAPPED_PRINTER", "Error");
                return;
            }

            if (_ftpUtils.DefaultTemplate == null)
            {
                Library.Utils.FlashMessage("Please enter a Report Template ID into configuration item FTP_NOT_MAPPED_REPORT", "Error");
                return;
            }

            if (_ftpUtils.DefaultCriteria == null)
            {
                Library.Utils.FlashMessage("Please enter a Report Template ID into configuration item FTP_NOT_MAPPED_CRITERIA", "Error");
                return;
            }

            // Get current printer
            var printer = _form.pebPrinter.Entity as PrinterInternal;

            _form.SetBusy("Processing email...");
            _rptManager.SendReport(_ftpUtils.ReportEntities, _ftpUtils.DefaultTemplate, printer);
            _form.ClearBusy();
        }

        private void BtnPreview_Click(object sender, EventArgs e)
        {
            if (_ftpUtils.DefaultTemplate == null)
            {
                Library.Utils.FlashMessage("Please enter a Report Template ID into configuration item FTP_NOT_MAPPED_REPORT", "Error");
                return;
            }
            
            if (_ftpUtils.DefaultCriteria == null)
            {
                Library.Utils.FlashMessage("Please enter a Report Template ID into configuration item FTP_NOT_MAPPED_CRITERIA", "Error");
                return;
            }

            _rptManager.PreviewReport(_ftpUtils.ReportEntities, _ftpUtils.DefaultTemplate);
        }

        private void ExplorerGrid1_SelectionChanged(object sender, ExplorerGridSelectionChangedEventArgs e)
        {
            if (e == null || e.Selection.Count == 0 || e.Selection.ActiveItems[0].EntityType == "FTP_TEST")
                return;

            // Fill samples grid
            var list = e.Selection.Cast<FtpSampleBase>().ToList();
            var tests = list.SelectMany(t => t.FtpTests.Cast<FtpTestBase>());

            var col = EntityManager.CreateEntityCollection(FtpTestBase.EntityName);
            col.AddCollection(tests);

            _form.ebFtpTest.Republish(col);
        }
    }
}
