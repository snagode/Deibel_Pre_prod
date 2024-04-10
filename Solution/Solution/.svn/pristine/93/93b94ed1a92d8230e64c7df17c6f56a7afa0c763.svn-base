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
	[SampleManagerTask("LocationExplorerTask", "GENERAL")]
	public class LocationExplorerTask : SampleManagerTask
	{
		#region Member Variables

		private FormLocationSampleExplorer m_FormLocationSampleExplorer;
		private FormLocationInstrumentExplorer m_FormLocationInstrumentExplorer;
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

				if ( Context.TaskParameters[0] == "LocationSampleExplorer" )
				{
					CreateSampleForm( );
				}
				else
				{
					CreateInstrumentForm();
				}
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
		/// Creates the instrument form.
		/// </summary>
		private void CreateInstrumentForm() 
		{
			m_FormLocationInstrumentExplorer = (FormLocationInstrumentExplorer)
			                                   FormFactory.CreateForm("LocationInstrumentExplorer", m_ParentLocation);
			m_FormLocationInstrumentExplorer.Created += FormLocationInstrumentExplorerCreated;
			m_FormLocationInstrumentExplorer.Loaded += FormLocationInstrumentExplorerLoaded;
			m_FormLocationInstrumentExplorer.Show();
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

				if ( m_FormLocationInstrumentExplorer != null )
				{
					m_FormLocationInstrumentExplorer.RepublishEntity(m_ParentLocation);
					m_FormLocationInstrumentExplorer.LocationPictureBox.SetImageByIconName( new IconName( m_ParentLocation.Icon.Identity ) );
				}
				else
				{
					m_FormLocationSampleExplorer.RepublishEntity(m_ParentLocation);
					m_FormLocationSampleExplorer.instrumentPictureBox.SetImageByIconName( new IconName( m_ParentLocation.Icon.Identity ) );
						
					IQuery query = GetSamplesByLocationAndIntervalQuery( InitializeSampleByLocationQuery( ) );
					LoadGrid( query );
					LoadChart( query );
				}
			}
		}

		#endregion

		#region LocationInstrumentExplorer

		/// <summary>
		/// Handles the Loaded event of the m_FormLocationInstrumentExplorer control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void FormLocationInstrumentExplorerLoaded(object sender, EventArgs e)
		{
			m_FormLocationInstrumentExplorer.LocationPictureBox.SetImageByIconName(new IconName(m_ParentLocation.Icon.Identity));
		}

		/// <summary>
		/// Handles the Created event of the m_FormLocationInstrumentExplorer control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void FormLocationInstrumentExplorerCreated(object sender, EventArgs e)
		{
			m_FormLocationInstrumentExplorer.InstrumentExplorerGrid.SelectionChanged +=
				InstrumentExplorerGrid_SelectionChanged;

			IQuery resultsByInstrument = EntityManager.CreateQuery(TableNames.SampTestResult);
			resultsByInstrument.AddEquals(SampTestResultPropertyNames.IdNumeric, 0);
			m_FormLocationInstrumentExplorer.TestInstrumentBrowse.Republish(resultsByInstrument);
		}

		/// <summary>
		/// Handles the SelectionChanged event of the InstrumentExplorerGrid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ExplorerGridSelectionChangedEventArgs"/> instance containing the event data.</param>
		private void InstrumentExplorerGrid_SelectionChanged(object sender, ExplorerGridSelectionChangedEventArgs e)
		{
			IEntityCollection instruments = e.Selection;
			if (instruments.Count > 0)
			{
				IQuery query = EntityManager.CreateQuery(TableNames.Test);
				bool first = true;
				foreach (Instrument instrument in instruments)
				{
					if (!first)
					{
						query.AddOr();
					}
					else
					{
						first = false;
					}
					query.AddEquals(TestPropertyNames.Instrument, instrument);
				}
				m_FormLocationInstrumentExplorer.TestInstrumentBrowse.Republish(query);
			}
			else
			{
				IQuery emptyQuery = EntityManager.CreateQuery(TableNames.Test);
				emptyQuery.AddEquals(TestPropertyNames.TestNumber, 0);
				m_FormLocationInstrumentExplorer.TestInstrumentBrowse.Republish(emptyQuery);
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