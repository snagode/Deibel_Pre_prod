using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(NotificationResultEmpty), "WorkflowCallback")]
    class NotificationResultEmpty : SampleManagerTask
    {
        private string context = "";
        private string message = "";

        protected override void SetupTask()
        {

            base.SetupTask();
            if (Context.SelectedItems.ActiveCount == 0)
            { Exit(false); return; }

            var job = Context.SelectedItems[0] as JobHeader;
            var sample = Context.SelectedItems[0] as Sample;
            var test = Context.SelectedItems[0] as Test;

            try
            {

                //if (job != null)
                //{
                //    // context = "Job " + job.JobName;
                //    var _samples = job.Samples.ActiveItems.Cast<Sample>().ToList();

                //    foreach (Sample _sample in _samples)
                //    {

                //        var tests = _sample.Tests.Cast<Test>().Where(x => x.Status.PhraseId != "X");
                //        message = string.Empty;
                //        foreach (Test _test in tests)
                //        {
                //            if (!CheckResultComponents(_test))
                //                message += _test.Analysis.Name + "\n";
                //        }
                //        if (!string.IsNullOrEmpty(message))
                //            context += "Sample " + _sample.IdText + " with Analysis\n " + message + "\n";
                //    }
                //    if (!string.IsNullOrEmpty(context))
                //        if (!Library.Utils.FlashMessageYesNo(context + " has a missing result. Are you sure you want to authorize?", ""))
                //            Exit(false);

                //}
              //  else
                 if (sample != null)
                {
                    context = "Sample " + sample.IdText + " with Analysis ";
                    var _tests = sample.Tests.ActiveItems.Cast<Test>().Where(x => x.Status.PhraseId != "X").ToList();
                    // var compList = _tests.SelectMany(x => x.Analysis.Components.ActiveItems.Cast<VersionedComponentBase>()).ToList();
                    var resultsAllTest = _tests.SelectMany(t => t.Results.Cast<Result>()).ToList();

                    //Demo(compList, resultsAllTest);
                    foreach (Test _test in _tests)
                    {
                        // Demo(_test);
                        if (!CheckResultComponents(_test))
                            message += _test.Analysis.Name + "\n";
                    }
                    if (!string.IsNullOrEmpty(message))
                    {
                        //if (job != null) job.LockRelease();
                        //if (sample != null) sample.LockRelease();
                        //if (test != null) test.LockRelease();

                        //  if (!Library.Utils.FlashMessageYesNo(context +"\n"+ message + "There are tests that have partial, or no results entered. Are you sure you want to continue?", ""))
                        Library.Utils.FlashMessage(context + "\n" + message + "There are tests that have partial, or no results entered.", "");
                        //Exit(false);
                        
                        
                        foreach (Test _test in _tests)
                        {
                            _test.SetStatus(_test.OldStatus.PhraseId);

                            var resultsAllTest1 = _test.Results.Cast<Result>().ToList();//Select(t => t.Results.Cast<Result>()).ToList();
                           // Library.Utils.FlashMessage("Test Status=" + _test.Status.PhraseId + "  TEST Old status=" + _test.OldStatus.PhraseId, "");
                            foreach (var item in resultsAllTest1)
                            {
                                 //Library.Utils.FlashMessage("Result Status=" + item.Status.PhraseId + "Result Old status=" + item.OldStatus.PhraseId, "");
                                if (item.Status.PhraseId == "X" && item.Status.PhraseId!="C")
                                {
                                    item.SetStatus(item.OldStatus.PhraseId);
                                }

                            }

                        }
                        sample.SetStatus(sample.OldStatus.PhraseId);
                        sample.JobName.SetJobStatus(sample.JobName.OldStatus.PhraseId);
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

            // var testcmpList = test.Analysis.Components.ActiveItems.Cast<VersionedComponent>().Select(x =>(object)x.Name).Distinct().ToList();
            var q = EntityManager.CreateQuery(VersionedCLEntryBase.EntityName);
            if (!string.IsNullOrEmpty(test.ComponentList))
            {
                q.AddEquals(VersionedCLEntryPropertyNames.CompList, test.ComponentList);
            }
            q.AddEquals(VersionedCLEntryPropertyNames.Analysis, test.Analysis);
            var compList = EntityManager.Select(q).ActiveItems.Cast<VersionedCLEntryBase>().Distinct().ToList();
            //  var testComp = test.Analysis.Components.Cast<VersionedComponent>().ToList();
            //if (!test.Analysis.Components.ActiveItems.Cast<VersionedComponent>().Any(x => !test.Results.ActiveItems.Cast<Result>().Select(b => b.Name).Contains(x.Name)))

            var a = test.Analysis.Components.ActiveItems.Cast<VersionedComponent>().Where(s => compList.Select(e => e.Name).Contains(s.Name)).ToList();
            if (a.Count() == 0)
            {
                a = test.Analysis.Components.Cast<VersionedComponent>().ToList();
            }

            // var aa = test.Results.ActiveItems.Cast<Result>().Where(d => !string.IsNullOrEmpty(d.Text));
            // if (!a.Any(x => test.Results.ActiveItems.Cast<Result>().Select(b => b.Name).Contains(x.Name)))

            // var t = a.All(x => test.Results.ActiveItems.Cast<Result>().Where(d => !string.IsNullOrEmpty(d.Text)).Select(b => b.Name).Contains(x.Name));
            if (a.All(x => test.Results.ActiveItems.Cast<Result>().Where(d => !string.IsNullOrEmpty(d.Text)).Select(b => b.Name).Contains(x.Name)))

                //if (Library.Utils.FlashMessageYesNo(context + " has a missing result for the following test " + test.Analysis.Name + ".   Are you sure you want to authorize?", ""))
                return true;
            else
                return false;
        }
        private void WriteLog(String ex)
        {
            string logFilePath = Library.Environment.GetFolderList("smp$dbl_DevLogFilesPath") + "\\ResultEntryNotificationLog";
            Common.WriteLog(logFilePath, $"{DateTime.Now.ToShortTimeString()}:" + ex + "\r\n");
        }
    }
}
