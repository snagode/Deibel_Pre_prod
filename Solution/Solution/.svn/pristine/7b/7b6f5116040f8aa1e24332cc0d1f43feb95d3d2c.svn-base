using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Scheduled Group Sample Login - but for a given date range
	/// </summary>
	[SampleManagerTask("ScheduleGroupLoginTask", "GENERAL")]
	public class ScheduleGroupLoginTask : ScheduleLoginTask
	{
		#region Get Schedules

		/// <summary>
		/// Gets the schedules.
		/// </summary>
		protected override void GetSchedules()
		{
			m_Schedules = EntityManager.CreateEntityCollection(ScheduleBase.EntityName);

			if (Context.SelectedItems.Count == 0)
			{
				ScheduleGroup group = GetScheduleGroup();

				if (group != null)
					AddScheduleGroup(group);
			}
			else
			{
				foreach (ScheduleGroup group in Context.SelectedItems)
					AddScheduleGroup(group);
			}
		}

		/// <summary>
		/// Adds the schedules associated with the schedule group.
		/// </summary>
		/// <param name="group">The group.</param>
		private void AddScheduleGroup(ScheduleGroup group)
		{
			foreach (Schedule schedule in group.AssignedSchedules)
				m_Schedules.Add(schedule);
		}

		/// <summary>
		/// Gets the schedule.
		/// </summary>
		/// <returns></returns>
		private ScheduleGroup GetScheduleGroup()
		{
			IEntity schedule;
			
			string title = m_Form.StringTable.PromptGroupTitleSchedule;
			string message = m_Form.StringTable.PromptGroupMessageSchedule;

			FormResult result = Library.Utils.PromptForEntity( title, message,
			                                                   ScheduleGroupBase.EntityName, out schedule);

			if (result == FormResult.OK)
				return (ScheduleGroup) schedule;

			return null;
		}

		#endregion
	}
}