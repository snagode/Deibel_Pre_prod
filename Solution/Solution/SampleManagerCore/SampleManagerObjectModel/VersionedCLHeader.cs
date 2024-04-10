using System;
using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the VERSIONED_C_L_HEADER entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class VersionedCLHeader : VersionedCLHeaderBase
	{
		#region Entry Management

		/// <summary>
		/// Updates the entries.
		/// </summary>
		public void UpdateEntries()
		{
			IList<VersionedCLEntry> entries = new List<VersionedCLEntry>();

			foreach (VersionedCLEntry entry in CLEntries.ActiveItems)
			{
				if (! entry.Selected )
				{
					entries.Add(entry);
				}
			}

			foreach(VersionedCLEntry entry in entries)
			{
				CLEntries.Remove(entry);
			}
		}

		#endregion
	}
}