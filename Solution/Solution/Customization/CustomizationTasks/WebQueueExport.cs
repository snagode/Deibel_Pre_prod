using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.ObjectModel;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(WebQueueExport), "WorkflowCallback")]
    public class WebQueueExport : SampleManagerTask
    {
        protected override void SetupTask()
        {
            if (Context.SelectedItems.ActiveCount == 0)
            { Exit(false); return; }

            var tests = new List<Test>();

            var job = Context.SelectedItems[0] as JobHeader;
            var sample = Context.SelectedItems[0] as Sample;
            var test = Context.SelectedItems[0] as Test;
            if (job != null)
                tests = job.Samples.Cast<Sample>().SelectMany(s => s.Tests.Cast<Test>()).ToList();
            else if (sample != null)
                tests = sample.Tests.Cast<Test>().ToList();
            else if (test != null)
                tests.Add(test as Test);
            else
            {
                Exit(true);
                return;
            }

            WriteResults(tests);
            Exit(true);
        }

        void WriteResults(List<Test> tests)
        {
            var results = tests.SelectMany(t => t.Results.Cast<Result>()).ToList();

            var file = Library.File.GetWriteFile("smp$dbl_web_export", Guid.NewGuid() + ".csv");
            File.AppendAllText(file.FullName, "SampleCode,TestCode,ComponentName,Result\r\n");
            foreach (var result in results)
            {
                var line = $"{result.TestNumber.Sample.ImSampleRefId},{result.TestNumber.Analysis.Identity},{result.ResultName},{result.Text}\r\n";
                File.AppendAllText(file.FullName, line);
            }
        }
    }
}
