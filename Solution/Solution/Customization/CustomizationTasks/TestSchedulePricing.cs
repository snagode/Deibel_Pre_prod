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
    [SampleManagerTask("TestSchedulePricing")]
    public class TestSchedulePricing : SampleManagerTask
    {
        private FormDeibelTestSchedule _form;
        bool modifications = false;


        protected override void SetupTask()
        {
            _form = (FormDeibelTestSchedule)FormFactory.CreateForm(typeof(FormDeibelTestSchedule));
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
            var selectedItem = (TestSchedHeaderBase)this.Context.SelectedItems.GetFirst();
            _form.TestScheduleBrowsePrompt.Entity = selectedItem;
            _form.OutsourcedLabPrice.Visible = false;
            _form.RealEditLabMarkUpPrice.Visible = false;
            if (_form.CheckEditOutsourcedTestSchedule.Checked == true)
            {
                _form.OutsourcedLabPrice.Visible = true;
                _form.RealEditLabMarkUpPrice.Visible = true;
            }
            _form.CheckEditOutsourcedTestSchedule.CheckedChanged += CheckEditOutsourcedTestSchedule_CheckedChanged;
            _form.ButtonOk.Click += ButtonOk_Click;
            _form.ButtonCancel.Click += ButtonCancel_Click;
        }

        private void CheckEditOutsourcedTestSchedule_CheckedChanged(object sender, Thermo.SampleManager.Library.ClientControls.CheckEventArgs e)
        {
            if (_form.OutsourcedLabPrice.Visible == false)
            {
                _form.OutsourcedLabPrice.Visible = true;
                _form.RealEditLabMarkUpPrice.Visible = true;

                _form.RealEditPrice.Enabled = false;
            }
            else
            {
                _form.OutsourcedLabPrice.Visible = false;
                _form.RealEditLabMarkUpPrice.Visible = false;
                _form.RealEditPrice.Enabled = true;
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            _form.Close();
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            var SelectedTestSchedule = (TestSchedHeaderBase)_form.TestScheduleBrowsePrompt.Entity;
            var Price = _form.RealEditPrice.Number;
            var labMarkupPrice = _form.RealEditLabMarkUpPrice.Number;
            var Region = _form.PromptEntityBrowse2.RawText;
            if (_form.RealEditLabMarkUpPrice.RawText != null)
            {
                SelectedTestSchedule.LabMarkupPrice = labMarkupPrice;
            }

            if (_form.CheckEditOutsourcedTestSchedule.Checked)
            {
                if(_form.OutsourcedLabPrice.RawText!=null)
                {
                    SelectedTestSchedule.OutsourcedTestPrice = _form.OutsourcedLabPrice.Number;
                    SelectedTestSchedule.OutsourcedTestType = true;
                }
                else
                {
                    SelectedTestSchedule.StandardPrice = Price;
                    SelectedTestSchedule.OutsourcedTestType = false;
                }
               
            }
            SelectedTestSchedule.StandardPrice = Price;
            EntityManager.Transaction.Add(SelectedTestSchedule);
            EntityManager.Commit();
            _form.Close();

        }
    }
}
