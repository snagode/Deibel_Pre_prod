using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(AutoPopulateTestWorkflow), "WorkflowCallback")]
   public class AutoPopulateTestWorkflow : SampleManagerTask
    {
        protected override void SetupTask()
        {
            try
            {
                //var tests = Context.SelectedItems[0] as Test;
                //if (tests.Analysis.Name== "APC_NB" && tests.Analysis.Name == "EC-C_VRBA_MUG_NB" && tests.Analysis.Name == "S_AUREUS_NB" && tests.Analysis.Name == "YM_NB")
                //{
                //    var sample = (Sample)tests.Sample;
                //    var a = sample.Tests;
                //    //var b=a.Select(x=>x.Analysis.Identity== "INDICATOR_BUNDLE_LW1");
                  
                //    var query = EntityManager.CreateQuery(VersionedAnalysis.EntityName);
                //    query.AddEquals(VersionedAnalysisPropertyNames.Identity, "INDICATOR_BUNDLE_LW1");
                   
                //    var test = (VersionedAnalysisInternal)EntityManager.Select(query).ActiveItems.FirstOrDefault(); 
                //    List<TestInternal> newTests = sample.AddTest(test);
                //    if (test!=null)
                //    {
                //        EntityManager.Commit();
                //    }
                //    Exit(true);
                //}
            }
            catch (Exception ex)
            {
               // ExceptionWrite(ex);
            }
        }
        private void ExceptionWrite(Exception ex)
        {
            //string logFilePath = Library.Environment.GetFolderList("smp$dbl_Web_Queue_Export_LogFilePath") + "ErrorLog\\".ToString();
            //Common.WriteLog(logFilePath, $"{DateTime.Now.ToShortTimeString()}:" + ex.Message + "\r\n");
        }
    }
}
