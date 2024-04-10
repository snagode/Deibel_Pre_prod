using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel;
using Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel.WebApi;

namespace Thermo.SampleManager.Tasks.ChromeleonLinkTasks
{
	/// <summary>
	/// Chromeleon Task
	/// </summary>
	[SampleManagerTask("ChromeleonMappingTask", PhraseFormCat.PhraseIdLABTABLE, ChromeleonMappingBase.EntityName)]
	public class ChromeleonMappingTask : GenericLabtableTask
	{
		#region Member Variables

		private FormChromeleonMapping m_Form;
		private ChromeleonMappingEntity m_ChromeleonMapping;

		#endregion

		#region Properties

		/// <summary>
		/// Gets a value indicating whether this is in insertion mode
		/// </summary>
		/// <value>
		///   <c>true</c> if insertion mode ; otherwise, <c>false</c>.
		/// </value>
		private bool InsertingRecord
		{
			get 
			{ 
				return Context.LaunchMode == CopyOption || Context.LaunchMode == AddOption; 
			}
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormChromeleonMapping)MainForm;
			m_ChromeleonMapping = (ChromeleonMappingEntity)MainForm.Entity;

			base.MainFormCreated();
		}

		/// <summary>
		/// Called when the main form has loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			base.MainFormLoaded();

			m_Form.ConnectButton.ClickAndWait += ConnectButton_ClickAndWait;
			m_Form.RefreshButton.ClickAndWait += ConnectButton_ClickAndWait;
			m_Form.ResultConnectButton.ClickAndWait += ConnectButton_ClickAndWait;
			m_Form.ResultRefreshButton.ClickAndWait += ConnectButton_ClickAndWait;
			m_Form.SequenceConnectButton.ClickAndWait += ConnectButton_ClickAndWait;
			m_Form.SequenceRefreshButton.ClickAndWait += ConnectButton_ClickAndWait;

			// Method Mapping Mode

			ToggleMapping(m_ChromeleonMapping.WorkflowMapping);

			m_Form.WorkflowRadio.Checked = m_ChromeleonMapping.WorkflowMapping;
			m_Form.MethodRadio.Checked = !m_ChromeleonMapping.WorkflowMapping;

			m_Form.WorkflowRadio.CheckedChanged += WorkflowRadio_CheckedChanged;
			m_Form.MethodRadio.CheckedChanged += MethodRadio_CheckedChanged;

			// Don't enable connect before an instrument is defined.

			if (m_ChromeleonMapping.Chromeleon != null)
			{
				m_Form.ConnectButton.Enabled = true;
			}

			m_ChromeleonMapping.PropertyChanged += ChromeleonMapping_PropertyChanged;

			// Auto Sampler Default Positions

			var instrument = (ChromeleonInstrumentEntity)m_ChromeleonMapping.ChromeleonInstrument;
			PublishSamplerPositions(instrument);

			// Only allow key fields to be changed on insert/copy

			if (InsertingRecord)
			{
				PublishInstruments();
				return;
			}

			m_Form.Analysis.ReadOnly = true;
			m_Form.Instrument.ReadOnly = true;
			m_Form.Chromeleon.ReadOnly = true;
		}

		#endregion

		#region Connections

		/// <summary>
		/// Handles the ChromeleonConnected event of the Chromeleon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Chromeleon_ChromeleonConnected(object sender, System.EventArgs e)
		{
			PublishFolders();
			PublishInstrumentMethods();
			PublishProcessingMethods();
			PublishEWorkflow();
			PublishChromeleonComponents();

			ToggleConnected(true);
		}

		/// <summary>
		/// Publishes the sampler positions.
		/// </summary>
		private void PublishSamplerPositions(ChromeleonInstrumentEntity instrument)
		{
			if (!BaseEntity.IsValid(instrument)) return;
			m_Form.SamplerPositionBrowse.Republish(instrument.AutosamplerPositionList);
		}

		/// <summary>
		/// Publishes the folders.
		/// </summary>
		private void PublishFolders()
		{
			if (m_ChromeleonMapping.Chromeleon != null && m_ChromeleonMapping.Chromeleon.Connected)
			{
				m_Form.FolderBrowse.Republish(m_ChromeleonMapping.Chromeleon.Folders, "ResourceUriFormatted");
				m_Form.DefaultFolderBrowse.Republish(m_ChromeleonMapping.Chromeleon.Folders, "ResourceUriFormatted");

				return;
			}

			if (string.IsNullOrEmpty(m_ChromeleonMapping.InstrumentMethodFolderUri)) return;
			m_Form.FolderBrowse.Republish(new List<string> { m_ChromeleonMapping.InstrumentMethodFolderUri });
		}

		/// <summary>
		/// Publishes the processing methods.
		/// </summary>
		private void PublishProcessingMethods()
		{
			m_ChromeleonMapping.ReadProcessingMethods();
			m_Form.ProcessingMethodBrowse.Republish(m_ChromeleonMapping.ProcessingMethods, "ProcessingMethodName");
		}

		/// <summary>
		/// Publishes the components available within the Chromeleon Component screen.
		/// </summary>
		private void PublishChromeleonComponents()
		{
			m_ChromeleonMapping.ReadProcessingMethod();
			if (m_ChromeleonMapping.ApiProcessingMethod == null) return;
			m_Form.ChromeleonComponentBrowse.Republish(m_ChromeleonMapping.ApiProcessingMethod.Components, "ComponentName");
		}

		/// <summary>
		/// Publishes the workflow.
		/// </summary>
		private void PublishEWorkflow()
		{
			m_Form.WorkflowBrowse.Republish(m_ChromeleonMapping.Chromeleon.EWorkflows,"WorkflowName");
		}

		/// <summary>
		/// Publishes the instrument methods.
		/// </summary>
		private void PublishInstrumentMethods()
		{
			if (m_ChromeleonMapping.Chromeleon != null && m_ChromeleonMapping.Chromeleon.Connected)
			{
				m_ChromeleonMapping.ReadInstrumentMethods();
				m_Form.InstrumentMethodBrowse.Republish(m_ChromeleonMapping.InstrumentMethods, "InstrumentMethodName");
				return;
			}

			if (string.IsNullOrEmpty(m_ChromeleonMapping.InstrumentMethodFolderUri)) return;
			m_Form.FolderBrowse.Republish(new List<string> { m_ChromeleonMapping.InstrumentMethodFolderUri });
		}

		/// <summary>
		/// Toggles the connected state
		/// </summary>
		/// <param name="connected">if set to <c>true</c> connected.</param>
		private void ToggleConnected(bool connected)
		{
			m_Form.ConnectButton.Visible = !connected;
			m_Form.RefreshButton.Visible = connected;

			m_Form.ResultConnectButton.Visible = !connected;
			m_Form.ResultRefreshButton.Visible = connected;

			m_Form.SequenceConnectButton.Visible = !connected;
			m_Form.SequenceRefreshButton.Visible = connected;
		}

		/// <summary>
		/// Handles the ClickAndWait event of the ConnectButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void ConnectButton_ClickAndWait(object sender, System.EventArgs e)
		{
			if (m_ChromeleonMapping.Chromeleon == null) return;

			m_ChromeleonMapping.Chromeleon.ChromeleonConnected -= Chromeleon_ChromeleonConnected;
			m_ChromeleonMapping.Chromeleon.ChromeleonConnected += Chromeleon_ChromeleonConnected;

			m_ChromeleonMapping.Chromeleon.ChromeleonConnectionError -= Chromeleon_ChromeleonConnectionError;
			m_ChromeleonMapping.Chromeleon.ChromeleonConnectionError += Chromeleon_ChromeleonConnectionError;

			m_ChromeleonMapping.Chromeleon.Connect();
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

		#region Mapping Modes

		/// <summary>
		/// Toggles the mapping.
		/// </summary>
		/// <param name="workflowMapping">if set to <c>true</c> [workflow mapping].</param>
		private void ToggleMapping(bool workflowMapping)
		{
			m_Form.InstrumentMethod.Enabled = !workflowMapping;
			m_Form.ProcessingMethod.Enabled = !workflowMapping;

			m_Form.InstrumentMethodFolder.Enabled = !workflowMapping;
			m_Form.ProcessingMethodFolder.Enabled = !workflowMapping;

			m_Form.DefaultVolume.Enabled = !workflowMapping;

			m_Form.OverwriteInjNames.Enabled = workflowMapping;
			m_Form.EWorkflow.Enabled = workflowMapping;
		}

		/// <summary>
		/// Handles the CheckedChanged event of the MethodRadio control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="SampleManager.Library.ClientControls.CheckedChangedEventArgs"/> instance containing the event data.</param>
		private void MethodRadio_CheckedChanged(object sender, SampleManager.Library.ClientControls.CheckedChangedEventArgs e)
		{
			ToggleMapping(!e.Checked);
		}

		/// <summary>
		/// Handles the CheckedChanged event of the WorkflowRadio control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="SampleManager.Library.ClientControls.CheckedChangedEventArgs"/> instance containing the event data.</param>
		private void WorkflowRadio_CheckedChanged(object sender, SampleManager.Library.ClientControls.CheckedChangedEventArgs e)
		{
			ToggleMapping(e.Checked);
		}

		#endregion

		#region Property Events

		/// <summary>
		/// Handles the PropertyChanged event of the ChromeleonMapping control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PropertyEventArgs"/> instance containing the event data.</param>
		private void ChromeleonMapping_PropertyChanged(object sender, PropertyEventArgs e)
		{
			if (e.PropertyName == ChromeleonMappingPropertyNames.ChromeleonId)
			{
				PublishInstruments();
				m_Form.ConnectButton.Enabled = (m_ChromeleonMapping.Chromeleon != null);
				ToggleConnected(false);
			}
			if (e.PropertyName == ChromeleonMappingPropertyNames.InstrumentId)
			{
				var instrument = m_ChromeleonMapping.GetChromeleonInstrument();
				PublishSamplerPositions(instrument);
			}
			else if (e.PropertyName == ChromeleonMappingPropertyNames.AnalysisId)
			{
				if (m_ChromeleonMapping.ChromeleonMappingResults.ActiveCount > 0)
				{
					m_ChromeleonMapping.ChromeleonMappingResults.RemoveAll();
				}
			}
			else if (e.PropertyName == ChromeleonMappingPropertyNames.InstrumentMethodFolderUri)
			{
				PublishInstrumentMethods();
			}
			else if (e.PropertyName == ChromeleonMappingPropertyNames.ProcessingMethodFolderUri)
			{
				PublishProcessingMethods();
			}
			else if (e.PropertyName == ChromeleonMappingPropertyNames.ProcessingMethod)
			{
				PublishChromeleonComponents();
			}
		}

		/// <summary>
		/// Publishes the instruments.
		/// </summary>
		private void PublishInstruments()
		{
			bool added = false;
			var query = EntityManager.CreateQuery(ChromeleonInstrumentBase.EntityName);
			query.AddEquals(ChromeleonInstrumentPropertyNames.ChromeleonId, m_ChromeleonMapping.ChromeleonId);

			// Spin through and build a query of mapped instruments

			var instQuery = EntityManager.CreateQuery(InstrumentBase.EntityName);
			instQuery.HideRemoved();

			if (BaseEntity.IsValid(m_ChromeleonMapping.Chromeleon))
			{
				var chromeleon = (ChromeleonBase) EntityManager.Select(ChromeleonBase.EntityName, new Identity(m_ChromeleonMapping.ChromeleonId));
				if (BaseEntity.IsValid(chromeleon))
				{
					foreach (ChromeleonInstrumentBase instrument in chromeleon.ChromeleonInstruments)
					{
						if (added) instQuery.AddOr();
						instQuery.AddEquals(InstrumentPropertyNames.Identity, instrument.InstrumentId);
						added = true;
					}
				}
			}

			// Blank the list if no instruments are valid

			if (!added)
			{
				instQuery.AddEquals(InstrumentPropertyNames.Identity, string.Empty);
			}

			instQuery.AddDefaultOrder();
			m_Form.InstrumentBrowse.Republish(instQuery);
		}

		#endregion
	}
}