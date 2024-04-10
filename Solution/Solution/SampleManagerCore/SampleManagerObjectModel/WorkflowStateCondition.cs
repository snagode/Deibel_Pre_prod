using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Server.Workflow.Definition;
using Thermo.SampleManager.Server.Workflow.Helpers;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the WORKFLOW STATE CONDITION entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class WorkflowStateCondition : WorkflowStateConditionInternal
	{
		#region Overrides

		/// <summary>
		/// Called when when a property changes.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged(PropertyEventArgs e)
		{
			if (e.PropertyName == WorkflowStateConditionPropertyNames.Property)
			{
				Operator = Condition.OperatorEquals;
				Value = null;
			}

			if (e.PropertyName == WorkflowStateConditionPropertyNames.Operator)
			{
				Value = null;
			}

			base.OnPropertyChanged(e);
		}

		#endregion
	}
}