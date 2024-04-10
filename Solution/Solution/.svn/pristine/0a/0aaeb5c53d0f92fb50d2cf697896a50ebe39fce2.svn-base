using System.Collections.Generic;
using System.Runtime.Serialization;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Result Data
	/// </summary>
	[DataContract(Name="resultData")]
	public class ResultData : DataCollection
	{
		#region Properties

		/// <summary>
		/// Gets or sets a value indicating whether this has finished loading the page.
		/// </summary>
		/// <value>
		///   <c>true</c> if finished loading; otherwise, <c>false</c>.
		/// </value>
		public bool FinishedPage { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to determine count.
		/// There is a processor/db cost to determining this.
		/// </summary>
		/// <value>
		///   <c>true</c> if determine count; otherwise, <c>false</c>.
		/// </value>
		public bool DetermineCount { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="SearchData"/> class.
		/// </summary>
		public ResultData()
		{
			DetermineCount = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SearchData"/> class.
		/// </summary>
		/// <param name="initialise">if set to <c>true</c> initialise.</param>
		public ResultData(bool initialise = false) : base (initialise)
		{
			DetermineCount = true;
		}

		#endregion

		#region Column Definition

		/// <summary>
		/// Loads the columns.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <param name="resultMode">The result mode.</param>
		/// <param name="columnMode">The column mode.</param>
		/// <returns></returns>
		public static List<Column> LoadResultEntryColumns(StandardLibrary library, string resultMode, string columnMode)
		{
			var logger = Logger.GetInstance(typeof(ResultData));
			var columns = new List<Column>();

			logger.DebugFormat("Loading Result Entry Columns {0}/{1}", resultMode, columnMode);

			var cols = (DataVariable)library.VGL.RunVGLRoutine("$LIB_RE_COLUMNS", "lib_re_columns_get_mobile", resultMode, columnMode);

			int count = 1;

			while (cols.ReadArrayValue(new[] {count, 1}) != null)
			{
				var table = (string)cols.ReadArrayValue(new[] { count, 1 });
				var field = (string)cols.ReadArrayValue(new[] { count, 2 });
				var title = (string)cols.ReadArrayValue(new[] { count, 3 });
				var size = cols.ReadArrayValue(new[] { count, 4 });

				var width = int.Parse(size.ToString().Trim());

				logger.DebugFormat("{0} Processing {1}.{2} = {3} size {4}", count, table, field, title, width);

				var property = EntityType.DeducePropertyName(table, field);

				if (property == ResultPropertyNames.ResultName) property = "Name";

				var column = Column.LoadColumn(library, table, property);

				column.Width = width;
				column.Name = GetLocalizedString(library, title);
				columns.Add(column);

				logger.DebugFormat("After tagging {0} = {1}", title, column.Name);

				count++;
			}

			logger.Debug("Result Entry Columns Loaded");

			return columns;
		}

		#endregion

		#region Data

		/// <summary>
		/// Loads the data for the specified sample
		/// </summary>
		/// <param name="sample">The sample.</param>
		public void LoadSample(Sample sample)
		{
			if (!BaseEntity.IsValid(sample)) return;

			foreach (Test test in sample.Tests)
			{
				LoadTest(test);
			}
		}

		/// <summary>
		/// Loads the test.
		/// </summary>
		/// <param name="test">The test.</param>
		public void LoadTest(Test test)
		{
			if (!BaseEntity.IsValid(test)) return;

			// Applies if we have component lists of have modified the result list

			if (test.HasResultList)
			{
				foreach (Result result in test.Results)
				{
					LoadResult(result);
					if (FinishedPage) return;
				}

				return;
			}

			// Load in Results where available, components otherwise.

			IList<ResultBase> foundResults = new List<ResultBase>();

			foreach (VersionedComponent component in test.Analysis.Components)
			{
				bool found = false;

				foreach (Result result in test.Results)
				{
					if (result.ResultName == component.VersionedComponentName)
					{
						LoadResult(result);
						foundResults.Add(result);
						if (FinishedPage) return;
						found = true;
						break;
					}
				}

				if (found) continue;

				// Load from the Analysis Component

				var resultBlank = LoadComponent(test, component);
				foundResults.Add(resultBlank);
				if (FinishedPage) return;
			}

			// Adhocs

			foreach (Result result in test.Results)
			{
				if (foundResults.Contains(result)) continue;
				LoadResult(result);
				if (FinishedPage) return;
			}
		}

		/// <summary>
		/// Loads the result.
		/// </summary>
		/// <param name="result">The result.</param>
		public void LoadResult(Result result)
		{
			if (!BaseEntity.IsValid(result)) return;
			if (FinishedPage) return;

			// Only load rows in the correct 'page'

			int start = (PageSize * (Page - 1));

			if (Count >= start + PageSize && PageSize != 0)
			{
				// Drop out if we don't want the record count.

				if (!DetermineCount)
				{
					FinishedPage = true;
					Count = 0;
					return;
				}

				// Increment the count, skip the row.

				Count = Count + 1;
				return;
			}
			
			if (Count >= start)
			{
				var row = ResultDataRow.LoadDataRow(result, Columns);
				Rows.Add(row);
			}

			Count = Count + 1;
		}

		/// <summary>
		/// Loads the component.
		/// </summary>
		/// <param name="test">The test.</param>
		/// <param name="resultName">Name of the result.</param>
		/// <returns></returns>
		public ResultBase LoadComponent(Test test, string resultName)
		{
			var component = (VersionedComponent) test.Analysis.Components[resultName];
			if (!BaseEntity.IsValid(component)) return null;
			return (LoadComponent(test, component));
		}

		/// <summary>
		/// Loads the component.
		/// </summary>
		/// <param name="test">The test.</param>
		/// <param name="component">The component.</param>
		public ResultBase LoadComponent(Test test, VersionedComponent component)
		{
			var result = (Result)test.AddResult(component);
			LoadResult(result);
			return result;
		}

		#endregion
	}
}
