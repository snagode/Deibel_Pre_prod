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
    [SampleManagerTask(nameof(LindtReportMoveToArchive))]
    public class LindtReportMoveToArchive : SampleManagerTask, IBackgroundTask
    {
        int transId = -1;
        protected override void SetupTask()
        {
            LogInfo("Start");
         
            // Check if this is run in background, i.e. executed by timer queue service
            if (Library.Environment.IsBackground())
                return;
            if (Library.Environment.IsInteractive())
            {
                Launch();
            }

            // If it's not from timer queue, the intention is to debug and this is called
            // from a menu item.  
            LogInfo($"{DateTime.Now}: Begin Archive File Functionality -  Start.");

           

        }

        public void Launch()
        {
            LogInfo($"{DateTime.Now}: Begin Archive FileLaunch Method Start.");
            try
            {
                var path = Library.Environment.GetFolderList("smp$dbl_Lendit_Report");
                
                var files = Library.File.GetFiles(path);
                LogInfo(files.Count.ToString());
                if (files.Count == 0)
                {
                    LogInfo($"{DateTime.Now}:There is no file to Archive.");
                    return;
                }
                foreach(var file in files)
                {
                    transId++;
                    LogInfo(files.Count.ToString());
                   // var destination = @"D:\inetpub\ftproot\LocalUser\LindtArchiveFileTEstAvinash";
                    File.Move(file.FullName, "smp$dbl_Lendit_ArchiveReport"+transId+".csv");
                   // File.Move(file.FullName, destination + ".csv");
               
                    LogInfo("file moved is" + file.FullName+file.DirectoryName);
                }
           }

            catch (Exception e) { LogInfo(e.Message + e.StackTrace); }
            LogInfo($"{DateTime.Now}: Archiving of file complete.\r\n");
      }

       
        public void LogInfo(string message)
        {
            // The first parameter, smp$userfiles, is in registry at HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\LabSystems\SampleManager Server\SMUAT
            var file = Library.File.GetWriteFile("Smp$LenditArchiveLogFileLog", "LenditArchiveReport.txt");
            File.AppendAllText(file.FullName, message + "\r\n");
        }

        DateTime GetDate(string datestring, string timestring)
        {
            // "12/18/2019","11:42 AM"

            var date = DateTime.ParseExact(datestring, @"MM/dd/yyyy", null);
            var hour = int.Parse(timestring.Substring(0, 2));
            hour = timestring.Contains("AM") ? hour : hour + 12;
            date.AddHours(hour);
            date.AddMinutes(int.Parse(timestring.Substring(3, 2)));
            return date;
        }

    }
}
