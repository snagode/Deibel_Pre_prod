using System;
using System.Drawing;
using System.Globalization;
using System.Text;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Lot Inventory Dashboard Task
	/// </summary>
	[SampleManagerTask("LotInventoryDashboardTask", "GENERAL", "LOT_DETAILS")]
	public class LotInventoryDashboardTask : DefaultSingleEntityTask
	{
		#region Member variables

		/// <summary>
		/// The m_ form
		/// </summary>
		private FormLotInventoryDashboard m_Form;
		private LotDetails m_LotDetails;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="P:Thermo.SampleManager.Tasks.DefaultFormTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			m_Form = (FormLotInventoryDashboard) MainForm;
			m_LotDetails = (LotDetails) MainForm.Entity;

			m_Form.XYChartUsage.Series.Clear();
			PopulateXYBy();
			PopulateXYRemaining();
			PopulatePieLotConsumption();
			PopulatePieLotQuantity();
			base.MainFormLoaded();
		}

		/// <summary>
		/// Populates the pie lot quantity.
		/// </summary>
		private void PopulatePieLotQuantity()
		{
			PieChartSeriesTextual series = new PieChartSeriesTextual("Lot Reservations", PieChartType.Pie);

			PieChartSeriesPointTextual pointTotalQuantityAvailable =
				new PieChartSeriesPointTextual(Library.Message.GetMessage("WorkflowMessages", "PointTotalQuantityAvailable"),
					m_LotDetails.QuantityAvailable);

			series.Points.Add(pointTotalQuantityAvailable);

			PieChartSeriesPointTextual pointTotalReservedRemaining =
				new PieChartSeriesPointTextual(Library.Message.GetMessage("WorkflowMessages", "PointTotalReservedRemaining"),
					m_LotDetails.QuantityReserved);
			series.Points.Add(pointTotalReservedRemaining);

			PieChartSeriesPointTextual pointTotalConsumptionOfReserved =
				new PieChartSeriesPointTextual(Library.Message.GetMessage("WorkflowMessages", "PointTotalConsumptionOfReserved"),
					m_LotDetails.TotalConsumptionOfReserved);
			series.Points.Add(pointTotalConsumptionOfReserved);

			PieChartSeriesPointTextual pointTotalReconciledAndUsed =
				new PieChartSeriesPointTextual(Library.Message.GetMessage("WorkflowMessages", "PointTotalReconciledAndUsed"),
					m_LotDetails.QuantityReconciledAndUsed);
			series.Points.Add(pointTotalReconciledAndUsed);

			m_Form.PieChartRemaining.AddUnboundSeries(series);
		}

		/// <summary>
		/// Populates the XY chart.
		/// </summary>
		private void PopulateXYRemaining()
		{
			XYChartSeriesDateTime series = new XYChartSeriesDateTime("Running Total");

			XYChartSeriesPointDateTime previousTimePoint = null;

			foreach (LotInventory inventory in m_LotDetails.LotInventories)
			{
				XYChartSeriesPointDateTime point = new XYChartSeriesPointDateTime(inventory.DateCreated, inventory.QuantityRunningTotal);
				if (inventory.Usage.PhraseId == PhraseLotUsage.PhraseIdMODIFIED)
				{
					point.AddCircleAnnotation(Color.Gray);
					point.Tooltip = PhraseLotUsage.PhraseIdMODIFIED + " " + inventory.QuantityAdjusted;
				}

				// Only occues when timepoints happen on the same date, only add the last one of the day
				if (previousTimePoint != null && previousTimePoint.X.Date == point.X.Date)
				{
					series.Points.Remove(previousTimePoint);
				}
				series.Points.Add(point);
				previousTimePoint = point;
			}

			m_Form.XYChartUsage.Series.Add(series);
		}

		/// <summary>
		/// Populates the XY chart.
		/// </summary>
		private void PopulateXYBy()
		{
			m_Form.XYChartUsage.AxisYTitle = String.Format(Library.Message.GetMessage("WorkflowMessages", "XYUsageYTitle"), m_LotDetails.Units);
			XYChartSeriesDateTime seriesConsumedBy = new XYChartSeriesDateTime("Usages", XYChartType.Bar, true, true);

			IQuery query = EntityManager.CreateQuery(LotInventoryBase.EntityName);
			query.AddEquals(LotInventoryPropertyNames.LotId, m_LotDetails.LotId);
			query.AddAnd();
			query.AddNotEquals(LotInventoryPropertyNames.ToLot, String.Empty);
			query.AddOrder(LotInventoryPropertyNames.DateCreated, true);
			IEntityCollection log = EntityManager.Select(query);

			LotInventory prevLotInv = null;
			double total = 0;
			StringBuilder sb = new StringBuilder();

			foreach (LotInventory inventory in log)
			{
				sb.AppendLine(inventory.ToLot.LotDetailsName + ":" + inventory.Quantity);

				//new date time, register prev value (skip first run)
				if (prevLotInv != null && prevLotInv.DateCreated.ToDateTime(CultureInfo.CurrentCulture).Date != inventory.DateCreated.ToDateTime(CultureInfo.CurrentCulture).Date)
				{
					XYChartSeriesPointDateTime point;
					if (total < 0)
					{
						point = new XYChartSeriesPointDateTime(prevLotInv.DateCreated.ToDateTime(CultureInfo.CurrentCulture).Date, total*-1, Color.Red);
					}
					else
					{
						point = new XYChartSeriesPointDateTime(prevLotInv.DateCreated.ToDateTime(CultureInfo.CurrentCulture).Date, total);
					}
					point.Tooltip = sb.ToString();
					seriesConsumedBy.Points.Add(point);
					total = 0;
					sb = new StringBuilder();
				}

				total += inventory.Quantity;
				prevLotInv = inventory;
			}
			if (prevLotInv != null) //incase no inv
			{
				XYChartSeriesPointDateTime lastPoint;
				if (total < 0)
				{
					lastPoint = new XYChartSeriesPointDateTime(prevLotInv.DateCreated.ToDateTime(CultureInfo.CurrentCulture).Date, total*-1, Color.Red);
				}
				else
				{
					lastPoint = new XYChartSeriesPointDateTime(prevLotInv.DateCreated.ToDateTime(CultureInfo.CurrentCulture).Date, total);
				}
				lastPoint.Tooltip = prevLotInv.ToLot.LotId;
				lastPoint.Tooltip = prevLotInv.ToLot.LotDetailsName;
				seriesConsumedBy.Points.Add(lastPoint);
			}
			m_Form.XYChartUsage.Series.Add(seriesConsumedBy);
		}

		/// <summary>
		/// Populates the pie chart.
		/// </summary>
		private void PopulatePieLotConsumption()
		{
			PieChartSeriesTextual series = new PieChartSeriesTextual("Consumed Quantities", PieChartType.Pie);

			foreach (LotRelation childLot in m_LotDetails.ChildLots)
			{
				double consumed = childLot.QuantityConsumed;
				PieChartSeriesPointTextual point = new PieChartSeriesPointTextual(childLot.ToLot.LotId, consumed);
				series.Points.Add(point);
			}

			PieChartSeriesPointTextual pointRemaining = new PieChartSeriesPointTextual("Remaining", m_LotDetails.QuantityRemaining);
			series.Points.Add(pointRemaining);

			m_Form.PieChartUsage.AddUnboundSeries(series);
		}

		#endregion
	}
}