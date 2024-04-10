using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Calendar server side task
	/// </summary>
	[SampleManagerTask("ScheduleCalendarTask", "LABTABLE", "SCHEDULE_CALENDAR")]
	public class ScheduleCalendarTask : GenericLabtableTask
	{
		#region Member Variables

		private ScheduleCalendar m_CalendarEntity;
		private FormScheduleCalendar m_Form;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormScheduleCalendar) MainForm;
			m_CalendarEntity = (ScheduleCalendar) m_Form.Entity;
		}

		/// <summary>
		/// Called before the property sheet or wizard is saved.
		/// </summary>
		/// <returns></returns>
		protected override bool OnPreSave()
		{
			bool periodsAreValid = m_CalendarEntity.ValidatePeriods();

			if (!periodsAreValid)
			{
				Library.Utils.FlashMessage(m_Form.StringTable.DateMissingError, m_Form.Title);
				return false;
			}

			return base.OnPreSave();
		}

		#endregion
	}
}