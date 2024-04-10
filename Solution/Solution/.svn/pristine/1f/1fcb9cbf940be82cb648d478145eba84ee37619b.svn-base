using System.Collections.Generic;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.ClientControls;

namespace Thermo.SampleManager.Tasks.BusinessObjects
{
	/// <summary>
	/// Journal Interface
	/// </summary>
	public interface IJournal
	{
		/// <summary>
		/// Entity Type for Journal entries
		/// </summary>
		string EntityType { get; }

		/// <summary>
		/// Entity the journal belongs to
		/// </summary>
		IEntity Entity { get; }

		/// <summary>
		/// Title for Journal Form
		/// </summary>
		string FormTitle { get; }

		/// <summary>
		/// Journal Start Date
		/// </summary>
		NullableDateTime Start { get; }

		/// <summary>
		/// Journal End Date
		/// </summary>
		NullableDateTime End { get; }

		/// <summary>
		/// List of journal intervals
		/// </summary>
		IList<JournalInterval> JournalIntervals { get; }

		/// <summary>
		/// Collection with the journal entries
		/// </summary>
		IEntityCollection JournalCollection { get; }

		/// <summary>
		/// Fill the journal
		/// </summary>
		void Fill();

		/// <summary>
		/// Transform entries and periods to unboundcalendarevents
		/// </summary>
		/// <returns></returns>
		IList<UnboundCalendarEvent> GetJournalAsCalendarEvents();
	}

	/// <summary>
	/// JournalInterval
	/// </summary>
	public class JournalInterval
	{
		#region Properties

		/// <summary>
		/// Interval Name 
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Start date
		/// </summary>
		public NullableDateTime StartOn { get; private set; }

		/// <summary>
		/// End date
		/// </summary>
		public NullableDateTime EndOn { get; private set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Creates an instace of JournalInterval
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="start">Start date</param>
		public JournalInterval(string name, NullableDateTime start)
			: this(name, start, new NullableDateTime())
		{
		}

		/// <summary>
		/// Create an instance of JournalInterval
		/// </summary>
		/// <param name="name">The name</param>
		/// <param name="start">Start date</param>
		/// <param name="end">End date</param>
		public JournalInterval(string name, NullableDateTime start, NullableDateTime end)
		{
			Name = name;
			StartOn = start;
			EndOn = end;
		}

		#endregion
	}

}