using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using System.Collections.Generic;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Workflow Action Type Task
	/// </summary>
	[SampleManagerTask("WorkflowActionTypeTask", "LABTABLE", "WORKFLOW_ACTION_TYPE")]
	public class WorkflowActionTypeTask : GenericLabtableTask
	{
		#region Member Variables

		private WorkflowActionType m_Entity;
		private FormWorkflowActionType m_Form;
		private bool m_SubMenuInitialised;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Entity = (WorkflowActionType)MainForm.Entity;
			m_Form = (FormWorkflowActionType)MainForm;
		}

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded( )
		{
			base.MainFormLoaded();

			MenuTabInitialize();

			m_Entity.ObjectPropertyChanged += WorkflowActionTypeObjectPropertyChanged;

			// Set up the list of appropriate tasks

			var tasks = Library.Task.GetTaskList(WorkflowActionType.EntityName, "Execute");
			m_Form.ActionTask.Browse = BrowseFactory.CreateStringBrowse(tasks);
		}

		/// <summary>
		/// Handle the object property changed event on the workflow action type
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.Framework.Core.ExtendedObjectPropertyEventArgs"/> instance containing the event data.</param>
		void WorkflowActionTypeObjectPropertyChanged( object sender, Framework.Core.ExtendedObjectPropertyEventArgs e )
		{
			if ( e.PropertyName == WorkflowActionTypePropertyNames.TableName ||
				 e.PropertyName == WorkflowActionTypePropertyNames.SubMenuGroup )
			{
				RefreshSubMenuBrowse( );
			}

			if (e.PropertyName == WorkflowActionTypePropertyNames.TableName ||
			    e.PropertyName == WorkflowActionTypePropertyNames.SubMenuGroup)
			{
				RefreshSubMenuBrowse();
				return;
			}

			if (e.PropertyName == WorkflowActionTypePropertyNames.IncludeInMenu)
			{
				MenuTabSetReadOnly(!m_Entity.IncludeInMenu);
			}
		}

		#endregion

		#region Context Menu Tab

		/// <summary>
		/// Menus tab initialize.
		/// </summary>
		private void MenuTabInitialize()
		{
			if (!m_Entity.IncludeInMenu) return;
			if (Context.LaunchMode == DisplayOption) return;
			if (Context.LaunchMode == AddOption) return;
			if (Context.LaunchMode == SubmitOption) return;

			MenuTabSetReadOnly(false);
		}

		/// <summary>
		/// Set the Menu Tab readonly status
		/// </summary>
		/// <param name="read">if set to <c>true</c> [read].</param>
		private void MenuTabSetReadOnly(bool read = true)
		{
			m_Form.WorkflowRoles.Enabled = !read;
			m_Form.MenuTextPrompt.ReadOnly = read;
			m_Form.SubMenuPrompt.ReadOnly = read;
			m_Form.IconPrompt.ReadOnly = read;

			// Initialise the Sub Menu Prompt (just once)

			if (!read)
			{
				if (m_SubMenuInitialised) return;
				RefreshSubMenuBrowse();
			}
		}

		/// <summary>
		/// Refreshes the sub menu browse.
		/// </summary>
		private void RefreshSubMenuBrowse()
		{
			if ( string.IsNullOrEmpty( m_Entity.TableName ) ) return;

			HashSet<string> browseStrings = new HashSet<string>( );

			// Add workflow action type menus
			IQuery query = EntityManager.CreateQuery(WorkflowActionType.EntityName);
			query.AddEquals(WorkflowActionTypePropertyNames.TableName,m_Entity.TableName);

			IEntityCollection actions = EntityManager.Select(WorkflowActionType.EntityName, query);

			foreach (WorkflowActionType action in actions)
			{
				BuildBrowseStrings(action.SubMenuGroup, browseStrings);
			}

			// Add workflow menus
			query = EntityManager.CreateQuery( Workflow.EntityName );
			query.AddEquals( WorkflowPropertyNames.TableName, m_Entity.TableName );

			actions = EntityManager.Select( Workflow.EntityName, query );

			foreach ( Workflow action in actions )
			{
				BuildBrowseStrings( action.SubMenuGroup, browseStrings );
			}

            List<string> browseList = new List<string>(browseStrings);
            browseList.Sort();

			m_Form.SubMenuPrompt.Browse = BrowseFactory.CreateStringBrowse(browseList);
			m_SubMenuInitialised = true;
		}

		/// <summary>
		/// Builds the browse strings.
		/// </summary>
		/// <param name="subMenu">The sub menu.</param>
		/// <param name="browseStrings">The browse strings.</param>
		private static void BuildBrowseStrings( string subMenu, HashSet<string> browseStrings )
		{
			for ( int pos = subMenu.IndexOf( '/' ); pos >= 0; pos = subMenu.IndexOf( '/', pos + 1 ) )
			{
				browseStrings.Add(subMenu.Substring(0, pos));
			}

			browseStrings.Add( subMenu );
		}

		#endregion
	}
}