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
	/// Implementation of STOCK collection dashboard.
	/// </summary>
	[SampleManagerTask("StockOverviewDashTask", "GENERAL")]
	public class StockOverviewDashTask : SampleManagerTask
	{
		#region Member Variables

		private FormStockOverviewDash m_Form;
		private IEntityCollection m_StockCollection;

		#endregion

		#region Overrides

		/// <summary>
		/// Override to catch the additional launch modes.
		/// </summary>
		protected override void SetupTask()
		{
			IQuery select = EntityManager.CopyQuery(Context.FolderQuery);

			m_StockCollection = EntityManager.Select(TableNames.Stock, select);

			if (m_StockCollection.Count > 0)
			{
				// Publish the user interface passed in the task parameters.

				m_Form = (FormStockOverviewDash) FormFactory.CreateForm(Context.TaskParameters[0]);

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
		protected override void TaskParametersRefreshed( )
		{
			IQuery select = EntityManager.CopyQuery( Context.FolderQuery );

			m_StockCollection = EntityManager.Select( TableNames.Stock, select );

			// Load the grid
			m_Form.StockBrowse.Republish( m_StockCollection );

			//Load the chart
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
			// Load the grid
			m_Form.StockBrowse.Republish(m_StockCollection);

			//Load the chart
			LoadChart();
		}

		/// <summary>
		/// Loads the chart.
		/// </summary>
		private void LoadChart()
		{
			GenericObjectList<XYChartSeriesPointTextual> pointList = new GenericObjectList<XYChartSeriesPointTextual>();

			foreach (Stock stock in m_StockCollection)
			{
				double percentRemaining = stock.PercentRemaining;

				if (percentRemaining < 0.0)
					percentRemaining = 0.0;
				else if (percentRemaining > 100.0)
					percentRemaining = 100.0;

				pointList.Add(new XYChartSeriesPointTextual(stock.StockName,
				                                            percentRemaining,
				                                            Color.Empty));
			}

			m_Form.XYChart.AddUnboundSeries(m_Form.StringTable.ChartSeriesStock, XYChartType.Bar, true, false, pointList);
		}

		#endregion
	}
}