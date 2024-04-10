using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Workflow;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;
using Thermo.SampleManager.Server.Workflow;
using Form = Thermo.SampleManager.Library.ClientControls.Form;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Base class for all static data property sheets (LTEs)
	/// </summary>
	[SampleManagerTask("GenericLabtableTask", "LABTABLE")]
	public class GenericLabtableTask : SampleManagerTask
	{
		#region sval result

		/// <summary>
		/// The mode to initialise the server and smplib
		/// </summary>
		protected enum ApprovalResultType
		{
			/// <summary>
			/// Not approved
			/// </summary>
			NotApproved,

			/// <summary>
			/// Approved
			/// </summary>
			Approved,

			/// <summary>
			/// Rejected
			/// </summary>
			Rejected,

			/// <summary>
			/// Cancel out of the screen
			/// </summary>
			Cancel
		}

		#endregion

		#region Constants

		private const string ApprovalSecurityHigh = "HIGH";
		private const string ApprovalSecurityLow = "LOW";

		private const string DefaultPrintReport = "DEFAULT_PRINT_REPORT";
		private const string DefaultListReport = "DEFAULT_LIST_REPORT";

		// Text that identifies an identity passed into a build operation
		private const string BuildIdStart = "BUILD_ID=";

		private const string ApprovalSingleActiveVersion = "APPROVAL_SINGLE_ACTIVE_VERSION";

		private const int PrivilegeApproveOnBehalf = 9016;

		private const string ApprovalReviewRejected = "R";
		private const string ApprovalReviewApproved = "A";

		#endregion

		#region Public Constants

		/// <summary>
		/// Add Option
		/// </summary>
		public const string AddOption = "ADD";

		/// <summary>
		/// Add Option
		/// </summary>
		public const string InsertOption = "INSERT";

		/// <summary>
		/// Copy Option
		/// </summary>
		public const string CopyOption = "COPY";

		/// <summary>
		/// Display Option
		/// </summary>
		public const string DisplayOption = "DISPLAY";

		/// <summary>
		/// Print Label Option
		/// </summary>
		public const string LabelOption = "PRINTLABEL";

		/// <summary>
		/// List Option
		/// </summary>
		public const string ListOption = "LIST";

		/// <summary>
		/// Modify Option
		/// </summary>
		public const string ModifyOption = "MODIFY";

		/// <summary>
		/// New Version Option
		/// </summary>
		public const string NewVersionOption = "NEWVERSION";

		/// <summary>
		/// Print Option
		/// </summary>
		public const string PrintOption = "PRINT";

		/// <summary>
		/// Remove Option
		/// </summary>
		public const string RemoveOption = "REMOVE";

		/// <summary>
		/// Restore Option
		/// </summary>
		public const string RestoreOption = "RESTORE";

		/// <summary>
		/// Test Option
		/// </summary>
		public const string TestOption = "TEST";

		/// <summary>
		/// Submit Option
		/// </summary>
		public const string SubmitOption = "SUBMIT";

		/// <summary>
		/// Approve Option
		/// </summary>
		public const string ApproveOption = "AUTHORISE";

		/// <summary>
		/// Workflow Option
		/// </summary>
		public const string WorkflowOption = "WORKFLOW";

		/// <summary>
		/// Attachments
		/// </summary>
		public const string AttachOption = "ATTACH";

		#endregion

		#region Member Variables

		private IEntity m_CopiedEntity;
		private bool m_SetAsModified;
		private bool m_MainFormCreated;
		private TriState m_ShowAllVersions = TriState.Default;
		private string m_ApprovalStatus = string.Empty;
		private bool m_Submitted;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets a value indicating whether to set the initial state as modified.
		/// </summary>
		/// <value>
		///   <c>true</c> if set as modified; otherwise, <c>false</c>.
		/// </value>
		protected bool SetAsModified
		{
			get { return m_SetAsModified; }
			set { m_SetAsModified = value; }
		}

		/// <summary>
		/// Gets the main form of the labtable.
		/// </summary>
		/// <value>The main form.</value>
		protected Form MainForm
		{
			get
			{
				if (!m_MainFormCreated)
				{
					return null;
				}

				return FormFactory[Context.TaskParameters[0]];
			}
		}

		/// <summary>
		/// Copy from this Identity when creating new records
		/// </summary>
		/// <value>The identity of the record to copy</value>
		protected string DefaultCopy { get; set; }

		/// <summary>
		/// Id of the operator to use during inspection, privileged user can deputise
		/// </summary>
		/// <value>The identity of operator</value>
		protected string InspectionUser { get; set; }

		/// <summary>
		/// Gets or sets the inspection entity.
		/// </summary>
		/// <value>The inspection entity.</value>
		protected IEntity InspectionEntity { get; set; }

		/// <summary>
		/// Gets or sets the approval choose user form.
		/// </summary>
		/// <value>
		/// The approval choose user form.
		/// </value>
		protected FormApprovalChooseUser ApprovalChooseUserForm { get; set; }

		/// <summary>
		/// Gets or sets the approval confirmation form.
		/// </summary>
		/// <value>
		/// The approval confirmation form.
		/// </value>
		protected FormApprovalConfirmation ApprovalConfirmationForm { get; set; }

		/// <summary>
		/// Gets or sets the approval confirmation result.
		/// </summary>
		/// <value>
		/// The approval confirmation result.
		/// </value>
		protected ApprovalResultType ApprovalConfirmationResult { get; set; }

		/// <summary>
		/// Gets the copied entity.
		/// </summary>
		/// <value>The copied entity.</value>
		protected IEntity CopiedEntity
		{
			get { return m_CopiedEntity; }
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="GenericLabtableTask"/> is submitted. If the GenericLabtableTask tried to submit the entity.
		/// </summary>
		/// <value>
		///   <c>true</c> if submitted; otherwise, <c>false</c>.
		/// </value>
		protected bool Submitted
		{
			get { return m_Submitted; }
		}

		/// <summary>
		/// Gets or sets the entity error.
		/// </summary>
		/// <value>
		/// The entity error.
		/// </value>
		protected string EntityError { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to force display only mode.
		/// </summary>
		/// <value>
		///   <c>true</c> if force display mode; otherwise, <c>false</c>.
		/// </value>
		protected bool ForceDisplay { get; set; }

		#endregion

		#region Setup

		/// <summary>
		/// Setup the SampleManager LTE task
		/// </summary>
		protected override void SetupTask()
		{
			FormFactory.InterfaceLoaded += InterfaceLoaded;

			switch (Context.LaunchMode)
			{
				case AddOption:
				case InsertOption:
					Add();
					break;
				case ModifyOption:
					Modify();
					break;
				case DisplayOption:
					Display();
					break;
				case CopyOption:
					Copy();
					break;
				case TestOption:
					Test();
					break;
				case RemoveOption:
					Remove();
					break;
				case RestoreOption:
					Restore();
					break;
				case ListOption:
					List();
					Exit();
					break;
				case PrintOption:
					Print();
					Exit();
					break;
				case LabelOption:
					PrintListLabel();
					break;
				case NewVersionOption:
					NewVersion();
					break;
				case SubmitOption:
					Submit();
					break;
				case ApproveOption:
					Approve();
					break;
				case WorkflowOption:
					CreateFromWorkflow();
					break;
				default:
					throw new SampleManagerError(string.Format(GetMessage("EditorModeException"), Context.LaunchMode));
			}

			//save to MRU
			var launchMode = Context.LaunchMode;
			if (!(NewVersionOption == launchMode || RemoveOption == launchMode || CopyOption == launchMode))
			{
				if (Context.SelectedItems != null && Context.SelectedItems.Count == 1)
				{
					UpdateMRUSelection(Context.SelectedItems[0]);
				}
			}
		}

		#endregion

		#region Message Utilities

		/// <summary>
		/// Get a message
		/// </summary>
		/// <returns></returns>
		protected string GetMessage(string messageIdentity)
		{
			return Library.Message.GetMessage("GeneralMessages", messageIdentity);
		}

		/// <summary>
		/// Get a message
		/// </summary>
		/// <returns></returns>
		protected string GetMessage(string messageIdentity, params string[] param)
		{
			return Library.Message.GetMessage("GeneralMessages", messageIdentity, param);
		}

		#endregion

		#region Task Events

		/// <summary>
		/// Called when the FormFactory Loads the Interface Object - allows you to tinker before loading the form.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.InterfaceEventArgs"/> instance containing the event data.</param>
		protected virtual void InterfaceLoaded(object sender, InterfaceEventArgs e)
		{
		}

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected virtual void MainFormCreated()
		{
			// Nothing at this level
		}

		/// <summary>
		/// Handles the Created event of the MainForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void FormCreated(object sender, EventArgs e)
		{
			m_MainFormCreated = true;
			MainForm.Title = GetTitle();
			MainFormCreated();
		}

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been loaded.
		/// </summary>
		protected virtual void MainFormLoaded()
		{
		}

		/// <summary>
		/// Handles the Loaded event of the MainForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void FormLoaded(object sender, EventArgs e)
		{
			// Set the state as modified

			if (m_SetAsModified)
			{
				Library.Task.StateModified();
			}

			// Generate a warning for non-modifiable records.

			WarnIfNonModifiable();

			InitialApprovalState();

			MainFormLoaded();
		}

		/// <summary>
		/// Called before the property sheet or wizard is saved.
		/// </summary>
		/// <returns>
		/// true to allow the save to continue, false to abort the save.
		/// Please also ensure that you call the base.OnPreSave when continuing
		/// successfully.
		/// </returns>
		protected override bool OnPreSave()
		{
			// Clear post processing flags

			m_Submitted = false;

			// Check if the Approval stuff has been filled in

			if (TableHasApproval())
			{
				if (Context.LaunchMode == ApproveOption)
				{
					if (ApprovalConfirmationResult == ApprovalResultType.NotApproved)
					{
						if (ApprovalConfirmationForm == null)
							InspectionPreSave();
						return false;
					}

					return true;
				}

				if (!ApprovalCheckPreSave(MainForm.Entity))
				{
					return false;
				}
			}

			// Deal with non modifiable records

			if (!ConfirmModifiable(MainForm.Entity))
			{
				return false;
			}

			// Add the entity to the transaction.

			EntityManager.Transaction.Add(MainForm.Entity);

			return base.OnPreSave();
		}

		/// <summary>
		/// Get Title
		/// </summary>
		/// <returns>Formatted title of the property sheet</returns>
		private string GetTitle()
		{
			string title = MainForm.Title;
			string action = GetMessage(string.Format("EditorMode{0}", Context.LaunchMode));

			int count = Context.SelectedItems.Count;

			if (count == 0 || Context.LaunchMode == AddOption)
			{
				title = GetMessage("LabTableTitleNone", title, action);
			}
			else if (count == 1)
			{
				string item = Context.SelectedItems[0].Name;
				title = GetMessage("LabTableTitleSingle", title, action, Library.Message.ConvertTaggedField(item));
			}
			else if (count > 1 && (Context.LaunchMode == ModifyOption || Context.LaunchMode == DisplayOption))
			{
				string item = Context.SelectedItems[count - 1].Name;
				title = GetMessage("LabTableTitleSingle", title, action, Library.Message.ConvertTaggedField(item));
			}
			else
			{
				string format = GetMessage("LabTableTitleMultiple");
				title = string.Format(format, title, action, count);
			}

			return title;
		}

		#endregion

		#region Task Actions

		/// <summary>
		/// Tests this instance.
		/// </summary>
		private void Test()
		{
			if (Context.SelectedItems.Count == 0)
			{
				Add();
			}
			else
			{
				Modify();
			}
		}

		/// <summary>
		/// Add option.
		/// </summary>
		protected virtual void Add()
		{
			// Create a new entity, copy from default if specified.

			IEntity entity;
			IEntity defaultEntity = null;

			if (string.IsNullOrEmpty(DefaultCopy))
			{
				entity = CreateNewEntity();
			}
			else
			{
				defaultEntity = EntityManager.Select(Context.EntityType, DefaultCopy);
				entity = defaultEntity.CreateCopy();
			}

			// Edit the newly created record.

			if (entity != null)
			{
				// Check if a parameter has been passed for the new id.

				TryAssignNewEntityId(entity);

				// Publish the user interface passed in the task parameters.

				Form form = FormFactory.CreateForm(Context.TaskParameters[0], entity);

				// Deal with Text File copies

				if (form.TextFile != null && defaultEntity != null)
				{
					form.TextFile.Copy(defaultEntity);
				}

				// Setup the form

				form.Created += FormCreated;
				form.Loaded += FormLoaded;
				form.Show(Context.MenuWindowStyle);
				form.Closing += MainFormClosing;
			}
			else
			{
				// If no entities have been selected then exit the task
				Exit();
			}
		}

		/// <summary>
		/// Tries the assign new entity id.
		/// </summary>
		/// <param name="entity">The entity.</param>
		private void TryAssignNewEntityId(IEntity entity)
		{

			if (Context.TaskParameters.GetUpperBound(0) >= 1)
			{

				ISchemaTable table = entity.FindSchemaTable();

				string newId = Context.TaskParameters[1];

				// VGL Screens do not have the BuildIdStart but only pass 1 identity 
				if (newId.StartsWith(BuildIdStart) || (table != null && table.KeyFields.Count == 1))
				{
					// Assign the ID into the relevant key field
					newId = newId.Replace(BuildIdStart, "");

					if (table != null && table.KeyFields.Count <= 2)
					{
						// If a composite key is comprised of anything other than an Identity and a Version Field, don;t bother trying as it's a minefield
						bool hasNonVersionedCompositeKey = table.KeyFields.Count == 2 && table.VersionField == null;
						if (hasNonVersionedCompositeKey)
						{
							return;
						}

						// Get the key field
						ISchemaField keyField = (table.KeyFields.Count == 2 && table.KeyFields[0] == table.VersionField)
							? table.KeyFields[1]
							: table.KeyFields[0];

						// Check the key field isn't a GUID.
						if (keyField.IsGuid)
						{
							// Set the name instead
							if (table.NameField != null)
							{
								entity.SetField(table.NameField.Name, newId);
								m_SetAsModified = true;
							}
							return;
						}

						// Get the target field name
						string keyFieldName = keyField.Name;

						string propertyName = EntityType.DeducePropertyName(table.Name, keyFieldName);
						string identityString = BaseEntity.CleanValueUsingAllowedChars(entity, propertyName, newId);
						entity.SetField(keyFieldName, identityString);
						m_SetAsModified = true;
					}
				}

			}
		}

		/// <summary>
		/// Handles the Closing event of the form control. Set the return value to return the entity if the OK button was pressed.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void MainFormClosing(object sender, CancelEventArgs e)
		{
			Form form = (Form) sender;
			if (form == null) return;

			// If we're in the process of approving then cancel the close

			if (Context.LaunchMode == ApproveOption)
			{
				if (ApprovalConfirmationResult == ApprovalResultType.NotApproved)
				{
					if (ApprovalConfirmationForm != null)
					{
						e.Cancel = true;
						return;
					}
				}
			}

			if (form.FormResult == FormResult.OK)
			{
				Context.ReturnValue = form.Entity;
			}
			else
			{
				Context.ReturnValue = null;
			}

			// Release the lock explicitly

			if (form.Entity.Locked)
			{
				form.Entity.LockRelease();
			}
		}

		/// <summary>
		/// Called after the property sheet or wizard is saved.
		/// </summary>
		protected override void OnPostSave()
		{
			IEntity entity = MainForm.Entity;

			if (ShouldSubmitOnSave(entity))
			{
				SubmitOnSave(entity);
			}

			// Lock the entity if you have just added it.

			if (Context.LaunchMode == AddOption || Context.LaunchMode == WorkflowOption)
			{
				entity.Lock();
			}

			DeactivateUnapproved(entity);

			// If a new version has been added then prompt to deactivate the old one.

			if ((Context.LaunchMode == NewVersionOption) && (! SingleActiveVersion))
			{
				DeactivateOldVersion();
			}

			// save to MRU
			var launchMode = Context.LaunchMode;
			if (AddOption == launchMode || InsertOption == launchMode || CopyOption == launchMode || WorkflowOption == launchMode)
			{
				UpdateMRUSelection(entity);
			}
		}

		/// <summary>
		/// Deactivates the old version.
		/// </summary>
		private void DeactivateOldVersion()
		{
			IEntity entity = MainForm.Entity;
			ISchemaTable table = entity.FindSchemaTable();

			if (table.ActiveFlagField == null || table.ActiveEndDateField == null) return;

			if (!entity.CheckActive()) return;
			if (!m_CopiedEntity.CheckActive()) return;

			string oldNumber = m_CopiedEntity.GetString(table.VersionField.Name);

			if (Library.Utils.FlashMessageYesNo(GetMessage("DeactivateVersionText", oldNumber.Trim()),
			                                    GetMessage("DeactivateVersionTitle")))
			{
				m_CopiedEntity.Set(table.ActiveEndDateField.Name, Library.Environment.ClientNow);
				m_CopiedEntity.Set(table.ActiveFlagField.Name, false);

				EntityManager.Transaction.Add(m_CopiedEntity);
				EntityManager.Commit();
			}
		}

		/// <summary>
		/// Check entity is authorised for approval and deactivate of not.
		/// </summary>
		/// <param name="entity"></param>
		private void DeactivateUnapproved(IEntity entity)
		{
			// Check if the current labtable has a approval status

			if (GetApprovalStatusProperty() == null) return;

			if (!IsApprovalAuthorised(entity))
			{
				Deactivate(entity);
			}
			else if (SingleActiveVersion)
			{
				SetPending(entity);
			}
		}

		/// <summary>
		/// Deactivate
		/// </summary>
		/// <param name="entity"></param>
		private void Deactivate(IEntity entity)
		{
			if (!entity.CheckActive()) return;

			ISchemaTable table = entity.FindSchemaTable();

			if (table.ActiveFlagField != null)
			{
				if (entity.GetBoolean(table.ActiveFlagField.Name))
				{
					entity.Set(table.ActiveFlagField.Name, false);

					EntityManager.Transaction.Add(entity);
					EntityManager.Commit();

					string message = GetMessage("DeactivateVersionApproval",
					                            Framework.Utilities.TextUtils.GetDisplayText(entity.EntityType),
					                            entity.Name);

					Library.Utils.FlashMessage(message, MainForm.Title);
				}
			}
		}

		/// <summary>
		/// Set the entity to pending if a future start date is specified
		/// </summary>
		/// <param name="entity"></param>
		private void SetPending(IEntity entity)
		{
			if (entity.CheckActive()) return;

			string approvalStatusProperty = GetApprovalStatusProperty();

			if (approvalStatusProperty != null)
			{
				entity.Set(approvalStatusProperty, PhraseApprStat.PhraseIdP);

				EntityManager.Transaction.Add(entity);
				EntityManager.Commit();
			}
		}

		/// <summary>
		/// Display option.
		/// </summary>
		protected virtual void Display()
		{
			ShowEditorMultiple();
		}

		/// <summary>
		/// Modify option.
		/// </summary>
		protected virtual void Modify()
		{
			ShowEditorMultiple();
		}

		/// <summary>
		/// Modify/Display multiple.
		/// </summary>
		private void ShowEditorMultiple()
		{
			List<IEntity> entities = FindEntities();

			if (entities.Count == 0)
			{
				Exit();
				return;
			}

			// Use this task for the first entity in the selected list

			IEntity entity = entities[entities.Count - 1];

			if ((Context.LaunchMode == TestOption) || (Library.Security.CheckPrivilege(Context.MenuProcedureNumber, entity)))
			{
				LockRecord(entity);
				ShowEditor(Context.TaskParameters[0], entity);
			}
			else
			{
				if ((Library.Environment.GetGlobalInt("CLIENT_TYPE") == 1) &&
				    (!Library.Security.GetMasterMenu(Context.MenuProcedureNumber).WebEnabled))
					throw new SampleManagerError(ServerMessageManager.Current.GetMessage("CommonMessages", "ErrorSecurityTitle"),
					                             ServerMessageManager.Current.GetMessage("WebExplorerMessages",
					                                                                     "MasterMenuAlertNotWebSupportedText",
					                                                                     Context.MenuProcedureNumber));

				throw new SampleManagerError(ServerMessageManager.Current.GetMessage("CommonMessages", "ErrorSecurityTitle"),
				                             ServerMessageManager.Current.GetMessage("CommonMessages",
				                                                                     "ErrorInsufficientPrivileges",
				                                                                     Context.MenuProcedureNumber));
			}

			// For all entities other than the first, launch a new task

			for (int i = 0; i < entities.Count - 1; i++)
			{
				int menuProc = Context.MenuProcedureNumber;
				Library.Task.CreateTask(menuProc, entities[i], string.Empty);
			}
		}

		/// <summary>
		/// Shows the editor.
		/// </summary>
		/// <param name="formName">Name of the form.</param>
		/// <param name="entity">The entity.</param>
		private void ShowEditor(string formName, IEntity entity)
		{
			Form form = FormFactory.CreateForm(formName, entity);

			form.Created += FormCreated;
			form.Loaded += FormLoaded;

			form.Show(Context.MenuWindowStyle);

			form.Closing += MainFormClosing;
		}

		/// <summary>
		/// Copy option.
		/// </summary>
		protected virtual void Copy()
		{
			// Find an appropriate entity

			IEntity entity = FindEntity();

			if (entity == null)
			{
				Exit();
				return;
			}

			// Create a copy of the entity

			m_CopiedEntity = entity;
			IEntity copy = entity.CreateCopy();

			// Check if a parameter has been passed for the new id.

			TryAssignNewEntityId(copy);

			// Copy across attachments

			if (m_CopiedEntity is IAttachments)
			{
				if (((IAttachments) m_CopiedEntity).Attachments.Count > 0)
				{
					foreach (Attachment attachment in ((IAttachments) m_CopiedEntity).Attachments)
					{
						Attachment copyOfAttachment = (Attachment) attachment.CreateCopy();
						((IAttachments) copy).Attachments.Add(copyOfAttachment);
					}
				}
			}

			// Publish the user interface passed in the task parameters.

			Form form = FormFactory.CreateForm(Context.TaskParameters[0], copy);

			// Deal with Text File copies

			if (form.TextFile != null)
			{
				form.TextFile.Copy(entity);
			}

			form.Created += FormCreated;
			form.Loaded += FormLoaded;
			form.Show(Context.MenuWindowStyle);
			form.Closing += MainFormClosing;
		}

		/// <summary>
		/// Create New Version
		/// </summary>
		protected virtual void NewVersion()
		{
			// Find an appropriate entity

			m_CopiedEntity = FindEntity();

			if (m_CopiedEntity == null)
			{
				Exit();
				return;
			}

			// Check if other versions of the record prevent a new version being created

			if (!NewVersionAllowed(m_CopiedEntity))
			{
				Exit();
				return;
			}

			// Create a copy of the entity

			IEntity copy = CreateNewVersion(m_CopiedEntity);

			m_SetAsModified = true;

			// Publish the user interface passed in the task parameters.

			Form form = FormFactory.CreateForm(Context.TaskParameters[0], copy);

			// Deal with Text File copies

			if (form.TextFile != null)
			{
				form.TextFile.Copy(m_CopiedEntity);
			}

			form.Created += FormCreated;
			form.Loaded += FormLoaded;
			form.Show(Context.MenuWindowStyle);
			form.Closing += MainFormClosing;
		}

		/// <summary>
		/// Returns true if user is allowed to create a new version.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		protected virtual bool NewVersionAllowed(IEntity entity)
		{
			// Multiple active version mode

			if (!SingleActiveVersion) return true;

			// If there's no apporval status field allow new version

			string approval = GetApprovalStatusProperty();
			if (string.IsNullOrEmpty(approval)) return true;

			// Count the number of versions that are not Approved

			ISchemaTable table = Schema.Current.Tables[Context.EntityType];

			IQuery query = EntityManager.CreateQuery(entity.EntityType);

			foreach (ISchemaField keyField in table.KeyFields)
			{
				if (keyField != table.VersionField)
				{
					query.AddEquals(keyField.Name, entity.GetString(keyField.Name));
				}
			}

			query.AddNotEquals(approval, PhraseApprStat.PhraseIdA);
			query.AddNotEquals(approval, PhraseApprStat.PhraseIdX);

			query.HideRemoved();

			int numVersions = EntityManager.SelectCount(query);

			if (numVersions > 0)
				Library.Utils.FlashMessage(Library.Message.GetMessage("GeneralMessages", "NewVersionNotAllowed"),
				                           Library.Message.GetMessage("GeneralMessages", "NewVersionNotAllowedCaption"));

			return numVersions == 0;
		}

		/// <summary>
		/// Creates the new version.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		private IEntity CreateNewVersion(IEntity entity)
		{
			IEntity copy = entity.CreateNewVersion();

			if (m_CopiedEntity is IAttachments)
			{
				if (((IAttachments) m_CopiedEntity).Attachments.Count > 0)
				{
					if (
						Library.Utils.FlashMessageYesNo(
							Library.Message.GetMessage("LaboratoryMessages", "CopyAttachmentsToNewVersion"),
							Library.Message.GetMessage("LaboratoryMessages", "AttachmentHeader")))
					{
						foreach (Attachment attachment in ((IAttachments) m_CopiedEntity).Attachments)
						{
							if (attachment.IsLatestVersion)
							{
								Attachment copyOfAttachment = (Attachment) attachment.CreateCopy();
								copyOfAttachment.Version = 1;
								((IAttachments) copy).Attachments.Add(copyOfAttachment);
							}
						}
					}
				}
			}

			return copy;
		}

		/// <summary>
		/// Remove option.
		/// </summary>
		protected virtual void Remove()
		{
			List<IEntity> entitiesToRemove = FindEntities();
			LockAllEntities(entitiesToRemove);

			foreach (IEntity entity in entitiesToRemove)
			{
				// Get the remove field

				ISchemaTable table = entity.FindSchemaTable();
				ISchemaField removeField = table.RemoveField;

				// Check this entity does have a remove field

				if (removeField == null)
				{
					throw new SampleManagerError(GetMessage("RemoveException", table.Name));
				}

				// Setup the prompt

				string prompt = GetMessage("RemoveConfirmText", entity.Name);
				string title = GetMessage("RemoveConfirmTitle", table.Name);

				if (Library.Utils.FlashMessageYesNo(prompt, title))
				{
					// Set the remove flag.

					entity.SetRemovedFlag();

					// Commit the changes to the database.

					EntityManager.Transaction.Add(entity);
					EntityManager.Commit();
				}
			}

			Exit();
		}

		/// <summary>
		/// Restore option.
		/// </summary>
		protected virtual void Restore()
		{
			List<IEntity> entitiesToRestore = FindEntities();
			LockAllEntities(entitiesToRestore);

			foreach (IEntity entity in entitiesToRestore)
			{
				// Get the remove field

				ISchemaTable table = entity.FindSchemaTable();
				ISchemaField removeField = table.RemoveField;

				// Check this entity does have a remove field

				if (removeField == null)
				{
					throw new SampleManagerError(GetMessage("RestoreException", table.Name));
				}

				// Setup the prompt

				string prompt = GetMessage("RestoreConfirmText", entity.Name);
				string title = GetMessage("RestoreConfirmTitle", table.Name);

				if (Library.Utils.FlashMessageYesNo(prompt, title))
				{
					// Set the remove flag.

					entity.ClearRemovedFlag();

					// Commit the changes to the database.

					EntityManager.Transaction.Add(entity);
					EntityManager.Commit();
				}
			}

			Exit();
		}

		/// <summary>
		/// Creates the new entity.
		/// </summary>
		/// <returns></returns>
		protected virtual IEntity CreateNewEntity()
		{
			ObjectModel.Form form = (ObjectModel.Form) EntityManager.Select(TableNames.Form, Context.TaskParameters[0]);

			if (form != null && !String.IsNullOrEmpty(form.FormEntityDefinition))
			{
				return EntityManager.CreateEntity(form.FormEntityDefinition);
			}

			return EntityManager.CreateEntity(Context.EntityType);
		}

		/// <summary>
		/// Finds the entity either by using the selected entity from the context or by prompting for one.
		/// </summary>
		/// <returns>The entity to use in the lab table.</returns>
		protected IEntity FindEntity()
		{
			IEntity entity = null;

			if (Context.SelectedItems.Count > 1)
			{
				throw new SampleManagerError(GetMessage("FindEntityException"));
			}

			if (Context.SelectedItems.Count > 0)
			{
				if (EntityIsAppropriateForMode(Context.SelectedItems[0], Context.LaunchMode))
					entity = Context.SelectedItems[0];
				else
					ShowEntityError();
			}
			else
			{
				// An entity has not been selected then prompt for one

				FormResult result;
				IQuery query = GetEntityPromptQuery();

				do
				{
					result = Library.Utils.PromptForEntity(GetMessage("FindEntity"),
					                                       Context.MenuItem.Description,
					                                       query,
					                                       out entity,
					                                       m_ShowAllVersions,
					                                       Context.MenuProcedureNumber);

					if (result == FormResult.OK)
					{
						if (EntityIsAppropriateForMode(entity, Context.LaunchMode))
							Context.SelectedItems.Add(entity);
						else
						{
							ShowEntityError();
							entity = null;
						}
					}
					else
					{
						entity = null;
					}
				} while ((entity == null) && (result == FormResult.OK));
			}

			return entity;
		}

		private void ShowEntityError()
		{
			if (!string.IsNullOrWhiteSpace(EntityError))
			{
				Library.Utils.FlashMessage(EntityError, GetMessage("LabTableErrorTitle"));
				EntityError = string.Empty;
			}
		}

		/// <summary>
		/// Gets the entity prompt query.
		/// </summary>
		/// <returns></returns>
		protected virtual IQuery GetEntityPromptQuery()
		{
			IQuery query = EntityManager.CreateQuery(Context.EntityType);

			// Restore Option - show all removed versions

			if (Context.LaunchMode == RestoreOption)
			{
				query.RemovedOnly();
				m_ShowAllVersions = TriState.Yes;
				return query;
			}

			query.HideRemoved();

			string approval = GetApprovalStatusProperty();
			if (string.IsNullOrEmpty(approval)) return query;

			// Submit Option - only for ones pending inspection

			if (Context.LaunchMode == SubmitOption)
			{
				query.AddEquals(approval, PhraseApprStat.PhraseIdV);
				m_ShowAllVersions = TriState.Yes;
				return query;
			}

			// Modify Option - Only ones that are modifiable status

			if (Context.LaunchMode == ModifyOption)
			{
				if (!string.IsNullOrWhiteSpace(query.WhereClause))
					query.AddAnd();

				query.PushBracket();
				query.AddEquals(approval, PhraseApprStat.PhraseIdV);
				query.AddOr();
				query.AddEquals(approval, PhraseApprStat.PhraseIdR);
				query.AddOr();
				query.AddEquals(approval, PhraseApprStat.PhraseIdP);

				if (IsApprovalLow() && Library.Environment.GetGlobalBoolean("APPROVAL_SUBMIT_ON_ADD"))
				{
					query.AddOr();
					query.PushBracket();

					query.AddEquals(approval, PhraseApprStat.PhraseIdA);

					if (SingleActiveVersion && (GetApprovalActiveProperty() != null))
					{
						query.AddAnd();
						query.AddEquals(GetApprovalActiveProperty(), true);
					}

					query.PopBracket();
				}

				query.PopBracket();

				m_ShowAllVersions = TriState.Yes;

				return query;
			}

			// Display Option - Make the query pick all entries - all versions/status

			if (Context.LaunchMode == DisplayOption)
			{
				if (Library.Environment.GetGlobalBoolean("SHOW_VERSIONS"))
				{
					m_ShowAllVersions = TriState.Yes;
				}
				else
				{
					query.AddEquals(approval, PhraseApprStat.PhraseIdA);
					m_ShowAllVersions = TriState.No;
				}
			}

			// New Version Option - Only ones that are authorised status

			if (Context.LaunchMode == NewVersionOption)
			{
				query.AddEquals(approval, PhraseApprStat.PhraseIdA);

				m_ShowAllVersions = TriState.Yes;

				return query;
			}

			// Approve Option - Only ones that are in Inspection

			if (Context.LaunchMode == ApproveOption)
			{
				query.AddEquals(approval, PhraseApprStat.PhraseIdI);

				m_ShowAllVersions = TriState.Yes;

				return query;
			}

			return query;
		}


		/// <summary>
		/// Checks if entity is appropriate for mode.
		/// </summary>
		/// <returns></returns>
		protected virtual bool EntityIsAppropriateForMode(IEntity entity,
		                                                  string modeToCheck)
		{

			// Remove Option - Check is not removed

			if (modeToCheck == RemoveOption)
			{
				ISchemaTable table = entity.FindSchemaTable();
				ISchemaField removeField = table.RemoveField;

				// Check this entity does have a remove field

				if (removeField == null)
				{
					throw new SampleManagerError(GetMessage("RestoreException", table.Name));
				}

				if (entity.IsRemoved())
					EntityError = GetMessage("LabTableErrorRemoved", entity.Name);

				return !entity.IsRemoved();
			}

			// Restore Option - Check is removed

			if (modeToCheck == RestoreOption)
			{
				ISchemaTable table = entity.FindSchemaTable();
				ISchemaField removeField = table.RemoveField;

				// Check this entity does have a remove field

				if (removeField == null)
				{
					throw new SampleManagerError(GetMessage("RestoreException", table.Name));
				}

				if (!entity.IsRemoved())
					EntityError = GetMessage("LabTableErrorNotRemoved", entity.Name);

				return entity.IsRemoved();
			}

			if ((modeToCheck != DisplayOption) && (entity.IsRemoved()))
			{
				EntityError = GetMessage("LabTableErrorRemoved", entity.Name);
				return false;
			}

			string approval = GetApprovalStatusProperty();
			if (string.IsNullOrEmpty(approval)) return true;

			// Submit Option - only for ones pending inspection

			if (modeToCheck == SubmitOption)
			{
				if (IsApprovalCancelled(entity) ||
				    IsApprovalInspection(entity) ||
				    IsApprovalAuthorised(entity))
				{
					EntityError = GetMessage("LabTableErrorStatus",
					                         modeToCheck,
					                         entity.Name,
					                         GetApprovalStatus(entity).Name);
					return false;
				}

				return true;
			}

			// Modify Option - Only ones that are modifiable status

			if (modeToCheck == ModifyOption)
			{
				if (IsApproval(entity, PhraseApprStat.PhraseIdV) ||
				    IsApproval(entity, PhraseApprStat.PhraseIdR) ||
				    IsApproval(entity, PhraseApprStat.PhraseIdP))
					return true;

				if (IsApproval(entity, PhraseApprStat.PhraseIdA))
				{
					if (IsApprovalLow())
					{
						if (!SingleActiveVersion)
							return true;

						if (IsActive(entity))
						{
							if (!Library.Environment.GetGlobalBoolean("APPROVAL_SUBMIT_ON_ADD"))
							{
								EntityError = GetMessage("SingleVersionErrorNoSubmit");
								return false;
							}

							ISchemaTable table = entity.FindSchemaTable();
							ISchemaField inspectionField = table.InspectionField;

							if ((inspectionField != null) && !entity.GetEntity(inspectionField.Name).IsNull())
							{
								EntityError = GetMessage("SingleVersionErrorInspection");
								return false;
							}

							return true;
						}
					}

					EntityError = GetMessage("LabTableErrorModifyApproved",
					                         entity.Name);
					return false;
				}

				EntityError = GetMessage("LabTableErrorStatus",
				                         modeToCheck,
				                         entity.Name,
				                         GetApprovalStatus(entity).Name);
				return false;
			}

			// Display Option - Check the value of show versions

			if (modeToCheck == DisplayOption)
			{
				if (Context.ExplorerFolderShowVersions ||
				    Library.Environment.GetGlobalBoolean("SHOW_VERSIONS"))
					return true;

				if (IsApprovalAuthorised(entity))
					return true;

				EntityError = GetMessage("LabTableErrorDisplayApproved",
				                         entity.Name);

				return false;
			}

			// New Version Option - Only ones that are authorised status

			if (modeToCheck == NewVersionOption)
			{
				if (IsApprovalAuthorised(entity))
					return true;

				EntityError = GetMessage("LabTableErrorStatus",
				                         modeToCheck,
				                         entity.Name,
				                         GetApprovalStatus(entity).Name);
				return false;
			}

			// Approve Option - Only ones that are in Inspection

			if (modeToCheck == ApproveOption)
			{
				if (IsApproval(entity, PhraseApprStat.PhraseIdI))
					return true;

				EntityError = GetMessage("LabTableErrorStatus",
				                         modeToCheck,
				                         entity.Name,
				                         GetApprovalStatus(entity).Name);

				return false;
			}

			return true;
		}

		/// <summary>
		/// Finds a list of entities either by using the selected entities from the context or by prompting for one.
		/// </summary>
		/// <returns></returns>
		protected List<IEntity> FindEntities()
		{
			List<IEntity> entities = new List<IEntity>();

			if (Context.SelectedItems.Count == 0)
			{
				// No entities have been selected so call FindEntity so that the choose entity prompt is displayed

				IEntity entity = FindEntity();
				if (entity == null)
				{
					// No entity has been provided so exit

					return entities;
				}

				// Add the entity to the list

				entities.Add(entity);
			}
			else
			{
				// Add all selected entities to the list of entities to be removed

				entities = GetSelectedEntities(Context.SelectedItems);
			}

			if ((Context.LaunchMode == ModifyOption) && (entities.Count == 0) && (Context.SelectedItems.Count > 0))
			{
				string originalError = EntityError;

				entities = GetSelectedEntities(Context.SelectedItems, DisplayOption);

				if (entities.Count > 0)
				{
					// Give the user the option to continue in display mode

					string caption = Library.Message.GetMessage("GeneralMessages", "DisplayNotModifyCaption");
					string message = Library.Message.GetMessage("GeneralMessages", "DisplayNotModify", originalError);

					if (Library.Utils.FlashMessageYesNo(message, caption))
					{
						ForceDisplay = true;
					}
					else
					{
						entities = new List<IEntity>();
						EntityError = string.Empty;
					}
				}
				else
				{
					EntityError = originalError;
				}
			}

			if (entities.Count == 0)
			{
				ShowEntityError();
			}

			return entities;
		}

		/// <summary>
		/// Gets the selected entities.
		/// </summary>
		/// <param name="selectedEntities">The selected entities.</param>
		/// <param name="modeToCheck">The mode to check.</param>
		/// <returns></returns>
		protected virtual List<IEntity> GetSelectedEntities(IEntityCollection selectedEntities,
		                                                    string modeToCheck)
		{
			List<IEntity> entities = new List<IEntity>();

			foreach (IEntity entity in selectedEntities)
			{
				if (EntityIsAppropriateForMode(entity, modeToCheck))
					entities.Add(entity);
			}

			return entities;
		}

		/// <summary>
		/// Gets the selected entities.
		/// </summary>
		/// <returns></returns>
		protected virtual List<IEntity> GetSelectedEntities(IEntityCollection selectedEntities)
		{
			return GetSelectedEntities(selectedEntities, Context.LaunchMode);
		}

		/// <summary>
		/// List option.
		/// </summary>
		protected virtual void List()
		{
			//Make sure menu item is setup correctly
			if (Context.TaskParameters.Length == 0 || string.IsNullOrEmpty(Context.TaskParameters[0]))
			{
				throw new SampleManagerError(string.Format(GetMessage("EditorModeException", "")));
			}

			//Select List report data
			IEntityCollection listItems;
			if (Context.SelectedItems.Count > 1)
			{
				//List the selected items in folder
				listItems = Context.SelectedItems;
			}
			else
			{
				if (Context.FolderQuery == null)
				{
					//List all items for this entity type
					listItems = EntityManager.Select(Context.EntityType, Context.FolderQuery);
				}
				else
				{
					//List all items in the folder
					listItems = EntityManager.Select(Context.FolderQuery.TableName, Context.FolderQuery);
				}
			}

			//Get the List report name
			string reportName = Context.TaskParameters[0];

			//Is this the default LTE List Report
			if (reportName == DefaultListReport)
			{
				//Display the default report
				Library.Reporting.PreviewDefaultLTEReport(DefaultReport.List, listItems);
			}
			else
			{
				//Preview the report
				Library.Reporting.PreviewReport(reportName, listItems, new ReportOptions());
			}
		}

		/// <summary>
		/// Print option.
		/// </summary>
		protected virtual void Print()
		{
			//Make sure menu item is setup correctly
			if (Context.TaskParameters.Length == 0 || string.IsNullOrEmpty(Context.TaskParameters[0]))
			{
				throw new SampleManagerError(string.Format(GetMessage("EditorModeException", "")));
			}

			//Get the List report name
			string reportName = Context.TaskParameters[0];

			//Is this the default LTE Print Report
			if (reportName == DefaultPrintReport)
			{
				//Display the default report
				Library.Reporting.PreviewDefaultLTEReport(DefaultReport.Print, Context.SelectedItems);
			}
			else
			{
				//Preview the report
				Library.Reporting.PreviewReport(reportName, Context.SelectedItems, new ReportOptions());
			}
		}

		/// <summary>
		/// Print option.
		/// </summary>
		protected virtual void PrintListLabel()
		{
			IEntity labelTemplate = null;
			string parameters = Context.TaskParameters[0].Trim();

			IEntity entityToPrint = null;
			IEntityCollection entitiesToPrint = null;

			// Get the selected items or prompt for one

			if (Context.SelectedItems.Count > 0)
			{
				entitiesToPrint = Context.SelectedItems;
			}
			else
			{
				Library.Utils.PromptForEntity(GetMessage("FindEntity"),
				                              Context.MenuItem.Description,
				                              Context.EntityType,
				                              out entityToPrint);
			}

			// See if entity pass in has a label template as a property if so use that, if not check
			// parameters then as a last resort prompt for it.

			if (entityToPrint != null && entityToPrint.ContainsProperty("LabelTemplate"))
			{
				labelTemplate = entityToPrint.GetEntity("LabelTemplate");
			}
			else if (entitiesToPrint != null && entitiesToPrint[0].ContainsProperty("LabelTemplate"))
			{
				labelTemplate = entitiesToPrint[0].GetEntity("LabelTemplate");
			}

			if (parameters != string.Empty)
			{
				labelTemplate = EntityManager.Select(TableNames.LabelTemplate, parameters);
			}

			if (labelTemplate == null)
			{
				labelTemplate = PromptForLabelTemplate();
			}

			// Print the entity or entities

			if (labelTemplate != null)
			{
				if (entitiesToPrint != null)
				{
					Library.Utils.PrintLabel((LabelTemplate) labelTemplate, entitiesToPrint);
				}
				else if (entityToPrint != null)
				{
					Library.Utils.PrintLabel((LabelTemplate) labelTemplate, entityToPrint);
				}
			}
		}

		/// <summary>
		/// Prompts for label template.
		/// </summary>
		/// <returns></returns>
		private IEntity PromptForLabelTemplate()
		{
			IEntity labelTemplate;
			IQuery query = EntityManager.CreateQuery(TableNames.LabelTemplate);
			query.AddEquals(LabelTemplatePropertyNames.DataEntityDefinition, Context.EntityType);

			string templatePrompt = Library.Message.GetMessage("LabelTemplateMessages",
			                                                   "SelectLabelTemplate",
			                                                   new[] {Context.SelectedItems.EntityType});

			FormResult result = Library.Utils.PromptForEntity(templatePrompt,
			                                                  Context.MenuItem.Description,
			                                                  query,
			                                                  out labelTemplate);

			if (result != FormResult.OK)
			{
				labelTemplate = null;
			}
			return labelTemplate;
		}

		#endregion

		#region Approval

		/// <summary>
		/// Submit option.
		/// </summary>
		protected virtual void Submit()
		{
			m_SetAsModified = true;
			ShowEditorMultiple();
		}

		/// <summary>
		/// Table has approval fields
		/// </summary>
		/// <returns></returns>
		private bool TableHasApproval()
		{
			// See if we have an approval status field

			string property = GetApprovalStatusProperty();
			if (string.IsNullOrEmpty(property)) return false;

			// See if we have an inspection field

			ISchemaTable table = Schema.Current.Tables[Context.EntityType];
			if (table == null) return false;
			if (table.InspectionField == null) return false;

			return true;
		}

		/// <summary>
		/// Check Approval Before Saving
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		private bool ApprovalCheckPreSave(IEntity entity)
		{
			// Drop out successfully if approval is not relevant

			if (Context.LaunchMode == DisplayOption) return true;

			ISchemaTable table = Schema.Current.Tables[Context.EntityType];

			ResetApprovalStatus(table, entity);

			InspectionHeader inspectHead = (InspectionHeader) entity.GetEntity(table.InspectionField.Name);

			if (inspectHead.IsNull() && !IsApprovalLow())
			{
				// Inspection Plan is Missing

				Library.Utils.FlashMessage(Library.VGL.GetMessage("GEN_APPR_MUST_GIVE_PLAN"), MainForm.Title);
				return false;
			}

			if (IsApprovalLow())
			{
				if (WasActive && SingleActiveVersion)
				{
					if (!Library.Environment.GetGlobalBoolean("APPROVAL_SUBMIT_ON_ADD"))
					{
						Library.Utils.FlashMessage(GetMessage("SingleVersionErrorNoSubmit"), MainForm.Title);
						return false;
					}

					if (!inspectHead.IsNull())
					{
						Library.Utils.FlashMessage(GetMessage("SingleVersionErrorInspection"), MainForm.Title);
						return false;
					}
				}

				return true;
			}

			if (inspectHead.InspectionRecords.Count == 0)
			{
				// Inspection is Empty

				if (Library.Environment.GetGlobalString("APPROVAL_INSPECT_SECURITY") == ApprovalSecurityHigh)
				{
					Library.Utils.FlashMessage(Library.VGL.GetMessage("GEN_APPR_NO_INSP_ANOTHER"), MainForm.Title);
					return false;
				}

				if (!Library.Utils.FlashMessageYesNo(Library.VGL.GetMessage("GEN_APPR_NO_INSP_CONT"), MainForm.Title))
				{
					return false;
				}
			}
			else if (Library.Environment.GetGlobalString("APPROVAL_INSPECT_SECURITY") == ApprovalSecurityHigh)
			{
				// Make sure the current user is not on the inspection plan

				Personnel currentUser = (Personnel) Library.Environment.CurrentUser;
				bool userOnPlan = false;

				foreach (InspectionRecord inspectRect in inspectHead.InspectionRecords)
				{
					if (inspectRect.InspectionUserType.PhraseId == PhraseInspUser.PhraseIdUSER)
					{
						if (inspectRect.PersonnelId.Identity == currentUser.Identity)
						{
							userOnPlan = true;
						}
					}
				}

				if (userOnPlan)
				{
					Library.Utils.FlashMessage(Library.VGL.GetMessage("GEN_APPR_NOT_BELONG"), MainForm.Title);
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Resets the approval status.
		/// </summary>
		/// <param name="table">The table.</param>
		/// <param name="entity">The entity.</param>
		private void ResetApprovalStatus(ISchemaTable table, IEntity entity)
		{
			if (table.ApprovalStatusField != null)
			{
				entity.Set(table.ApprovalStatusField.Name, PhraseApprStat.PhraseIdV);
				m_ApprovalStatus = string.Empty;
			}
		}

		/// <summary>
		/// Submit for Approval
		/// </summary>
		protected virtual bool ShouldSubmitOnSave(IEntity entity)
		{
			if (Context.LaunchMode == SubmitOption) return true;

			return Library.Environment.GetGlobalBoolean("APPROVAL_SUBMIT_ON_ADD") && (Submittable(entity));
		}

		/// <summary>
		/// Is this submittable
		/// </summary>
		/// <returns></returns>
		private bool Submittable(IEntity entity)
		{
			if (GetInspectionField() == null) return false;
			if (Context.LaunchMode == AddOption || Context.LaunchMode == CopyOption || Context.LaunchMode == NewVersionOption)
				return true;
			if (IsApprovalInspection(entity) || IsApprovalCancelled(entity) || IsApprovalAuthorised(entity)) return false;
			return true;
		}

		/// <summary>
		/// Gets the inspection field.
		/// </summary>
		/// <returns></returns>
		private ISchemaField GetInspectionField()
		{
			ISchemaTable table = Schema.Current.Tables[Context.EntityType];
			if (table == null) return null;
			return table.InspectionField;
		}

		/// <summary>
		/// Submit for Approval
		/// </summary>
		/// <param name="entity">The entity.</param>
		protected virtual void SubmitOnSave(IEntity entity)
		{
			// Work out if we should submit for approval

			bool lowSecurity = (Library.Environment.GetGlobalString("APPROVAL_INSPECT_SECURITY") == ApprovalSecurityLow);

			// If a plan is specified then we're going to ask - even if in low security mode.

			ISchemaField field = GetInspectionField();
			if (field != null)
			{
				IEntity plan = entity.GetEntity(field.Name);
				if (plan != null && !plan.IsNull()) lowSecurity = false;
			}

			// Optionally Ask the user if they wish to submit for inspection.

			if (lowSecurity || Context.LaunchMode == SubmitOption)
			{
				m_Submitted = true;
			}
			else
			{
				string message = Library.VGL.GetMessage("LABTABLIB_SUBMIT_NOW");
				m_Submitted = Library.Utils.FlashMessageYesNo(message, MainForm.Title);
			}

			// Submit using VGL

			if (m_Submitted)
			{
				SubmitForApproval(entity, lowSecurity);
			}
		}

		/// <summary>
		/// Submits for approval.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="quiet">if set to <c>true</c> [quiet].</param>
		private void SubmitForApproval(IEntity entity, bool quiet)
		{
			// Release the lock whilst the VGL does the work.

			bool locked = entity.Locked;
			if (locked) entity.LockRelease();

			// Run the VGL to do the submission

			ISchemaTable table = Schema.Current.Tables[Context.EntityType];

			m_ApprovalStatus = (string) Library.VGL.RunVGLRoutine("$GEN_APPR",
			                                                      "GEN_APPR_SUB_ENTRY_EX",
			                                                      table.Name,
			                                                      entity.IdentityString);

			// Raise the watcher event

			EntityManager.SetEntityCacheAsOutOfDate();
			EntityManager.Reselect(entity);

			Library.EntityWatcher.RaiseQueuedEvents();

			// Relock (this might just be the apply button)

			if (locked) entity.Lock();

			// Tell the user if it was authorised

			if (m_ApprovalStatus == PhraseApprStat.PhraseIdA)
			{
				if (!quiet)
				{
					string message = Library.VGL.GetMessage("LABTABLIB_AUTO_APPR");
					Library.Utils.FlashMessage(message, MainForm.Title);
				}

				return;
			}

			if (m_ApprovalStatus != PhraseApprStat.PhraseIdI)
			{
				string message = Library.VGL.GetMessage("LABTABLIB_SUBMIT_FAILED");
				Library.Utils.FlashMessage(message, MainForm.Title);
			}
		}

		#endregion

		#region Inspection Utilities

		/// <summary>
		/// Approve option.
		/// </summary>
		protected virtual void Approve()
		{
			InspectionEntity = FindEntity();

			if (InspectionEntity == null)
			{
				Exit();
				return;
			}

			// Privileged user can approve as deputy

			if (Library.Security.CheckPrivilege(PrivilegeApproveOnBehalf))
			{
				InspectionUser = string.Empty;

				ApprovalChooseUserForm =
					(FormApprovalChooseUser) FormFactory.CreateForm(FormApprovalChooseUser.GetInterfaceName());

				ApprovalChooseUserForm.Created += ApprovalChooseUserFormCreated;
				ApprovalChooseUserForm.Loaded += ApprovalChooseUserLoaded;

				ApprovalChooseUserForm.ShowDialog();
			}
			else
			{
				InspectionUser = ((IEntity) Library.Environment.CurrentUser).IdentityString;

				if (UserCanInspect(InspectionEntity, InspectionUser))
				{
					// All's good, show the property sheet

					ApprovalShowMainForm();
				}
				else
				{
					Exit();
				}
			}
		}

		private void ApprovalChooseUserFormCreated(object sender, EventArgs e)
		{
			ApprovalChooseUserForm.RadioCurrentUser.Caption += string.Format(" - {0}",
			                                                                 ((IEntity) Library.Environment.CurrentUser).Name);
		}

		private void ApprovalChooseUserLoaded(object sender, EventArgs e)
		{
			ApprovalChooseUserForm.Closing += ApprovalChooseUserClosing;
			ApprovalChooseUserForm.Closed += ApprovalChooseUserClosed;

			ApprovalChooseUserForm.RadioChooseUser.CheckedChanged += ApprovalChooseUserRadioChooseUserCheckedChanged;
		}

		private void ApprovalChooseUserRadioChooseUserCheckedChanged(object sender,
		                                                             Library.ClientControls.CheckedChangedEventArgs e)
		{
			ApprovalChooseUserForm.PromptOperator.Enabled = e.Checked;
		}

		private void ApprovalChooseUserClosing(object sender, CancelEventArgs e)
		{
			if (ApprovalChooseUserForm.FormResult == FormResult.OK)
			{
				if (ApprovalChooseUserForm.RadioChooseUser.Checked)
				{
					if ((ApprovalChooseUserForm.PromptOperator.Entity != null) &&
					    (ApprovalChooseUserForm.PromptOperator.Entity.IsValid()))
						InspectionUser = ApprovalChooseUserForm.PromptOperator.Entity.IdentityString;
					else
						e.Cancel = true;
				}
				else
				{
					InspectionUser = ((IEntity) Library.Environment.CurrentUser).IdentityString;
				}
			}
			else
			{
				InspectionUser = string.Empty;
			}
		}

		private void ApprovalChooseUserClosed(object sender, EventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(InspectionUser) && UserCanInspect(InspectionEntity, InspectionUser))
			{
				// All's good, show the property sheet

				ApprovalShowMainForm();
			}
			else
			{
				ApprovalChooseUserForm.Closing -= ApprovalChooseUserClosing;
				ApprovalChooseUserForm.Closed -= ApprovalChooseUserClosed;

				Exit();
			}
		}

		/// <summary>
		/// Check Inspection Before Saving
		/// </summary>
		private void InspectionPreSave()
		{
			// Prompt for authorisation comments

			ApprovalConfirmationResult = ApprovalResultType.NotApproved;

			ApprovalConfirmationForm =
				(FormApprovalConfirmation) FormFactory.CreateForm(FormApprovalConfirmation.GetInterfaceName());

			ApprovalConfirmationForm.Loaded += ApprovalConfirmationFormLoaded;

			ApprovalConfirmationForm.Show();
		}

		private void ApprovalConfirmationFormLoaded(object sender, EventArgs e)
		{
			ApprovalConfirmationForm.Closed += ApprovalConfirmationFormClosed;

			ApprovalConfirmationForm.ButtonApprove.ClickAndWait += ButtonApproveClick;
			ApprovalConfirmationForm.ButtonReject.ClickAndWait += ButtonRejectClick;
		}

		private void ButtonApproveClick(object sender, EventArgs e)
		{
			ApprovalConfirmationResult = ApprovalResultType.Approved;
			ApprovalConfirmationForm.ActionButtonCancel.PerformClick();
		}

		private void ButtonRejectClick(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(ApprovalConfirmationForm.CommentPrompt.ValueForQuery as string))
			{
				Library.Utils.FlashMessage(Library.VGL.GetMessage("GEN_APPR_MUST_COMMENT"), MainForm.Title);
				return;
			}

			ApprovalConfirmationResult = ApprovalResultType.Rejected;
			ApprovalConfirmationForm.ActionButtonCancel.PerformClick();
		}

		private void ApprovalConfirmationFormClosed(object sender, EventArgs e)
		{
			InspectionEntity.LockRefused = false;
			InspectionEntity.LockRelease();

			string approvalReview;

			if (ApprovalConfirmationResult == ApprovalResultType.Approved)
			{
				approvalReview = ApprovalReviewApproved;
			}
			else if (ApprovalConfirmationResult == ApprovalResultType.Rejected)
			{
				approvalReview = ApprovalReviewRejected;
			}
			else
			{
				approvalReview = string.Empty;
			}

			ApprovalConfirmationResult = ApprovalResultType.Cancel;

			if (!string.IsNullOrWhiteSpace(approvalReview))
			{
				ISchemaTable table = Schema.Current.Tables[InspectionEntity.EntityType];

				var inspectResult = Library.VGL.RunVGLRoutine("$GEN_APPR",
				                                              "GEN_APPR_APPR_ENTRY_EX",
				                                              table.Name,
				                                              InspectionEntity.IdentityString,
				                                              InspectionUser,
				                                              approvalReview,
				                                              ApprovalConfirmationForm.CommentPrompt.ValueForQuery ?? string.Empty);

				if ((inspectResult is string) && (!string.IsNullOrWhiteSpace(inspectResult as string)))
				{
					Library.Utils.FlashMessage(inspectResult as string, GetMessage("ApprovalErrorCaption"));
				}
				else if (SingleActiveVersion)
				{
					// Raise the watcher event

					EntityManager.SetEntityCacheAsOutOfDate();
					EntityManager.Reselect(InspectionEntity);

					Library.EntityWatcher.RaiseQueuedEvents();

					SetPending(InspectionEntity);
				}
			}

			Library.Task.StateModified(false);
			MainForm.Close();
		}

		/// <summary>
		/// Can the user inspect the current entity
		/// </summary>
		/// <returns></returns>
		protected virtual bool UserCanInspect(IEntity inspectionEntity, string inspectionUser)
		{
			ISchemaTable table = Schema.Current.Tables[inspectionEntity.EntityType];

			var canInspect = Library.VGL.RunVGLRoutine("$LIB_INSPECT",
			                                           "LIB_INSPECT_USER_CAN_INSPECT",
			                                           table.Name,
			                                           inspectionEntity.IdentityString,
			                                           inspectionUser);

			bool userCanInspect = canInspect is bool && (bool) canInspect;

			if (!userCanInspect)
			{
				string caption = Library.Message.GetMessage("GeneralMessages", "ApprovalNotRequiredCaption");
				string message = Library.Message.GetMessage("GeneralMessages",
				                                            "ApprovalNotRequired",
				                                            inspectionUser.Trim(),
				                                            inspectionEntity.Name);

				Library.Utils.FlashMessage(message, caption);
			}

			return userCanInspect;
		}

		/// <summary>
		///  Lock the record for approval
		/// </summary>
		/// <param name="entity">The entity under inspection.</param>
		private bool ApprovalLockRecord(IEntity entity)
		{
			// Try and lock the record

			if (entity.Lock())
			{
				entity.LockRefused = true;
				return true;
			}

			string caption = Library.Message.GetMessage("GeneralMessages", "LockUnable");
			string message = Library.Locking.GetLockMessage(entity);

			Library.Utils.FlashMessage(message, caption);

			// Drop out, user not allowed to continue

			Exit();
			return false;
		}

		/// <summary>
		/// Shows the main form in Approval mode
		/// </summary>
		private void ApprovalShowMainForm()
		{
			m_SetAsModified = true;
			ForceDisplay = true;

			if (Library.Security.CheckPrivilege(Context.MenuProcedureNumber, InspectionEntity))
			{
				if (ApprovalLockRecord(InspectionEntity))
					ShowEditor(Context.TaskParameters[0], InspectionEntity);
			}
		}

		#endregion

		#region Approval Utilities

		/// <summary>
		/// Determines whether is approval level is low.
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if is approval low; otherwise, <c>false</c>.
		/// </returns>
		private bool IsApprovalLow()
		{
			return (Library.Environment.GetGlobalString("APPROVAL_INSPECT_SECURITY") == ApprovalSecurityLow);
		}

		/// <summary>
		/// Determines whether is approval is desirable
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if is approval mode > low; otherwise, <c>false</c>.
		/// </returns>
		private bool IsApprovalWanted()
		{
			return (!IsApprovalLow());
		}

		/// <summary>
		/// Gets the approval active property.
		/// </summary>
		/// <returns></returns>
		private string GetApprovalActiveProperty()
		{
			ISchemaTable table = Schema.Current.Tables[Context.EntityType];
			if (table == null) return null;
			if (table.ActiveFlagField == null) return null;

			return (table.ActiveFlagField.Name);
		}

		/// <summary>
		/// Gets the approval status property.
		/// </summary>
		/// <returns></returns>
		private string GetApprovalStatusProperty()
		{
			ISchemaTable table = Schema.Current.Tables[Context.EntityType];
			if (table == null) return null;
			if (table.ApprovalStatusField == null) return null;

			return (table.ApprovalStatusField.Name);
		}

		/// <summary>
		/// Gets the approval status property.
		/// </summary>
		/// <returns></returns>
		protected IEntity GetApprovalStatus(IEntity entity)
		{
			string property = GetApprovalStatusProperty();
			if (string.IsNullOrEmpty(property)) return null;
			return entity.GetEntity(property);
		}

		/// <summary>
		/// Determines whether the entity is authorised
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns>
		/// 	<c>true</c> if authorised; otherwise, <c>false</c>.
		/// </returns>
		protected bool IsApprovalAuthorised(IEntity entity)
		{
			return (IsApproval(entity, PhraseApprStat.PhraseIdA));
		}

		/// <summary>
		/// Determines whether the entity is cancelled
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns>
		/// 	<c>true</c> if cancelled; otherwise, <c>false</c>.
		/// </returns>
		protected bool IsApprovalCancelled(IEntity entity)
		{
			return (IsApproval(entity, PhraseApprStat.PhraseIdX));
		}

		/// <summary>
		/// Determines whether the entity is in inspection
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns>
		/// 	<c>true</c> if in inspection; otherwise, <c>false</c>.
		/// </returns>
		protected bool IsApprovalInspection(IEntity entity)
		{
			return (IsApproval(entity, PhraseApprStat.PhraseIdI));
		}

		/// <summary>
		/// Determines whether the specified entity is of approval status
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="ofStatus">The of status.</param>
		/// <returns>
		/// 	<c>true</c> if the specified entity is approval status; otherwise, <c>false</c>.
		/// </returns>
		protected bool IsApproval(IEntity entity, string ofStatus)
		{
			if (!string.IsNullOrEmpty(m_ApprovalStatus))
			{
				return (m_ApprovalStatus == ofStatus);
			}
			IEntity status = GetApprovalStatus(entity);
			if (!BaseEntity.IsValid(status)) return false;
			return (status.Name == ofStatus);
		}

		/// <summary>
		/// Returns true if single active version mode is enabled.
		/// </summary>
		/// <returns></returns>
		protected virtual bool SingleActiveVersion
		{
			get
			{
				if (Library.Environment.CheckGlobalExists(ApprovalSingleActiveVersion))
					return Library.Environment.GetGlobalBoolean(ApprovalSingleActiveVersion);

				return false;
			}
		}

		/// <summary>
		/// Determines whether the specified entity has its active flag set
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns>
		/// 	<c>true</c> if the specified entity is active; otherwise, <c>false</c>.
		/// </returns>
		protected bool IsActive(IEntity entity)
		{
			string property = GetApprovalActiveProperty();
			if (string.IsNullOrEmpty(property)) return true;
			return entity.GetBoolean(property);
		}

		/// <summary>
		/// Gets or sets a value indicating whether the entity was originally active
		/// </summary>
		/// <value>
		///   <c>true</c> if the entity was active; otherwise, <c>false</c>.
		/// </value>
		protected bool WasActive { get; set; }

		/// <summary>
		/// Records the initial approval state of the entity.
		/// </summary>
		private void InitialApprovalState()
		{
			WasActive = IsActive(MainForm.Entity) && IsApprovalAuthorised(MainForm.Entity) && !MainForm.Entity.IsNew();
		}

		#endregion

		#region Locking

		/// <summary>
		/// Locks all entities.
		/// </summary>
		/// <param name="entities">The entities.</param>
		protected static void LockAllEntities(IEnumerable<IEntity> entities)
		{
			foreach (IEntity entity in entities)
			{
				entity.LockOrThrow();
			}
		}

		/// <summary>
		///  Lock if Modify or Submit - Can change mode to Display if the user chooses to.
		/// </summary>
		/// <param name="entity">The entity.</param>
		private void LockRecord(IEntity entity)
		{
			// If ForceDisplay, then pretend the record is locked.

			if (ForceDisplay)
			{
				entity.LockRefused = true;
				return;
			}

			// Only Care about modification locks in modify mode

			if ((Context.LaunchMode != ModifyOption) &&
			    (Context.LaunchMode != SubmitOption))
				return;

			// Check the Approval Status

			bool wrongStatus = IsApprovalCancelled(entity) || IsApprovalInspection(entity);

			// If this entity is Authorise and not low security fail the lock

			if (IsApprovalWanted() && IsApprovalAuthorised(entity)) wrongStatus = true;

			if (wrongStatus)
			{
				string error = Library.Message.GetMessage("GeneralMessages", "InvalidApprovalStatus");
				string errorCap = Library.Message.GetMessage("GeneralMessages", "InvalidApprovalStatusCaption");
				Library.Utils.ShowAlert(errorCap, entity.Icon, error);
				entity.LockRefused = true;
				return;
			}

			// Try and lock the record

			if (entity.Lock()) return;

			if (Context.LaunchMode == ModifyOption)
			{
				// Give the user the option to continue in display mode

				string caption = Library.Message.GetMessage("GeneralMessages", "LockedForModifyCaption");
				string format = Library.Message.GetMessage("GeneralMessages", "LockedForModify");

				string lockInfo = Library.Locking.GetMultilineLockMessage(entity);
				string message = string.Format(format, lockInfo);

				if (Library.Utils.FlashMessageYesNo(message, caption)) return;
			}
			else
			{
				string caption = Library.Message.GetMessage("GeneralMessages", "LockUnable");
				string message = Library.Locking.GetLockMessage(entity);

				Library.Utils.FlashMessage(message, caption);
			}

			// Drop out, user not allowed to continue

			Exit();
		}

		#endregion

		#region Non-Modifiable

		/// <summary>
		/// Confirms that you really want to modify a modifiable record.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		private bool ConfirmModifiable(IEntity entity)
		{
			if (entity.IsModifiable()) return true;
			string message = GetMessage("ConfirmModifiable");
			return (Library.Utils.FlashMessageYesNo(message, MainForm.Title));
		}

		/// <summary>
		/// Warns if non modifiable.
		/// </summary>
		private void WarnIfNonModifiable()
		{
			if (MainForm.Entity.IsModifiable()) return;
			if (Context.LaunchMode != ModifyOption) return;

			var securityService = (ISecurityService) Library.GetService(typeof (ISecurityService));

			if (securityService.CheckPrivilege(SMPrivilege.AccessNoneModifiable))
			{
				string caption = ServerMessageManager.Current.GetMessage("GeneralMessages", "WarnOverrideCaption");
				string message = ServerMessageManager.Current.GetMessage("GeneralMessages", "WarnOverrideMessage");

				Library.Utils.ShowAlert(caption, "HAMMER", message);
				return;
			}

			string failCaption = ServerMessageManager.Current.GetMessage("GeneralMessages", "WarnOverrideFailCaption");
			string failMessage = ServerMessageManager.Current.GetMessage("GeneralMessages", "WarnOverrideFailMessage");

			Library.Utils.ShowAlert(failCaption, "MESSAGE", failMessage);
		}

		#endregion

		#region Workflow Task

		/// <summary>
		/// Creates from workflow.
		/// </summary>
		private void CreateFromWorkflow()
		{
			// Find an appropriate entity

			if (Context.Workflow == null)
			{
				Exit();
				return;
			}

			// Start the workflow

			WorkflowPropertyBag propertyBag = new WorkflowPropertyBag();

			Library.Workflow.Perform(Context.Workflow, propertyBag);

			if (propertyBag.HasErrors)
			{
				WorkflowError firstError = propertyBag.Errors[0];
				Library.Utils.FlashMessage(firstError.Message, Context.Workflow.Name);
				Exit();
			}

			// Get the entity from the workflow

			IEntity newEntity = propertyBag.GetEntities(Context.EntityType).SingleOrDefault();

			if (newEntity == null)
			{
				Exit();
				return;
			}

			// Publish the user interface passed in the task parameters.

			Form form = FormFactory.CreateForm(Context.TaskParameters[0], newEntity);
			m_SetAsModified = true;

			form.Created += FormCreated;
			form.Loaded += FormLoaded;
			form.Show(Context.MenuWindowStyle);
			form.Closing += MainFormClosing;
		}

		#endregion
	}
}
