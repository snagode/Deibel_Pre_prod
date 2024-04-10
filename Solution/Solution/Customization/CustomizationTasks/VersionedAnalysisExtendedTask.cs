using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Library;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Customization.Tasks
{
    [SampleManagerTask("VersionedAnalysisTask", "LABTABLE", "VersionedAnalysis")]
    public class VersionedAnalysisExtendedTask : VersionedAnalysisTask
    {
        private FormVersionedAnalysis _form;
        private VersionedAnalysis _entity;
        //private string url = "https://deibellabs.azurewebsites.net/api/InventoryControl/CreateItem";
        //private string modifyurl = "https://deibellabs.azurewebsites.net/api/InventoryControl/UpdateItem";

        string url = "https://asgsageintacctapi.azurewebsites.net/api/InventoryControl/CreateItem";
        string modifyurl = "https://asgsageintacctapi.azurewebsites.net/api/InventoryControl/UpdateItem";

        protected override void MainFormLoaded()
        {
            base.MainFormLoaded();
            _entity = (VersionedAnalysis)MainForm.Entity;
            _form = (FormVersionedAnalysis)MainForm;

            _form.ButtonEdit1.Click += ButtonEdit1_Click;


            _entity.AllowMultipleTaxgrp = false;
            _entity.StandardCost = "0.00";
            _entity.TaxGrp = "HST Tax";
            _entity.Taxable = true;
            _entity.UomGroup = "Each";
            _entity.ItemType = "Non-Inventory (Sales only)";
            _entity.BasePrice = "0.01";

            if (string.IsNullOrEmpty(_entity.SiItemid))
                _form.ButtonEdit1.Enabled = true;
            else
                _form.ButtonEdit1.Enabled = false;

        }

        private void ButtonEdit1_Click(object sender, EventArgs e)
        {
            dynamic obj = prepareBillingFields();
            if (callAPI(obj))
            //callAPIModify(obj);
            {
                this.EntityManager.Transaction.Add(_entity);
                this.EntityManager.Commit();
                _form.ButtonEdit1.Enabled = false;
            }


        }

        //protected override bool OnPreSave()
        //{
        //    return base.OnPreSave();
        //}

        private bool callAPI(dynamic obj)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var value = Newtonsoft.Json.JsonConvert.SerializeObject(obj);

            using (HttpClient client = new HttpClient())
            {
                StringContent str = new StringContent(value);
                HttpContent content = str;
                content.Headers.Add("ApiKey", "BE8BF7076B73B39676884557650F8480");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = client.PostAsync(url, content);
                var result = response.Result;
                var res = result.Content.ReadAsStringAsync().Result;
                dynamic output = Newtonsoft.Json.JsonConvert.DeserializeObject(res);


                if (output.error == null)
                {
                    var itemID = output.key;
                    _entity.SiItemid = itemID;
                    Library.Utils.FlashMessage("Updated successfully", "Updated successfully");
                    return true;
                }
                else
                {
                    Library.Utils.FlashMessage(output.error.Value, "Sage Intacct Integration error");
                    return false;
                }
            }

        }

        private void callAPIModify(dynamic obj)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var value = Newtonsoft.Json.JsonConvert.SerializeObject(obj);

            using (HttpClient client = new HttpClient())
            {
                StringContent str = new StringContent(value);
                HttpContent content = str;
                content.Headers.Add("ApiKey", "BE8BF7076B73B39676884557650F8480");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = client.PostAsync(modifyurl, content);
                var result = response.Result;
                var res = result.Content.ReadAsStringAsync().Result;
                dynamic output = Newtonsoft.Json.JsonConvert.DeserializeObject(res);


                if (output.error == null)
                {
                    var itemID = output.key;
                    Library.Utils.FlashMessage("Updated successfully", "Updated successfully");
                    _entity.SiItemid = itemID;
                }
                else
                {
                    Library.Utils.FlashMessage(output.error.Value, "Sage Intacct Integration error");
                   // _form.ButtonEdit1.Enabled = false;
                }
            }

        }

        private dynamic prepareBillingFields()
        {
            dynamic obj = new System.Dynamic.ExpandoObject();
            dynamic objintacctlogin = new System.Dynamic.ExpandoObject();
            dynamic objdata = new System.Dynamic.ExpandoObject();
            List<System.Dynamic.ExpandoObject> objcustomfields = new List<System.Dynamic.ExpandoObject>();
            dynamic objcustomfields_Content = new System.Dynamic.ExpandoObject();

            objintacctlogin.companyid = "deibellabs-imp";
            objintacctlogin.user = "LIMSAdmin";
            objintacctlogin.pwd = "iTS4K9S!EH0";
            objintacctlogin.locationentityid = "";
            objintacctlogin.useapisession = "false";
            objintacctlogin.apisession = "FsH8Aon_U3KJG_9hTc5JEqMOcogbihbBp9ls4f1AiRvvYU3OSQSjDnKJ";

            obj.intacctlogin = objintacctlogin;

            objdata.itemid = _entity.Identity;
            //objdata.itemid = _entity.Identity + DateTime.Now.ToString("H:mm"); // to be commented, for testing
            objdata.name = _entity.Name;
            objdata.status = _entity.Active ? "active" : "inactive";
            objdata.itemtype = _entity.ItemType;
            objdata.productlineid = _entity.ProductLine;
            objdata.extended_description = _entity.Description;
            objdata.uomgrp = _entity.UomGroup;
            objdata.glgroup = _entity.GlGroup;
            objdata.standard_cost = _entity.StandardCost;
            objdata.baseprice = _entity.BasePrice;
            objdata.taxable = _entity.Taxable.ToString().ToLower();
            objdata.allowmultipletaxgrps = _entity.AllowMultipleTaxgrp.ToString().ToLower();
            objdata.taxgrp = _entity.TaxGrp;
            objdata.specification1 = _entity.Method;
            objdata.specification2 = _entity.Reference;
            objdata.specification3 = _entity.ReportingUnits;

            objcustomfields_Content.customfieldname = "ANALYSIS_TYPE";
            objcustomfields_Content.customfieldvalue = _entity.SiAnalysisType;
            objcustomfields.Add(objcustomfields_Content);
            objdata.customfields = objcustomfields;

            obj.data = objdata;

            return obj;

        }
    }
}
