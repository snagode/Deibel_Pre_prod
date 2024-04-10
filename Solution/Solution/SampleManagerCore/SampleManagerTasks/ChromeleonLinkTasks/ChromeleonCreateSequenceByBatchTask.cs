using Thermo.SampleManager.Common;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel;

namespace Thermo.SampleManager.Tasks.ChromeleonLinkTasks
{
	/// <summary>
	/// Chromeleon Sequence Task using a Batch
	/// </summary>
	[SampleManagerTask("ChromeleonCreateSequenceByBatchTask")]
	public class ChromeleonCreateSequenceByBatchTask : SampleManagerTask
	{
		#region Member Variables

		private FormChromeleonSequenceFromBatch m_BatchForm;
		private BatchHeader m_BatchHeader;
		private ChromeleonMappingEntity m_Mapping;
		private bool m_Saving;

		#endregion

		#region Setup

		/// <summary>
		/// Setups the task.
		/// </summary>
		protected override void SetupTask()
		{
			base.SetupTask();

			// Start the form

			m_BatchForm = FormFactory.CreateForm<FormChromeleonSequenceFromBatch>();

			m_BatchForm.Loaded += BatchFormLoaded;
			m_BatchForm.Closing += BatchFormClosing;
			m_BatchForm.Closed += BatchFormClosed;

			m_BatchForm.Show(FormDisplayStyle.Default);
		}

		#endregion

		#region Get Data

		/// <summary>
		/// Loads the context data.
		/// </summary>
		private void LoadContextData()
		{
			if (Context.SelectedItems != null)
			{
				m_BatchHeader = (BatchHeader) Context.SelectedItems.GetFirst();
				m_BatchForm.Batch.Entity = m_BatchHeader;
			}
		}

		/// <summary>
		/// Gets the data for browses
		/// </summary>
		private void LoadBatchData()
		{
			if (BaseEntity.IsValid(m_BatchHeader))
			{
				var analysisId = new Identity(m_BatchHeader.Analysis.Identity);
				var analysis = (VersionedAnalysis) EntityManager.SelectLatestVersion(VersionedAnalysisBase.EntityName, analysisId);

				// If we cannot select the analysis drop out.

				if (!BaseEntity.IsValid(analysis))
				{
					m_BatchForm.Analysis.Entity = null;
					LoadInstrumentBrowse(null);
					return;
				}

				IQuery thisAnalysis = EntityManager.CreateQuery(VersionedAnalysisBase.EntityName);
				thisAnalysis.AddEquals(VersionedAnalysisPropertyNames.Identity, analysis.Identity);
				m_BatchForm.AnalysisBrowse.Republish(thisAnalysis);
				m_BatchForm.Analysis.Entity = analysis;

				LoadInstrumentBrowse(analysis);
			}
			else
			{
				m_BatchForm.Analysis.Entity = null;
				LoadInstrumentBrowse(null);
			}
		}

		/// <summary>
		/// Loads the instrument browse.
		/// </summary>
		/// <param name="analysis">The analysis.</param>
		private void LoadInstrumentBrowse(VersionedAnalysis analysis)
		{
			// Clear the prompts

			if (!BaseEntity.IsValid(analysis))
			{
				IQuery noInstruments = EntityManager.CreateQuery(InstrumentBase.EntityName);
				noInstruments.AddEquals(InstrumentPropertyNames.Identity, string.Empty);
				m_BatchForm.InstrumentBrowse.Republish(noInstruments);

				IQuery noAnalysis = EntityManager.CreateQuery(VersionedAnalysisBase.EntityName);
				noAnalysis.AddEquals(VersionedAnalysisPropertyNames.Identity, string.Empty);
				m_BatchForm.AnalysisBrowse.Republish(noAnalysis);

				return;
			}

			// Load valid browse data

			IQuery matches = EntityManager.CreateQuery(ChromeleonMappingBase.EntityName);
			matches.AddEquals(ChromeleonMappingPropertyNames.AnalysisId, analysis.Identity);
			matches.AddEquals(ChromeleonMappingPropertyNames.Removeflag, false);

			IEntityCollection matchingMaps = EntityManager.Select(ChromeleonMappingBase.EntityName, matches);

			// Browse on Instruments rather than Chromeleon Mapping Instruments.

			var instruments = EntityManager.CreateEntityCollection(InstrumentBase.EntityName);

			foreach (ChromeleonMappingEntity map in matchingMaps)
			{
				if (!BaseEntity.IsValid(map.ChromeleonInstrument)) continue;
				Instrument instrument = (Instrument) map.ChromeleonInstrument.Instrument;
				if (!BaseEntity.IsValid(instrument)) continue;
				if (instruments.Contains(instrument)) continue;
				if (instrument.Removeflag) continue;
				instruments.Add(instrument);
			}

			// There is a problem publishing a collection browse, it sometimes results in 
			// a right click crash so use queries to load the browse appropriately

			IQuery setInstrument = EntityManager.CreateQuery(InstrumentBase.EntityName);
			bool first = true;

			foreach (InstrumentBase instrument in instruments)
			{
				if (!first) setInstrument.AddOr();
				setInstrument.AddEquals(InstrumentPropertyNames.Identity, instrument.Identity);
				first = false;
			}

			setInstrument.AddDefaultOrder();
			m_BatchForm.InstrumentBrowse.Republish(setInstrument);

			// Drop out if the batch is not specified

			if (!BaseEntity.IsValid(m_BatchHeader))
			{
				m_BatchForm.Instrument.Entity = null;
				return;
			}

			// Default to the Batch Instrument or the first mapping found.

			var batchInstrument = m_BatchHeader.Instrument;

			if (BaseEntity.IsValid(batchInstrument))
			{
				m_BatchForm.Instrument.Entity = batchInstrument;
			}
			else if (instruments.Count > 0)
			{
				m_BatchForm.Instrument.Entity = instruments[0];
			}
		}

		#endregion

		#region Behaviour

		/// <summary>
		/// Handles the Loaded event of the Form control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		public void BatchFormLoaded(object sender, System.EventArgs e)
		{
			LoadContextData();
			LoadBatchData();

			m_BatchForm.CreateButton.Click += CreateButton_Click;
			m_BatchForm.Batch.EntityChanged += Batch_EntityChanged;
			m_BatchForm.Instrument.EntityChanged += Instrument_EntityChanged;
		}

		/// <summary>
		/// Handles the EntityChanged event of the Instrument control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.EntityChangedEventArgs"/> instance containing the event data.</param>
		private void Instrument_EntityChanged(object sender, SampleManager.Library.ClientControls.EntityChangedEventArgs e)
		{
			bool valid = BaseEntity.IsValid(m_BatchForm.Instrument.Entity);
			m_BatchForm.CreateButton.Enabled = valid;

			if (valid)
			{
				string summary = m_BatchForm.StringTable.SummaryFound;

				IQuery counter = EntityManager.CreateQuery(BatchEntryBase.EntityName);
				counter.AddEquals(BatchEntryPropertyNames.Identity, m_BatchHeader.Identity);
				int count = EntityManager.SelectCount(counter);

				summary = string.Format(summary, count);

				m_BatchForm.SummaryLabel.Caption = summary;
			}
			else
			{
				m_BatchForm.SummaryLabel.Caption = m_BatchForm.StringTable.SummaryNotFound;
			}
		}

		/// <summary>
		/// Handles the EntityChanged event of the Batch control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="SampleManager.Library.ClientControls.EntityChangedEventArgs"/> instance containing the event data.</param>
		private void Batch_EntityChanged(object sender, SampleManager.Library.ClientControls.EntityChangedEventArgs e)
		{
			m_BatchHeader = (BatchHeader) m_BatchForm.Batch.Entity;
			LoadBatchData();
		}

		/// <summary>
		/// Handles the Click event of the CreateButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void CreateButton_Click(object sender, System.EventArgs e)
		{
			m_Saving = true;
			m_BatchForm.Close();
		}

		#endregion

		#region Saving

		/// <summary>
		/// Handles the Closing event of the Form control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void BatchFormClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!m_Saving && m_BatchForm.FormResult != FormResult.OK) return;

			if (!BaseEntity.IsValid(m_BatchForm.Batch.Entity))
			{
				m_BatchForm.Batch.ShowError(m_BatchForm.StringTable.NoAnalysis);
				e.Cancel = true;
			}

			if (! BaseEntity.IsValid(m_BatchForm.Analysis.Entity))
			{
				m_BatchForm.Analysis.ShowError(m_BatchForm.StringTable.NoAnalysis);
				e.Cancel = true;
			}

			if (!BaseEntity.IsValid(m_BatchForm.Instrument.Entity))
			{
				m_BatchForm.Instrument.ShowError(m_BatchForm.StringTable.NoInstrument);
				e.Cancel = true;
			}

			// Mapping

			IQuery mapping = EntityManager.CreateQuery(ChromeleonMappingBase.EntityName);
			mapping.AddEquals(ChromeleonMappingPropertyNames.AnalysisId, m_BatchForm.Analysis.Entity);
			mapping.AddEquals(ChromeleonMappingPropertyNames.InstrumentId, m_BatchForm.Instrument.Entity);
			mapping.AddEquals(ChromeleonMappingPropertyNames.Removeflag, false);

			IEntityCollection matches = EntityManager.Select(ChromeleonMappingBase.EntityName, mapping);

			if (matches.Count == 0)
			{
				Library.Utils.FlashMessage(m_BatchForm.StringTable.NoMappingMessage, m_BatchForm.StringTable.NoMappingTitle);
				e.Cancel = true;
			}

			if (matches.Count >= 1)
			{
				m_Mapping = (ChromeleonMappingEntity) matches[0];
			}

			m_Saving = !e.Cancel;
		}

		/// <summary>
		/// Sample Form Closed - Save and Proxy to next task.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void BatchFormClosed(object sender, System.EventArgs e)
		{
			// Call the next task with the information.

			if (!m_Saving && m_BatchForm.FormResult != FormResult.OK) return;

			IEntityCollection batch = EntityManager.CreateEntityCollection(BatchHeaderBase.EntityName);
			batch.Add(m_BatchHeader);

			Library.Task.CreateTask("ChromeleonCreateSequenceTask", m_Mapping.Identity, "Create", BatchHeaderBase.EntityName, m_BatchHeader);

			// Exit the Task

			Exit();
		}

		#endregion
	}
}