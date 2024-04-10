using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the ANALYSIS_VIEW entity.
	/// </summary> 
	[SampleManagerEntity(AnalysisViewBase.EntityName)]
	public class AnalysisView : AnalysisViewBase
	{
		#region Public Methods

		/// <summary>
		/// Converts this instance to a Versioned Analysis.
		/// </summary>
		/// <returns></returns>
		public VersionedAnalysis ToVersionedAnalysis()
		{
			VersionedAnalysis analysis = EntityManager.Select(TableNames.VersionedAnalysis, new Identity(this.Name, this.AnalysisVersion)) as VersionedAnalysis;

			return analysis;
		}

		#endregion
	}
}
