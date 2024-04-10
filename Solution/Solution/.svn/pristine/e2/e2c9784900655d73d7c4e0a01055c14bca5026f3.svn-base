using System;
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
	[DataContract(Name="resultentry")]
	public class FunctionResultEntry : Function
	{
		#region Constants

		/// <summary>
		/// Result Entry Feature
		/// </summary>
		public const string FeatureName = "Result Entry";

		/// <summary>
		/// The result entry function number
		/// </summary>
		public const int FunctionResultEntryNumber = 15638;

		/// <summary>
		/// The result mode modify
		/// </summary>
		public const string ResultModeModify = "MODIFY";

		/// <summary>
		/// The result mode authorise
		/// </summary>
		public const string ResultModeAuthorise = "AUTHORISE";

		/// <summary>
		/// The result mode display
		/// </summary>
		public const string ResultModeDisplay = "DISPLAY";

		/// <summary>
		/// The result mode add
		/// </summary>
		public const string ResultModeAdd = "ADD";

		/// <summary>
		/// The result mode test
		/// </summary>
		public const string ResultModeTest = "TEST";

		/// <summary>
		/// The result mode complete
		/// </summary>
		public const string ResultModeComplete = "COMPLETE";

		/// <summary>
		/// The result columns sub samples
		/// </summary>
		public const string ResultColumnsSubSamples = "TSS_";

		/// <summary>
		/// The result columns samples
		/// </summary>
		public const string ResultColumnsSamples = "TSR_";

		/// <summary>
		/// The result columns jobs
		/// </summary>
		public const string ResultColumnsJobs = "TJR_";

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the execute URI.
		/// </summary>
		/// <value>
		/// The execute URI.
		/// </value>
		[DataMember(Name = "executeUri")]
		public Uri ExecuteUri { get; set; }

		/// <summary>
		/// Gets or sets the save URI.
		/// </summary>
		/// <value>
		/// The save URI.
		/// </value>
		[DataMember(Name = "saveUri")]
		public Uri SaveUri { get; set; }

		/// <summary>
		/// Gets or sets the columns.
		/// </summary>
		/// <value>
		/// The columns.
		/// </value>
		[DataMember(Name = "columns")]
		public List<Column> Columns { get; set; }

		/// <summary>
		/// Gets or sets the name of the table.
		/// </summary>
		/// <value>
		/// The name of the table.
		/// </value>
		[DataMember(Name = "tableName")]
		public string TableName { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionResultEntry"/> class.
		/// </summary>
		public FunctionResultEntry()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public FunctionResultEntry(ExplorerRmb rmb) : this(rmb.Menuproc)
		{
			Feature = FeatureName;
			SearchesUri = MakeLink("/mobile/searches/{0}/{1}", rmb.Cabinet, Name);
			ExecuteUri = MakeLink("/mobile/results/samples");
			SaveUri = MakeLink("/mobile/results/samples");
			TableName = SampleBase.StructureTableName;
		}

		/// <summary>
		/// Loads the menu.
		/// </summary>
		/// <param name="menu">The menu.</param>
		public FunctionResultEntry(MasterMenuBase menu) : base(menu)
		{
			Description = "Result Entry";
			LoadColumns(menu.Library);
		}

		#endregion

		#region Feature Support

		/// <summary>
		/// Result Entry Functions
		/// </summary>
		/// <param name="rmb">The RMB.</param>
		/// <returns></returns>
		public static bool IsFunction(ExplorerRmb rmb)
		{
			if (rmb.Menuproc == null) return false;
			if (rmb.Menuproc.ProcedureNum == FunctionResultEntryNumber)
			{
				return true;
			}

			return false;
		}

		#endregion

		#region Column Definition

		/// <summary>
		/// Loads the columns.
		/// </summary>
		/// <param name="library">The library.</param>
		public virtual void LoadColumns(StandardLibrary library)
		{
			Columns = LoadResultEntryColumns(library);
		}

		/// <summary>
		/// Loads the result entry columns.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <returns></returns>
		public static List<Column> LoadResultEntryColumns(StandardLibrary library)
		{
			return ResultData.LoadResultEntryColumns(library, ResultModeModify, ResultColumnsSamples);
		}

		#endregion
	}
}
