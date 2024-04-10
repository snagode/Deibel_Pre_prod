using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the R_WORK_PROFILE_COMPONENTS_LIST entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class RWorkProfileComponentsList : RWorkProfileComponentsListBase
	{
		/// <summary>
		/// Gets the name of the analysis/version.
		/// </summary>
		/// <value>
		/// The name of the analysis/version.
		/// </value>
		[PromptText]
		public string AnalysisName
		{
			get { return Analysis + " / " + AnalysisVersion; }
		}

		/// <summary>
		/// Gets the level identifier display text.
		/// </summary>
		/// <value>
		/// The level identifier display text.
		/// </value>
		[PromptText]
		public string LevelIdText
		{
			get
			{
				if (LevelId.ToString() == "XXXXX") return string.Empty;
				return LevelId.ToString();
			}
		}

		/// <summary>
		/// Structure Field MIN_LIMIT
		/// </summary>
		public override string MinLimit
		{
			get
			{
				if (base.MinLimit == "XXXXX") return string.Empty;
				return base.MinLimit;
			}
		}

		/// <summary>
		/// Structure Field MAX_LIMIT
		/// </summary>
		public override string MaxLimit
		{
			get
			{
				if (base.MaxLimit == "XXXXX") return string.Empty;
				return base.MaxLimit;
			}
		}
	}
}