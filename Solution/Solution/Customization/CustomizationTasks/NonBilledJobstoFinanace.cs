using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
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
using Thermo.SampleManager.Server;
using Thermo.SampleManager.Tasks;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(NonBilledJobstoFinanace))]
    class NonBilledJobstoFinanace : SampleManagerTask, IBackgroundTask
    {
        protected override void SetupTask()
        { // Check if this is run in background, i.e. executed by timer queue service
            if (Library.Environment.IsBackground())
                return;
            if (Library.Environment.IsInteractive())
            {
                Launch();
            }

            LogInfo($"{DateTime.Now}: Begin Finanac Api--Sending Non Billed Jobs to finance");
        }

        public void Launch()
        {
            LogInfo("Launch Method Start");
            var CurrenetDate = DateTime.Now;
            var LessThan10Days = CurrenetDate.AddDays(-10);


            var q = EntityManager.CreateQuery(JobHeaderBase.EntityName);
            q.PushBracket();
            q.AddEquals(JobHeaderPropertyNames.JobStatus, "A");
            q.AddAnd();
            q.AddEquals(JobHeaderPropertyNames.BillingStatus, "N");
            q.AddAnd();
            q.AddLessThan(JobHeaderPropertyNames.DateCreated, LessThan10Days);
            q.PopBracket();
            var data = EntityManager.Select(q);
            if (data.Count == 0)
            {
                LogInfo("There is no CSV file to send the data to Sage Intacct");
                LogInfo("End");
                return;
            }
            else
            {
                var val = createCSv(data);
                if (val == "Success")
                {
                    var path = Library.Environment.GetFolderList("Smp$FinanceFiles");
                    var files = Library.File.GetFiles(path);
                    foreach (var file in files)
                    {
                        email_send(file);
                    }
                }
            }
            LogInfo("Launch Method End");
        }

        private string createCSv(IEntityCollection data)
        {
            try
            {
                LogInfo("Creating CSV File");
                var csv = new StringBuilder();
                csv.AppendLine("JobName" + "," + "GroupId" + "," + "CustomerID");
                foreach (JobHeader i in data)
                {
                    csv.AppendLine(i.JobName + "," + i.GroupId + "," + i.CustomerId);

                }
                string logFile = DateTime.Now.ToString("yyyyMMdd");
                if (!System.IO.File.Exists(logFile))
                {
                    System.IO.File.Create(logFile);
                }
                File.WriteAllText(@"C:\Thermo\SampleManager\Server\SMUAT\FinanaceAPI\Logs25th\Data25\" + logFile + ".csv", csv.ToString());
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
                    // mail.To.Add("NMatson@DeibelLabs.com");
                    // mail.To.Add("Helpdesk@DeibelLabs.com");
                    //  mail.To.Add("deibelar@deibellabs.com");
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
                File.Move(file.FullName, @"C:\Thermo\SampleManager\Server\SMUAT\FinanaceAPI\Logs\Data\Moved File");

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
            var file = Library.File.GetWriteFile("Smp$FinanaceLogFile25", logFile);
            File.AppendAllText(file.FullName, message + "\r\n");
        }

    }
}
