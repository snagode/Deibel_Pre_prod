using System;
using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.ClientControls.Browse;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel;
using Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel.WebApi;

namespace Thermo.SampleManager.Tasks.ChromeleonLinkTasks
{
	/// <summary>
	/// Chromeleon Task
	/// </summary>
	[SampleManagerTask("ChromeleonTask", "LABTABLE", "CHROMELEON")]
	public class ChromeleonTask : GenericLabtableTask
	{
		#region Member Variables

		private FormChromeleon m_Form;
		private ChromeleonEntity m_Chromeleon;
		private StringBrowse m_BatchHeaderFieldNameBrowse;
		private StringBrowse m_SampleFieldNameBrowse;
		private StringBrowse m_TestFieldNameBrowse;
		private StringBrowse m_BatchEntryFieldNameBrowse;

		#endregion

		#region Properties

		/// <summary>
		/// Gets a value indicating whether display only.
		/// </summary>
		/// <value>
		///   <c>true</c> if display only; otherwise, <c>false</c>.
		/// </value>
		private bool DisplayOnly
		{
			get { return Context.LaunchMode != ModifyOption && Context.LaunchMode != CopyOption && Context.LaunchMode != AddOption; }
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormChromeleon) MainForm;
			m_Chromeleon = (ChromeleonEntity) MainForm.Entity;

			m_Chromeleon.ChromeleonConnected += Chromeleon_ChromeleonConnected;
			m_Chromeleon.ChromeleonDisconnected += Chromeleon_ChromeleonDisconnected;
			m_Chromeleon.ChromeleonConnectionError += Chromeleon_ChromeleonConnectionError;
			m_Chromeleon.PropertyChanged += Chromeleon_PropertyChanged;

			m_Form.GridChromeleonProperties.CellEditor += GridChromeleonProperties_CellEditor;
			m_Form.InstrumentMappingGrid.CellEditor += InstrumentMappingGrid_CellEditor;
			m_Form.InstrumentMappingGrid.BeforeRowDelete += InstrumentMappingGrid_BeforeRowDelete;
			m_Form.InstrumentMappingGrid.CellEnabled += InstrumentMappingGrid_CellEnabled;

			m_Form.ConnectButton.ClickAndWait += ConnectButton_ClickAndWait;
			m_Form.MapConnectButton.ClickAndWait += ConnectButton_ClickAndWait;
			m_Form.InstConnectButton.ClickAndWait += ConnectButton_ClickAndWait;

			m_Form.RefreshButton.ClickAndWait += ConnectButton_ClickAndWait;
			m_Form.MapRefreshButton.ClickAndWait += ConnectButton_ClickAndWait;
			m_Form.InstRefreshButton.ClickAndWait += ConnectButton_ClickAndWait;

			base.MainFormCreated();
		}

		/// <summary>
		/// Called when the main form has loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			base.MainFormLoaded();

			m_TestFieldNameBrowse = CreateSortedFieldBrowse(TableNames.Test);
			m_SampleFieldNameBrowse = CreateSortedFieldBrowse(TableNames.Sample);
			m_BatchHeaderFieldNameBrowse = CreateSortedFieldBrowse(TableNames.BatchHeader);
			m_BatchEntryFieldNameBrowse = CreateSortedFieldBrowse(TableNames.BatchEntry);

			if (DisplayOnly)
			{
				m_Form.MapConnectButton.Visible = false;
				m_Form.MapRefreshButton.Visible = false;
			}
		}

		#endregion

		#region Browses

		/// <summary>
		/// Create a sorted field browse.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		/// <returns></returns>
		private StringBrowse CreateSortedFieldBrowse(string tableName)
		{
			List<string> fields = new List<string>();
			ISchemaTable table;

			if (Library.Schema.Tables.TryGetValue(tableName, out table))
			{
				foreach (ISchemaField field in table.Fields)
				{
					fields.Add(field.Name);
				}
			}

			fields.Sort();
			return BrowseFactory.CreateStringBrowse(fields);
		}

		#endregion

		#region Connections

		/// <summary>
		/// Handles the ChromeleonDisconnected event of the Chromeleon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Chromeleon_ChromeleonDisconnected(object sender, EventArgs e)
		{
			ToggleConnected(false);
		}

		/// <summary>
		/// Handles the ChromeleonConnected event of the Chromeleon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Chromeleon_ChromeleonConnected(object sender, EventArgs e)
		{
			PublishInstruments();
			PublishVaults();
			PublishFolders();

			ToggleConnected(true);
		}

		/// <summary>
		/// Toggles the connected state
		/// </summary>
		/// <param name="connected">if set to <c>true</c> connected.</param>
		private void ToggleConnected(bool connected)
		{
			m_Form.ConnectButton.Visible = !connected;
			m_Form.RefreshButton.Visible = connected;

			m_Form.InstConnectButton.Visible = !connected;
			m_Form.InstRefreshButton.Visible = connected;

			if (DisplayOnly) return;

			m_Form.MapConnectButton.Visible = !connected;
			m_Form.MapRefreshButton.Visible = connected;

			m_Form.DefaultVaultName.ReadOnly = !connected;
		}

		/// <summary>
		/// Handles the ClickAndWait event of the ConnectButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void ConnectButton_ClickAndWait(object sender, EventArgs e)
		{
			m_Chromeleon.Connect();
		}

		/// <summary>
		/// Handles the ChromeleonConnectionError event of the Chromeleon server.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="ErrorEventArgs"/> instance containing the event data.</param>
		private void Chromeleon_ChromeleonConnectionError(object sender, ErrorEventArgs e)
		{
			Library.Utils.FlashMessage(e.Error.Message, e.Error.Title, MessageButtons.OK, MessageIcon.Error, MessageDefaultButton.Button1);
		}

		#endregion

		#region Default Vaults

		/// <summary>
		/// Publishes the vaults.
		/// </summary>
		private void PublishVaults()
		{
			m_Form.DefaultVaultName.Browse.Republish(m_Chromeleon.Vaults, "VaultName");
		}

		#endregion

		#region Instruments

		/// <summary>
		/// Publishes the Instruments
		/// </summary>
		private void PublishInstruments()
		{
			m_Form.InstrumentBrowse.Republish(m_Chromeleon.Instruments, "InstrumentName");
		}

		#endregion

		#region Folders

		/// <summary>
		/// Handles the PropertyChanged event of the Chromeleon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PropertyEventArgs"/> instance containing the event data.</param>
		private void Chromeleon_PropertyChanged(object sender, PropertyEventArgs e)
		{
			if (e.PropertyName == ChromeleonPropertyNames.ServerName ||
			    e.PropertyName == ChromeleonPropertyNames.VaultName)
			{
				PublishFolders();
			}
		}

		/// <summary>
		/// Publishes the Folders
		/// </summary>
		private void PublishFolders()
		{
			m_Form.MethodFolderBrowse.Republish(m_Chromeleon.Folders, "ResourceUriFormatted");
		}

		#endregion

		#region Property Grid

		/// <summary>
		/// Handles the CellEditor event of the GridChromeleonProperties control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="SampleManager.Library.ClientControls.CellEditorEventArgs"/> instance containing the event data.</param>
		private void GridChromeleonProperties_CellEditor(object sender, CellEditorEventArgs e)
		{
			if (e.PropertyName == ChromeleonPropertyPropertyNames.FieldName)
			{
				ChromeleonPropertyEntity property = (ChromeleonPropertyEntity) e.Entity;

				if (property.TableName == TableNames.Test) e.Browse = m_TestFieldNameBrowse;
				if (property.TableName == TableNames.Sample) e.Browse = m_SampleFieldNameBrowse;
				if (property.TableName == TableNames.BatchHeader) e.Browse = m_BatchHeaderFieldNameBrowse;
				if (property.TableName == TableNames.BatchEntry) e.Browse = m_BatchEntryFieldNameBrowse;
			}
		}

		#endregion

		#region Instrument Mapping Grid

		/// <summary>
		/// Handles the CellEditor event of the InstrumentMappingGrid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="SampleManager.Library.ClientControls.CellEditorEventArgs"/> instance containing the event data.</param>
		private void InstrumentMappingGrid_CellEditor(object sender, CellEditorEventArgs e)
		{
			if (!e.HasFocus) return;
			if (!m_Chromeleon.Connected) return;

			ChromeleonInstrumentEntity instrument = (ChromeleonInstrumentEntity) e.Entity;
			instrument.ChromeleonLink = m_Chromeleon;

			if (e.PropertyName == ChromeleonInstrumentPropertyNames.InstrumentMethod)
			{
				e.Browse = BrowseFactory.CreateStringBrowse(instrument.InstrumentMethods, "InstrumentMethodName");
			}
		}

		/// <summary>
		/// Handles the CellEnabled event of the InstrumentMappingGrid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="SampleManager.Library.ClientControls.CellEnabledEventArgs"/> instance containing the event data.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		private void InstrumentMappingGrid_CellEnabled(object sender, CellEnabledEventArgs e)
		{
			ChromeleonInstrumentEntity instrument = (ChromeleonInstrumentEntity) e.Entity;
			if (instrument.IsNew()) return;
			if (e.PropertyName != InstrumentPropertyPropertyNames.Instrument) return;
			e.Enabled = false;
			e.DisabledMode = DisabledCellDisplayMode.ShowContents;
		}

		/// <summary>
		/// Handles the BeforeRowDelete event of the InstrumentMappingGrid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="SampleManager.Library.ClientControls.BeforeRowDeleteEventArgs"/> instance containing the event data.</param>
		private void InstrumentMappingGrid_BeforeRowDelete(object sender, BeforeRowDeleteEventArgs e)
		{
			ChromeleonInstrumentEntity instrument = (ChromeleonInstrumentEntity) e.Entity;
			if (instrument.IsNew()) return;

			var exists = EntityManager.CreateQuery(ChromeleonMappingBase.EntityName);
			exists.AddEquals(ChromeleonMappingPropertyNames.InstrumentId, instrument.InstrumentId);
			exists.AddEquals(ChromeleonMappingPropertyNames.ChromeleonId, m_Chromeleon.Identity);

			if (EntityManager.SelectCount(exists) > 0)
			{
				string message = m_Form.StringTable.MappingExistsMessage;
				string caption = m_Form.StringTable.MappingExistsCaption;

				Library.Utils.FlashMessage(message, caption, MessageButtons.OK, MessageIcon.Information, MessageDefaultButton.Button1);
				e.Cancel = true;
			}
		}

		#endregion
	}
}