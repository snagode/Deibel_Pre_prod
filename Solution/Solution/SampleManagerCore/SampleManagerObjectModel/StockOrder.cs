using System;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the STOCK_ORDER entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class StockOrder : StockOrderBase
	{
		#region Overrides

		/// <summary>
		/// Called when when a property changes.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged(PropertyEventArgs e)
		{
			switch (e.PropertyName)
			{
				case StockOrderPropertyNames.OrderDate:
				case StockOrderPropertyNames.Supplier:

					// Estimated Delivery Date is always Order Date plus suppliers Lead Time
					SetEstimatedDeliveryDate();

					break;
			}
		}

		/// <summary>
		/// Push in the Order number
		/// </summary>
		protected override void OnEntityCreated()
		{
			SetOrderNumber();
		}

		/// <summary>
		/// Setup events to keep track of changes to the entity and its children
		/// </summary>
		protected override void OnEntityLoaded()
		{
			StockOrderItems.Changed -= StockOrderItemsChanged;

			StockOrderItems.Changed += StockOrderItemsChanged;
		}

		/// <summary>
		/// Set the order number before saving
		/// </summary>
		protected override void OnPreCommit()
		{
			if (IsNew() && EstimatedDeliveryDate.IsNull)
			{
				SetEstimatedDeliveryDate();
			}

			if (PropertyHasChanged(StockOrderPropertyNames.Status))
			{
				if (Status.PhraseId == PhraseStkOStat.PhraseIdR)
				{
					foreach (StockOrderItem stockOrderItem in StockOrderItems)
					{
						if (stockOrderItem.Received)
						{
							StockBatch stockBatch = (StockBatch) EntityManager.CreateEntity(
							                                     	StockBatchBase.EntityName,
							                                     	new Identity(stockOrderItem.Stock,
							                                     	             stockOrderItem.Stock.StockBatchs.Count + 1));

							stockBatch.Stock = stockOrderItem.Stock;
							stockBatch.StockOrder = this;
							stockBatch.Supplier = Supplier;
							stockBatch.Description = stockOrderItem.Stock.Description;
							stockBatch.Location = stockOrderItem.Location;
							stockBatch.Batch = stockOrderItem.Batch;
							stockBatch.ExpiryDate = stockOrderItem.ExpiryDate;

							if (string.IsNullOrEmpty(stockOrderItem.Stock.InventoryUnit))
							{
								stockBatch.InitialAmount = stockOrderItem.QuantityReceived;
								stockBatch.Unit = stockOrderItem.Unit;
							}
							else
							{
								stockBatch.InitialAmount = Library.Utils.UnitConvert(stockOrderItem.QuantityReceived, stockOrderItem.Unit,
								                                                     stockOrderItem.Stock.InventoryUnit);
								stockBatch.Unit = stockOrderItem.Stock.InventoryUnit;
							}

							stockBatch.DateCreated = Library.Environment.ClientNow;
							stockBatch.CreatedBy = (PersonnelBase) Library.Environment.CurrentUser;

							stockOrderItem.Stock.StockBatchs.Add(stockBatch);

							EntityManager.Transaction.Add(stockOrderItem.Stock);
						}
					}
				}
				else
				{
					foreach (StockOrderItem stockOrderItem in StockOrderItems)
					{
						if (stockOrderItem.QuantityReceived == 0)
							stockOrderItem.QuantityReceived = stockOrderItem.QuantityOrdered;
					}
				}
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Sets the estimated delivery date.
		/// </summary>
		private void SetEstimatedDeliveryDate()
		{
			if ((!OrderDate.IsNull) && (Supplier != null))
			{
				foreach (StockOrderItem stockOrderItem in StockOrderItems)
				{
					foreach (SupplierDetail supplierStock in Supplier.SupplierDetails)
					{
						if (supplierStock.Stock == stockOrderItem.Stock)
						{
							NullableDateTime stockDeliveryDate = OrderDate.Value + supplierStock.LeadTime;

							if ((DateTime) stockDeliveryDate > (DateTime) EstimatedDeliveryDate)
								EstimatedDeliveryDate = stockDeliveryDate;
						}
					}
				}
			}
		}

		/// <summary>
		/// Sets the order number.
		/// </summary>
		private void SetOrderNumber()
		{
			PackedDecimal highestOrderNumber =
				(PackedDecimal) EntityManager.SelectMax(TableNames.StockOrder, "STOCK_ORDER_NUMBER");

			if (highestOrderNumber == null)
				StockOrderNumber = new PackedDecimal(1);
			else
				StockOrderNumber = ++highestOrderNumber;
		}

		/// <summary>
		/// Order has changed - recalculate the cost
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void StockOrderItemsChanged(object sender, EntityCollectionEventArgs e)
		{
			if (!CostOverride)
			{
				CalculateTotalCost();
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Summ the cost of the stock order items
		/// </summary>
		public void CalculateTotalCost()
		{
			TotalCost = 0;

			foreach (StockOrderItem stockOrderItem in StockOrderItems)
			{
				TotalCost += stockOrderItem.Cost;
			}
		}

		#endregion
	}
}