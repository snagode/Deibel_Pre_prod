using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;

namespace Customization.Tasks
{
    [SampleManagerTask("CustomerTask1", "LABTABLE1", "CUSTOMER1")]
    public partial class CustomerPricingTask : CustomerTask
    {
        #region Members

        private Customer _entity;
        private FormCustomer _form;

        #endregion

        protected override void MainFormLoaded()
        {
            base.MainFormLoaded();
            _entity = (Customer)MainForm.Entity;
            _form = (FormCustomer)MainForm;
        }
    }
}
