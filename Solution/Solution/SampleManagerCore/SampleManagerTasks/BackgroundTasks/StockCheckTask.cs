using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Thermo.SampleManager.Common.CommandLine;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks.BackgroundTasks
{
	/// <summary>
	/// Background task which updates the state of stock batches
	/// </summary>
	[SampleManagerTask("StockCheck")]
	public class StockCheckTask : SampleManagerTask, IBackgroundTask
	{
		#region Member Variables

		private readonly List<StockBatch> m_ExpiredStockBatch = new List<StockBatch>();
		private readonly List<Stock> m_LowStock = new List<Stock>();
		private readonly List<StockBatch> m_LowStockBatch = new List<StockBatch>();
		private readonly List<StockBatch> m_WarnExpiryStockBatch = new List<StockBatch>();

		#endregion

		#region IBackgroundTask Members

		/// <summary>
		/// Launches this background task.
		/// </summary>
		public void Launch()
		{
			Logger.Debug("Starting Stock Check Task...");

			// Select all Stock Batches

			IQuery stockBatchQuery = EntityManager.CreateQuery(TableNames.StockBatch);

			stockBatchQuery.AddEquals("STATUS", PhraseStkBStat.PhraseIdV);
			stockBatchQuery.AddOrder("STOCK", true);
			stockBatchQuery.AddOrder("STOCK_BATCH", true);

			IEntityCollection stockBatchCollection = EntityManager.Select(TableNames.StockBatch, stockBatchQuery);

			foreach (StockBatch stockBatch in stockBatchCollection)
			{
				if (stockBatch.Lock())
				{
					CheckBatch(stockBatch);

					stockBatch.CheckStatus();

					if (stockBatch.IsModified())
						EntityManager.Transaction.Add(stockBatch);
				}
			}
			EntityManager.Commit();
			stockBatchCollection.ReleaseAll();

			// Select all Stocks

			IEntityCollection stockCollection = EntityManager.Select(TableNames.Stock);

			foreach (Stock stock in stockCollection)
			{
				CheckInventory(stock);
			}

			// Send an email to the resposible operator

			MailResponsibleOperators();
		}

		#endregion

		#region Checks

		/// <summary>
		/// Checks the batch.
		/// </summary>
		/// <param name="stockBatch">The stock batch.</param>
		private void CheckBatch(StockBatch stockBatch)
		{
			if (MailableUser(stockBatch.Stock.OperatorId))
			{
				if (stockBatch.CurrentQuantity <= 0)
				{
					m_LowStockBatch.Add(stockBatch);
				}
				else if (!stockBatch.ExpiryDate.IsNull)
				{
					DateTime expiry = stockBatch.ExpiryDate.ToDateTime(CultureInfo.CurrentCulture);
					DateTime expiryWarning = expiry.Subtract(stockBatch.Stock.ShelfLife);

					if (expiry <= (DateTime) Library.Environment.ClientNow)
					{
						m_ExpiredStockBatch.Add(stockBatch);
					}
					else if ((stockBatch.Stock.ReorderOnShelfLife) && (expiryWarning <= (DateTime) Library.Environment.ClientNow))
					{
						m_WarnExpiryStockBatch.Add(stockBatch);
					}
				}
			}
		}

		/// <summary>
		/// Checks the inventory.
		/// </summary>
		/// <param name="stock">The stock.</param>
		private void CheckInventory(Stock stock)
		{
			if (MailableUser(stock.OperatorId))
			{
				if (stock.ReorderOnAmount)
				{
					double currentQuantity = stock.CurrentQuantity;

					if (!string.IsNullOrEmpty(stock.ReorderUnit))
						currentQuantity = Library.Utils.UnitConvert(stock.CurrentQuantity,
																	stock.InventoryUnit,
																	stock.ReorderUnit);

					if (currentQuantity <= stock.ReorderAmount)
					{
						m_LowStock.Add(stock);
						return;
					}
				}

				if (stock.ReorderOnPercentage)
				{
					if (stock.PercentRemaining <= stock.ReorderPercentage)
						m_LowStock.Add(stock);
				}
			}
		}

		#endregion

		#region Mail

		/// <summary>
		/// Mailable user.
		/// </summary>
		/// <param name="oper">The oper.</param>
		/// <returns></returns>
		private static bool MailableUser(PersonnelBase oper)
		{
			if (oper.IsNull()) return false;
			Personnel person = (Personnel)oper;
			return person.IsMailable;
		}

		/// <summary>
		/// Mails the responsible operators.
		/// </summary>
		private void MailResponsibleOperators()
		{
			while (m_LowStock.Count > 0)
			{
				MailToUser((Personnel)m_LowStock[0].OperatorId);
			}

			while (m_LowStockBatch.Count > 0)
			{
				MailToUser((Personnel)m_LowStockBatch[0].Stock.OperatorId);
			}

			while (m_WarnExpiryStockBatch.Count > 0)
			{
				MailToUser((Personnel)m_WarnExpiryStockBatch[0].Stock.OperatorId);
			}

			while (m_ExpiredStockBatch.Count > 0)
			{
				MailToUser((Personnel)m_ExpiredStockBatch[0].Stock.OperatorId);
			}
		}

		/// <summary>
		/// Mails to user.
		/// </summary>
		/// <param name="oper">The oper.</param>
		private void MailToUser(Personnel oper)
		{
			StringBuilder mailBody = new StringBuilder();

			mailBody.Append(Library.Message.GetMessage("LaboratoryMessages", "StockCheckIntro"));

			AddLowStock(oper, mailBody);
			AddLowStockBatch(oper, mailBody);
			AddWarnExpiryStockBatch(oper, mailBody);
			AddExpiredStockBatch(oper, mailBody);

			try
			{
				string subject = Library.Message.GetMessage("LaboratoryMessages", "StockCheckSubject");
				oper.Mail(subject, mailBody.ToString());
				Logger.DebugFormat("Mail sent to {0}", oper.Email);
			}
			catch (Exception e)
			{
				Logger.DebugFormat("Error Sending mail {0} - {1}", e.Message, e.InnerException.Message);
			}
		}

		/// <summary>
		/// Adds the low stock.
		/// </summary>
		/// <param name="oper">The oper.</param>
		/// <param name="mailBody">The mail body.</param>
		private void AddLowStock(PersonnelBase oper, StringBuilder mailBody)
		{
			bool first = true;
			int idLength = 0;
			List<Stock> tempStock = new List<Stock>();

			for (int i = m_LowStock.Count - 1; i >= 0; i--)
			{
				if (m_LowStock[i].OperatorId == oper)
				{
					idLength = m_LowStock[i].Identity.Length > idLength ? m_LowStock[i].Identity.Length : idLength;
					tempStock.Insert(0, m_LowStock[i]);
					m_LowStock.RemoveAt(i);
				}
			}

			foreach (Stock stock in tempStock)
			{
				// Header if required
				if (first)
				{
					mailBody.Append(Library.Message.GetMessage("LaboratoryMessages", "StockCheckReorder"));
					first = false;
				}

				// Each stock
				mailBody.Append("\t");
				mailBody.Append(stock.Identity.PadRight(idLength));
				mailBody.Append("\t");
				mailBody.Append(stock.Description);
				mailBody.Append("\n");
			}
		}

		/// <summary>
		/// Adds the low stock batch.
		/// </summary>
		/// <param name="oper">The oper.</param>
		/// <param name="mailBody">The mail body.</param>
		private void AddLowStockBatch(PersonnelBase oper, StringBuilder mailBody)
		{
			bool first = true;
			int idLength = 0;
			List<StockBatch> tempStockBatch = new List<StockBatch>();

			for (int i = m_LowStockBatch.Count - 1; i >= 0; i--)
			{
				if (m_LowStockBatch[i].Stock.OperatorId == oper)
				{
					idLength = m_LowStockBatch[i].StockBatchId.Length > idLength ? m_LowStockBatch[i].StockBatchId.Length : idLength;
					tempStockBatch.Insert(0, m_LowStockBatch[i]);
					m_LowStockBatch.RemoveAt(i);
				}
			}

			foreach (StockBatch stockBatch in tempStockBatch)
			{
				// Header if required
				if (first)
				{
					mailBody.Append(Library.Message.GetMessage("LaboratoryMessages", "StockCheckEmpty"));
					first = false;
				}

				// Each stock batch
				mailBody.Append("\t");
				mailBody.Append(stockBatch.StockBatchId.PadRight(idLength));
				mailBody.Append("\t");
				mailBody.Append(stockBatch.Description);
				mailBody.Append("\n");
			}
		}

		/// <summary>
		/// Adds the warn expiry stock batch.
		/// </summary>
		/// <param name="oper">The oper.</param>
		/// <param name="mailBody">The mail body.</param>
		private void AddWarnExpiryStockBatch(PersonnelBase oper, StringBuilder mailBody)
		{
			bool first = true;
			int idLength = 0;
			List<StockBatch> tempStockBatch = new List<StockBatch>();

			for (int i = m_WarnExpiryStockBatch.Count - 1; i >= 0; i--)
			{
				if (m_WarnExpiryStockBatch[i].Stock.OperatorId == oper)
				{
					idLength = m_WarnExpiryStockBatch[i].StockBatchId.Length > idLength
								? m_WarnExpiryStockBatch[i].StockBatchId.Length
								: idLength;

					tempStockBatch.Insert(0, m_WarnExpiryStockBatch[i]);
					m_WarnExpiryStockBatch.RemoveAt(i);
				}
			}

			foreach (StockBatch stockBatch in tempStockBatch)
			{
				// Header if required
				if (first)
				{
					mailBody.Append(Library.Message.GetMessage("LaboratoryMessages", "StockCheckExpire"));
					first = false;
				}

				// Each stock batch
				mailBody.Append("\t");
				mailBody.Append(stockBatch.StockBatchId.PadRight(idLength));
				mailBody.Append("\t");
				mailBody.Append(stockBatch.Description);
				mailBody.Append("\n");
			}
		}

		/// <summary>
		/// Adds the expired stock batch.
		/// </summary>
		/// <param name="oper">The oper.</param>
		/// <param name="mailBody">The mail body.</param>
		private void AddExpiredStockBatch(PersonnelBase oper, StringBuilder mailBody)
		{
			bool first = true;
			int idLength = 0;
			List<StockBatch> tempStockBatch = new List<StockBatch>();

			for (int i = m_ExpiredStockBatch.Count - 1; i >= 0; i--)
			{
				if (m_ExpiredStockBatch[i].Stock.OperatorId == oper)
				{
					idLength = m_ExpiredStockBatch[i].StockBatchId.Length > idLength
								? m_ExpiredStockBatch[i].StockBatchId.Length
								: idLength;

					tempStockBatch.Insert(0, m_ExpiredStockBatch[i]);
					m_ExpiredStockBatch.RemoveAt(i);
				}
			}

			foreach (StockBatch stockBatch in tempStockBatch)
			{
				// Header if required
				if (first)
				{
					mailBody.Append(Library.Message.GetMessage("LaboratoryMessages", "StockCheckExpired"));
					first = false;
				}

				// Each stock batch
				mailBody.Append("\t");
				mailBody.Append(stockBatch.StockBatchId.PadRight(idLength));
				mailBody.Append("\t");
				mailBody.Append(stockBatch.Description);
				mailBody.Append("\n");
			}
		}

		#endregion
	}
}