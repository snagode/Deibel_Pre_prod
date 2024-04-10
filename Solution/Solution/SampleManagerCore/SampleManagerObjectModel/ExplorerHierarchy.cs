using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the EXPLORER_HIERARCHY entity.
	/// </summary>
	[SampleManagerEntity(ExplorerHierarchyBase.EntityName)]
	public class ExplorerHierarchy : ExplorerHierarchyBase
	{
		#region Overrides

		/// <summary>
		/// Perform post copy processing.
		/// </summary>
		/// <param name="sourceEntity">The entity that was used to create this instance.</param>
		protected override void OnEntityCopied(IEntity sourceEntity)
		{
			ExplorerHierarchy copy = (ExplorerHierarchy)sourceEntity;
			TableName = copy.TableName;
		}

		#endregion

	}
}
