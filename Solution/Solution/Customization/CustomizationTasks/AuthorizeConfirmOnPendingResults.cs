using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;


namespace Customization.Tasks
{
    [SampleManagerTask(nameof(AuthorizeConfirmOnPendingResults), "WorkflowCallback")]
    public class AuthorizeConfirmOnPendingResults : SampleManagerTask
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

            if (job != null)
            {
                // context = "Job " + job.JobName;
                var _samples = job.Samples.ActiveItems.Cast<Sample>().ToList();//.SelectMany(x => x.Tests.ActiveItems.Cast<Test>()).ToList();
                                                                               // var _sampleids = job.Samples.ActiveItems.Cast<Sample>().ToList().Select(c => c.IdText).Distinct();
                foreach (Sample _sample in _samples)
                {
                    message = string.Empty;
                    foreach (Test _test in _sample.Tests)
                    {
                        if (!CheckResultComponents(_test))
                            message += _test.Analysis.Name + " ";
                    }
                    if (!string.IsNullOrEmpty(message))
                        context += "Sample " + _sample.IdText + " with Analysis " + message;
                }
                if (!string.IsNullOrEmpty(context))
                    if (!Library.Utils.FlashMessageYesNo(context + " has a missing result. Are you sure you want to authorize?", ""))
                        return;

            }
            else if (sample != null)
            {
                context = "Sample " + sample.IdText + " with Analysis ";
                var _tests = sample.Tests.ActiveItems.Cast<Test>().ToList();
                foreach (Test _test in _tests)
                {
                    if (!CheckResultComponents(_test))
                        message += _test.Analysis.Name + " ";
                }
                if (!string.IsNullOrEmpty(message))
                    if (!Library.Utils.FlashMessageYesNo(context + message + " has a missing result. Are you sure you want to authorize?", ""))
                        return;
            }
           
            Exit(false);
        }
        private bool CheckResultComponents(Test test)
        {
            if (!test.Analysis.Components.ActiveItems.Cast<VersionedComponent>().Any(x => test.Results.ActiveItems.Cast<Result>().Select(b => b.Name).Contains(x.Name)))
                //if (Library.Utils.FlashMessageYesNo(context + " has a missing result for the following test " + test.Analysis.Name + ".   Are you sure you want to authorize?", ""))
                return true;
            else
                return false;

        }
    }
}
