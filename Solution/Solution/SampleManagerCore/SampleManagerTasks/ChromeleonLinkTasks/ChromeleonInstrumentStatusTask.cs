using System;
using System.Diagnostics;
using System.Globalization;
using System.Timers;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel;

namespace Thermo.SampleManager.Tasks.ChromeleonLinkTasks
{
	/// <summary>
	/// Getting started creating a task in SampleManager
	/// </summary>
	[SampleManagerTask("ChromeleonInstrumentStatusTask")]
	public class ChromeleonInstrumentStatusTask : SampleManagerTask
	{
		#region Constants

		private const string LocalSettingType = "Chromeleon";
		private const string LocalSettingIdentity = "InstrumentStatus";
		private const string LocalSettingRate = "RefreshRate";
		private const string LocalSettingPoll = "PollActive";

		private const string LockNameFormat = "CLInstrumentStatus{0}";

		#endregion

		#region Member Variables

		private FormChromeleonInstrumentStatus m_Form;
		private IEntityCollection m_Instruments;
		private Timer m_Timer;
		private bool m_Closed;
		private bool m_Updating;
		private string m_LockName;

		#endregion

		#region Setup

		/// <summary>
		/// Setup form.
		/// </summary>
		protected override void SetupTask()
		{
			// Apparently there may only be one of these in a given session

			m_LockName = string.Format(LockNameFormat, Process.GetCurrentProcess().Id);

			if (!Library.Locking.GrantLock(m_LockName))
			{
				Exit();
			}

			// Create the Status Form

			m_Form = FormFactory.CreateForm<FormChromeleonInstrumentStatus>();

			m_Form.Loaded += FormLoaded;
			m_Form.Closed += FormClosed;
			m_Form.Closing += FormClosing;

			m_Form.Show();
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Handles the form loaded event. This is the correct place to add event handles for the controls on the form.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void FormLoaded(object sender, EventArgs e)
		{
			// Use the user settings to load the Refresh rate spinbox if available.

			int rate;
			if (int.TryParse(Library.Environment.ReadLocalSetting(LocalSettingType, LocalSettingIdentity, LocalSettingRate), NumberStyles.Any, CultureInfo.InvariantCulture, out rate))
			{
				m_Form.RefreshRate.Number = rate;
			}

			// Use the user settings to load the polling checkbox if available.

			bool poll;
			if (bool.TryParse(Library.Environment.ReadLocalSetting(LocalSettingType, LocalSettingIdentity, LocalSettingPoll), out poll))
			{
				m_Form.AutoRefresh.Checked = poll;
			}

			// Create the timer based on initial interval setting.

			m_Timer = new Timer(m_Form.RefreshRate.Number*1000);
			m_Timer.Elapsed += Timer_Elapsed;
			m_Timer.AutoReset = false;

			// Additional handlers

			m_Form.CloseButton.Click += CloseButton_Click;
			m_Form.RefreshButton.ClickAndWait += RefreshButton_Click;
			m_Form.RefreshRate.NumberChanged += RefreshRate_NumberChanged;
			m_Form.AutoRefresh.CheckedChanged += AutoRefresh_CheckedChanged;

			// Context Menu

			AddContextMenu();

			// Keep track of the initial data selection

			LoadInstruments();

			// Queue the first update for the remote instrument information.

			System.Threading.ThreadPool.QueueUserWorkItem(GetInitialData);
		}

		/// <summary>
		/// Initial call to update remote instrument information.  Starts timer when complete.
		/// </summary>
		/// <param name="state"></param>
		private void GetInitialData(object state)
		{
			if (m_Closed) return;
			UpdateInstruments(state);

			if (m_Form.AutoRefresh.Checked)
			{
				m_Timer.Start();
			}
		}

		/// <summary>
		/// Loads the instruments.
		/// </summary>
		private void LoadInstruments()
		{
			m_Instruments = m_Form.ChromeleonInstruments.Data;

			IQuery query = EntityManager.CreateQuery(ChromeleonBase.EntityName);
			query.AddEquals(ChromeleonPropertyNames.Removeflag, false);

			var chromeleons = EntityManager.Select(query.TableName, query);
	
			foreach (ChromeleonEntity chromeleon in chromeleons)
			{
				foreach (ChromeleonInstrumentEntity instrument in chromeleon.ChromeleonInstruments)
				{
					m_Instruments.Add(instrument);
				}
			}

			m_Instruments.AddSortField(ChromeleonInstrumentPropertyNames.InstrumentId, false);
			m_Instruments.Sort();
		}

		/// <summary>
		/// Handles the Click event of the RefreshButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		void RefreshButton_Click(object sender, EventArgs e)
		{
            System.Threading.ThreadPool.QueueUserWorkItem(UpdateInstruments);
		}

		/// <summary>
		/// Handles the CheckedChanged event of the AutoRefresh control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CheckEventArgs"/> instance containing the event data.</param>
		private void AutoRefresh_CheckedChanged(object sender, CheckEventArgs e)
		{
			// Save the user setting.

			Library.Environment.WriteLocalSetting(LocalSettingType, LocalSettingIdentity, LocalSettingPoll, m_Form.AutoRefresh.Checked.ToString(CultureInfo.InvariantCulture));

			// Modify the timer's status.

			m_Timer.Enabled = e.Checked;
		}

		/// <summary>
		/// Handles the Click event of the CloseButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void CloseButton_Click(object sender, EventArgs e)
		{
			m_Timer.Stop();
			m_Closed = true;
			m_Form.Close();
		}

		/// <summary>
		/// Ensure the timer event stops for sure
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			m_Timer.Stop();
			m_Closed = true;
		}

		/// <summary>
		/// Form closing, stop timer and mark local variable.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FormClosed(object sender, EventArgs e)
		{
			m_Timer.Stop();
			m_Closed = true;

			Library.Locking.ReleaseLock(m_LockName);
		}

		/// <summary>
		/// Handles the NumberChanged event of the RefreshRate control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="NumberChangedEventArgs"/> instance containing the event data.</param>
		private void RefreshRate_NumberChanged(object sender, NumberChangedEventArgs e)
		{
			// Save the user setting.

			Library.Environment.WriteLocalSetting(LocalSettingType, LocalSettingIdentity, LocalSettingRate, m_Form.RefreshRate.Number.ToString(CultureInfo.InvariantCulture));

			// Clear any previous range error if value is in range now.

			m_Form.RefreshRate.ShowError(null);

			// Modify the timer's interval.

			m_Timer.Stop();

			m_Timer.Interval = e.Number * 1000;
			m_Timer.AutoReset = false;
			m_Timer.Enabled = m_Form.AutoRefresh.Checked;
		}

		/// <summary>
		/// Process update for remote instrument information.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			// Don't run the code if the form is closed now.

			if (m_Closed) return;
			if (m_Updating) return;

			UpdateInstruments(null);
		}

		#endregion

		#region Refresh

		/// <summary>
		/// Updates the instruments.
		/// </summary>
        private void UpdateInstruments(object state)
		{
			if (m_Updating) return;
			if (m_Closed) return;
			m_Updating = true;

			// Init progress bar.

			m_Form.ProgressBar.Position = 0;
			m_Form.ProgressBar.Visible = true;
			m_Form.ProgressBar.Maximum = m_Instruments.Count;

			// Iterate over servers and get refreshed information.

			try
			{
				foreach (ChromeleonInstrumentEntity instrument in m_Instruments)
				{
					instrument.UpdateInstrumentFromApi();
					if (m_Closed) return;
					m_Form.ProgressBar.PerformStep();
				}
			}
			catch (Exception e)
			{
				Logger.Debug(e.Message, e);
			}

			// Wait a second before resetting progress bar.

			System.Threading.Thread.Sleep(500);

			// Check to see if the form is still open before updating

			if (m_Closed) return;
			m_Form.ProgressBar.Visible = false;

			// Reset the Timer

			m_Updating = false;

			if (m_Form.AutoRefresh.Checked)
			{
				m_Timer.Start();
			}
		}

		#endregion

		#region Context Menu

		/// <summary>
		/// Adds the context menu.
		/// </summary>
		private void AddContextMenu()
		{
			ContextMenuItem refreshItem = new ContextMenuItem(m_Form.StringTable.MenuRefreshInstrumentText, m_Form.StringTable.MenuRefreshInstrumentIcon);
			refreshItem.ItemClicked += refreshItem_ItemClicked;
			m_Form.InstrumentGrid.ContextMenu.AddItem(refreshItem);

			ContextMenuItem refreshAllItem = new ContextMenuItem(m_Form.StringTable.MenuRefreshText, m_Form.StringTable.MenuRefreshIcon);
			refreshAllItem.ItemClicked += refreshAllItem_ItemClicked;
			m_Form.InstrumentGrid.ContextMenu.AddItem(refreshAllItem);
		}

		/// <summary>
		/// Handles the ItemClicked event of the refreshItem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="ContextMenuItemEventArgs"/> instance containing the event data.</param>
		private void refreshItem_ItemClicked(object sender, ContextMenuItemEventArgs e)
		{
			var instrument = e.Entity as ChromeleonInstrumentEntity;
			if (instrument == null) return;
			instrument.UpdateInstrumentFromApi();
		}

		/// <summary>
		/// Handles the ItemClicked event of the refreshAllItem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="ContextMenuItemEventArgs"/> instance containing the event data.</param>
		private void refreshAllItem_ItemClicked(object sender, ContextMenuItemEventArgs e)
		{
            System.Threading.ThreadPool.QueueUserWorkItem(UpdateInstruments);
		}

		#endregion
	}
}