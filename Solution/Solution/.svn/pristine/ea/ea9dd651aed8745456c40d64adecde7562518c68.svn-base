using System.Linq;
using System.Text;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Library.ImportExport;
using Thermo.SampleManager.Library.ObjectModel;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.ObjectModel.Import_Helpers;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks
{
	[SampleManagerTask("SelectEntityTask")]
	internal class SelectEntityTask : DefaultFormTask
	{
		private static string PreviousEntity = "";
		private IEntity m_CurrentCriteria;
		private FormSelectEntity m_Form;
		private IEntityCollection m_SelectedEntityCollection;
		private bool m_UpdatingGrid;
		private bool m_exportAll = false;
		private string m_SelectedEntityName = "";
		//private bool m_Loaded;

		/// <summary>
		///     Called when the <see cref="DefaultFormTask.MainForm" /> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			base.MainFormLoaded();

			m_Form = (FormSelectEntity) MainForm;

			SetupImportableEntityTypes();
			SetupDropdownEvents();
			SetupExportContextMenu();
			SetupSelectionBehaviour();

			m_Form.EntityDropDownSelection.Text = PreviousEntity;
			m_Form.exportButton.Click += (s, e) => { ExportItems(); };
			m_Form.refreshButton.Click += (s, e) => { if(m_SelectedEntityName!=""){RefreshUI();} };
			m_Form.filterCriteriaDropdown.Enabled = false;
			m_Form.exportAllButton.Click += (s, e) =>
			{
				m_exportAll = true;
				m_Form.sourceGrid.SelectAll();
				
			};
		}

		/// <summary>
		/// Setups the importable entity types.
		/// </summary>
		private void SetupImportableEntityTypes()
		{
			foreach (var importableEntityType in ImportExportUtilities.ImportableEntityTypes)
			{

				m_Form.importableEntityStringBrowse.AddItem(EntityNameToNiceName(importableEntityType.Key));
			}
		}

		/// <summary>
		///     Setups the dropdown events.
		/// </summary>
		private void SetupDropdownEvents()
		{
			m_Form.EntityDropDownSelection.StringChanged += (s, e) =>
			{
				if (!string.IsNullOrEmpty(e.Text))
				{
					
					m_SelectedEntityName= NiceNameToEntityName(m_Form.EntityDropDownSelection.Text);

					var table = Library.Schema.Tables[m_SelectedEntityName];

					if (!m_UpdatingGrid)
					{
						m_UpdatingGrid = true;

						m_Form.filterCriteriaDropdown.Entity = null;

						//load criteria
						m_Form.filterCriteriaDropdown.Enabled = true;
						m_CurrentCriteria = null;
						RefreshUI();

						PreviousEntity = e.Text;
						m_UpdatingGrid = false;
					}
				}
				
			};

			m_Form.filterCriteriaDropdown.EntityChanged += (s, e) =>
			{
					if (e.Entity != null)
					{
						if (!m_UpdatingGrid)
						{
							if (!Equals(e.Entity, m_CurrentCriteria))
							{
								m_UpdatingGrid = true;

								m_CurrentCriteria = e.Entity;
								RefreshUI();
								m_UpdatingGrid = false;
							}
						}
					}
					else
					{
						if (m_CurrentCriteria != null)
						{
							m_UpdatingGrid = true;
							m_CurrentCriteria = null;
							RefreshUI();
							m_UpdatingGrid = false;
						}
					}
				
			};


		}

		/// <summary>
		/// Refreshes the UI.
		/// </summary>
		private void RefreshUI()
		{
			m_exportAll = false;

			PublishCriteria();
			UpdateEntityGrid();
		}

		/// <summary>
		/// Publishes the criteria.
		/// </summary>
		private void PublishCriteria()
		{
			var query = EntityManager.CreateQuery("CRITERIA_SAVED");
			query.AddEquals("TABLE_NAME", m_SelectedEntityName);
			
			m_Form.filterCriteriaBrowse.Republish(EntityManager.Select(query));
		}

		/// <summary>
		///     Setups the selection behaviour.
		/// </summary>
		private void SetupSelectionBehaviour()
		{
			m_Form.sourceGrid.SelectionChanged += (s, e) =>
			{
				m_SelectedEntityCollection = e.Selection;
				if (m_exportAll)
				{
					m_exportAll = false;
					ExportItems();
				}
			};
		}

		/// <summary>
		///     Setups the export context menu.
		/// </summary>
		private void SetupExportContextMenu()
		{
			var exportMenuItem = new ContextMenuItem(Library.Message.GetMessage("LaboratoryMessages","Export"), "ENTITY_EXPORT", true);
			exportMenuItem.ItemClicked += (s, e) => { ExportItems(); };
			m_Form.sourceGrid.ContextMenu.AddItem(exportMenuItem);
		}

		/// <summary>
		///     Updates the entity grid.
		/// </summary>
		private void UpdateEntityGrid()
		{
			var q = EntityManager.CreateQuery(m_SelectedEntityName);
			m_Form.sourceGrid.Browse = BrowseFactory.CreateEntityBrowse(q);

			if (m_CurrentCriteria != null)
			{
				m_Form.sourceGrid.Browse.SetCriteria(null);
				m_Form.sourceGrid.Browse.SetCriteria((CriteriaSavedInternal)m_CurrentCriteria);
			}
			else
			{
				m_Form.sourceGrid.Browse.SetCriteria(null);
			}
		}

		/// <summary>
		///     Exports the items.
		/// </summary>
		private void ExportItems()
		{
			if (m_SelectedEntityCollection == null || m_SelectedEntityCollection.Count == 0)
			{
				Library.Utils.FlashMessage(Library.Message.GetMessage("LaboratoryMessages", "ExportNoEntitySelected"), "");
				return;
			}

			
			ExplorerFolderCriteriaMessage();

			Library.Task.CreateTask(35254, m_SelectedEntityCollection);
		}

		/// <summary>
		/// Message user about criteria saved on explorer folders
		/// </summary>
		private void ExplorerFolderCriteriaMessage()
		{
			var message = new StringBuilder();

			foreach (var entity in m_SelectedEntityCollection)
			{
				if (entity is ExplorerFolder)
				{
					var criteriaSaved = ((ExplorerFolder) entity).CriteriaSavedIdentity;
					if (!string.IsNullOrEmpty(criteriaSaved))
					{
						message.AppendLine(string.Format(Library.Message.GetMessage("LaboratoryMessages", "Export_ExplorerFolderCriteriaSavedExport"), criteriaSaved));
					}
				}
			}
			if (message.Length>0)
			{
				Library.Utils.FlashMessage(message.ToString(), "");
			}
		}

		/// <summary>
		/// Entities the name of the name to nice.
		/// </summary>
		/// <param name="name">The name.</param>
		private string EntityNameToNiceName(string name)
		{
			return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.ToLower().Replace("_"," "));
		}
		
		/// <summary>
		/// Entities the name of the name to nice.
		/// </summary>
		/// <param name="name">The name.</param>
		private string NiceNameToEntityName(string name)
		{
			return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToUpper(name.Replace(" ", "_"));
		}

	
	}
}