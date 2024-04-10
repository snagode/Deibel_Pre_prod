using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the STOCK_INVENTORY entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class StockInventory : StockInventoryBase
	{
		#region Overrides

		/// <summary>
		/// Set defaults for new record
		/// </summary>
		protected override void OnEntityCreated()
		{
			DateCreated = Library.Environment.ClientNow;
			CreatedBy = (PersonnelBase) Library.Environment.CurrentUser;

			if ((StockBatchInventory != null) && (!StockBatchInventory.IsNull()))
			{
				if (StockBatchInventory.Stock != null)
					Unit = StockBatchInventory.Stock.InventoryUnit;
			}
		}

		/// <summary>
		/// Make sure the Stock Batch Id is set correctly
		/// </summary>
		protected override void OnPreCommit()
		{
			StockBatchId = StockBatchInventory.StockBatchId;
		}

		/// <summary>
		/// Copy relevant information from stock to stock_inventory
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged(PropertyEventArgs e)
		{
			if (e.PropertyName == StockInventoryPropertyNames.Stock)
				Unit = StockBatchInventory.Stock.InventoryUnit;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets a valid reference to the Unit unit.
		/// </summary>
		/// <value>The valid Unit unit.</value>
		public UnitHeaderBase UnitValid
		{
			get
			{
				if (string.IsNullOrEmpty(Unit))
					return null;

				return (UnitHeaderBase) EntityManager.Select(UnitHeaderBase.EntityName, Unit);
			}
			set
			{
				if (value == null)
					Unit = string.Empty;
				else
					Unit = value.Identity;
			}
		}

		#endregion

		/// <summary>
		/// Create an inventory record for stock batch
		/// </summary>
		/// <param name="stockBatch"></param>
		/// <param name="inventoryType"></param>
		/// <param name="amount"></param>
		public static void CreateInventoryItem(StockBatch stockBatch, string inventoryType, double amount)
		{
			StockInventory stockInventory = (StockInventory)stockBatch.EntityManager.CreateEntity(EntityName);

			stockInventory.StockBatchInventory = stockBatch;
			stockInventory.SetUseType(inventoryType);
			stockInventory.Amount = amount;
			stockInventory.Unit = stockBatch.Unit;
			stockInventory.DateCreated = stockBatch.Library.Environment.ClientNow;
			stockInventory.CreatedBy = (PersonnelBase)stockBatch.Library.Environment.CurrentUser;

			stockBatch.StockInventories.Add(stockInventory);
		}
	}
}