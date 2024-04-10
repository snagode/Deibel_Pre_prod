using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the CRITERIA_CONDITION entity.
	/// </summary>
	[SampleManagerEntity(CriteriaConditionBase.EntityName)]
    public class CriteriaCondition : CriteriaConditionInternal
	{
		#region Overrides

		/// <summary>
		/// Called when when a property changes.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged(PropertyEventArgs e)
		{
			if (e.PropertyName == CriteriaConditionPropertyNames.CriteriaField)
			{
				Value = string.Empty;
			}

			base.OnPropertyChanged(e);
		}

		#endregion
	}
}
