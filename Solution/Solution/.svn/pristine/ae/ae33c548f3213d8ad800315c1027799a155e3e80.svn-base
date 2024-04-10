using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core.Definition;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Server;
using Thermo.Informatics.Common.Forms.Core;
using Thermo.SampleManager.Tasks.BackgroundTasks;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Versions Page
	/// </summary>
	[SampleManagerPage("VersionPage")]
	public class VersionPage : PageBase
	{
		#region Constants

		private const string ApprovalSingleActiveVersion = "APPROVAL_SINGLE_ACTIVE_VERSION";

		#endregion

		#region Member Variables

		private bool m_Setup;
		private bool m_ReadOnly;
		private ISchemaTable m_Table;
		private IEntity m_MainEntity;
		private string m_ActiveStartPropertyName;
		private string m_ActiveEndPropertyName;
		private string m_ApprovalStatusPropertyName;
		private string m_ActiveFlagPropertyName;

		#endregion

		#region Overrides

		/// <summary>
		/// Page Selected is called once the user selects this page and therefore will not
		/// effect property sheet loading. Labour intensive code should be place here or
		/// on a background task.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="T:Thermo.SampleManager.Library.RuntimeFormsEventArgs"/> instance containing the event data.</param>
		public override void PageSelected(object sender, RuntimeFormsEventArgs e)
		{
			base.PageSelected(sender, e);

			if (m_Setup) return;
			// Setup links to the version fields if they exist

			m_Table = Library.Schema.Tables[Context.EntityType];

			if (!m_ReadOnly)
			{
				if (SetupPageControls())
				{
					SetupVersionHistory();
				}
				else
				{
					ClearTab();
				}

				m_Setup = true;

				UpdateVersionHistoryGrid();
				UpdateActiveStatus();
			}
			else
			{
				TextEdit versionNumberEdit = (TextEdit) MainForm.Controls[FormVersionPage.VersionNumberControlName];
				DateEdit versionStartEdit = (DateEdit) MainForm.Controls[FormVersionPage.VersionActiveFromControlName];
				DateEdit versionEndEdit = (DateEdit) MainForm.Controls[FormVersionPage.VersionActiveToControlName];
				TextEdit versionStatusEdit = (TextEdit) MainForm.Controls[FormVersionPage.ActiveControlName];

				versionNumberEdit.Text = MainForm.Entity.Get(m_Table.VersionField.Name).ToString();
				versionStartEdit.Date = (NullableDateTime) MainForm.Entity.Get(m_Table.ActiveStartDateField.Name);
				versionEndEdit.Date = (NullableDateTime) MainForm.Entity.Get(m_Table.ActiveEndDateField.Name);
				if ((bool) MainForm.Entity.Get(m_Table.ActiveFlagField.Name))
				{
					versionStatusEdit.Text = "Active";
				}
				else
				{
					versionStatusEdit.Text = "Not Active";

				}
				SetupVersionHistory();
			}
		}

		/// <summary>
		/// Page Interface Loaded called after corresponding Form event. Multiple pages
		/// placing code here will result slower property sheet loading.
		/// </summary>
		/// <param name="formsUserInterface"></param>
		public override void PageInterfaceLoaded(IFormsUserInterface formsUserInterface)
		{
			base.PageInterfaceLoaded(formsUserInterface);
			if (formsUserInterface.EntityType == null)
			{
				m_ReadOnly = true;
			}
			else
			{
				m_ReadOnly = false;
			}
		}

		#endregion

		#region Setup Controls

		/// <summary>
		/// Setup the page controls.
		/// </summary>
		/// <returns></returns>
		private bool SetupPageControls()
		{

			if (m_Table == null) return false;
			if (m_Table.VersionField == null) return false;

			m_MainEntity = MainForm.Entity;
			m_MainEntity.PropertyChanged += MainEntityPropertyChanged;

			// Find the controls from the controls array

			TextEdit versionNumberEdit = (TextEdit) MainForm.Controls[FormVersionPage.VersionNumberControlName];
			DateEdit versionStartEdit = (DateEdit) MainForm.Controls[FormVersionPage.VersionActiveFromControlName];
			DateEdit versionEndEdit = (DateEdit) MainForm.Controls[FormVersionPage.VersionActiveToControlName];

			// Enable the controls if the fields exist

			versionStartEdit.Enabled = (m_Table.ActiveStartDateField != null);

			if (SingleActiveVersion)
			{
				versionEndEdit.Enabled = false;
				versionEndEdit.Visible = false;
			}
			else if (m_Table.ActiveEndDateField == null)
				versionEndEdit.Enabled = false;
			else
				versionEndEdit.Enabled = true;

			// If the fields exist then bind the controls to the fields

			versionNumberEdit.BindToProperty(m_Table.VersionField.Name);

			if (m_Table.ActiveStartDateField != null)
			{
				m_ActiveStartPropertyName = EntityType.DeducePropertyName(m_Table.Name, m_Table.ActiveStartDateField.Name);
				versionStartEdit.BindToProperty(m_Table.ActiveStartDateField.Name);
				versionStartEdit.DateTimeChanged += UpdateActive;

				if (SingleActiveVersion)
				{
					if (!m_MainEntity.IsNew() && ! IsPending())
					{
						versionStartEdit.ReadOnly = true;
					}
				}
			}

			if ((m_Table.ActiveEndDateField != null) && (!SingleActiveVersion))
			{
				m_ActiveEndPropertyName = EntityType.DeducePropertyName(m_Table.Name, m_Table.ActiveEndDateField.Name);
				versionEndEdit.BindToProperty(m_Table.ActiveEndDateField.Name);
				versionEndEdit.DateTimeChanged += UpdateActive;
			}

			if (m_Table.ActiveFlagField != null)
			{
				m_ActiveFlagPropertyName = EntityType.DeducePropertyName(m_Table.Name, m_Table.ActiveFlagField.Name);
			}

			if (m_Table.ApprovalStatusField != null)
			{
				m_ApprovalStatusPropertyName = EntityType.DeducePropertyName(m_Table.Name, m_Table.ApprovalStatusField.Name);
			}

			return true;
		}

		/// <summary>
		/// Property Changed Event Handler
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PropertyEventArgs"/> instance containing the event data.</param>
		private void MainEntityPropertyChanged(object sender, PropertyEventArgs e)
		{
			if (e.PropertyName == "Name" || e.PropertyName == m_ActiveStartPropertyName ||
			    e.PropertyName == m_ActiveEndPropertyName || e.PropertyName == m_ApprovalStatusPropertyName)
			{
				UpdateVersionHistoryGrid();
			}

			if (e.PropertyName == m_ActiveFlagPropertyName)
			{
				UpdateActiveStatus();
			}
		}

		/// <summary>
		/// Update Version History Grid
		/// </summary>
		private void UpdateVersionHistoryGrid()
		{
			ExplorerGrid grid = (ExplorerGrid) MainForm.Controls[FormVersionPage.VersionHistoryControlName];
			IEntityCollection allVersions = EntityManager.SelectAllVersions(MainForm.Entity);
			grid.Browse.Republish(allVersions);
		}

		/// <summary>
		/// Updates the active flag
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="DateTimeChangedEventArgs"/> instance containing the event data.</param>
		private void UpdateActive(object sender, DateTimeChangedEventArgs e)
		{
			MainForm.Entity.Set(m_Table.ActiveFlagField.Name, MainForm.Entity.CheckActive());
		}

		/// <summary>
		/// Update Active Status
		/// </summary>
		private void UpdateActiveStatus()
		{
			TextEdit label = (TextEdit) MainForm.Controls[FormVersionPage.ActiveControlName];
			bool active = m_MainEntity.CheckActive();

			if (!active)
			{
				label.Text = Library.Message.GetMessage("ControlMessages", "ActiveStatusInactive");
			}
			else if (!IsActive())
			{
				label.Text = Library.Message.GetMessage("ControlMessages", "ActiveStatusInactiveFlag");
			}
			else
			{
				label.Text = Library.Message.GetMessage("ControlMessages", "ActiveStatusActive");
			}

			// Check for the Approval Status.

			if (!IsApproved())
			{
				if (active)
				{
					label.Text = Library.Message.GetMessage("ControlMessages", "ActiveStatusActiveUnapproved");
				}
				else
				{
					label.Text = Library.Message.GetMessage("ControlMessages", "ActiveStatusInactiveUnapproved");
				}
			}

			if (IsPending() && !active)
			{
				label.Text = Library.Message.GetMessage("ControlMessages", "ActiveStatusPending");
			}
		}

		/// <summary>
		/// Is the record at approval status approved?
		/// </summary>
		/// <returns></returns>
		private bool IsApproved()
		{
			if (m_ApprovalStatusPropertyName == null) return true;
			IEntity approvalStatus = MainForm.Entity.GetEntity(m_ApprovalStatusPropertyName);

			if (m_MainEntity.EntityType == ActiveRecordTask.StudyTableName)
			{
				// Stability Specific Approval Checking

				return ActiveRecordTask.StudyStatusToActivate.Contains(approvalStatus.Name);
			}

			return (approvalStatus.Name == PhraseApprStat.PhraseIdA);
		}

		/// <summary>
		/// Is the record at pending status?
		/// </summary>
		/// <returns></returns>
		private bool IsPending()
		{
			if (m_Table.ApprovalStatusField == null) return false;
			IEntity status = m_MainEntity.GetEntity(m_Table.ApprovalStatusField.Name);
			if (!BaseEntity.IsValid(status)) return false;
			return (status.Name == PhraseApprStat.PhraseIdP);
		}

		/// <summary>
		/// Is the record marked as active
		/// </summary>
		/// <returns></returns>
		private bool IsActive()
		{
			if (m_Table.ActiveFlagField == null) return false;
			return m_MainEntity.GetBoolean(m_Table.ActiveFlagField.Name);
		}

		/// <summary>
		/// Setup the version history.
		/// </summary>
		private void SetupVersionHistory()
		{
			IEntityCollection allVersions = EntityManager.SelectAllVersions(MainForm.Entity);

			ExplorerGrid grid = (ExplorerGrid) MainForm.Controls[FormVersionPage.VersionHistoryControlName];

			EntityBrowseColumnCollection columns = new EntityBrowseColumnCollection();

			AddColumn(columns, m_Table.BrowseField, 100);
			AddColumn(columns, m_Table.VersionField, 50);
			AddColumn(columns, m_Table.ActiveFlagField, 30);
			AddColumn(columns, m_Table.ActiveStartDateField, 75);

			if (!SingleActiveVersion)
			{
				AddColumn(columns, m_Table.ActiveEndDateField, 75);
			}

			grid.Browse = BrowseFactory.CreateEntityBrowse(allVersions, columns);
			grid.DoubleClick += GridDoubleClick;
		}

		/// <summary>
		/// Handle the double click event on the grid.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.EntityEventArgs"/> instance containing the event data.</param>
		private static void GridDoubleClick(object sender, EntityEventArgs e)
		{
			ExplorerGrid grid = (ExplorerGrid) sender;
			grid.DoDefaultAction();
		}

		/// <summary>
		/// AddColumn
		/// </summary>
		/// <param name="columns"></param>
		/// <param name="field"></param>
		/// <param name="width"></param>
		private static void AddColumn(EntityBrowseColumnCollection columns, ISchemaField field, int width)
		{
			if (field == null) return;

			EntityBrowseColumnDefinition column = new EntityBrowseColumnDefinition {Column = field.Name, Width = width};
			columns.Add(column);
		}

		/// <summary>
		/// AddColumn
		/// </summary>
		private void ClearTab()
		{
			((VisualControl) MainForm.Controls[FormVersionPage.GroupBoxVersionControlLabel]).Visible = false;
			((VisualControl) MainForm.Controls[FormVersionPage.GroupBoxVersionControlName]).Visible = false;
			((VisualControl) MainForm.Controls[FormVersionPage.UnsupportedPanelControlName]).Visible = true;
		}

		/// <summary>
		/// Method to decide if page should be added. False will not add the page to the property sheet
		/// or call the extension page methods.
		/// </summary>
		/// <param name="entityType">Type of the entity.</param>
		/// <returns></returns>
		public override bool AddPage(string entityType)
		{
			ISchemaTable table = Schema.Current.Tables[entityType];

			if (table.VersionField == null)
			{
				m_ParentSampleManagerTask.Library.Utils.ShowAlert(Library.Message.GetMessage("ControlMessages", "VersionPageError"));
				return false;
			}

			return true;
		}

		/// <summary>
		/// Returns true if single active version mode is enabled.
		/// </summary>
		/// <returns></returns>
		private bool SingleActiveVersion
		{
			get
			{
				if (Library.Environment.CheckGlobalExists(ApprovalSingleActiveVersion))
					return Library.Environment.GetGlobalBoolean(ApprovalSingleActiveVersion);

				return false;
			}
		}

		#endregion
	}
}
