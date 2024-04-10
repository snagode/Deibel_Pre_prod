using System;
using System.Drawing;
using System.Globalization;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the SCHEDULE entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class Schedule : ScheduleBase
	{
		#region Public Constants

		/// <summary>
		/// Group Control Enabled Property Name
		/// </summary>
		public const string PropertyGroupControlEnabled = "GroupControlEnabled";

		/// <summary>
		/// Locally Managed Property Name
		/// </summary>
		public const string PropertyLocallyManaged = "LocallyControlled";

		/// <summary>
		/// Schedule Status Property Name
		/// </summary>
		public const string PropertyStatus = "Status";

		#endregion

		#region Member Variables

		private PhraseBase m_Status;

		#endregion

		#region Color Faffing

		private static readonly KnownColor[] PointColors = new KnownColor[]
		                                                   {
		                                                   	KnownColor.SkyBlue, KnownColor.LightYellow,
		                                                   	KnownColor.LavenderBlush, KnownColor.SeaGreen,
		                                                   	KnownColor.Honeydew,
		                                                   	KnownColor.Khaki, KnownColor.Azure, KnownColor.Firebrick,
		                                                   	KnownColor.LemonChiffon, KnownColor.MediumPurple,
		                                                   	KnownColor.Olive, KnownColor.MintCream, KnownColor.LightSalmon,
		                                                   	KnownColor.Maroon
		                                                   };

		/// <summary>
		/// Called after the Entity is Loaded
		/// </summary>
		protected override void OnEntityLoaded()
		{
			SchedulePoints.Loaded -= new EntityCollectionEventHandler(SchedulePointsLoaded);
			SchedulePoints.ItemAdded -= new EntityCollectionEventHandler(SchedulePointsItemAdded);

			SchedulePoints.Loaded += new EntityCollectionEventHandler(SchedulePointsLoaded);
			SchedulePoints.ItemAdded += new EntityCollectionEventHandler(SchedulePointsItemAdded);
		}

		/// <summary>
		/// Schedules the points item added.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void SchedulePointsItemAdded(object sender, EntityCollectionEventArgs e)
		{
			SetColors();
		}

		/// <summary>
		/// Schedules the points loaded.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void SchedulePointsLoaded(object sender, EntityCollectionEventArgs e)
		{
			SetColors();
		}

		/// <summary>
		/// Sets the colors.
		/// </summary>
		private void SetColors()
		{
			int i = 0;
			foreach (SchedulePoint point in SchedulePoints)
			{
				if (i < PointColors.GetUpperBound(0))
					point.Color = Color.FromKnownColor(PointColors[i]);
				else
					point.Color = GetRandomColor();

				i++;
			}
		}

		/// <summary>
		/// Gets the random color.
		/// </summary>
		/// <returns></returns>
		private Color GetRandomColor()
		{
			byte a = Convert.ToByte(Library.Utils.Random(100, 255));
			byte r = Convert.ToByte(Library.Utils.Random(0, 255));
			byte g = Convert.ToByte(Library.Utils.Random(0, 255));
			byte b = Convert.ToByte(Library.Utils.Random(0, 255));

			return Color.FromArgb(a, r, g, b);
		}

		#endregion

		#region Property Changed Events

		/// <summary>
		/// Called when when a property changes.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged(PropertyEventArgs e)
		{
			if (e.PropertyName == SchedulePropertyNames.ScheduleGroup ||
			    e.PropertyName == SchedulePropertyNames.ScheduleGroupControlled)
			{
				RefreshStatus();
				NotifyPropertyChanged(PropertyStatus);
				NotifyPropertyChanged(SchedulePropertyNames.StartDate);
				NotifyPropertyChanged(SchedulePropertyNames.EndDate);
				NotifyPropertyChanged(SchedulePropertyNames.Calendar);
				NotifyPropertyChanged(PropertyLocallyManaged);
				NotifyPropertyChanged(PropertyGroupControlEnabled);
			}
			else if (e.PropertyName == SchedulePropertyNames.StartDate ||
			         e.PropertyName == SchedulePropertyNames.EndDate ||
			         e.PropertyName == SchedulePropertyNames.Active)
			{
				RefreshStatus();
				NotifyPropertyChanged(PropertyStatus);
			}
		}

		/// <summary>
		/// Update the Timerqueue
		/// </summary>
		protected override void OnPreCommit()
		{
			UpdateTimerQueue(EntityManager, Library.Environment.ClientNow.ToDateTime(CultureInfo.CurrentCulture));
		}

		#endregion

		#region Status Indication

		/// <summary>
		/// Gets the current status of the schedule based on Start and End Dates.
		/// </summary>
		/// <value>The schedule status.</value>
		[PromptValidPhrase(PhraseSchedStat.Identity)]
		public PhraseBase Status
		{
			get
			{
				if (m_Status == null)
					RefreshStatus();

				return m_Status;
			}
		}

		/// <summary>
		/// Refreshes the status.
		/// </summary>
		private void RefreshStatus()
		{
			if (!Active)
			{
				SetStatus(PhraseSchedStat.PhraseIdI);
				return;
			}

			if (Running)
			{
				SetStatus(PhraseSchedStat.PhraseIdR);
				return;
			}

			if (Ended)
			{
				SetStatus(PhraseSchedStat.PhraseIdC);
				return;
			}

			SetStatus(PhraseSchedStat.PhraseIdP);
		}

		/// <summary>
		/// Sets the status.
		/// </summary>
		/// <param name="phraseId">The phrase id.</param>
		private void SetStatus(string phraseId)
		{
			m_Status = (Phrase) EntityManager.SelectPhrase(PhraseSchedStat.Identity, phraseId);
		}

		#endregion

		#region Schedule Group Support

		/// <summary>
		/// Structure Field END_DATE
		/// </summary>
		/// <value></value>
		[PromptDate]
		public override NullableDateTime EndDate
		{
			get
			{
				if (ScheduleGroupControlled) return ScheduleGroup.EndDate;
				return base.EndDate;
			}
			set { base.EndDate = value; }
		}

		/// <summary>
		/// Structure Field START_DATE
		/// </summary>
		/// <value></value>
		[PromptDate]
		public override NullableDateTime StartDate
		{
			get
			{
				if (ScheduleGroupControlled) return ScheduleGroup.StartDate;
				return base.StartDate;
			}
			set { base.StartDate = value; }
		}

		/// <summary>
		/// Gets a value indicating whether this schedule is group managed.
		/// </summary>
		/// <value><c>true</c> if group managed; otherwise, <c>false</c>.</value>
		[PromptBoolean]
		public override bool ScheduleGroupControlled
		{
			get
			{
				if (ScheduleGroup.IsNull()) return false;
				return ScheduleGroup.ForceScheduleControl || base.ScheduleGroupControlled;
			}
			set
			{
				if (!GroupControlEnabled) return;
				base.ScheduleGroupControlled = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this is locally managed.
		/// </summary>
		/// <value><c>true</c> if locally managed; otherwise, <c>false</c>.</value>
		[PromptBoolean]
		public bool LocallyControlled
		{
			get { return !ScheduleGroupControlled; }
		}

		/// <summary>
		/// Links to Type CalendarBase
		/// </summary>
		/// <value></value>
		[PromptLink(ScheduleCalendarBase.EntityName)]
		public override ScheduleCalendarBase Calendar
		{
			get
			{
				if (ScheduleGroupControlled) return ScheduleGroup.Calendar;
				return base.Calendar;
			}
			set { base.Calendar = value; }
		}

		/// <summary>
		/// Gets a value indicating whether group control is enabled
		/// </summary>
		/// <value><c>true</c> if not force group managed; otherwise, <c>false</c>.</value>
		[PromptBoolean]
		public bool GroupControlEnabled
		{
			get
			{
				if (ScheduleGroup.IsNull()) return false;
				return !ScheduleGroup.ForceScheduleControl;
			}
		}

		#endregion

		#region Activity Periods

		/// <summary>
		/// Is this Effectively Active
		/// </summary>
		/// <value></value>
		[PromptBoolean]
		public bool IsActive
		{
			get
			{
				if (ScheduleGroupControlled) return ScheduleGroup.Active;
				return base.Active;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="Schedule"/> is started.
		/// </summary>
		/// <value><c>true</c> if started; otherwise, <c>false</c>.</value>
		public bool Started
		{
			get { return StartsBefore(DateTime.Now); }
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="Schedule"/> is ended.
		/// </summary>
		/// <value><c>true</c> if ended; otherwise, <c>false</c>.</value>
		public bool Ended
		{
			get { return !EndsAfter(DateTime.Now); }
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="Schedule"/> is running.
		/// </summary>
		/// <value><c>true</c> if running; otherwise, <c>false</c>.</value>
		public bool Running
		{
			get { return Started && !Ended; }
		}

		/// <summary>
		/// Does the schedule start on or before the passed in time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
		public bool StartsBefore(DateTime time)
		{
			if (StartDate.IsNull) return false;
			return (StartDate.Value <= time);
		}

		/// <summary>
		/// Does the schedule end on or after the passed in time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
		public bool EndsAfter(DateTime time)
		{
			if (EndDate.IsNull) return true;
			return (EndDate.Value >= time);
		}

		/// <summary>
		/// Does the schedule start after the passed in time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
		public bool StartsAfter(DateTime time)
		{
			if (StartDate.IsNull) return false;
			return (StartDate.Value > time);
		}

		/// <summary>
		/// Does the schedule ends before the passed in time
		/// </summary>
		/// <param name="time">The time.</param>
		/// <returns></returns>
		public bool EndsBefore(DateTime time)
		{
			if (EndDate.IsNull) return false;
			return (EndDate.Value < time);
		}

		#endregion

		#region TimerQueue Static

		/// <summary>
		/// Forces the time of the specified task
		/// </summary>
		/// <param name="entityManager">The entity manager.</param>
		/// <param name="nextRun">The next run.</param>
		/// <returns></returns>
		public static bool UpdateTimerQueue(IEntityManager entityManager, DateTime? nextRun)
		{
			Logger logger = Logger.GetInstance(typeof (Schedule));
			logger.Debug("Update the TimerQueue with the next run time - if appropriate");

			if (nextRun == null)
			{
				logger.Info("Next Run Time is Null - Timerqueue Entry Ignored.");
				return false;
			}

			// Select the existing timerqueue entry

			IQuery query = entityManager.CreateQuery(Timerqueue.EntityName);
			query.AddEquals(TimerqueuePropertyNames.Task, "Scheduler");
			query.AddEquals(TimerqueuePropertyNames.TaskParams, "-updatetq");

			IEntityCollection timerQueueRecords = entityManager.Select(Timerqueue.EntityName, query);

			if (timerQueueRecords.Count == 0)
			{
				logger.Info("No timer queue entries ");
				return false;
			}

			if (timerQueueRecords.Count > 1)
			{
				logger.Info("Multiple timerqueue entries found - unable to adjust time of next run");
				return false;
			}

			// Set the next time the timerqueue is due to run

			Timerqueue timerQueue = (Timerqueue) timerQueueRecords[0];

			if (nextRun >= timerQueue.RunTime)
			{
				logger.InfoFormat("Next Timerqueue run time is {0} - which is BEFORE the next predicted event {1}. Timerqueue not updated.", timerQueue.RunTime, nextRun);
				return false;
			}

			logger.InfoFormat("Timerqueue updated - Next Run will be {0}", nextRun);

			timerQueue.RunTime = (DateTime) nextRun;
			entityManager.Transaction.Add(timerQueue);
			return true;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Links to Type LocationBase
		/// </summary>
		/// <value></value>
		[PromptHierarchyLink(LocationBase.EntityName, true, ObjectModel.Location.HierarchyPropertyName)]
		public override LocationBase Location
		{
			get
			{
				return base.Location;
			}
			set
			{
				base.Location = value;
			}
		}

		#endregion
	}
}