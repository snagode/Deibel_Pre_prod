using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Server.Workflow.Definition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the WORKFLOW STATE entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class WorkflowState : WorkflowStateInternal
	{
		#region Overrides

		/// <summary>
		/// Called when when a property changes.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged(PropertyEventArgs e)
		{
			// If someone changes the table name - then the conditions are invalid.

			if (e.PropertyName == WorkflowStatePropertyNames.TableName)
			{
				WorkflowStateConditions.Clear();
			}

			base.OnPropertyChanged(e);
		}

		#endregion
	}
}