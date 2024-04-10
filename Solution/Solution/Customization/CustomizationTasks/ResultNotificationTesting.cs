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
    [SampleManagerTask(nameof(ResultNotificationTesting), "WorkflowCallback")]
    public class ResultNotificationTesting : SampleManagerTask
    {
        private string context = "";
        private string message = "";
        protected override void SetupTask()
        {
            try
            {
                base.SetupTask();
                if (Context.SelectedItems.ActiveCount == 0)
                { Exit(false); return; }
                
                var sample = Context.SelectedItems[0] as Sample;
                var test = Context.SelectedItems[0] as Test;
                
                if (sample != null)
                {
                    context = "Sample " + sample.IdText + " with Analysis ";
                    var _tests = sample.Tests.ActiveItems.Cast<Test>().Where(x => x.Status.PhraseId != "X").ToList();
                    var resultsAllTest = _tests.SelectMany(t => t.Results.Cast<Result>()).ToList();
                    foreach (Test _test in _tests)
                    {
                        if (!CheckResultComponents(_test))
                            message += _test.Analysis.Name + "\n";
                    }
                    if (!string.IsNullOrEmpty(message))
                    {
                        Library.Utils.FlashMessage(context + "\n" + message + "There are tests that have partial, or no results entered.", "");
                       
                        Exit(false);
                        sample.SetStatus(PhraseSampStat.PhraseIdV);
                        sample.JobName.SetJobStatus(PhraseSampStat.PhraseIdV);
                        
                    }
                }

                  Exit(true);
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message + ex.StackTrace);
            }
        }
        private bool CheckResultComponents(Test test)
        {
            var q = EntityManager.CreateQuery(VersionedCLEntryBase.EntityName);
            if (!string.IsNullOrEmpty(test.ComponentList))
            {
                q.AddEquals(VersionedCLEntryPropertyNames.CompList, test.ComponentList);
            }
            q.AddEquals(VersionedCLEntryPropertyNames.Analysis, test.Analysis);
            var compList = EntityManager.Select(q).ActiveItems.Cast<VersionedCLEntryBase>().Distinct().ToList();
            var a = test.Analysis.Components.ActiveItems.Cast<VersionedComponent>().Where(s => compList.Select(e => e.Name).Contains(s.Name)).ToList();
            if (a.Count() == 0)
            {
                a = test.Analysis.Components.Cast<VersionedComponent>().ToList();
            }
            if (a.All(x => test.Results.ActiveItems.Cast<Result>().Where(d => !string.IsNullOrEmpty(d.Text)).Select(b => b.Name).Contains(x.Name)))
            {
                return true;
            }
            else
            {
               var q1 = a.All(x => test.Results.ActiveItems.Cast<Result>().Select(b => b.Name).Contains(x.Name));
               test.SetStatus(PhraseSampStat.PhraseIdV);
                var s = test.Results.ActiveItems.Cast<Result>().ToList();
                foreach (var item in s)
                {
                    if (item.Status.PhraseId== "X")
                    {
                        item.SetStatus(PhraseReslStat.PhraseIdU);
                      //  item.SetStatus(item.OldStatus.PhraseId);
                    }
                    
                }


                return false;
               
            }


        }
        private void WriteLog(String ex)
        {
            string logFilePath = Library.Environment.GetFolderList("smp$dbl_DevLogFilesPath") + "\\ResultEntryNotificationLog";
            Common.WriteLog(logFilePath, $"{DateTime.Now.ToShortTimeString()}:" + ex + "\r\n");
        }
    }
}
