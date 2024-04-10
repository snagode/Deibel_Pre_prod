using System;
using System.Runtime.Serialization;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Basic Result Entry
	/// </summary>
	[DataContract(Name="basic.move")]
	public class FunctionBasicMove : FunctionAuxiliary
	{
		#region Constants 

		private const string FunctionMoveName = "Move";

		/// <summary>
		/// The procedure number for move
		/// </summary>
		public const int FunctionMoveNumber = 11004;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionBasicMove"/> class.
		/// </summary>
		public FunctionBasicMove()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionBasicMove"/> class.
		/// </summary>
		/// <param name="rmb">The RMB.</param>
		public FunctionBasicMove(ExplorerRmb rmb) : base(rmb)
		{
			Name = FunctionMoveName;
			SearchesUri = MakeLink("/mobile/searches/movesamples");
			DisplayText = null;
			Description = null;
		}

		#endregion

		#region Feature Support

		/// <summary>
		/// Determines whether the specified RMB is this function.
		/// </summary>
		/// <param name="rmb">The RMB.</param>
		/// <returns></returns>
		public new static bool IsFunction(ExplorerRmb rmb)
		{
			if (rmb.Menuproc == null) return false;
			if (rmb.Menuproc.ProcedureNum == FunctionMoveNumber)
			{
				return true;
			}

			return false;
		}

		#endregion
	}
}
