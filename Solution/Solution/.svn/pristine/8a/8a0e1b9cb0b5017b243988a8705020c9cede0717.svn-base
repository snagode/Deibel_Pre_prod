using System.Globalization;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the R_BATCH_TMPL_COMPONENTS_LIST entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class RBatchTmplComponentsList : RBatchTmplComponentsListBase
	{
		/// <summary>
		/// Structure Field MAX_LIMIT
		/// </summary>
		public override string MaxLimit
		{
			get { return base.MaxLimit == "XXXXX" ? string.Empty : base.MaxLimit; }
		}

		/// <summary>
		/// Structure Field MIN_LIMIT
		/// </summary>
		public override string MinLimit
		{
			get { return base.MinLimit == "XXXXX" ? string.Empty : base.MinLimit; }
		}

		/// <summary>
		/// Gets the places string.
		/// </summary>
		/// <value>
		/// The places string.
		/// </value>
		[PromptText]
		public string PlacesString
		{
			get { return base.Places == -1 ? "X" : base.Places.ToString(CultureInfo.InvariantCulture).Trim(); }
		}
	}
}
