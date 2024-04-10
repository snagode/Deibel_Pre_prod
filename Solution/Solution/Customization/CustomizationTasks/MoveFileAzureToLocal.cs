using System;
using System.Collections.Generic;
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
    [SampleManagerTask(nameof(MoveFileAzureToLocal))]
    public class MoveFileAzureToLocal : SampleManagerTask, IBackgroundTask
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
              PushFileFromServerToLocal();
              //PushFileFromLocalToServer();
        }
        /// <summary>
        /// The following method makes the scheduler run every five minutes while pushing files from the server directory to the local computer directory.
        /// Scheduler  : The MoveFileAzureToLocal scheduler was created in the sample manager with a 5-minute timer value.
        /// Azure Path : \\10.2.1.22\FileQueue\Outgoing. This path is defined in the regedit key AS smp$dbl_AzureFile_Import.
        /// Local Path : C:\Thermo\SampleManager\Server\SMUAT\Matrix_Queue\IMPORT.This path is defined in the regedit key AS smp$dbl_web_import.
        ///</summary>
        private void PushFileFromServerToLocal()
        {
            string logFilePath = Library.Environment.GetFolderList("smp$dbl_DevLogFilesPath") + "\\MoveFilesAzureToLocal";
            try
            { 
                Common.WriteLog(logFilePath, $"{DateTime.Now.ToShortTimeString()}:File Moving Start Azure To Local."); 
                string AzurePath = Library.Environment.GetFolderList("smp$dbl_AzureFile_Import").ToString();
                string LocalPath = Library.Environment.GetFolderList("smp$dbl_Matrix_import").ToString();//+ "\\" + "A";
             
                using (new NetworkConnection(AzurePath, new System.Net.NetworkCredential("GuardianUser", "kljds@sj!slOp!bB")))
                { 
                    if (Directory.GetFiles(AzurePath, ".", SearchOption.TopDirectoryOnly).Length > 0)
                    {
                        foreach (var newPath in Directory.GetFiles(AzurePath, ".", SearchOption.TopDirectoryOnly))
                        {
                            FileInfo info = new FileInfo(newPath);
                            if (!File.Exists(LocalPath + "\\" + info.Name))
                            {
                                File.Move(newPath, newPath.Replace(AzurePath, LocalPath));
                            }
                            else
                            {
                                File.Move(newPath, newPath.Replace(AzurePath, LocalPath + "\\" + "Dublicate"));
                                //Library.Utils.FlashMessage("File already exists", "File already exists");
                            }
                        }
                        Common.WriteLog(logFilePath,$"{DateTime.Now.ToShortTimeString()}:File Upload Completed.\r\n");
                    }
                    else
                    { 
                      Common.WriteLog(logFilePath, $"{DateTime.Now.ToShortTimeString()}:File Does Not Exist in the Outgoing Directory.\r\n");
                    }
                }
            }
            catch (Exception ex) {Common.WriteLog(logFilePath, $"{DateTime.Now.ToShortTimeString()}:Error"+ ex.Message + ex.StackTrace);}
        }
    }
}
