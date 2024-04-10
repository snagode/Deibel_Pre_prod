using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;
using System.Net.Mail;
namespace Customization.Tasks
{
    [SampleManagerTask(nameof(EmailNotification_SampleRefrigerated), "WorkflowCallback")]
    public class EmailNotification_SampleRefrigerated : SampleManagerTask
    {
        protected override void SetupTask()
        {
            base.SetupTask();
            if (Context.SelectedItems.ActiveCount == 0)
            { Exit(false); return; }
            var job = (JobHeader)Context.SelectedItems[0];
            if (job != null)
            {
                var sample = job.Samples;
                verifyTemp(sample);
            }
            Exit(true);
        }

        private void verifyTemp(IEntityCollection samples)
        {
            string emailBody = "";
            foreach (var item in samples)
            {

                Sample sample = (Sample)item;
                string str = sample.TempOnReceipt;
                // Find the index of 'C'
                int index = str.IndexOf('C');
                string grouId = sample.GroupId.Identity;
                var grouId1 = sample.GroupId.Name;


                // If 'C' is found, extract the substring from the beginning to 'C'
                double result = Convert.ToDouble((index >= 0) ? str.Substring(0, index) : str);
                if (sample.Refrigerated && sample.GroupId.Name =="NI")
                { 
                    if (result < 2 || result > 8)
                    {
                        //Console.WriteLine("The value is not between 2 and 8."); 
                        emailBody = emailBody + Environment.NewLine + "<html><body> </p>Sample:" + sample.IdText + ", Description:" + sample.Description + ", Temp On Receipt:" + sample.TempOnReceipt + ", Login Date:" + sample.LoginDate + ", by:" + sample.LoginBy + Environment.NewLine;

                    } 
                }
            }
            if (!string.IsNullOrEmpty(emailBody))
            {
                string reply = Environment.NewLine+"\n This is an automated notification and we kindly request that you do not reply to this email as we are unable to receive replies.";
                emailBody = emailBody.Replace("\n", "<br />");
                emailBody = emailBody.Replace("\n", "<br />");
                emailBody = emailBody.Replace("\n", "<br />");
                emailBody = emailBody + Environment.NewLine + reply+"</body></html>";
                email_send(emailBody);
            }
        }
        public void email_send(string emailBody)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    SmtpClient SmtpServer = new SmtpClient("10.3.1.106");
                    mail.From = new MailAddress("SampleManager@DeibelLabs.com");
                    mail.To.Add("snagode@deibellabs.com");
                    //foreach (var email in emailArr)
                    //{
                    //    mail.To.Add(email);
                    //}
                    mail.Subject = "Refrigerated Temperature OOS email from Deibel Labs";
                    String body = emailBody;
                    mail.IsBodyHtml = true;
                    mail.Body = body;
                    SmtpServer.UseDefaultCredentials = false;
                    //SmtpServer.Credentials = new System.Net.NetworkCredential("results", "3SFS@7120!");
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
