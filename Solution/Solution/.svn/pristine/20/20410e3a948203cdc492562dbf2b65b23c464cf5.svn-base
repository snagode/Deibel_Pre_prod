using System;
using System.Collections.Generic;
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
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(FtpSampleResetLoginFlag))]
    public class FtpSampleResetLoginFlag : SampleManagerTask
    {

        protected override void SetupTask()
        {
            var selectedItem = (FtpSampleBase)Context.SelectedItems[0];
            var selectedItems = Context.SelectedItems.Cast<FtpSampleBase>().ToList();
            var LoginFlagStatus = selectedItem.LoginFlag;
            if (LoginFlagStatus)
            {
                selectedItem.LoginFlag = false;
            }
            else
            {
                selectedItem.LoginFlag = true;
            }
            EntityManager.Transaction.Add(selectedItem);
            EntityManager.Commit();
        }
    }
}
