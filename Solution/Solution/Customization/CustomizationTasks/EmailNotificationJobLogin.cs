using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.Framework.Core;
using System.Globalization;

namespace Customization.Tasks
{

    [SampleManagerTask(nameof(EmailNotificationJobLogin), "WorkflowCallback")]

    public class EmailNotificationJobLogin : SampleManagerTask
    {
        DateTime _startdate;
        DateTime _enddate;
        // LogsMaintainInDB objLog = new LogsMaintainInDB();
        protected override void SetupTask()
        {
            base.SetupTask();
            if (Context.SelectedItems.ActiveCount == 0)
            { Exit(false); return; }
            var job = (JobHeader)Context.SelectedItems[0];
            //var custId = job.CustomerId;

            //var lastYearDate = DateTime.Now.AddYears(-1).ToString("M/d/yyyy hh:mm:ss tt");
            //var DateYesterday = DateTime.Now.AddDays(-1).ToString("M/d/yyyy hh:mm:ss tt");
            _startdate = DateTime.ParseExact(DateTime.Now.AddYears(-1).ToString("M/d/yyyy hh:mm:ss tt"), "M/d/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
            _enddate = DateTime.ParseExact(DateTime.Now.AddDays(-1).ToString("M/d/yyyy hh:mm:ss tt"), "M/d/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
            //  var q2 = "Select * From JOB_HEADER WHERE CONVERT(date,DATE_CREATED,103) between CONVERT(date,'" + lastYearDate.ToString("dd/MM/yyyy") + "',103) AND CONVERT(date,'" + DateYesterday.ToString("dd/MM/yyyy") + "',103) AND CUSTOMER_ID='" + job.CustomerId.Identity + "'";
            var query = "Select * From JOB_HEADER WHERE CONVERT(date,DATE_CREATED,101) between CONVERT(date,'" + _startdate + "',101) AND CONVERT(date,'" + _enddate + "',101) AND CUSTOMER_ID='" + job.CustomerId.Identity + "'";
            var jobCount = EntityManager.SelectDynamic(TableNames.JobHeader, query).ActiveItems.Cast<JobHeader>().Count();
            string logMsg = "Result Count-" + jobCount + " - Customer Id = " + job.CustomerId.Identity + ", LastYearDate=" + _startdate + ",DateYesterday=" + _enddate;

            //Exclude Prefix start
            var excludedPrefixes = new List<string> { "MSP_", "MSC_", "MWP_", "MSV_", "MWV_", "MSDH_", "MSHN_", "MSS_", "MSSVA_", "MSVA_" };
            string customerId = job.CustomerId.Identity;
            bool startsWithExcludedPrefix = excludedPrefixes.Any(prefix => customerId.StartsWith(prefix));
            //Getting the customer's email and separating them with semicolons.
            var emailds = job.GroupId.Email;
            string[] emailArr = emailds.Split(';');

            //customers are created in a year or not
            DateTime customerDateCreated = (DateTime)job.CustomerId.DateCreated;
            string custDate = customerDateCreated.ToString("M/d/yyyy hh:mm:ss tt");
            DateTime _customerDateCreated = DateTime.ParseExact(custDate, "M/d/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
            double Year = calCustomerDateCreatedYear(_customerDateCreated);
            // Checking conditions
            if (jobCount == 0 && !startsWithExcludedPrefix && Year > 1)
            {

                if (!string.IsNullOrEmpty(job.GroupId.Email))
                {
                    email_send(job.GroupId.Email, job.CustomerId.Identity.ToString(), job.GroupId.Identity.ToString(), logMsg + "Job Name=" + job.JobName, emailArr);
                }
                else
                {
                    emailNotificationLogs(null, "Failed", "Please update the lab-" + job.GroupId.Identity + " email address.");
                }
            }
            else
            {

            }
            Exit(true);
        }



        private double calCustomerDateCreatedYear(DateTime dateCreated)
        {
            double year = 0;
            try
            {
                //DateTime lastYearDate = DateTime.Now;
                //DateTime DateCreated =Convert.ToDateTime(dateCreated); 
                // year = lastYearDate.Year - DateCreated.Year;



                DateTime lastYearDate = Convert.ToDateTime(dateCreated);
                DateTime currentDate = Convert.ToDateTime(DateTime.Now);

                TimeSpan difference = currentDate - lastYearDate;

                int years = difference.Days / 365; // Assuming 365 days in a year
                int Months = (difference.Days % 365) / 30; // Assuming 30 days in a month
                if (Months == 12)
                {
                    years++;
                    Months = 0;
                }
                year = Convert.ToDouble($"{years}.{Months}");

            }
            catch (Exception ex)
            {
                emailNotificationLogs(null, "Faild" + ex.Message + "Date=" + dateCreated, ex.StackTrace);
            }
            return year;
        }

        public void email_send(string toMail, string customerId, string GroupId, string logMsg, string[] emailArr)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    SmtpClient SmtpServer = new SmtpClient("10.3.1.106");
                    mail.From = new MailAddress("SampleManager@DeibelLabs.com");
                    foreach (var email in emailArr)
                    {
                        mail.To.Add(email);
                    }
                    mail.Subject = "UCIF - Email Notification";
                    String body = "<html><body> <p>The following client <b>" + customerId + "</b> has not tested with Deibel Labs in over a year.   In an effort to keep our systems contact information up to date, please have the client fill out the following form: <a href='https://forms.office.com/r/wim8FPnxUt'>Click Here</a><br> <b>Lab Location: " + GroupId + "</b></p></body></html>";
                    mail.IsBodyHtml = true;
                    mail.Body = body;
                    SmtpServer.UseDefaultCredentials = false;
                    //SmtpServer.Credentials = new System.Net.NetworkCredential("results", "3SFS@7120!");
                    SmtpServer.EnableSsl = false;
                    SmtpServer.Port = 25;
                    SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                    SmtpServer.Send(mail);
                    // objLog.Logs(toMail, "Sent", ""); 
                    emailNotificationLogs(toMail, "Sent", logMsg);
                }
            }
            catch (Exception ex)
            {
                emailNotificationLogs(toMail, "Faild-from email_send", ex.StackTrace);
            }
        }

        private void emailNotificationLogs(string toMail, string emailStatus, string errorMessage)
        {
            var e = (UcifBase)EntityManager.CreateEntity(TableNames.Ucif);
            e.EmailSentTo = toMail;
            e.DateSent = DateTime.Now;
            e.EmailStatus = emailStatus;
            e.ErrorMessage = errorMessage;
            EntityManager.Transaction.Add(e);
            EntityManager.Commit();
        }
    }
}
