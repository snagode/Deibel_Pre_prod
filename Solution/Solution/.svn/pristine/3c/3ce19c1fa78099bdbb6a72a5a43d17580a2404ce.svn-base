using System.Collections.Generic;
using System.ComponentModel;
using System.ServiceModel.Web;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.WebApiTasks.Mobile.Data;

namespace Thermo.SampleManager.WebApiTasks.Mobile
{
	/// <summary>
	///  Features
	/// </summary>
	[SampleManagerWebApi("mobile.features")]
	public class FeatureTask : SampleManagerWebApiTask
	{
		#region Features

		/// <summary>
		/// All available features
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/features", Method = "GET")]
		[Description("List of all supported Mobile features available from this Web API")]
		public List<string> Features()
		{
			return Function.GetAllFeatures(Library);
		}

		#endregion
	}
}

