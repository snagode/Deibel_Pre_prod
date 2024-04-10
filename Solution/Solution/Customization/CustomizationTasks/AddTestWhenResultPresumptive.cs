using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(AddTestWhenResultPresumptive), "WorkflowCallback")]
    class AddTestWhenResultPresumptive : SampleManagerTask
    {
        Workflow _jobWorkflow;
        protected override void SetupTask()
        {
            try
            {
                if (Context.SelectedItems.ActiveCount == 0)
                { Exit(false); return; }
                var result = Context.SelectedItems[0] as Result;
                if (result.RepControl.Contains("R") && result.Text.Contains("Pres") || result.Text.Contains("See"))
                {
                    var sampleId = result.TestNumber.Sample;
                    var compList = (CustomerComponentsEmpBase)getCompList(result);
                    var confTest = (CustomerComponentsEmpBase)getConfirmationTest(compList);
                    if (confTest != null)
                    {
                        string msg = "Do You want to add Confirmation \nTest=" + confTest.Analysis+"\t"+"in Sample="+sampleId.IdNumeric.ToString().Trim()+ " ?";
                        if (Library.Utils.FlashMessageYesNo(msg, ""))
                        { 
                            var query = EntityManager.CreateQuery(TableNames.VersionedAnalysis);
                            query.AddEquals(VersionedAnalysisPropertyNames.Identity, confTest.Analysis);
                            var vAnalysis = EntityManager.Select(query).ActiveItems.Cast<VersionedAnalysisBase>().ToList().FirstOrDefault();
                            var samples = EntityManager.CreateEntityCollection(Sample.EntityName);
                            Sample sample;
                            sample = sampleId as Sample;
                            var analysis = confTest.Analysis;
                            var q = EntityManager.CreateQuery(TableNames.VersionedAnalysis);
                            q.AddEquals(VersionedAnalysisPropertyNames.VersionedAnalysisName, analysis);
                            var col = EntityManager.Select(q).ActiveItems[0];
                            var ana = col as VersionedAnalysisInternal;
                            var test5 = sample.AddTest(ana);
                            samples.Add(sample);
                            EntityManager.Transaction.Add(sample);
                            

                            //var e1 = (Test)EntityManager.CreateEntity(TableNames.Test);
                            //e1.Analysis = vAnalysis;
                            //e1.TestNumber = 33563238;

                            //e1.Sample = sampleId;
                            //// e1.Status.PhraseId = "V";
                            //e1.SetStatus(PhraseTestStat.PhraseIdV);

                            //EntityManager.Transaction.Add(e1);
                            //EntityManager.Commit();
                            //Library.Utils.FlashMessage("Test Added..!", "Info..!");

                        }
                    }
                }
                Exit(true);
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message + ex.StackTrace);
            }
        }

        //private Sample getSampleDetails(SampleBase sample)
        //{
        //    return null;
        //}

        private object getConfirmationTest(CustomerComponentsEmpBase compList)
        {
            var query = EntityManager.CreateQuery(TableNames.CustomerComponentsEmp);
            query.AddEquals(CustomerComponentsEmpPropertyNames.AnalysisAlias, compList.AnalysisAlias);
            query.AddEquals(CustomerComponentsEmpPropertyNames.ComponentAlias, compList.ComponentAlias);
            query.AddEquals(CustomerComponentsEmpPropertyNames.AnalysisOrder, compList.AnalysisOrder + 1);

            var confTest = EntityManager.Select(query).ActiveItems.Cast<CustomerComponentsEmpBase>().ToList().FirstOrDefault();
            return confTest;
        }

        private object getCompList(Result result)
        {
            var query = EntityManager.CreateQuery(TableNames.CustomerComponentsEmp);
            query.AddEquals(CustomerComponentsEmpPropertyNames.Analysis, result.TestNumber.Analysis.Identity);
            //var  results = (CustomerComponentsEmpBase)EntityManager.Select(query).ActiveItems[0];
            var compList = EntityManager.Select(query).ActiveItems.Cast<CustomerComponentsEmpBase>().ToList().LastOrDefault();

            return compList;
        }
        private void WriteLog(String ex)
        {
            string logFilePath = Library.Environment.GetFolderList("smp$dbl_DevLogFilesPath") + "\\AddPresumptiveTest";
            Common.WriteLog(logFilePath, $"{DateTime.Now.ToShortTimeString()}:" + ex + "\r\n");
        } 
    }
}
