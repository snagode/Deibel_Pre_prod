using System;
using System.IO;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.ClientControls.Browse;
using Thermo.SampleManager.Library.ClientControls.Data;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;
using Form = Thermo.SampleManager.Library.ClientControls.Form;
using ToolBar = Thermo.SampleManager.Library.ClientControls.ToolBar;
using ToolBarButton = Thermo.SampleManager.Library.ClientControls.ToolBarButton;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Attachment Page allows Attachments to me modified on a property sheet.
	/// </summary>
	[SampleManagerPage("AttachmentPage")]
	public class AttachmentPage : PageBase
	{
		#region Constants

		/// <summary>
		/// AttachmentsGridName
		/// </summary>
		public const string AttachmentsGridName = "GridAttachments";

		/// <summary>
		/// AttachmentsEntityBrowseName
		/// </summary>
		public const string AttachmentsEntityBrowseName = "AttachmentsExplorerEntityBrowse";

		private const int DisplayOption = 35109;
		private const int ModifyOption = 35108;
		private const int ModifyFileOption = 35107;
		private const int DeleteOption = 35118;
		private const int NewNoteOption = 35113;
		private const int NewLinkOption = 35114;
		private const int NewFileOption = 35115;
		private const int NewVersionOption = 35106;
		private const int DisplayVersionOption = 35111;

		#endregion

		#region Member Variables

		private ExplorerGrid m_AttachmentsExplorerGrid;
		private EntityBrowse m_AttachmentEntityBrowse;
		private ToolBar m_AttachmentsToolBar;
		private RichTextEdit m_PreviewAttachmentText;

		private ToolBarButton m_AddNoteButton;
		private ToolBarButton m_AddLinkButton;
		private ToolBarButton m_AddFileButton;
		private ToolBarButton m_ModifyButton;
		private ToolBarButton m_DisplayButton;
		private ToolBarButton m_RemoveButton;
		private ToolBarButton m_ToggleButton;
		private ToolBarButton m_NewVersionButton;
		private ToolBarButton m_DisplayAllVersionsButton;
		private ToolBarButton m_ModifyFileButton;

		private DataEntity m_DataEntity;

		private IEntity m_EntityWithAttachment;

		private Attachment m_OrigAttachment;
		private string m_ClientTempAttachmentsFolder;
		private string m_FileAttachment;

		private AttachmentFormMode m_NoteFormMode;
		private FormAttachmentFile m_FileForm;
		private FormAttachmentLink m_LinkForm;
		private bool m_RowChanged;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="AttachmentPage"/> class.
		/// </summary>
		public AttachmentPage()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AttachmentPage"/> class.
		/// </summary>
		/// <param name="attachment">The attachment.</param>
		public AttachmentPage(Attachment attachment)
		{
			m_EntityWithAttachment = EntityManager.Select(attachment.TableName, attachment.RecordKey0);
		}

		#endregion

		#region Page Override Methods

		/// <summary>
		/// Method to decide if page should be added. False will not add the page to the property sheet
		/// or call the extension page methods.
		/// </summary>
		/// <param name="entityType">Type of the entity.</param>
		/// <returns></returns>
		public override bool AddPage(string entityType)
		{
			ISchemaTable table = Schema.Current.Tables[entityType];

			if (table != null && table.HasAttachmentsField != null)
			{
				return true;
			}

			m_ParentSampleManagerTask.Library.Utils.ShowAlert(Library.Message.GetMessage("ControlMessages",
				"AttachmentPageError"));
			return false;
		}

		/// <summary>
		/// Page Created called after corresponding Form event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public override void PageCreated(object sender, RuntimeFormsEventArgs e)
		{
			base.PageCreated(sender, e);
			m_EntityWithAttachment = MainForm.Entity;
			m_EntityWithAttachment.Modified += EntityWithAttachmentModified;
		}

		/// <summary>
		/// Page Closed called after corresponding Form event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public override void PageClosed(object sender, RuntimeFormsEventArgs e)
		{
			base.PageClosed(sender, e);

			var attachmentEntity = e.Form.Entity as IAttachments;

			if (attachmentEntity == null) return;

			// Clear temp server files

			foreach (Attachment attachment in attachmentEntity.Attachments)
				attachment.DeleteTempDirectory();

			// Delete temp client files

			if (!string.IsNullOrEmpty(m_ClientTempAttachmentsFolder))
				Library.File.DeleteDirectoryFromClientTemp(m_ClientTempAttachmentsFolder);
		}

		/// <summary>
		/// PageSelected
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public override void PageSelected(object sender, RuntimeFormsEventArgs e)
		{
			base.PageSelected(sender, e);
			AttachmentPageSelected();
		}

		#endregion

		#region Events

		/// <summary>
		/// Occurs when the helper has finished when in standalone mode.
		/// </summary>
		internal event EventHandler<AttachmentHelperExitedEventArgs> Exited;

		#endregion

		#region Enums

		/// <summary>
		/// Mode in which a sub form has been launched
		/// </summary>
		private enum AttachmentFormMode
		{
			/// <summary>
			/// Add
			/// </summary>
			Add,

			/// <summary>
			/// Modify
			/// </summary>
			Modify,

			/// <summary>
			/// Display
			/// </summary>
			Display
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Form Created Method, so event can be hooked up.
		/// </summary>
		public void AttachmentPageSelected()
		{
			if (m_AttachmentEntityBrowse != null) return;
			var attachmentEntity = m_EntityWithAttachment as IAttachments;
			if (attachmentEntity == null) return;

			m_AttachmentEntityBrowse = (EntityBrowse) MainForm.NonVisualControls[AttachmentsEntityBrowseName];
			m_AttachmentsExplorerGrid = (ExplorerGrid) MainForm.Controls[AttachmentsGridName];
			m_PreviewAttachmentText = (RichTextEdit) MainForm.Controls[FormAttachmentPage.RichAttachmentsNotePreviewControlName];


			m_DataEntity = (DataEntity) MainForm.NonVisualControls["SelectedAttachmentDataEntityDesign"];

			if (Context.LaunchMode == GenericLabtableTask.CopyOption && Context.SelectedItems.Count == 0)
			{
				// Is an attachment host being copied?

				IEntity sourceEntity = Context.SelectedItems[0];
				if (sourceEntity != null)
				{
					// Copy attachments from source to new entity

					CopyAttachments(sourceEntity, m_EntityWithAttachment);
				}
			}

			m_AttachmentsToolBar = (ToolBar) MainForm.Controls[FormAttachmentPage.AttachmentsToolBarControlName];

			m_AddNoteButton = SetupToolBarButton(NewNoteOption, "AddNote", AddNoteButtonClick);
			m_AddLinkButton = SetupToolBarButton(NewLinkOption, "AddLink", AddLinkButtonClick);
			m_AddFileButton = SetupToolBarButton(NewFileOption, "AddFile", AddFileButtonClick);
			m_ModifyButton = SetupToolBarButton(ModifyOption, "Modify", ModifyButtonClick);
			m_ModifyFileButton = SetupToolBarButton(ModifyFileOption, "ModifyFile", ModifyFileButtonClick);
			m_NewVersionButton = SetupToolBarButton(NewVersionOption, "NewVersion", NewVersionButtonClick);
			m_DisplayAllVersionsButton = SetupToolBarButton(DisplayVersionOption, "DisplayVersions", DisplayVersionButtonClick);
			m_DisplayButton = SetupToolBarButton(DisplayOption, "Display", DisplayButtonClick);
			m_RemoveButton = SetupToolBarButton(DeleteOption, "Remove", RemoveButtonClick);

			m_ToggleButton = m_AttachmentsToolBar.FindButton("Toggle");
			m_ToggleButton.Click += ToggleButtonClick;

			m_AttachmentsExplorerGrid.SelectionChanged += AttachmentsExplorerGridSelectionChanged;
			m_AttachmentsExplorerGrid.ContextMenu.BeforePopup += ContextMenuBeforePopup;

			m_PreviewAttachmentText.ContentChanged += UpdateCommentsFromPreview;
			m_PreviewAttachmentText.SelectionChanged += UpdateCommentsFromPreview;
			m_AddNoteButton.Enabled = Modifiable();
			m_AddLinkButton.Enabled = Modifiable();
			m_AddFileButton.Enabled = Modifiable();

			// Attach the Browse

			m_AttachmentsExplorerGrid.Browse = m_AttachmentEntityBrowse;
			UpdateExplorerGrid();

			// Select the first attachment if appropriate.

			if (attachmentEntity.Attachments.ActiveCount > 0)
			{
				SelectAttachmentExplorerRow((Attachment) attachmentEntity.Attachments[0]);
				EnableToolBarButtons((Attachment) attachmentEntity.Attachments[0]);
			}
			else
			{
				EnableToolBarButtons(null);
			}
		}

		/// <summary>
		/// Updates the comments from preview box.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void UpdateCommentsFromPreview(object sender, EventArgs e)
		{
			if (!m_RowChanged)
			{
				m_DataEntity.Data.SetField("COMMENTS", m_PreviewAttachmentText.RichTextContent);
			}
		}


		/// <summary>
		/// Setups the tool bar button.
		/// </summary>
		/// <param name="procedureNumber">The procedure number.</param>
		/// <param name="buttonName">Name of the button.</param>
		/// <param name="handler">The handler.</param>
		/// <returns></returns>
		private ToolBarButton SetupToolBarButton(int procedureNumber, string buttonName, EventHandler handler)
		{
			ToolBarButton button = m_AttachmentsToolBar.FindButton(buttonName);

			if (Library.Security.CheckPrivilege(procedureNumber))
			{
				button.Click += handler;
			}
			else
			{
				button.Visible = false;
			}

			return button;
		}

		/// <summary>
		/// Handles the Click event of the toggleButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void ToggleButtonClick(object sender, EventArgs e)
		{
			m_PreviewAttachmentText.Visible = m_ToggleButton.IsPressed;
		}

		/// <summary>
		/// Handles the Click event of the removeButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void RemoveButtonClick(object sender, EventArgs e)
		{
			((IAttachments) m_EntityWithAttachment).Attachments.Remove(m_DataEntity.Data);
			Library.Task.StateModified();
			UpdateExplorerGrid();

			var attachmentEntity = m_EntityWithAttachment as IAttachments;

			if (attachmentEntity != null && attachmentEntity.Attachments.ActiveCount > 0)
			{
				SelectAttachmentExplorerRow((Attachment) attachmentEntity.Attachments[0]);
				EnableToolBarButtons((Attachment) attachmentEntity.Attachments[0]);
			}
			else
			{
				m_PreviewAttachmentText.LoadText(string.Empty);
				EnableToolBarButtons(null);
			}
		}

		/// <summary>
		/// Handles the Click event of the addFileButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void AddFileButtonClick(object sender, EventArgs e)
		{
			AddFileAttachment();
		}

		/// <summary>
		/// Handles the Click event of the addLinkButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void AddLinkButtonClick(object sender, EventArgs e)
		{
			AddLinkAttachment();
		}

		/// <summary>
		/// Handles the Click event of the addNoteButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void AddNoteButtonClick(object sender, EventArgs e)
		{
			AddNoteAttachment();
		}

		/// <summary>
		/// Handles the Click event of the m_DisplayButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void DisplayButtonClick(object sender, EventArgs e)
		{
			OpenAttachment((Attachment) m_DataEntity.Data, false, true);
		}

		/// <summary>
		/// Handles the Click event of the m_ModifyButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void ModifyButtonClick(object sender, EventArgs e)
		{
			OpenAttachment((Attachment) m_DataEntity.Data, true, false);
		}

		/// <summary>
		/// Modifies the file button click.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void ModifyFileButtonClick(object sender, EventArgs e)
		{
			if (Library.Environment.GetGlobalInt("CLIENT_TYPE") == 1)
			{
				string message = Library.Message.GetMessage("WebExplorerMessages", "FileModifyUnsupported");
				string headermessage = Library.Message.GetMessage("WebExplorerMessages", "ModifyFile");

				Library.Utils.FlashMessage(message, headermessage,
					MessageButtons.OK, MessageIcon.Exclamation, MessageDefaultButton.Button1);

				return;
			}

			OpenAttachment((Attachment) m_DataEntity.Data, true, true);
		}

		/// <summary>
		/// News the version button click.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void NewVersionButtonClick(object sender, EventArgs e)
		{
			CreateNewVersion((Attachment) m_DataEntity.Data);
		}

		/// <summary>
		/// News the version button click.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void DisplayVersionButtonClick(object sender, EventArgs e)
		{
			UpdateExplorerGrid();
		}

		/// <summary>
		/// Handles the SelectionChanged event of the m_AttachmentsExplorerGrid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ExplorerGridSelectionChangedEventArgs"/> instance containing the event data.</param>
		private void AttachmentsExplorerGridSelectionChanged(object sender, ExplorerGridSelectionChangedEventArgs e)
		{
			if (e.Selection.Count == 1)
			{
				m_RowChanged = true;
				SelectAttachmentExplorerRow((Attachment) e.Selection[0]);
				m_RowChanged = false;
			}
		}

		/// <summary>
		/// Selects the explorer row.
		/// </summary>
		/// <param name="attachment">The attachment.</param>
		private void SelectAttachmentExplorerRow(Attachment attachment)
		{
			if (attachment.Comments.Contains("{\\rtf"))
			{
				m_PreviewAttachmentText.LoadRichText(attachment.Comments);
			}
			else
			{
				m_PreviewAttachmentText.LoadText(attachment.Comments);
			}
			EnableToolBarButtons(attachment);
			m_DataEntity.Publish(attachment);
		}

		/// <summary>
		/// Updates the explorer grid.
		/// </summary>
		public void UpdateExplorerGrid()
		{
			bool showAll = m_DisplayAllVersionsButton.IsPressed;
			IEntityCollection items = EntityManager.CreateEntityCollection(AttachmentBase.EntityName);

			foreach (Attachment item in ((IAttachments) m_EntityWithAttachment).Attachments.ActiveItems)
			{
				if (showAll || item.IsLatestVersion) items.Add(item);
			}

			m_AttachmentEntityBrowse.Republish(items);
		}

		/// <summary>
		/// Displays the attachment selection.
		/// </summary>
		private void DisplayAttachmentSelection()
		{
			// Display the attachment form

			FormAttachmentSelection form = FormFactory.CreateForm<FormAttachmentSelection>();
			form.Closed += AttachmentSelectionFormClosed;
			form.Loaded += AttachmentSelectionFormLoaded;

			form.ShowDialog();
		}

		/// <summary>
		/// Handles the Closing event of the NoteForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void NoteFormClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Form form = (Form) sender;
			if (form.FormResult == FormResult.OK)
			{
				Attachment attachment = (Attachment) form.Entity;
				if (string.IsNullOrEmpty(attachment.AttachmentName.Trim()))
				{
					Library.Utils.FlashMessage(Library.Message.GetMessage("LaboratoryMessages", "AttachmentNoteValid"),
						Library.Message.GetMessage("LaboratoryMessages", "AttachmentNote"),
						MessageButtons.OK, MessageIcon.Warning, MessageDefaultButton.Button1);
					e.Cancel = true;
				}

				if (!e.Cancel)
				{
					e.Cancel = !ValidName(m_OrigAttachment, attachment);
				}
			}
		}

		/// <summary>
		/// Handles the Loaded event of the AttachmentSelectionForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void AttachmentSelectionFormLoaded(object sender, EventArgs e)
		{
			FormAttachmentSelection form = (FormAttachmentSelection) sender;
			form.PromptPhraseBrowse1.Phrase = EntityManager.SelectPhrase("ATT_TYPE", PhraseAttType.PhraseIdFILE);
		}

		/// <summary>
		/// Handles the Closed event of the form control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void AttachmentSelectionFormClosed(object sender, EventArgs e)
		{
			FormAttachmentSelection form = (FormAttachmentSelection) sender;

			if (form.FormResult == FormResult.OK)
			{
				// Determine what type of attachment is being created

				PhraseBase phrase = (PhraseBase) form.PromptPhraseBrowse1.Phrase;
				AddAttachment(phrase.PhraseId);
			}
			else
			{
				OnExited(false);
			}
		}

		/// <summary>
		/// Handles the Closing event of the LinkForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void LinkFormClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			FormAttachmentLink form = (FormAttachmentLink) sender;

			if (form.FormResult == FormResult.OK)
			{
				Attachment attachment = (Attachment) form.Entity;
				if (string.IsNullOrEmpty(attachment.AttachmentName.Trim()) || string.IsNullOrEmpty(attachment.Attachment))
				{
					Library.Utils.FlashMessage(Library.Message.GetMessage("LaboratoryMessages", "AttachmentLinkValid"),
						Library.Message.GetMessage("LaboratoryMessages", "AttachmentLink"),
						MessageButtons.OK, MessageIcon.Warning, MessageDefaultButton.Button1);
					e.Cancel = true;
				}

				if (!e.Cancel)
				{
					e.Cancel = !ValidName(m_OrigAttachment, attachment);
				}
			}
		}

		/// <summary>
		/// Handles the Closed event of the AddAttachmentForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void AddLinkAttachmentFormClosed(object sender, EventArgs e)
		{
			Form form = (Form) sender;
			if (form.FormResult == FormResult.OK)
			{
				AddAttachment((Attachment) form.Entity);
			}

			m_LinkForm.ButtonEditFindFile.Click -= ButtonEditFindLinkClick;
			m_LinkForm.Closed -= AddLinkAttachmentFormClosed;
			m_LinkForm.Closing -= LinkFormClosing;

			if (Library.Environment.GetGlobalInt("CLIENT_TYPE") == 1)
			{
				m_LinkForm.ButtonEditFindFile.Visible = true;
			}

			m_LinkForm = null;

			OnExited(form.FormResult == FormResult.OK);
		}

		/// <summary>
		/// Handles the Closed event of the AddTextAttachmentForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void AddNoteAttachmentFormClosed(object sender, EventArgs e)
		{
			FormAttachmentNote form = (FormAttachmentNote) sender;
			if (form.FormResult == FormResult.OK)
			{
				// Add the new attachment
				Attachment entity = (Attachment) form.Entity;
				entity.Comments = form.RichTextEditDescription.RichTextContent;

				AddAttachment(entity);
				UpdateExplorerGrid();
			}
			OnExited(form.FormResult == FormResult.OK);
		}

		/// <summary>
		/// Handles the Closed event of the ModifyTextAttachmentForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void ModifyAttachmentFormClosed(object sender, EventArgs e)
		{
			if (sender is FormAttachmentNote)
			{
				FormAttachmentNote form = sender as FormAttachmentNote;

				if (form.FormResult == FormResult.OK)
				{
					// Apply changes to attachment in grid
					Attachment modifiedAttachment = (Attachment) form.Entity;

					m_OrigAttachment.AttachmentName = modifiedAttachment.AttachmentName;
					m_OrigAttachment.Attachment = modifiedAttachment.Attachment;
					m_OrigAttachment.Category = modifiedAttachment.Category;
					m_OrigAttachment.GroupId = modifiedAttachment.GroupId;
					m_OrigAttachment.Comments = form.RichTextEditDescription.RichTextContent;

					UpdateModificationFields(m_OrigAttachment);
					UpdateExplorerGrid();
				}

				OnExited(form.FormResult == FormResult.OK);
			}
			else
			{
				Form form = sender as Form;

				if (form != null)
				{
					if (form.FormResult == FormResult.OK)
					{
						// Apply changes to attachment in grid
						Attachment modifiedAttachment = (Attachment) form.Entity;

						m_OrigAttachment.AttachmentName = modifiedAttachment.AttachmentName;
						m_OrigAttachment.Attachment = modifiedAttachment.Attachment;
						m_OrigAttachment.Category = modifiedAttachment.Category;
						m_OrigAttachment.GroupId = modifiedAttachment.GroupId;
						m_OrigAttachment.Comments = modifiedAttachment.Comments;

						UpdateModificationFields(m_OrigAttachment);
						UpdateExplorerGrid();
					}

					OnExited(form.FormResult == FormResult.OK);
				}
			}
		}

		/// <summary>
		/// Update toolbar when entity has changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EntityWithAttachmentModified(object sender, EventArgs e)
		{
			if (m_DataEntity != null)
				EnableToolBarButtons((Attachment) m_DataEntity.Data);
		}

		#endregion

		#region Context Menu

		/// <summary>
		/// Contexts the menu before popup.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ContextMenuBeforePopupEventArgs"/> instance containing the event data.</param>
		private void ContextMenuBeforePopup(object sender, ContextMenuBeforePopupEventArgs e)
		{
			m_AttachmentsExplorerGrid.ContextMenu.CustomItems.Clear();

			if (Modifiable())
			{
				string newMenuName = Library.Message.GetMessage("LaboratoryMessages", "AttachmentNewMenu");
				ContextMenuItem newGroupItem = new ContextMenuItem(newMenuName, null) {BeginGroup = false};
				m_AttachmentsExplorerGrid.ContextMenu.CustomItems.Add(newGroupItem);

				AddMenuOption(newGroupItem.CustomItems, NewNoteOption, false, ContextMenuAddNoteClicked);
				AddMenuOption(newGroupItem.CustomItems, NewLinkOption, false, ContextMenuAddLinkClicked);
				AddMenuOption(newGroupItem.CustomItems, NewFileOption, false, ContextMenuAddFileClicked);
			}

			if (e.EntityCollection.ActiveCount == 1)
			{
				Attachment item = (Attachment) e.EntityCollection.ActiveItems[0];

				if (Modifiable())
				{
					AddMenuOption(m_AttachmentsExplorerGrid.ContextMenu.CustomItems, ModifyOption, true, ContextMenuModifyItemClicked);
				}

				if (!item.IsNew())
				{
					AddMenuOption(m_AttachmentsExplorerGrid.ContextMenu.CustomItems, DisplayOption, !Modifiable(),
						ContextMenuDisplayItemClicked);
				}
				if (Modifiable())
				{
					if (item.IsFileType)
					{
						if (!item.IsNew() && !Library.Task.Modified)
						{
							AddMenuOption(m_AttachmentsExplorerGrid.ContextMenu.CustomItems, ModifyFileOption, false, ContextMenuModifyFileClicked);
						}

						if (!item.IsNew() && item.IsLatestVersion)
						{
							AddMenuOption(m_AttachmentsExplorerGrid.ContextMenu.CustomItems, NewVersionOption, false, ContextMenuNewVersionClicked);
						}
					}

					AddMenuOption(m_AttachmentsExplorerGrid.ContextMenu.CustomItems, DeleteOption, true, ContextMenuDeleteClicked);
				}
			}
		}

		/// <summary>
		/// Adds the menu option.
		/// </summary>
		/// <param name="menu">The menu.</param>
		/// <param name="procedureNumber">The procedure number.</param>
		/// <param name="beginGroup">if set to <c>true</c> [begin group].</param>
		/// <param name="itemClickHandler">The item click handler.</param>
		private void AddMenuOption(ContextMenuItemCollection menu, int procedureNumber, bool beginGroup, ContextMenuItemClickedEventHandler itemClickHandler)
		{
			if (Library.Security.CheckPrivilege(procedureNumber))
			{
				ExplorerMasterMenuCache menuObject = Library.Security.GetMasterMenu(procedureNumber);

				ContextMenuItem newItem = new ContextMenuItem(menuObject.ShortText, menuObject.Icon) {BeginGroup = beginGroup};
				newItem.ItemClicked += itemClickHandler;
				menu.Add(newItem);
			}
		}

		/// <summary>
		/// Contexts the menu modify item clicked.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ContextMenuItemEventArgs"/> instance containing the event data.</param>
		private void ContextMenuModifyItemClicked(object sender, ContextMenuItemEventArgs e)
		{
			if (e.EntityCollection.Count != 1) return;
			OpenAttachment((Attachment) e.EntityCollection[0], true, false);
		}

		/// <summary>
		/// Contexts the menu display item clicked.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ContextMenuItemEventArgs"/> instance containing the event data.</param>
		private void ContextMenuDisplayItemClicked(object sender, ContextMenuItemEventArgs e)
		{
			if (e.EntityCollection.Count != 1) return;
			OpenAttachment((Attachment) e.EntityCollection[0], false, true);
		}

		/// <summary>
		/// Contexts the menu delete clicked.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ContextMenuItemEventArgs"/> instance containing the event data.</param>
		private void ContextMenuDeleteClicked(object sender, ContextMenuItemEventArgs e)
		{
			((IAttachments) m_EntityWithAttachment).Attachments.Remove(m_DataEntity.Data);
			Library.Task.StateModified();
			UpdateExplorerGrid();
		}

		/// <summary>
		/// Contexts the menu add note clicked.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ContextMenuItemEventArgs"/> instance containing the event data.</param>
		private void ContextMenuAddNoteClicked(object sender, ContextMenuItemEventArgs e)
		{
			AddNoteAttachment();
		}

		/// <summary>
		/// Contexts the menu add link clicked.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ContextMenuItemEventArgs"/> instance containing the event data.</param>
		private void ContextMenuAddLinkClicked(object sender, ContextMenuItemEventArgs e)
		{
			AddLinkAttachment();
		}

		/// <summary>
		/// Contexts the menu add file clicked.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ContextMenuItemEventArgs"/> instance containing the event data.</param>
		private void ContextMenuAddFileClicked(object sender, ContextMenuItemEventArgs e)
		{
			AddFileAttachment();
		}

		/// <summary>
		/// Contexts the menu new version clicked.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ContextMenuItemEventArgs"/> instance containing the event data.</param>
		private void ContextMenuNewVersionClicked(object sender, ContextMenuItemEventArgs e)
		{
			if (e.EntityCollection.Count != 1) return;
			CreateNewVersion((Attachment) e.EntityCollection[0]);
		}

		/// <summary>
		/// Contexts the menu modify file clicked.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.ContextMenuItemEventArgs"/> instance containing the event data.</param>
		private void ContextMenuModifyFileClicked(object sender, ContextMenuItemEventArgs e)
		{
			if (Library.Environment.GetGlobalInt("CLIENT_TYPE") == 1)
			{
				string message = Library.Message.GetMessage("WebExplorerMessages", "FileModifyUnsupported");
				string headermessage = Library.Message.GetMessage("WebExplorerMessages", "ModifyFile");

				Library.Utils.FlashMessage(message, headermessage,
					MessageButtons.OK, MessageIcon.Exclamation, MessageDefaultButton.Button1);

				return;
			}

			if (e.EntityCollection.Count != 1) return;
			OpenAttachment((Attachment) e.EntityCollection[0], true, true);
		}

		#endregion

		#region Attachment Handling

		/// <summary>
		/// Mark as modifiable.
		/// </summary>
		/// <returns></returns>
		private bool Modifiable()
		{
			if (Context.LaunchMode == GenericLabtableTask.DisplayOption) return false;
			if (Context.LaunchMode == GenericLabtableTask.RemoveOption) return false;

			if (Context.LaunchMode == GenericLabtableTask.ModifyOption)
			{
				if (m_EntityWithAttachment.IsRemoved())
				{
					return false;
				}
				return m_EntityWithAttachment.Locked;
			}
			if (Context.LaunchMode == GenericLabtableTask.AttachOption) return m_EntityWithAttachment.Locked;


			return true;
		}

		/// <summary>
		/// Enables the tool bar buttons.
		/// </summary>
		/// <param name="attachment">The attachment.</param>
		private void EnableToolBarButtons(Attachment attachment)
		{
			if (BaseEntity.IsValid(attachment))
			{
				m_ModifyFileButton.Enabled = Modifiable() && attachment.IsFileType && !attachment.IsNew();
				m_NewVersionButton.Enabled = Modifiable() && attachment.IsFileType && !attachment.IsNew() && attachment.IsLatestVersion && !m_EntityWithAttachment.IsModified();

				m_ModifyButton.Enabled = Modifiable();
				m_DisplayButton.Enabled = !attachment.IsNew();
				m_RemoveButton.Enabled = Modifiable();
				m_DisplayAllVersionsButton.Enabled = !m_EntityWithAttachment.IsModified();
				return;
			}
			m_DisplayAllVersionsButton.Enabled = false;
			m_ModifyButton.Enabled = false;
			m_ModifyFileButton.Enabled = false;
			m_RemoveButton.Enabled = false;
			m_DisplayButton.Enabled = false;
			m_NewVersionButton.Enabled = false;
		}

		#region Notes

		/// <summary>
		/// Modifies a note attachment.
		/// </summary>
		/// <param name="attachment">The attachment.</param>
		/// <param name="allowModify">if set to <c>true</c> [allow modify].</param>
		private void OpenNoteAttachment(Attachment attachment, bool allowModify)
		{
			m_NoteFormMode = allowModify ? AttachmentFormMode.Modify : AttachmentFormMode.Display;
			m_OrigAttachment = attachment;

			Attachment clonedAttachment = GetClonedAttachment(attachment);
			FormAttachmentNote form = FormFactory.CreateForm<FormAttachmentNote>(clonedAttachment);
			if (allowModify)
			{
				// Allow changes to be saved

				form.Closed += ModifyAttachmentFormClosed;
				form.Closing += NoteFormClosing;
			}
			else
			{
				// Not needed

				m_OrigAttachment = null;
			}

			form.Loaded += NoteAttachmentFormLoaded;

			form.ShowDialog();
		}

		/// <summary>
		/// Gets the cloned attachment.
		/// </summary>
		/// <param name="attachment">The attachment.</param>
		/// <returns></returns>
		private Attachment GetClonedAttachment(Attachment attachment)
		{
			Attachment clonedAttachment = (Attachment) EntityManager.CreateEntity(AttachmentBase.EntityName);

			clonedAttachment.TableName = attachment.TableName;
			clonedAttachment.Version = attachment.Version;
			clonedAttachment.RecordKey0 = attachment.RecordKey0;
			clonedAttachment.Type = attachment.Type;
			clonedAttachment.Attachment = attachment.Attachment;
			clonedAttachment.AttachmentName = attachment.AttachmentName;
			clonedAttachment.Category = attachment.Category;
			clonedAttachment.GroupId = attachment.GroupId;
			clonedAttachment.Comments = attachment.Comments;

			return clonedAttachment;
		}

		/// <summary>
		/// Adds a note attachment.
		/// </summary>
		private void AddNoteAttachment()
		{
			m_NoteFormMode = AttachmentFormMode.Add;

			Attachment newAttachment = (Attachment) EntityManager.CreateEntity(AttachmentBase.EntityName);
			newAttachment.SetType(PhraseAttType.PhraseIdNOTE);

			FormAttachmentNote form = FormFactory.CreateForm<FormAttachmentNote>(newAttachment);

			form.Closed += AddNoteAttachmentFormClosed;
			form.Closing += NoteFormClosing;
			form.Loaded += NoteAttachmentFormLoaded;

			form.ShowDialog();
		}

		/// <summary>
		/// Handles the Loaded event of the NoteAttachmentForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void NoteAttachmentFormLoaded(object sender, EventArgs e)
		{
			FormAttachmentNote form = (FormAttachmentNote) sender;
			var entity = (Attachment) form.Entity;

			bool readOnly = (!Modifiable() || m_NoteFormMode == AttachmentFormMode.Display);

			if (entity.Comments.StartsWith(@"{\rtf"))
			{
				form.RichTextEditDescription.LoadRichText(entity.Comments);
			}
			else if (string.IsNullOrEmpty(entity.Comments))
			{
				form.RichTextEditDescription.LoadRichText(Attachment.DefaultRichText);
			}
			else
			{
				form.RichTextEditDescription.LoadText(entity.Comments);
			}

			form.RichTextEditDescription.ReadOnly = readOnly;
			form.TextEditName.ReadOnly = readOnly;
			form.Category.ReadOnly = readOnly;
			form.Group.ReadOnly = readOnly;
		}

		#endregion

		#region Link

		/// <summary>
		/// Opens the link attachment.
		/// </summary>
		/// <param name="attachment">The attachment.</param>
		/// <param name="allowModify">if set to <c>true</c> [allow modify].</param>
		private void OpenLinkAttachment(Attachment attachment, bool allowModify)
		{
			m_NoteFormMode = allowModify ? AttachmentFormMode.Modify : AttachmentFormMode.Display;
			m_OrigAttachment = attachment;

			Attachment clonedAttachment = GetClonedAttachment(attachment);
			m_LinkForm = FormFactory.CreateForm<FormAttachmentLink>(clonedAttachment);

			if (allowModify)
			{
				// Allow changes to be saved

				m_LinkForm.Closed += ModifyAttachmentFormClosed;
				m_LinkForm.ButtonEditFindFile.Click += ButtonEditFindLinkClick;
				m_LinkForm.Closing += LinkFormClosing;

			}
			else
			{
				// Not needed

				m_OrigAttachment = null;
			}

			m_LinkForm.Loaded += LinkAttachmentFormLoaded;
			m_LinkForm.ShowDialog();
		}

		/// <summary>
		/// Handles the Loaded event of the LinkAttachmentForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void LinkAttachmentFormLoaded(object sender, EventArgs e)
		{
			FormAttachmentLink form = (FormAttachmentLink) sender;

			if (!Modifiable() || m_NoteFormMode == AttachmentFormMode.Display)
			{
				form.TextEditName.ReadOnly = true;
				form.PromptLink.ReadOnly = true;
				form.Category.ReadOnly = true;
				form.Group.ReadOnly = true;
				return;
			}

			if (Library.Environment.GetGlobalInt("CLIENT_TYPE") == 1)
			{
				form.ButtonEditFindFile.Visible = false;
			}

			form.PromptLink.ReadOnly = false;
			form.TextEditName.ReadOnly = false;
			form.Category.ReadOnly = false;
			form.Group.ReadOnly = false;
		}

		/// <summary>
		/// Adds a LINK attachment.
		/// </summary>
		private void AddLinkAttachment()
		{
			Attachment newAttachment = (Attachment) EntityManager.CreateEntity(AttachmentBase.EntityName);
			newAttachment.SetType(PhraseAttType.PhraseIdLINK);

			m_LinkForm = FormFactory.CreateForm<FormAttachmentLink>(newAttachment);

			if (Library.Environment.GetGlobalInt("CLIENT_TYPE") != 1)
			{
				m_LinkForm.ButtonEditFindFile.Click += ButtonEditFindLinkClick;
			}

			m_LinkForm.Loaded += LinkFormOnLoaded;
			m_LinkForm.Closed += AddLinkAttachmentFormClosed;
			m_LinkForm.Closing += LinkFormClosing;

			m_LinkForm.ShowDialog();
		}

		/// <summary>
		/// Links the form on loaded.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void LinkFormOnLoaded(object sender, EventArgs eventArgs)
		{
			if (Library.Environment.GetGlobalInt("CLIENT_TYPE") == 1)
			{
				m_LinkForm.ButtonEditFindFile.Visible = false;
			}
		}

		/// <summary>
		/// Handles the Click event of the ButtonEditFindLink control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void ButtonEditFindLinkClick(object sender, EventArgs e)
		{
			try
			{
				string title = Library.Message.GetMessage("LaboratoryMessages", "AttachmentLinkBrowse");
				string pattern = Library.Message.GetMessage("LaboratoryMessages", "AttachmentFilePattern");
				string clientFileName = Library.Utils.PromptForFile(title, pattern);

				if (!string.IsNullOrEmpty(clientFileName))
				{
					// Set Attachment to selected file name

					Attachment attachment = (Attachment) m_LinkForm.Entity;
					attachment.Attachment = clientFileName;
				}
			}
			catch (Exception)
			{
				Library.Utils.FlashMessage(Library.Message.GetMessage("LaboratoryMessages", "FileInUse"), "");
			}
		}

		#endregion

		#region File

		/// <summary>
		/// Opens the file attachment.
		/// </summary>
		/// <param name="attachment">The attachment.</param>
		/// <param name="allowModify">if set to <c>true</c> [allow modify].</param>
		private void OpenFileAttachment(Attachment attachment, bool allowModify)
		{
			// Display the Text attachment form

			m_NoteFormMode = allowModify ? AttachmentFormMode.Modify : AttachmentFormMode.Display;
			m_OrigAttachment = attachment;

			Attachment clonedAttachment = GetClonedAttachment(attachment);
			m_FileForm = FormFactory.CreateForm<FormAttachmentFile>(clonedAttachment);

			if (allowModify)
			{
				// Allow changes to be saved

				m_FileForm.Closed += ModifyAttachmentFormClosed;
				m_FileForm.Closing += FileFormClosing;
			}
			else
			{
				// Not needed

				m_OrigAttachment = null;
			}

			m_FileForm.Loaded += FileAttachmentFormLoaded;
			m_FileForm.ShowDialog();
		}

		/// <summary>
		/// Handles the Loaded event of the FileAttachmentForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void FileAttachmentFormLoaded(object sender, EventArgs e)
		{
			FormAttachmentFile form = (FormAttachmentFile) sender;

			if (!Modifiable() || m_NoteFormMode == AttachmentFormMode.Display)
			{
				form.TextEditName.ReadOnly = true;
				form.PromptFile.ReadOnly = true;
				form.Category.ReadOnly = true;
				form.Group.ReadOnly = true;
				return;
			}

			form.PromptFile.ReadOnly = m_NoteFormMode != AttachmentFormMode.Add;
			form.TextEditName.ReadOnly = m_NoteFormMode != AttachmentFormMode.Add;
			form.Category.ReadOnly = false;
			form.Group.ReadOnly = false;
		}

		/// <summary>
		/// Adds a file attachment.
		/// </summary>
		private void AddFileAttachment()
		{
			// Display the attachment form

			Attachment newAttachment = (Attachment) EntityManager.CreateEntity(AttachmentBase.EntityName);
			newAttachment.SetType(PhraseAttType.PhraseIdFILE);
			m_OrigAttachment = null;

			// Display file attachment form

			m_FileForm = FormFactory.CreateForm<FormAttachmentFile>(newAttachment);
			m_FileForm.ButtonEditFindFile.Click += ButtonEditFindFileClick;
			m_FileForm.Closing += FileFormClosing;
			m_FileForm.Closed += FileAttachmentFormClosed;

			m_FileForm.ShowDialog();
		}

		/// <summary>
		/// Handles the Click event of the ButtonEditFindFile control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void ButtonEditFindFileClick(object sender, EventArgs e)
		{
			string title = Library.Message.GetMessage("LaboratoryMessages", "AttachmentFileBrowse");
			string pattern = Library.Message.GetMessage("LaboratoryMessages", "AttachmentFilePattern");
			string clientFileName = Library.Utils.PromptForFile(title, pattern);

			if (!string.IsNullOrEmpty(clientFileName))
			{
				// Set Attachment to selected file name

				Attachment attachment = (Attachment) m_FileForm.Entity;
				m_FileAttachment = clientFileName;
				attachment.Attachment = Path.GetFileName(clientFileName);
			}
		}

		/// <summary>
		/// Handles the Closing event of the m_FileForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void FileFormClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			FormAttachmentFile formAttachmentFile = (FormAttachmentFile) sender;
			if (formAttachmentFile.FormResult == FormResult.OK)
			{
				Attachment attachment = (Attachment) formAttachmentFile.Entity;
				if (string.IsNullOrEmpty(attachment.AttachmentName.Trim()) || string.IsNullOrEmpty(attachment.Attachment))
				{
					Library.Utils.FlashMessage(Library.Message.GetMessage("LaboratoryMessages", "AttachmentFileValid"),
						Library.Message.GetMessage("LaboratoryMessages", "AttachmentFile"),
						MessageButtons.OK, MessageIcon.Warning, MessageDefaultButton.Button1);
					e.Cancel = true;
				}

				if (m_NoteFormMode == AttachmentFormMode.Add || (m_NoteFormMode == AttachmentFormMode.Modify && !m_FileForm.PromptFile.ReadOnly))
				{

					//Check if the user has overridden the file name after selecting it through the file browse

					if (!string.IsNullOrEmpty(m_FileAttachment))
					{
						string originalFileName = Path.GetFileName(m_FileAttachment);
						if (originalFileName != attachment.Attachment)
						{
							// if the selected and type file names are not the same assume typed has the full
							// path and null the selected.
							m_FileAttachment = null;
						}
					}

					// If the file browse was not used check the attachment holds the full path
					if (string.IsNullOrEmpty(m_FileAttachment) && !string.IsNullOrEmpty(attachment.Attachment))
					{
						if (!File.Exists(attachment.Attachment))
						{
							m_FileForm.PromptFile.ShowError(Library.Message.GetMessage("LaboratoryMessages", "AttachmentFileNotFound"));
							e.Cancel = true;
						}
					}
				}

				if (!e.Cancel)
				{
					e.Cancel = !ValidName(m_OrigAttachment, attachment);
				}
			}
			else
			{
				RemoveWebTempFile(m_FileAttachment);
			}
		}

		/// <summary>
		/// Removes the web temporary file.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <exception cref="Thermo.SampleManager.Core.Exceptions.SampleManagerException"></exception>
		private void RemoveWebTempFile(string path)
		{
			try
			{
				if (Library.Environment.GetGlobalInt("CLIENT_TYPE") == 1 &&
				    m_FileAttachment != null && File.Exists(path))
				{

					File.Delete(path);
				}
			}
			catch (Exception ex)
			{
				throw new SampleManagerException(ex.Message);
			}
		}

		/// <summary>
		/// Valid Name
		/// </summary>
		/// <param name="originalAttachment"></param>
		/// <param name="currentAttachment"></param>
		/// <returns></returns>
		private bool ValidName(Attachment originalAttachment, Attachment currentAttachment)
		{
			if (originalAttachment != null && originalAttachment.Name == currentAttachment.Name)
			{
				return true;
			}

			foreach (Attachment existingAttachment in ((IAttachments) m_EntityWithAttachment).Attachments)
			{
				if (existingAttachment.AttachmentName == currentAttachment.AttachmentName)
				{
					Library.Utils.FlashMessage(Library.Message.GetMessage("LaboratoryMessages", "AttachmentFound"),
						Library.Message.GetMessage("LaboratoryMessages", "AttachmentHeader"),
						MessageButtons.OK, MessageIcon.Warning, MessageDefaultButton.Button1);
					return false;
				}

				if (currentAttachment.IsFileType && existingAttachment.Attachment == currentAttachment.Attachment)
				{
					Library.Utils.FlashMessage(string.Format(m_FileForm.StringTable.DuplicateFileError, existingAttachment.Attachment),
						Library.Message.GetMessage("LaboratoryMessages", "AttachmentHeader"));
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Handles the Closed event of the m_FileForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void FileAttachmentFormClosed(object sender, EventArgs e)
		{
			bool modified = m_FileForm.FormResult == FormResult.OK;
			if (modified)
			{
				// Add the attachment

				Attachment newAttachment = (Attachment) m_FileForm.Entity;
				if (m_FileAttachment == null)
				{
					m_FileAttachment = newAttachment.Attachment;
				}

				newAttachment.AttachClientFile(m_FileAttachment);
				AddAttachment(newAttachment);
				Library.Task.StateModified();
			}

			// Remove events

			m_FileForm.ButtonEditFindFile.Click -= ButtonEditFindFileClick;
			m_FileForm.Closed -= FileAttachmentFormClosed;
			m_FileForm.Closing -= FileFormClosing;
			m_FileForm = null;

			OnExited(modified);
		}

		#endregion

		/// <summary>
		/// Adds the attachment.
		/// </summary>
		private void AddAttachment(Attachment attachment)
		{
			((IAttachments) m_EntityWithAttachment).Attachments.Add(attachment);
			UpdateExplorerGrid();
		}

		/// <summary>
		/// Creates the new version.
		/// </summary>
		/// <param name="attachment">The attachment.</param>
		public void CreateNewVersion(Attachment attachment)
		{
			var attachmentEntity = m_EntityWithAttachment as IAttachments;
			if (attachmentEntity == null) return;

			attachment.CreateNewVersion(m_EntityWithAttachment);
			Library.Task.StateModified();

			UpdateExplorerGrid();

			// Display Confirmation

			Library.Utils.FlashMessage(Library.Message.GetMessage("LaboratoryMessages", "AttachmentNewVersion"),
				Library.Message.GetMessage("LaboratoryMessages", "AttachmentHeader"), MessageButtons.OK,
				MessageIcon.Information, MessageDefaultButton.Button1);

			OnExited(true);
		}

		/// <summary>
		/// Displays the attachment.
		/// </summary>
		/// <param name="attachment">The attachment.</param>
		/// <param name="allowModify">if set to <c>true</c> [allow modify].</param>
		/// <param name="openActualAttachment">if set to <c>true</c> [open actual attachment].</param>
		public void OpenAttachment(Attachment attachment, bool allowModify, bool openActualAttachment)
		{
			if (attachment.IsFileType)
			{
				if (openActualAttachment)
				{
					if (string.IsNullOrEmpty(attachment.Attachment))
					{
						Library.Utils.FlashMessage(Library.Message.GetMessage("ControlMessages", "EntryExists"),
							Library.Message.GetMessage("LaboratoryMessages", "AttachmentHeader"));
						return;
					}

					// Setup client temp folder

					string clientFolder = m_ClientTempAttachmentsFolder;

					if (string.IsNullOrEmpty(clientFolder))
						clientFolder = Path.Combine("Attachment", Guid.NewGuid().ToString());

					// Open file attachment on client

					if (attachment.OpenFileAttachment(clientFolder, allowModify))
					{

						if (string.IsNullOrEmpty(m_ClientTempAttachmentsFolder))
							m_ClientTempAttachmentsFolder = clientFolder;

						if (allowModify)
						{
							UpdateModificationFields(attachment);
							Library.Task.StateModified();
						}
					}
				}
				else
				{
					OpenFileAttachment(attachment, allowModify);
				}

				return;
			}

			if (attachment.IsLinkType)
			{
				if (openActualAttachment)
				{
					if (string.IsNullOrEmpty(attachment.Attachment))
					{
						Library.Utils.FlashMessage(Library.Message.GetMessage("LaboratoryMessages", "AttachmentNoLink"),
							Library.Message.GetMessage("LaboratoryMessages", "AttachmentHeader"));
						return;
					}

					Library.File.OpenClientFile(attachment.Attachment, !allowModify);
				}
				else
				{
					OpenLinkAttachment(attachment, allowModify);
				}
				return;
			}

			if (attachment.IsNoteType)
			{
				OpenNoteAttachment(attachment, allowModify);
			}
		}

		#region Modification Tracking

		/// <summary>
		/// Sets the modified.
		/// </summary>
		private void UpdateModificationFields(Attachment attachment)
		{
			// Update Modified on and Modified By - at least ensure the record is flagged as modified.

			attachment.ModifiedOn = Library.Environment.ClientNow;
			attachment.ModifiedBy = (PersonnelBase) Library.Environment.CurrentUser;
		}

		#endregion

		#endregion

		#region Copy Attachments

		/// <summary>
		/// Copies attachments from one entity to another.
		/// </summary>
		/// <param name="sourceEntity">The source entity.</param>
		/// <param name="targetEntity">The target entity.</param>
		internal void CopyAttachments(IEntity sourceEntity, IEntity targetEntity)
		{
			// Validate Arguments

			if (sourceEntity == null)
			{
				throw new ArgumentNullException("sourceEntity");
			}
			if (targetEntity == null)
			{
				throw new ArgumentNullException("targetEntity");
			}

			// Copy attachments

			foreach (Attachment attachment in ((IAttachments) sourceEntity).Attachments)
			{
				// Copy attachment and add it to the attachments collection of the new entity

				Attachment newAttachment = (Attachment) attachment.CreateCopy();
				((IAttachments) targetEntity).Attachments.Add(newAttachment);
			}
		}

		#endregion

		#region Standalone Operation

		/// <summary>
		/// Adds the attachment to entity.
		/// </summary>
		/// <param name="attachmentHost">The attachment host.</param>
		/// <param name="attachmentType">Type of the attachment.</param>
		internal void AddAttachmentToEntity(IEntity attachmentHost, string attachmentType)
		{
			m_EntityWithAttachment = attachmentHost;

			if (string.IsNullOrEmpty(attachmentType))
			{
				DisplayAttachmentSelection();
			}
			else
			{
				AddAttachment(attachmentType);
			}
		}

		/// <summary>
		/// Adds the attachment.
		/// </summary>
		/// <param name="attachmentType">Type of the attachment.</param>
		private void AddAttachment(string attachmentType)
		{
			switch (attachmentType)
			{
				case PhraseAttType.PhraseIdFILE:
					AddFileAttachment();
					break;
				case PhraseAttType.PhraseIdLINK:
					AddLinkAttachment();
					break;
				case PhraseAttType.PhraseIdNOTE:
					AddNoteAttachment();
					break;
			}
		}

		/// <summary>
		/// Called when [exited].
		/// </summary>
		private void OnExited(bool modified)
		{
			EventHandler<AttachmentHelperExitedEventArgs> handler = Exited;
			if (handler != null)
			{
				handler(this, new AttachmentHelperExitedEventArgs(m_EntityWithAttachment, modified));
			}

			if (modified)
			{
				ISchemaTable table = m_EntityWithAttachment.FindSchemaTable();
				if (table.ModifiedOnField != null && table.ModifiedByField != null)
				{
					m_EntityWithAttachment.Set(table.ModifiedOnField.Name, Library.Environment.ClientNow);
					m_EntityWithAttachment.Set(table.ModifiedByField.Name, Library.Environment.CurrentUser);
				}
			}
		}

		#endregion
	}
}