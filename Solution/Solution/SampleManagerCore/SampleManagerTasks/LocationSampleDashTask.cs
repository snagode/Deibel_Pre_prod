using System;
using System.Collections;
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
	/// Location server side task.
	/// </summary>
	[SampleManagerTask( "LocationSampleDashTask", "GENERAL" )]
	public class LocationSampleDashTask : SampleManagerTask
	{
		#region Member Variables

		private FormLocationSampleExplorer m_FormLocationSampleExplorer;
		private Location m_ParentLocation;

		#endregion

		#region Setup Task

		/// <summary>
		/// Override to catch the additional launch modes.
		/// </summary>
		protected override void SetupTask()
		{
			if (Context.SelectedItems.Count == 1 && Context.SelectedItems[0] is Location)
			{
				m_ParentLocation = (Location) Context.SelectedItems[0];

				CreateSampleForm( );
			}
		}

		/// <summary>
		/// Creates the sample form.
		/// </summary>
		private void CreateSampleForm() 
		{
			m_FormLocationSampleExplorer = (FormLocationSampleExplorer)
				FormFactory.CreateForm( "LocationSampleExplorer", m_ParentLocation );
			m_FormLocationSampleExplorer.Loaded += FormLocationSampleExplorerLoaded;
			m_FormLocationSampleExplorer.Created += FormLocationSampleExplorerCreated;
			m_FormLocationSampleExplorer.refreshButtonEdit.Click += RefreshButtonEditClick;
			m_FormLocationSampleExplorer.Show();
		}

		/// <summary>
		/// Called when the task parametes and Context object have been refreshed.
		/// </summary>
		/// <remarks>
		/// This is normally caused when the explorer switches tree items but still uses the same task.
		/// </remarks>
		protected override void TaskParametersRefreshed( )
		{
			if ( Context.SelectedItems.Count == 1 && Context.SelectedItems[0] is Location )
			{
				m_ParentLocation = (Location)Context.SelectedItems[0];

				m_FormLocationSampleExplorer.RepublishEntity(m_ParentLocation);
				m_FormLocationSampleExplorer.instrumentPictureBox.SetImageByIconName( new IconName( m_ParentLocation.Icon.Identity ) );
					
				IQuery query = GetSamplesByLocationAndIntervalQuery( InitializeSampleByLocationQuery( ) );
				LoadGrid( query );
				LoadChart( query );
			}
		}

		#endregion

		#region LocationSampleExplorer

		/// <summary>
		/// Handles the Created event of the m_FormLocationSampleExplorer control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void FormLocationSampleExplorerCreated(object sender, EventArgs e)
		{
			m_FormLocationSampleExplorer.ThroughputIntervalEdit.Interval = new TimeSpan(-7, 0, 0, 0);
		}

		/// <summary>
		/// Handles the Loaded event of the m_FormLocationSampleExplorer control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void FormLocationSampleExplorerLoaded(object sender, EventArgs e)
		{
			m_FormLocationSampleExplorer.instrumentPictureBox.SetImageByIconName(new IconName(m_ParentLocation.Icon.Identity));
			IQuery query = GetSamplesByLocationAndIntervalQuery(InitializeSampleByLocationQuery());
			LoadGrid(query);
			LoadChart(query);
		}

		/// <summary>
		/// Handles the Click event of the refreshButtonEdit control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void RefreshButtonEditClick(object sender, EventArgs e)
		{
			IQuery query = GetSamplesByLocationAndIntervalQuery(InitializeSampleByLocationQuery());
			LoadGrid(query);
			LoadChart(query);
		}

		#endregion

		#region Location

		/// <summary>
		/// Gets the base samples by location.
		/// </summary>
		/// <returns></returns>
		private IQuery InitializeSampleByLocationQuery()
		{
			IEntityCollection childLocations = m_ParentLocation.ChildLocations;
			IQuery samplesByLocationQuery = EntityManager.CreateQuery(TableNames.Sample);
			samplesByLocationQuery.PushBracket();
			samplesByLocationQuery.AddEquals(SamplePropertyNames.LocationId, m_ParentLocation);
			foreach (Location location in childLocations)
			{
				samplesByLocationQuery.AddOr();
				samplesByLocationQuery.AddEquals(SamplePropertyNames.LocationId, location);
			}
			samplesByLocationQuery.PopBracket();
			return samplesByLocationQuery;
		}

		#endregion

		#region Load Data

		/// <summary>
		/// Gets the samples by location and interval query.
		/// </summary>
		/// <param name="baseQuery">The base query.</param>
		/// <returns></returns>
		private IQuery GetSamplesByLocationAndIntervalQuery(IQuery baseQuery)
		{
			IQuery withInterval = baseQuery;
			withInterval.AddAnd();
			withInterval.PushBracket();
			withInterval.AddGreaterThanOrEquals(SamplePropertyNames.LoginDate,
			                                    m_FormLocationSampleExplorer.ThroughputIntervalEdit.Interval);
			withInterval.PopBracket();
			return withInterval;
		}

		/// <summary>
		/// Loads the grid.
		/// </summary>
		private void LoadGrid(IQuery query)
		{
			m_FormLocationSampleExplorer.SampleEntityBrowse.Republish(query);
		}

		/// <summary>
		/// Loads the chart.
		/// </summary>
		private void LoadChart(IQuery query)
		{
			GenericObjectList<XYChartSeriesPointTextual> sampleList = new GenericObjectList<XYChartSeriesPointTextual>();

			IEntityCollection throughputSampleCollection = EntityManager.Select(TableNames.Sample, query);
			Hashtable totals = new Hashtable();
			Hashtable canceled = new Hashtable();

			foreach (Sample sample in throughputSampleCollection)
			{
				if (totals.ContainsKey(sample.LocationId.Name))
				{
					int total = (int) totals[sample.LocationId.Name];
					totals[sample.LocationId.Name] = total + 1;
				}
				else
				{
					totals.Add(sample.LocationId.Name, 1);
				}
				if (sample.Status.PhraseText == "Cancelled")
				{
					string canceledString = string.Format("{0} ({1})", sample.LocationId.Name, "Cancelled");
					if (canceled.ContainsKey(canceledString))
					{
						int canceledTotal = (int) canceled[canceledString];
						canceled[canceledString] = canceledTotal + 1;
					}
					else
					{
						canceled.Add(canceledString, 1);
					}
				}
			}

			m_FormLocationSampleExplorer.XYChart1.Series.Clear();

			foreach (DictionaryEntry total in totals)
			{
				sampleList.Add( new XYChartSeriesPointTextual( (string)total.Key, Convert.ToDouble( total.Value ) ) );
				string canceledString = string.Format("{0} ({1})", total.Key, "Cancelled");
				if (canceled.Contains(canceledString))
				{
					sampleList.Add( new XYChartSeriesPointTextual( canceledString, Convert.ToDouble( canceled[canceledString] ) ) );
				}
			}

			m_FormLocationSampleExplorer.XYChart1.AddUnboundSeries( m_FormLocationSampleExplorer.StringTable.ChartSeriesSample,
																	   XYChartType.Bar, true, false, sampleList );
		}

		#endregion
	}
}