using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(TestPropertySheet))]
    class TestPropertySheet : SampleManagerTask
    {
        FormUCIFTest _form;

        protected override void SetupTask()
        {
            _form = FormFactory.CreateForm<FormUCIFTest>();
            _form.Loaded += _form_Loaded;
            _form.Show();

        }
        protected override bool OnPreSave()
        {
            //if (analysisOrder>0)
            //{
            //    _entity.AnalysisOrder = analysisOrder;
            //  EntityManager.Transaction.Add(_entity);
            //}

            EntityManager.Commit();
            return base.OnPreSave();
        }
        private void _form_Loaded(object sender, EventArgs e)
        {
            //_form.btnSubmit.Click += btnSubmit_Click;

            //_form.ddl_Analysis.EntityChanged += ddl_Analysis_EntityChanged;
            //_form.PromptString_CompName.StringChanged += PromptString_CompName_StringChanged;

            //_form.DataGridDesign1.UnboundColumnValueChanged += DataGridDesign1_GridCellChanged;
        }
    }
}
