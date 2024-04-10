using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(SampFieldChangerTask))]
    public class SampFieldChangerTask : SampleManagerTask
    {
        FormSampFieldChanger _form;
        List<Sample> _samples;

        protected override void SetupTask()
        {
            if (Context.SelectedItems.ActiveCount == 0)
                return;

            _form = FormFactory.CreateForm<FormSampFieldChanger>();
            _form.Loaded += _form_Loaded;
            _form.Show();
        }

        private void _form_Loaded(object sender, EventArgs e)
        {
            _samples = Context.SelectedItems.ActiveItems.Cast<Sample>().ToList();
            _samples[0].Lock();
            _form.btnChange.Click += BtnChange_Click;
        }

        private void BtnChange_Click(object sender, EventArgs e)
        {
            var customer = _form.pebCustomer.Entity as Customer;
            var job = _form.pebJob.Entity as JobHeader;

            if(customer == null && job == null)
            {
                Library.Utils.FlashMessage("No entities selected", "Invalid input");
                return;
            }

            foreach(var sample in _samples)
            {
                if (customer != null)
                    sample.CustomerId = customer;
                if (job != null)
                    sample.JobName = job;

                EntityManager.Transaction.Add(sample);
            }
            EntityManager.Commit();
            _samples[0].LockRelease();
            Library.Utils.FlashMessage("Transaction completed.", "");
        }
    }
}
