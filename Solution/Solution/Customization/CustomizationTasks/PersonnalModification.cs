﻿using System;
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
    [SampleManagerTask(nameof(PersonnalModification), "LABTABLE", "PERSONNEL")]
    public class PersonnalModification : DefaultFormTask
    {
        FormPersonnel _form;
        protected override void SetupTask()
        {
            _form = FormFactory.CreateForm<FormPersonnel>();
            _form.Loaded += _form_Loaded;
            _form.Show();
        }
        private void _form_Loaded(object sender, EventArgs e)
        {
           // _form.SMPrompt5.Leave += SMPrompt5_Leave;
        }

        private void SMPrompt5_Leave(object sender, EventArgs e)
        {
           // var value = _form.SMPrompt5.RawText;
        }
    }
}