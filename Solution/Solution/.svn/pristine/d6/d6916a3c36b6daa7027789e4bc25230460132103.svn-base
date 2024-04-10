using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of STOCK BATCH MOVE.
	/// </summary>
	[SampleManagerTask("StockBatchMoveTask", "GENERAL", "STOCK_BATCH")]
	public class StockBatchMoveTask : DefaultSingleEntityTask
	{
		#region Handle Move Form

		/// <summary>
		/// Handles the Loaded event of the Stock Batch Move Form.
		/// </summary>
		protected override void MainFormLoaded()
		{
			FormStockBatchMove form = (FormStockBatchMove) MainForm;
			StockBatch stockBatch = (StockBatch) MainForm.Entity;

			IEntityCollection nothing = EntityManager.CreateEntityCollection(TableNames.Location);
			form.CurrentLocation.Entity = stockBatch.Location;
			form.CurrentLocation.Browse = BrowseFactory.CreateEntityBrowse(nothing);
		}

		#endregion
	}
}