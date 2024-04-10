using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Tasks;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(FtpSampleTask), "LABTABLE", "FTP_SAMPLE")]
    public class FtpSampleTask : GenericLabtableTask
    {
        FormFtpSample _form;
        FtpSampleBase _entity;

        protected override void MainFormLoaded()
        {
            if (MainForm.Entity == null)
                return;

            _entity = MainForm.Entity as FtpSampleBase;
            _form = MainForm as FormFtpSample;
            _form.btnTest.Click += BtnTest_Click;

            if(Context.LaunchMode == GenericLabtableTask.AddOption)
            {
                var transId = Library.Increment.GetIncrement("FTP_TRANSACTION", "KEY0");
                var id = Library.Increment.GetIncrement("FTP_SAMPLE", "KEY0");
                _entity.TransactionId = transId;
                _entity.Identity = id;
            }
            else
                LoadTransactionText();
        }

        private void BtnTest_Click(object sender, EventArgs e)
        {
            if (!Library.Utils.FlashMessageYesNo("Test FTP import using the LIMSML?", ""))
                return;

            var import = new FtpImportTask();
            import.Launch(EntityManager, Library);
        }

        void LoadTransactionText()
        {
            if(Context.SelectedItems.Count == 0 || Context.SelectedItems[0] == null)
            {
                return;
            }

            var e = Context.SelectedItems[0] as FtpSampleBase;

            // Build filename
            var path = $@"{Library.Environment.GetFolderList("smp$ftplimsml")}\Processed\{e.TransactionId}.xml";

            if (File.Exists(path))
                _form.txtLimsml.TextContent = File.ReadAllText(path);            
            else
                _form.txtLimsml.TextContent = "Error 404:  File not found.";

        }

        protected override bool OnPreSave()
        {
            var tests = _entity.FtpTests.Cast<FtpTestBase>().ToList();
            return base.OnPreSave();
        }
    }
}
