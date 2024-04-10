using System;
using System.Collections.Generic;
using System.Drawing;
using Thermo.Framework.Core;
using Thermo.Framework.Utilities;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of STOCK dashboard.
	/// </summary>
	[SampleManagerTask("StockDashTask", "GENERAL")]
	public class StockDashTask : SampleManagerTask
	{
		#region Member Variables

		private const int MoveStockMenuNumber = 15953;
		private const int StockTakeMenuNumber = 15954;

		private FormStockDash m_Form;
		private Stock m_Stock;
		private IEntityCollection m_StockCollection;

		#endregion

		#region Overides

		/// <summary>
		/// Override to catch the additional launch modes.
		/// </summary>
		protected override void SetupTask()
		{
			// Publish the user interface passed in the task parameters.

			m_Form = (FormStockDash) FormFactory.CreateForm(Context.TaskParameters[0], m_Stock);

			m_Form.Loaded += new EventHandler(FormLoaded);
			m_Form.Show();
		}

		/// <summary>
		/// Called when the task parametes and Context object have been refreshed.
		/// </summary>
		/// <remarks>
		/// This is normally caused when the explorer switches tree items but still uses the same task.
		/// </remarks>
		protected override void TaskParametersRefreshed( )
		{
			LoadStockData( );

			LoadChart( );
		}

		#endregion

		#region Handle Move Form

		/// <summary>
		/// Set data dependent prompt browse
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void FormLoaded(object sender, EventArgs e)
		{
			LoadStockData();

			LoadChart();

			LoadChartPopup( );
		}

		/// <summary>
		/// Loads the stock data.
		/// </summary>
		private void LoadStockData() 
		{
			IQuery select = EntityManager.CopyQuery( Context.FolderQuery );

			select.AddOrder( "STOCK_BATCH", true );

			m_StockCollection = EntityManager.Select( TableNames.StockBatch, select );

			if ( m_StockCollection.Count > 0 )
			{
				m_Stock = (Stock)( ( (StockBatch)m_StockCollection[0] ).Stock );

				m_Stock.UseFilteredInventory( m_StockCollection );
			}

			// Setup the icon

			m_Form.StockIcon.SetImageByIconName(new IconName(((IEntity) m_Stock).Icon));

			m_Form.RepublishEntity(m_Stock);

			// Publish the batches explorer

			m_Form.StockBatches.Republish(m_StockCollection);

		}

		/// <summary>
		/// Loads the chart.
		/// </summary>
		private void LoadChart()
		{
			// Clear the current chart
			m_Form.XYChart.Series.Clear( );

			GenericObjectList<XYChartSeriesPointTextual> pointList = new GenericObjectList<XYChartSeriesPointTextual>();

			int pos = 0;
			while (pos < m_StockCollection.Count)
			{
				StockBatch stockBatch = (StockBatch) m_StockCollection[pos];

				pointList.Add(new XYChartSeriesPointTextual(stockBatch.StockBatchId,
				                                            stockBatch.PercentRemaining,
				                                            Color.Empty,
				                                            pos));

				pos++;
			}

			m_Form.XYChart.AddUnboundSeries(m_Form.StringTable.ChartSeriesStock, XYChartType.Bar, true, false, pointList);
		}

		#endregion

		#region Popup menu

		/// <summary>
		/// Loads the chart popup.
		/// </summary>
		private void LoadChartPopup()
		{
			ContextMenuItem menuItem = new ContextMenuItem(m_Form.StringTable.MenuMoveStock, null);
			menuItem.ItemClicked += new ContextMenuItemClickedEventHandler(ChartMoveStockItemClicked);
			m_Form.XYChart.ContextMenu.CustomItems.Add(menuItem);

			menuItem = new ContextMenuItem(m_Form.StringTable.MenuStockTake, null);
			menuItem.ItemClicked += new ContextMenuItemClickedEventHandler(ChartStockTakeItemClicked);
			m_Form.XYChart.ContextMenu.CustomItems.Add(menuItem);

			m_Form.XYChart.ContextMenu.BeforePopup += ChartContextMenuBeforePopup;
		}

		/// <summary>
		/// Chart context menu before popup.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void ChartContextMenuBeforePopup( object sender, ContextMenuBeforePopupEventArgs e )
		{
			XYChartSeriesPoint point = m_Form.XYChart.SelectedPoint;

			foreach (ContextMenuItem menuItem in m_Form.XYChart.ContextMenu.CustomItems)
			{
				menuItem.Visible = (point != null);
			}
		}

		/// <summary>
		/// Chart Move stock item clicked.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ContextMenuItemEventArgs"/> instance containing the event data.</param>
		private void ChartMoveStockItemClicked(object sender, ContextMenuItemEventArgs e)
		{
			XYChartSeriesPoint selectedPoint = m_Form.XYChart.SelectedPoint;

			if (selectedPoint != null)
			{
				if (selectedPoint.Tag != null)
				{
					StockBatch stockBatch = (StockBatch)m_StockCollection[(int)selectedPoint.Tag];
					Library.Task.CreateTask(MoveStockMenuNumber, stockBatch);
				}
			}
		}

		/// <summary>
		/// Chart stock take item clicked.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ContextMenuItemEventArgs"/> instance containing the event data.</param>
		private void ChartStockTakeItemClicked(object sender, ContextMenuItemEventArgs e)
		{
			XYChartSeriesPoint selectedPoint = m_Form.XYChart.SelectedPoint;

			if (selectedPoint != null)
			{
				if (selectedPoint.Tag != null)
				{
					StockBatch stockBatch = (StockBatch)m_StockCollection[(int)selectedPoint.Tag];
					Library.Task.CreateTask(StockTakeMenuNumber, stockBatch);
				}
			}
		}

		#endregion
	}
}