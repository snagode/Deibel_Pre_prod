﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;
using System.Net.Mail;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(JobAuthorisedNotification), "WorkflowCallback")]
    public class JobAuthorisedNotification : SampleManagerTask
    {
        string url = "http://52.165.193.61:3005/v1/alert-notification";
        protected override void SetupTask()
        {
            base.SetupTask();
            if (Context.SelectedItems.ActiveCount == 0)
            { Exit(false); return; }
            var job = (JobHeader)Context.SelectedItems[0];
            var JobName = job.JobName.ToString();
           // var CompanyName = " ";
            // dynamic dataobj = new System.Dynamic.ExpandoObject();
            dynamic obj1 = new List<dynamic>();
            if (job != null)
            {

                var sample = job.Samples.ActiveItems.Cast<Sample>().ToList();

                foreach (var s in sample)
                {
                    dynamic obj2 = new System.Dynamic.ExpandoObject();
                    obj2.id_text = s.IdText;
                    obj2.samp_batch_number = s.SampBatchNumber;
                    obj2.description = s.Description;
                    obj2.description_b = s.DescriptionB;
                    obj2.description_c = s.DescriptionC;

                    obj1.Add(obj2);
                }


            }
            PostRequest(url, obj1, JobName);
            Exit(true);
        }

        public void PostRequest(string url, dynamic obj, String JOBNAME)
        {

            try
            {


                dynamic obj3 = new System.Dynamic.ExpandoObject();
                obj3.type = "job";
                obj3.job_status = "A";
                obj3.job_name = JOBNAME;
                obj3.company_name = "";

                obj3.data = obj;

                var value = Newtonsoft.Json.JsonConvert.SerializeObject(obj3);


                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                using (HttpClient client = new HttpClient())
                {
                    StringContent str = new StringContent(value);
                    HttpContent content = str;
                    //content.Headers.Add("ApiKey", "53dFsLkzL88rZpDpjqGCYeLD0qyHT6CoH4BhNBO3UPAzSeCVQ5r2");
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = client.PostAsync(url, content);
                    var result = response.Result;

                    var res = result.Content.ReadAsStringAsync().Result;
                    dynamic output = Newtonsoft.Json.JsonConvert.DeserializeObject(res);

                }
            }

            catch (Exception ex)
            {

            }


        }
    }
}