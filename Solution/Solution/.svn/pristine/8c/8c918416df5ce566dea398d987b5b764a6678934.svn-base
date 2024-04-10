using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Web;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;
using Thermo.SampleManager.WebApiTasks.Mobile.Data;
using Thermo.SM.LIMSML.Helper.Low;
using Entity = Thermo.SM.LIMSML.Helper.Low.Entity;
using Object = Thermo.SampleManager.WebApiTasks.Data.Object;

namespace Thermo.SampleManager.WebApiTasks.Mobile
{
	/// <summary>
	///  Results
	/// </summary>
	[SampleManagerWebApi("mobile.results")]
	[MobileFeature(FunctionResultEntry.FeatureName)]
	public class ResultEntry : WebApiLimsmlBaseTask
	{
		#region Constants

		private const int LockRetryCount = 5;
		private const int LockRetryDelay = 2; // second.

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the Explorer cache.
		/// </summary>
		/// <value>
		/// The cache.
		/// </value>
		protected IExplorerCacheService ExplorerCache { get; set; }

		#endregion

		#region Overrides

		/// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			ExplorerCache = Library.GetService<IExplorerCacheService>();
			base.SetupTask();
		}

		#endregion

		#region Result Information

		/// <summary>
		/// Result Information - for Selected Samples
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/results/samples?page={page}&pageSize={size}", Method = "POST")]
		[Description("Result Information for the posted samples with optional paging (15638)")]
		[MenuSecurity(FunctionResultEntry.FunctionResultEntryNumber)]
		public ResultData SelectedResultInformation(int page, int size, SelectedValues selectedValues)
		{
			if (selectedValues == null) return null;

			// Return appropriate sizes

			if (page == 0) page = 1;
			if (size == 0) size = 100;

			ResultData data = new ResultData(initialise: true);
			data.Page = page;
			data.PageSize = size;
			data.Columns = FunctionResultEntry.LoadResultEntryColumns(Library);

			foreach (var sampleId in selectedValues.Selected)
			{
				var sample = (Sample)EntityManager.Select(SampleBase.EntityName, new Identity(sampleId));
				if (!BaseEntity.IsValid(sample)) continue;
				data.LoadSample(sample);
			}

			data.SetUri(string.Format("/mobile/results/samples"), data.FinishedPage);

			if (data.Rows == null || data.Rows.Count == 0) return null;
			data.Columns = null;

			return data;
		}

		/// <summary>
		/// Result Information - for a Sample
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/results/samples/{sampleId}?page={page}&pageSize={size}", Method = "GET")]
		[Description("Result Information for the specified sample with optional paging (15638)")]
		[MenuSecurity(FunctionResultEntry.FunctionResultEntryNumber)]
		public ResultData SampleResultInformation(string sampleId, int page, int size)
		{
			// Return appropriate sizes

			if (page == 0) page = 1;
			if (size == 0) size = 100;

			var sample = (Sample)EntityManager.Select(SampleBase.EntityName, new Identity(sampleId));
			if (!BaseEntity.IsValid(sample)) return null;

			ResultData data = new ResultData(initialise: true);
			data.Page = page;
			data.PageSize = size;
			data.Columns = FunctionResultEntry.LoadResultEntryColumns(Library);
			data.LoadSample(sample);

			data.SetUri(string.Format("/mobile/results/samples/{0}", sampleId), data.FinishedPage);

			if (data.Rows == null || data.Rows.Count == 0) return null;
			data.Columns = null;

			return data;
		}

		/// <summary>
		/// Job result information.
		/// </summary>
		/// <param name="jobName">Name of the job.</param>
		/// <param name="page">The page.</param>
		/// <param name="size">The size.</param>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/results/jobs/{jobName}?page={page}&pageSize={size}", Method = "GET")]
		[Description("Result Information for the specified job with optional paging (15638)")]
		[MenuSecurity(FunctionResultEntry.FunctionResultEntryNumber)]
		public ResultData JobResultInformation(string jobName, int page, int size)
		{
			// Return appropriate sizes

			if (page == 0) page = 1;
			if (size == 0) size = 100;

			// Return back the data along with the parameters

			var data = new ResultData(initialise: true);
			data.Columns = FunctionResultEntry.LoadResultEntryColumns(Library);
			data.Page = page;
			data.PageSize = size;

			var job = (JobHeader)EntityManager.Select(JobHeaderBase.EntityName, new Identity(jobName));
			if (!BaseEntity.IsValid(job)) return null;

			foreach (Sample sample in job.Samples)
			{
				data.LoadSample(sample);
			}

			data.SetUri(string.Format("/mobile/results/jobs/{0}", jobName), data.FinishedPage);

			if (data.Rows == null || data.Rows.Count == 0) return null;
			data.Columns = null;

			return data;
		}

		/// <summary>
		/// Test Result Information
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/results/tests/{testNumber}?page={page}&pageSize={size}", Method = "GET")]
		[Description("Result Information for the specified test with optional paging (15638)")]
		[MenuSecurity(FunctionResultEntry.FunctionResultEntryNumber)]
		public ResultData TestResultInformation(string testNumber, int page, int size)
		{
			if (InvalidTestNumber(testNumber)) return null;

			var test = (Test)EntityManager.Select(TestBase.EntityName, new Identity(testNumber));
			if (!BaseEntity.IsValid(test)) return null;

			// Return appropriate sizes

			if (page == 0) page = 1;
			if (size == 0) size = 100;

			ResultData data = new ResultData(initialise: true);
			data.Columns = FunctionResultEntry.LoadResultEntryColumns(Library);
			data.LoadTest(test);
			data.Page = page;
			data.PageSize = size;

			data.SetUri(string.Format("/mobile/results/tests/{0}", testNumber), data.FinishedPage);

			if (data.Rows == null || data.Rows.Count == 0) return null;
			data.Columns = null;

			return data;
		}

		/// <summary>
		/// Result Information - for a Result
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/results/{testNumber}/{resultName}", Method = "GET")]
		[Description("Result Information for the specified result (15638)")]
		[MenuSecurity(FunctionResultEntry.FunctionResultEntryNumber)]
		public ResultDataRow ResultInformation(string testNumber, string resultName)
		{
			if (InvalidTestNumber(testNumber)) return null;

			var test = (Test)EntityManager.Select(TestBase.EntityName, new Identity(testNumber));
			if (!BaseEntity.IsValid(test)) return null;

			// Load

			ResultData data = new ResultData(initialise: true);
			data.Columns = FunctionResultEntry.LoadResultEntryColumns(Library);
			data.LoadTest(test);

			if (data.Rows == null || data.Rows.Count == 0) return null;
			foreach (var row in data.Rows.Cast<ResultDataRow>())
			{
				if (row.Name == null) continue;
				if (row.Name.Equals(resultName)) return row;
			}

			return null;
		}

		/// <summary>
		/// Result Information - for Selected Tests
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/results/tests?page={page}&pageSize={size}", Method = "POST")]
		[Description("Result Information for the posted tests with optional paging (15646)")]
		[MenuSecurity(FunctionResultEntryByTest.FunctionResultEntryByTestNumber)]
		public ResultData SelectedTestResultInformation(int page, int size, SelectedValues selectedValues)
		{
			if (selectedValues == null) return null;

			// Return appropriate sizes

			if (page == 0) page = 1;
			if (size == 0) size = 100;

			ResultData data = new ResultData(initialise: true);
			data.Page = page;
			data.PageSize = size;
			data.Columns = FunctionResultEntryByTest.LoadResultEntryColumns(Library);

			foreach (var testId in selectedValues.Selected)
			{
				var test = (Test)EntityManager.Select(TestBase.EntityName, new Identity(testId));
				if (!BaseEntity.IsValid(test)) continue;
				data.LoadTest(test);
			}

			data.SetUri(string.Format("/mobile/results/tests"), data.FinishedPage);

			if (data.Rows == null || data.Rows.Count == 0) return null;
			data.Columns = null;

			return data;
		}

		#endregion

		#region Result Entry

		/// <summary>
		/// Result Entry
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/results/samples", Method = "PUT")]
		[Description("Multiple Result Entry using the put result values (15638)")]
		[MenuSecurity(FunctionResultEntry.FunctionResultEntryNumber, checkSignature: true)]
		public ResultData ResultsPut(List<ResultValue> resultValues)
		{
			return ResultsPut(resultValues, FunctionResultEntry.LoadResultEntryColumns(Library));
		}

		/// <summary>
		/// Result Entry - Same as Results/PUT but with different menu security.
		/// for Result Entry by test save. Makes sure we get the right esigs.
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/results/tests", Method = "PUT")]
		[Description("Multiple Result Entry using the put result values (15646)")]
		[MenuSecurity(FunctionResultEntryByTest.FunctionResultEntryByTestNumber, checkSignature: true)]
		public ResultData ResultsTestsPut(List<ResultValue> resultValues)
		{
			return ResultsPut(resultValues, FunctionResultEntryByTest.LoadResultEntryColumns(Library));
		}

		/// <summary>
		/// Results Entry
		/// </summary>
		/// <param name="resultValues">The result values.</param>
		/// <param name="resultColumns">The result columns.</param>
		/// <returns></returns>
		private ResultData ResultsPut(List<ResultValue> resultValues, List<Column> resultColumns)
		{
			var limsml = LimsmlGet();
			var entity = GetResultEntryLimsml(limsml);

			int testNumber = 0;
			Entity test = null;

			foreach (var resultValue in resultValues)
			{
				if (resultValue.TestNumber == 0 || resultValue.Name == null) return null;
				if (resultValue.TestNumber != testNumber || test == null)
				{
					testNumber = resultValue.TestNumber;
					test = entity.AddChild(TestBase.EntityName);
					test.DirSetField("TEST_NUMBER", new PackedDecimal(testNumber));
				}

				var result = test.AddChild(ResultBase.EntityName);
				result.DirSetField("NAME", resultValue.Name);

				// Get the correctly formatted result values.

				string resultText = GetFormattedResult(resultValue.TestNumber, resultValue.Name, resultValue.Text);
				result.DirSetField("TEXT", resultText);

				// Pass through any additional properties to the VGL

				foreach (var item in resultValue.AdditionalData)
				{
					result.DirSetField(item.Key, item.Value.ToString());
				}
			}

			// LIMSML Result Entry

			var response = LimsmlProcess(limsml, LockRetryCount, LockRetryDelay);

			ResultData data = new ResultData(initialise: true);
			data.Columns = resultColumns;

			foreach (var resultValue in resultValues)
			{
				var resultId = new Identity(resultValue.TestNumber, resultValue.Name);
				var result = (Result) EntityManager.Select(ResultBase.EntityName, resultId);

				if (result == null)
				{
					var testEntity = (Test) EntityManager.Select(TestBase.EntityName, new Identity(resultValue.TestNumber));
					if (testEntity == null) continue;

					data.LoadComponent(testEntity, resultValue.Name);
				}
				else
				{
					data.LoadResult(result);
				}
			}

			data.Columns = null;

			// Check for errors and return appropriate data

			if (LimsmlCheckOk(response, HttpStatusCode.Conflict))
			{
				UpdateResultErrorStatus(response, data);
				return data;
			}

			return null;
		}

		/// <summary>
		/// Result Entry
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/results/{testNumber}/{resultName}", Method = "PUT")]
		[Description("Result Entry for the specified test/result name (15646)")]
		[MenuSecurity(FunctionResultEntryByTest.FunctionResultEntryByTestNumber, checkSignature: true)]
		public ResultDataRow TestResultPut(string testNumber, string resultName, string resultText)
		{
			if (InvalidTestNumber(testNumber)) return null;

			var limsml = LimsmlGet();
			var entity = GetResultEntryLimsml(limsml);

			var testId = new PackedDecimal(testNumber);

			var test = entity.AddChild(TestBase.EntityName);
			test.DirSetField("TEST_NUMBER", testId);
			
			var result = test.AddChild(ResultBase.EntityName);
			result.DirSetField("NAME", resultName);
			result.DirSetField("TEXT", GetFormattedResult(testId.Value, resultName, resultText));

			// Result Entry via LIMSML

			var response = LimsmlProcess(limsml, LockRetryCount, LockRetryDelay);

			ResultData data = new ResultData(initialise: true);
			data.Columns = FunctionResultEntryByTest.LoadResultEntryColumns(Library);

			var resultId = new Identity(testNumber, resultName);
			var resultValue = (Result)EntityManager.Select(ResultBase.EntityName, resultId);

			// For cases where the result doesn't exist - use the component.

			if (resultValue == null)
			{
				var testData = (Test)EntityManager.Select(TestBase.EntityName, new Identity(testNumber));
				if (!BaseEntity.IsValid(testData)) return null;

				var component = (VersionedComponent)testData.Analysis.Components[resultName];
				if (!BaseEntity.IsValid(component)) return null;

				data.LoadComponent(testData, component);
			}
			else
			{
				data.LoadResult(resultValue);
			}

			data.Columns = null;

			// Check for errors and return an appropriate code.

			if (LimsmlCheckOk(response, HttpStatusCode.Conflict))
			{
				UpdateResultErrorStatus(response, data);
				if (data.Rows == null || data.Rows.Count == 0) return null;

				var dataResult = (ResultDataRow)data.Rows.First();
				if (!string.IsNullOrEmpty(dataResult.UpdateStatus))
				{
					SetHttpStatus(HttpStatusCode.Conflict, dataResult.UpdateStatus);
					return null;
				}

				return dataResult;
			}

			return null;
		}

		/// <summary>
		/// Gets the result entry limsml.
		/// </summary>
		/// <param name="limsml">The limsml.</param>
		/// <returns></returns>
		private Entity GetResultEntryLimsml(Limsml limsml)
		{
			var trans = LimsmlGetTransaction(limsml);

			var entity = trans.AddEntity(SampleBase.StructureTableName);
			var action = entity.AddAction("RESULT_ENTRY");

			action.AddParameter("FULL_RESPONSE", "TRUE");
			action.AddParameter("UPDATE_SAMPLE", "FALSE");
			action.AddParameter("UPDATE_TESTS", "FALSE");

			return entity;
		}

		/// <summary>
		/// Checks the LIMSML response for errors.
		/// </summary>
		/// <param name="response">The response.</param>
		/// <param name="resultData">The result data.</param>
		private void UpdateResultErrorStatus(Limsml response, ResultData resultData)
		{
			if (response == null || response.Transactions == null || response.Transactions.Count == 0) return;
			var trans = response.GetTransaction(0);
			var sample = trans.GetEntity(0);
			if (sample == null) return;

			foreach (var test in sample.Children)
			{
				var testField = test.GetFieldById("TEST_NUMBER");
				if (testField == null) continue;

				foreach (var result in test.Children)
				{
					var nameField = result.GetFieldById("NAME");
					var errorField = result.GetFieldById("_ERROR");
					if (nameField == null || errorField == null) continue;
					UpdateResultErrorStatus(resultData, testField.Value, nameField.Value, errorField.Value);
				}
			}
		}

		/// <summary>
		/// Updates the result error status.
		/// </summary>
		/// <param name="resultData">The result data.</param>
		/// <param name="testNumber">The test number.</param>
		/// <param name="name">The name.</param>
		/// <param name="error">The error.</param>
		private void UpdateResultErrorStatus(ResultData resultData, string testNumber, string name, string error)
		{
			foreach (ResultDataRow row in resultData.Rows.Cast<ResultDataRow>())
			{
				if (row.TestNumber != int.Parse(testNumber)) continue;
				if (!row.Name.Equals(name)) continue;
				row.UpdateStatus = error;
			}
		}

		/// <summary>
		/// Detects an invalid test number.
		/// </summary>
		/// <param name="testNumber">The test number.</param>
		/// <returns></returns>
		private bool InvalidTestNumber(string testNumber)
		{
			int test;
			if (int.TryParse(testNumber, out test)) return false;
			SetHttpStatus(HttpStatusCode.BadRequest, string.Format("'{0}' is not a valid test number", testNumber));
			return true;
		}

		#endregion

		#region File Type results

		/// <summary>
		/// Result File
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/results/{testNumber}/{resultName}/file", Method = "GET")]
		[Description("Result File for the specified test/result name (15638)")]
		[MenuSecurity(FunctionResultEntry.FunctionResultEntryNumber)]
		public Stream TestResultFileGet(string testNumber, string resultName)
		{
			if (InvalidTestNumber(testNumber)) return null;

			var id = new Identity(testNumber, resultName);
			var result = (Result)EntityManager.Select(ResultBase.EntityName, id);
			if (!BaseEntity.IsValid(result)) return null;
			if (!result.ResultType.Equals(PhraseResType.PhraseIdF)) return null;
			if (string.IsNullOrEmpty(result.Text)) return null;

			string fileName = result.Text;
			if (!File.Exists(fileName)) return null;

			string shortName = Path.GetFileName(fileName);
			string mimeType = GetMimeType(shortName);
			SetContentDispositionFile(shortName, mimeType);
			return File.OpenRead(fileName);
		}

		/// <summary>
		/// Result File Put - Using Content-Disposition
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/results/{testNumber}/{resultName}/file", Method = "PUT")]
		[Description("Save Result File for the specified test/result name (15638)")]
		[MenuSecurity(FunctionResultEntry.FunctionResultEntryNumber, checkSignature: true)]
		public ResultDataRow TestResultFilePutSimple(string testNumber, string resultName, Stream fileStream)
		{
			string fileName = GetContentDispositionFile();

			if (string.IsNullOrEmpty(fileName))
			{
				SetHttpStatus(HttpStatusCode.BadRequest, "Specify Content-Disposition attachment with file extension");
				return null;
			}

			return TestResultFilePut(testNumber, resultName, fileName, fileStream);
		}

		/// <summary>
		/// Result File Put
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/results/{testNumber}/{resultName}/file/{fileName}", Method = "PUT")]
		[Description("Save Result File for the specified test/result name (15638)")]
		[MenuSecurity(FunctionResultEntry.FunctionResultEntryNumber, checkSignature: true)]
		public ResultDataRow TestResultFilePut(string testNumber, string resultName, string fileName, Stream fileStream)
		{
			if (InvalidTestNumber(testNumber)) return null;

			// Save the file to disk - same logic is in $LIB_RE_FILE_BROWSE

			var ext = Path.GetExtension(fileName);
			fileName = string.Format("{0:D10}-{1}{2}", int.Parse(testNumber), Guid.NewGuid(), ext);
			var file = Library.File.GetWriteFile(EnvironmentLibrary.SMP.TextFiles, fileName);

			// The file should never exist - if it does - bounce the request.

			if (file.Exists)
			{
				SetHttpStatus(HttpStatusCode.Conflict, fileName);
				return null;
			}

			// Stream the file from the request to the file system

			try
			{
				FileTask.SaveFile(file, fileStream);
			}
			catch (UnauthorizedAccessException ex)
			{
				SetHttpStatus(HttpStatusCode.Forbidden, ex.Message);
				return null;
			}

			// Do put test results

			var data = TestResultPut(testNumber, resultName, file.FullName);

			// If something went wrong with the save, delete the newly created file.

			if (data == null)
			{
				File.Delete(file.FullName);
				return null;
			}

			return data;
		}

		#endregion

		#region Result Formatting

		/// <summary>
		/// Gets the formatted result.
		/// </summary>
		/// <param name="testNumber">The test number.</param>
		/// <param name="name">The name.</param>
		/// <param name="text">The text.</param>
		/// <returns></returns>
		private string GetFormattedResult(int testNumber, string name, string text)
		{
			// Using the ID should make it use the Entity Cache.

			var testId = new Identity(testNumber.ToString(CultureInfo.InvariantCulture));
			var test = (TestBase)EntityManager.Select(TestBase.EntityName, testId);
			if (test == null) return text;

			var componentId = new Identity(test.Analysis, test.AnalysisVersion, name);
			var component = (VersionedComponentBase)EntityManager.Select(VersionedComponentBase.EntityName, componentId);
			if (component == null) return text;

			// Intervals

			if (component.ResultType.IsPhrase(PhraseResType.PhraseIdI))
			{
				return Interval.ToVglString(text);
			}

			// Dates

			if (component.ResultType.IsPhrase(PhraseResType.PhraseIdD))
			{
				// Adjust time - store specifically in server time.

				return Object.GetServerDateText(Library, text);
			}

			// Everything Else

			return text;
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Locking error.
		/// </summary>
		/// <param name="response">The response.</param>
		/// <returns></returns>
		protected override bool LimsmlLockingError(Limsml response)
		{
			if (base.LimsmlLockingError(response)) return true;

			string locked = Library.VGL.GetMessage("RESULT_ENTRY_LOCKED");
			string sampleLocked = Library.VGL.GetMessage("RESULT_ENTRY_SAMPLELOCKED");
			string testLocked = Library.VGL.GetMessage("RESULT_ENTRY_TESTLOCKED");

			if (response == null || response.Transactions == null || response.Transactions.Count == 0) return false;
			var trans = response.GetTransaction(0);
			var sample = trans.GetEntity(0);
			if (sample == null) return false;

			foreach (var test in sample.Children)
			{
				var testField = test.GetFieldById("TEST_NUMBER");
				if (testField == null) continue;

				foreach (var result in test.Children)
				{
					var errorField = result.GetFieldById("_ERROR");
					if (errorField == null) continue;

					if (errorField.Value == locked) return true;
					if (errorField.Value == sampleLocked) return true;
					if (errorField.Value == testLocked) return true;
				}
			}

			return false;
		}

		#endregion

		#region Function Support

		/// <summary>
		/// Gets the result entry function.
		/// </summary>
		/// <param name="rmb">The RMB.</param>
		/// <returns></returns>
		[MobileFunction]
		public static Function GetResultEntryFunction(ExplorerRmb rmb)
		{
			if (!FunctionResultEntry.IsFunction(rmb)) return null;
			return new FunctionResultEntry(rmb);
		}

		/// <summary>
		/// Gets the result entry by test function.
		/// </summary>
		/// <param name="rmb">The RMB.</param>
		/// <returns></returns>
		[MobileFunction]
		public static Function GetResultEntryByTestFunction(ExplorerRmb rmb)
		{
			if (!FunctionResultEntryByTest.IsFunction(rmb)) return null;
			return new FunctionResultEntryByTest(rmb);
		}

		#endregion
	}
}
