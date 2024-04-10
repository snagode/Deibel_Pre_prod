using System;

namespace Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel
{
	/// <summary>
	/// Chromeleon Sequence Entry Event Args
	/// </summary>
	public class EntryEventArgs : EventArgs
	{
		#region Properties

		/// <summary>
		/// Gets the entry.
		/// </summary>
		/// <value>
		/// The entry.
		/// </value>
		public ChromeleonSequenceEntryEntity Entry { get; private set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="EntryEventArgs"/> class.
		/// </summary>
		/// <param name="entry">The entry.</param>
		public EntryEventArgs(ChromeleonSequenceEntryEntity entry)
		{
			Entry = entry;
		}

		#endregion
	}
}
