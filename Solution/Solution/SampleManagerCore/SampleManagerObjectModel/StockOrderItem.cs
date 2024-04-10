using System;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the STOCK_ORDER_ITEM entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class StockOrderItem : StockOrderItemBase
	{
		#region Member Variables

		private bool m_SettingProperties;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets a valid reference to the Unit.
		/// </summary>
		/// <value>The valid unit reference.</value>
		public UnitHeaderBase UnitValid
		{
			get
			{
				if (string.IsNullOrEmpty(Unit))
				{
					return null;
				}

				return (UnitHeaderBase) EntityManager.Select(UnitHeaderBase.EntityName, Unit);
			}
			set
			{
				if (value == null)
				{
					Unit = string.Empty;
				}
				else
				{
					Unit = value.Identity;
				}
			}
		}

		/// <summary>
		/// Links to Type LocationBase
		/// </summary>
		/// <value></value>
		[PromptHierarchyLink(LocationBase.EntityName, true, ObjectModel.Location.HierarchyPropertyName)]
		public override LocationBase Location
		{
			get
			{
				return base.Location;
			}
			set
			{
				base.Location = value;
			}
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Called when when a property changes.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged(PropertyEventArgs e)
		{
			if (!m_SettingProperties)
			{
				m_SettingProperties = true;

				switch (e.PropertyName)
				{
					case StockOrderItemPropertyNames.SupplierCode:

						SetFromOrderCode();

						Unit = SupplierUnit;

						if (string.IsNullOrEmpty(Unit))
							Unit = Stock.InventoryUnit;

						QuantityFromPreferredAmount();

						CalculateCost();

						break;

					case StockOrderItemPropertyNames.Stock:

						SetOrderCode();

						Unit = SupplierUnit;

						if (string.IsNullOrEmpty(Unit))
							Unit = Stock.InventoryUnit;

						QuantityFromPreferredAmount();

						CalculateCost();

						break;

					case StockOrderItemPropertyNames.NumberOrdered:

						QuantityFromNumber();

						CalculateCost();

						break;

					case StockOrderItemPropertyNames.QuantityOrdered:

						NumberFromQuantity();

						CalculateCost();

						break;
				}

				m_SettingProperties = false;
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Sets from order code.
		/// </summary>
		private void SetFromOrderCode()
		{
			StockOrder stockOrder = (StockOrder)Parent;

			if ((!stockOrder.IsNull()) && (!stockOrder.Supplier.IsNull()))
			{
				foreach (SupplierDetail supplierDetail in stockOrder.Supplier.SupplierDetails)
				{
					if ((supplierDetail.Stock == Stock) &&
						(supplierDetail.SupplierCode == SupplierCode))
					{
						SupplierDescription = supplierDetail.SupplierDescription;
						SupplierQuantity = supplierDetail.SupplierQuantity;
						SupplierUnit = supplierDetail.SupplierUnit;
						break;
					}
				}
			}
		}

		/// <summary>
		/// Sets the order code.
		/// </summary>
		private void SetOrderCode()
		{
			StockOrder stockOrder = (StockOrder) Parent;

			if ((!stockOrder.IsNull()) && (!stockOrder.Supplier.IsNull()))
			{
				SupplierDetail bestSupplierDetail = null;

				foreach (SupplierDetail supplierDetail in stockOrder.Supplier.SupplierDetails)
				{
					if (supplierDetail.Stock == Stock)
					{
						if (bestSupplierDetail == null)
							bestSupplierDetail = supplierDetail;

						else if ((supplierDetail.SupplierQuantity == Stock.PreferredOrderAmount) &&
						         (supplierDetail.SupplierUnit == Stock.PreferredOrderUnit))
							bestSupplierDetail = supplierDetail;
					}
				}

				if (bestSupplierDetail != null)
				{
					SupplierCode = bestSupplierDetail.SupplierCode;
					SupplierDescription = bestSupplierDetail.SupplierDescription;
					SupplierQuantity = bestSupplierDetail.SupplierQuantity;
					SupplierUnit = bestSupplierDetail.SupplierUnit;
				}
			}
		}

		/// <summary>
		/// Quantities from preferred amount.
		/// </summary>
		private void QuantityFromPreferredAmount()
		{
			if (SupplierQuantity == 0)
			{
				NumberOrdered = 1;
			}
			else if (Stock.PreferredOrderAmount != 0)
			{
				double supplierAmount = 0;

				if (Stock.PreferredOrderUnit == SupplierUnit)
				{
					supplierAmount = SupplierQuantity;
				}
				else if ((!string.IsNullOrEmpty(Stock.PreferredOrderUnit)) && (!string.IsNullOrEmpty(SupplierUnit)))
				{
					supplierAmount = Library.Utils.UnitConvert(SupplierQuantity, SupplierUnit, Stock.PreferredOrderUnit);
				}

				if (supplierAmount != 0)
				{
					int num = (int) Math.Truncate(Stock.PreferredOrderAmount/supplierAmount);

					if ((num*supplierAmount) < Stock.PreferredOrderAmount)
					{
						NumberOrdered = num + 1;
					}
					else
					{
						NumberOrdered = num;
					}
				}
			}

			if (NumberOrdered == 0)
			{
				NumberOrdered = 1;
				QuantityOrdered = SupplierQuantity;
			}

			QuantityFromNumber();
		}

		/// <summary>
		/// Number from quantity.
		/// </summary>
		private void NumberFromQuantity()
		{
			if ((SupplierQuantity != 0) && (QuantityOrdered != 0))
			{
				int num = (int) Math.Truncate(QuantityOrdered/SupplierQuantity);

				if ((num*SupplierQuantity) < QuantityOrdered)
				{
					NumberOrdered = num + 1;
				}
				else
				{
					NumberOrdered = num;
				}
			}
		}

		/// <summary>
		/// Quantity from number.
		/// </summary>
		private void QuantityFromNumber()
		{
			if ((SupplierQuantity != 0) && (NumberOrdered != 0))
			{
				QuantityOrdered = NumberOrdered*SupplierQuantity;
			}
		}

		/// <summary>
		/// Calculate the cost from the data provided
		/// </summary>
		private void CalculateCost()
		{
			StockOrder stockOrder = (StockOrder) Parent;

			if ((!stockOrder.CostOverride) && (NumberOrdered != 0) && (!stockOrder.IsNull()) && (!stockOrder.Supplier.IsNull()))
			{
				foreach (SupplierDetail supplierDetail in stockOrder.Supplier.SupplierDetails)
				{
					if ((supplierDetail.Stock == Stock) &&
					    (supplierDetail.SupplierCode == SupplierCode))
					{
						Cost = supplierDetail.Cost*NumberOrdered;
						break;
					}
				}
			}
		}

		#endregion
	}
}