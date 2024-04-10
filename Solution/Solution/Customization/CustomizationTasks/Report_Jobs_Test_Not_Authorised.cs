using System;
using System.Collections.Generic;
using System.Drawing;
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
using Thermo.SampleManager.Core.Definition;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(Report_Jobs_Test_Not_Authorised), "LABTABLE", "Job_Header")]
    public class Report_Jobs_Test_Not_Authorised : DefaultFormTask
    {
        FormJobsNotTestNotAuthorised _form;
     
        protected override void SetupTask()
        {
            _form = FormFactory.CreateForm<FormJobsNotTestNotAuthorised>();
            _form.Loaded += _form_Loaded;
            _form.Show();
        }
        private void _form_Loaded(object sender, EventArgs e)
        {

            _form.ButtonEdit1.Click += BtnEdit1_Click;
        }
        private void BtnEdit1_Click(object sender, EventArgs e)
        {
            LogInfo("Launch Method Start");
            var StartDate = _form.DateEdit1.Date;
            var EndDate = _form.DateEdit2.Date;
            

            var q = EntityManager.CreateQuery(JobsNotAuthoriedTestNotAuthorisedBase.EntityName);

            q.AddNotEquals(JobsNotAuthoriedTestNotAuthorisedPropertyNames.JobStatus, "A");

            q.AddNotEquals(JobsNotAuthoriedTestNotAuthorisedPropertyNames.Status, "A");

            q.AddEquals(JobsNotAuthoriedTestNotAuthorisedPropertyNames.Status, "C");
            q.AddGreaterThan(JobsNotAuthoriedTestNotAuthorisedPropertyNames.DateAuthorised, StartDate);
            q.AddLessThan(JobsNotAuthoriedTestNotAuthorisedPropertyNames.DateAuthorised, EndDate);


            var data = EntityManager.Select(q);
            // data.ActiveItems.GroupBy<c>
            if (data.Count == 0)
            {
                LogInfo("There is no CSV file to send the data to Finanance");
                LogInfo("End");
                Library.Utils.FlashMessage("There is no report for the selected Dates", " There is no report for the selected Dates");
                _form.Close();
                return;
            }
            else
            {
                var val = createCSv(data);

            
                //if (val == "Success")
                //{
                //    var path = Library.Environment.GetFolderList("Smp$FinanceFiles");
                //    var files = Library.File.GetFiles(path);
                //    foreach (var file in files)
                //    {
                //        email_send(file);
                //    }
                //}
            }
            LogInfo("Launch Method End"); 
        }





    private string createCSv(IEntityCollection data)
        {
            try
            {
                LogInfo("Creating CSV File");
                var csv = new StringBuilder();
                csv.AppendLine("JobName" + "," + "JobStatus" + "," + "Analysis" + "," + "GroupId" + "," + "DateAuthorised" +","+"TestStatus");
                foreach (JobsNotAuthoriedTestNotAuthorisedBase i in data)
                {
                    csv.AppendLine(i.IdText + "," + i.JobStatus + "," + i.Analysis + "," + i.GroupId + "," + i.DateAuthorised +","+i.Status);

                }
                string logFile = DateTime.Now.ToString("yyyyMMdd");
                if (!System.IO.File.Exists(logFile))
                {
                    System.IO.File.Create(logFile);
                }
                File.WriteAllText(@"C:\Thermo\SampleManager\Server\SMUAT\ReportJobsNotTestAuthorised\Logs\Data\" + logFile + ".csv", csv.ToString());
                LogInfo("CSV File Created Successfully");
                var path = Path.GetFileName(logFile);
               
                Library.File.TransferToClient(@"C:\Thermo\SampleManager\Server\SMUAT\ReportJobsNotTestAuthorised\Logs\Data\"+logFile+".csv", logFile+".csv", true);

            }
            catch (Exception ex)
            {
                return "Failure";
            }
            return "Success";

        }
      
        public void LogInfo(string message)
        {
            string logFile = DateTime.Now.ToString("yyyyMMdd") + ".txt";
            if (!System.IO.File.Exists(logFile))
            {
                System.IO.File.Create(logFile);
            }
            // The first parameter, smp$userfiles, is in registry at HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\LabSystems\SampleManager Server\SMUAT
            var file = Library.File.GetWriteFile("smp$ReportJobsNotTestAuthorised", logFile);
            File.AppendAllText(file.FullName, message + "\r\n");
        }

    }
}
