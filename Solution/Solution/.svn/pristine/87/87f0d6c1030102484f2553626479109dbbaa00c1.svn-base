using System;
using System.Drawing;
using Thermo.Framework.Utilities;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of STOCK BATCH collection dashboard.
	/// </summary>
	[SampleManagerTask("StockBatchOverviewDashTask", "GENERAL")]
	public class StockBatchOverviewDashTask : SampleManagerTask
	{
		#region Member Variables

		private FormStockBatchOverviewDash m_Form;
		private IEntityCollection m_StockBatchCollection;

		#endregion

		#region Overides

		/// <summary>
		/// Override to catch the additional launch modes.
		/// </summary>
		protected override void SetupTask()
		{
			m_Form = (FormStockBatchOverviewDash)FormFactory.CreateForm( Context.TaskParameters[0] );

			m_Form.Loaded += new EventHandler( FormLoaded );
			m_Form.Show( );
		}

		/// <summary>
		/// Loads the stock batch data.
		/// </summary>
		private void LoadStockBatchData() 
		{
			IQuery select = EntityManager.CopyQuery( Context.FolderQuery );

			select.AddOrder("STOCK", true);
			select.AddOrder("STOCK_BATCH", true);

			m_StockBatchCollection = EntityManager.Select(TableNames.StockBatch, select);

			// Load the grid
			m_Form.StockBatchBrowse.Republish( m_StockBatchCollection );
		}

		/// <summary>
		/// Called when the task parametes and Context object have been refreshed.
		/// </summary>
		/// <remarks>
		/// This is normally caused when the explorer switches tree items but still uses the same task.
		/// </remarks>
		protected override void TaskParametersRefreshed( )
		{
			LoadStockBatchData( );
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
			LoadStockBatchData( );
			LoadChart( );
		}

		private void LoadChart()
		{
			m_Form.XYChart.Series.Clear( );

			GenericObjectList<XYChartSeriesPointTextual> pointList = new GenericObjectList<XYChartSeriesPointTextual>();

			foreach (StockBatch stockBatch in m_StockBatchCollection)
			{
				double percentRemaining = stockBatch.PercentRemaining;

				if (percentRemaining < 0.0)
					percentRemaining = 0.0;
				else if (percentRemaining > 100.0)
					percentRemaining = 100.0;

				pointList.Add(new XYChartSeriesPointTextual(stockBatch.StockBatchId,
				                                            percentRemaining,
				                                            Color.Empty));
			}

			m_Form.XYChart.AddUnboundSeries(m_Form.StringTable.StockSeriesName, XYChartType.Bar, true, false, pointList);
		}

		#endregion
	}
}