using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the GROUP_HEADER entity.
	/// </summary>
	[SampleManagerEntity(GroupHeaderBase.EntityName)]
	public class GroupHeader : GroupHeaderBase
	{
		#region Overrides

		/// <summary>
		/// Called before the entity is committed as part of a transaction.
		/// </summary>
		protected override void OnPreCommit()
		{
			if (IsNew()) GroupId = Identity;
		}

		#endregion
	}
}
