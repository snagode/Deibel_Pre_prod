using System.Collections.Generic;
using System.Globalization;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the PERSONNEL entity.
	/// </summary>
	[SampleManagerEntity(PersonnelBase.EntityName)]
	public class Personnel : PersonnelBase
	{
		#region Member Variables

		private IEntityCollection m_DistinctRoleAssignments;
		private IEntityCollection m_AvailableMenuItems;

		#endregion

		#region Properties

		/// <summary>
		/// IEntityCollection property to hold RoleAssignments
		/// Original field name: ROLE_ASSIGNMENTS
		/// </summary>
		/// <value></value>
		[PromptCollection(TableNames.RoleAssignment, false)]
		public IEntityCollection DistinctRoleAssignments
		{
			get
			{
				if (m_DistinctRoleAssignments == null)
				{
					m_DistinctRoleAssignments = EntityManager.CreateEntityCollection(TableNames.RoleAssignment);

					foreach (RoleAssignment roleAssignment in RoleAssignments)
					{
						if (!m_DistinctRoleAssignments.Contains(roleAssignment))
						{
							m_DistinctRoleAssignments.Add(roleAssignment);
						}
					}
				}

				return m_DistinctRoleAssignments;
			}
		}

		/// <summary>
		/// Returns true if personnel entity has access to the passed RoleHeader
		/// </summary>
		/// <param name="role"></param>
		/// <returns></returns>
		public bool HasRole(RoleHeaderBase role)
		{
			foreach (RoleAssignment roleAssignment in RoleAssignments)
			{
				if (roleAssignment.RoleId == role)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Returns true if personnel entity has access to the passed procedureNum
		/// </summary>
		/// <param name="procedureNum"></param>
		/// <returns></returns>
		public bool HasProcedureNumber(int procedureNum)
		{
			foreach (RoleAssignment roleAssignment in DistinctRoleAssignments)
			{
				foreach (RoleEntry roleEntry in roleAssignment.RoleId.RoleEntries)
				{
					if (roleEntry.ProcedureNum == procedureNum)
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Gets the master menu items available to this user.
		/// </summary>
		[PromptCollection(TableNames.MasterMenu, false, StopAutoPublish = true)]
		public IEntityCollection AvailableMasterMenuItems
		{
			get
			{
				if (m_AvailableMenuItems != null) return m_AvailableMenuItems;
				m_AvailableMenuItems = EntityManager.CreateEntityCollection(MasterMenu.EntityName);

				foreach (RoleAssignment roleAssignment in DistinctRoleAssignments)
				{
					foreach (RoleEntry roleEntry in roleAssignment.RoleId.RoleEntries)
					{
						MasterMenu item = (MasterMenu) roleEntry.MasterMenu;
						if (m_AvailableMenuItems.Contains(item)) continue;
						if (item.Removeflag) continue;
						m_AvailableMenuItems.Add(item);
					}
				}

				m_AvailableMenuItems.AddSortField(MasterMenuPropertyNames.ProcedureNum, true);
				m_AvailableMenuItems.Sort();

				return m_AvailableMenuItems;
			}
		}

		/// <summary>
		/// Links to Type LocationBase
		/// </summary>
		/// <value></value>
		[PromptHierarchyLink(Location.EntityName, true, Location.HierarchyPropertyName)]
		public override LocationBase LocationId
		{
			get
			{
				return base.LocationId;
			}
			set
			{
				base.LocationId = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance has password.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance has password; otherwise, <c>false</c>.
		/// </value>
		[PromptBoolean]
		public bool HasPassword 
		{
			get
			{
				var password = EntityManager.Select("PASSWORD", base.Identity);
				return password != null;
			}
		}

		/// <summary>
		/// Gets the approvals.
		/// </summary>
		/// <value>
		/// The approvals.
		/// </value>
		[PromptCollection(Approval.EntityName, true)]
		public IEntityCollection Approvals
		{
			get
			{
				var q = EntityManager.CreateQuery(ApprovalBase.EntityName);
				q.AddEquals(ApprovalPropertyNames.OperatorId,Identity);
				return EntityManager.Select(q);
			}
		}
		#endregion

		#region Email

		/// <summary>
		/// Gets a value indicating whether this user is emailable.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this user is emailable; otherwise, <c>false</c>.
		/// </value>
		public bool IsMailable
		{
			get { return !string.IsNullOrEmpty(Email); }
		}

		/// <summary>
		/// Mails the user
		/// </summary>
		/// <param name="subject">The subject.</param>
		/// <returns>True if the user has an email address</returns>
		public bool Mail(string subject)
		{
			return Mail(subject, null);
		}

		/// <summary>
		/// Mails the user
		/// </summary>
		/// <param name="subject">The subject.</param>
		/// <param name="body">The body.</param>
		/// <returns>True if the user has an email address</returns>
		public bool Mail(string subject, string body)
		{
			return Mail(subject, body, null);
		}

		/// <summary>
		/// Mails the user
		/// </summary>
		/// <param name="subject">The subject.</param>
		/// <param name="body">The body.</param>
		/// <param name="attachment">The attachment.</param>
		/// <returns>True if the user has an email address</returns>
		public virtual bool Mail(string subject, string body, string attachment)
		{
			if (!IsMailable) return false;
			Library.Utils.Mail(Email, subject, body, attachment);
			return true;
		}

		/// <summary>
		/// Mails the report as PDF
		/// </summary>
		/// <param name="reportTemplate">The report template.</param>
		/// <param name="entities">The entities.</param>
		/// <returns></returns>
		public virtual bool MailReport(ReportTemplate reportTemplate, IEntityCollection entities)
		{
			if (!IsMailable) return false;
			Library.Reporting.MailReport(Email, reportTemplate, entities);
			return true;
		}

		/// <summary>
		/// Mails the report as PDF
		/// </summary>
		/// <param name="reportTemplate">The report template.</param>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		public virtual bool MailReport(ReportTemplate reportTemplate, IEntity entity)
		{
			if (!IsMailable) return false;
			Library.Reporting.MailReport(Email, reportTemplate, entity);
			return true;
		}

		/// <summary>
		/// Mails the report as HTML
		/// </summary>
		/// <param name="reportTemplate">The report template.</param>
		/// <param name="entities">The entities.</param>
		/// <returns></returns>
		public virtual bool MailHtmlReport(ReportTemplate reportTemplate, IEntityCollection entities)
		{
			if (!IsMailable) return false;
			Library.Reporting.MailHtmlReport(Email, reportTemplate, entities);
			return true;
		}

		/// <summary>
		/// Mails the report as HTML
		/// </summary>
		/// <param name="reportTemplate">The report template.</param>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		public virtual bool MailHtmlReport(ReportTemplate reportTemplate, IEntity entity)
		{
			if (!IsMailable) return false;
			Library.Reporting.MailHtmlReport(Email, reportTemplate, entity);
			return true;
		}

		#endregion
	}
}
