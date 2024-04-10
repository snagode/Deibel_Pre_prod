using System;
using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.ClientControls.Browse;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of the phrases LTE which extends base class.
	/// </summary>
	[SampleManagerTask("ExplorerHierarchyTask", "LABTABLE", "EXPLORER_HIERARCHY")]
	public class ExplorerHierarchyTask : GenericLabtableTask
	{
		#region Constants

		private const string TABLE_NAME_BROWSE = "BrowseTableName";

		#endregion

		#region Member Variables

		private EntityBrowse m_CriteriaBrowse;
		private ExplorerHierarchy m_ExplorerHierarchy;
		private FormExplorerHierarchy m_Form;
		private IGrid m_Grid;

		#endregion

		#region Task Load

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormExplorerHierarchy) MainForm;
			m_ExplorerHierarchy = (ExplorerHierarchy) MainForm.Entity;

			m_Grid = m_Form.Levels;

			m_Grid.BeforeRowDelete += new EventHandler<BeforeRowDeleteEventArgs>(m_Grid_BeforeRowDelete);
			m_Grid.BeforeRowAdd += new EventHandler<BeforeRowAddedEventArgs>(m_Grid_BeforeRowAdd);
			m_Grid.RowAdded += new EventHandler<RowAddedEventArgs>(m_Grid_RowAdded);
			m_Grid.FocusedRowChanged += new EventHandler<FocusedRowChangedEventArgs>(m_Grid_FocusedRowChanged);
			m_Grid.GridData.ItemPropertyChanged += new EntityCollectionEventHandler(GridData_ItemPropertyChanged);

			m_CriteriaBrowse = m_Form.criteriaBrowse;

			//Assign column events
			m_Grid.CellEditor += new EventHandler<CellEditorEventArgs>(m_Grid_CellEnter);

			//Hook up CellEnabled event
			m_Grid.CellEnabled += new EventHandler<CellEnabledEventArgs>(m_Grid_RowCellEnabled);
		}

		#endregion

		#region Events

		/// <summary>
		/// Handles the BeforeRowDelete event of the m_Grid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.BeforeRowDeleteEventArgs"/> instance containing the event data.</param>
		private void m_Grid_BeforeRowDelete(object sender, BeforeRowDeleteEventArgs e)
		{
			if (!m_Grid.GridData.IsLast(e.Entity))
			{
				e.Cancel = true;
				Library.Utils.FlashMessage(m_Form.StringTable.RowDeleteMessage, m_Form.StringTable.RowDeleteTitle, MessageButtons.OK,
				                           MessageIcon.Information, MessageDefaultButton.Button1);
			}
		}

		/// <summary>
		/// Handles the BeforeRowAdd event of the m_Grid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.BeforeRowAddedEventArgs"/> instance containing the event data.</param>
		private void m_Grid_BeforeRowAdd(object sender, BeforeRowAddedEventArgs e)
		{
			if (String.IsNullOrEmpty(m_Form.PromptTableNameBrowse1.TableName))
			{
				Library.Utils.FlashMessage(m_Form.StringTable.TableNameMissing, m_Form.StringTable.RowAddedTitle, MessageButtons.OK,
											   MessageIcon.Error, MessageDefaultButton.Button1);
				e.Cancel = true;
				return;
			}
			
			if (m_Grid.GridData.ActiveItems.Count > 0)
			{
				ExplorerHierarchyLevel level =
					(ExplorerHierarchyLevel) m_Grid.GridData.ActiveItems[m_Grid.GridData.ActiveItems.Count - 1];

				if (!Library.Schema.Tables.Contains(level.LinkTableName))
				{
					e.Cancel = true;
					Library.Utils.FlashMessage(m_Form.StringTable.RowAddedMessage, m_Form.StringTable.RowAddedTitle, MessageButtons.OK,
					                           MessageIcon.Information, MessageDefaultButton.Button1);
				}
			}
		}

		/// <summary>
		/// Handles the RowAdded event of the m_Grid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.RowAddedEventArgs"/> instance containing the event data.</param>
		private void m_Grid_RowAdded(object sender, RowAddedEventArgs e)
		{
			IEntity parent = m_Grid.GridData.GetPrevious(e.Entity);

			if (parent != null)
			{
				ExplorerHierarchyLevel parentLevel = (ExplorerHierarchyLevel) parent;
				ExplorerHierarchyLevel level = (ExplorerHierarchyLevel) e.Entity;

				//Assign the Table Name. This is always set to the LINK_TABLE_NAME of the first level.
				string parentTableName = parentLevel.TableName;
				if (level.TableName != parentTableName)
					level.TableName = parentTableName;
			}
			else
			{
				AutoAssignFields(e.Entity);
				m_Grid.Refresh();
			}
		}

		/// <summary>
		/// Handles the FocusedRowChanged event of the m_Grid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.FocusedRowChangedEventArgs"/> instance containing the event data.</param>
		private void m_Grid_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
		{
			if (e.Entity != null)
			{
				//Re-Publish Criteria Browse
				if (!m_Grid.GridData.IsFirst(e.Entity))
				{
					IQuery query = EntityManager.CreateQuery(TableNames.CriteriaSaved);

					ExplorerHierarchyLevel thisLevel = (ExplorerHierarchyLevel) e.Entity;
					if (!string.IsNullOrEmpty(thisLevel.TableName))
						query.AddEquals(CriteriaSavedPropertyNames.TableName, thisLevel.LinkTableName);
					else
						query.AddEquals(CriteriaSavedPropertyNames.TableName, "");

					m_CriteriaBrowse = BrowseFactory.CreateEntityBrowse(m_CriteriaBrowse.Name, query);
				}

				//Re-Publish the TableName browse view based on the previous record's Linked Table Name, 
				//i.e. only those tables that link to the previous LINK_TABLE_NAME are available for selection on the new row.
				if (m_Grid.GridData.IsLast(e.Entity))
				{
					if (m_Grid.GridData.IsFirst(e.Entity))
					{
						//Publish all tables
						BrowseFactory.CreateTableNameBrowse(TABLE_NAME_BROWSE);
					}
					else
						PublishTableNameBrowse(e.Entity);
				}
			}
		}

		/// <summary>
		/// Handles the ItemPropertyChanged event of the GridData control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void GridData_ItemPropertyChanged(object sender, EntityCollectionEventArgs e)
		{
			//Try to auto populate the link fields
			if (e.PropertyName == ExplorerHierarchyLevelPropertyNames.LinkTableName)
			{
				AutoAssignFields(e.Entity);
				m_Grid.Refresh();
			}
		}

		/// <summary>
		/// Handles the CellEditor event of the m_Grid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.CellEditorEventArgs"/> instance containing the event data.</param>
		private void m_Grid_CellEnter(object sender, CellEditorEventArgs e)
		{
			if (e.PropertyName == ExplorerHierarchyLevelPropertyNames.LinkFromParent)
			{
				if (!m_Grid.GridData.IsFirst(e.Entity))
				{
					ExplorerHierarchyLevel previousLevel = (ExplorerHierarchyLevel) m_Grid.GridData.GetPrevious(e.Entity);
					PublishParentLinkFields(previousLevel.LinkTableName, e);
				}
			}
			else if (e.PropertyName == ExplorerHierarchyLevelPropertyNames.LinkToParent)
			{
				if (!m_Grid.GridData.IsFirst(e.Entity))
				{
					ExplorerHierarchyLevel level = (ExplorerHierarchyLevel) e.Entity;
					PublishParentLinkFields(level.LinkTableName, e);
				}
			}
			else if (e.PropertyName == ExplorerHierarchyLevelPropertyNames.TreeDisplayFields)
			{
				ExplorerHierarchyLevel level = (ExplorerHierarchyLevel) e.Entity;
				PublishParentLinkFields(level.LinkTableName, e);
			}
			else if (e.PropertyName == ExplorerHierarchyLevelPropertyNames.LinkTableName)
			{
				if (!m_Grid.GridData.IsFirst(e.Entity))
					PublishTableNameBrowse(e.Entity);
			}
			else if (e.PropertyName == ExplorerHierarchyLevelPropertyNames.ExplorerFormTask)
			{
				if (!m_Grid.GridData.IsFirst(e.Entity))
					e.Browse = BrowseFactory.CreateStringBrowse(Library.Task.GetTaskList());
			}
		}

		/// <summary>
		/// Enables / disables cells.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.CellEnabledEventArgs"/> instance containing the event data.</param>
		private void m_Grid_RowCellEnabled(object sender, CellEnabledEventArgs e)
		{
			switch (e.PropertyName)
			{
				case ExplorerHierarchyLevelPropertyNames.LinkTableName:

					e.Enabled = (m_Grid.GridData.IsLast(e.Entity) && !m_Grid.GridData.IsFirst(e.Entity));
					e.DisabledMode = DisabledCellDisplayMode.ShowContents;
					break;

				case ExplorerHierarchyLevelPropertyNames.LinkFromParent:
				case ExplorerHierarchyLevelPropertyNames.LinkToParent:
				case ExplorerHierarchyLevelPropertyNames.ExplorerForm:
				case ExplorerHierarchyLevelPropertyNames.ExplorerFormTask:
				case ExplorerHierarchyLevelPropertyNames.Criteria:

					e.Enabled = !m_Grid.GridData.IsFirst(e.Entity);
					e.DisabledMode = DisabledCellDisplayMode.GreyHideContents;
					break;

				default:
					if (0 == m_Grid.GridData.ActiveItems.IndexOf(e.Entity))
					{
						//On the first row, 'Tree Display Fields' to be edited.
						e.Enabled = e.PropertyName == ExplorerHierarchyLevelPropertyNames.TreeDisplayFields;
						e.DisabledMode = DisabledCellDisplayMode.ShowContents;
					}

					break;
			}
		}

		#endregion

		#region Helper Methods

		/// <summary>
		/// Auto assigns link fields when Link table field is changed.
		/// </summary>
		/// <param name="level">The level.</param>
		private void AutoAssignFields(IEntity level)
		{
			//Check that this level has a parent
			ExplorerHierarchyLevel currentLevel = (ExplorerHierarchyLevel) level;
			ExplorerHierarchyLevel parent = m_Grid.GridData.GetPrevious(level) as ExplorerHierarchyLevel;

			if (parent != null)
			{
				//Populate the LINK_FROM_PARENT and LINK_TO_PARENT fields
				ExplorerHierarchyLevel parentLevel = parent;

				//Assign the LINK_FROM_PARENT and LINK_TO_PARENT fields
				AssignLinkFields(currentLevel, parentLevel.LinkTableName, currentLevel.LinkTableName);

				//Assign the TREE_DISPLAY_FIELDS
				AssignTreeDisplayFields(currentLevel, currentLevel.LinkTableName);
			}
			else
			{
				//Just set the TABLE_NAME as this is the first item in the grid
				currentLevel.TableName = m_ExplorerHierarchy.TableName;
				currentLevel.LinkTableName = currentLevel.TableName;

				//Assign the TREE_DISPLAY_FIELDS
				AssignTreeDisplayFields(currentLevel, currentLevel.LinkTableName);
			}
		}

		/// <summary>
		/// Assigns the TREE_DISPLAY_FIELDS field based on the table key.
		/// </summary>
		/// <param name="level">The level.</param>
		/// <param name="currentTableName">Name of the current table.</param>
		private void AssignTreeDisplayFields(ExplorerHierarchyLevel level, string currentTableName)
		{
			try
			{
				// Now set the Tree Display Fields
				string displayFields = "";
				ISchemaTable table = Library.Schema.Tables[currentTableName];

				for (int i = 0; i < table.KeyFields.Count; i++)
				{
					//Comma separate the field names
					if (i > 0)
						displayFields += ",";

					ISchemaField keyField = table.KeyFields[i];
					displayFields += keyField.Name;
				}

				level.TreeDisplayFields = displayFields;
			}
			catch (KeyNotFoundException ex)
			{
				throw new SampleManagerError(m_Form.StringTable.TableNameError + ":\n" + ex.Message);
			}
		}

		/// <summary>
		/// Assigns the LINK_FROM_PARENT and LINK_TO_PARENT fields based on the relationship to the parent.
		/// </summary>
		/// <param name="level">The level.</param>
		/// <param name="parentTable">The parent table.</param>
		/// <param name="currentTableName">Name of the current table.</param>
		private void AssignLinkFields(ExplorerHierarchyLevel level, string parentTable, string currentTableName)
		{
			//Get all relationships that have the parent as a destination
			List<ISchemaRelationship> relationships = Library.Schema.Relationships.GetRelationshipsFromDestination(parentTable);
			foreach (ISchemaRelationship relationship in relationships)
			{
				if (relationship.SourceTable.Name == currentTableName)
				{
					string fromParent = "";
					string toParent = "";

					for (int i = 0; i < relationship.Predicates.Count; i++)
					{
						//Comma separate the field names
						if (i > 0)
						{
							fromParent += ",";
							toParent += ",";
						}

						//Get the predicate
						ISchemaRelationshipPredicate predicate =
							(ISchemaRelationshipPredicate) relationship.Predicates[i];

						//Get the field names
						fromParent += predicate.DestinationField.Name;
						toParent += predicate.SourceField.Name;
					}

					//Update the Entity
					level.LinkFromParent = fromParent;
					level.LinkToParent = toParent;

					break;
				}
			}
		}

		private void PublishTableNameBrowse(IEntity currentEntity)
		{
			//Publish only those tables that link to the previous LINK_TABLE_NAME
			ExplorerHierarchyLevel parentLevel = (ExplorerHierarchyLevel) m_Grid.GridData.GetPrevious(currentEntity);
			string aboveTableName = parentLevel.LinkTableName;
			List<ISchemaRelationship> relationships = Library.Schema.Relationships.GetRelationshipsFromDestination(aboveTableName);
			List<string> tableNames = new List<string>();

			if (relationships != null)
			{
				for (int i = 0; i < relationships.Count; i++)
				{
					string tableName = relationships[i].SourceTable.Name;
					if (!tableNames.Contains(tableName))
						tableNames.Add(tableName);
				}
			}

			BrowseFactory.CreateTableNameBrowse(TABLE_NAME_BROWSE, tableNames);
		}

		/// <summary>
		/// Publishes the parent link field drop downs.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.CellEditorEventArgs"/> instance containing the event data.</param>
		private void PublishParentLinkFields(string tableName, CellEditorEventArgs e)
		{
			if (!string.IsNullOrEmpty(tableName))
			{
				if (Library.Schema.Tables.Contains(tableName))
				{
					FieldNameBrowse fieldBrowse = BrowseFactory.CreateFieldNameBrowse(tableName);
					e.BrowseAllowMultiple = true;
					e.Browse = fieldBrowse;
				}
			}
		}

		#endregion
	}
}