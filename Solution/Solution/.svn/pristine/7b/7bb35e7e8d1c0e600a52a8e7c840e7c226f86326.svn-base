using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Sample Login Task
	/// </summary>
	[SampleManagerTask("SampleLoginTask")]
	public class SampleLoginTask : SampleAdminBaseTask
	{
		#region Setup

		/// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			// Set the default workflow to be this one.

			if (BaseEntity.IsValid(Context.Workflow))
			{
				DefaultWorkflow = (WorkflowBase)Context.Workflow;
			}
			else if (Context.SelectedItems.ActiveCount == 1)
			{
				IEntity item = Context.SelectedItems[0];

				if (item.EntityType == Workflow.EntityName)
				{
					DefaultWorkflow = (WorkflowBase) Context.SelectedItems[0];
				}
			}

			base.SetupTask();
		}

		#endregion

		#region Data Initialisation

		/// <summary>
		/// Initialises the data.
		/// </summary>
		/// <returns></returns>
		protected override bool InitialiseTopLevelEntities(out IEntityCollection topLevelEntities)
		{
			topLevelEntities = EntityManager.CreateEntityCollection(m_TopLevelTableName);
			return true;
		}

		#endregion

		#region General Overrides

		/// <summary>
		/// Get hold of the entity from the context
		/// if there isn't one don't create one, this preserves the New Sample count
		/// </summary>
		/// <returns></returns>
		protected override IEntity GetEntity()
		{
			IEntity entity = null;

			if (Context.SelectedItems.Count > 0)
			{
				entity = Context.SelectedItems[0];
			}

			return entity;
		}

		/// <summary>
		/// Gets the name of the top level table.
		/// </summary>
		/// <returns></returns>
		protected override string GetTopLevelTableName()
		{
			if (BaseEntity.IsValid(DefaultWorkflow)) return DefaultWorkflow.TableName;
			return Context.EntityType;
		}

		/// <summary>
		/// Gets a value indicating whether this instance is job workflow.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is job workflow; otherwise, <c>false</c>.
		/// </value>
		protected override bool IsJobWorkflow
		{
			get { return GetTopLevelTableName() == JobHeader.EntityName; }
		}

		/// <summary>
		/// Sets the title.
		/// </summary>
		/// <returns></returns>
		protected override string GetTitle()
		{
			if (OneShotMode && BaseEntity.IsValid(DefaultWorkflow))
			{
				return string.Format(m_Form.StringTable.TitleWorkflowLogin, DefaultWorkflow.WorkflowName);
			}

			if (IsJobWorkflow)
			{
				return m_Form.StringTable.TitleJobLogin;
			}
			return m_Form.StringTable.TitleSampleLogin;
		}

		#endregion
	}
}
