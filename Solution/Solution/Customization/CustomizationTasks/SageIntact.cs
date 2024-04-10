using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
using Thermo.SampleManager.Tasks;


namespace Customization.Tasks
{
    [SampleManagerTask(nameof(SageIntact))]
    public class SageIntact : SampleManagerTask, IBackgroundTask
    {
        string supDocId;
        string url = "https://asgsageintacctapi.azurewebsites.net/api/OrderEntry/CreateOETransaction";
        string url1 = "https://asgsageintacctapi.azurewebsites.net/api/Company/CreateAttachment";
        protected override void SetupTask()
        {

            // Check if this is run in background, i.e. executed by timer queue service
            if (Library.Environment.IsBackground())
                return;
            //if (Library.Environment.IsInteractive())
            //{
            //    Launch();
            //}
            Launch();
            // If it's not from timer queue, the intention is to debug and this is called
            // from a menu item.  
            LogInfo($"{DateTime.Now}: Begin Sage Intacct Intgration to create attachment and send data from SM to SI(SetupTask Method).");
        }

        public void Launch()
        {
            LogInfo("\n\n\n");
            LogInfo($"{DateTime.Now}: Begin API Integration. Launch Method Start");
            try
            {
                //Avinash Start
                var SERVER_PATH = Library.Environment.GetFolderList("smp$billing").ToString();
                var MAS_PATH = Library.Environment.GetFolderList("smp$masdestination").ToString();
               // var MAS_PATH =  @"C:\Thermo\SampleManager\Server\SMUAT\Billing\MAS-CV-FLAGSTONE_FOODS_NC-Feb-20240305080249";
                var q = EntityManager.CreateQuery(MasBillingBase.EntityName);
                q.PushBracket();
                q.AddEquals(MasBillingPropertyNames.Status, "P");
                q.AddOr();
                q.AddEquals(MasBillingPropertyNames.Status, "B");
                q.AddOr();
                q.AddEquals(MasBillingPropertyNames.Status, "R");
                q.PopBracket();
                q.AddAnd();
                q.AddEquals(MasBillingPropertyNames.ApprovalStatus, "A");

                var data = EntityManager.Select(q);
                if (data.Count == 0)
                {
                    LogInfo("There is no CSV file to send the data to Sage Intacct");
                    LogInfo("End");
                    return;
                }

                foreach (var item in data)
                {
                    var record = (MasBillingBase)item;
                   // var files  = 

                    MasBilling_Send(MAS_PATH + "//" + record.LabCode + "//" + record.FileName,
                    SERVER_PATH + "\\" + record.FileName,
                   record);

                }
            }

            catch (Exception e) { LogInfo(e.Message + e.StackTrace); }
        }
        private void MasBilling_Send(string maspath, string serverpath, MasBillingBase q)
        {
            String currentdate = DateTime.Now.ToString();
            string date1 = currentdate.Replace(@"/", "");
            date1 = date1.Replace(" ", "");
            date1 = date1.Replace(":", "");
            date1 = date1.Remove(date1.Length - 2, 2);

            supDocId = "";
            var User_Id = q.CreatedBy;
            var customerID = q.CustomerId.SiCustomerid;
            var GroupID = q.CustomerId.GroupId;
            LogInfo("Sage Intact CustomerID = " + customerID);
            customerID = customerID + date1;

            LogInfo("GroupID =" + GroupID);
            var query = EntityManager.CreateQuery(TableNames.Personnel);
            query.AddEquals(PersonnelPropertyNames.Identity, User_Id);
            var result = EntityManager.Select(query).Cast<PersonnelBase>().FirstOrDefault();
            var email = result.Email;


            string[] name = serverpath.Split('\\');

            string filename = name[6] + ".xlsx";

            string status = q.Status;

            var serverpathextn = "";
            var serverpathextn1 = "";
            var maspathextn = "";
            try
            {
                serverpathextn1 = serverpath + ".csv";
                serverpathextn = serverpath + ".xlsx";
                FileInfo fi = new FileInfo(serverpathextn);
                FileInfo fi1 = new FileInfo(serverpathextn1);

                //Create multiple attachment 

                Attatchmentcreation(fi, customerID,q);
                DoEverything(fi1, email, customerID,q);

                LogInfo($"Integration Completed Successfully");


                if (q.Mode != "POS")
                {
                    serverpathextn = serverpath + ".xlsx";
                    maspathextn = maspath + ".xlsx";


                    q.Status = "T";
                    q.StatusMessage = "Send complete";

                }

            }
            catch (Exception ex)
            {
                q.Status = "F";
                q.StatusMessage = "Send failed - exceeded max attempts";

                var tries = q.Tries;
                if (tries < 5)
                {
                    //IF tries < GLOBAL("MAX_MAS_ATTEMPTS") THEN
                    q.Status = "R";
                    q.StatusMessage = "Retrying - Previous send failed";
                    q.Tries = tries + 1;
                    q.ErrorDescription = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                }
                else
                {
                    if (!(String.IsNullOrEmpty(email)))
                    {
                        email_send(email);
                    }
                    q.Status = "F";
                    q.StatusMessage = "Send failed - exceeded max attempts";
                    q.ErrorDescription = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                }
            }
            q.DateProcessed = DateTime.Now;
            EntityManager.Transaction.Add(q);
            EntityManager.Commit();

        }
        void DoEverything(FileInfo file, String Email, string c, MasBillingBase record)
        {
            try
            {
                LogInfo("Do Everythhing Method Start -  Posting ApI Data to Sage Intacct ");

                var currentuseremail = Email;
                var name = file.Name;
                String[] resName = name.Split('.');
                string filename = resName[0];
                string filename1 = filename.Replace("-", string.Empty);

               // supDocId = c;
                LogInfo("Sales Order Id SupDocID =" + supDocId);


                using (var sr = new StreamReader(file.FullName))
                {
                    LogInfo("file name is =" + file.FullName);
                    dynamic obj1 = new List<dynamic>();
                    dynamic dataobj = new System.Dynamic.ExpandoObject();
                    dataobj.transactiontype = "Sales Order";
                    // dataobj.datedue = "03/01/2023";
                    dataobj.documentno = "";
                    dataobj.supdocid = supDocId;

                    String customerId = "";
                    var dateCreated = "";
                    var datePosted = "";
                    var DueDateDue = "";
                    var PoNumber = "";
                    var Currency = "";
                    var currencyExchangeRate = "";
                    var CurrencyExchangeType = "";
                    var SlDepositAmount = "";
                    var SlDepositNumber = "";
                    var Termname = "";



                    while (!sr.EndOfStream)
                    {
                        dynamic obj2 = new System.Dynamic.ExpandoObject();
                        var line = sr.ReadLine();
                        if (string.IsNullOrEmpty(line))
                            continue;
                        var v = line.Replace("\"", "").Split(',').ToList();
                        customerId = v[6];
                         PoNumber = v[3];
                        Currency = v[10];
                        currencyExchangeRate = v[11];
                        CurrencyExchangeType = v[12];
                        SlDepositAmount = v[15];
                        SlDepositNumber = v[14];
                        Termname = v[13];
                        // DueDateDue = v[13];

                        dateCreated = v[0];
                        string year = v[0].Substring(0, 4);
                        string month = v[0].Substring(4, 2);
                        string date = v[0].Substring(6, 2);
                        dateCreated = month + "/" + date + "/" + year;
                        datePosted = dateCreated;


                        obj2.itemid = v[8];
                        obj2.quantity = v[9];
                        obj2.unit = "Each";
                        obj2.locationid = v[5];
                        obj2.departmentid = v[7];
                        obj2.classid = v[4];
                        obj2.memo = "";
                        obj2.warehouseid = "1";


                        obj1.Add(obj2);

                    }
                    dataobj.sotransitems = obj1;
                    dataobj.customerid = customerId;
                    dataobj.datecreated = dateCreated;
                    dataobj.dateposted = datePosted;
                    //new changes payment terms start
                     dataobj.datedue = null;
                   // dataobj.datedue = DueDateDue;
                    //End
                    dataobj.termname = Termname;
                    //dataobj.termname = "CC Autopay";
                    //New Changes Start
                    if (Currency == "" || Currency == "USD")
                    {

                    }
                    else
                    {

                        dataobj.currency = Currency;
                        // dataobj.exchratetype = CurrencyExchangeType;
                        if (String.IsNullOrEmpty(CurrencyExchangeType))
                        {

                        }
                        else
                        {
                            dataobj.exchrate = currencyExchangeRate;
                        }
                    }
                    dataobj.referenceno = filename1;

                    //Avinash 28th Nov Start

                    List<System.Dynamic.ExpandoObject> obj8 = new List<System.Dynamic.ExpandoObject>();

                    dynamic obj7 = new System.Dynamic.ExpandoObject();
                    obj7.customfieldname = "LIMS_DEPOSIT_AMOUNT";
                    obj7.customfieldvalue = SlDepositAmount;


                    dynamic obj10 = new System.Dynamic.ExpandoObject();
                    obj10.customfieldname = "LIMS_DEPOSIT_REFERENCE";
                    obj10.customfieldvalue = SlDepositNumber;

                    obj8.Add(obj7);
                    obj8.Add(obj10);


                   

                    dataobj.customfields = obj8;
                    if (String.IsNullOrEmpty(PoNumber))
                    {
                        dataobj.customerponumber = "See Lab Report Summary";
                    }
                    else
                    {
                        dataobj.customerponumber = PoNumber;
                    }
                    //New Changes ENd
                    PostRequest(url, dataobj, currentuseremail,record);
                    LogInfo("Termname=" + dataobj.termname);
                    LogInfo("DepositAmount=" + SlDepositAmount);
                    LogInfo("DepositNumber=" + SlDepositNumber);

                    LogInfo("Do Everythhing Method End -  Posting ApI Data to Sage Intacct ");
                }
            }
            catch (Exception ex)
            {
                LogInfo("Exception Occoured in DoEvrything Method --" + ex);
            }


        }

        public void PostRequest(string url, dynamic obj, string email, MasBillingBase rec)
        {
            email = "akumar@deibellabs.com";
            try
            {
                LogInfo("Postrequest Method Start for Posting data to Sage Intacct");

                dynamic obj3 = new System.Dynamic.ExpandoObject();
                obj3.intacctlogin = new System.Dynamic.ExpandoObject();
                obj3.intacctlogin.companyid = "deibellabs-imp";
                obj3.intacctlogin.user = "LIMSAdmin";
                //obj3.intacctlogin.pwd = "iTS4K9S!EH0";
                obj3.intacctlogin.pwd = "4bNVGFU5O!3";
                obj3.intacctlogin.locationentityid = "";
                obj3.intacctlogin.useapisession = "false";
                obj3.intacctlogin.apisession = "UrpmrFm0miCDU_Uq8e024_nHIIJTgFK6PXe8qzQSg1PlKvHtNvXpxyCD";

                obj3.data = new System.Dynamic.ExpandoObject();
                obj3.data = obj;

                var value = Newtonsoft.Json.JsonConvert.SerializeObject(obj3);
                LogInfo("Request data : " + value);

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                using (HttpClient client = new HttpClient())
                {
                    StringContent str = new StringContent(value);
                    HttpContent content = str;
                    content.Headers.Add("ApiKey", "53dFsLkzL88rZpDpjqGCYeLD0qyHT6CoH4BhNBO3UPAzSeCVQ5r2");
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = client.PostAsync(url, content);
                    var result = response.Result;
                  LogInfo("Result= " + result);
                    var res = result.Content.ReadAsStringAsync().Result;
                    dynamic output = Newtonsoft.Json.JsonConvert.DeserializeObject(res);
                    if (output.error == null)
                    {
                        var itemID = output.key;
                        rec.ApiErrorDescription = itemID;
                       
                    }
                    else
                    {
                        rec.ApiErrorDescription = output.error.Value;
                        email_sendforAPIErrors(email, rec.ApiErrorDescription);
                    }
                    //if (res.Contains("found"))
                    //{
                    //    email_send(email);
                    //}
                    LogInfo("Sales Order Id = " + res);
                }
            }

            catch (Exception ex)
            {
                LogInfo("Exception Occoured in PostRequest method--" + ex);
            }

            LogInfo("Postrequest Method End for Posting data to Sage Intacct");
        }

        void Attatchmentcreation(FileInfo file, String C, MasBillingBase record)
        {
            try
            {
                LogInfo("DoEverything1 Method Start");

                var name = file.Name;
                string[] restr = name.Split('.');
                string filename = restr[0];
               // string supDocId = filename.Replace("-", string.Empty);
                String date = DateTime.Now.Date.ToString();


                supDocId = C;


                var data = GetBinaryFile(file.FullName);
                dynamic obj1 = new List<dynamic>();
                dynamic dataobj = new System.Dynamic.ExpandoObject();
                //dataobj.supdocid = "SO0016";
                dataobj.supdocid = supDocId;
                dataobj.supdocname = "A";
                dataobj.supdocfoldername = "LRS Files";
                dataobj.supdocdescription = "test";

                dynamic obj2 = new System.Dynamic.ExpandoObject();
                obj2.attachmentname = restr[0];
                //obj2.attachmenttype = "csv";
                obj2.attachmenttype = "xlsx";
                obj2.attachmentdatatype = "base64string";
                obj2.attachmentdata = data;

                obj1.Add(obj2);
                dataobj.attachments = obj1;

                LogInfo("Attachment Id=" + supDocId);


                PostRequestforAttatchmentCreation(url1, dataobj,record);

                LogInfo("DoEvrything1 Method End");
            }

            catch (Exception ex)
            {
                LogInfo("Exception Occoured in DoEvrything1 Method --" + ex);
            }

        }
        private byte[] GetBinaryFile(string name)
        {
            byte[] bytes;
            using (FileStream file = new FileStream(name, FileMode.Open, FileAccess.Read))
            {
                bytes = new byte[file.Length];
                file.Read(bytes, 0, (int)file.Length);
            }
            return bytes;
        }

        public void PostRequestforAttatchmentCreation(string url1, dynamic obj, MasBillingBase rec)
        {
            try
            {
                LogInfo("PostRequest1 method start- Attachment creation ");
                dynamic obj3 = new System.Dynamic.ExpandoObject();
                obj3.intacctlogin = new System.Dynamic.ExpandoObject();
                obj3.intacctlogin.companyid = "deibellabs-imp";
                obj3.intacctlogin.user = "LIMSAdmin";
                obj3.intacctlogin.pwd = "4bNVGFU5O!3";
                obj3.intacctlogin.locationentityid = "";
                obj3.intacctlogin.useapisession = "false";
                obj3.intacctlogin.apisession = "UrpmrFm0miCDU_Uq8e024_nHIIJTgFK6PXe8qzQSg1PlKvHtNvXpxyCD";

                obj3.data = new System.Dynamic.ExpandoObject();
                obj3.data = obj;

                var value = Newtonsoft.Json.JsonConvert.SerializeObject(obj3);
                 LogInfo("Request data : " + value);

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                using (HttpClient client = new HttpClient())
                {
                    StringContent str = new StringContent(value);
                    HttpContent content = str;
                    content.Headers.Add("ApiKey", "53dFsLkzL88rZpDpjqGCYeLD0qyHT6CoH4BhNBO3UPAzSeCVQ5r2");
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = client.PostAsync(url1, content);
                    var result = response.Result;
                    LogInfo("Result= " + result);
                    var res = result.Content.ReadAsStringAsync().Result;
                    dynamic output = Newtonsoft.Json.JsonConvert.DeserializeObject(res);
                    LogInfo("Request= " + str);
                    if (output.error == null)
                    {
                        var itemID = output.key;
                        rec.ApiErrorDescription = itemID;

                    }
                    else
                    {
                        rec.ApiErrorDescription = output.error.Value;
                        
                    }
                    LogInfo("Attachment result =" + res);

                }
            }

            catch (Exception ex)
            {
                LogInfo("Exception occoured in Postrequest1 Method--" + ex);
            }
            LogInfo("PostRequest1 Method End");
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
            string logFile = DateTime.Now.ToString("yyyyMMdd") + ".txt";
            if (!System.IO.File.Exists(logFile))
            {
                System.IO.File.Create(logFile);
            }
            // The first parameter, smp$userfiles, is in registry at HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\LabSystems\SampleManager Server\SMUAT
            var file = Library.File.GetWriteFile("Smp$SageIntactLogfile", logFile);
            File.AppendAllText(file.FullName, message + "\r\n");
        }
        public void email_send(string email)
        {
            LogInfo("Send Mail Functionality Email Send Method Start for Customers not present");

            try
            {
                String ToEmail;
                ToEmail = email;
                // string[] Multi = ToEmail.Split(',');
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("10.3.1.106");
                mail.From = new MailAddress("Results@deibellabs.com");
                mail.To.Add("Helpdesk@DeibelLabs.com");
                mail.To.Add("eberrios@deibellabs.com");
                mail.To.Add("bcochuyt@deibellabs.com");
                mail.To.Add("NMatson@DeibelLabs.com");
                mail.To.Add("deibelar@deibellabs.com");
                mail.To.Add(ToEmail);
                //mail.To.Add("akumar@deibellabs.com");

                mail.Subject = "Sage Intaact Customer ID is missing in Sample Manager.";
                mail.Body = "Sage Intaact Customer ID is missing in Sample Manager for Customer_ID. Please update with the correct customer ID and remove flag in order to bill.";




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

        public void email_sendforAPIErrors(string email,string apierrordesccription)
        {
            String ToEmail;
            LogInfo("Send Mail Functionality Email Send Method Start for API Error");


          
            

            try
            {
              
                ToEmail = email;
                // string[] Multi = ToEmail.Split(',');
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("10.3.1.106");
                var query = EntityManager.CreateQuery(TableNames.NotificationsLims);
                query.AddEquals(NotificationsLimsPropertyNames.EntryId, "API_ERROR");
                var result = EntityManager.Select(query).ActiveItems.Cast<NotificationsLimsBase>().ToList();
                foreach (var item in result)
                {
                    mail.To.Add(item.Email);
               }
                mail.From = new MailAddress("Results@deibellabs.com");
               // mail.To.Add("eberrios@deibellabs.com");
                mail.To.Add(ToEmail);
            

                mail.Subject = "Pre-Prod Sales Order API Error";
                mail.Body = apierrordesccription;




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
    }
}
