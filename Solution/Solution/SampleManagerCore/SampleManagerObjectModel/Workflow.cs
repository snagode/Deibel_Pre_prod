using System;
using System.Collections.Generic;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.ImportExport;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.ImportExport;
using Thermo.SampleManager.ObjectModel.Import_Helpers;
using Thermo.SampleManager.Server;
using Thermo.SampleManager.Server.Workflow.Definition;
using Thermo.SampleManager.Server.Workflow.Services;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the WORKFLOW entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class Workflow : WorkflowInternal,IImportableEntity
	{
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="Workflow"/> class.
		/// </summary>
		public Workflow()
		{
			ObjectPropertyChanged += Workflow_ObjectPropertyChanged;
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Handles the ObjectPropertyChanged event of the Workflow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.Framework.Core.ExtendedObjectPropertyEventArgs"/> instance containing the event data.</param>
		void Workflow_ObjectPropertyChanged(object sender, ExtendedObjectPropertyEventArgs e)
		{
			if (e.PropertyName == WorkflowPropertyNames.IncludeInMenu)
			{
				if (IncludeInMenu)
				{
					// Add user role

					IEntity role = EntityManager.CreateEntity(WorkflowRoleBase.EntityName);
					role.Set(WorkflowRolePropertyNames.RoleId, "USER");
					WorkflowRoles.Add(role);
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
			WorkflowRoles.Clear();
			MenuText = null;
			SubMenuGroup = null;
		}

		#endregion

		#region Custom Add Menu

		/// <summary>
		/// Checks if a procedure number should be included in the context menu.
		/// </summary>
		/// <remarks>
		/// Called from an override on the ExplorerFolder class to setup context menus.
		/// </remarks>
		/// <param name="menuProc">The menu proc.</param>
		/// <param name="items">The items.</param>
		/// <param name="folderPath">The folder path.</param>
		/// <param name="groupValue">The group value.</param>
		/// <returns></returns>
		public static bool IncludeMenuItem(int menuProc, ICollection<IExtendedObject> items, string folderPath, object groupValue)
		{
			// Check if the folder is a grouped folder then check all the 'Add...' items for the correct workflow type
			// this is implemented as a static and called from the folder - as we want this to work even if nothing is selected.

			if (groupValue == null) return true;

			// Determine the appropriate add option for this workflow type

			var att = WorkflowNodeFactory.GetWorkflowType(groupValue.ToString());
			if (att == null) return true;
			if (att.AddOption == menuProc) return true;

			// If it's the add menu item for another type then exclude it.

			foreach (var other in WorkflowNodeFactory.GetWorkflowTypes().Values)
			{
				if (other.AddOption == att.AddOption) continue;
				if (menuProc == other.AddOption) return false;
			}

			return true;
		}

		/// <summary>
		/// Determine which default menu item to use.
		/// </summary>
		/// <param name="menuProc">The menu proc - defaulted from the folder</param>
		/// <param name="folderPath">The folder path.</param>
		/// <param name="groupValue">The group value.</param>
		/// <returns></returns>
		public override int DefaultMenuItem(int menuProc, string folderPath, object groupValue)
		{
			// The standard menu default is 35054 - if someone has changed this respect that

			if (menuProc != 35054) return menuProc;

			// Choose an appropriate menu option based on type

			int option = GetDefaultModifyOption();

			if (option == -1 || !Library.Security.CheckPrivilege(option))
			{
				option = GetDefaultDisplayOption();
			}

			if (option == -1) return menuProc;
			return option;
		}

		/// <summary>
		/// Get the Default Modify Option
		/// </summary>
		/// <returns></returns>
		protected virtual int GetDefaultModifyOption()
		{
			if (IsValid(WorkflowTypeInformation)) return WorkflowTypeInformation.ModifyOption;
			return -1;
		}

		/// <summary>
		/// Get the Default Display Option
		/// </summary>
		/// <returns></returns>
		protected virtual int GetDefaultDisplayOption()
		{
			if (IsValid(WorkflowTypeInformation)) return WorkflowTypeInformation.DisplayOption;
			return -1;
		}

		#endregion

		#region IImportableEntity Implementation

		/// <summary>
		/// Validates the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="primitiveEntities">The primitive entities.</param>
		/// <returns></returns>
		public ImportValidationResult CheckImportValidity(IEntity entity, List<ExportDataEntity> primitiveEntities)
		{
			var helper = new WorkflowImportHelper(EntityManager, Library);
			var result = helper.CheckImportValidity(entity, primitiveEntities);
			if (result.AlreadyExists) result.AvailableActions.Add(ImportValidationResult.ImportActions.New_Version);

			return result;
		}


		/// <summary>
		/// Imports the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="result">The result.</param>
		/// <returns></returns>
		public ImportCommitResult Import(IEntity entity, ImportValidationResult result)
		{
			var helper = new WorkflowImportHelper(EntityManager, Library);

			if (result.SelectedImportAction == ImportValidationResult.ImportActions.New_Version)
			{
				entity = CreateNewImportedVersion(entity);
			}

			return helper.Import(entity, result);		
		}

		/// <summary>
		/// Creates the new imported version.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		private IEntity CreateNewImportedVersion(IEntity entity)
		{
			Workflow workflow = entity as Workflow;

			var nodes = new WorkflowNode[workflow.WorkflowNodes.Count];

			workflow.WorkflowNodes.CopyTo(nodes, 0);

			workflow = workflow.CreateNewVersion() as Workflow;

			workflow.WorkflowNodes.Clear();

			// Clearing all the nodes will create a default root node, zap that later.

			var oldRoot = workflow.WorkflowRootNode;

			// Clone the Root node

			var root = (WorkflowNodeInternal) WorkflowRootNode;
			var copyRoot = (WorkflowNodeInternal) root.CreateCopy();

			copyRoot.ParentWorkflow = workflow;
			workflow.WorkflowNodes.Add(copyRoot);

			// Zap the original.

			workflow.WorkflowNodes.Remove(oldRoot);

			if (nodes.Length >= 1)
			{
				var importNode = nodes[1];
				//// Add all its kids
				copyRoot.WorkflowNodeName = importNode.WorkflowNodeName;
				copyRoot.Description = importNode.Description;

				foreach (WorkflowNode n in importNode.WorkflowNodes)
				{
					n.CopyTo(copyRoot);
				}
			}

			entity = workflow;
			return entity;
		}

		/// <summary>
		/// Export preprocessing
		/// </summary>
		/// <param name="entity"></param>
		public void ExportPreprocess(IEntity entity)
		{
			//do nothing
		}

		#endregion
	}
}