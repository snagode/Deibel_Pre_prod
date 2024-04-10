using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Group Header Laboratory Table Task
	/// </summary>
	[SampleManagerTask("GroupHeaderTask", "LABTABLE", "GROUP_HEADER")]
	public class GroupHeaderTask : GenericLabtableTask
	{

		#region Overrides

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

			// Automatically Grant Access to this Group for this User.

			if (ok && Context.LaunchMode == AddOption || Context.LaunchMode == CopyOption)
			{
				GrouplinkBase link = (GrouplinkBase) EntityManager.CreateEntity(GrouplinkBase.EntityName);
				link.OperatorId = (PersonnelBase) Library.Environment.CurrentUser;
				link.GroupId = (GroupHeaderBase) MainForm.Entity;
				EntityManager.Transaction.Add(link);
			}

			return ok;
		}

		#endregion
	}
}