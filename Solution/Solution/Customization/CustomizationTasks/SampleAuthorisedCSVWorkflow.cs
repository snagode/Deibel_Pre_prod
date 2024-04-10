using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;
using System.Collections;
using System.Collections.Generic;
namespace Customization.Tasks
{
    [SampleManagerTask(nameof(SampleAuthorisedCSVWorkflow), "WorkflowCallback")]
    public class SampleAuthorisedCSVWorkflow : SampleManagerTask
    {

        protected override void SetupTask()
        {
            try
            {
                if (Context.SelectedItems.ActiveCount == 0)
                { Exit(false); return; }
                var list = new List<Test>();
                WriteLog("Start the Execution");
                var sample = Context.SelectedItems[0] as Sample;

                if (sample != null && sample.JobName.EmpJobOrderId.StartsWith("Job"))//need to pass job//
                    list = sample.Tests.Cast<Test>().ToList();
                else
                {
                    WriteLog("EmpJobOrderId does not start with Job.");
                    Exit(true);
                    return;
                }


                List<Result> resultsAllTest = list.SelectMany(t => t.Results.Cast<Result>()).ToList();
                if (resultsAllTest != null)
                { 
                    WriteResults(list, sample, resultsAllTest);
                }
                else
                {
                    WriteLog("Test Result not found");
                }
                Exit(true);
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// In the following method, Once the sample status is Authorized, we build one CSV file and export it to the local path.
        /// Local Path : C:\Thermo\SampleManager\Server\SMUAT\Web_Queue\Export.This path is defined in the regedit key AS smp$dbl_ExportFilePath.
        /// <summary>
        void WriteResults(List<Test> tests, Sample sample, List<Result> resultsAllTest)
        {
            try
            {
                
                //Select the all Test Components from Customer_Component_Emp Table
                var q = EntityManager.CreateQuery(CustomerComponentsEmpBase.EntityName);
                q.AddIn(CustomerComponentsEmpPropertyNames.Analysis, tests.Select(x => (object)x.Analysis.Identity).ToList());
                var compList = EntityManager.Select(q).ActiveItems.Cast<CustomerComponentsEmpBase>().ToList();

                if (compList.Count > 0)
                {
                    var joinedList = (from test in tests
                                      join cmp in compList on test.Analysis.Identity equals cmp.Analysis
                                      select new { test.Sample.CustomerId, test.Sample.EmpSampleOrderId, test.EmpTestOrderId, test.Analysis, cmp.ComponentName, cmp.AnalysisAlias, cmp.ComponentAlias, cmp.AnalysisOrder }).ToList();

                    var versionCompList = tests.SelectMany(x => x.Analysis.Components.ActiveItems.Cast<VersionedComponentBase>().Where(a => a.RepControl.Contains("R"))).Select(n => new { n.Analysis, n.Name, n.RepControl }).ToList();
                    if (versionCompList != null)
                    {

                        if (joinedList != null)
                        {

                            //below query for Component it means Analysis order Id=1
                            var aliasMatchList_Component = joinedList.GroupBy(x => new { x.AnalysisAlias, x.ComponentAlias }).Where(grp => grp.Count() > 1).SelectMany(grp => grp.Select(r => r)).OrderByDescending(c => c.AnalysisOrder).GroupBy(p => p.AnalysisAlias).ToList()
                              .Select(g => g.OrderByDescending(p => p.AnalysisOrder).LastOrDefault()).ToList();

                            //below query for Component it means Analysis order Id=Max(2,3)
                            var aliasMatchList_ForResurlt = joinedList.GroupBy(x => new { x.AnalysisAlias, x.ComponentAlias }).Where(grp => grp.Count() > 1).SelectMany(grp => grp.Select(r => r)).OrderByDescending(c => c.AnalysisOrder).GroupBy(p => p.AnalysisAlias).ToList()
                             .Select(g => g.OrderByDescending(p => p.AnalysisOrder).FirstOrDefault());

                            var getResult_aliasMatch = (from t in aliasMatchList_ForResurlt
                                                        join result in resultsAllTest on t.Analysis.Identity equals result.TestNumber.Analysis.Identity
                                                        join c in versionCompList on result.Name equals c.Name
                                                        where c.RepControl.Contains("R")
                                                        select new
                                                        { t.CustomerId, t.EmpSampleOrderId, t.EmpTestOrderId, t.Analysis.Identity, result.Text, t.ComponentName, t.AnalysisAlias, t.ComponentAlias, t.AnalysisOrder }).ToList();

                            var matchfinalResult = (from matchComp in aliasMatchList_Component
                                                    join matchresult in getResult_aliasMatch
                             on matchComp.AnalysisAlias equals matchresult.AnalysisAlias
                                                    select new { matchComp.Analysis.Identity, matchComp.EmpSampleOrderId, matchComp.EmpTestOrderId, matchComp.ComponentName, matchresult.Text }).ToList();
                            //Without Match Alias

                            var aliasWithoutMatch = joinedList.GroupBy(x => new { x.AnalysisAlias, x.ComponentAlias }).Where(grp => grp.Count() == 1).SelectMany(grp => grp.Select(r => r)).ToList();

                            var getResult_aliasWithoutMatch = (from t in aliasWithoutMatch
                                                               join result in resultsAllTest on t.Analysis.Identity equals result.TestNumber.Analysis.Identity
                                                               join c in versionCompList on result.Name equals c.Name
                                                               where c.RepControl.Contains("R")
                                                               select new
                                                               { t.Analysis.Identity, t.EmpSampleOrderId, t.EmpTestOrderId, t.ComponentName, result.Text }).Where(x => x.EmpTestOrderId != "").ToList();



                            matchfinalResult.AddRange(getResult_aliasWithoutMatch);
                            if (matchfinalResult != null)
                            {
                                ExportResult(matchfinalResult);
                            }
                            else
                            {
                                WriteLog("Record Not found");
                            }
                        }
                    }
                    else
                    {
                        WriteLog("Record Not found in Versioned Component");
                    }
                }
                else
                {
                    WriteLog("Record Not found in CustomerComponentEmp Table");
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message + ex.StackTrace);
            }
        }

        private void ExportResult<T>(List<T> getResult_aliasMatch)
        {
            try
            {
                var file = Library.File.GetWriteFile("smp$dbl_ExportFilePath", "SmpAuth_" + DateTime.Now.ToString("ddMMyyyyhhmmsstt") + ".csv");

                if (getResult_aliasMatch != null)
                {
                    File.AppendAllText(file.FullName, "Sample Code,Test Order,Component Name,Result Text\r\n");
                    foreach (var result in getResult_aliasMatch)
                    {
                        var r = (dynamic)result;

                        string TestResult = $"{ r.Text }";
                        string str = !string.IsNullOrEmpty(TestResult) && TestResult.Equals("Positive") ? "1" :
                                     !string.IsNullOrEmpty(TestResult) && TestResult.Equals("Negative") ? "0" :
                                     TestResult;

                        var line = $"{r.EmpSampleOrderId},{r.EmpTestOrderId},{r.ComponentName},{str}\r\n";
                        File.AppendAllText(file.FullName, line);
                    }

                    WriteLog("All Records are successfully imported into CSV file");
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message + ex.StackTrace);
            }
        }
        private void WriteLog(String ex)
        {
            string logFilePath = Library.Environment.GetFolderList("smp$dbl_DevLogFilesPath") + "\\SampleAuthorized_Generate_CSV";
            Common.WriteLog(logFilePath, $"{DateTime.Now.ToShortTimeString()}:" + ex + "\r\n");
        }
        //private object test(List<Test> list)
        //{
        //    var q = EntityManager.CreateQuery(CustomerComponentsEmpBase.EntityName);
        //    var compList = EntityManager.Select(q).ActiveItems.Cast<CustomerComponentsEmpBase>();
        //    var testComp = (from test in list join cmp in compList on test.Analysis.Identity equals cmp.Analysis select cmp);
        //    // select cmp { test.Sample.CustomerId, test.Sample.SampleOrderId, test.TestOrderId, test.Analysis, cmp.ComponentName, cmp.AnalysisAlias, cmp.ComponentAlias, cmp.AnalysisOrder }).ToList();
        //    return testComp;
        //}
    }

}



//var aliasMatchList = joinedList.GroupBy(x => new { x.AnalysisAlias, x.ComponentAlias }).Where(grp => grp.Count() > 1).SelectMany(grp => grp.Select(r => r)).OrderByDescending(c => c.AnalysisOrder).GroupBy(p => p.AnalysisAlias).ToList()
// .Select(g => g.OrderByDescending(p => p.AnalysisOrder)).ToList();

//var getResult_aliasMatch = (from t in aliasMatchList.FirstOrDefault()
//                            join result in resultsAllTest on t.Analysis.Identity equals result.TestNumber.Analysis.Identity
//                            join c in versionCompList on result.Name equals c.Name
//                            where c.RepControl.Contains("R")
//                            select new
//                            { t.CustomerId, t.EmpSampleOrderId, t.EmpTestOrderId, t.Analysis.Identity, result.Text, t.ComponentName, t.AnalysisAlias, t.ComponentAlias, t.AnalysisOrder }).ToList().FirstOrDefault();

//var getResult_GetComponent = (from t in aliasMatchList.LastOrDefault()
//                            join result in resultsAllTest on t.Analysis.Identity equals result.TestNumber.Analysis.Identity
//                            join c in versionCompList on result.Name equals c.Name
//                            where c.RepControl.Contains("R")
//                            select new
//                            { t.CustomerId, t.EmpSampleOrderId, t.EmpTestOrderId, t.Analysis.Identity,  result.Text, t.ComponentName, t.AnalysisAlias, t.ComponentAlias, t.AnalysisOrder }).ToList().LastOrDefault();

//var combinedResult = new
//{
//    getResult_aliasMatch.CustomerId,
//    getResult_aliasMatch.EmpSampleOrderId,
//    getResult_aliasMatch.EmpTestOrderId,
//    getResult_aliasMatch.Identity,
//    getResult_aliasMatch.Text,
//    getResult_GetComponent.ComponentName,

//    getResult_GetComponent.ComponentAlias,
//    getResult_GetComponent.AnalysisAlias,
//    getResult_GetComponent.AnalysisOrder

//};




//private void CustomerComponentsEmpInsert<T>(List<T> getResult_aliasMatch)
//{
//    try
//    {
//        if (getResult_aliasMatch != null)
//        {
//            // int i=0;
//            foreach (var result in getResult_aliasMatch)
//            {
//                var r = (dynamic)result;
//                var e = EntityManager.CreateEntity(CustomerComponentsEmpBase.EntityName) as CustomerComponentsEmpBase;
//                e.CustomerId = r.CustomerId.ToString();
//                //e.Guid = null;
//                e.ComponentName = r.ResultName;
//                e.Analysis = r.Identity;
//                e.CodeGroup = null;
//                e.AnalysisAlias = r.AnalysisAlias;
//                e.ComponentAlias = r.ComponentAlias;
//                e.AnalysisOrder = r.AnalysisOrder;
//                e.ComponentList = null;
//                e.DeibelLab = null;
//                EntityManager.Transaction.Add(e);
//                // i += 1;
//            }
//            // EntityManager.Commit();
//        }
//        else
//        {
//            string logFilePath = Library.Environment.GetFolderList("smp$dbl_Web_Queue_Export_LogFilePath") + "ErrorLog\\".ToString();
//            Common.WriteLog(logFilePath, $"{DateTime.Now.ToShortTimeString()}:Result Not Found\r\n");
//        }
//    }
//    catch (Exception ex)
//    {
//        ex.Message.ToString();
//    }
//}


//if (aliasWithoutMatch!=null)
//{

//    foreach (var result in getResult_aliasWithoutMatch)
//    {
//        var line1 = $"{result.CustomerId},{result.SampleOrderId},{result.Identity},{result.ResultName},{result.Text}\r\n";
//        File.AppendAllText(file.FullName, line1);
//    }
//}

//var firstEachAnalysis = aliasMatch.GroupBy(p => p.AnalysisAlias)
//   .Select(g => g.OrderByDescending(p => p.AnalysisOrder)
//                 .FirstOrDefault());

//var getResult_aliasMatch = (from am in aliasMatch
//                            join result in resultsAllTest on am.Analysis.Identity equals result.TestNumber.Analysis.Identity

//                            select new
//                            { result.Name, result.ResultName, result.Text, am.AnalysisOrder }).FirstOrDefault();



//var getResult_aliasWithoutMatch = (from am in aliasWithoutMatch
//                                   join result in resultsAllTest on am.Analysis.Identity equals result.TestNumber.Analysis.Identity

//                                   select new
//                                   { am.CustomerId, am.SampleOrderId, am.Analysis.Identity, result, result.Name, result.ResultName, result.Text, am.AnalysisOrder }).ToList();

//For test

//foreach (var comp in joinedList)
//{

//    var confMap = sample.CustomerId.CustomerComponents
//    .Cast<CustomerComponentsBase>()
//    .Where(x => x.AnalysisAlias == comp.AnalysisAlias &&  x.ComponentAlias == comp.ComponentAlias).OrderByDescending(c => c.AnalysisOrder).ToList();

//    var resultFinel = (from test in tests
//                     join s in confMap on test.Analysis.Identity equals s.Analysis
//                     join r in results on s.Analysis equals r.TestNumber.Analysis.Identity
//                    // where s.AnalysisOrder == confMap.Max(x=>x.AnalysisOrder)
//                     select new {test.Sample.CustomerId,test.Sample.SampleOrderId,test.Analysis.Identity, r.ResultName, r.Text }).ToList();

//    //foreach (var mapItem in confMap)
//    //{
//    //    var results1 = tests.Where(t => t.Analysis.Identity == mapItem.Analysis).SelectMany(t => t.Results.Cast<Result>()).FirstOrDefault();
//    //}
//}

// var results = tests.SelectMany(t => t.Results.Cast<Result>()).ToList();