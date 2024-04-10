using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Thermo.Framework.Server;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Search Description
	/// </summary>
	[DataContract(Name="searchDescription")]
	public class SearchDescription : MobileObject
	{
		#region Properties

		/// <summary>
		/// Gets or sets the id.
		/// </summary>
		/// <value>
		/// The id.
		/// </value>
		[DataMember(Name = "id")]
		public string Id { get; set; }

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		[DataMember(Name = "name")]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the description
		/// </summary>
		/// <value>
		/// The description
		/// </value>
		[DataMember(Name = "description")]
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the URI.
		/// </summary>
		/// <value>
		/// The URI.
		/// </value>
		[DataMember(Name = "dataUri")]
		public Uri DataUri { get; set; }

		/// <summary>
		/// Gets or sets the columns.
		/// </summary>
		/// <value>
		/// The columns.
		/// </value>
		[DataMember(Name = "columns")]
		public List<SearchColumn> Columns { get; set; }

		/// <summary>
		/// Gets or sets the prompts.
		/// </summary>
		/// <value>
		/// The prompts.
		/// </value>
		[DataMember(Name = "prompts")]
		public List<Prompt> Prompts { get; set; }

		/// <summary>
		/// Gets or sets the hidden state
		/// </summary>
		/// <value>
		/// Hidden or visible
		/// </value>
		[DataMember(Name = "hidden")]
		public bool Hidden { get; set; }

		#endregion

		#region Load from RMB

		/// <summary>
		/// Loads the specified RMB.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <param name="rmb">The RMB.</param>
		/// <returns></returns>
		public bool LoadRmb(StandardLibrary library, ExplorerRmb rmb)
		{
			// Check the inputs

			ExplorerFolder folder = rmb.Folder as ExplorerFolder;
			if (!BaseEntity.IsValid(folder) || folder == null) return false;
			ExplorerCabinet cabinet = folder.Cabinet as ExplorerCabinet;
			if (!BaseEntity.IsValid(cabinet) || cabinet == null) return false;

			// Validate access.

			if (!library.Security.CheckPrivilege(rmb.Menuproc.ProcedureNum)) return false;

			// Populate the values

			Id = folder.FolderNumber.ToString().Trim();
			Name = GetLocalizedString(library, folder.Name);
			Description = GetLocalizedString(library, folder.Description);
			DataUri = MakeLink("/mobile/searches/{0}/{1}/{2}/data", cabinet.Identity, rmb.Menuproc.ProcedureNum, Id);

			// Prompts

			Prompts = Prompt.LoadCriteria(library, folder.CriteriaSaved);

			return true;
		}

		/// <summary>
		/// Loads the columns.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <param name="node">The node.</param>
		public void LoadColumns(StandardLibrary library, ExplorerDataNode node)
		{
			if (node == null)
			{
				Columns = null;
				return;
			}

			Columns = new List<SearchColumn>();

			foreach (var explorerColumn in node.Columns)
			{
				var col = SearchColumn.LoadColumn(library, node, explorerColumn);
				Columns.Add(col);
			}
		}

		#endregion

		#region Utility Functions

		/// <summary>
		/// Loads the search descriptions
		/// </summary>
		public static List<SearchDescription> Load(StandardLibrary library, IEntityManager entityManager, string function, string cabinetName)
		{
			var searchDescriptions = new List<SearchDescription>();
			var cabinet = (ExplorerCabinet)entityManager.Select(ExplorerCabinetBase.EntityName, cabinetName);

			// Ensure the cabinet is valid

			if (cabinet == null || !cabinet.IsValid()) return null;
			if (cabinet.Removeflag) return null;

			// Function numbers are integers

			int functionId;
			if (!int.TryParse(function, out functionId)) return null;

			// Iterate through building a flat list of available folders for the specified rmb

			foreach (ExplorerFolder folder in cabinet.Folders)
			{
				foreach (ExplorerRmb rmb in folder.Rmbs)
				{
					if (!BaseEntity.IsValid(rmb.Menuproc)) continue;
					if (rmb.Menuproc.ProcedureNum != functionId) continue;
					if (folder.FolderNumber == null) continue;

					var desc = new SearchDescription();

					if (desc.LoadRmb(library, rmb))
					{
						searchDescriptions.Add(desc);
					}
				}
			}

			return searchDescriptions;
		}

		/// <summary>
		/// Loads the default.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <param name="entityManager">The entity manager.</param>
		/// <param name="site">The site.</param>
		/// <param name="function">The function.</param>
		/// <returns></returns>
		public static SearchDescription LoadDefault(StandardLibrary library, IEntityManager entityManager, IServerTaskSite site, string function)
		{
			MasterMenuBase menu = (MasterMenuBase)entityManager.Select(MasterMenuBase.EntityName, function);
			if (!BaseEntity.IsValid(menu)) return null;

			var folder = new SearchDescription();

			var service = library.GetService<IExplorerCacheService>();
			var defaults = service.GetDefaultsFolder(menu.TableName);
			var node = new ExplorerDataNode(defaults);
			node.TaskSite = site;
			folder.LoadColumns(library, node);
			folder.Id = "DEFAULT";

			folder.DataUri = MakeLink("/mobile/searches/default/{0}/data", function);
			folder.Hidden = true;

			return folder;
		}

		#endregion
	}
}
