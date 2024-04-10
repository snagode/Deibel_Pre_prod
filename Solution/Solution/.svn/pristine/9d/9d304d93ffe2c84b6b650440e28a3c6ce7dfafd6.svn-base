using System;
using System.Reflection;
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
	/// Chromeleon Sequence Task
	/// </summary>
	[SampleManagerTask("ChromeleonCreateSequenceByTestsTask")]
	public class ChromeleonCreateSequenceByTestsTask : SampleManagerTask
	{
		#region Member Variables

		private FormChromeleonSequenceFromTests m_TestsForm;
		private IEntityCollection m_Analyses;
		private IEntityCollection m_Tests;
		private IEntityCollection m_SelectedTests;
		private VersionedAnalysis m_CurrentAnalysis;
		private ChromeleonMappingEntity m_Mapping;
		private bool m_Saving;

		#endregion

		#region Setup

		/// <summary>
		/// Setup the task.
		/// </summary>
		protected override void SetupTask()
		{
			base.SetupTask();

			m_Analyses = EntityManager.CreateEntityCollection(VersionedAnalysisBase.EntityName);

			// Start the form

			m_TestsForm = FormFactory.CreateForm<FormChromeleonSequenceFromTests>();

			m_TestsForm.Loaded += TestsFormLoaded;
			m_TestsForm.Closing += TestsFormClosing;
			m_TestsForm.Closed += TestsFormClosed;

			m_TestsForm.Show(FormDisplayStyle.Default);
		}

		#endregion

		#region Get Data

		/// <summary>
		/// Loads the sample data.
		/// </summary>
		private void LoadSampleData()
		{
			m_SelectedTests = EntityManager.CreateEntityCollection(TestBase.EntityName);
			m_Tests = EntityManager.CreateEntityCollection(TestBase.EntityName);

			if (Context.SelectedItems != null)
			{
				foreach (TestBase test in Context.SelectedItems)
				{
					m_SelectedTests.Add(test);
				}
			}
		}

		/// <summary>
		/// Gets the data for browses
		/// </summary>
		private void LoadAnalysisData()
		{
			m_Analyses = EntityManager.CreateEntityCollection(VersionedAnalysisBase.EntityName);
			IEntityCollection potentials = EntityManager.CreateEntityCollection(VersionedAnalysisBase.EntityName);

			// Work out which analyses are in play

			foreach (TestBase test in m_SelectedTests.ActiveItems)
			{
				if (!test.Status.IsPhrase(PhraseTestStat.PhraseIdV)) continue;

				if (!potentials.Contains(test.Analysis))
				{
					potentials.Add(test.Analysis);
				}
			}

			// Just keep track of the mapped analyses

			foreach (VersionedAnalysis potential in potentials)
			{
				IQuery checkMapping = EntityManager.CreateQuery(ChromeleonMappingBase.EntityName);
				checkMapping.AddEquals(ChromeleonMappingPropertyNames.AnalysisId, potential.Identity);
				if (EntityManager.SelectCount(checkMapping) > 0)
				{
					m_Analyses.Add(potential);
				}
			}

			// There is a problem publishing a collection browse, it sometimes results in 
			// a right click crash so use queries to load the browse appropriately

			if (m_Analyses.Count == 0)
			{
				IQuery noAnalysis = EntityManager.CreateQuery(VersionedAnalysisBase.EntityName);
				noAnalysis.AddEquals(VersionedAnalysisPropertyNames.Identity, string.Empty);
				m_TestsForm.AnalysisBrowse.Republish(noAnalysis);
			}
			else
			{
				IQuery setAnalysis = EntityManager.CreateQuery(VersionedAnalysisBase.EntityName);
				bool first = true;

				foreach (VersionedAnalysisBase analysis in m_Analyses)
				{
					if (!first) setAnalysis.AddOr();
					setAnalysis.AddEquals(VersionedAnalysisPropertyNames.Identity, analysis.Identity);
					first = false;
				}

				setAnalysis.AddDefaultOrder();
				m_TestsForm.AnalysisBrowse.Republish(setAnalysis);
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
				m_TestsForm.InstrumentBrowse.Republish(noInstruments);

				return;
			}

			IQuery matches = EntityManager.CreateQuery(ChromeleonMappingBase.EntityName);
			matches.AddEquals(ChromeleonMappingPropertyNames.AnalysisId, analysis.Identity);
			matches.AddEquals(ChromeleonMappingPropertyNames.Removeflag, false);

			var matchingMaps = EntityManager.Select(ChromeleonMappingBase.EntityName, matches);

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
			m_TestsForm.InstrumentBrowse.Republish(setInstrument);

			// Default the first mapping found.

			if (instruments.Count > 0)
			{
				m_TestsForm.Instrument.Entity = instruments[0];
			}
		}

		#endregion

		#region Behaviour

		/// <summary>
		/// Handles the Loaded event of the Form control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		public void TestsFormLoaded(object sender, EventArgs e)
		{
			LoadSampleData();
			LoadAnalysisData();

			m_TestsForm.Tests.Publish(m_SelectedTests);

			m_TestsForm.Analysis.EntityChanged += Analysis_EntityChanged;
			m_TestsForm.TestsGrid.BeforeRowAdd += TestsGrid_BeforeRowAdd;

			m_SelectedTests.ItemReordered += Tests_ItemReordered;
			m_SelectedTests.ItemRemoved += Tests_ItemRemoved;

			m_TestsForm.CreateButton.Click += CreateButton_Click;

			DefaultFirst();
		}

		/// <summary>
		/// Handles the Click event of the CreateButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void CreateButton_Click(object sender, EventArgs e)
		{
			m_Saving = true;
			m_TestsForm.Close();
		}

		/// <summary>
		/// Defaults first
		/// </summary>
		private void DefaultFirst()
		{
			if (m_Analyses.Count > 0)
			{
				m_TestsForm.Analysis.Entity = m_Analyses[0];
			}
		}

		/// <summary>
		/// Handles the ItemRemoved event of the Tests control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void Tests_ItemRemoved(object sender, EntityCollectionEventArgs e)
		{
			FindTests();
		}

		/// <summary>
		/// Handles the ItemReordered event of the Tests control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EntityCollectionReorderEventArgs"/> instance containing the event data.</param>
		private void Tests_ItemReordered(object sender, EntityCollectionReorderEventArgs e)
		{
			FindTests();
		}

		/// <summary>
		/// Handles the BeforeRowAdd event of the TestsGrid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="SampleManager.Library.ClientControls.BeforeRowAddedEventArgs"/> instance containing the event data.</param>
		private void TestsGrid_BeforeRowAdd(object sender, SampleManager.Library.ClientControls.BeforeRowAddedEventArgs e)
		{
			IQuery testQuery = EntityManager.CreateQuery(TestBase.EntityName);
			testQuery.AddEquals(TestPropertyNames.Status, PhraseTestStat.PhraseIdV);
			IEntity test;

			if (Library.Utils.PromptForEntity(m_TestsForm.StringTable.AddTestMessage,
				m_TestsForm.StringTable.AddTestTitle, testQuery, out test) == FormResult.OK)
			{
				if (m_SelectedTests.ActiveItems.Contains(test))
				{
					Library.Utils.FlashMessage(m_TestsForm.StringTable.TestExistsMessage,
					                           m_TestsForm.StringTable.TestExistsTitle);
				}
				else
				{
					AddRestoreEntity(m_SelectedTests, test);

					// See if have any new potential analyses

					LoadAnalysisData();

					// Default the first choice if appropriate.

					if (!BaseEntity.IsValid(m_CurrentAnalysis))
					{
						DefaultFirst();
					}
					else
					{
						FindTests();
					}
				}
			}

			e.Cancel = true;
		}

		/// <summary>
		/// Handles the EntityChanged event of the Analysis control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="SampleManager.Library.ClientControls.EntityChangedEventArgs"/> instance containing the event data.</param>
		private void Analysis_EntityChanged(object sender, SampleManager.Library.ClientControls.EntityChangedEventArgs e)
		{
			if (m_CurrentAnalysis != null && m_CurrentAnalysis.Equals(e.Entity)) return;

			m_CurrentAnalysis = (VersionedAnalysis) e.Entity;
			LoadInstrumentBrowse(m_CurrentAnalysis);
			FindTests();
		}

		#endregion

		#region Tests

		/// <summary>
		/// Finds the tests.
		/// </summary>
		private void FindTests()
		{
			// Find Available tests for the specified analysis

			m_Tests.ReleaseAll();

			if (BaseEntity.IsValid(m_CurrentAnalysis))
			{
				foreach (TestBase test in m_SelectedTests.ActiveItems)
				{
					if (!BaseEntity.IsValid(test.Analysis)) continue;
					if (test.Analysis.Equals(m_CurrentAnalysis))
					{
						m_Tests.Add(test);
					}
				}
			}

			// Update the summary

			if (m_Tests.Count > 0 && BaseEntity.IsValid(m_TestsForm.Instrument.Entity))
			{
				m_TestsForm.SummaryLabel.Caption = string.Format(m_TestsForm.StringTable.SummaryFound, m_Tests.Count);
				m_TestsForm.CreateButton.Enabled = true;
			}
			else if (!BaseEntity.IsValid(m_TestsForm.Instrument.Entity))
			{
				m_TestsForm.SummaryLabel.Caption = string.Format(m_TestsForm.StringTable.SummaryFoundNoInstrument, m_Tests.Count);
				m_TestsForm.CreateButton.Enabled = false;
			}
			else
			{
				m_TestsForm.SummaryLabel.Caption = m_TestsForm.StringTable.SummaryNotFound;
				m_TestsForm.CreateButton.Enabled = false;
			}
		}

		#endregion

		#region Saving

		/// <summary>
		/// Handles the Closing event of the Form control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void TestsFormClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!m_Saving && m_TestsForm.FormResult != FormResult.OK) return;
			if (! BaseEntity.IsValid(m_TestsForm.Analysis.Entity))
			{
				m_TestsForm.Analysis.ShowError(m_TestsForm.StringTable.NoAnalysis);
				e.Cancel = true;
			}

			if (!BaseEntity.IsValid(m_TestsForm.Instrument.Entity))
			{
				m_TestsForm.Instrument.ShowError(m_TestsForm.StringTable.NoInstrument);
				e.Cancel = true;
			}

			if (m_Tests == null || m_Tests.ActiveCount == 0)
			{
				e.Cancel = true;
				Library.Utils.FlashMessage(m_TestsForm.StringTable.NoTestsMessage, m_TestsForm.StringTable.NoTestsTitle);
				return;
			}

			// Mapping

			IQuery mapping = EntityManager.CreateQuery(ChromeleonMappingBase.EntityName);
			mapping.AddEquals(ChromeleonMappingPropertyNames.AnalysisId, m_TestsForm.Analysis.Entity);
			mapping.AddEquals(ChromeleonMappingPropertyNames.InstrumentId, m_TestsForm.Instrument.Entity);
			mapping.AddEquals(ChromeleonMappingPropertyNames.Removeflag, false);

			IEntityCollection matches = EntityManager.Select(ChromeleonMappingBase.EntityName, mapping);

			if (matches.Count == 0)
			{
				Library.Utils.FlashMessage(m_TestsForm.StringTable.NoMappingMessage, m_TestsForm.StringTable.NoMappingTitle);
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
		private void TestsFormClosed(object sender, EventArgs e)
		{
			IEntityCollection tests = EntityManager.CreateEntityCollection(TestBase.EntityName);
			foreach (var item in m_Tests.ActiveItems)
			{
				tests.Add(item);
			}

			// Throw away the intermediates

			m_SelectedTests.ReleaseAll();
			m_Analyses.ReleaseAll();
			m_Tests.ReleaseAll();

			// Call the next task with the information.

			if (!m_Saving && m_TestsForm.FormResult != FormResult.OK) return;
			Library.Task.CreateTask("ChromeleonCreateSequenceTask", m_Mapping.Identity, "Create", TestBase.EntityName, tests);

			// Exit the Task

			Exit();
		}

		#endregion

		#region Add/Restore an Entity

		/// <summary>
		/// Adds/restores an entity.
		/// </summary>
		/// <param name="collection">The collection.</param>
		/// <param name="entity">The entity.</param>
		private void AddRestoreEntity(IEntityCollection collection, IEntity entity)
		{
			if (collection.ActiveItems.Contains(entity)) return;

			// Add/Restore a record

			if (collection.DeletedItems.Contains(entity))
			{
				EntityManager.Reselect(entity);

				// Sneaky - using protected methods. Obviously not supported.

				try
				{
					MethodInfo addMethod = (collection.GetType()).GetMethod("OnItemAdded", BindingFlags.Instance | BindingFlags.NonPublic);
					addMethod.Invoke(collection, new object[] { entity, -1 });

					MethodInfo changeMethod = (collection.GetType()).GetMethod("OnChanged", BindingFlags.Instance | BindingFlags.NonPublic);
					changeMethod.Invoke(collection, null);
				}
				catch (Exception ex)
				{
					Logger.Debug("Force Collection Add Events error", ex);
				}
			}

			collection.Add(entity);
		}

		#endregion
	}
}