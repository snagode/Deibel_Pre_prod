using System;
using System.ComponentModel;
using Thermo.SampleManager.Common;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel;

namespace Thermo.SampleManager.Tasks.ChromeleonLinkTasks
{
	/// <summary>
	/// Chromeleon Sequence Task
	/// </summary>
	[SampleManagerTask("ChromeleonCreateSequenceTask")]
	public class ChromeleonCreateSequenceTask : SampleManagerTask
	{
		#region Member Variables

		private FormChromeleonCreateSequence m_SequenceForm;
		private ChromeleonSequenceEntity m_ChromeleonSequence;
		private IEntityCollection m_Tests;
		private ChromeleonMappingEntity m_Mapping;
		private ChromeleonEntity m_Chromeleon;
		private BatchHeader m_BatchHeader;
		private string m_StartPosition = string.Empty;

		#endregion

		#region Setup

		/// <summary>
		/// Setups the task.
		/// </summary>
		protected override void SetupTask()
		{
			base.SetupTask();

			CreateInitialSequence();

			// Start the form

			m_SequenceForm = FormFactory.CreateForm<FormChromeleonCreateSequence>(m_ChromeleonSequence);

			m_SequenceForm.Loaded += SequenceFormLoaded;
			m_SequenceForm.Created += SequenceFormCreated;

			m_SequenceForm.CreateButton.ClickAndWait += CreateButton_Click;

			m_SequenceForm.Show(FormDisplayStyle.Default);
		}

		#endregion

		#region Get Data

		/// <summary>
		/// Creates the initial sequence.
		/// </summary>
		private void CreateInitialSequence()
		{
			m_ChromeleonSequence = (ChromeleonSequenceEntity) EntityManager.CreateEntity(ChromeleonSequenceBase.EntityName);

			if (Context.TaskParameters == null) return;
			if (Context.TaskParameters.GetLength(0) == 0) return;
			string id = Context.TaskParameters[0];
			m_Mapping = (ChromeleonMappingEntity) EntityManager.Select(ChromeleonMappingBase.EntityName, id);
			if (!BaseEntity.IsValid(m_Mapping)) return;

			m_ChromeleonSequence.SetFromMapping(m_Mapping);
			m_Chromeleon = m_Mapping.Chromeleon;
			m_StartPosition = m_ChromeleonSequence.StartVialPosition;

			// Create using appropriate data

			if (Context.EntityType == BatchHeaderBase.EntityName)
			{
				CreateInitialBatchSequence();
			}
			else
			{
				CreateInitialTestSequence();
			}

			// Auto-values

			if (!string.IsNullOrEmpty(m_ChromeleonSequence.StartVialPosition) && !m_Mapping.WorkflowMapping)
			{
				m_ChromeleonSequence.AutoPosition();
			}

			m_ChromeleonSequence.AutoName(m_Mapping);

		}

		#endregion

		#region Batch

		/// <summary>
		/// Creates the initial batch sequence.
		/// </summary>
		private void CreateInitialBatchSequence()
		{
			LoadBatchData();

			if (!BaseEntity.IsValid(m_BatchHeader)) return;

			m_ChromeleonSequence.SetFromBatch(m_BatchHeader, m_Mapping.TranslateTypes);

		}

		/// <summary>
		/// Loads the Batch Data
		/// </summary>
		private void LoadBatchData()
		{
			if (Context.SelectedItems == null) return;
			if (Context.SelectedItems.EntityType != BatchHeaderBase.EntityName) return;
			if (Context.SelectedItems.Count == 0) return;
			m_BatchHeader = (BatchHeader)Context.SelectedItems[0];
		}

		#endregion

		#region Tests

		/// <summary>
		/// Creates the initial test sequence.
		/// </summary>
		private void CreateInitialTestSequence()
		{
			LoadTestData();

			foreach (TestBase test in m_Tests)
			{
				m_ChromeleonSequence.AddByTest(test, m_Mapping.TranslateTypes);
			}

			if (! string.IsNullOrEmpty(m_ChromeleonSequence.StartVialPosition) && !m_Mapping.WorkflowMapping)
			{
				m_ChromeleonSequence.AutoPosition();
			}

			m_ChromeleonSequence.AutoName(m_Mapping);
		}

		/// <summary>
		/// Loads the Test data.
		/// </summary>
		private void LoadTestData()
		{
			m_Tests = EntityManager.CreateEntityCollection(TestBase.EntityName);
			if (Context.SelectedItems == null) return;
			if (Context.SelectedItems.EntityType != TestBase.EntityName) return;

			foreach (TestBase test in Context.SelectedItems)
			{
				m_Tests.Add(test);
			}
		}

		#endregion

		#region Behaviour

		/// <summary>
		/// Sequences form loaded.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		public void SequenceFormLoaded(object sender, EventArgs e)
		{
			m_SequenceForm.StartPosition.StringChanged += StartPosition_StringChanged;

			ConfigureMapping();

			var worker = new BackgroundWorker();
			worker.DoWork += worker_DoWork;
			worker.RunWorkerAsync();
		}

		/// <summary>
		/// Sequences form created.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void SequenceFormCreated(object sender, EventArgs e)
		{
			m_SequenceForm.EntryGrid.CellEnabled += EntryGrid_CellEnabled;
		}

		/// <summary>
		/// Handles the CellEnabled event of the EntryGrid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CellEnabledEventArgs"/> instance containing the event data.</param>
		private void EntryGrid_CellEnabled(object sender, CellEnabledEventArgs e)
		{
			if (e.PropertyName == ChromeleonSequenceEntryPropertyNames.VialPosition)
			{
				if (m_Mapping.WorkflowMapping)
				{
					e.Enabled = false;
					e.DisabledMode = DisabledCellDisplayMode.GreyHideContents;
				}
			}

			if (e.PropertyName == ChromeleonSequenceEntryPropertyNames.ChromeleonSequenceEntryName)
			{
				if (! m_Mapping.AllowRename)
				{
					e.Enabled = false;
					e.DisabledMode = DisabledCellDisplayMode.GreyShowContents;
				}
			}
		}

		/// <summary>
		/// Refreshes the browses.
		/// </summary>
		private void RefreshBrowses()
		{
			// Autosampler Positions

			var instrument = (ChromeleonInstrumentEntity)m_ChromeleonSequence.ChromeleonInstrument;
			if (BaseEntity.IsValid(instrument))
			{
				m_SequenceForm.SamplerPositionsBrowse.Republish(instrument.AutosamplerPositionList);
			}

			// Folders

			if (!m_Mapping.AllowFolderChange) return;

			m_Chromeleon.ReadFolders();
			m_SequenceForm.FolderBrowse.Republish(m_Chromeleon.Folders, "FolderUri");
		}

		/// <summary>
		/// ConfigureMapping
		/// </summary>
		private void ConfigureMapping()
		{
			// Control which fields are visible.

			if (!m_Mapping.AllowFolderChange)
			{
				m_SequenceForm.SequenceFolder.ReadOnly = true;
			}

			if (!m_Mapping.AllowRename)
			{
				m_SequenceForm.SequenceName.ReadOnly = true;
			}
		}

		/// <summary>
		/// Handles the DoWork event of the worker control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
		private void worker_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				RefreshBrowses();
			}
			catch (Exception ex)
			{
				Logger.Debug("Exception updating browses", ex);
			}
		}

		/// <summary>
		/// Handles the ClickAndWait event of the CreateButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		private void CreateButton_Click(object sender, EventArgs e)
		{
			string name = m_ChromeleonSequence.ChromeleonSequenceName;
			string folder = m_ChromeleonSequence.SequenceFolderUri;
			string startPosition = m_ChromeleonSequence.StartVialPosition;

			// Make sure we have a start position

			if (m_Mapping.WorkflowMapping && string.IsNullOrEmpty(startPosition))
			{
				m_SequenceForm.StartPosition.ShowError(m_SequenceForm.StringTable.MissingStartPosition);
				return;
			}

			// Folders and names only mandatory for general sequences

			if (!m_Mapping.WorkflowMapping)
			{
				if (string.IsNullOrEmpty(name))
				{
					m_SequenceForm.SequenceName.ShowError(m_SequenceForm.StringTable.MissingName);
					return;
				}

				if (string.IsNullOrEmpty(folder))
				{
					m_SequenceForm.SequenceFolder.ShowError(m_SequenceForm.StringTable.MissingFolder);
					return;
				}
			}

			// Try to Create the Sequence

			try
			{
				CreateSequence();

				string message = string.Format(m_SequenceForm.StringTable.SequenceCreatedMessage, m_ChromeleonSequence.SequenceUri);
				string title = m_SequenceForm.StringTable.SequenceCreatedTitle;

				Library.Utils.FlashMessage(message, title);

				var worker = new BackgroundWorker();
				worker.DoWork += CloserWorker_DoWork;
				worker.RunWorkerAsync();
			}
			catch (SampleManagerError error)
			{
				Library.Utils.FlashMessage(error.Message, error.Title, MessageButtons.OK, MessageIcon.Error, MessageDefaultButton.Button1);
			}
		}

		/// <summary>
		/// Creates the sequence.
		/// </summary>
		private void CreateSequence()
		{
			m_Chromeleon.CreateSequence(m_ChromeleonSequence);

			// Update the Identity/Name

			string name = m_ChromeleonSequence.ChromeleonSequenceName;
			m_ChromeleonSequence.Identity = new PackedDecimal(Library.Increment.GetIncrement("CHROMELEON", "SEQUENCE"));
			m_ChromeleonSequence.ChromeleonSequenceName = name;

			// Save to the Database

			EntityManager.Transaction.Add(m_ChromeleonSequence);
			EntityManager.Commit();
		}

		/// <summary>
		/// Close the form via a separate task
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
		private void CloserWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			m_SequenceForm.Close();
		}

		#endregion

		#region Auto Positioning

		/// <summary>
		/// Handles the StringChanged event of the StartPosition control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="SampleManager.Library.ClientControls.TextChangedEventArgs"/> instance containing the event data.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		private void StartPosition_StringChanged(object sender, TextChangedEventArgs e)
		{
			if (m_Mapping.WorkflowMapping) return;
			if (m_StartPosition == m_ChromeleonSequence.StartVialPosition) return;
			if (!m_ChromeleonSequence.CanAutoPosition) return;

			m_StartPosition = m_ChromeleonSequence.StartVialPosition;

			string caption = m_SequenceForm.StringTable.AutoPositionTitle;
			string message = m_SequenceForm.StringTable.AutoPostionMessage;

			message = string.Format(message, m_ChromeleonSequence.StartVialPosition);

			if (Library.Utils.FlashMessageYesNo(message, caption))
			{
				m_ChromeleonSequence.AutoPosition();
			}
		}

		#endregion
	}
}