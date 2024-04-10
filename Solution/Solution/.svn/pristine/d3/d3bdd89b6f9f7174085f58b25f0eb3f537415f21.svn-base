using System;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of STOCK ORDER status.
	/// </summary>
	[SampleManagerTask("StockOrderStatusTask", "LABTABLE", "STOCK_ORDER")]
	public class StockOrderStatusTask : DefaultSingleEntityTask
	{
		#region Member Variables

		private FormStockOrderStatus m_FormStockOrderStatus;
		private string m_NewStockOrderStatus;
		private StockOrder m_StockOrder;

		#endregion

		#region Constants

		/// <summary>
		/// Cancel option
		/// </summary>
		private const string StockOrderModeCancel = "CANCEL";

		/// <summary>
		/// Order option
		/// </summary>
		private const string StockOrderModeOrder = "ORDER";

		/// <summary>
		/// Receive option
		/// </summary>
		private const string StockOrderModeReceive = "RECEIVE";

		#endregion

		#region Overrides

		/// <summary>
		/// Add the status check to the browse query
		/// </summary>
		/// <param name="query"></param>
		protected override void FindSingleEntityQuery(IQuery query)
		{
			switch (Context.TaskParameters[1].Trim())
			{
				case StockOrderModeOrder:
					query.AddEquals("STATUS", PhraseStkOStat.PhraseIdW);
					break;

				case StockOrderModeCancel:
					query.PushBracket();
					query.AddEquals("STATUS", PhraseStkOStat.PhraseIdW);
					query.AddOr();
					query.AddEquals("STATUS", PhraseStkOStat.PhraseIdO);
					query.PopBracket();
					break;

				case StockOrderModeReceive:
					query.AddEquals("STATUS", PhraseStkOStat.PhraseIdO);
					break;
			}
		}

		/// <summary>
		/// Check passed entity is compatible with operation
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		protected override bool FindSingleEntityValidate(IEntity entity)
		{
			StockOrder stockOrder = (StockOrder) entity;

			switch (Context.TaskParameters[1].Trim())
			{
				case StockOrderModeOrder:
					return (stockOrder.Status.PhraseId == PhraseStkOStat.PhraseIdW);

				case StockOrderModeCancel:
					return ((stockOrder.Status.PhraseId == PhraseStkOStat.PhraseIdO) ||
					        (stockOrder.Status.PhraseId == PhraseStkOStat.PhraseIdW));

				case StockOrderModeReceive:
					return (stockOrder.Status.PhraseId == PhraseStkOStat.PhraseIdO);
			}
			return false;
		}

		/// <summary>
		/// Override MainFormCreated to setup task specifics
		/// </summary>
		protected override void MainFormCreated()
		{
			m_FormStockOrderStatus = (FormStockOrderStatus)MainForm;
			m_StockOrder = (StockOrder)MainForm.Entity;

			m_StockOrder.PropertyChanged += StockOrderPropertyChanged;
			m_FormStockOrderStatus.CalculateButton.Click += CalculateButtonClick;
		}

		/// <summary>
		/// Override MainFormLoaded to setup task specifics
		/// </summary>
		protected override void MainFormLoaded()
		{
			m_FormStockOrderStatus = (FormStockOrderStatus) MainForm;
			m_StockOrder = (StockOrder) MainForm.Entity;

			Library.Task.StateModified();

			SetCostPromptState();

			switch (Context.TaskParameters[1].Trim())
			{
				case StockOrderModeOrder:

					m_FormStockOrderStatus.Title = m_FormStockOrderStatus.TitleTable.Order;

					m_FormStockOrderStatus.Status.Enabled = false;
					m_FormStockOrderStatus.ReceivedDate.Enabled = false;

					m_NewStockOrderStatus = PhraseStkOStat.PhraseIdO;

					if (m_StockOrder.OrderDate.IsNull)
						m_StockOrder.OrderDate = Library.Environment.ClientNow;

					m_FormStockOrderStatus.SMGrid1.Columns[StockOrderItemPropertyNames.QuantityReceived].ReadOnly = true;
					m_FormStockOrderStatus.SMGrid1.Columns[StockOrderItemPropertyNames.QuantityReceived].IsGrayBackground = true;
					m_FormStockOrderStatus.SMGrid1.Columns[StockOrderItemPropertyNames.Batch].ReadOnly = true;
					m_FormStockOrderStatus.SMGrid1.Columns[StockOrderItemPropertyNames.Batch].IsGrayBackground = true;
					m_FormStockOrderStatus.SMGrid1.Columns[StockOrderItemPropertyNames.ExpiryDate].ReadOnly = true;
					m_FormStockOrderStatus.SMGrid1.Columns[StockOrderItemPropertyNames.ExpiryDate].IsGrayBackground = true;
					m_FormStockOrderStatus.SMGrid1.Columns[StockOrderItemPropertyNames.Location].ReadOnly = true;
					m_FormStockOrderStatus.SMGrid1.Columns[StockOrderItemPropertyNames.Location].IsGrayBackground = true;
					m_FormStockOrderStatus.SMGrid1.Columns[StockOrderItemPropertyNames.Received].ReadOnly = true;
					m_FormStockOrderStatus.SMGrid1.Columns[StockOrderItemPropertyNames.Received].IsGrayBackground = true;

					break;

				case StockOrderModeCancel:

					m_FormStockOrderStatus.Title = m_FormStockOrderStatus.TitleTable.Cancel;

					m_FormStockOrderStatus.Status.Enabled = false;

					m_NewStockOrderStatus = PhraseStkOStat.PhraseIdX;

					break;

				case StockOrderModeReceive:

					m_FormStockOrderStatus.Title = m_FormStockOrderStatus.TitleTable.Receive;

					m_FormStockOrderStatus.Status.Enabled = false;
					m_FormStockOrderStatus.Supplier.Enabled = false;
					m_FormStockOrderStatus.PurchaseRequisition.Enabled = false;
					m_FormStockOrderStatus.OrderDate.Enabled = false;
					m_FormStockOrderStatus.EstimatedDeliveryDate.Enabled = false;

					m_NewStockOrderStatus = PhraseStkOStat.PhraseIdR;

					if (m_StockOrder.ReceivedDate.IsNull)
						m_StockOrder.ReceivedDate = Library.Environment.ClientNow;

					m_FormStockOrderStatus.SMGrid1.Columns[StockOrderItemPropertyNames.SupplierCode].ReadOnly = true;
					m_FormStockOrderStatus.SMGrid1.Columns[StockOrderItemPropertyNames.SupplierCode].IsGrayBackground = true;
					m_FormStockOrderStatus.SMGrid1.Columns[StockOrderItemPropertyNames.NumberOrdered].ReadOnly = true;
					m_FormStockOrderStatus.SMGrid1.Columns[StockOrderItemPropertyNames.NumberOrdered].IsGrayBackground = true;
					m_FormStockOrderStatus.SMGrid1.Columns[StockOrderItemPropertyNames.QuantityOrdered].ReadOnly = true;
					m_FormStockOrderStatus.SMGrid1.Columns[StockOrderItemPropertyNames.QuantityOrdered].IsGrayBackground = true;
					m_FormStockOrderStatus.SMGrid1.Columns[StockOrderItemPropertyNames.Unit].ReadOnly = true;
					m_FormStockOrderStatus.SMGrid1.Columns[StockOrderItemPropertyNames.Unit].IsGrayBackground = true;

					foreach (StockOrderItem stockOrderItem in m_StockOrder.StockOrderItems)
					{
						stockOrderItem.Received = true;

						if (stockOrderItem.Location.IsNull())
							stockOrderItem.Location = stockOrderItem.Stock.DefaultLocation;
					}

					break;

				default:
					throw new ArgumentException(string.Format(Library.Message.GetMessage("GeneralMessages", "EditorModeException"),
					                                          Context.TaskParameters[1].Trim()));
			}
		}

		/// <summary>
		/// Override OnPreSave to update the status if required
		/// </summary>
		/// <returns></returns>
		protected override bool OnPreSave()
		{
			if (m_NewStockOrderStatus != null)
				m_StockOrder.SetStatus(m_NewStockOrderStatus);

			return base.OnPreSave();
		}

		#endregion

		#region Stock Cost handling

		/// <summary>
		/// Stocks the order property changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.PropertyEventArgs"/> instance containing the event data.</param>
		private void StockOrderPropertyChanged(object sender, PropertyEventArgs e)
		{
			if (e.PropertyName == StockOrderPropertyNames.CostOverride)
				SetCostPromptState();
		}

		/// <summary>
		/// Set the Cost prompt to readonly as required
		/// </summary>
		private void SetCostPromptState()
		{
			m_FormStockOrderStatus.TotalCost.ReadOnly = !m_StockOrder.CostOverride;

			if (!m_StockOrder.CostOverride)
				m_StockOrder.CalculateTotalCost();

			m_FormStockOrderStatus.SMGrid1.Columns[StockOrderItemPropertyNames.Cost].ReadOnly = !m_StockOrder.CostOverride;
			m_FormStockOrderStatus.SMGrid1.Columns[StockOrderItemPropertyNames.Cost].IsGrayBackground = !m_StockOrder.CostOverride;
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