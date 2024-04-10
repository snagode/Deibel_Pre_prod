using System;
using Thermo.Framework.Core;
using Thermo.Framework.Utilities;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.TaskLibrary;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of STOCK BATCH.
	/// </summary>
	[SampleManagerTask("StockBatchDashTask", "GENERAL")]
	public class StockBatchDashTask : SampleManagerTask
	{
		#region Member Variables

		private FormStockBatchDash m_Form;
		private StockBatch m_StockBatch;

		#endregion

		#region Overides

		/// <summary>
		/// Override to catch the additional launch modes.
		/// </summary>
		protected override void SetupTask()
		{
			IQuery select = EntityManager.CopyQuery(Context.FolderQuery);

			IEntityCollection stockBatchCollection = EntityManager.Select(TableNames.StockBatch, select);

			if (stockBatchCollection.Count == 1)
			{
				m_StockBatch = (StockBatch)stockBatchCollection[0];

				// Publish the user interface passed in the task parameters.
				m_Form = (FormStockBatchDash)FormFactory.CreateForm(Context.TaskParameters[0], m_StockBatch);

				m_Form.Loaded += new EventHandler(FormLoaded);
				m_Form.Show();
			}
		}
		
		/// <summary>
		/// Called when the task parametes and Context object have been refreshed.
		/// </summary>
		/// <remarks>
		/// This is normally caused when the explorer switches tree items but still uses the same task.
		/// </remarks>
		protected override void TaskParametersRefreshed()
		{
			IQuery select = EntityManager.CopyQuery(Context.FolderQuery);

			IEntityCollection stockBatchCollection = EntityManager.Select(TableNames.StockBatch, select);

			if (stockBatchCollection.Count == 1)
			{
				m_StockBatch = (StockBatch)stockBatchCollection[0];
				m_Form.RepublishEntity(m_StockBatch);
			}

			m_Form.StockInventoryBrowse.Republish(m_StockBatch.StockInventories);
			m_Form.StockBatchIcon.SetImageByIconName(new IconName(((IEntity)m_StockBatch).Icon));

			LoadChart();
		}

		#endregion

		#region Handle Move Form

		/// <summary>
		/// Set data dependent prompt browse
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void FormLoaded(object sender, EventArgs e)
		{
			m_Form.StockInventoryBrowse.Republish(m_StockBatch.StockInventories);
			m_Form.StockBatchIcon.SetImageByIconName(new IconName(((IEntity)m_StockBatch).Icon));

			LoadChart();
		}

		/// <summary>
		/// Loads the chart.
		/// </summary>
		private void LoadChart()
		{
			m_Form.XYChart.Series.Clear(); 
			string inventoryUnit = m_StockBatch.Stock.InventoryUnit;

			if (string.IsNullOrEmpty(inventoryUnit))
			{
				inventoryUnit = m_StockBatch.Unit;
			}

			double runningTotal = Library.Utils.UnitConvert(m_StockBatch.InitialAmount,
															m_StockBatch.Unit,
															inventoryUnit);

			GenericObjectList<XYChartSeriesPointDateTime> pointList = new GenericObjectList<XYChartSeriesPointDateTime>();

			pointList.Add(new XYChartSeriesPointDateTime(m_StockBatch.DateCreated, runningTotal));

			try
			{
				foreach (StockInventory stockInventory in m_StockBatch.StockInventories)
				{
					switch (stockInventory.UseType.PhraseId)
					{
						case PhraseStockUse.PhraseIdMOVEIN:
						case PhraseStockUse.PhraseIdORDER:
							runningTotal += Library.Utils.UnitConvert(stockInventory.Amount,
																	  stockInventory.Unit,
																	  inventoryUnit);
							break;
						case PhraseStockUse.PhraseIdCONSUME:
						case PhraseStockUse.PhraseIdTEST:
							if (stockInventory.ConsumedFlag)
								runningTotal -= Library.Utils.UnitConvert(stockInventory.Amount,
																		  stockInventory.Unit,
																		  inventoryUnit);
							break;
						case PhraseStockUse.PhraseIdMOVEOUT:
							runningTotal -= Library.Utils.UnitConvert(stockInventory.Amount,
																	  stockInventory.Unit,
																	  inventoryUnit);
							break;
						case PhraseStockUse.PhraseIdSTOCKTAKE:
							runningTotal = Library.Utils.UnitConvert(stockInventory.Amount,
																	 stockInventory.Unit,
																	 inventoryUnit);
							break;
					}

					if (runningTotal < 0)
						runningTotal = 0;

					pointList.Add(new XYChartSeriesPointDateTime(stockInventory.DateCreated, runningTotal));
				}
			}
			catch (Exception)
			{
				pointList.Add(new XYChartSeriesPointDateTime(m_StockBatch.DateCreated, runningTotal));
			}
			m_Form.XYChart.AddUnboundSeries(m_Form.StringTable.ChartSeriesStock, XYChartType.Line, true, false, pointList);
		}

		#endregion
	}
}