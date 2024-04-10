using System;
using System.Collections.Generic;
using System.Text;
using Thermo.Framework.Server;
using Thermo.Framework.Utilities;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks.BusinessObjects
{
	/// <summary>
	/// Scheduled Point Event
	/// </summary>
	public class SchedulePointEvent : SchedulePointEventBase
	{
		#region Member Variables

		private readonly IList<ISchedulePointItem> m_Items;
		private readonly SchedulePoint m_SchedulePoint;
		private DateTime m_LoginEvent;
		private DateTime m_SamplingEvent;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="SchedulePointEvent"/> class.
		/// </summary>
		/// <param name="schedulePoint">The sample point entry.</param>
		/// <param name="loginEvent">The login event.</param>
		/// <param name="samplingEvent">The sampling event.</param>
		public SchedulePointEvent(SchedulePoint schedulePoint, DateTime loginEvent, DateTime samplingEvent)
		{
			m_SchedulePoint = schedulePoint;
			m_LoginEvent = loginEvent;
			m_SamplingEvent = samplingEvent;
			m_Items = new List<ISchedulePointItem>();
			UpdateFields(schedulePoint, SampleBase.StructureTableName);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the sample point entry.
		/// </summary>
		/// <value>The sample point entry.</value>
		public SchedulePoint SchedulePoint
		{
			get { return m_SchedulePoint; }
		}

		/// <summary>
		/// Gets the sample point analyses.
		/// </summary>
		/// <value>The sample point analyses.</value>
		public IList<ISchedulePointItem> Items
		{
			get { return m_Items; }
		}

		/// <summary>
		/// Gets or sets the sampling event.
		/// </summary>
		/// <value>The sampling event.</value>
		public DateTime SamplingEvent
		{
			get { return m_SamplingEvent; }
			set { m_SamplingEvent = value; }
		}

		/// <summary>
		/// Gets or sets the login event.
		/// </summary>
		/// <value>The login event.</value>
		public DateTime LoginEvent
		{
			get { return m_LoginEvent; }
			set { m_LoginEvent = value; }
		}

		#endregion

		#region Comments

		/// <summary>
		/// Gets the comment text.
		/// </summary>
		/// <returns></returns>
		public string GetSubjectText()
		{
			if (SchedulePoint == null) return string.Empty;

			string format = ServerMessageManager.Current.GetMessage("LaboratoryMessages", "SchedulePointEventSubjectFormat");
			return string.Format(format, SamplingEvent, SchedulePoint.SchedulePointName);
		}

		/// <summary>
		/// Gets the description text.
		/// </summary>
		/// <returns></returns>
		public string GetDescriptionText()
		{
			StringBuilder builder = new StringBuilder();

			foreach (ISchedulePointItem item in m_Items)
			{
				builder.AppendLine(item.GetCommentText());
			}

			return builder.ToString();
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			string sched = SchedulePoint.Schedule.ScheduleName;
			string point = SchedulePoint.SamplePoint.SamplePointName;

			if (LoginEvent != SamplingEvent)
			{
				string format = ServerMessageManager.Current.GetMessage("LaboratoryMessages", "SchedulePointEventFormat");
				return string.Format(format, sched, point, LoginEvent, SamplingEvent);
			}

			string format2 = ServerMessageManager.Current.GetMessage("LaboratoryMessages", "SchedulePointEventFormat2");
			return string.Format(format2, sched, point, LoginEvent);
		}

		#endregion
	}
}