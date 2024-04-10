using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Thermo.SampleManager.Common;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Library.ObjectModel;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;

namespace Customization.Tasks
{
    [SampleManagerTask("ResultSpecCallback", "WorkflowCallback")]
    public class WorkflowResultSpecTask : SampleManagerTask
    {
        // Members
        Result _result;

        protected override void SetupTask()
        {
            base.SetupTask();
            if (Context.SelectedItems.Count > 0)
            {
                _result = (Result)Context.SelectedItems[0];
            }
            else
                Exit();

            if(_result != null && !_result.IsNull())
            {
                if (HasSpecComp())
                    _result.SpecStatus = _result.OutOfRange ? "FAIL" : "PASS";
                else
                    _result.SpecStatus = "NO_SPEC";
            }
            
            Exit();
        }

        bool HasSpecComp()
        {
            

          var product = _result.TestNumber.Sample.Product;
            var analysis = _result.TestNumber.Analysis.Identity;
            var component = _result.ResultName;

            var q = EntityManager.CreateQuery(MlpView.EntityName);
            q.AddEquals(MlpViewPropertyNames.ProductId, product);
            q.AddEquals(MlpViewPropertyNames.AnalysisId, analysis);
            q.AddEquals(MlpViewPropertyNames.ComponentName, component);
            var specRow = EntityManager.Select(MlpView.EntityName, q).GetFirst();

            bool hasSpec = true;
            if (specRow == null || specRow.IsNull())
                hasSpec = false;

            return hasSpec;
        }
    }
}