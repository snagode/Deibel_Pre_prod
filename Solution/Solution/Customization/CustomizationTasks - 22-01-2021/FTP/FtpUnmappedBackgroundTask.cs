using System;
using System.Linq;
using Thermo.SampleManager.Common.CommandLine;
using Thermo.SampleManager.Library;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(FtpUnmappedBackgroundTask))]
    public class FtpUnmappedBackgroundTask : SampleManagerTask, IBackgroundTask
    {
        public void Launch()
        {
            var rptManager = new ReportManager(Library);
            var ftpUtils = new FtpUnmappedUtils(Library, EntityManager);

            rptManager.SendReport(ftpUtils.ReportEntities, ftpUtils.DefaultTemplate, ftpUtils.DefaultPrinter, "Unmapped FTP Tests");
        }
    }
}
