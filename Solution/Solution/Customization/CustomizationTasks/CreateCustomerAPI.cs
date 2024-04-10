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
    [SampleManagerTask(nameof(CreateCustomerAPI))]
    public class CreateCustomerAPI : SampleManagerTask, IBackgroundTask
    {

        string createCustomerUrl = "https://developer.intacct.com/api/accounts-receivable/customers/#create-customer";
        string getCustomerUrl1 = "https://developer.intacct.com/api/accounts-receivable/customers/#get-customer";
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
            //LogInfo($"{DateTime.Now}: Begin Sage Intacct Intgration to create attachment and send data from SM to SI(SetupTask Method).");
        }

        public void Launch()
        {

            try
            {
                //Avinash Start
                var SERVER_PATH = Library.Environment.GetFolderList("smp$billing").ToString();
                var MAS_PATH = Library.Environment.GetFolderList("smp$masdestination").ToString();
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
                    return;
                }

               
            }

            catch (Exception e) { }; 
        }
        
        void createCustomer(FileInfo file, String Email, string c)
        {

                    dynamic obj1 = new List<dynamic>();

                    dynamic dataobj = new System.Dynamic.ExpandoObject();
                    dynamic dataobj1 = new System.Dynamic.ExpandoObject();
                    dynamic dataobj2 = new System.Dynamic.ExpandoObject();
                    dynamic dataobj3 = new System.Dynamic.ExpandoObject();
                    dynamic dataobj4 = new System.Dynamic.ExpandoObject();
                    dynamic dataobj5 = new System.Dynamic.ExpandoObject();


                    dataobj.name = "C1028";
                    dataobj.status = "active";
                    dataobj.termname = "Net 30";
                    dataobj.onhold = "false";
                    dataobj.delivery_options = "print";
                    dataobj.currency = "USD";

                    dataobj1.printas = "C1028";
                    dataobj1.companyname ="company name";
                    dataobj1.prefix = "company name";
                    dataobj1.firstname = "first name";
                    dataobj1.lastname = "last name";
                    dataobj1.initial = " name";
                    dataobj1.phone1="234891254";
                    dataobj1.email1 = "";
                    dataobj1.url1 = "www.intacct.com";

                    dataobj2.address1 = "258 test";
                    dataobj2.city = "allen";
                    dataobj2.state = "tx";
                    dataobj2.zip = "75013";
                    dataobj2.country = "US";
                    dataobj3.categoryname = "test";
                    dataobj4.name = "test";

                    dataobj5.customfieldname = "LIMES_CODES";
                    dataobj5.customfieldvalue = "5000";
                }
            }
            


        }

        









        
    

