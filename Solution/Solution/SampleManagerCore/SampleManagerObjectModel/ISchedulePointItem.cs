using System;
using System.Collections.Generic;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Utilities;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Schedule Point Frequency
	/// </summary>
	public interface ISchedulePointItem : INamedObject
	{
		/// <summary>
		/// Gets the frequency object.
		/// </summary>
		/// <value>The frequency object.</value>
		Frequency FrequencyObject { get; }

		/// <summary>
		/// Gets the schedule point.
		/// </summary>
		/// <value>The schedule point.</value>
		SchedulePoint SchedulePoint { get; }

		/// <summary>
		/// Gets the test details.
		/// </summary>
		/// <value>The test details.</value>
		IEnumerable<ISchedulePointEventTest> TestDetails { get; }

		/// <summary>
		/// Gets the run window.
		/// </summary>
		/// <value>The run window.</value>
		TimeSpan RunWindow { get; }

		/// <summary>
		/// Gets the comment text.
		/// </summary>
		/// <returns></returns>
		string GetCommentText();
	}
}