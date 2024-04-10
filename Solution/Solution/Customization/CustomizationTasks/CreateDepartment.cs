using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common;
using Thermo.SampleManager.Common.CommandLine;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Utilities;
using Thermo.SampleManager.Core.Definition;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(CreateDepartment), "LABTABLE", "USER_DEPARTMENT")]
    public class CreateDepartment : DefaultFormTask
    {
        FormCreateDepartment _form;
        protected override void SetupTask()
        {
            _form = FormFactory.CreateForm<FormCreateDepartment>();
            _form.Loaded += _form_Loaded;
            _form.Show();
        }
        private void _form_Loaded(object sender, EventArgs e)
        {

            _form.ActionButton1.BeforeAction += ActionButton1_BeforeAction;
           // _form.DepartmentType.PhraseChanged += DepartmentType_PhraseChanged;
            //_form.DepartmentType.TextChanged += DepartmentType_TextChanged;
           // _form.DepartmentType.Leave += DepartmentType_Leave;
            
        }

        private void DepartmentType_Leave(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DepartmentType_TextChanged(object sender, TextChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DepartmentType_PhraseChanged(object sender, EntityChangedEventArgs e)
        {
            
        }

        private void ActionButton1_BeforeAction(object sender, ControlCancelEventArgs e)
        {
            var DepartmentType = _form.DepartmentType.RawText;
            var DepartmentId = Convert.ToInt32(_form.SMPrompt2.RawText);


            // var phrase = EntityManager.SelectPhrase(PhraseDeibelDep.Identity, PhraseDeibelDep.PhraseIdADVISORY);


            //var e1 = EntityManager.CreateEntity(UserDepartmentBase.EntityName) as UserDepartmentBase;
            // e1.Departmenttype = DepartmentType;
            //e1.Deptid = DepartmentId;
            //EntityManager.Transaction.Add(e1);
            //EntityManager.Commit();

        }
    }
}
