using System.ComponentModel;
using System.ServiceModel.Web;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;
using Thermo.SampleManager.WebApiTasks.Mobile.Data;

namespace Thermo.SampleManager.WebApiTasks.Mobile
{
	/// <summary>
	/// Search Task
	/// </summary>
	[SampleManagerWebApi("mobile.browses")]
	public class BrowseTask : SampleManagerWebApiTask
	{
		#region Properties

		/// <summary>
		/// Gets or sets the Explorer cache.
		/// </summary>
		/// <value>
		/// The cache.
		/// </value>
		protected IExplorerCacheService ExplorerCache { get; set; }

		/// <summary>
		/// Gets or sets the criteria service.
		/// </summary>
		/// <value>
		/// The criteria service.
		/// </value>
		protected ICriteriaTaskService CriteriaService { get; set; }

		#endregion

		#region Overrides

		/// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			ExplorerCache = Library.GetService<IExplorerCacheService>();
			CriteriaService = Library.GetService<ICriteriaTaskService>();
			base.SetupTask();
		}

		#endregion

		#region Browse Functions

		/// <summary>
		/// Browse - Regular
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="page">The page.</param>
		/// <param name="size">The size.</param>
		/// <param name="filter">The filter.</param>
		/// <param name="criteria">The criteria.</param>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/browses/{entity}?page={page}&pageSize={size}&filter={filter}&criteria={criteria}", Method = "GET")]
		[Description("Browse for the specified entity with optional paging and find filter")]
		public SearchData BrowseDataGet(string entity, int page, int size, string filter, string criteria)
		{
			var response = GetBrowseData(entity, page, size, filter, criteria);
			if (response == null) return null;
			response.SetUri(string.Format("mobile/browses/{0}", entity));
			return response;
		}

		/// <summary>
		/// Browse - Hierarchy
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="property">The property.</param>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/browses/{entity}/{property}", Method = "GET")]
		[Description("Hierarchy Browse for the specified entity")]
		public SearchData BrowseDataGetHierarchy(string entity, string property)
		{
			var response = GetHierarchyData(entity, property, null);
			if (response == null) return null;
			response.SetUri(string.Format("mobile/browses/{0}/{1}", entity, property));
			return response;
		}

		/// <summary>
		/// Browse - Hierarchy Child
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="property">The property.</param>
		/// <param name="parent">The child.</param>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/browses/{entity}/{property}/{parent}", Method = "GET")]
		[Description("Child Hierarchy Browse for the specified entity parent")]
		public SearchData BrowseDataGetHierarchyChild(string entity, string property, string parent)
		{
			var response = GetHierarchyData(entity, property, parent);
			if (response == null) return null;
			response.SetUri(string.Format("mobile/browses/{0}/{1}", entity, property));
			return response;
		}

		/// <summary>
		/// Gets the search data.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="page">The page.</param>
		/// <param name="size">The size.</param>
		/// <param name="filter">The filter value.</param>
		/// <param name="criteria">The criteria.</param>
		/// <returns></returns>
		private SearchData GetBrowseData(string entity, int page, int size, string filter, string criteria)
		{
			// Return appropriate sizes

			if (page == 0) page = 1;
			if (size == 0) size = 100;

			if (!string.IsNullOrEmpty(criteria)) criteria = criteria.ToUpperInvariant();

			// Return back the data along with the parameters

			var response = new SearchData(initialise:true);
			response.Page = page;
			response.PageSize = size;
			response.FindFilter = filter;
			response.Criteria = criteria;

			entity = entity.ToUpperInvariant();

			// Load the response

			ExplorerDataNode node = GetDefaultsNode(entity, criteria);
			if (node == null) return null;
			node.TaskSite = TaskSite;

			// Apply filtering

			if (!string.IsNullOrWhiteSpace(filter))
			{
				node.ApplyFindFilter(filter);
			}

			response.Count = node.DisplayCount;
			response.LoadColumns(Library, node);
			response.LoadData(node);

			return response;
		}

		/// <summary>
		/// Gets the search data.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="hierarchy">The hierarchy.</param>
		/// <param name="node">The node.</param>
		/// <returns></returns>
		private SearchData GetHierarchyData(string entity, string hierarchy, string node)
		{
			// Return back the data along with the parameters

			var response = new SearchData(initialise:true);
			entity = entity.ToUpperInvariant();

			// Response for normal browse

			EntityHierarchyNode treeNode = GetHierarchyNode(entity, hierarchy);

			if (treeNode == null) return null;

			response.LoadColumns(Library, treeNode);
			response.LoadHierarchy(EntityManager, treeNode, hierarchy, node);

			// Clear any empty data.

			if (response.Rows != null && response.Rows.Count == 0)
			{
				response.Rows = null;
			}

			return response;
		}

		#endregion

		#region Utility Functions

		/// <summary>
		/// Gets the defaults explorer node.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="criteriaName">Name of the criteria.</param>
		/// <returns></returns>
		protected ExplorerBrowseNode GetDefaultsNode(string entity, string criteriaName)
		{
			entity = entity.ToUpperInvariant();
			if (!Library.Schema.Tables.Contains(entity)) return null;

			var defaults = ExplorerCache.GetDefaultsFolder(entity);

			var defaultQuery = (Query)EntityManager.CreateQuery(entity);
			defaultQuery.AddDefaultOrder();
			defaultQuery.AddAssignableFlagCheck(true);

			// Get the base criteria

			CriteriaSaved criteria = null;

			if (!string.IsNullOrWhiteSpace(criteriaName))
			{
				criteria = (CriteriaSaved)EntityManager.Select(CriteriaSavedBase.EntityName, new Identity(entity, criteriaName));
				if (!BaseEntity.IsValid(criteria)) criteria = null;
			}

			if (criteria == null || !BaseEntity.IsValid(criteria)) return new ExplorerBrowseNode(defaults, defaultQuery, null);

			// Zap any prompt conditions.

			criteria.PromptConditions.RemoveAll();

			// Form the criteria query

			var query = (Query) CriteriaService.GetCriteriaQuery(criteria);
			return new ExplorerBrowseNode(defaults, defaultQuery, query);
		}

		/// <summary>
		/// Gets the hierarchy node.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="property">The property.</param>
		/// <returns></returns>
		protected EntityHierarchyNode GetHierarchyNode(string entity, string property)
		{
			entity = entity.ToUpperInvariant();
			var defaults = ExplorerCache.GetDefaultsFolder(entity);

			var defaultQuery = (Query)EntityManager.CreateQuery(entity);
			defaultQuery.AddDefaultOrder();

			EntityHierarchyNode node = new EntityHierarchyNode(TaskSite, defaults, (EntityManager)EntityManager, property);
			return node;
		}

		#endregion
	}
}