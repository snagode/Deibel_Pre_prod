using System;
using System.Collections.Generic;
using System.Text;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Tasks;
using Thermo.SampleManager.Tasks.BusinessObjects;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Library.ClientControls.Browse;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;
using Thermo.SampleManager.Library.ClientControls;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.IO;

namespace Customization.Tasks
{
    [SampleManagerTask("CustomerTask", "LABTABLE", "CUSTOMER")]
    public partial class CustomerTaskExtended : CustomerTask
    {

        string fileName;
        string supDocID;
        string serverfileName;
        string createCustomerUrl = "https://asgsageintacctapi.azurewebsites.net/api/AccountsReceivable/createCustomer";
        string getCustomerUrl = "https://developer.intacct.com/api/accounts-receivable/customers/#get-customer";
        string url1 = "https://asgsageintacctapi.azurewebsites.net/api/Company/CreateAttachment";
        #region Members

        private Customer _entity;
        private FormCustomer _form;

        FieldNameBrowse _fbJob;
        FieldNameBrowse _fbSample;
        FieldNameBrowse _fbTest;
        FieldNameBrowse _fbResult;
        FieldNameBrowse _fbFtpSample;
        FieldNameBrowse _fbFtpTest;

        StringBrowse _sbJobFieldsNodeLevel;
        StringBrowse _sbSampleFieldsNodeLevel;
        StringBrowse _sbTestFieldsNodeLevel;
        StringBrowse _sbResultFieldsNodeLevel;
        StringBrowse _sbFtpSampleFieldsNodeLevel;
        StringBrowse _sbFtpTestfieldsNodeLevel;

        #endregion

        #region Events

        protected override void MainFormLoaded()
        {
            base.MainFormLoaded();
            _entity = (Customer)MainForm.Entity;
            _form = (FormCustomer)MainForm;

            /* changes by  suman */

            // _form.

            _entity.PropertyChanged += new PropertyEventHandler(mEntity_PropertyChanged);
            _form.ComponentsGrid.CellLeave += ComponentsGrid_CellLeave;
            _form.btnRefreshTemplate.Click += BtnRefreshTemplate_Click;
            _form.btnPreviewOutbound.Click += BtnPreviewOutbound_Click;

            _form.gridXmlOutbound.DataLoaded += GridXmlOutbound_DataLoaded;
            _form.gridXmlOutbound.FocusedRowChanged += GridXmlOutbound_FocusedRowChanged;
            _form.gridXmlOutbound.ValidateCell += GridXmlOutbound_ValidateCell;

            _form.gridXmlInbound.DataLoaded += GridXmlInbound_DataLoaded;
            _form.gridXmlInbound.FocusedRowChanged += GridXmlInbound_FocusedRowChanged;
            _form.gridXmlInbound.ValidateCell += GridXmlInbound_ValidateCell;

            //Avinash Start

            _form.Attachment.Click += Attachment_Click;

            _form.ButtonSubmit.Click += ButtonSubmit_Click;

            _form.currency.Leave += Currency_Leave;

            //Avinash End

            _fbJob = BrowseFactory.CreateFieldNameBrowse(JobHeader.EntityName);
            _fbSample = BrowseFactory.CreateFieldNameBrowse(Sample.EntityName);
            _fbTest = BrowseFactory.CreateFieldNameBrowse(Test.EntityName);
            _fbResult = BrowseFactory.CreateFieldNameBrowse(Result.EntityName);
            _fbFtpSample = BrowseFactory.CreateFieldNameBrowse(FtpSampleBase.EntityName);
            _fbFtpTest = BrowseFactory.CreateFieldNameBrowse(FtpTestBase.EntityName);

            var jobLevels = new List<string> { JobHeader.EntityName, Sample.EntityName, Test.EntityName, Result.EntityName };
            _sbJobFieldsNodeLevel = BrowseFactory.CreateStringBrowse(jobLevels);

            var sampleLevels = new List<string> { Sample.EntityName, Test.EntityName, Result.EntityName };
            _sbSampleFieldsNodeLevel = BrowseFactory.CreateStringBrowse(sampleLevels);

            var testLevels = new List<string> { Test.EntityName, Result.EntityName };
            _sbTestFieldsNodeLevel = BrowseFactory.CreateStringBrowse(testLevels);

            var resultLevels = new List<string> { Result.EntityName };
            _sbResultFieldsNodeLevel = BrowseFactory.CreateStringBrowse(resultLevels);

            var ftpSampleLevels = new List<string> { Sample.EntityName, Test.EntityName, Result.EntityName };
            _sbFtpSampleFieldsNodeLevel = BrowseFactory.CreateStringBrowse(ftpSampleLevels);

            var ftpTestLevels = new List<string> { Result.EntityName };
            _sbFtpTestfieldsNodeLevel = BrowseFactory.CreateStringBrowse(ftpTestLevels);

            var q = EntityManager.CreateQuery(JobHeader.EntityName);
            //q.AddEquals(JobHeaderPropertyNames.JobStatus, PhraseJobStat.PhraseIdA);
            q.AddNotEquals(JobHeaderPropertyNames.CustomerId, "");
            q.AddOrder(JobHeaderPropertyNames.DateCreated, false);
            _form.ebJobs.Republish(q);

            var z = EntityManager.CreateQuery(Customer.EntityName);
            z.AddEquals(CustomerPropertyNames.ParentCustomerId, _entity.Identity);
            _form.ebChildren.Republish(z);



            FillEmailsString();
            //Sandip Start
            checkDate();
         //   DateCreatedUpdate();
           
            var currentUser = (Personnel)Library.Environment.CurrentUser;
            _form.CreatedBy.Text = currentUser.ToString();
            //Sandip End
        }

        private void checkDate()
        { 
            var q1 = EntityManager.CreateQuery(CustomerBase.EntityName);
            q1.AddEquals(CustomerPropertyNames.Identity, _entity.Identity);
            var data = EntityManager.Select(q1).ActiveItems.Cast<CustomerBase>().FirstOrDefault();
            if (data!=null)
            {
                if (!string.IsNullOrEmpty(data.DateCreated.ToString())) 
                {
                    _form.DateCreated1.Date = data.DateCreated;//
                }
                else
                {
                    _form.DateCreated1.Date = _entity.ModifiedOn;
                }
            } 
            else
            {
                _form.DateCreated1.Date = DateTime.Now;
            }
          
        }

        private void DateCreatedUpdate()
        {
            if (_form.ModifiedOn.RawText != "")
            {
                //DateTime dateModified = Convert.ToDateTime(_form.ModifiedOn.RawText);
                //int mYear = dateModified.Year;
                //int val = DateTime.Now.Year;
                //int y = val - mYear;
                //if (dateModified.ToString("dd/MM/yyyy") == DateTime.Now.ToString("dd/MM/yyyy"))
                //{
                //    _form.DateCreated1.Date = _entity.DateCreated;
                //}
                //else if (y > 1)
                //{
                //    _form.DateCreated1.Date = Convert.ToDateTime("1/1/2022");
                //}
                //else if (y <= 1)
                //{
                //    _form.DateCreated1.Date = Convert.ToDateTime("5/1/2022");
                //}
            }
            else
            {
                _form.DateCreated1.Date = DateTime.Now;
            }

        }

        private void Currency_Leave(object sender, EventArgs e)
        {
            var selectedValue = _form.currency.RawText;
            if (selectedValue == "CAD")
            {
                _entity.CurrencyExchange = "1";
                _entity.CurrencyExchType = "CUSTOM";
                // _form.SMPrompt38

            }
            else
            {
                _entity.CurrencyExchange = "";
                _entity.CurrencyExchType = "";
            }
        }

        private void ShowAttachment_Click(object sender, EventArgs e)
        {

        }

        private void Attachment_Click(object sender, EventArgs e)
        {

            fileName = Library.Utils.PromptForFile("Select Customer Form", "All Files(*.*)|*.*");

            var name = fileName.Split('\\').Last();
            serverfileName = @"C:\Thermo\SampleManager\Server\SMUAT\Billing\ClientForms\" + name;
            //Library.File.TransferFromClient(fileName, @"C:\Thermo\SampleManager\Server\SMUAT\Billing\ClientForms\test.csv");
            Library.File.TransferFromClient(fileName, @"C:\Thermo\SampleManager\Server\SMUAT\Billing\ClientForms\" + name);
            Attachment h = (Attachment)EntityManager.CreateEntity(Attachment.EntityName);
            h.Attachment = fileName;
            h.Version = 1;
            h.TableName = "CUSTOMER";
            h.RecordKey0 = "Akumar";

            _entity.HasAttachments = true;
            // EntityManager.Transaction.Add(h);
            // EntityManager.Commit();
            Library.Utils.FlashMessage("Customer  Form Added", " Customer Form Addedsub");



        }

        void FillEmailsString()
        {
            var emails = _entity.CustomerContacts.Cast<CustomerContactsBase>().Where(c => c.EmailReportFlag).Select(c => c.Email).ToList();
            var theString = string.Join("; ", emails);
            _form.txtEmails.ReadOnly = false;
            _form.txtEmails.Text = theString;
        }

        void mEntity_PropertyChanged(object sender, PropertyEventArgs e)
        {
            if (e.PropertyName == CustomerPropertyNames.ParentCustomerId)
            {
                _entity.ParentCustomerName = _entity.ParentCustomerId.CompanyName;
            }
            else if (e.PropertyName == CustomerPropertyNames.SalespersonId)
            {
                _entity.SalespersonName = _entity.SalespersonId.PersonnelName;
            }
        }

        //Changed by Bhavani

        private void ButtonSubmit_Click(object sender, EventArgs e)
        {
            LogInfo("Crete Customer Method Start - On preSave Method()");
            _form.SetBusy("", "Creating Sage Intacct Customer Id...");
            createAttchment(serverfileName);
            createCustomer(fileName, supDocID);
            _form.ClearBusy();
            LogInfo("CreateCustomer Method Started");
        }
        public void createAttchment(string file)
        {
            LogInfo("create Attachment Method-Start");
            String currentdate = DateTime.Now.ToString();
            string date1 = currentdate.Replace(@"/", "");
            date1 = date1.Replace(" ", "");
            date1 = date1.Replace(":", "");
            date1 = date1.Remove(date1.Length - 2, 2);



            var customerID = _entity.Identity;
            string str = customerID.Substring(0, 2);
            customerID = str + date1;
            // customerID = customerID + date1;


            var name = file;
            string[] restr = name.Split('.');
            string filename = restr[0];
            supDocID = filename.Replace("-", string.Empty);
            String date = DateTime.Now.Date.ToString();


            supDocID = customerID;
            LogInfo("Sup Doc Id =" + supDocID);
            var data = GetBinaryFile(file);
            dynamic obj1 = new List<dynamic>();
            dynamic dataobj = new System.Dynamic.ExpandoObject();
            //dataobj.supdocid = "SO0016";
            dataobj.supdocid = supDocID;
            dataobj.supdocname = "A";
            //dataobj.supdocfoldername = "LRS Files";
            dataobj.supdocfoldername = "Client Forms";
            dataobj.supdocdescription = "test";

            dynamic obj2 = new System.Dynamic.ExpandoObject();
            obj2.attachmentname = restr[0];
            obj2.attachmenttype = "pdf";
            // obj2.attachmenttype = "xlsx";
            obj2.attachmentdatatype = "base64string";
            obj2.attachmentdata = data;

            obj1.Add(obj2);
            dataobj.attachments = obj1;

            LogInfo("Attachment Creation Method Ends");
            LogInfo("Post request Method of Attachment Starts");

            PostRequest1forCreateAttachment(url1, dataobj);

        }

        public void PostRequest1forCreateAttachment(string url1, dynamic obj)
        {
            try
            {
                LogInfo("PostRequest1 method start- Attachment creation ");
                dynamic obj3 = new System.Dynamic.ExpandoObject();
                obj3.intacctlogin = new System.Dynamic.ExpandoObject();
                obj3.intacctlogin.companyid = "deibellabs-imp";
                //obj3.intacctlogin.user = "lberrios";
                obj3.intacctlogin.user = "LIMSAdmin";
                // obj3.intacctlogin.pwd = "Ldf999@2!";
                obj3.intacctlogin.pwd = "iTS4K9S!EH0";
                obj3.intacctlogin.locationentityid = "";
                obj3.intacctlogin.useapisession = "false";
                obj3.intacctlogin.apisession = "UrpmrFm0miCDU_Uq8e024_nHIIJTgFK6PXe8qzQSg1PlKvHtNvXpxyCD";

                obj3.data = new System.Dynamic.ExpandoObject();
                obj3.data = obj;

                var value = Newtonsoft.Json.JsonConvert.SerializeObject(obj3);
                // LogInfo("Request data : " + value);

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                using (HttpClient client = new HttpClient())
                {
                    StringContent str = new StringContent(value);
                    HttpContent content = str;
                    content.Headers.Add("ApiKey", "BE8BF7076B73B39676884557650F8480");
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = client.PostAsync(url1, content);
                    var result = response.Result;
                    var res = result.Content.ReadAsStringAsync().Result;

                    LogInfo("Attachment result =" + res);

                    LogInfo("Post Request Method of create attachment Ends");

                }
            }

            catch (Exception ex)
            {
                LogInfo("Exception occoured in Postrequest1 Method--" + ex);
            }
            LogInfo("PostRequest1 Method End");
        }
        protected override bool OnPreSave()
        {
            // createCustomer(fileName);
            //start Sandip
            if (!string.IsNullOrEmpty(_form.DateCreated1.RawText))
            {
                _entity.DateCreated = _form.DateCreated1.Date;
            }
            _entity.CreatedBy = _form.CreatedBy.Text;
            //End Sandip

            var ftpcontactvalues = _form.Ftp_Contact.RawText; 
            _entity.FtpContact = ftpcontactvalues;
            EntityManager.Transaction.Add(_entity);
            var contacts = _entity.CustomerContacts.Cast<CustomerContactsBase>().Where(c => c.CompanyName != _entity.CompanyName).ToList();
            foreach (var contact in contacts)
            {
                contact.CompanyName = _entity.CompanyName;
                contact.
                EntityManager.Transaction.Add(contact);
            }
            return base.OnPreSave();
        }



        //private string calculateYear()
        //{

        //    var crDate = "";
        //    if (_form.ModifiedOn.RawText!="")
        //    {

        //    DateTime zeroTime = new DateTime(1, 1, 1);
        //    DateTime dateModified = Convert.ToDateTime(_form.ModifiedOn.RawText);
        //    DateTime dateCreated = (DateTime)_form.DateCreated1.Date;
        //    if (dateModified!=null && dateCreated!=null)
        //    {
        //        int years = dateCreated.Year - dateModified.Year;
        //        //TimeSpan span = dateCreated - dateModified;
        //        //int years = (zeroTime + span).Year - 1;

              
        //        if (years > 1)
        //        {
        //            crDate = "1/1/2022";
        //        }
        //        else if (years <= 1)
        //        {
        //            crDate = "12/1/2022";
        //        }

        //      }

        //    }
        //    else
        //    { 
        //            crDate = DateTime.Now.ToString();
               
        //    }
        //    return crDate;
        //}

        void createCustomer(string fileName1, string supdocId)
        {
            LogInfo("Create Customer Method Starts");
            dynamic dataobj = new System.Dynamic.ExpandoObject();
            dynamic dataobj1 = new System.Dynamic.ExpandoObject();
            dynamic dataobj2 = new System.Dynamic.ExpandoObject();
            //New Changes Start



            //New Changes End
            dataobj.supdocid = supdocId;
            LogInfo("supdocId from Create customer Method" + supdocId);
            dataobj.name = _entity.CompanyName;
            dataobj.status = "active";
            dataobj.termname = "Net 30";
            dataobj.onhold = "false";
            //dataobj.delivery_options = "Print";
            dataobj.delivery_options = _entity.DeliveryOptions.PhraseText;
            // dataobj.currency = "USD";
            dataobj.currency = _entity.Currency.PhraseText;



            dataobj1.printas = _entity.CompanyName;
            dataobj1.companyname = "";
            dataobj1.prefix = "pre";
            dataobj1.firstname = _entity.FirstName;
            dataobj1.lastname = _entity.LastName;
            dataobj1.initial = _entity.Initial;
            dataobj1.phone1 = _entity.PhoneNum;
            dataobj1.email1 = _entity.Primaryemail;
            dataobj1.email2 = _entity.Secondaryemail;
            dataobj1.url1 = _entity.Url1;


            dataobj.displaycontact = dataobj1;


            dataobj2.address1 = _entity.Billingaddress1;
            dataobj2.city = _entity.Billingcity;
            dataobj2.state = _entity.Billingstate;
            dataobj2.zip = _entity.Billingzip;
            dataobj2.country = _entity.Billingcountry;

            dataobj1.mailaddress = dataobj2;
            //dataobj.mailaddress.add(dataobj2);

            //foreach(CustomerContactsBase i in _entity.CustomerContacts)
            //    {
            //        dynamic obj = new System.Dynamic.ExpandoObject();
            //        obj.categoryname = i.Name;

            //        dynamic obj2 = new System.Dynamic.ExpandoObject();
            //        obj2.name = i.Name;

            //        obj.contact = obj2;
            //        dataobj.contactlistinfo.Add(obj);
            //    }

            //foreach (var i in _entity.)
            //{
            List<System.Dynamic.ExpandoObject> obj2 = new List<System.Dynamic.ExpandoObject>();

            dynamic obj7 = new System.Dynamic.ExpandoObject();
            obj7.customfieldname = "LIMS_CODES";
            obj7.customfieldvalue = _entity.Identity;
            obj2.Add(obj7);

            //obj2[0]= new System.Dynamic.ExpandoObject();
            ////obj2[0].customfieldname = "LIMS_CODE";
            //obj2[0].customfieldvalue = _entity.Identity;

            dataobj.customfields = obj2;


            //New Changes End

            // }

            PostRequestForCreateCustomer(createCustomerUrl, dataobj);




        }


        public void PostRequestForCreateCustomer(string url, dynamic obj)
        {
            try
            {
                LogInfo("Post Request Method Of create customer Starts");

                dynamic obj3 = new System.Dynamic.ExpandoObject();
                obj3.intacctlogin = new System.Dynamic.ExpandoObject();
                obj3.intacctlogin.companyid = "deibellabs-imp";
                // obj3.intacctlogin.user = "lberrios";
                // obj3.intacctlogin.pwd = "Ldf999@2!";
                obj3.intacctlogin.user = "LIMSAdmin";
                obj3.intacctlogin.pwd = "iTS4K9S!EH0";
                obj3.intacctlogin.locationentityid = "";
                obj3.intacctlogin.useapisession = "false";
                obj3.intacctlogin.apisession = "UrpmrFm0miCDU_Uq8e024_nHIIJTgFK6PXe8qzQSg1PlKvHtNvXpxyCD";

                obj3.data = new System.Dynamic.ExpandoObject();
                obj3.data = obj;

                var value = Newtonsoft.Json.JsonConvert.SerializeObject(obj3);
                //LogInfo("Request data : " + value);

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                using (System.Net.Http.HttpClient client = new HttpClient())
                {
                    StringContent str = new StringContent(value);
                    HttpContent content = str;
                    content.Headers.Add("ApiKey", "BE8BF7076B73B39676884557650F8480");
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = client.PostAsync(url, content);
                    var result = response.Result;
                    // result.IsSuccessStatusCode
                    var res = result.Content.ReadAsStringAsync().Result;
                    dynamic value1 = Newtonsoft.Json.JsonConvert.DeserializeObject(res);
                    var x = value1.key;
                    LogInfo("PostrequestMethod of create customer Endsd");
                    LogInfo(res);
                    if (result.IsSuccessStatusCode)
                    {
                        Getcustomer(x);
                        // Library.Utils.PromptForFile()
                    }

                }
            }

            catch (Exception ex)
            {

            }


        }
        private byte[] GetBinaryFile(string name)
        {
            LogInfo("In to GetBinary File Method");
            byte[] bytes;
            using (FileStream file = new FileStream(name, FileMode.Open, FileAccess.Read))
            {
                bytes = new byte[file.Length];
                file.Read(bytes, 0, (int)file.Length);
            }
            return bytes;
        }
        public void LogInfo(string message)
        {
            string logFile = DateTime.Now.ToString("yyyyMMdd") + ".txt";
            if (!System.IO.File.Exists(logFile))
            {
                System.IO.File.Create(logFile);
            }
            // The first parameter, smp$userfiles, is in registry at HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\LabSystems\SampleManager Server\SMUAT
            var file = Library.File.GetWriteFile("smp$CustomerLogs", logFile);
            File.AppendAllText(file.FullName, message + "\r\n");
        }

        public async void Getcustomer(dynamic value)
        {
            //get the record no value from create customer post method result
            LogInfo("Get Customer Method Starts");
            var recordno = value;
            var GoogleMapsAPIkey = "BE8BF7076B73B39676884557650F8480";
            string url = "https://asgsageintacctapi.azurewebsites.net/api/Common/company/deibellabs-imp/user/LIMSAdmin/pwd/iTS4K9S!EH0/objectname/CUSTOMER/data?filter=RECORDNO='" + recordno + " ' + &fields=RECORDNO,CUSTOMERID,NAME";


            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            using (System.Net.Http.HttpClient client = new HttpClient())
            {
                HttpRequestMessage mesage = new HttpRequestMessage();
                mesage.RequestUri = new Uri(url);
                mesage.Method = HttpMethod.Get;
                mesage.Headers.Add("ApiKey", "BE8BF7076B73B39676884557650F8480");
                var response = client.SendAsync(mesage);
                var result = response.Result;
                var res = result.Content.ReadAsStringAsync().Result;
                var json = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                dynamic value1 = Newtonsoft.Json.JsonConvert.DeserializeObject(res);
                var xcustomerID = value1[0].CUSTOMERID;
                if (_entity.SiCustomerid != null)
                {
                    _entity.SiCustomerid = xcustomerID;
                    EntityManager.Transaction.Add(_entity);
                }
                EntityManager.Commit();
                _form.ButtonSubmit.Enabled = false;
                LogInfo("Get Customer Method Ends");
            }
        }
        //Changed by Bhavani
        protected override void OnPostSave()
        {
            base.OnPostSave();
        }

        #endregion

        #region Customer Components Grid

        private void ComponentsGrid_CellLeave(object sender, CellEventArgs e)
        {
            if (e.Entity == null)
                return;

            var comp = e.Entity as CustomerComponentsBase;
            if (comp == null)
                return;

            if (e.PropertyName == "Analysis")
            {
                // Fill components 
                var qc = EntityManager.CreateQuery(VersionedComponent.EntityName);
                qc.AddEquals(VersionedComponentPropertyNames.Analysis, comp.Analysis);
                _form.ebComponent.Republish(qc);

                // Fill component lists
                var q = EntityManager.CreateQuery(VersionedCLHeader.EntityName);
                q.AddEquals(VersionedCLHeaderPropertyNames.Analysis, comp.Analysis);
                var lists = EntityManager.Select(VersionedCLHeader.EntityName, q);
                var names = lists.Cast<VersionedCLHeader>().Select(c => c.CompList).ToList();
                names.Insert(0, "");
                _form.sbComponentLists.Republish(names);
            }
        }

        #endregion


        #region Inbound Tab

        private void GridXmlInbound_DataLoaded(object sender, EventArgs e)
        {
            UpdateInboundBrowses();
        }

        private void GridXmlInbound_FocusedRowChanged(object sender, DataGridFocusedRowChangedEventArgs e)
        {
            UpdateInboundBrowses();
        }

        private void GridXmlInbound_ValidateCell(object sender, DataGridValidateCellEventArgs e)
        {
            if (e.Column.Property != "TableName")
                return;

            UpdateInboundBrowses(e.Value.ToString());
        }

        void UpdateInboundBrowses(string tableName = "")
        {
            var grid = _form.gridXmlInbound;
            if (grid == null)
                return;

            var entity = grid.FocusedEntity as CustomerXmlInboundBase;
            if (entity == null)
                return;

            tableName = entity.TableName == "" ? tableName : entity.TableName;
            if (string.IsNullOrWhiteSpace(tableName))
                return;

            var ftpFields = _form.gridXmlInbound.GetColumnByProperty("FtpFieldName");
            var tableFields = _form.gridXmlInbound.GetColumnByProperty("TableFieldName");

            if (tableName == Sample.EntityName)
            {
                ftpFields.SetCellBrowse(entity, _fbFtpSample, false);
                tableFields.SetCellBrowse(entity, _fbSample, false);
            }
            else if (tableName == Test.EntityName)
            {
                ftpFields.SetCellBrowse(entity, _fbFtpTest, false);
                tableFields.SetCellBrowse(entity, _fbTest, false);
            }
        }

        private void BtnRefreshTemplate_Click(object sender, EventArgs e)
        {
            var writer = new DeibelXmlWriter(EntityManager, _entity);
            var template = writer.GetInboundTemplate(_entity.CustomerXmlInbounds.Cast<CustomerXmlInboundBase>().ToList(), "    ");
            _form.txtXmlTemplate.TextContent = template;
        }

        #endregion

        #region Outbound Tab

        private void GridXmlOutbound_DataLoaded(object sender, EventArgs e)
        {
            UpdateOutboundBrowses();
        }

        private void GridXmlOutbound_FocusedRowChanged(object sender, DataGridFocusedRowChangedEventArgs e)
        {
            UpdateOutboundBrowses();
        }

        private void GridXmlOutbound_ValidateCell(object sender, DataGridValidateCellEventArgs e)
        {
            if (e?.Column?.Property == null || e?.Column?.Property != "TableName")
                return;

            UpdateOutboundBrowses(e.Value.ToString());
        }

        void UpdateOutboundBrowses(string tableName = "")
        {
            var grid = _form.gridXmlOutbound;
            if (grid == null)
                return;

            var entity = grid.FocusedEntity as CustomerXmlOutboundBase;
            if (entity == null)
                return;

            tableName = entity.TableName == "" ? tableName : entity.TableName;
            if (string.IsNullOrWhiteSpace(tableName))
                return;

            DataGridColumn tableFields = null;
            try
            {
                tableFields = _form.gridXmlOutbound.GetColumnByProperty("TableFieldName");
            }
            catch { return; }
            FieldNameBrowse fbToUse = null;

            DataGridColumn xmlLevels;
            try
            {
                xmlLevels = _form.gridXmlOutbound.GetColumnByProperty("XmlNodeLevel");
            }
            catch { return; }
            StringBrowse sbToUse = null;

            if (tableFields == null || xmlLevels == null)
                return;

            switch (tableName)
            {
                case JobHeader.EntityName:
                    fbToUse = _fbJob;
                    sbToUse = _sbJobFieldsNodeLevel;
                    break;
                case Sample.EntityName:
                    fbToUse = _fbSample;
                    sbToUse = _sbSampleFieldsNodeLevel;
                    break;
                case Test.EntityName:
                    fbToUse = _fbTest;
                    sbToUse = _sbTestFieldsNodeLevel;
                    break;
                case Result.EntityName:
                    fbToUse = _fbResult;
                    sbToUse = _sbResultFieldsNodeLevel;
                    break;
                case FtpSampleBase.EntityName:
                    fbToUse = _fbFtpSample;
                    sbToUse = _sbFtpSampleFieldsNodeLevel;
                    break;
                case FtpTestBase.EntityName:
                    fbToUse = _fbFtpTest;
                    sbToUse = _sbFtpTestfieldsNodeLevel;
                    break;
            }

            if (fbToUse != null)
                tableFields.SetCellBrowse(entity, fbToUse, false);

            if (sbToUse != null)
                xmlLevels.SetCellBrowse(entity, sbToUse, false, false);
        }

        private void BtnPreviewOutbound_Click(object sender, EventArgs e)
        {
            var entity = _form.pebJobs.Entity as JobHeader;
            if (entity == null || string.IsNullOrWhiteSpace(entity.JobName))
                return;

            if (entity.Samples.Count == 0)
            {
                Library.Utils.FlashMessage("No samples on job", "Invalid data");
                return;
            }

            var samples = entity.Samples.Cast<Sample>().ToList();
            var tests = samples.SelectMany(s => s.Tests.ActiveItems).Cast<Test>().ToList();
            var writer = new DeibelXmlWriter(EntityManager, _entity);
            var xmlString = writer.GetOutboundXML(entity, samples, tests, indent: "    ");
            if (xmlString == string.Empty)
                Library.Utils.FlashMessage("No results that are valid per \"FTP Configuration\" setting", "");

            _form.txtXmlOutbound.TextContent = xmlString;
        }

        #endregion

    }
}

