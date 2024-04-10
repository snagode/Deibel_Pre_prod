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
    [SampleManagerTask(nameof(Report_Jobs_Not_Test_Authorised), "LABTABLE", "Job_Header")]
    public class Report_Jobs_Not_Test_Authorised : DefaultFormTask
    {
        FormJobNotAndTestAuthorised _form;

        protected override void SetupTask()
        {
            _form = FormFactory.CreateForm<FormJobNotAndTestAuthorised>();
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
            //int rowNumber = 0;

            var q = EntityManager.CreateQuery(JobsNotAuthoriedAllTestAuthorisedBase.EntityName);

            q.AddNotEquals(JobsNotAuthoriedAllTestAuthorisedPropertyNames.JobStatus, "A");

            q.AddEquals(JobsNotAuthoriedAllTestAuthorisedPropertyNames.Status, "A");

            q.AddEquals(JobsNotAuthoriedAllTestAuthorisedPropertyNames.SampleStatus, "A");

            q.AddGreaterThan(JobsNotAuthoriedTestNotAuthorisedPropertyNames.DateAuthorised, StartDate);
            q.AddLessThan(JobsNotAuthoriedTestNotAuthorisedPropertyNames.DateAuthorised, EndDate);


            var data = EntityManager.Select(q);

            if (data.Count == 0)
            {
                LogInfo("There is no CSV file to send the data to Finanance");
                LogInfo("End");
                return;
            }
            else
            {
                var val = createCSv(data);
                //if (val == "Success")
                //{
                //    var path = Library.Environment.GetFolderList("Smp$FinanceFiles");
                //    var files = Library.File.GetFiles(path);
                //    //foreach (var file in files)
                //    //{
                //    //    email_send(file);
                //    //}
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
                csv.AppendLine("JobName" + "," + "JobStatus" + "," + "Analysis" + "," + "GroupId" + "," + "DateAuthorised" + "," + "TestStatus" + "," + "SampleStatus");
                foreach (JobsNotAuthoriedAllTestAuthorisedBase i in data)
                {
                    csv.AppendLine(i.IdText + "," + i.JobStatus + "," + i.Analysis + "," + i.GroupId + "," + i.DateAuthorised + "," + i.Status + "," + i.SampleStatus);

                }
                string logFile = DateTime.Now.ToString("yyyyMMdd");
                if (!System.IO.File.Exists(logFile))
                {
                    System.IO.File.Create(logFile);
                }
                File.WriteAllText(@"C:\Thermo\SampleManager\Server\SMUAT\ReportJobsTestNotAuthorised\Logs\Data\" + logFile + ".csv", csv.ToString());
                LogInfo("CSV File Created Successfully");

            }
            catch (Exception ex)
            {
                return "Failure";
            }
            return "Success";

        }
        public void email_send(FileInfo file)
        {
            LogInfo("Send Mail Functionality Email Send Method Start");
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    SmtpClient SmtpServer = new SmtpClient("10.3.1.4");
                    mail.From = new MailAddress("SampleManager@DeibelLabs.com");
                    // mail.To.Add("bcochuyt@deibellabs.com");
                    //mail.To.Add("NMatson@DeibelLabs.com");
                    //mail.To.Add("Helpdesk@DeibelLabs.com");
                    //mail.To.Add("deibelar@deibellabs.com");
                    mail.To.Add("akumar@deibellabs.com");
                    mail.Subject = "List of Jobs Not Billed";
                    mail.Body = "Attached is the list of outstanding jobs that have not been billed for lab";

                    if (file != null && file.Length > 0)
                    {
                        var attachment = new System.Net.Mail.Attachment(file.FullName);
                        mail.Attachments.Add(attachment);

                    }
                    SmtpServer.UseDefaultCredentials = false;
                    SmtpServer.Credentials = new System.Net.NetworkCredential("results", "3SFS@7120!");
                    SmtpServer.EnableSsl = false;

                    SmtpServer.Port = 25;
                    SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;


                    SmtpServer.Send(mail);
                    // mail.Attachments.Dispose();


                }
                File.Move(file.FullName, @"C:\Thermo\SampleManager\Server\SMUAT\FinanaceAPI\Logs\Data\MovedFile\" + file.Name);

                LogInfo("Send Mail Functionality Email Send Method End");
            }

            catch (Exception ex)
            {
                LogInfo(ex.Message);
            }

        }


        public void LogInfo(string message)
        {
            string logFile = DateTime.Now.ToString("yyyyMMdd") + ".txt";
            if (!System.IO.File.Exists(logFile))
            {
                System.IO.File.Create(logFile);
            }
            // The first parameter, smp$userfiles, is in registry at HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\LabSystems\SampleManager Server\SMUAT
            var file = Library.File.GetWriteFile("Smp$FinananceLogfile", logFile);
            File.AppendAllText(file.FullName, message + "\r\n");
        }

    }
}
