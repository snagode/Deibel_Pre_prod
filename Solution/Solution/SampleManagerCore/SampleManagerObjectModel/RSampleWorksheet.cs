using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the R_SAMPLE_WORKSHEET entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class RSampleWorksheet : RSampleWorksheetBase
	{
		/// <summary>
		/// Gets the analysis test count.
		/// </summary>
		/// <value>
		/// The analysis test count.
		/// </value>
		[PromptText]
		public string AnalysisTestCount
		{
			get { return Analysis.Trim() + "/" + TestCount; }
		}
	}
}