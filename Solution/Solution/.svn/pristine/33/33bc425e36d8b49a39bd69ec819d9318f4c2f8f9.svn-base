using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.ImportExport;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the TEST_SCHED_ENTRY entity.
	/// </summary>
	/// <remarks>
	/// Extensions within this class are required to support existing VGL data structures.
	/// </remarks>
	[SampleManagerEntity(EntityName)]
	public class TestSchedEntry : TestSchedEntryBase
	{
		#region Member Variables

		private PhraseBase m_EntryTypeAnalysis;
		private PhraseBase m_EntryTypeSchedule;
		private TestSchedHeader m_Schedule;
		private VersionedAnalysis m_Analysis;

		#endregion

		#region Setup

		/// <summary>
		/// Perform post creation processing.
		/// </summary>
		protected override void OnEntityCreated()
		{
			base.OnEntityCreated();

			IsAnalysis = true;
		}

		/// <summary>
		/// Perform post creation processing.
		/// </summary>
		protected override void OnEntityLoaded()
		{
			base.OnEntityLoaded();

			// Load the available phrase types (Analysis and Schedule)
			m_EntryTypeAnalysis = (PhraseBase)EntityManager.SelectPhrase(PhraseSchdEnTy.Identity, PhraseSchdEnTy.PhraseIdANALYSIS);
			m_EntryTypeSchedule = (PhraseBase)EntityManager.SelectPhrase(PhraseSchdEnTy.Identity, PhraseSchdEnTy.PhraseIdSCHEDULE);
		}

		#endregion

		#region Property Changes

		/// <summary>
		/// Called when when a property changes.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged(PropertyEventArgs e)
		{
			if (e.PropertyName == TestSchedEntryPropertyNames.InstrumentType)
			{
				// Reset the Instrument when the Instrument ID changes
				base.InstrumentId = null;
			}
		}

		#endregion

		#region Virtual Properties

		/// <summary>
		/// Gets the display name.
		/// </summary>
		/// <value>The display name.</value>
		[PromptText]
		public string DisplayName
		{
			get
			{
				if (IsAnalysis)
				{
					// An Analysis is assigned, return it's name
					if (IsValid(Analysis))
					{
						return Analysis.VersionedAnalysisName;
					}

					return string.Empty;
				}

				// A Schedule is assigned, return it's name

				if (IsValid(Schedule))
				{
					return Schedule.Name;
				}

				return string.Empty;
			}
		}

		/// <summary>
		/// Gets or sets the type of the entry.
		/// </summary>
		/// <value>The type of the entry.</value>
		[PromptPhrase(PhraseSchdEnTy.Identity, false, true, false)]
		public PhraseBase EntryType
		{
			get
			{
				return IsAnalysis ? m_EntryTypeAnalysis : m_EntryTypeSchedule;
			}
			set
			{
				if (IsValid(value))
				{
					IsAnalysis = value.PhraseId == PhraseSchdEnTy.PhraseIdANALYSIS;

					NotifyPropertyChanged("EntryType");
				}				
			}
		}

		/// <summary>
		/// Gets or sets the schedule.
		/// </summary>
		/// <value>The schedule.</value>
		[PromptLink(VersionedAnalysis.EntityName)]
		public VersionedAnalysis Analysis
		{
			get
			{
				if (IsAnalysis)
				{
					if (m_Analysis == null && !string.IsNullOrEmpty(AnalysisId))
					{
						// Select the Analysis
						m_Analysis = (VersionedAnalysis)EntityManager.SelectLatestVersion(VersionedAnalysis.EntityName, new Identity(AnalysisId));
					}

					return m_Analysis;
				}

				// This entry is a Schedule
				return null;
			}
			set
			{
				if (!IsAnalysis)
				{
					throw new SampleManagerError(Library.Message.GetMessage("LaboratoryMessages", "TestSchedAssignAnalysisError"));	
				}

				m_Analysis = value;

				if (m_Analysis != null)
					AnalysisId = m_Analysis.Identity;
				else
					AnalysisId = string.Empty;

				NotifyPropertyChanged("Analysis");
			}
		}

		/// <summary>
		/// Gets or sets the schedule.
		/// </summary>
		/// <value>The schedule.</value>
		[PromptLink(TestSchedHeader.EntityName)]
		public TestSchedHeader Schedule
		{
			get
			{
				if (!IsAnalysis)
				{
					if (m_Schedule == null && !string.IsNullOrEmpty(AnalysisId))
					{
						// Select the Schedule
						m_Schedule = (TestSchedHeader)EntityManager.Select(TestSchedHeader.EntityName, new Identity(AnalysisId));
					}

					return m_Schedule;
				}

				// This entry is an Analysis
				return null;
			}
			set
			{
				if (IsAnalysis)
				{
					throw new SampleManagerError(Library.Message.GetMessage("LaboratoryMessages", "TestSchedAssignScheduleError"));
				}

				m_Schedule = value;

				if (m_Schedule != null)
				{
					// Set the AnalysisId field to the Identity of the assigned Test Schedule
					AnalysisId = m_Schedule.Identity;
				}
				else
				{
					AnalysisId = string.Empty;
				}

				NotifyPropertyChanged("Schedule");
			}
		}

		#endregion

		#region Export

		/// <summary>
		/// Gets the Properties that must be processed on the model.
		/// </summary>
		/// <returns></returns>
		public override List<string> GetCustomExportableProperties()
		{
			List<string> properties = base.GetCustomExportableProperties();
			properties.Add(TestSchedEntryPropertyNames.AnalysisId);
			return properties;
		}

		/// <summary>
		/// Gets Property's value linked data.
		/// </summary>
		/// <param name="propertyName">The property name to process</param>
		/// <param name="exportList">The Entity Export List</param>
		public override void GetLinkedData(string propertyName, EntityExportList exportList)
		{
			if (propertyName == TestSchedEntryPropertyNames.AnalysisId)
			{
				if (IsAnalysis)
				{
					exportList.AddEntity(Analysis);
				}
				else
				{
					exportList.AddEntity(Schedule);
				}
			}

			base.GetLinkedData(propertyName,exportList);
		}

		#endregion
	}
}
