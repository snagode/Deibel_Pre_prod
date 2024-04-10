using System.Collections.Generic;
using System.ComponentModel;
using System.ServiceModel.Web;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.WebApiTasks.Mobile.Data;

namespace Thermo.SampleManager.WebApiTasks.Mobile
{
	/// <summary>
	///  Function Task
	/// </summary>
	[SampleManagerWebApi("mobile.functions")]
	public class FunctionTask : SampleManagerWebApiTask
	{
		#region Functions

		/// <summary>
		/// Default Function List
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/functions", Method = "GET")]
		[Description("List of functions available in the Mobile cabinet matching the basic feature set")]
		public List<Function> FunctionsDefault()
		{
			return Function.Load(Library, EntityManager, Session.DefaultCabinetName);
		}

		/// <summary>
		/// Specific Cabinet Functions
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/functions/{cabinet}", Method = "GET")]
		[Description("List of all functions available in the specified cabinet")]
		public List<Function> Functions(string cabinet)
		{
			return Function.Load(Library, EntityManager, cabinet.ToUpperInvariant(), Function.GetAllFeatures(Library));
		}

		/// <summary>
		/// Check Access
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/functions/available/{function}", Method = "GET")]
		[Description("Check Access to a function")]
		public bool CheckAccess(string function)
		{
			return CheckMenuSecurity(function);
		}

		#endregion
	}
}