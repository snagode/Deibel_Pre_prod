using System;
using System.Runtime.Serialization;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Basic Result Entry
	/// </summary>
	[DataContract(Name = "basic.receive")]
	public class FunctionBasicReceive : FunctionAuxiliary
	{
		#region Constants 

		private const string FunctionReceiveName = "Receive";

		/// <summary>
		/// The procedure number for recieve
		/// </summary>
		public const int FunctionReceiveNumber = 11008;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionBasicReceive"/> class.
		/// </summary>
		public FunctionBasicReceive()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionBasicReceive"/> class.
		/// </summary>
		/// <param name="rmb">The RMB.</param>
		public FunctionBasicReceive(ExplorerRmb rmb) : base(rmb)
		{
			Name = FunctionReceiveName;
			SearchesUri = MakeLink("/mobile/searches/receivesamples");
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
			if (rmb.Menuproc.ProcedureNum == FunctionReceiveNumber)
			{
				return true;
			}

			return false;
		}

		#endregion
	}
}
