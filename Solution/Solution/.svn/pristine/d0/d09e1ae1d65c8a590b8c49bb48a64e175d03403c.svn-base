using System;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Personnel Task
	/// </summary>
	[SampleManagerTask("PersonnelTask", "LABTABLE", "PERSONNEL")]
	public class PersonnelTask : GenericLabtableTask
	{
		#region Member Variables

		// Used only when an Operator is being copied.
		private IEntity m_CopyMasterOperator;

		private const int TrainingOverrideProcedureNum = 9031;
		private FormPersonnel m_Form;
		private Personnel m_Oper;
		private RoleHeaderBase m_SelectedRole;
		private bool m_RoleGroups;
		private IEntityCollection m_AvailableGroups;
		private IEntityCollection m_AvailableRoles;

		#endregion

		#region Overrides

		/// <summary>
		/// Copy option.
		/// </summary>
		protected override void Copy()
		{
			//The entity of the operator that's being copied
			m_CopyMasterOperator = FindEntity();
			base.Copy();
		}

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Oper = (Personnel)MainForm.Entity;
			m_Form = (FormPersonnel)MainForm;

			m_Form.Loaded += FormLoaded;
			m_Form.LicenseTypeTTextBox.ButtonClick += LaunchLicenseTypeEditor;
			LoadLicenseType();
			RestrictGroupAssignment();
			RestrictRoleAssignment();
		}



		/// <summary>
		/// Launches the license type editor.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void LaunchLicenseTypeEditor(object sender, EventArgs e)
		{
			Library.Task.CreateTaskAndWait(15028,"JUST_RUN");
			RefreshLicensing();
			LoadLicenseType();
		}

		/// <summary>
		/// Refreshes the licensing.
		/// </summary>
		private void RefreshLicensing()
		{
			//reselect several times for change to become apparent
			for (var i = 0; i < 5; i++)
			{
				Context.SelectedItems.Reselect();
			}
		}

		/// <summary>
		/// Form Loaded
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void FormLoaded(object sender, EventArgs e)
		{

			m_Form.RoleSelectionGrid.ItemSelected += RoleSelectionGridItemSelected;
			m_Form.RoleSelectionGrid.ItemDeSelected += RoleSelectionGridItemDeselected;

			m_Form.RoleGroupsRole.FocusedRowChanged += RoleGroupsRoleFocusedRowChanged;
			m_Form.RoleGroupsRole.ItemSelected += RoleGroupsRoleItemSelected;
			m_Form.RoleGroupsRole.ItemDeSelected += RoleGroupsRoleItemDeSelected;

			m_Form.RoleGroupsGroups.ItemSelected += RoleGroupsGroupsItemSelected;
			m_Form.RoleGroupsGroups.ItemDeSelected += RoleGroupsGroupsItemDeSelected;

			m_RoleGroups = Library.Environment.GetGlobalBoolean("ROLE_GROUPS_ENABLED");
			m_Form.page_RoleGroups.Visible = m_RoleGroups;
			m_Form.page_RoleAssignments.Visible = !m_RoleGroups;

			// Only changes when an Operator is copied
			string currentOperator = m_CopyMasterOperator == null ? m_Oper.Identity : m_CopyMasterOperator.Identity;

			// Roles loaded from the designer configuration so only load the selected items for the selected operator
			// Filtered to only the root items with not groups as the control will exhibit strange behaviour if not limited
			PublishAssignedRoles(currentOperator);

			// Publish Groups into the groups grid, fixed list as only selections change.
			m_Form.AvailableGroups.Publish(m_AvailableGroups);

			SetTrainingDescription();

			UpdateLicenseTextboxVisibility();
		}

		#endregion

		#region Load Methods

		/// <summary>
		/// Restricts the group assignment.
		/// </summary>
		private void RestrictGroupAssignment()
		{
			Personnel currentUser = (Personnel)Library.Environment.CurrentUser;

			if (Context.LaunchMode == DisplayOption || currentUser.HasProcedureNumber(9037))
			{
				IQuery notRemoved = EntityManager.CreateQuery(GroupHeaderBase.EntityName);
				notRemoved.AddEquals(GroupHeaderPropertyNames.Removeflag, false);

				EntityManager.PushSecurityOverride();
				m_AvailableGroups = EntityManager.Select(GroupHeaderBase.EntityName, notRemoved);
#pragma warning disable 168
// ReSharper disable once UnusedVariable
				int ignore = m_AvailableGroups.Count; // Ensure Loaded
#pragma warning restore 168
				EntityManager.PopSecurityOverride();
			}
			else
			{
				// Just show groups the user has access to.

				m_AvailableGroups = EntityManager.CreateEntityCollection(GroupHeaderBase.EntityName);

				foreach (GrouplinkBase link in ((Personnel)Library.Environment.CurrentUser).Grouplinks)
				{
					if (link.GroupId.Removeflag) continue;
					m_AvailableGroups.Add(link.GroupId);
				}
			}

			m_Form.GroupsSelectionGrid.BrowseData = m_AvailableGroups;
		}

		/// <summary>
		/// Restricts the role assignment.
		/// </summary>
		private void RestrictRoleAssignment()
		{
			Personnel currentUser = (Personnel)Library.Environment.CurrentUser;

			if (Context.LaunchMode == DisplayOption || currentUser.HasProcedureNumber(9036))
			{
				IQuery notRemoved = EntityManager.CreateQuery(RoleHeaderBase.EntityName);
				notRemoved.AddEquals(RoleHeaderPropertyNames.Removeflag, false);

				m_AvailableRoles = EntityManager.Select(RoleHeaderBase.EntityName, notRemoved);
			}
			else
			{
				// Just show roles the user has access to.

				m_AvailableRoles = EntityManager.CreateEntityCollection(RoleHeaderBase.EntityName);

				foreach (RoleAssignmentBase link in ((Personnel)Library.Environment.CurrentUser).RoleAssignments)
				{
					if (link.RoleId.Removeflag) continue;
					m_AvailableRoles.Add(link.RoleId);
				}
			}

			m_Form.RoleSelectionGrid.BrowseData = m_AvailableRoles;
			m_Form.RoleGroupsRole.BrowseData = m_AvailableRoles;
		}

		/// <summary>
		/// Loads the type of the license.
		/// </summary>
		private void LoadLicenseType()
		{
			m_Form.LicenseTypeTTextBox.Text = LicenseType;
			m_Form.LicenseTypeTTextBox.Text = LicenseType;
		}

		/// <summary>
		/// Updates the license editor button.
		/// </summary>
		private void UpdateLicenseTextboxVisibility()
		{
			m_Form.LicenseTypeTTextBox.Enabled = !m_Oper.IsNew();
		}

		/// <summary>
		/// Called after the property sheet or wizard is saved.
		/// </summary>
		protected override void OnPostSave()
		{
			base.OnPostSave();
			UpdateLicenseTextboxVisibility();
		}

		#endregion

		#region Role Groups - Role Selection

		/// <summary>
		/// Role Group Selected
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.SelectionGridItemEventArgs"/> instance containing the event data.</param>
		private void RoleGroupsRoleItemSelected(object sender, SelectionGridItemEventArgs e)
		{
			RoleAssignment role = (RoleAssignment) e.DataEntity;
			role.OperatorId = m_Oper;
			role.RoleId = (RoleHeaderBase) e.BrowseEntity;
			role.GroupId = null;

			SetAssignedRoleGroups((RoleHeaderBase) e.BrowseEntity);

			m_SelectedRole = (RoleHeaderBase) e.BrowseEntity;
			m_Form.RoleGroupsGroups.Enabled = (m_AvailableGroups.ActiveCount > 0);
		}

		/// <summary>
		/// Role Group De-Selected
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.SelectionGridItemEventArgs"/> instance containing the event data.</param>
		private void RoleGroupsRoleItemDeSelected(object sender, SelectionGridItemEventArgs e)
		{
			if (e.DataEntity != null)
			{
				m_Oper.RoleAssignments.Remove(e.DataEntity);
				m_Form.RolesAssignments.Data.Remove(e.DataEntity);

				RemoveAssignedRoleGroups((RoleHeaderBase)e.BrowseEntity);

				m_SelectedRole = null;
				m_Form.RoleGroupsGroups.Enabled = false;
			}
		}

		/// <summary>
		/// Role Group Focused Row Changed event
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.SelectionGridFocusedRowChangedEventArgs"/> instance containing the event data.</param>
		private void RoleGroupsRoleFocusedRowChanged(object sender, SelectionGridFocusedRowChangedEventArgs e)
		{
			// BrowseEntity if null if nothing is selected, shouldn't be the case but used for error handling only
			if (e.BrowseEntity != null)
			{
				// SelectedEntity is only set if the currently selected item has been checked so populate the groups grid
				if (e.SelectedEntity != null)
				{
					SetAssignedRoleGroups((RoleHeaderBase)e.BrowseEntity);
					m_SelectedRole = (RoleHeaderBase)e.BrowseEntity;
					m_Form.RoleGroupsGroups.Enabled = (m_AvailableGroups.ActiveCount > 0);
				}
				else
				{
					ClearAssignedRoleGroups((RoleHeaderBase)e.BrowseEntity);
					m_SelectedRole = null;
					m_Form.RoleGroupsGroups.Enabled = false;
				}

				RoleHeaderBase roleHeader = (RoleHeaderBase)e.BrowseEntity;
				m_Form.GroupCaption.Caption = String.Format(m_Form.RoleAssignmentStrings.GroupCaption, roleHeader.Identity);
			}
		}

		/// <summary>
		/// Publishes the role groups for the selected role.
		/// </summary>
		/// <param name="role">The role.</param>
		private void SetAssignedRoleGroups(RoleHeaderBase role)
		{
			if (role != null)
			{
				IEntityCollection groups = EntityManager.CreateEntityCollection(RoleAssignmentBase.EntityName);

				foreach (RoleAssignment roleAssignment in m_Oper.RoleAssignments.ActiveItems)
				{
					if (roleAssignment.RoleId.Equals(role) && !String.IsNullOrEmpty(roleAssignment.GroupId.Name.TrimEnd(' ')))
					{
						groups.Add(roleAssignment);
					}
				}

				m_Form.RoleAssignedGroups.Publish(groups);
			}
		}

		/// <summary>
		/// Removes the role groups for the deselected role.
		/// </summary>
		/// <param name="role">The role.</param>
		private void RemoveAssignedRoleGroups(RoleHeaderBase role)
		{
			if (role != null)
			{
				foreach (RoleAssignment roleAssignment in m_Oper.RoleAssignments.ActiveItems)
				{
					if (roleAssignment.RoleId.Equals(role))
					{
						m_Oper.RoleAssignments.Remove(roleAssignment);
					}
				}

				ClearAssignedRoleGroups(role);
			}
		}

		/// <summary>
		/// Clears the role groups for the selected unchecked role.
		/// </summary>
		/// <param name="role">The role.</param>
		private void ClearAssignedRoleGroups(RoleHeaderBase role)
		{
			if (role != null)
			{
				IEntityCollection groups = EntityManager.CreateEntityCollection(RoleAssignmentBase.EntityName);
				m_Form.RoleAssignedGroups.Publish(groups);
			}
		}

		#endregion

		#region Role Groups - Groups Selection

		/// <summary>
		/// Role Group Selected
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RoleGroupsGroupsItemSelected(object sender, SelectionGridItemEventArgs e)
		{
			RoleAssignment role = (RoleAssignment)e.DataEntity;
			role.OperatorId = m_Oper;
			role.RoleId = m_SelectedRole;
			role.GroupId = ((GroupHeaderBase)e.BrowseEntity);
		}

		/// <summary>
		/// Role Group De-Selected
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.SelectionGridItemEventArgs"/> instance containing the event data.</param>
		private void RoleGroupsGroupsItemDeSelected(object sender, SelectionGridItemEventArgs e)
		{
			if (e.DataEntity != null)
			{
				m_Oper.RoleAssignments.Remove(e.DataEntity);
			}
		}

		#endregion

		#region Role Selection

		/// <summary>
		/// Role De-Selected
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.SelectionGridItemEventArgs"/> instance containing the event data.</param>
		private void RoleSelectionGridItemDeselected(object sender, SelectionGridItemEventArgs e)
		{
			SetTrainingDescription();
		}

		/// <summary>
		/// Roles Selected
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.SelectionGridItemEventArgs"/> instance containing the event data.</param>
		private void RoleSelectionGridItemSelected(object sender, SelectionGridItemEventArgs e)
		{
			SetTrainingDescription();
		}

		private void PublishAssignedRoles(string currentOperator)
		{
			const string dynamicSql = "SELECT * FROM ROLE_ASSIGNMENT RA1 WHERE RA1.GROUP_ID = (SELECT MIN(GROUP_ID) FROM ROLE_ASSIGNMENT RA2 WHERE RA2.OPERATOR_ID = RA1.OPERATOR_Id AND RA2.ROLE_ID = RA1.ROLE_ID ) AND (RA1.OPERATOR_ID = '{0}')";

			string dynamicSqlOper = string.Format(dynamicSql, currentOperator);

			IEntityCollection rolesAssigned = EntityManager.SelectDynamic(TableNames.RoleAssignment, dynamicSqlOper, dynamicSqlOper);

			m_Form.RolesAssignments.Publish(rolesAssigned);
		}

		#endregion

		#region Training

		/// <summary>
		/// Set the description on the training course entry screen to indicate if operator has override
		/// </summary>
		private void SetTrainingDescription()
		{
			m_Form.TrainingCourseGrid.Caption = m_Oper.HasProcedureNumber(TrainingOverrideProcedureNum)
													? m_Form.StringTable.Override
													: m_Form.StringTable.NoOverride;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the type of the license.
		/// </summary>
		/// <value>
		/// The type of the license.
		/// </value>
		public string LicenseType {
			get
			{
				var user = EntityManager.Select("PERSONNEL", m_Oper.Identity);
				return user == null ? "" : ((Personnel)user).LicenseType.PhraseText;
			}
		}

		#endregion

	}
}