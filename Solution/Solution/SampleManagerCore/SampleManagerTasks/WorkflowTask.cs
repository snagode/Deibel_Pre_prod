using System;
using System.Collections.Generic;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Utilities;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server.Workflow.Definition;
using Thermo.SampleManager.Server.Workflow.Nodes;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Workflow Task
	/// </summary>
	[SampleManagerTask("WorkflowTask", "LABTABLE", "WORKFLOW")]
	public class WorkflowTask : GenericLabtableTask
	{
		#region Constants

		private const string ParameterValueColumnName = "ParameterValue";

		#endregion

		#region Member Variables

		private Workflow m_Workflow;
		private FormWorkflow m_Form;
		private Dictionary<WorkflowNode, SimpleTreeListNodeProxy> m_NodeCache;
		private ToolBox m_NodesToolBox;
		private DataGridColumn m_ValueGridColumn;
		private string m_WorkflowType;
		private string m_TableName;
		private bool m_SubMenuInitialised;

		#endregion

		#region Type Specific Menu Items

		/// <summary>
		/// Gets the entity prompt query.
		/// </summary>
		/// <returns></returns>
		protected override IQuery GetEntityPromptQuery()
		{
			IQuery query = base.GetEntityPromptQuery();

			if (Context.LaunchMode != SubmitOption)
			{
				query.AddEquals(WorkflowPropertyNames.WorkflowType, m_WorkflowType);
			}

			return query;
		}

		/// <summary>
		/// Setup the SampleManager LTE task
		/// </summary>
		protected override void SetupTask()
		{
			// Get hold of the Workflow Type

			if ((Context.LaunchMode != SubmitOption) && (Context.LaunchMode != ApproveOption))
			{
				if (Context.TaskParameters.GetUpperBound(0) < 1)
				{
					throw new SampleManagerError(Context.MenuItem.Description,
					                             "Illegal number of parameters - this task must have Form,WorkflowType");
				}

				m_WorkflowType = Context.TaskParameters[1];
			}

			// Parameterized Table Name

			if ((Context.LaunchMode == AddOption))
			{
				if (Context.TaskParameters.Length > 2)
				{
					m_TableName = Context.TaskParameters[2];
				}
			}

			// Make sure all the items are of the specified type

			if ((Context.LaunchMode != AddOption) &&
			    (Context.LaunchMode != SubmitOption) &&
			    (Context.LaunchMode != ApproveOption))
			{
				foreach (Workflow workflow in Context.SelectedItems)
				{
					if (workflow.WorkflowType.PhraseId != m_WorkflowType)
					{
						string message = string.Format("Workflow '{0}' is not of the required type '{1}'", workflow.Name, m_WorkflowType);
						throw new SampleManagerError(Context.MenuItem.Description, message);
					}
				}
			}

			base.SetupTask();
		}

		/// <summary>
		/// Creates the new entity.
		/// </summary>
		/// <returns></returns>
		protected override IEntity CreateNewEntity()
		{
			Workflow workflow = (Workflow) base.CreateNewEntity();
			workflow.SetWorkflowType(m_WorkflowType);
			return workflow;
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			MainForm.SetBusy();
			m_Workflow = (Workflow) MainForm.Entity;
			m_Form = (FormWorkflow) MainForm;
			base.MainFormCreated();
		}

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			// Set the Workflow Type

			SetWorkflowType();

			// Setup the tree. Use lightweight mode for fast loading
			m_Form.WorkflowNodeTree.StartLightweightLoading();
			LoadWorkflowNodes();
			m_Form.WorkflowNodeTree.FinishLightweightLoading();

			m_Form.WorkflowNodeTree.FocusedNodeChanged += WorkflowNodeTreeFocusedNodeChanged;
			m_Form.WorkflowNodeTree.NodeRemoved += WorkflowNodeTreeNodeRemoved;
			m_Form.WorkflowNodeTree.NodePasting += WorkflowNodeTree_NodePasting;
			m_Form.WorkflowNodeTree.NodePasted += WorkflowNodeTree_NodePasted;
			m_Form.WorkflowNodeTree.NodeMoved += WorkflowNodeTree_NodeMoved;
			m_Form.WorkflowNodeTree.DropTransferToolboxItem += WorkflowNodeTree_DropTransferToolboxItem;

			m_Form.NodeParametersGrid.FocusedRowChanged += NodeParametersGrid_FocusedRowChanged;

			m_ValueGridColumn = m_Form.NodeParametersGrid.GetColumnByProperty(ParameterValueColumnName);

			// Setup Nodes ToolBox

			m_NodesToolBox = m_Form.ToolBoxNodes;
			m_NodesToolBox.ItemClicked += NodesToolBoxItemClicked;

			// Populate ToolBox with available NodeTypes

			PopulateNodesToolBox();

			// Sort out the Menu Tab

			MenuTabInitialize();

			// Hook into workflow specific events

			m_Workflow.ObjectPropertyChanged += WorkflowObjectPropertyChanged;
			m_Workflow.WorkflowReset += WorkflowReset;
			m_Workflow.WorkflowResetting += WorkflowResetting;

			m_Workflow.WorkflowNodes.ItemPropertyChanged += WorkflowNodes_ItemPropertyChanged;

			MainForm.ClearBusy();
		}

		/// <summary>
		/// Sets the type of the workflow.
		/// </summary>
		private void SetWorkflowType()
		{
			// No menu stuff for LifeCycles/General/SubSample

			m_Form.PageMenu.Visible = ShowMenuTab();

			// Drop out if Read Only mode/table is hard coded

			if (Context.LaunchMode != AddOption || m_WorkflowType == PhraseWflowType.PhraseIdGENERAL)
			{
				m_Form.EntityType.ReadOnly = true;
				return;
			}

			// Work out which tables are appropriate

			List<string> names = new List<string>();

			// General Workflow - no table required

			if (m_WorkflowType == PhraseWflowType.PhraseIdGENERAL)
			{
				m_Form.EntityType.ReadOnly = true;
				return;
			}

			// Workout which tables are valid.

			if (m_WorkflowType == PhraseWflowType.PhraseIdSAMPLE ||
			    m_WorkflowType == PhraseWflowType.PhraseIdSUBSAMPLE)
			{
				names.Add("SAMPLE");
			}
			else if (m_WorkflowType == PhraseWflowType.PhraseIdJOB_HEADER)
			{
				names.Add("JOB_HEADER");
			}
			else if (m_WorkflowType == PhraseWflowType.PhraseIdMETHOD)
			{
				names.Add("TEST");
			}
			else if (!string.IsNullOrEmpty(m_TableName))
			{
				names.Add(m_TableName);
			}
			else
			{
				// All real tables

				foreach (ISchemaTable table in Library.Schema.Tables)
				{
					if (table.IsView) continue;
					if (!string.IsNullOrEmpty(table.Tableset)) continue;
					if (names.Contains(table.Name)) continue;
					names.Add(table.Name);
				}

				names.Sort();
			}

			// Choose the first one and off we go

			m_Form.EntityType.Browse = BrowseFactory.CreateStringBrowse(names);
			m_Form.EntityType.Text = names[0];
		}

		/// <summary>
		/// Should the menu tab be shown?
		/// </summary>
		/// <returns></returns>
		private bool ShowMenuTab()
		{
			if (m_Workflow.Lifecycle) return false;
			if (m_Workflow.WorkflowType.IsPhrase(PhraseWflowType.PhraseIdSUBSAMPLE)) return false;
			if (m_Workflow.WorkflowType.IsPhrase(PhraseWflowType.PhraseIdGENERAL)) return false;
			if (BaseEntity.IsValid(m_Workflow.WorkflowTypeInformation)) return m_Workflow.WorkflowTypeInformation.ShowMenuTab;
			return true;
		}

		/// <summary>
		/// Workflows the resetting.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void WorkflowResetting(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (m_Workflow.WorkflowNodes.ActiveCount > 1)
			{
				string caption = Library.Message.GetMessage("WorkflowMessages", "WorkflowResetConfirmCaption");
				string message = Library.Message.GetMessage("WorkflowMessages", "WorkflowResetConfirmMessage");

				e.Cancel = !Library.Utils.FlashMessageYesNo(message, caption, MessageIcon.Question);
			}
		}

		/// <summary>
		/// Loads the workflow nodes.
		/// </summary>
		private void LoadWorkflowNodes()
		{
			m_Form.WorkflowNodeTree.ClearNodes();
			m_NodeCache = new Dictionary<WorkflowNode, SimpleTreeListNodeProxy>();

			foreach (WorkflowNode wfNode in m_Workflow.WorkflowNodes.ActiveItems)
			{	
				AddNodeToTree(wfNode);
			}
		}

		/// <summary>
		/// Adds the node to tree.
		/// </summary>
		/// <param name="wfNode">The wf node.</param>
		private void AddNodeToTree(WorkflowNode wfNode)
		{
			SimpleTreeListNodeProxy parentNode = m_NodeCache.ContainsKey((WorkflowNode) wfNode.ParentNode)
				? m_NodeCache[(WorkflowNode) wfNode.ParentNode]
				: null;

			AddNodeToTree(wfNode, parentNode);
		}

		/// <summary>
		/// Adds the node to tree.
		/// </summary>
		/// <param name="wfNode">The wf node.</param>
		/// <param name="parentNode">The parent node.</param>
		private SimpleTreeListNodeProxy AddNodeToTree(WorkflowNode wfNode, SimpleTreeListNodeProxy parentNode)
		{
			SimpleTreeListNodeProxy node = m_Form.WorkflowNodeTree.AddNode(parentNode, wfNode.WorkflowNodeName, new IconName(wfNode.IconId), wfNode);

			((WorkflowNodeInternal)wfNode.ParentNode).AddUniqueNode(wfNode.NodeTypeEntity);

			if (!m_NodeCache.ContainsKey(wfNode))
			{
				m_NodeCache.Add(wfNode, node);
			}

			return node;
		}

		/// <summary>
		/// Adds the nodes to tree.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="parentNode">The parent node.</param>
		/// <returns></returns>
		private void AddNodesToTree(WorkflowNode node, SimpleTreeListNodeProxy parentNode)
		{
			SimpleTreeListNodeProxy childNode = AddNodeToTree(node, parentNode);

			foreach (WorkflowNode child in node.WorkflowNodes)
			{
				// RECURSIVE

				AddNodesToTree(child, childNode);
			}
		}

		/// <summary>
		/// Workflow reset.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void WorkflowReset(object sender, EventArgs e)
		{
			WorkflowReload();
		}

		/// <summary>
		/// Reload the Workflow
		/// </summary>
		private void WorkflowReload()
		{
			m_Form.WorkflowNodeTree.ClearNodes();
			m_NodeCache = new Dictionary<WorkflowNode, SimpleTreeListNodeProxy>();

			WorkflowNode rootNode = (WorkflowNode) m_Workflow.WorkflowRootNode;
			AddNodeToTree(rootNode);
			SetCurrentNode(rootNode);
		}

		/// <summary>
		/// Workflows the object property changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.Framework.Core.ExtendedObjectPropertyEventArgs"/> instance containing the event data.</param>
		private void WorkflowObjectPropertyChanged(object sender, ExtendedObjectPropertyEventArgs e)
		{
			if (e.PropertyName == WorkflowPropertyNames.TableName ||
			    e.PropertyName == WorkflowPropertyNames.SubMenuGroup)
			{
				RefreshSubMenuBrowse();
				return;
			}

			if (e.PropertyName == WorkflowPropertyNames.IncludeInMenu)
			{
				MenuTabSetReadOnly(!m_Workflow.IncludeInMenu);
			}
		}

		#endregion

		#region Tree Handling

		/// <summary>
		/// Handles the ItemPropertyChanged event of the WorkflowNodes control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void WorkflowNodes_ItemPropertyChanged(object sender, EntityCollectionEventArgs e)
		{
			// Set Tree Node Display Text to WF Node Name

			WorkflowNode wfNode = e.Entity as WorkflowNode;
			if (wfNode != null)
			{
				SimpleTreeListNodeProxy treeNode = m_Form.WorkflowNodeTree.FindNode(wfNode);
				if (treeNode != null)
				{
					treeNode.DisplayText = wfNode.WorkflowNodeName;
				}
			}
		}

		/// <summary>
		/// Workflows node tree node removed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.SimpleTreeListNodeEventArgs"/> instance containing the event data.</param>
		private void WorkflowNodeTreeNodeRemoved(object sender, SimpleTreeListNodeEventArgs e)
		{
			if (m_Workflow.WorkflowNodes.Contains(e.Node.Data))
			{
				WorkflowNodeInternal parent = ((WorkflowNodeInternal) e.Node.Data).ParentNodeInternal;
				parent.RemoveUniqueNode(((WorkflowNodeInternal)e.Node.Data).NodeTypeEntity);
				m_Workflow.WorkflowNodes.Remove(e.Node.Data);
			}
		}

		/// <summary>
		/// Workflows node tree focused node changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.SimpleFocusedNodeChangedEventArgs"/> instance containing the event data.</param>
		private void WorkflowNodeTreeFocusedNodeChanged(object sender, SimpleFocusedNodeChangedEventArgs e)
		{
			if ((e.OldNode != null) && (e.OldNode.Data != null))
			{
				WorkflowNode wfNodeOld = (WorkflowNode) e.OldNode.Data;

				// Reset the Overlay if this is now valid.

				if (wfNodeOld.Valid) e.OldNode.ClearOverlayIcon();
			}

			if (e.NewNode == null) return;
			WorkflowNode wfNode = (WorkflowNode) e.NewNode.Data;

			SetCurrentNode(wfNode);
		}

		/// <summary>
		/// Handles the NodePasting event of the WorkflowNodeTree control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.NodePastingEventArgs"/> instance containing the event data.</param>
		private void WorkflowNodeTree_NodePasting(object sender, NodePastingEventArgs e)
		{
			if (e.TargetNode == null)
			{
				// Trying to paste to the root level, don't allow this

				string caption = Library.Message.GetMessage("WorkflowMessages", "InvalidCopyCaption");
				string message = Library.Message.GetMessage("WorkflowMessages", "InvalidCopyMessage");
				Library.Utils.FlashMessage(message, caption, MessageButtons.OK, MessageIcon.Stop, MessageDefaultButton.Button1);

				e.Cancel = true;
				return;
			}

			WorkflowNode from = (WorkflowNode) e.PastedNode.Data;
			WorkflowNode to = (WorkflowNode) e.TargetNode.Data;

			// Check this is a valid follower

			if (!PasteIsValid(from, to, e.IsCut))
			{
				e.Cancel = true;
				return;
			}

			// Make sure data type message match.

			if (!DataIsValid(from, to, e.IsCut))
			{
				e.Cancel = true;
				return;
			}

			if (e.IsCut) return;

			// Copy the nodes to the target.

			WorkflowNode newNode = (WorkflowNode) from.CopyTo(to);
			AddNodesToTree(newNode, e.TargetNode);
			e.Cancel = true;
		}

		/// <summary>
		/// Handles the NodePasted event of the WorkflowNodeTree control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.NodePastedEventArgs"/> instance containing the event data.</param>
		private void WorkflowNodeTree_NodePasted(object sender, NodePastedEventArgs e)
		{
			if (!e.IsCut) return;

			// Get the Old Parent WFNode, the New Parent WFNode and the WFNode being pasted

			WorkflowNode oldParent = (WorkflowNode) e.OldParentNode.Data;
			WorkflowNode newParent = (WorkflowNode) e.TargetNode.Data;
			WorkflowNode pastedNode = (WorkflowNode) e.PastedNode.Data;

			// Remove the pasted node from it's old parent

			oldParent.WorkflowNodes.Release(pastedNode);
			oldParent.RemoveUniqueNode(pastedNode.NodeTypeEntity);

			// Add/Insert the pasted node to it's new parent

			if (e.ChildIndex != -1 && e.ChildIndex < newParent.WorkflowNodes.Count)
			{
				newParent.WorkflowNodes.Insert(e.ChildIndex, pastedNode);
			}
			else
			{
				newParent.WorkflowNodes.Add(pastedNode);
			}
			newParent.AddUniqueNode(pastedNode.NodeTypeEntity);

			// Set Focus
			m_Form.WorkflowNodeTree.FocusNode(e.PastedNode);
			RefreshNodesToolBox((WorkflowNode)e.TargetNode.Data);
		}

		/// <summary>
		/// Allow the node to be pasted if it can follow the target node type.
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		/// <param name="isCut">if set to <c>true</c> [is cut].</param>
		/// <returns></returns>
		private bool PasteIsValid(WorkflowNodeInternal from, WorkflowNodeInternal to, bool isCut)
		{
			if (!ValidUniqueNode((WorkflowNode) to, from.NodeTypeEntity))
			{
				Library.Utils.FlashMessage("Can not paste unquie node type", "caption", MessageButtons.OK, MessageIcon.Stop, MessageDefaultButton.Button1);
				return false;
			}

			foreach (WorkflowNodeTypeInternal follower in to.Followers)
			{
				if (follower.NodeType == from.NodeType) return true;
			}

			if (isCut)
			{
				string caption = Library.Message.GetMessage("WorkflowMessages", "InvalidMoveCaption");
				string message = Library.Message.GetMessage("WorkflowMessages", "InvalidMoveMessage");
				Library.Utils.FlashMessage(message, caption, MessageButtons.OK, MessageIcon.Stop, MessageDefaultButton.Button1);
			}
			else
			{
				string caption = Library.Message.GetMessage("WorkflowMessages", "InvalidCopyCaption");
				string message = Library.Message.GetMessage("WorkflowMessages", "InvalidCopyMessage");
				Library.Utils.FlashMessage(message, caption, MessageButtons.OK, MessageIcon.Stop, MessageDefaultButton.Button1);
			}

			return false;
		}

		/// <summary>
		/// Allow the node to be pasted if it can follow the target node type.
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		/// <param name="isCut">if set to <c>true</c> [is cut].</param>
		/// <returns></returns>
		private bool DataIsValid(WorkflowNodeInternal from, WorkflowNodeInternal to, bool isCut)
		{
			string fromSource = from.GetRequiredDataSource();
			if (string.IsNullOrEmpty(fromSource)) return true;

			if (to.CreatedDataSources.Count > 0)
			{
				List<string> items = to.CreatedDataSources;
				if (items.IndexOf(fromSource) == 0) return true;
			}
			else
			{
				List<string> items = to.GetAvailableDataSources();
				if (items.IndexOf(fromSource) == 0) return true;
			}

			fromSource = Library.Utils.MakeName(fromSource);

			if (isCut)
			{
				string caption = Library.Message.GetMessage("WorkflowMessages", "InvalidMoveDataCaption");
				string message = Library.Message.GetMessage("WorkflowMessages", "InvalidMoveDataMessage", fromSource);
				Library.Utils.FlashMessage(message, caption, MessageButtons.OK, MessageIcon.Stop, MessageDefaultButton.Button1);
			}
			else
			{
				string caption = Library.Message.GetMessage("WorkflowMessages", "InvalidCopyDataCaption");
				string message = Library.Message.GetMessage("WorkflowMessages", "InvalidCopyDataMessage", fromSource);
				Library.Utils.FlashMessage(message, caption, MessageButtons.OK, MessageIcon.Stop, MessageDefaultButton.Button1);
			}

			return false;
		}

		/// <summary>
		/// Sets the current node.
		/// </summary>
		/// <param name="wfNode">The wf node.</param>
		private void SetCurrentNode(WorkflowNode wfNode)
		{
			if (wfNode == null)
			{
				m_Form.WorkflowNode.Publish(null);
				return;
			}

			try
			{
				m_Form.NodeParametersGrid.BeginUpdate();

				m_Form.NodeParametersCollection.Data.ItemPropertyChanged -= VisibleWorkflowNodeParameters_ItemPropertyChanged;

				m_Form.WorkflowNode.Publish(wfNode);
				m_Form.NodeParametersCollection.Publish(wfNode.VisibleWorkflowNodeParameters);

				m_Form.NodeParametersCollection.Data.ItemPropertyChanged += VisibleWorkflowNodeParameters_ItemPropertyChanged;

				// Set up the information on the right hand side

				m_Form.NodeIcon.SetImageByIconName(new IconName(wfNode.IconId));
				m_Form.NodeTypeDescription.Caption = wfNode.NodeTypeEntity.LongDescription;
				m_Form.ParameterDescriptionBox.Visible = wfNode.HasWorkflowNodeParameters;

				RefreshNodesToolBox(wfNode);

				// Update the value cell editor in the params grid

				SetParameterValueCellEditors();
			}
			finally
			{
				m_Form.NodeParametersGrid.EndUpdate();
			}
		}

		#endregion

		#region Save

		/// <summary>
		/// Called before the property sheet or wizard is saved.
		/// </summary>
		/// <returns>
		/// true to allow the save to continue, false to abort the save
		/// </returns>
		protected override bool OnPreSave()
		{
			// Check unique name.

			if (!m_Workflow.HasUniqueName)
			{
				string message = Library.Message.GetMessage("WorkflowMessages", "InvalidWorkflowName", m_Workflow.WorkflowName);
				Library.Utils.FlashMessage(message, Context.MenuItem.Description);
				return false;
			}

			// Validate the Nodes 

			if (!ValidWorkflow())
			{
				string message = Library.Message.GetMessage("WorkflowMessages", "InvalidWorkflow");
				Library.Utils.FlashMessage(message, Context.MenuItem.Description);
				return false;
			}

			// Reorder the nodes

			int orderNumber = 0;
			OrderChildren(m_Form.WorkflowNodeTree.Nodes, ref orderNumber);

			return base.OnPreSave();
		}

		/// <summary>
		/// Orders the children.
		/// </summary>
		/// <param name="nodes">The nodes.</param>
		/// <param name="orderNumber">The order number.</param>
		private void OrderChildren(SimpleTreeListNodeCollection nodes, ref int orderNumber)
		{
			foreach (SimpleTreeListNodeProxy treeNode in nodes)
			{
				WorkflowNode node = (WorkflowNode) treeNode.Data;
				m_Workflow.WorkflowNodes.MoveToPosition(node, orderNumber++, true);

				OrderChildren(treeNode.Nodes, ref orderNumber);
			}
		}

		#endregion

		#region Node Details

		/// <summary>
		/// Adds the new workflow node.
		/// </summary>
		/// <param name="nodeType">Type of the node.</param>
		private void AddNewWorkflowNode(WorkflowNodeTypeInternal nodeType)
		{
			// Get the focused tree node
			Library.Task.StateModified();

			SimpleTreeListNodeProxy focusedNode = m_Form.WorkflowNodeTree.FocusedNode;
			if (focusedNode == null) return;

			AddNewWorkflowNode(nodeType, focusedNode);

			RefreshNodesToolBox((WorkflowNode)focusedNode.Data);
		}

		/// <summary>
		/// Adds the new workflow node.
		/// </summary>
		/// <param name="nodeType">Type of the node.</param>
		/// <param name="focusedNode">The focused node.</param>
		private void AddNewWorkflowNode(WorkflowNodeTypeInternal nodeType, SimpleTreeListNodeProxy focusedNode)
		{
			// Create a new node and add it to the currently selected node's children

			WorkflowNode newNode = (WorkflowNode) EntityManager.CreateEntity(TableNames.WorkflowNode);

			WorkflowNode currentNode = ((WorkflowNode) focusedNode.Data);
			currentNode.WorkflowNodes.Add(newNode);
			newNode.SetNodeType(nodeType);

			// Add the new WF node to the tree under the focused node

			AddNodeToTree(newNode, focusedNode);

			// Specific code for If/Then/Else

			AddAutomaticChildNodes(newNode);
		}

		/// <summary>
		/// Adds the automatic child nodes.
		/// </summary>
		/// <param name="newNode">The new node.</param>
		private void AddAutomaticChildNodes(WorkflowNode newNode)
		{
			if (newNode.NodeType != IfThenElseNode.NodeType) return;

			WorkflowNode ifNode = (WorkflowNode) EntityManager.CreateEntity(TableNames.WorkflowNode);
			newNode.WorkflowNodes.Add(ifNode);
			ifNode.SetNodeType(FormulaConditionNode.NodeType);
			AddNodeToTree(ifNode);

			WorkflowNode elseNode = (WorkflowNode) EntityManager.CreateEntity(TableNames.WorkflowNode);
			newNode.WorkflowNodes.Add(elseNode);
			elseNode.SetNodeType(ElseNode.NodeType);
			AddNodeToTree(elseNode);

			SimpleTreeListNodeProxy proxy = m_NodeCache[ifNode];
			m_Form.WorkflowNodeTree.FocusNode(proxy);
		}

		#endregion

		#region Nodes ToolBox

		/// <summary>
		/// Populates the nodes tool box with all available node types.
		/// </summary>
		private void PopulateNodesToolBox()
		{
			try
			{
				m_NodesToolBox.BeginUpdate();

				// Pre load Groups to give some structure to the ordering

				LoadToolBoxGroups();

				foreach (WorkflowNodeTypeInternal nodeType in Library.Workflow.GetAllWorkflowNodeTypes())
				{
					// Skip inappropriate entries

					if (InvalidType(nodeType)) continue;

					// Get Node Properties

					string displayText = nodeType.TypeName;
					IconName icon = new IconName(nodeType.IconName);
					string category = nodeType.Category;
					string itemName = TextUtils.MakePascalCase(displayText);

					// Get Target Group

					ToolBoxGroup group = m_NodesToolBox.FindGroupByCaption(category) ??
					                     m_NodesToolBox.AddGroup(category, category, null);

					// Create toolBox Item

					m_NodesToolBox.AddItem(group, itemName, displayText, icon, null, nodeType, nodeType.LongDescription);
				}
			}
			finally
			{
				m_NodesToolBox.EndUpdate();
			}
		}

		/// <summary>
		/// Determines if the node type is valid for this 
		/// </summary>
		/// <param name="nodeType">Type of the node.</param>
		/// <returns></returns>
		private bool InvalidType(WorkflowNodeTypeInternal nodeType)
		{
			bool valid = true;
			if (nodeType.Hidden) return true;

			if (nodeType.ValidForTypes != null)
			{
				valid = false;

				foreach (string type in nodeType.ValidForTypes)
				{
					if (m_Workflow.WorkflowType.PhraseId == type)
					{
						valid = true;
						break;
					}
				}
			}

			return !valid;
		}

		/// <summary>
		/// Show/hide node types within the Nodes ToolBox based on the selected node within the Tree.
		/// </summary>
		/// <param name="node">The node.</param>
		private void RefreshNodesToolBox(WorkflowNode node)
		{
			try
			{
				m_NodesToolBox.BeginUpdate();

				// Get the valid follower list

				IEntityCollection followers = EntityManager.CreateEntityCollection(WorkflowNodeTypeInternal.EntityName);
				if (BaseEntity.IsValid(node)) followers = node.Followers;

				// Show/Hide toolBox Items based on the valid follower list

				foreach (ToolBoxGroup group in m_NodesToolBox.Groups)
				{
					foreach (ToolBoxItem item in group.Items)
					{
						// Just disable everything in display mode

						if (Context.LaunchMode == DisplayOption || Context.LaunchMode == SubmitOption)
						{
							item.Enabled = false;
							continue;
						}
						
						// Get the Node Type

						WorkflowNodeTypeInternal nodeType = (WorkflowNodeTypeInternal) item.Data;
						item.Enabled = followers.Contains(nodeType);

						// Diable if we already have a child unquie nodes.

						if (item.Enabled)
						{
							item.Enabled = ValidUniqueNode(node, nodeType);
						}
					}
				}
			}
			finally
			{
				m_NodesToolBox.EndUpdate();
			}
		}

		/// <summary>
		/// Check the node type is unique by searching it peers, children and direct parents.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="nodeType">Type of the node.</param>
		/// <returns></returns>
		public bool ValidUniqueNode(WorkflowNode node, WorkflowNodeTypeInternal nodeType )
		{
			if (!nodeType.IsUnique)
			{
				return true;
			}

			bool valid = !node.GetUnquieNodesTypes().Contains(nodeType);

			if (valid && !node.ParentNode.IsNull())
			{
				valid = ValidUniqueNodeParent((WorkflowNode)node.ParentNode, nodeType);
			}
			if (valid)
			{
				valid = ValidUniqueNodeChildren(node, nodeType);
			}
			return valid;
		}

		/// <summary>
		/// Check the nodes parents for the same type.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="nodeType">Type of the node.</param>
		/// <returns></returns>
		public bool ValidUniqueNodeParent(WorkflowNode node, WorkflowNodeTypeInternal nodeType)
		{
			bool valid = !node.GetUnquieNodesTypes().Contains(nodeType);

			if (valid && !node.ParentNode.IsNull())
			{
				valid = ValidUniqueNodeParent((WorkflowNode)node.ParentNode, nodeType);
			}
			return valid;
		}

		/// <summary>
		/// Check the nodes children for the sample type
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="nodeType">Type of the node.</param>
		/// <returns></returns>
		public bool ValidUniqueNodeChildren(WorkflowNode node, WorkflowNodeTypeInternal nodeType)
		{
			bool valid = !node.GetUnquieNodesTypes().Contains(nodeType);

			if (valid)
			{
				foreach (WorkflowNodeInternal child in node.GetChildNodes())
				{
					valid = ValidUniqueNodeChildren((WorkflowNode)child, nodeType);
					if (!valid)
					{
						break;
					}
				}
			}
			return valid;
		}


		/// <summary>
		/// Loads the tool box groups.
		/// </summary>
		private void LoadToolBoxGroups()
		{
			m_NodesToolBox.AddGroup("one", Library.Message.GetMessage("WorkflowMessages", "NodeCategoryOne"), null);
			m_NodesToolBox.AddGroup("two", Library.Message.GetMessage("WorkflowMessages", "NodeCategoryTwo"), null);
			m_NodesToolBox.AddGroup("three", Library.Message.GetMessage("WorkflowMessages", "NodeCategoryThree"), null);
			m_NodesToolBox.AddGroup("four", Library.Message.GetMessage("WorkflowMessages", "NodeCategoryFour"), null);
			m_NodesToolBox.AddGroup("five", Library.Message.GetMessage("WorkflowMessages", "NodeCategoryFive"), null);
			m_NodesToolBox.AddGroup("six", Library.Message.GetMessage("WorkflowMessages", "NodeCategorySix"), null);
			m_NodesToolBox.AddGroup("seven", Library.Message.GetMessage("WorkflowMessages", "NodeCategorySeven"), null);
		}

		/// <summary>
		/// Handles the ItemClicked event of the m_NodesToolBox control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ToolBoxItemClickedEventArgs"/> instance containing the event data.</param>
		private void NodesToolBoxItemClicked(object sender, ToolBoxItemClickedEventArgs e)
		{
			WorkflowNodeTypeInternal nodeType = (WorkflowNodeTypeInternal) e.Item.Data;
			AddNewWorkflowNode(nodeType);
		}

		#endregion

		#region Validation

		/// <summary>
		/// Validates the entire workflow
		/// </summary>
		/// <returns>True if the workflow is valid</returns>
		private bool ValidWorkflow()
		{
			if (m_Workflow.Valid) return true;

			// Show the invalid nodes

			bool first = true;

			foreach (WorkflowNode node in m_Workflow.InvalidNodes)
			{
				var treeNode = m_Form.WorkflowNodeTree.FindNode(node);
				if (treeNode == null) continue;

				treeNode.OverlayIcon(new IconName("INT_OVERLAY_STOP_BL"));

				// Focus on the First problematic node

				if (first)
				{
					treeNode.Focus();
					first = false;
				}
			}

			return false;
		}

		#endregion

		#region Parameters

		/// <summary>
		/// Handles the ItemPropertyChanged event of the VisibleWorkflowNodeParameters control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void VisibleWorkflowNodeParameters_ItemPropertyChanged(object sender, EntityCollectionEventArgs e)
		{
			SetParameterValueCellEditors(true);
		}

		/// <summary>
		/// Sets the parameter value cell editor.
		/// </summary>
		private void SetParameterValueCellEditors(bool refreshing = false)
		{
			WorkflowNode node = (WorkflowNode) m_Form.WorkflowNode.Data;
			if (!BaseEntity.IsValid(node)) return;

			foreach (IEntity entity in m_Form.NodeParametersGrid.GridData)
			{
				WorkflowNodeParameter param = (WorkflowNodeParameter) entity;

				if (BaseEntity.IsValid(param))
				{
					if (param.Attribute.ForceRefresh)
					{
						EventHandler forceRefreshHandler = (s, e) => { m_ValueGridColumn.Grid.ForceRefresh(); };

						param.Changed -= forceRefreshHandler;
						param.Changed += forceRefreshHandler;

					}



					if (refreshing && !param.VolatileContents) continue;
					
					try
					{
						node.LinkParameter(param);
						param.Attribute.BuildCellBrowse(this, node, entity, m_ValueGridColumn);
					}
					catch (Exception e)
					{
						Logger.ErrorFormat("Error Setting Parameter {0} - {1}", param.ParameterName, e.Message);
						Logger.Error(e.Message, e);

						string format = Library.Message.GetMessage("WorkflowMessages", "ErrorParameterPrompt");
						string message = string.Format(format, param.ParameterName);
						Library.Utils.ShowAlert(message, "TEXT_TREE_BUG", e.Message);

						m_ValueGridColumn.DisableCell(entity);
						continue;
					}

					if (param.ReadOnly)
					{
						m_ValueGridColumn.DisableCell(entity);
					}

					if (param.Mandatory)
					{
						m_ValueGridColumn.SetCellMandatory(param);
					}
				}
			}
		}

		/// <summary>
		/// Handles the FocusedEntityChanged event of the NodeParametersGrid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.EntityEventArgs"/> instance containing the event data.</param>
		private void NodeParametersGrid_FocusedRowChanged(object sender, DataGridFocusedRowChangedEventArgs e)
		{
			if (e.Row != null)
			{
				WorkflowNodeParameterInternal param = (WorkflowNodeParameterInternal) e.Row;
				m_Form.ParameterDescription.Caption = param.LongDescription;
			}
		}

		/// <summary>
		/// Handles the NodeMoved event of the WorkflowNodeTree control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.SimpleTreeListNodeMovedEventArgs"/> instance containing the event data.</param>
		private void WorkflowNodeTree_NodeMoved(object sender, SimpleTreeListNodeMovedEventArgs e)
		{
			WorkflowNode movedNode = (WorkflowNode) e.Node.Data;
			WorkflowNode parentNode = (WorkflowNode) movedNode.ParentNode;

			if (e.MoveUp)
			{
				parentNode.WorkflowNodes.MoveUp(movedNode);
			}
			else
			{
				parentNode.WorkflowNodes.MoveDown(movedNode);
			}
		}

		#endregion

		#region Context Menu Tab

		/// <summary>
		/// Menus tab initialize.
		/// </summary>
		private void MenuTabInitialize()
		{
			if (!m_Workflow.IncludeInMenu) return;
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
			if (!ShowMenuTab()) return;

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
			if (string.IsNullOrEmpty(m_Workflow.TableName)) return;

			HashSet<string> browseStrings = new HashSet<string>();

			// Add workflow action type menus

			IQuery query = EntityManager.CreateQuery(WorkflowActionTypeBase.EntityName);
			query.AddEquals(WorkflowActionTypePropertyNames.TableName, m_Workflow.TableName);

			IEntityCollection actions = EntityManager.Select(WorkflowActionTypeBase.EntityName, query);

			foreach (WorkflowActionType action in actions)
			{
				BuildBrowseStrings(action.SubMenuGroup, browseStrings);
			}

			// Add workflow menus

			query = EntityManager.CreateQuery(WorkflowBase.EntityName);
			query.AddEquals(WorkflowPropertyNames.TableName, m_Workflow.TableName);

			actions = EntityManager.Select(WorkflowBase.EntityName, query);

			foreach (Workflow action in actions)
			{
				BuildBrowseStrings(action.SubMenuGroup, browseStrings);
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
		private static void BuildBrowseStrings(string subMenu, HashSet<string> browseStrings)
		{
			for (int pos = subMenu.IndexOf('/'); pos >= 0; pos = subMenu.IndexOf('/', pos + 1))
			{
				browseStrings.Add(subMenu.Substring(0, pos));
			}

			browseStrings.Add(subMenu);
		}

		#endregion

		#region Copy

		/// <summary>
		/// Copy option.
		/// </summary>
		protected override void Copy()
		{
			SetAsModified = true;
			base.Copy();
		}

		#endregion

		#region Toolbox Drop

		/// <summary>
		/// Handles the ToolboxItemDropped event of the WorkflowNodeTree control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="SimpleTreeListToolboxEventArgs"/> instance containing the event data.</param>
		private void WorkflowNodeTree_DropTransferToolboxItem(object sender, SimpleTreeListToolboxEventArgs e)
		{
			var item = m_Form.ToolBoxNodes.FindItemByName(e.ToolboxItemName);
			if (item == null) return;

			WorkflowNodeTypeInternal nodeType = (WorkflowNodeTypeInternal) item.Data;
			if (nodeType == null) return;

			WorkflowNodeInternal node = (WorkflowNodeInternal) e.Node.Data;
			if (node == null) return;

			// Add a new node of the specified type

			if (node.Followers.Contains(nodeType))
			{
				AddNewWorkflowNode(nodeType, e.Node);
				return;
			}

			// If we can't drop it into this node, put it at the same level

			if (node.ParentNodeInternal != null)
			{
				if (node.ParentNodeInternal.Followers.Contains(nodeType))
				{
					AddNewWorkflowNode(nodeType, e.Node.ParentNode);
				}
			}
		}

		#endregion
	}
}
