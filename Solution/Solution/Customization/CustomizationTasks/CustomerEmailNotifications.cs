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
using System.Globalization;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(CustomerEmailNotifications), "WorkflowCallback")]
    public class CustomerEmailNotifications : SampleManagerTask
    {

        protected override void SetupTask()
        {
            base.SetupTask();
            if (Context.SelectedItems.ActiveCount == 0)
            { Exit(false); return; }

            var job = (JobHeader)Context.SelectedItems[0];
            var samples = job.Samples;
            var customerID = job.CustomerId;
            var query = EntityManager.CreateQuery(TableNames.Customer);
            query.AddEquals(CustomerPropertyNames.CustomerName, job.CustomerId);
            var results = (Customer) EntityManager.Select(query).ActiveItems[0];
            var Jobnotifications = results.Jobloginnotifications;
            var emailds = results.EmailCustomer.ToString();
            string [] userInfo = emailds.Split(';');
            // string[] multipleEmailIds = results.EmailCustomer.ToString();
            var companyName = results.CompanyName;
            var dateReceived = job.DateReceived. ToString();
            var customerId = job.CustomerId.ToString();
            
            if(Jobnotifications)
            {
                 email_send(emailds, samples,job,companyName, dateReceived, customerId, userInfo);
            }
            Exit(true);
        }
        public void email_send(string toMail, IEntityCollection samples, JobHeader Job, String  companyName,string  dateReceived,String CustomerName, string [] userinfo)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                 
                    SmtpClient SmtpServer = new SmtpClient("10.3.1.106");
                    mail.From = new MailAddress("SampleManager@DeibelLabs.com");
                    foreach (var email in userinfo)
                    {
                        mail.To.Add(email);
                    }
                    //mail.To.Add(toMail);
                    string sub = "";
                    string subject ="Job Number" +" " + Job +" has been logged in for"+ " " + companyName +" today,"+ " "+ dateReceived+" "+ "for the following samples: " + Environment.NewLine;
                    foreach (Sample sample in samples)
                    {
                        var Idtext = sample.IdText;
                        var description = sample.Description;
                        var descriptionB = sample.DescriptionB;
                        var descriptionC = sample.DescriptionC;
                        sub = sub + Environment.NewLine + "<html><body><b>"+Idtext+ "</b>" + " : " + description + " , " + descriptionB + " , " + descriptionC + Environment.NewLine ;
                        //sub = "<html><body><p>sub +  + Idtext + " " + description + " " + descriptionB + " " + descriptionC + "\n"</p></html></body>";
                        
                    }
                    subject = subject + "\n" + sub +"\n"+"\n";
                    mail.Subject = "Job" +" " + Job +" "+"Logged in for customer" +" " + CustomerName;
                    mail.IsBodyHtml = true;
                    subject = subject.Replace("\r\n", "\n");
                    subject = subject.Replace("\n", "<br />");
                    String body = subject;
                  
                    mail.Body = body;
                    SmtpServer.UseDefaultCredentials = false;
                
                    SmtpServer.EnableSsl = false;
                    SmtpServer.Port = 25;
                    SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                    SmtpServer.Send(mail);
                

                }
            }
            catch (Exception ex)
            {
                
            }
        }

    }
}