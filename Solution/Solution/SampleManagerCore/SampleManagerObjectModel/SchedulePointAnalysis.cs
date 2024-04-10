using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Utilities;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the SAMPLE_POINT_ANALYSIS entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class SchedulePointAnalysis : SchedulePointAnalysisBase, ISchedulePointItem
	{
		#region Member Variables

		private Frequency m_FrequencyObject;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when when a property changes.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged(PropertyEventArgs e)
		{
			base.OnPropertyChanged(e);

			if (e.PropertyName == SchedulePointAnalysisPropertyNames.Frequency)
			{
				try
				{
					m_FrequencyObject = (string.IsNullOrEmpty(Frequency)) ? new Frequency() : new Frequency(Frequency);
				}
				catch (InvalidFrequencyException)
				{
					m_FrequencyObject = null;
				}
			}
			else if (e.PropertyName == SchedulePointAnalysisPropertyNames.Analysis)
			{
				string analysisName = Analysis;
				ComponentList = null;

				if (!string.IsNullOrEmpty(Analysis))
				{
					VersionedAnalysis analysis = (VersionedAnalysis) EntityManager.SelectLatestVersion(VersionedAnalysis.EntityName, new Identity(Analysis));

					if ( analysis != null )
					{
						analysisName = analysis.VersionedAnalysisName;
					}
				}

				SchedulePointAnalysisName = analysisName;

				NotifyPropertyChanged(SchedulePointAnalysisPropertyNames.SchedulePointAnalysisName);
				NotifyPropertyChanged(SchedulePointAnalysisPropertyNames.ComponentList);
			}
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the frequency object.
		/// </summary>
		/// <value>The frequency object.</value>
		public Frequency FrequencyObject
		{
			get
			{
				if (IsLoaded() && (m_FrequencyObject == null))
				{
					m_FrequencyObject = (string.IsNullOrEmpty(Frequency)) ? new Frequency() : new Frequency(Frequency);
				}

				return m_FrequencyObject;
			}
		}

		#endregion

		#region Schedule Point Item Implementation

		/// <summary>
		/// Maps to Parent SchedulePoint
		/// </summary>
		/// <value></value>
		SchedulePoint ISchedulePointItem.SchedulePoint
		{
			get { return (SchedulePoint) base.SchedulePoint; }
		}

		/// <summary>
		/// Gets the test details.
		/// </summary>
		/// <value>The test details.</value>
		IEnumerable<ISchedulePointEventTest> ISchedulePointItem.TestDetails
		{
			get { return GetTestDetails(); }
		}

		/// <summary>
		/// Gets the comment text.
		/// </summary>
		/// <returns></returns>
		string ISchedulePointItem.GetCommentText()
		{
			ISchedulePointEventTest test = GetTestDetail();
			return test.ToString();
		}

		/// <summary>
		/// Gets the test details.
		/// </summary>
		/// <returns></returns>
		private IList<ISchedulePointEventTest> GetTestDetails()
		{
			List<ISchedulePointEventTest> tests = new List<ISchedulePointEventTest>();
			ISchedulePointEventTest test = GetTestDetail();
			tests.Add(test);
			return tests;
		}

		/// <summary>
		/// Gets the test detail.
		/// </summary>
		/// <returns></returns>
		private ISchedulePointEventTest GetTestDetail()
		{
			SchedulePointEventTest test = new SchedulePointEventTest(Name, Analysis, ComponentList, ReplicateCount);
			test.UpdateFields(this);
			return test;
		}

		#endregion
	}
}