using System;
using System.Collections.Generic;
using System.Linq;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.ClientControls.Validation;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;
using DataGrid = Thermo.SampleManager.Library.ClientControls.DataGrid;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of Explorer Folder
	/// </summary>
	[SampleManagerTask("TableDefaultsTask", "LABTABLE", "EXPLORER_FOLDER")]
	public class TableDefaultsTask : GenericLabtableTask
	{
		#region Member Variables

		private readonly Logger m_Logger = Logger.GetInstance(typeof(TableDefaultsTask));
		private ExplorerRmb m_CurrentRmb;
		private ExplorerFolder m_ExplorerFolder;
		private FormTableDefaults m_Form;
		private bool m_RmbUsingEllipsisActive;
		private bool m_RmbUsingEllipsisVisible;
		#endregion

		#region Overriden Functionality

		/// <summary>
		/// Creates the new entity.
		/// </summary>
		/// <returns></returns>
		protected override IEntity CreateNewEntity()
		{
			ExplorerCabinet cabinet = null;

			// If this task is called from the explorer the cabinet and folder will be set in the context object
			if (!string.IsNullOrEmpty(Context.ExplorerCabinet))
			{
				cabinet = (ExplorerCabinet)EntityManager.Select(TableNames.ExplorerCabinet, Context.ExplorerCabinet);

				if (cabinet != null && Context.ExplorerFolderNumber > 0)
				{
					ExplorerFolder contextFolder =
						(ExplorerFolder)
						EntityManager.Select(TableNames.ExplorerFolder,
											 new Identity(Context.ExplorerCabinet, Context.ExplorerFolderNumber));

					// If this is a folder of folders then use the TABLE_DETAILS cabinet.
					if (contextFolder != null && contextFolder.TableName == TableNames.ExplorerFolder)
						cabinet = (ExplorerCabinet)EntityManager.Select(TableNames.ExplorerCabinet, "TABLE_DETAILS");
				}
			}

			// Create the folder and fill in some default values for the key. 
			ExplorerFolder folder = (ExplorerFolder)EntityManager.CreateEntity(TableNames.ExplorerFolder);

			// Setup the folder and order numbers based on other folders in the cabinet
			SetFolderNumber(cabinet, folder);

			// The cabinet is selected from the key in the context object.
			folder.Cabinet = cabinet;

			return folder;
		}

		/// <summary>
		/// Sets the folder number.
		/// </summary>
		/// <param name="cabinet">The cabinet.</param>
		/// <param name="folder">The folder.</param>
		private void SetFolderNumber(ExplorerCabinet cabinet, ExplorerFolder folder)
		{
			int folderNumber = 1;
			int orderNumber = 1;

			// Go through each folder in the parent cabinet and select the next highest number.

			foreach (ExplorerFolder sibling in cabinet.Folders)
			{
				if (!sibling.Equals(folder))
				{
					try
					{
						int orderNum = (int)sibling.OrderNumber;
						int folderNum = (int)sibling.FolderNumber;

						orderNumber = (orderNum >= orderNumber) ? orderNum + 1 : orderNumber;
						folderNumber = (folderNum >= folderNumber) ? folderNum + 1 : folderNumber;
					}
					catch (Exception e)
					{
						m_Logger.DebugFormat("Exception whilst setting {0}.{1} folder number - {2}", cabinet.Name, folder.Name, e.Message);
						m_Logger.Debug("Exception", e);
					}
				}
			}

			folder.OrderNumber = orderNumber;
			folder.FolderNumber = folderNumber;
		}

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			// Cast the main form and entity to a more specific class.

			m_Form = (FormTableDefaults)MainForm;
			m_ExplorerFolder = (ExplorerFolder)MainForm.Entity;

			// Track the RMB tree list changes

			m_Form.RMBTreeList.FocusedNodeChanged += RMBTreeListFocusedNodeChanged;
			m_Form.RMBTreeList.NodeAdding += RMBTreeListNodeAdding;
			m_Form.RMBTreeList.NodePasting += RMBTreeListNodePasting;

			m_Form.EditMasterMenuButton.Click += EditMasterMenuButtonClick;

			// If the table name is valid then setup the dependant browses. During an add operation the table will
			// initially be blank so these browses will be setup in property changed event.

			if (!string.IsNullOrEmpty(m_ExplorerFolder.TableName))
			{
				m_Form.ContextFieldNames.Republish(m_ExplorerFolder.TableName);
			}

			SetDefaultActionBrowse();

			// Set default property values.
			m_Form.RMBEsigLevel.Enabled = false;
			m_Form.RMBEsigReason.Enabled = false;

			if (m_ExplorerFolder.Rmbs.Count == 0)
				DisableRmbPrompts();

			m_ExplorerFolder.PropertyChanged += ExplorerFolderPropertyChanged;
			m_Form.TableNameValidator.Validate += TableNameValidatorValidate;

			// Only allow changing the table name in add mode
			m_Form.TableNameEdit.Enabled = (Context.LaunchMode == AddOption);

			EnableDefaultReportPrompt();

			// The columns tab
			m_Form.ColumnGrid.CellEditor += ColumnGridCellEditor;
			m_Form.ColumnGrid.CellLeave += ColumnGridCellLeave;

			m_Form.ExplorerFormTaskPrompt.Browse = BrowseFactory.CreateStringBrowse(Library.Task.GetTaskList());
		}

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			base.MainFormLoaded();

			SetupTemplatesTab();
			SetupLinksTab();
			SetupAppearanceTab();
			SetupRmbUsingBrowser();
			// The columns tab
			m_Form.ColumnGrid.Enabled = (m_ExplorerFolder.ColumnMode.PhraseId == PhraseColumns.PhraseIdFIXED);
		}

		/// <summary>
		/// Handles the Validate event of the TableNameValidator control.
		/// </summary>
		/// <remarks>
		/// Used to make sure that only one global defaults record exists per table.
		/// </remarks>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.Validation.ServerValidatorEventArgs"/> instance containing the event data.</param>
		private void TableNameValidatorValidate(object sender, ServerValidatorEventArgs e)
		{
			if (m_ExplorerFolder.Cabinet.Identity == "TABLE_DETAILS")
			{
				// Go through each folder in the parent cabinet and check for another table the same.
				foreach (ExplorerFolder folder in m_ExplorerFolder.Cabinet.Folders)
				{
					if (!m_ExplorerFolder.Equals(folder))
					{
						if (m_ExplorerFolder.TableName == folder.TableName)
						{
							e.Valid = false;
							e.ErrorMessage = string.Format(m_Form.StringTable1.GlobalDefaultsDuplicateTable, folder.Name, folder.TableName);
							break;
						}
					}
				}
			}
		}

		/// <summary>
		/// Handles the PropertyChanged event of the ExplorerFolder entity.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.PropertyEventArgs"/> instance containing the event data.</param>
		private void ExplorerFolderPropertyChanged(object sender, PropertyEventArgs e)
		{
			// If the table name changes then setup the field browse.
			if (e.PropertyName == ExplorerFolderPropertyNames.TableName && !string.IsNullOrEmpty(m_ExplorerFolder.TableName))
			{
				// Rebuild the context browse on RMBs
				m_Form.ContextFieldNames.Republish(m_ExplorerFolder.TableName);

				// Update the criteria
				m_ExplorerFolder.ResetCriteriaTable(m_ExplorerFolder.TableName);
				m_Form.FolderCriteriaEditor.SetCriteria(m_ExplorerFolder.CriteriaSaved);

				// Reset the hierarchy
				m_ExplorerFolder.HierarchyLink = null;

				// Update the title on a global table default editor
				if (m_ExplorerFolder.Cabinet.Identity == "TABLE_DETAILS")
					m_Form.Title = m_Form.StringTable1.GlobalDefaultsTitle + m_ExplorerFolder.TableName;

				SetupTemplatesTab();

				// Default Actions

				SetDefaultActionBrowse();
				m_ExplorerFolder.DefaultAction = null;

				SetupLinksTab();
			}
			if (e.PropertyName == ExplorerFolderPropertyNames.TableName && string.IsNullOrEmpty(m_ExplorerFolder.TableName))
			{
				SetDefaultActionBrowse();
				m_ExplorerFolder.DefaultAction = null;
			}
			else if (e.PropertyName == ExplorerFolderPropertyNames.ColumnMode)
			{
				m_Form.ColumnGrid.Enabled = (m_ExplorerFolder.ColumnMode.PhraseId == PhraseColumns.PhraseIdFIXED);
			}

			//Set Default Report to enabled / disabled

			EnableDefaultReportPrompt();
		}

		/// <summary>
		/// Sets the default action browse.
		/// </summary>
		private void SetDefaultActionBrowse()
		{
			if (string.IsNullOrEmpty(m_ExplorerFolder.TableName))
			{
				IEntityCollection nothing = EntityManager.CreateEntityCollection(MasterMenu.EntityName);
				m_Form.PromptDefaultAction.Browse = BrowseFactory.CreateEntityBrowse(nothing);
			}
			else
			{
				IQuery menuItems = EntityManager.CreateQuery(TableNames.MasterMenu);
				menuItems.AddEquals(MasterMenuPropertyNames.TableName, m_ExplorerFolder.TableName);
				m_Form.PromptDefaultAction.Browse = BrowseFactory.CreateEntityBrowse(menuItems);
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
			// The tree updates the rmb list using the order_number as the link to the parent_number.
			// The order_number automatically updated at commit time which will mean the parent
			// numbers are wrong. This PreSave override copies the current order_numbers then forces a 
			// manual update of ordering so it can correct the parent numbers.

			Dictionary<PackedDecimal, ExplorerRmb> oldRmbNumbers = new Dictionary<PackedDecimal, ExplorerRmb>();

			foreach (ExplorerRmb rmb in m_ExplorerFolder.Rmbs)
			{
				oldRmbNumbers.Add(rmb.OrderNumber, rmb);
			}

			m_ExplorerFolder.Rmbs.UpdateOrderNumbers();

			foreach (ExplorerRmb rmb in m_ExplorerFolder.Rmbs)
			{
				if (oldRmbNumbers.ContainsKey(rmb.ParentNumber))
				{
					rmb.ParentNumber = oldRmbNumbers[rmb.ParentNumber].OrderNumber;
				}
			}

			SaveTemplateFields();
			SaveFolderLinks();

			// Sort out the column list

			m_ExplorerFolder.Columns.UpdateOrderNumbers();

			foreach (ExplorerColumn column in m_ExplorerFolder.Columns)
			{
				column.ColumnNumber = column.OrderNumber;
			}

			return base.OnPreSave();
		}



		#endregion

		#region Setup Controls

		/// <summary>
		/// Disables the RMB prompts.
		/// </summary>
		private void DisableRmbPrompts()
		{
			EnableRmbPrompts(null);
		}

		/// <summary>
		/// Enables all controls.
		/// </summary>
		/// <param name="explorerRmb">The explorer RMB.</param>
		private void EnableRmbPrompts(ExplorerRmb explorerRmb)
		{
			bool isItem = false;
			bool isGroup = false;
			bool isValidRmb = BaseEntity.IsValid(explorerRmb);

			if (isValidRmb)
			{
				isItem = explorerRmb.IsItem;
				isGroup = explorerRmb.IsGroup;
			}

			m_Form.OptionsGroupBox.Enabled = isItem;
			m_Form.RMBRadioButtonItem.Enabled = isValidRmb && !explorerRmb.IsGroup;
			m_Form.RMBRadioButtonGroup.Enabled = isValidRmb && !explorerRmb.IsGroup;
			m_Form.RMBRadioButtonSeparator.Enabled = isValidRmb && !explorerRmb.IsGroup;
			m_Form.RMBLineProperties.Enabled = isValidRmb;
			m_Form.RMBDescription.Enabled = isItem;
			m_Form.ContextGrid.Enabled = isItem;
			m_Form.RMBGroupId.Enabled = isItem;
			m_Form.RMBMenuProc.Enabled = isItem;
			m_Form.RMBName.Enabled = isItem || isGroup;
			m_Form.RMBUsing.Enabled = isItem;
			m_RmbUsingEllipsisActive = isItem;
			m_Form.usingTree.Visible = false;
			m_RmbUsingEllipsisVisible = false;
		}

		/// <summary>
		/// Enables the default report prompt.
		/// </summary>
		private void EnableDefaultReportPrompt()
		{
			m_Form.PromptDefaultReport.Enabled = !string.IsNullOrEmpty(m_ExplorerFolder.TableName);
		}

		/// <summary>
		/// Sets the master menu properties.
		/// </summary>
		/// <param name="entity">The entity.</param>
		private void SetMasterMenuProperties(ExplorerRmb entity)
		{
			MasterMenu masterMenu = entity.Menuproc as MasterMenu;

			if (masterMenu != null && !masterMenu.IsNull())
			{
				IEntityCollection rolesCollection = masterMenu.RoleEntries;

				m_Form.MasterMenuRolesCollection.Publish(rolesCollection);
				m_Form.RMBEsigLevel.Text = masterMenu.EsigLevel.PhraseId;
				m_Form.RMBEsigReason.Text = masterMenu.EsigReason;
			}
		}

		#endregion

		#region RMB Tab

		/// <summary>
		/// Setups the RMB using browser.
		/// </summary>
		/// <summary>
		/// Setups the RMB using browser.
		/// </summary>
		private void SetupRmbUsingBrowser()
		{
			m_Form.usingTree.Visible = false;

			//open and close of dropdown
			m_Form.RMBUsing.ButtonClick += (s, e) =>
			{
				if (m_RmbUsingEllipsisActive)
				{
					m_RmbUsingEllipsisVisible ^= true;
					m_Form.usingTree.Visible = m_RmbUsingEllipsisVisible;
				}
			};

			EventHandler<EventArgs> lostFocusEvent = (s, e) =>
			{
				m_Form.usingTree.Visible = false;
				m_RmbUsingEllipsisVisible = false;
			};

			m_Form.usingTree.Leave += lostFocusEvent;
			

			//order fields for dropdown
			List<string> fields = null;
			try
			{
				fields = (from object field in Schema.Current.Tables[m_ExplorerFolder.TableName].Fields
						  select field.ToString()).OrderBy(n => n).ToList();
			}
			catch
			{
				//empty catch - somethings gone wrong; fields will be null
			}

			if (fields == null || fields.Count == 0)
			{
				m_RmbUsingEllipsisActive= false;	//no ellipsis
			}
			else
			{
				m_RmbUsingEllipsisActive = true;
				foreach (var field in fields)
				{
					m_Form.usingTree.AddNode(null, field, new IconName("INT_DATA_ROWS"));
				}

				//add additional tool items
				foreach (var field in new List<string> { "$DO", "$EXIT", "$UP", "$DOWN", "$LEFT", "$OPERATOR_ID", "$DATE"})
				{
					m_Form.usingTree.AddNode(null, field, new IconName("INT_TOOLS"));
				}

				//selection
				m_Form.usingTree.MouseDoubleClick += (s, e) =>
				{
					var node = m_Form.usingTree.FocusedNode;
					if (node != null)
					{
						m_Form.RMBUsing.Text += "," + node.DisplayText;
						m_Form.RMBUsing.Text = m_Form.RMBUsing.Text.Trim(',');
					}
				};
			}
		}

		/// <summary>
		/// Handles the FocusedNodeChanged event of the RMBTreeList control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.FocusedNodeChangedEventArgs"/> instance containing the event data.</param>
		private void RMBTreeListFocusedNodeChanged(object sender, FocusedNodeChangedEventArgs e)
		{
			if (m_CurrentRmb != null)
			{
				m_CurrentRmb.PropertyChanged -= RMBEntityDataPropertyChanged;
			}

			if (e.Entity == null)
			{
				// No RMB selected

				DisableRmbPrompts();

				m_CurrentRmb = null;

				return;
			}

			// RMB Is Selected

			m_CurrentRmb = (ExplorerRmb)e.Entity;

			EnableRmbPrompts(m_CurrentRmb);
			SetMasterMenuProperties(m_CurrentRmb);
			SetRmbContextPrompts(m_CurrentRmb);

			m_Form.ContextGrid.GridData = m_CurrentRmb.Context;
			m_CurrentRmb.PropertyChanged += RMBEntityDataPropertyChanged;
		}

		/// <summary>
		/// Handles the NodeAdding event of the RMBTreeList control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.TreeListNodeCancelEventArgs"/> instance containing the event data.</param>
		private void RMBTreeListNodeAdding(object sender, TreeListNodeCancelEventArgs e)
		{
			e.Cancel = !ValidateTargetRmb(e.Entity as ExplorerRmb);
		}

		/// <summary>
		/// Handles the NodePasting event of the RMBTreeList control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.TreeListNodePastingEventArgs"/> instance containing the event data.</param>
		private void RMBTreeListNodePasting(object sender, TreeListNodePastingEventArgs e)
		{
			e.Cancel = !ValidateTargetRmb(e.TargetEntity as ExplorerRmb);
		}

		/// <summary>
		/// Validates the target RMB.
		/// </summary>
		/// <param name="target">The target.</param>
		private bool ValidateTargetRmb(ExplorerRmb target)
		{
			if (target == null || target.IsGroup)
			{
				// This is ok

				return true;
			}

			// Cannot add children to items or separators
			string message = Library.Message.GetMessage("LaboratoryMessages", "InvalidRmbTarget");
			Library.Utils.FlashMessage(message, m_Form.Title, MessageButtons.OK, MessageIcon.Error, MessageDefaultButton.Button1);
			return false;
		}

		/// <summary>
		/// Handles the PropertyChanged event of the RMBEntityData control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.PropertyEventArgs"/> instance containing the event data.</param>
		private void RMBEntityDataPropertyChanged(object sender, PropertyEventArgs e)
		{
			ExplorerRmb explorerRmb = (ExplorerRmb)sender;

			if (e.PropertyName == ExplorerRmbPropertyNames.Menuproc)
			{
				explorerRmb.Description = explorerRmb.Menuproc.Description;
				explorerRmb.ExplorerRmbName = explorerRmb.Menuproc.ShortText;
				SetMasterMenuProperties(explorerRmb);
			}
			else if (e.PropertyName == ExplorerRmbPropertyNames.Type)
			{
				EnableRmbPrompts(explorerRmb);
			}
			else if (e.PropertyName == ExplorerRmbPropertyNames.ContextField)
			{
				explorerRmb.ContextItem.Value = null;
				SetRmbContextPrompts(explorerRmb);
			}
			else if (e.PropertyName == ExplorerRmbPropertyNames.ContextOperator)
			{
				SetRmbContextPrompts(explorerRmb);
			}
		}

		/// <summary>
		/// Sets the RMB context prompts.
		/// </summary>
		/// <param name="explorerRmb">The explorer RMB.</param>
		private void SetRmbContextPrompts(ExplorerRmb explorerRmb)
		{
			var col = m_Form.ContextGrid.GetColumnByProperty(ExplorerRmbContext.PropertyValue);

			// Disable if nothing filled in

			if (string.IsNullOrEmpty(explorerRmb.ContextField))
			{
				col.DisableCell(explorerRmb.ContextItem);
				return;
			}

			ExplorerRmbContext context = explorerRmb.ContextItem;

			// LIKE, IN, NOT LIKE, NOT IN

			if (context.IsText)
			{
				col.SetCellEditorFromObjectModel(context, ExplorerRmb.EntityName, ExplorerRmbPropertyNames.ContextValue);
				return;
			}

			// Otherwise pick up the information from the object model

			if (string.IsNullOrEmpty(context.PropertyName)) return;
			col.SetCellEditorFromObjectModel(context, m_ExplorerFolder.TableName, explorerRmb.ContextField);
		}

		/// <summary>
		/// Handles the Click event of the EditMasterMenuButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void EditMasterMenuButtonClick(object sender, EventArgs e)
		{
			if (BaseEntity.IsValid(m_CurrentRmb))
			{
				Library.Task.CreateTask(923, m_CurrentRmb.Menuproc);
			}
		}

		#endregion

		#region Appearance Tab

		private void SetupAppearanceTab()
		{
			m_Form.DataIconGrid.CellEditor += DataIconGridCellEditor;

			m_Form.ExpressionsGrid.CellButtonClicked += ExpressionsGridCellButtonClicked;
		}

		void ExpressionsGridCellButtonClicked(object sender, DataGridCellButtonClickedEventArgs e)
		{
			string promptValue = e.Entity.Get(e.Property.Property).ToString();

			string newPromptValue = Library.Formula.EditFormula(m_ExplorerFolder.TableName, promptValue, m_ExplorerFolder.TableName, string.Empty);
			
			if (promptValue != newPromptValue)
			{
				((DataGrid)sender).WriteValue(e.Entity, e.Property, newPromptValue);
			}
		}

		/// <summary>
		/// Handles the CellEditor event of the DataIconGrid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.CellEditorEventArgs"/> instance containing the event data.</param>
		private void DataIconGridCellEditor(object sender, CellEditorEventArgs e)
		{
			ExplorerFolderIcon icon = (ExplorerFolderIcon)e.Entity;

			if (e.PropertyName == ExplorerFolderIconPropertyNames.FieldName)
			{
				if (!string.IsNullOrEmpty(m_ExplorerFolder.TableName))
					e.Browse = BrowseFactory.CreateFieldNameBrowse(m_ExplorerFolder.TableName);
			}
			else if (e.PropertyName == ExplorerFolderIconPropertyNames.Value)
			{
				e.DataType = SMDataType.Text;

				if (!string.IsNullOrEmpty(m_ExplorerFolder.TableName) &&
					 !string.IsNullOrEmpty(icon.FieldName))
				{
					if (icon.Operator.PhraseId != PhraseCritOp.PhraseIdLIKE)
					{
						e.SetFromSchema(m_ExplorerFolder.TableName, icon.FieldName);
					}
				}
			}
		}

		#endregion

		#region Links Tab

		/// <summary>
		/// Sets up the links tab.
		/// </summary>
		private void SetupLinksTab()
		{
			if (string.IsNullOrEmpty(m_ExplorerFolder.TableName)) return;

			m_Form.LinkGrid.BeginUpdate();
			m_Form.LinkGrid.ClearRows();

			IList<string> properties = EntityType.GetReflectedPropertyNames(m_ExplorerFolder.TableName, true, false, false);

			if (m_ExplorerFolder.TableName == TableNames.Attachment && properties.Contains("Pages"))
				properties.Remove("Pages");

			foreach (string property in properties.OrderBy(name => name))
			{
				UnboundGridRow row = m_Form.LinkGrid.AddRow(null);
				row[m_Form.LinkGrid.Columns[0]] = m_ExplorerFolder.FolderLinks.Contains(ExplorerFolderLinkPropertyNames.CollectionName, property);
				row[m_Form.LinkGrid.Columns[1]] = property;
			}

			m_Form.LinkGrid.EndUpdate();
			m_Form.LinkGrid.CellValueChanged += LinkGridCellValueChanged;

			m_Form.TreeDisplayFormula.ButtonClick += TreeDisplayFormulaButtonClick;
			m_Form.TreeDisplayFormula.TextChanged += TreeDisplayFormulaTextChanged;
		}

		private void TreeDisplayFormulaTextChanged(object sender, TextChangedEventArgs e)
		{
			if (!Library.Formula.Validate(m_ExplorerFolder, m_ExplorerFolder.TreeDisplayField))
			{
				m_Form.TreeDisplayFormula.ShowError(Library.Message.GetMessage("LaboratoryMessages", "TreeDisplayFormulaError"));
			}
		}

		private void TreeDisplayFormulaButtonClick(object sender, EventArgs e)
		{
			m_ExplorerFolder.TreeDisplayField = Library.Formula.EditFormula(m_ExplorerFolder.TableName, m_ExplorerFolder.TreeDisplayField);
		}

		private void LinkGridCellValueChanged(object sender, UnboundGridValueChangedEventArgs e)
		{
			Library.Task.StateModified();
		}

		private void SaveFolderLinks()
		{
			m_ExplorerFolder.FolderLinks.RemoveAll();

			foreach (UnboundGridRow row in m_Form.LinkGrid.Rows)
			{
				if ((bool)row[m_Form.LinkGrid.Columns[0]])
				{
					ExplorerFolderLink link = (ExplorerFolderLink)EntityManager.CreateEntity(ExplorerFolderLinkBase.EntityName);

					link.CollectionName = (string)row[m_Form.LinkGrid.Columns[1]];

					m_ExplorerFolder.FolderLinks.Add(link);
				}
			}
		}

		#endregion

		#region Template Fields

		private void SetupTemplatesTab()
		{
			if (!EntityTemplateSupported())
			{
				m_Form.PageTemplateFields.Visible = false;
				return;
			}

			m_Form.PageTemplateFields.Visible = true;

			IQuery query = EntityManager.CreateQuery(EntityTemplateField.EntityName);
			query.AddEquals(EntityTemplateFieldPropertyNames.TableName, m_ExplorerFolder.TableName);

			IEntityCollection fields = EntityManager.Select(EntityTemplateField.EntityName, query);

			m_Form.TemplateFieldsGrid.BeginUpdate();

			m_Form.TemplateFieldsGrid.ClearRows();

			IList<string> properties = EntityType.GetReflectedPropertyNames(m_ExplorerFolder.TableName, false);

			foreach (string property in properties.OrderBy(name => name))
			{
				UnboundGridRow row = m_Form.TemplateFieldsGrid.AddRow(null);
				row[m_Form.TemplateFieldsGrid.Columns[0]] = fields.Contains(EntityTemplateFieldPropertyNames.FieldName, property);
				row[m_Form.TemplateFieldsGrid.Columns[1]] = property;
			}

			m_Form.TemplateFieldsGrid.EndUpdate();
			m_Form.TemplateFieldsGrid.CellValueChanged += TemplateFieldsGridCellValueChanged;
		}

		void TemplateFieldsGridCellValueChanged(object sender, UnboundGridValueChangedEventArgs e)
		{
			Library.Task.StateModified();
		}

		private void SaveTemplateFields()
		{
			IQuery query = EntityManager.CreateQuery(EntityTemplateField.EntityName);
			query.AddEquals(EntityTemplateFieldPropertyNames.TableName, m_ExplorerFolder.TableName);

			IEntityCollection fields = EntityManager.Select(EntityTemplateField.EntityName, query);

			foreach (IEntity field in fields)
			{
				EntityManager.Delete(field);
			}

			foreach (UnboundGridRow row in m_Form.TemplateFieldsGrid.Rows)
			{
				if ((bool)row[m_Form.TemplateFieldsGrid.Columns[0]])
				{
					EntityTemplateField field = (EntityTemplateField)EntityManager.CreateEntity(EntityTemplateField.EntityName);

					field.TableName = m_ExplorerFolder.TableName;
					field.FieldName = (string)row[m_Form.TemplateFieldsGrid.Columns[1]];

					EntityManager.Transaction.Add(field);
				}
			}
		}

		private bool EntityTemplateSupported()
		{

			if (string.IsNullOrEmpty(m_ExplorerFolder.TableName)) return false;

			// Check table supports Entity Templates
			IQuery query = EntityManager.CreateQuery(TableNames.Phrase);
			query.AddEquals(PhrasePropertyNames.Phrase, PhraseEntTmpTy.Identity);
			query.AddEquals(PhrasePropertyNames.PhraseText, m_ExplorerFolder.TableName);
			IEntityCollection phrases = EntityManager.Select(TableNames.Phrase, query);

			return (phrases.Count != 0);
		}

		#endregion

		#region Columns Tab

		/// <summary>
		/// Column grid cell editor.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.CellEditorEventArgs"/> instance containing the event data.</param>
		private void ColumnGridCellEditor(object sender, CellEditorEventArgs e)
		{
			if (e.ColumnName == ExplorerColumnPropertyNames.FieldName)
			{
				if (!string.IsNullOrEmpty(m_ExplorerFolder.TableName))
					e.Browse = BrowseFactory.CreatePropertyNameBrowse(m_ExplorerFolder.TableName, PropertyFilter.NonCollection, true);
			}
		}

		/// <summary>
		/// Column grid leave cell.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.CellEventArgs"/> instance containing the event data.</param>
		private void ColumnGridCellLeave(object sender, CellEventArgs e)
		{
			if (e.ColumnName == ExplorerColumnPropertyNames.FieldName)
			{
				ExplorerColumn column = (ExplorerColumn)e.Entity;

				if (column != null)
				{
					if (string.IsNullOrWhiteSpace(column.ExplorerColumnName))
					{
						column.ExplorerColumnName = column.FieldName;

						if ((column.Width == 10) || (column.Width == 0))
							column.Width = 100;

						m_Form.ColumnGrid.RefreshRow(e.Entity);
					}
				}
			}
		}

		#endregion
	}
}
