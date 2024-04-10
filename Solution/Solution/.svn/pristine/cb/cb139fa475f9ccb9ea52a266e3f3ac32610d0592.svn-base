using System;
using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.ClientControls.Browse;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of STOCK ORDER.
	/// </summary>
	[SampleManagerTask("StockOrderTask", "LABTABLE", "STOCK_ORDER")]
	public class StockOrderTask : DefaultSingleEntityTask
	{
		#region Member Variables

		private FormStockOrder m_FormStockOrder;
		private EntityBrowse m_StockBrowse;
		private StockOrder m_StockOrder;

		#endregion

		#region Constants

		/// <summary>
		/// Add option
		/// </summary>
		private const string StockOrderModeAdd = "ADD";

		/// <summary>
		/// Modify option
		/// </summary>
		private const string StockOrderModeModify = "MODIFY";

		/// <summary>
		/// Modify option
		/// </summary>
		private const string StockOrderModeDisplay = "DISPLAY";

		#endregion

		#region Overrides

		/// <summary>
		/// Override SetupTask to allow processing of add option
		/// </summary>
		protected override void SetupTask()
		{
			if (Context.LaunchMode == StockOrderModeAdd)
			{
				// Need to add RMB added stocks to stock order
				//if (Context.SelectedItems.EntityType != TableNames.Stock)
				//{
				//}

				Context.SelectedItems.ReleaseAll();
				Context.SelectedItems.Add(EntityManager.CreateEntity(TableNames.StockOrder));
			}

			base.SetupTask();
		}

		/// <summary>
		/// Override MainFormCreated to setup task specifics
		/// </summary>
		protected override void MainFormCreated()
		{
			m_FormStockOrder = (FormStockOrder) MainForm;
			m_StockOrder = (StockOrder) MainForm.Entity;

			// Build up the supplier specific browse

			CreateStockBrowse();
		}

		/// <summary>
		/// Override MainFormLoaded to setup task specifics
		/// </summary>
		protected override void MainFormLoaded()
		{
			m_StockOrder.PropertyChanged += StockOrderPropertyChanged;
			m_FormStockOrder.StockOrderItemGrid.CellEditor += StockOrderItemGridCellEditor;
			m_FormStockOrder.CalculateButton.Click += CalculateButtonClick;

			switch (Context.LaunchMode)
			{
				case StockOrderModeAdd:
					m_FormStockOrder.Description.Enabled = true;
					m_FormStockOrder.Status.ReadOnly = true;
					m_FormStockOrder.Supplier.Enabled = true;
					m_FormStockOrder.PurchaseRequisition.Enabled = true;
					m_FormStockOrder.OrderDate.Enabled = true;
					m_FormStockOrder.EstimatedDeliveryDate.ReadOnly = true;
					m_FormStockOrder.ReceivedDate.ReadOnly = true;
					break;

				case StockOrderModeModify:
					m_FormStockOrder.Description.Enabled = true;
					m_FormStockOrder.Status.ReadOnly = true;
					m_FormStockOrder.Supplier.Enabled = true;
					m_FormStockOrder.PurchaseRequisition.Enabled = true;
					m_FormStockOrder.OrderDate.Enabled = true;
					m_FormStockOrder.EstimatedDeliveryDate.Enabled = true;
					m_FormStockOrder.ReceivedDate.Enabled = true;
					break;

				default:
					m_FormStockOrder.Description.ReadOnly = true;
					m_FormStockOrder.Status.ReadOnly = true;
					m_FormStockOrder.Supplier.ReadOnly = true;
					m_FormStockOrder.PurchaseRequisition.ReadOnly = true;
					m_FormStockOrder.OrderDate.ReadOnly = true;
					m_FormStockOrder.EstimatedDeliveryDate.ReadOnly = true;
					m_FormStockOrder.ReceivedDate.ReadOnly = true;
					break;
			}

			SetCostPromptState();
		}

		#endregion

		#region StockOrderForm handling

		/// <summary>
		/// Creates the stock browse.
		/// </summary>
		private void CreateStockBrowse()
		{
			IQuery supplierDetail = EntityManager.CreateQuery(TableNames.SupplierDetail);
			supplierDetail.AddEquals("SUPPLIER", m_StockOrder.Supplier);
			List<object> stockList = EntityManager.SelectDistinct(supplierDetail, "STOCK");

			IQuery supplierStock = EntityManager.CreateQuery(TableNames.Stock);

			if (stockList.Count > 0)
				supplierStock.AddIn("IDENTITY", stockList);

			supplierStock.AddOrder("IDENTITY", true);

			m_StockBrowse = BrowseFactory.CreateEntityBrowse(supplierStock);
		}

		/// <summary>
		/// Stocks the order property changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.PropertyEventArgs"/> instance containing the event data.</param>
		private void StockOrderPropertyChanged(object sender, PropertyEventArgs e)
		{
			if (e.PropertyName == StockOrderPropertyNames.Supplier)
				CreateStockBrowse();

			else if (e.PropertyName == StockOrderPropertyNames.CostOverride)
				SetCostPromptState();
		}

		/// <summary>
		/// Republish the list of allowed supplier codes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void StockOrderItemGridCellEditor(object sender, CellEditorEventArgs e)
		{
			switch (e.PropertyName)
			{
				case StockOrderItemPropertyNames.SupplierCode:
				{
					IQuery supplierCodes = EntityManager.CreateQuery(TableNames.SupplierDetail);

					supplierCodes.AddEquals("STOCK", ((StockOrderItem) e.Entity).Stock);
					supplierCodes.AddEquals("SUPPLIER", m_StockOrder.Supplier);

					e.Browse = BrowseFactory.CreateEntityBrowse(supplierCodes, true, SupplierDetailPropertyNames.SupplierCode);
				}
					break;

				case StockOrderItemPropertyNames.Stock:
					e.Browse = m_StockBrowse;
					break;
			}
		}

		/// <summary>
		/// Set the Cost prompt to readonly as required
		/// </summary>
		private void SetCostPromptState()
		{
			if (Context.LaunchMode == StockOrderModeDisplay) return;

			m_FormStockOrder.TotalCost.ReadOnly = !m_StockOrder.CostOverride;

			if (!m_StockOrder.CostOverride)
				m_StockOrder.CalculateTotalCost();

			m_FormStockOrder.StockOrderItemGrid.Columns[StockOrderItemPropertyNames.Cost].ReadOnly = !m_StockOrder.CostOverride;
			m_FormStockOrder.StockOrderItemGrid.Columns[StockOrderItemPropertyNames.Cost].IsGrayBackground =
				!m_StockOrder.CostOverride;

		}

		/// <summary>
		/// Click event on calculate button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CalculateButtonClick(object sender, EventArgs e)
		{
			m_StockOrder.CalculateTotalCost();
		}

		#endregion
	}
}