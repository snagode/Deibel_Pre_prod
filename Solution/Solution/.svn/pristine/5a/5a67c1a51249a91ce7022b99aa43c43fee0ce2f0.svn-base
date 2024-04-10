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
	[SampleManagerTask( "LocationInstrumentDashTask", "GENERAL" )]
	public class LocationInstrumentDashTask : SampleManagerTask
	{
		#region Member Variables

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

				m_FormLocationInstrumentExplorer = (FormLocationInstrumentExplorer)
				                                   FormFactory.CreateForm("LocationInstrumentExplorer", m_ParentLocation);
				m_FormLocationInstrumentExplorer.Created += FormLocationInstrumentExplorerCreated;
				m_FormLocationInstrumentExplorer.Loaded += FormLocationInstrumentExplorerLoaded;
				m_FormLocationInstrumentExplorer.Show();
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
			if ( Context.SelectedItems.Count == 1 && Context.SelectedItems[0] is Location )
			{
				m_ParentLocation = (Location)Context.SelectedItems[0];

				m_FormLocationInstrumentExplorer.RepublishEntity(m_ParentLocation);
				m_FormLocationInstrumentExplorer.LocationPictureBox.SetImageByIconName( new IconName( m_ParentLocation.Icon.Identity ) );

				IQuery resultsByInstrument = EntityManager.CreateQuery( TableNames.SampTestResult );
				resultsByInstrument.AddEquals( SampTestResultPropertyNames.IdNumeric, 0 );
				m_FormLocationInstrumentExplorer.TestInstrumentBrowse.Republish( resultsByInstrument );
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
	}
}