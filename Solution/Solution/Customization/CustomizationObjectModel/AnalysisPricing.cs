using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Tasks;
namespace Customization.Tasks
{
    [SampleManagerTask("AnalysisPricing")]
    public class AnalysisPricing : SampleManagerTask
    {
        private FormDeibelAnalysisBilling _form;
        bool modifications = false;


        protected override void SetupTask()
        {
            _form = (FormDeibelAnalysisBilling)FormFactory.CreateForm(typeof(FormDeibelAnalysisBilling));
            _form.Created += _form_Created;
            _form.Show();
            
        }

        void _form_Created(object sender, EventArgs e)
        {
            // _form.RealEdit1.Enabled = true;
            var user = Library.Environment.CurrentUser as PersonnelBase;
            //if(user.RoleAssignments.Contains("ANALYSISPRICING"))
            var roles = user.RoleAssignments;
            foreach (RoleAssignmentBase role in roles)
            {
                if (role.EntityNameForFile == "SYSTEMANALYSISPRICING")
                {
                    modifications = true;
                }
            }

            _form.RealEdit2.Visible = false;
            _form.OutsourcedTest.CheckedChanged += OutsourcedTest_CheckedChanged;
            if (_form.OutsourcedTest.Checked == true)
            {
                _form.RealEdit2.Visible = true;
            }
            var selectedItem = (VersionedAnalysisBase)this.Context.SelectedItems.GetFirst();
            _form.PromptEntityBrowse1.Entity = selectedItem;
            _form.RealEdit1.Number = selectedItem.StandardPrice;
            if ((modifications) == false)
            {
                _form.RealEdit1.Enabled = false;
            }
            else
            {
                _form.RealEdit1.Enabled = true;
            }


            // if(roles.Contains("ANALYSISPRICING"))

            //{
            //    _form.RealEdit1.Enabled = true;
            //}
            //else
            //{
            //    _form.RealEdit1.Enabled = false;
            //}


            _form.ButtonOk.Click += ButtonOk_Click;
            _form.ButtonCancel.Click += ButtonCancel_Click;

        }

        private void OutsourcedTest_CheckedChanged(object sender, Thermo.SampleManager.Library.ClientControls.CheckEventArgs e)
        {
            if (_form.RealEdit2.Visible == false)
            {
                _form.RealEdit2.Visible = true;

                _form.RealEdit1.Enabled = false;
            }
            else
            {
                _form.RealEdit2.Visible = false;
                _form.RealEdit1.Enabled = true;
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            _form.Close();
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            var selectedAnalysis = (VersionedAnalysisBase)_form.PromptEntityBrowse1.Entity;
            var pricing = _form.RealEdit1.Number;
            var region = _form.PromptEntityBrowse2.RawText;
            var labMarkupPrice = _form.LabMarkUpPrice.Number;
            if(_form.LabMarkUpPrice.RawText!=null)
            {
                selectedAnalysis.LabMarkupPrice = labMarkupPrice;
            }

            if (_form.OutsourcedTest.Checked)
            {
                if (_form.RealEdit2.RawText != null)
                {
                    selectedAnalysis.OutsourcedTestPrice = _form.RealEdit2.Number;
                    selectedAnalysis.OutsourcedTestType = true;
                }
            }
            else
            {
                selectedAnalysis.StandardPrice = pricing;
                selectedAnalysis.OutsourcedTestType = false;
            }
            EntityManager.Transaction.Add(selectedAnalysis);
            EntityManager.Commit();
            _form.Close();


        }
    }
}
