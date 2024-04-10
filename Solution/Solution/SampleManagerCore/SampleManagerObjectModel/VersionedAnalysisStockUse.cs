using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the VERSIONED_ANALYSIS_STOCK_USE entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class VersionedAnalysisStockUse : VersionedAnalysisStockUseBase
	{
		#region Overrides

		/// <summary>
		/// Called when when a property changes.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged(PropertyEventArgs e)
		{
			if (e.PropertyName == VersionedAnalysisStockUsePropertyNames.Stock)
			{
				if (string.IsNullOrEmpty(Unit))
					Unit = Stock.InventoryUnit;
			}
		}

		#endregion
	}
}