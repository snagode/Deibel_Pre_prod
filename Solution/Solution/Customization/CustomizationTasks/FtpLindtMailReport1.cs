﻿using System;
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
    [SampleManagerTask(nameof(FtpLindtMailReport1))]
    public class FtpLindtMailReport1 : SampleManagerTask, IBackgroundTask
    {
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
            LogInfo($"{DateTime.Now}: Begin Mail Send Functionality -  Start.");
        }

        public void Launch()
        {
            LogInfo($"{DateTime.Now}: Begin Mail Functionality Launch Method Start.");
            try
            {
                var path = Library.Environment.GetFolderList("smp$dbl_Lendit_Report");
                var files = Library.File.GetFiles(path);
                LogInfo(files.Count.ToString());
                if (files.Count == 0)
                {
                    LogInfo($"{DateTime.Now}:There is no file to Email.");
                    return;
                }


                email_send(files);
                //File.Delete(path.ToString());
                //foreach (var file in files)
                //{
                //    File.Move(file.FullName, "smp$dbl_Lendit_ArchiveReport");
                //}
                // LogInfo($"Added {_itemsAdded} new entities.");
            }

            catch (Exception e) { LogInfo(e.Message + e.StackTrace); }
            LogInfo($"{DateTime.Now}: Email Send complete.\r\n");

        }

        public void email_send(IList<FileInfo> files)
        {
            LogInfo("Send Mail Functionality Email Send Method Start");
            // var destination = "";
            // destination = $@"{Library.Environment.GetFolderList("smp$dbl_Lendit_ArchiveReport")}\Archive";
            try
            {
                String ToEmail;
                ToEmail = "akumar@deibellabs.com,eberrios@deibellabs.com,EGlynn@lindt.com";
                string[] Multi = ToEmail.Split(',');
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("10.3.1.4");
                mail.From = new MailAddress("Results@deibellabs.com");
                // mail.To.Add("EGlynn@lindt.com");
                //mail.To.Add("akumar@deibellabs.com");
                foreach (string multiemailId in Multi)
                {
                    mail.To.Add(new MailAddress(multiemailId));
                }
                mail.Subject = "New reports from Deibel labs";
                mail.Body = "Please find the new reports attached.";

                // System.Net.Mail.Attachment attachment;
                // attachment = new System.Net.Mail.Attachment("c:/textfile.txt");
                foreach (var file in files)
                {


                    if (file != null && file.Length > 0)
                    {

                        var attachment = new System.Net.Mail.Attachment(file.FullName);

                        mail.Attachments.Add(attachment);

                    }
                }


                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Credentials = new System.Net.NetworkCredential("results", "3SFS@7120!");
                SmtpServer.EnableSsl = false;

                SmtpServer.Port = 25;
                SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;


                SmtpServer.Send(mail);


            }
            catch (Exception ex)
            {
                LogInfo(ex.Message);
            }

        }

        public void LogInfo(string message)
        {
            // The first parameter, smp$userfiles, is in registry at HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\LabSystems\SampleManager Server\SMUAT
            var file = Library.File.GetWriteFile("Smp$LenditLogFile", "LenditReport.txt");
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
