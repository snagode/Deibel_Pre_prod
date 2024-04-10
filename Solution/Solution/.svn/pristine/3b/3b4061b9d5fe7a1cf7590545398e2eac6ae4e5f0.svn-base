using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of STOCK BATCH SPLIT.
	/// </summary>
	[SampleManagerTask("StockBatchSplitTask", "GENERAL", "STOCK_BATCH")]
	public class StockBatchSplitTask : DefaultSingleEntityTask
	{
		#region Member Variables

		private FormStockBatchSplit m_Form;
		private StockBatch m_StockBatch;

		#endregion

		#region Handle Split Form

		/// <summary>
		/// Handles the Loaded event of the Stock Batch Split Form.
		/// </summary>
		protected override void MainFormLoaded()
		{
			m_Form = (FormStockBatchSplit) MainForm;
			m_StockBatch = (StockBatch) MainForm.Entity;

			m_Form.AmountToMove.Maximum = m_StockBatch.CurrentQuantity;
			m_Form.AmountToMove.Minimum = 0.0;

			IEntityCollection nothing = EntityManager.CreateEntityCollection(TableNames.Location);
			m_Form.CurrentLocation.Browse = BrowseFactory.CreateEntityBrowse(nothing); 

			m_Form.AddToBatchRadioButton.CheckedChanged += AddToBatchChanged;
		}

		/// <summary>
		/// Handles the Closing event of the Stock Batch Split Form.
		/// </summary>
		protected override bool OnPreSave()
		{
			if (m_Form.AmountToMove.Number == 0)
			{
				Library.Utils.FlashMessage(m_Form.StringTable.NoAmount, m_Form.StringTable.NoAmountTitle);
				return false;
			}

			if (m_Form.AddToBatchRadioButton.Checked &&
			    ((m_Form.TargetBatch.Entity == null) || m_Form.TargetBatch.Entity.IsNull()))
			{
				Library.Utils.FlashMessage(m_Form.StringTable.NoBatch, m_Form.StringTable.NoBatchTitle);
				return false;
			}

			double amount = m_Form.AmountToMove.Number;
			string unit = m_StockBatch.Unit;

			if (!string.IsNullOrEmpty(m_StockBatch.Stock.InventoryUnit) &&
				(m_StockBatch.Stock.InventoryUnit != m_StockBatch.Unit))
			{
				unit = m_StockBatch.Stock.InventoryUnit;
				amount = Library.Utils.UnitConvert(m_Form.AmountToMove.Number,
				                                   m_StockBatch.Unit,
				                                   m_StockBatch.Stock.InventoryUnit);
			}

			CreateRemovalRecord(amount, unit);

			if (m_Form.CreateBatchRadioButton.Checked)
			{
				CreateNewStockBatch(amount, unit);
			}
			else
			{
				CreateAdditionRecord(amount, unit, m_Form.TargetBatch.Entity);
			}

			return true;
		}

		/// <summary>
		/// Handles the Closing event of the Stock Batch Split Form.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void AddToBatchChanged(object sender, CheckedChangedEventArgs e)
		{
			m_Form.TargetBatch.Enabled = e.Checked;
			m_Form.NewLocation.Enabled = !e.Checked;
		}

		/// <summary>
		/// Creates the removal record.
		/// </summary>
		/// <param name="amount">The amount.</param>
		/// <param name="unit">The unit.</param>
		private void CreateRemovalRecord(double amount, string unit)
		{
			StockInventory stockInventory = (StockInventory) EntityManager.CreateEntity(StockInventoryBase.EntityName);

			stockInventory.StockBatchInventory = m_StockBatch;
			stockInventory.SetUseType(PhraseStockUse.PhraseIdMOVEOUT);
			stockInventory.Amount = amount;
			stockInventory.Unit = unit;
			stockInventory.DateCreated = Library.Environment.ClientNow;
			stockInventory.CreatedBy = (PersonnelBase) Library.Environment.CurrentUser;

			m_StockBatch.StockInventories.Add(stockInventory);
			EntityManager.Transaction.Add(m_StockBatch);
		}

		/// <summary>
		/// Creates the addition record.
		/// </summary>
		/// <param name="amount">The amount.</param>
		/// <param name="unit">The unit.</param>
		/// <param name="batch">The batch.</param>
		private void CreateAdditionRecord(double amount, string unit, IEntity batch)
		{
			StockBatch stockBatchToAddTo = (StockBatch) batch;

			StockInventory stockInventory = (StockInventory) EntityManager.CreateEntity(StockInventoryBase.EntityName);

			stockInventory.StockBatchInventory = stockBatchToAddTo;
			stockInventory.SetUseType(PhraseStockUse.PhraseIdMOVEIN);
			stockInventory.Amount = amount;
			stockInventory.Unit = unit;
			stockInventory.SourceStockBatchId = m_StockBatch.StockBatchId;
			stockInventory.DateCreated = Library.Environment.ClientNow;
			stockInventory.CreatedBy = (PersonnelBase) Library.Environment.CurrentUser;

			stockBatchToAddTo.StockInventories.Add(stockInventory);
			EntityManager.Transaction.Add(stockBatchToAddTo);
		}

		/// <summary>
		/// Creates the new stock batch.
		/// </summary>
		/// <param name="amount">The amount.</param>
		/// <param name="unit">The unit.</param>
		private void CreateNewStockBatch(double amount, string unit)
		{
			StockBatch stockBatch = (StockBatch) EntityManager.CreateEntity(
			                                     	StockBatchBase.EntityName,
			                                     	new Identity(m_StockBatch.Stock,
			                                     	             m_StockBatch.Stock.StockBatchs.Count + 1));

			stockBatch.Stock = m_StockBatch.Stock;
			stockBatch.Supplier = m_StockBatch.Supplier;
			stockBatch.Description = m_StockBatch.Stock.Description;
			stockBatch.Location = (Location) m_Form.NewLocation.Entity;
			stockBatch.Batch = m_StockBatch.Batch;
			stockBatch.ExpiryDate = m_StockBatch.ExpiryDate;
			stockBatch.InitialAmount = amount;
			stockBatch.Unit = unit;
			stockBatch.SourceStockBatchId = m_StockBatch.StockBatchId;
			stockBatch.DateCreated = Library.Environment.ClientNow;
			stockBatch.CreatedBy = (PersonnelBase) Library.Environment.CurrentUser;

			m_StockBatch.Stock.StockBatchs.Add(stockBatch);
			EntityManager.Transaction.Add(m_StockBatch.Stock);
		}

		#endregion
	}
}