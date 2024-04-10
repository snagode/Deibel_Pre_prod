using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of STOCK Reconcile.
	/// </summary>
	[SampleManagerTask("StockBatchReconcileTask", "GENERAL", "STOCK_BATCH")]
	public class StockBatchReconcileTask : DefaultSingleEntityTask
	{
		#region Overrides
		/// <summary>
		/// Handles the Closing event of the Stock Batch Stock Take Form.
		/// Set the return value to return the entity if the OK button was pressed.
		/// </summary>
		protected override bool OnPreSave()
		{
			FormStockBatchReconcile form = (FormStockBatchReconcile)MainForm;
			StockBatch stockBatch = (StockBatch) MainForm.Entity;

			StockInventory.CreateInventoryItem(stockBatch, PhraseStockUse.PhraseIdSTOCKTAKE, form.ActualAmount.Number);
			EntityManager.Transaction.Add(stockBatch);
			EntityManager.Commit();

			stockBatch.NeedsRecalc = true;

			return true;
		}

		#endregion
	}
}