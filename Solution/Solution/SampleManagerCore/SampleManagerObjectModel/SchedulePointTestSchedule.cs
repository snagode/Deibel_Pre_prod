using System.Collections.Generic;
using System.Text;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Utilities;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the SCHEDULE_POINT_TEST_SCHEDULE entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class SchedulePointTestSchedule : SchedulePointTestScheduleBase, ISchedulePointItem
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
			else if (e.PropertyName == SchedulePointTestSchedulePropertyNames.TestSchedule)
			{
				SchedulePointTestScheduleName = TestSchedule.TestSchedHeaderName;
				NotifyPropertyChanged(SchedulePointTestSchedulePropertyNames.SchedulePointTestScheduleName);
			}
		}

		#endregion

		#region Properties

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
			get { return (SchedulePoint)base.SchedulePoint; }
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
			StringBuilder builder = new StringBuilder();

			if (TestSchedule == null) return string.Empty;

			string format = Library.Message.GetMessage("LaboratoryMessages", "SchedulePointTestScheduleFormat");

			builder.AppendFormat(format, Name);
			builder.AppendLine();
			builder.AppendLine();

			foreach (ISchedulePointEventTest detail in GetTestDetails())
			{
				builder.AppendLine(detail.ToString());
			}

			return builder.ToString();
		}

		/// <summary>
		/// Gets the test details.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<ISchedulePointEventTest> GetTestDetails()
		{
			List<ISchedulePointEventTest> details = new List<ISchedulePointEventTest>();

			foreach (TestSchedEntryBase entry in TestSchedule.TestSchedEntries)
			{
				details.Add(GetTestDetail(entry));
			}

			return details;
		}

		/// <summary>
		/// Gets the test detail.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <returns></returns>
		private static SchedulePointEventTest GetTestDetail(TestSchedEntryBase entry)
		{
			string name = entry.AnalysisId;
			string analysis = entry.AnalysisId;
			string complist = entry.ComponentList;
			int replicates = entry.ReplicateCount;

			SchedulePointEventTest detail = new SchedulePointEventTest(name, analysis, complist, replicates);
			detail.UpdateFields(entry);
			return detail;
		}

		#endregion
	}
}