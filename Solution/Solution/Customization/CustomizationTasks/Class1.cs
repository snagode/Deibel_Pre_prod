using Customization.Tasks;
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
    /// <summary>
    /// Sample login grid.  This is assigned to an explorer folder and used to log
    /// samples in using data in table web_sample_queue.  
    /// </summary>
    [SampleManagerTask(nameof(TestSchedule1))]
    public class TestSchedule1 : TestScheduleTask
    {

        FormTestSchedule _form;
        
        protected override void MainFormCreated()
        {
            base.MainFormCreated();
        }
        protected override void MainFormLoaded()
        {
            base.MainFormLoaded();

            var val = (FormTestSchedule)MainForm;
            val.GridTestSchedEntries.BeforeRowAdd += GridTestSchedEntries_BeforeRowAdd;
            val.GridTestSchedEntries.BeforeRowDelete += GridTestSchedEntries_BeforeRowDelete;


        }

        private void GridTestSchedEntries_BeforeRowDelete(object sender, DataGridBeforeRowDeletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void GridTestSchedEntries_BeforeRowAdd(object sender, DataGridBeforeRowAddedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}