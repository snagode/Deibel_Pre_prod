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
    [SampleManagerTask(nameof(WebQueueImport))]
    public class WebQueueImport : SampleManagerTask, IBackgroundTask
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

            // If it's not from timer queue, the intention is to debug and this is called
            // from a menu item.  
            LogInfo($"{DateTime.Now}: Begin environment upload.");

            try
            {


                LogInfo($"Added {_itemsAdded} new entities.");
            }

            catch (Exception e) { LogInfo(e.Message + e.StackTrace); }
            LogInfo($"{DateTime.Now}: Upload complete.\r\n");

        }


        /// <summary>
        /// Called from SampleManager watchdog timer, aka timer queue service
        /// Timer queue is actually a windows service:  smptqsmuat
        /// </summary>
        public void Launch()
        {
            LogInfo($"{DateTime.Now}: Begin environment upload.");
            try
            {
                var path = Library.Environment.GetFolderList("smp$dbl_web_import");
                var files = Library.File.GetFiles(path);
                if (files.Count == 0)
                {
                    LogInfo($"{DateTime.Now}:There is no file to upload.");
                    return;
                }

                DoEverything(files);
                LogInfo($"Added {_itemsAdded} new entities.");
            }
            catch (Exception e) { LogInfo(e.Message + e.StackTrace); }
            LogInfo($"{DateTime.Now}: Upload complete.\r\n");

        }

        void DoEverything(IList<FileInfo> files)
        {

            //var files = Library.File.GetFiles("smp$dbl_web_import");
            foreach (var file in files)
            {
                //var filename = file.FullName;
                //filename = filename.Replace("_", "");
                //filename = filename.Substring(0, filename.Length - 4);


                var destination = "";

                if (file.Extension != ".csv" && file.Extension != ".txt")
                    continue;
                if (SaveWebSampleQueueFiles(file))
                {
                    destination = $@"{Library.Environment.GetFolderList("smp$dbl_web_import")}\Copy\{transId}.csv";

                }

                else
                {
                    destination = $@"{Library.Environment.GetFolderList("smp$dbl_web_import")}\Error\{transId}.csv";
                }
                File.Move(file.FullName, destination);

            }
        }

        bool SaveWebSampleQueueFiles(FileInfo file)
        {
            bool success = false;
            transId = Library.Increment.GetIncrement("FTP_TRANSACTION", "KEY0");
            using (var sr = new StreamReader(file.FullName))
            {  
                var headers = sr.ReadLine();
                while (!sr.EndOfStream)
                {

                    var line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        continue;

                    var v = line.Replace("\"", "").Split(',').ToList();


                    //Modified from 9 to 16(Avi)
                    if (v.Count != 15)
                        continue;


                    var e = EntityManager.CreateEntity(WebSampleQueueBase.EntityName) as WebSampleQueueBase;

                    e.CustomerId = v[0];

                    // newly added feilds (Bhavani)
                    e.WebJobOrder = v[1];
                    e.SampleSubmitter = v[2];
                    e.PoNumber = v[3];
                    e.DeibelLocation = v[4];
                    e.BatchNumber = v[5];
                    e.SampleOrderId = v[6];
                    e.ProductMatrix = v[7];
                    e.SampleDescription = v[8];
                    e.DescriptionB = v[9];
                    e.DescriptionC = v[10];
                    e.Comments = v[11];
                    e.TestOrderId = v[12];
                    e.AnalysisId = v[13];
                    //e.ComponentName = v[13];
                    e.ComponentList = v[14];
                    // e.ProductMatrix = v[15];
                    // end newly added fields

                    /*e.SampleCode = v[1];
                    e.SamplePoint = v[2];
                    e.SampleDescription = v[3];
                    e.TestCode = v[4];
                    e.ComponentName = v[5];
                    e.SamplingTech = v[6];
                    e.SamplingDatetime = GetDate(v[7], v[8]); */

                    EntityManager.Transaction.Add(e);
                    success = true;
                    _itemsAdded++;
                }
                EntityManager.Commit();
                return success;
            }

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

        public void LogInfo(string message)
        {
            // The first parameter, smp$userfiles, is in registry at HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\LabSystems\SampleManager Server\SMUAT
            var file = Library.File.GetWriteFile("smp$dblwebsampleLogfiles", "EnvironmentImport.txt");
            File.AppendAllText(file.FullName, message + "\r\n");
        }
    }
}
