using System;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the EXPLORER_GROUP entity.
	/// </summary>
	[SampleManagerEntity(ExplorerGroupBase.EntityName)]
	public class ExplorerGroup : ExplorerGroupBase
	{
		#region Overrides

		/// <summary>
		/// Perform post creation processing.
		/// </summary>
		protected override void OnEntityCreated()
		{
			Guid groupNumberGuid;

			if ((GroupNumber == null) || (Guid.TryParse(GroupNumber, out groupNumberGuid)))
				ApplyKeyIncrements("GROUP_NUMBER");

			ParentNumber = PackedDecimal.FromInt32(0);
		}

		#endregion
	}
}
