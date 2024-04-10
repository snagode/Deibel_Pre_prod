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
	[SampleManagerTask("StockBatchConsumptionTask", "GENERAL", "STOCK_BATCH")]
	public class StockBatchConsumptionTask : DefaultSingleEntityTask
	{
		#region Overrides

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm" /> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			((FormStockBatchConsumption)MainForm).ActualAmount.Number = ((StockBatch)MainForm.Entity).CurrentQuantity;
			((FormStockBatchConsumption)MainForm).Adjustment.NumberChanged += Adjustment_NumberChanged;
			base.MainFormLoaded();
		}

		/// <summary>
		/// Handles the NumberChanged event of the Adjustment control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Library.ClientControls.RealChangedEventArgs"/> instance containing the event data.</param>
		void Adjustment_NumberChanged(object sender, Library.ClientControls.RealChangedEventArgs e)
		{
			((FormStockBatchConsumption)MainForm).ActualAmount.Number = ((StockBatch)MainForm.Entity).CurrentQuantity - e.Number;
		}

		/// <summary>
		/// Handles the Closing event of the Stock Batch Stock Take Form.
		/// Set the return value to return the entity if the OK button was pressed.
		/// </summary>
		protected override bool OnPreSave()
		{
			FormStockBatchConsumption form = (FormStockBatchConsumption)MainForm;
			StockBatch stockBatch = (StockBatch)MainForm.Entity;
			
			StockInventory.CreateInventoryItem(stockBatch, PhraseStockUse.PhraseIdCONSUME, form.Adjustment.Number);
			EntityManager.Transaction.Add(stockBatch);
			EntityManager.Commit();
			
			stockBatch.NeedsRecalc = true;

			return true;
		}

		#endregion
	}
}