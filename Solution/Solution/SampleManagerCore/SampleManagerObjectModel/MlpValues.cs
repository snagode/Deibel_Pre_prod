using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the MLP_VALUES entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class MlpValues : MlpValuesBase
	{
		#region Overrides

		/// <summary>
		/// Perform post copy processing.
		/// </summary>
		/// <param name="sourceEntity">The entity that was used to create this instance.</param>
		protected override void OnEntityCopied(IEntity sourceEntity)
		{
			base.OnEntityCopied(sourceEntity);
			EntryCode = ((MlpValuesBase) sourceEntity).EntryCode;
			LevelId = ((MlpValuesBase) sourceEntity).LevelId;
		}

		#endregion
	}
}
