using System;
using System.Runtime.Serialization;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Basic Result Entry
	/// </summary>
	[DataContract(Name = "basic.resultentry")]
	public class FunctionBasicResultEntry : FunctionResultEntry
	{
		#region Constants

		private const string FunctionResultEntryName = "Result";

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionBasicResultEntry"/> class.
		/// </summary>
		public FunctionBasicResultEntry()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionBasicResultEntry"/> class.
		/// </summary>
		/// <param name="rmb"></param>
		public FunctionBasicResultEntry(ExplorerRmb rmb) : base(rmb)
		{
			Name = FunctionResultEntryName;
			SearchesUri = MakeLink("/mobile/searches/resultentry");
			DisplayText = null;
			Description = null;
		}

		#endregion

		#region Feature Support

		/// <summary>
		/// Basic Result Entry Functions
		/// </summary>
		/// <param name="rmb">The RMB.</param>
		/// <returns></returns>
		public new static bool IsFunction(ExplorerRmb rmb)
		{
			if (rmb.Menuproc == null) return false;
			if (rmb.Menuproc.ProcedureNum == FunctionResultEntryNumber)
			{
				return true;
			}

			return false;
		}

		#endregion
	}
}
