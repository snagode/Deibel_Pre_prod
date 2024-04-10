using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Server.Workflow.Definition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the WORKFLOW ACTION TYPE entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class WorkflowActionType : WorkflowActionTypeInternal
	{
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="WorkflowActionType"/> class.
		/// </summary>
		public WorkflowActionType()
		{
			ObjectPropertyChanged += WorkflowActionType_ObjectPropertyChanged;
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Handles the ObjectPropertyChanged event of the WorkflowActionType control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.Framework.Core.ExtendedObjectPropertyEventArgs"/> instance containing the event data.</param>
		void WorkflowActionType_ObjectPropertyChanged(object sender, Framework.Core.ExtendedObjectPropertyEventArgs e)
		{
			if (e.PropertyName == WorkflowActionTypePropertyNames.IncludeInMenu)
			{
				if (IncludeInMenu)
				{
					// Add user role

					IEntity role = EntityManager.CreateEntity(WorkflowActionRoleBase.EntityName);
					role.Set(WorkflowRolePropertyNames.RoleId, "USER");
					WorkflowActionRoles.Add(role);

					return;
				}

				ClearMenuInformation();
			}
		}

		#endregion

		#region Menu Visibility

		/// <summary>
		/// Clears the menu information.
		/// </summary>
		private void ClearMenuInformation()
		{
			WorkflowActionRoles.Clear();
			MenuText = null;
			SubMenuGroup = null;
		}

		#endregion
	}
}
