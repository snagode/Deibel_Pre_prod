using System.Collections.Generic;
using System.Runtime.Serialization;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Result Entry Function Information
	/// </summary>
	[DataContract(Name="resultentryByTest")]
	public class FunctionResultEntryByTest : FunctionResultEntry
	{
		#region Constants

		/// <summary>
		/// The result entry function number
		/// </summary>
		public const int FunctionResultEntryByTestNumber = 15646;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionResultEntryByTest"/> class.
		/// </summary>
		public FunctionResultEntryByTest()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionResultEntryByTest"/> class.
		/// </summary>
		/// <param name="rmb"></param>
		public FunctionResultEntryByTest(ExplorerRmb rmb) : this(rmb.Menuproc)
		{
			Feature = FeatureName;
			SearchesUri = MakeLink("/mobile/searches/{0}/{1}", rmb.Cabinet, Name);
			ExecuteUri = MakeLink("/mobile/results/tests");
			SaveUri = MakeLink("/mobile/results/tests");
			TableName = TestBase.StructureTableName;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionResultEntryByTest"/> class.
		/// </summary>
		/// <param name="menu">The menu.</param>
		public FunctionResultEntryByTest(MasterMenuBase menu) : base(menu)
		{
			Description = "Result Entry By Test";
			LoadColumns(menu.Library);
		}

		#endregion

		#region Feature Support

		/// <summary>
		/// Result Entry Functions
		/// </summary>
		/// <param name="rmb">The RMB.</param>
		/// <returns></returns>
		public new static bool IsFunction(ExplorerRmb rmb)
		{
			if (rmb.Menuproc == null) return false;
			if (rmb.Menuproc.ProcedureNum == FunctionResultEntryByTestNumber)
			{
				return true;
			}

			return false;
		}

		#endregion

		#region Load Columns

		/// <summary>
		/// Loads the columns.
		/// </summary>
		/// <param name="library">The library.</param>
		public override void LoadColumns(StandardLibrary library)
		{
			Columns = LoadResultEntryColumns(library);
		}

		/// <summary>
		/// Loads the result entry columns.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <returns></returns>
		public new static List<Column> LoadResultEntryColumns(StandardLibrary library)
		{
			return ResultData.LoadResultEntryColumns(library, ResultModeModify, ResultColumnsSubSamples);
		}

		#endregion
	}
}
