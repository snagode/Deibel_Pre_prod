using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common;
using Thermo.SampleManager.Common.CommandLine;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Utilities;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;

namespace Customization.Tasks
{
    /// <summary>
    /// Class to import the Web Queue files into SampleManager
    /// </summary>
    [SampleManagerTask(nameof(PushFilesLocalToAzure))]

    public class PushFilesLocalToAzure : SampleManagerTask, IBackgroundTask
    { 
        int _itemsAdded;
        int transId = -1;
        protected override void SetupTask()
        {
            // Check if this is run in background, i.e. executed by timer queue service
            if (Library.Environment.IsBackground())
                return;
            if (Library.Environment.IsInteractive())
            {
                Launch();
            }
        }
        public void Launch()
        {
              PushFileFromLocalToServer();
        }
        /// <summary>
        /// The following method makes the scheduler run every five minutes while pushing files from the server directory to the local computer directory.
        /// Scheduler  : The PushFilesLocalToAzure scheduler was created in the sample manager with a 5-minute timer value.
        /// Azure Path : \\10.2.1.22\FileQueue\Incoming. This path is defined in the regedit key AS smp$dbl_AzureIncomingDirPath.
        /// Local Path : C:\Thermo\SampleManager\Server\SMUAT\Web_Queue\EXPORT.This path is defined in the regedit key AS smp$dbl_LocalToAzure_Export.
        /// </summary>
        private void PushFileFromLocalToServer()
        {
            string logFilePath = Library.Environment.GetFolderList("smp$dbl_DevLogFilesPath") + "\\LocalToAzureFileMove";
            //@"C:\Thermo\SampleManager\Server\SMUAT\Web_Queue\EXPORT\Logs\";
            try
            {
                Common.WriteLog(logFilePath, $"{DateTime.Now.ToShortTimeString()}:File Moving Start Local To Azure.");
                string AzurePath = Library.Environment.GetFolderList("smp$dbl_AzureIncomingDirPath").ToString();
                string LocalPath = Library.Environment.GetFolderList("smp$dbl_LocalToAzure_Export").ToString();
                using (new NetworkConnection(AzurePath, new System.Net.NetworkCredential("GuardianUser", "kljds@sj!slOp!bB")))
                {
                    string[] files = Directory.GetFiles(LocalPath, ".", SearchOption.TopDirectoryOnly);
                    if (files.Length > 0)
                    {
                        foreach (var newPath in Directory.GetFiles(LocalPath, ".", SearchOption.TopDirectoryOnly))
                        {
                            FileInfo info = new FileInfo(newPath);
                            if (!File.Exists(AzurePath + "\\" + info.Name))
                            {
                                File.Move(newPath, newPath.Replace(LocalPath, AzurePath));
                            }
                            else
                            {
                                File.Move(newPath, newPath.Replace(LocalPath, AzurePath + "\\" + "Dublicate"));
                            }
                        }

                        Common.WriteLog(logFilePath, $"{DateTime.Now.ToShortTimeString()}:File Upload Completed.\r\n");
                    }
                    else
                    {
                        Common.WriteLog(logFilePath, $"{DateTime.Now.ToShortTimeString()}:File Does Not Exist in the Export Directory.\r\n");
                    }
                }
            }
            catch (Exception ex) { Common.WriteLog(logFilePath, $"{DateTime.Now.ToShortTimeString()}:Error" + ex.Message + ex.StackTrace); }
        }
    }
}
