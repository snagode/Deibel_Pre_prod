using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(WebSampleTask), "LABTABLE", "WEB_SAMPLE_QUEUE")]
    public class WebSampleTask : GenericLabtableTask
    {
        FormWebSampleQueue _form;

        protected override void MainFormLoaded()
        {
            if (MainForm.Entity == null)
                return;

            _form = MainForm as FormWebSampleQueue;
            // _form.ButtonEdit1.Click += ButtonEdit1_Click;

            var sample = (WebSampleQueueBase)Context.SelectedItems.ActiveItems[0];
            var sampleId = sample.SampleOrderId;
            // var tests = sample.Tests;
            // _form.
            // WebSampleQueueBase item = (WebSampleQueueBase)Context.SelectedItems.ActiveItems[0];
            //var webJobOrderId = item.WebJobOrder;
            var q = EntityManager.CreateQuery(TableNames.WebSampleQueue);
            q.AddEquals(WebSampleQueuePropertyNames.SampleOrderId, sampleId);
            var itemscount = EntityManager.Select(q).ActiveCount;
            var items = EntityManager.Select(q).ActiveItems.Cast<WebSampleQueueBase>().ToList();
            foreach (var item in items)
            {
                var row = _form.UnboundGridDesign1.AddRow();
                row.SetValue("AnalysisId", item.AnalysisId);
                row.SetValue("ComponentName", item.ComponentName);
                row.SetValue("ComponenetList", item.ComponentList);

            }
            // for(int i =0;i<itemscount;i++)
            //  {

            // row.SetVal("AnalysisID",items.)
            //  _form.UnboundGridDesign1.RowAdded += UnboundGridDesign1_RowAdded;
            // }
            //foreach(var row in _form.UnboundGridDesign1.Rows)
            //{
            //   // row = items[0];
            //}
            //var items =  EntityManager.Select(q).ActiveItems.ToList().Cast<WebSampleQueueBase>();






        }

        //private void UnboundGridDesign1_RowAdded(object sender, Thermo.SampleManager.Library.ClientControls.UnboundGridRowAddedEventArgs e)
        //{

        //    throw new NotImplementedException();
        //}

        //private void ButtonEdit1_Click(object sender, EventArgs e)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
