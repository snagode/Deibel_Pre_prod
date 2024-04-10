using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the REPORT entity.
	/// </summary>
	[SampleManagerEntity(ReportBase.EntityName)]
	public class Report : ReportBase
	{
		#region Icons

		/// <summary>
		/// Gets the report icon.
		/// </summary>
		/// <value>The report icon.</value>
		[EntityIcon]
		public string ReportIcon
		{
			get
			{
				if (Identity.Contains("LIB")) return "INT_BOOKS";
				if (Identity.Contains("AUX")) return "INT_AUX_ACTION";
				if (Identity.Contains("USER")) return "INT_SETTINGS";

				return "INT_VGL_REPORT";
			}
		}

		#endregion
	}
}
