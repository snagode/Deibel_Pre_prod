using System;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls.Browse;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Task to support the browse window
	/// </summary>
	[SampleManagerTask("BrowseTask")]
	public class BrowseTask : SampleManagerTask
	{
		#region Member Variables

		private FormBrowse m_Form;
		private string m_EntityType;
		private string m_Criteria;

		#endregion

		#region Setup

		/// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			if (Context.TaskParameters.Length == 0) Exit();

			m_EntityType = Context.TaskParameters[0];

			if (Context.TaskParameters.Length > 1)
			{
				m_Criteria = Context.TaskParameters[1];
			}

			m_Form = (FormBrowse) FormFactory.CreateForm("Browse");

			m_Form.Created += MainFormCreated;
			m_Form.Loaded += MainFormLoaded;

			m_Form.Show();
		}

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been loaded.
		/// </summary>
		protected void MainFormCreated(object sender, EventArgs e)
		{
			m_Form.BrowseGrid.DoubleClick += BrowseGridDoubleClick;

			Library.Environment.ExplorerRefresh += EnvironmentExplorerRefresh;
			Library.Environment.ExplorerHideRemovedChanged += EnvironmentExplorerRefresh;
			Library.Environment.ExplorerShowVersionsChanged += EnvironmentExplorerRefresh;
		}

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been loaded.
		/// </summary>
		protected void MainFormLoaded(object sender, EventArgs e)
		{
			m_Form.PromptTableName.TableName = m_EntityType;
			BuildGrid();
		}

		private void BuildGrid()
		{
			m_Form.Title = Library.Message.GetMessage("ExplorerMessages", "BrowseTitle", Library.Utils.MakeName(m_EntityType));
			m_Form.IconName = Library.Utils.GetDefaultIcon(m_EntityType);

			EntityManager.SetCacheStatic(false);
	
			IQuery query = EntityManager.CreateQuery(m_EntityType);

			EntityBrowse browse = BrowseFactory.CreateEntityBrowse(query);
			browse.AddColumnsFromTableDefaults();

			if (!string.IsNullOrEmpty(m_Criteria))
			{
				IEntity criteria = EntityManager.Select(TableNames.CriteriaSaved, new Identity(m_EntityType, m_Criteria));

				if (criteria != null)
				{
					browse.SetCriteria((CriteriaSaved) criteria);
				}
			}

			browse.ShowVersions = Library.Environment.ShowVersions ? TriState.Yes : TriState.No;
			browse.HideRemoved = Library.Environment.HideRemoved ? TriState.Yes : TriState.No;

			m_Form.BrowseGrid.Browse = browse;
		}

		private void EnvironmentExplorerRefresh(object sender, EventArgs e)
		{
			m_Form.BrowseGrid.Browse.ShowVersions = Library.Environment.ShowVersions ? TriState.Yes : TriState.No;
			m_Form.BrowseGrid.Browse.HideRemoved = Library.Environment.HideRemoved ? TriState.Yes : TriState.No;

			m_Form.BrowseGrid.Refresh();
		}

		/// <summary>
		/// Browses the grid double click.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.EntityEventArgs"/> instance containing the event data.</param>
		private void BrowseGridDoubleClick(object sender, Library.ClientControls.EntityEventArgs e)
		{
			m_Form.BrowseGrid.DoDefaultAction();
		}

		#endregion
	}
}