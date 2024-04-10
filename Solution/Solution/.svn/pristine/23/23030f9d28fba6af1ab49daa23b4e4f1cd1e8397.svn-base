using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Role Header Laboratory Table Task
	/// </summary>
	[SampleManagerTask("RoleHeaderTask", "LABTABLE", "ROLE_HEADER")]
	public class RoleHeaderTask : GenericLabtableTask
	{
		#region Member Variables

		private FormRoleHeader m_Form; 

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			RoleHeader role = (RoleHeader) MainForm.Entity;
			m_Form = (FormRoleHeader) MainForm;

			if (Context.LaunchMode == CopyOption)
			{
				role.RoleAssignments.RemoveAll();
			}

			Personnel currentUser = (Personnel)Library.Environment.CurrentUser;

			if (currentUser.HasProcedureNumber(9037))
			{
				IQuery allItems = EntityManager.CreateQuery(MasterMenu.EntityName);
				allItems.AddEquals(MasterMenuPropertyNames.Removeflag, false);
				m_Form.AvailableItems.Republish(allItems);
			}
			else
			{
				// Restrict this to menu items the user has.

				m_Form.AvailableItems.Republish(currentUser.AvailableMasterMenuItems);
			}
		}

		/// <summary>
		/// Called before the property sheet or wizard is saved.
		/// </summary>
		/// <returns>
		/// true to allow the save to continue, false to abort the save
		/// </returns>
		protected override bool OnPreSave()
		{
			bool ok = base.OnPreSave();

			Personnel currentUser = (Personnel)Library.Environment.CurrentUser;
			if (currentUser.HasProcedureNumber(9036)) return ok;

			// Automatically Grant Access to this Role for this User.

			if (ok && Context.LaunchMode == AddOption || Context.LaunchMode == CopyOption)
			{
				RoleAssignmentBase link = (RoleAssignmentBase) EntityManager.CreateEntity(RoleAssignmentBase.EntityName);
				link.OperatorId = (PersonnelBase) Library.Environment.CurrentUser;
				link.RoleId = (RoleHeaderBase) MainForm.Entity;
				EntityManager.Transaction.Add(link);
			}

			return ok;
		}

		#endregion
	}
}