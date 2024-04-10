using System;
using System.Collections.Generic;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.ClientControls.Browse;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Lot Inventory Dashboard Task
	/// </summary>
	[SampleManagerTask("LotRelationsDashboardTask", "GENERAL", "LOT_DETAILS")]
	public class LotRelationDashboardTask : DefaultSingleEntityTask
	{
		#region Member variables

		private FormLotRelationDashboard m_Form;
		private LotDetails m_LotDetails;
		private LotDetails m_RootLotDetails;
		private Stack<LotDetails> m_BackList;
		private Stack<LotDetails> m_ForwList;
		private IEntity m_SelectedTreeItem;
		private IQuery m_CriteriaQuery;
		private Dictionary<EntityBrowse, IEntityCollection> m_CriteriaBrowseLookup;
		private bool m_InitialisingCriteria;
		private string m_SelectedNode;
		private ContextMenuItem m_RmbFocus;
		private LotDetails m_FocusedLot;
		private bool m_ContextShown;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="P:Thermo.SampleManager.Tasks.DefaultFormTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			m_Form = (FormLotRelationDashboard) MainForm;
			m_LotDetails = (LotDetails) MainForm.Entity;
			m_RootLotDetails = (LotDetails) MainForm.Entity;
			m_CriteriaBrowseLookup = new Dictionary<EntityBrowse, IEntityCollection>();
			m_SelectedNode = m_LotDetails.LotId;
			m_BackList = new Stack<LotDetails>();
			m_ForwList = new Stack<LotDetails>();

			RefreshPropertiesGrid();
			BuildTrees();

			m_RmbFocus = m_Form.SimpleTreeListFrom.ContextMenu.AddItem("Set Focus", "INT_REFRESH");
			m_Form.SimpleTreeListUsed.ContextMenu.AddItem(m_RmbFocus);
			m_RmbFocus.ItemClicked += m_RmbFocus_ItemClicked;

			m_Form.ButtonBack.Click += ButtonBack_Click;
			m_Form.ButtonForward.Click += ButtonForward_Click;
			m_Form.ButtonHome.Click += ButtonHome_Click;

			base.MainFormLoaded();
		}

		private void ButtonHome_Click(object sender, EventArgs e)
		{
			m_BackList.Push(m_LotDetails);
			m_ForwList.Clear();

			m_Form.ButtonForward.Enabled = false;
			m_Form.ButtonBack.Enabled = true;
			m_LotDetails = m_RootLotDetails;

			BuildTrees();
		}

		private void ButtonForward_Click(object sender, EventArgs e)
		{
			m_BackList.Push(m_LotDetails);
			m_Form.ButtonBack.Enabled = true;

			m_LotDetails = m_ForwList.Pop();
			if (m_ForwList.Count == 0)
				m_Form.ButtonForward.Enabled = false;

			BuildTrees();
		}

		private void ButtonBack_Click(object sender, EventArgs e)
		{
			m_ForwList.Push(m_LotDetails);
			m_Form.ButtonForward.Enabled = true;

			m_LotDetails = m_BackList.Pop();
			if (m_BackList.Count == 0)
				m_Form.ButtonBack.Enabled = false;

			BuildTrees();
		}

		#endregion

		#region LotProperties

		/// <summary>
		/// Refresh Properties Grid
		/// </summary>
		private void RefreshPropertiesGrid()
		{
			m_SelectedNode = m_LotDetails.LotId;
			m_Form.UnboundGridProperties.BeginUpdate();
			m_Form.UnboundGridProperties.ClearGrid();

			string[] generalProperties =
			{
				LotDetailsPropertyNames.LotId,
				LotDetailsPropertyNames.Description,
				LotDetailsPropertyNames.Quantity,
				LotDetailsPropertyNames.QuantityRemaining,
				LotDetailsPropertyNames.Units,
				LotDetailsPropertyNames.Product,
				LotDetailsPropertyNames.LoginDate,
				LotDetailsPropertyNames.LoginBy,
				LotDetailsPropertyNames.DateAuthorised,
				LotDetailsPropertyNames.Authoriser
			};


			foreach (var property in generalProperties)
			{
				UnboundGrid grid = m_Form.UnboundGridProperties;
				UnboundGridColumn gridcolumn = grid.GetColumnByName(property);

				if (gridcolumn == null)
				{
					gridcolumn = grid.AddColumn(property, property, "General", 100);
					gridcolumn.SetColumnEditorFromObjectModel(LotDetailsBase.EntityName, property);
				}
			}

			BuildPropertyColumns(m_LotDetails, m_Form.UnboundGridProperties);
			BuildRows(m_LotDetails, m_Form.UnboundGridProperties);
			m_Form.UnboundGridProperties.EndUpdate();
		}

		/// <summary>
		/// Refresh Properties Grid
		/// </summary>
		private void RefreshPropertiesGrid(LotDetails lotDetail)
		{
			//prevents simultaneous population on load
			if (m_SelectedNode != lotDetail.LotId)
			{
				m_SelectedNode = lotDetail.LotId;
				m_Form.UnboundGridProperties.BeginUpdate();
				m_Form.UnboundGridProperties.ClearGrid();

				string[] generalProperties =
				{
					LotDetailsPropertyNames.LotId,
					LotDetailsPropertyNames.Description,
					LotDetailsPropertyNames.Quantity,
					LotDetailsPropertyNames.QuantityRemaining,
					LotDetailsPropertyNames.Units,
					LotDetailsPropertyNames.Product,
					LotDetailsPropertyNames.LoginDate,
					LotDetailsPropertyNames.LoginBy,
					LotDetailsPropertyNames.DateAuthorised,
					LotDetailsPropertyNames.Authoriser
				};


				foreach (var property in generalProperties)
				{
					UnboundGrid grid = m_Form.UnboundGridProperties;
					UnboundGridColumn gridcolumn = grid.GetColumnByName(property);

					if (gridcolumn == null)
					{
						gridcolumn = grid.AddColumn(property, property, "General", 100);
						gridcolumn.SetColumnEditorFromObjectModel(LotDetailsBase.EntityName, property);
					}
				}

				BuildPropertyColumns(lotDetail, m_Form.UnboundGridProperties);
				BuildRows(lotDetail, m_Form.UnboundGridProperties);

				m_Form.UnboundGridProperties.EndUpdate();
			}
		}

		/// <summary>
		/// Build Property Columns
		/// </summary>
		/// <param name="lotDetailsBase"></param>
		/// <param name="grid"></param>
		private void BuildPropertyColumns(LotDetailsBase lotDetailsBase, UnboundGrid grid)
		{
			// Get the entity template from the entity

			EntityTemplateInternal template = (EntityTemplateInternal) lotDetailsBase.EntityTemplate;

			foreach (EntityTemplateProperty property in template.EntityTemplateProperties)
			{
				// Add a column for this entity template property

				if (property.PromptType.IsPhrase(PhraseEntTmpPt.PhraseIdHIDDEN)) continue;

				if (property.Name == "Description") continue;

				// Retrieve or create column

				UnboundGridColumn gridcolumn = grid.GetColumnByName(property.PropertyName);

				if (gridcolumn == null)
				{
					gridcolumn = grid.AddColumn(property.PropertyName, property.LocalTitle, "Properties", 50);
					gridcolumn.SetColumnEditorFromObjectModel(template.TableName, property.PropertyName);
				}
			}
		}


		/// <summary>
		/// Build Rows
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="grid"></param>
		private void BuildRows(IEntity entity, UnboundGrid grid)
		{
			m_CriteriaBrowseLookup.Clear();

			EntityTemplateInternal template = (EntityTemplateInternal) ((LotDetailsBase) entity).EntityTemplate;

			// Add a row to the grid

			UnboundGridRow newRow = grid.AddRow();
			newRow.Tag = entity;

			// Set the row icon

			newRow.SetIcon(new IconName(entity.Icon));

			// Set cell values and enable/disable redundant cells based on the entity template

			for (int i = grid.FixedColumns; i < grid.Columns.Count; i++)
			{
				UnboundGridColumn column = grid.Columns[i];

				// Try getting the template property

				EntityTemplatePropertyInternal templateProperty = template.GetProperty(column.Name);

				if (templateProperty == null)
				{
					newRow[column] = entity.Get(column.Name);
				}
				else
				{
					if (templateProperty.IsHidden)
					{
						// Disable this cell
						column.DisableCell(newRow, DisabledCellDisplayMode.GreyHideContents);
					}

					// Set value

					newRow[column] = entity.Get(templateProperty.PropertyName);


					// This is an active cell

					if (templateProperty.IsMandatory)
					{
						// Make the cell appear yellow

						column.SetCellMandatory(newRow);
					}

					if (!string.IsNullOrEmpty(templateProperty.FilterBy))
					{
						// Setup this column for filtering

						// Mark the column that is used for filtering

						UnboundGridColumn filterBySourceColumn = grid.GetColumnByName(templateProperty.FilterBy);
						if (filterBySourceColumn != null)
						{
							filterBySourceColumn.Tag = true;
						}

						// Setup filter

						object filterValue = entity.Get(templateProperty.FilterBy);

						if (filterValue != null)
						{
							IEntity filterValueEntity = filterValue as IEntity;
							bool isValid = filterValueEntity == null || BaseEntity.IsValid(filterValueEntity);
							if (isValid)
							{
								SetupFilterBy(templateProperty, newRow, column, filterValue);
							}
						}
					}
					else if (!string.IsNullOrEmpty(templateProperty.Criteria))
					{
						// A criteria has been specified for this column, setup the browse

						ICriteriaTaskService criteriaTaskService =
							(ICriteriaTaskService) Library.GetService(typeof (ICriteriaTaskService));

						// Once the query is populated the Query Populated Event is raised. This is beacause the criteria
						// could prompt for VGL values or C# values.
						// Prompted Criteria is ignored

						string linkedType = EntityType.GetLinkedEntityType(template.TableName, templateProperty.PropertyName);
						CriteriaSaved criteria =
							(CriteriaSaved)
								EntityManager.Select(TableNames.CriteriaSaved, new Identity(linkedType, templateProperty.Criteria));


						if (BaseEntity.IsValid(criteria))
						{
							// Generate a query based on the criteria

							criteriaTaskService.QueryPopulated += CriteriaTaskService_QueryPopulated;
							m_CriteriaQuery = null;
							m_InitialisingCriteria = true;
							criteriaTaskService.GetPopulatedCriteriaQuery(criteria);
							m_InitialisingCriteria = false;

							if (m_CriteriaQuery != null)
							{
								// Assign the browse to the column
								m_CriteriaQuery.HideRemoved();
								IEntityCollection browseEntities = EntityManager.Select(m_CriteriaQuery.TableName, m_CriteriaQuery);
								EntityBrowse criteriaBrowse = BrowseFactory.CreateEntityBrowse(browseEntities);
								column.SetCellBrowse(newRow, criteriaBrowse);
								m_CriteriaBrowseLookup[criteriaBrowse] = browseEntities;

								// Make sure the cell's value is present within the browse

								IEntity defaultValueEntity = entity.GetEntity(templateProperty.PropertyName);
								if (BaseEntity.IsValid(defaultValueEntity) && !browseEntities.Contains(defaultValueEntity))
								{
									// The default value is not within the specified criteria, null out this cell

									newRow[templateProperty.PropertyName] = null;
								}
							}
						}
					}

					if (templateProperty.IsReadOnly)
					{
						// Disable the cell but display it's contents

						column.DisableCell(newRow, DisabledCellDisplayMode.ShowContents);
					}
				}
			}
		}

		/// <summary>
		/// SetupFilterBy
		/// </summary>
		/// <param name="templateProperty"></param>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <param name="filterValue"></param>
		private void SetupFilterBy(EntityTemplatePropertyInternal templateProperty, UnboundGridRow row, UnboundGridColumn column, object filterValue)
		{
			// Setup entity browse filtering

			IQuery filteredQuery = templateProperty.CreateFilterByQuery(filterValue);

			// Setup the property browse for the collection column to browse collection properties for the table.

			IEntityBrowse browse = BrowseFactory.CreateEntityOrHierarchyBrowse(filteredQuery.TableName, filteredQuery);

			column.SetCellEntityBrowse(row, browse);
		}

		/// <summary>
		/// CriteriaTaskService_QueryPopulated
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CriteriaTaskService_QueryPopulated(object sender, CriteriaTaskQueryPopulatedEventArgs e)
		{
			if (m_InitialisingCriteria)
				m_CriteriaQuery = e.PopulatedQuery;
		}

		#endregion

		#region Trees

		/// <summary>
		/// Builds the trees.
		/// </summary>
		private void BuildTrees()
		{
			m_Form.SimpleTreeListFrom.FocusedNodeChanged -= SimpleTreeList_FocusedNodeChanged;
			m_Form.SimpleTreeListUsed.FocusedNodeChanged -= SimpleTreeList_FocusedNodeChanged;

			var top = (LotRelation) EntityManager.CreateEntity(LotRelationBase.EntityName);
			top.FromLot = m_LotDetails;
			top.ToLot = m_LotDetails;

			// Populate FROM

			m_Form.SimpleTreeListFrom.ClearNodes();
			string nodeName = string.Format("{0} - Made from...", m_LotDetails.Name);
			BuildFromNode(top, nodeName);

			// Populate TO

			m_Form.SimpleTreeListUsed.ClearNodes();
			nodeName = string.Format("{0} - Used for...", m_LotDetails.Name);
			BuildToNode(top, nodeName);

			m_Form.SimpleTreeListUsed.CollapseAll();
			m_Form.SimpleTreeListFrom.CollapseAll();
			m_Form.SimpleTreeListFrom.FocusedNodeChanged += SimpleTreeList_FocusedNodeChanged;
			m_Form.SimpleTreeListUsed.FocusedNodeChanged += SimpleTreeList_FocusedNodeChanged;
			m_Form.SimpleTreeListUsed.ContextMenu.BeforePopup += ContextUsed_BeforePopup;
			m_Form.SimpleTreeListFrom.ContextMenu.BeforePopup += ContextFrom_BeforePopup;
		}

		/// <summary>
		/// Handles the BeforePopup event of the ContextUsed control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ContextMenuBeforePopupEventArgs"/> instance containing the event data.</param>
		private void ContextUsed_BeforePopup(object sender, ContextMenuBeforePopupEventArgs e)
		{
			m_ContextShown = true;
			LotDetailsBase lotDetail = ((LotRelation) e.Entity).ToLot;
			m_SelectedTreeItem = lotDetail;
		}

		/// <summary>
		/// Handles the BeforePopup event of the ContextFrom control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ContextMenuBeforePopupEventArgs"/> instance containing the event data.</param>
		private void ContextFrom_BeforePopup(object sender, ContextMenuBeforePopupEventArgs e)
		{
			m_ContextShown = true;
			LotDetailsBase lotDetail = ((LotRelation) e.Entity).FromLot;
			m_SelectedTreeItem = lotDetail;
		}

		/// <summary>
		/// Builds to node.
		/// </summary>
		/// <param name="relation">The relation.</param>
		/// <param name="nodeName">Name of the node.</param>
		/// <param name="root">The root.</param>
		/// <param name="depth">The depth.</param>
		private void BuildToNode(LotRelationBase relation, string nodeName, SimpleTreeListNodeProxy root = null, int depth = 1)
		{
			if (depth > 10)
			{
				return;
			}
			var toLot = (LotDetails) relation.ToLot;
			root = m_Form.SimpleTreeListUsed.AddNode(root, nodeName, toLot.Icon, relation);

			foreach (LotRelation to in toLot.ChildLots)
			{
				BuildToNode(to, to.ToLot.Name, root, depth + 1);
			}
		}

		/// <summary>
		/// Builds from node.
		/// </summary>
		/// <param name="relation">The relation.</param>
		/// <param name="nodeName">Name of the node.</param>
		/// <param name="root">The root.</param>
		/// <param name="depth">The depth.</param>
		private void BuildFromNode(LotRelationBase relation, string nodeName, SimpleTreeListNodeProxy root = null, int depth = 1)
		{
			if (depth > 10)
			{
				return;
			}
			var fromLot = (LotDetails) relation.FromLot;
			root = m_Form.SimpleTreeListFrom.AddNode(root, nodeName, fromLot.Icon, relation);

			foreach (LotRelation to in fromLot.ParentLots)
			{
				BuildFromNode(to, to.FromLot.Name, root, depth + 1);
			}
		}

		/// <summary>
		/// Handles the ItemClicked event of the m_RmbFocus control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ContextMenuItemEventArgs"/> instance containing the event data.</param>
		private void m_RmbFocus_ItemClicked(object sender, ContextMenuItemEventArgs e)
		{
			m_BackList.Push(m_LotDetails);
			m_ForwList.Clear();
			m_Form.ButtonForward.Enabled = false;
			m_Form.ButtonBack.Enabled = true;
			m_LotDetails = (LotDetails) m_SelectedTreeItem;
			BuildTrees();
		}

		/// <summary>
		/// Handles the FocusedNodeChanged event of the SimpleTreeListUsed control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.SimpleFocusedNodeChangedEventArgs"/> instance containing the event data.</param>
		private void SimpleTreeList_FocusedNodeChanged(object sender, SimpleFocusedNodeChangedEventArgs e)
		{
			if (m_ContextShown)
			{
				m_ContextShown = false;
				return;
			}

			if (e.NewNode != null && e.NewNode.Data != null)
			{
				SimpleTreeList control = (SimpleTreeList) sender;
				LotRelation lotRelation = (LotRelation) e.NewNode.Data;
				LotDetails lotDetail;
				if (control.Name.Contains("From"))
				{
					lotDetail = (LotDetails) lotRelation.FromLot;
				}
				else
				{
					lotDetail = (LotDetails) lotRelation.ToLot;
				}

				if (m_FocusedLot == null || m_FocusedLot.LotId != lotDetail.LotId)
				{
					ReselectSamples(lotDetail);
					RefreshPropertiesGrid(lotDetail);
				}
				m_FocusedLot = lotDetail;
			}
		}

		private void ReselectSamples(LotDetails lotDetail)
		{
			IQuery query = EntityManager.CreateQuery(SampJobViewBase.EntityName);
			query.AddEquals(SampJobViewPropertyNames.JobLotId, lotDetail.LotId);
			m_Form.EntityBrowseSamples.Republish(query);
		}
		#endregion
	}
}