using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.ServiceModel.Web;
using Newtonsoft.Json.Linq;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Utilities;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;
using Thermo.SampleManager.WebApiTasks.Mobile.Data;
using Object = Thermo.SampleManager.WebApiTasks.Data.Object;

namespace Thermo.SampleManager.WebApiTasks.Mobile
{
	/// <summary>
	/// Search Task
	/// </summary>
	[SampleManagerWebApi("mobile.searches")]
	public class SearchTask : SampleManagerWebApiTask
	{
		#region Constants

		private const string OperatorEqual = "1";
		private const string OperatorGreaterEqual = "2";
		private const string OperatorLessEqual = "3";
		private const string OperatorNotEqual = "4";
		private const string OperatorLessThan = "5";
		private const string OperatorGreaterThan = "6";
		private const string OperatorLike = "7";
		private const string OperatorNotLike = "8";
		private const string OperatorIn = "10";
		private const string OperatorNotIn = "11";

		private const int DefaultPageNumber = 1;
		private const int DefaultPageSize = 100;

		#endregion

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

		#region Search Information

		/// <summary>
		/// Search Descriptions for Functions
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/searches/{cabinet}/{function}", Method = "GET")]
		[Description("Mobile Folders for the specified cabinet where they contain the function number")]
		public SearchResponse Searches(string cabinet, string function)
		{
			// Force refresh the cache - added in response to specific demo scenarios.

			ExplorerCache.Clear();

			// Load the response information up.

			var response = new SearchResponse(initialise: true);

			// Folder Defaults

			var defaults = Data.SearchDescription.LoadDefault(Library, EntityManager, TaskSite, function);
			if (defaults != null)
			{
				response.SearchDescriptions.Add(defaults);
			}

			// Specific Folder Descriptions

			cabinet = cabinet.ToUpperInvariant();

			var explorerCabinet = (ExplorerCabinet) EntityManager.Select(ExplorerCabinetBase.EntityName, new Identity(cabinet));
			if (!BaseEntity.IsValid(explorerCabinet)) return null;

			foreach (ExplorerFolder folder in explorerCabinet.Folders)
			{
				var desc = SearchDescription(cabinet, function, folder.FolderNumber);
				if (desc == null) continue;
				response.SearchDescriptions.Add(desc);
			}

			if (response.SearchDescriptions == null || response.SearchDescriptions.Count == 0) return null;
			return response;
		}

		/// <summary>
		/// Search Description for Function/Folder
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/searches/{cabinet}/{function}/{folder}", Method = "GET")]
		[Description("Mobile Folder details for the specified cabinet/folder/function")]
		public SearchDescription SearchDescription(string cabinet, string function, string folder)
		{
			int functionId;
			int folderId;

			if (!int.TryParse(function, out functionId)) return null;
			if (!int.TryParse(folder, out folderId)) return null;
			cabinet = cabinet.ToUpperInvariant();

			var explorerFolder = (ExplorerFolder) EntityManager.Select(ExplorerFolderBase.EntityName, new Identity(cabinet, folderId));
			if (!BaseEntity.IsValid(explorerFolder)) return null;

			foreach (ExplorerRmb rmb in explorerFolder.Rmbs)
			{
				if (rmb.Menuproc == null) continue;
				if (rmb.Menuproc.ProcedureNum == functionId)
				{
					var response = new SearchDescription();
					if (!response.LoadRmb(Library, rmb)) return null;

					// Column Definitions come from the Explorer

					var node = GetExplorerNode(cabinet, function, folder, null);
					response.LoadColumns(Library, node);
					return response;
				}
			}

			return null;
		}

		#endregion

		#region Basic Feature Support

		/// <summary>
		/// Searches - Move Function
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/searches/movesamples", Method = "GET")]
		[Description("Mobile Folders in the Mobile cabinet containing the Move Samples (11004) function.")]
		public SearchResponse SearchesMove()
		{
			return Searches(Session.DefaultCabinetName, FunctionBasicMove.FunctionMoveNumber.ToString(CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Searches - Receive Function
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/searches/receivesamples", Method = "GET")]
		[Description("Mobile Folders in the Mobile cabinet containing the Receive Samples (11008) function.")]
		public SearchResponse SearchesRecieve()
		{
			return Searches(Session.DefaultCabinetName, FunctionBasicReceive.FunctionReceiveNumber.ToString(CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Searches - Result Entry Function
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/searches/resultentry", Method = "GET")]
		[Description("Mobile Folders in the Mobile cabinet containing the Result Entry (15638) function.")]
		public SearchResponse SearchesResultEntry()
		{
			return Searches(Session.DefaultCabinetName, FunctionResultEntry.FunctionResultEntryNumber.ToString(CultureInfo.InvariantCulture));
		}

		#endregion

		#region Search Results

		/// <summary>
		/// Search Data for Function/Folder - using passed in parameters
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/searches/{cabinet}/{function}/{folder}/data?page={page}&pageSize={size}", Method = "POST")]
		[Description("Search Data for the specified cabinet/function/folder with optional paging using the posted prompt values to restrict the data")]
		public SearchData SearchDataPost(string cabinet, string function, string folder, int page, int size, SearchPromptValues promptValues)
		{
			if (!CheckFunction(cabinet, function, folder)) return null;
			SetRoleGroups(int.Parse(function));

			var response = GetSearchData(cabinet, function, folder, page, size, promptValues);

			if (response != null)
			{
				response.SetUri(string.Format("mobile/searches/{0}/{1}/{2}/data", cabinet, function, folder));
			}

			return response;
		}

		/// <summary>
		/// Search Data for Function/Folder
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/searches/{cabinet}/{function}/{folder}/data?page={page}&pageSize={size}", Method = "GET")]
		[Description("Search Data for the specified cabinet/function/folder with optional paging")]
		public SearchData SearchDataGet(string cabinet, string function, string folder, int page, int size)
		{
			if (!CheckFunction(cabinet, function, folder)) return null;
			SetRoleGroups(int.Parse(function));

			var response = GetSearchData(cabinet, function, folder, page, size);

			if (response != null)
			{
				response.SetUri(string.Format("mobile/searches/{0}/{1}/{2}/data", cabinet, function, folder));
			}

			return response;
		}

		/// <summary>
		/// Default Search Data for Function
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/searches/default/{function}/data?page={page}&pageSize={size}", Method = "GET")]
		[Description("Search Data for the specified function with optional paging")]
		public SearchData SearchDataGetDefault(string function, int page, int size)
		{
			if (page == 0) page = DefaultPageNumber;
			if (size == 0) size = DefaultPageSize;

			if (!SetMenuSecurity(function)) return null;
			var defaults = GetDefaultExplorerNode(function);

			var response = GetSearchData(defaults, page, size);

			if (response != null)
			{
				response.SetUri(string.Format("mobile/searches/default/{0}/data", function));
			}

			return response;
		}

		/// <summary>
		/// Validate Search Data for Function
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/searches/default/{function}/data", Method = "POST")]
		[Description("Validate selected data for the specified function")]
		public SearchData SearchDataValidateDefault(string function, SelectedValues selected)
		{
			SearchData response = new SearchData(initialise:true);
			if (selected == null || selected.Selected.Count == 0) return response;
			if (!SetMenuSecurity(function)) return null;

			var defaults = GetDefaultExplorerNode(function);

			foreach (string value in selected.Selected)
			{
				IEntity entity;

				try
				{
					entity = defaults.GetEntityById(value, true);

					if (!BaseEntity.IsValid(entity))
					{
						entity = defaults.GetEntityByName(value);
					}
				}
				catch (Exception)
				{
					entity = null;
				}

				if (!BaseEntity.IsValid(entity))
				{
					string message = string.Format("{0} {1}", defaults.TableName, value);
					SetHttpStatus(HttpStatusCode.NotFound, message);
					return null;
				}

				SearchDataRow row = SearchDataRow.LoadDataRow(defaults, entity);
				response.Rows.Add(row);
			}

			if (response.Rows.Count == 0)
			{
				response.Rows = null;
			}

			return response;
		}

		#endregion

		#region Data Functions

		/// <summary>
		/// Gets the search data.
		/// </summary>
		/// <param name="cabinet">The cabinet.</param>
		/// <param name="function">The function.</param>
		/// <param name="folder">The folder.</param>
		/// <param name="page">The page.</param>
		/// <param name="size">The size.</param>
		/// <param name="promptValues">The prompt values.</param>
		/// <returns></returns>
		private SearchData GetSearchData(string cabinet, string function, string folder, int page, int size, SearchPromptValues promptValues = null)
		{
			// Return appropriate sizes

			if (page == 0) page = DefaultPageNumber;
			if (size == 0) size = DefaultPageSize;

			// Return back the data along with the parameters

			var node = GetExplorerNode(cabinet, function, folder, promptValues);
			return GetSearchData(node, page, size);
		}

		/// <summary>
		/// Searches the data.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="page">The page.</param>
		/// <param name="size">The size.</param>
		/// <returns></returns>
		private SearchData GetSearchData(ExplorerDataNode node, int page, int size)
		{
			var response = new SearchData(initialise:true);
			response.Page = page;
			response.PageSize = size;

			if (node == null) return null;

			// Build the return

			response.LoadData(node);
			return response;
		}

		#endregion

		#region Explorer Nodes

		/// <summary>
		/// Gets the explorer node.
		/// </summary>
		/// <param name="cabinet">The cabinet.</param>
		/// <param name="function">The function.</param>
		/// <param name="folderNumber">The folder number.</param>
		/// <param name="promptValues">The prompt values.</param>
		/// <returns></returns>
		protected ExplorerBrowseNode GetExplorerNode(string cabinet, string function, string folderNumber, SearchPromptValues promptValues)
		{
			cabinet = cabinet.ToUpperInvariant();

			var cachedFolder = ExplorerCache.GetFolder(new Identity(cabinet, folderNumber));
			if (cachedFolder == null) return null;

			var context = GetContextQuery(function, cachedFolder);
			var node = GetExplorerNode(cachedFolder, context, promptValues);
			if (node == null) return null;

			node.MenuNumber = int.Parse(function);
			node.TaskSite = TaskSite;

			return node;
		}

		/// <summary>
		/// Gets the explorer node.
		/// </summary>
		/// <param name="function">The function.</param>
		/// <returns></returns>
		protected ExplorerBrowseNode GetDefaultExplorerNode(string function)
		{
			MasterMenuBase menu = (MasterMenuBase)EntityManager.Select(MasterMenuBase.EntityName, function);
			if (!BaseEntity.IsValid(menu)) return null;

			var defaults = ExplorerCache.GetDefaultsFolder(menu.TableName);
			var query = GetFunctionQuery(function);
			var node  = new ExplorerBrowseNode(defaults, null, query);
			node.TaskSite = TaskSite;
			return node;
		}

		/// <summary>
		/// Gets the explorer node.
		/// </summary>
		/// <param name="cachedFolder">The cached folder.</param>
		/// <param name="context">The context.</param>
		/// <param name="promptValues">The prompt values.</param>
		/// <returns></returns>
		private ExplorerBrowseNode GetExplorerNode(ExplorerFolderCache cachedFolder, Query context, SearchPromptValues promptValues)
		{
			// Get the base criteria

			CriteriaSaved criteria = null;
			string criteriaName = cachedFolder.CriteriaName;
			string tableName = cachedFolder.TableName;

			if (!string.IsNullOrWhiteSpace(criteriaName))
			{
				criteria = (CriteriaSaved)EntityManager.Select(CriteriaSavedBase.EntityName, new Identity(tableName, criteriaName));
				if (!BaseEntity.IsValid(criteria)) criteria = null;
			}

			if (criteria == null || !BaseEntity.IsValid(criteria)) return new ExplorerBrowseNode(cachedFolder, context, null);

			// Populate any outstanding criteria.

			var missing = new List<IEntity>();

			foreach (CriteriaCondition item in criteria.PromptConditions)
			{
				string key = TextUtils.MakePascalCase(item.Value);
				object val = null;

				if (promptValues != null)
				{
					val = promptValues.GetOrDefault(key);
				}

				if (val is JValue)
				{
					var jval = (JValue) val;
					val = jval.Value;
				}

				// Keep track of missing prompts

				if (val == null)
				{
					missing.Add(item);
					continue;
				}

				// Criteria dont apply timezones automatically - so do it manually.

				if (val is DateTime)
				{
					val = Object.GetServerDate(Library, (DateTime) val);
				}

				criteria.PromptValues.Add(item.Value, val);
			}

			// Skip any prompt conditions that have missing values.

			foreach (var item in missing)
			{
				criteria.PromptConditions.Remove(item);
			}

			var query = (Query) CriteriaService.GetCriteriaQuery(criteria);
			return new ExplorerBrowseNode(cachedFolder, query, context);
		}

		#endregion

		#region Security Checks

		/// <summary>
		/// Checks the function.
		/// </summary>
		/// <param name="cabinet">The cabinet.</param>
		/// <param name="function">The function.</param>
		/// <param name="folder">The folder.</param>
		/// <returns></returns>
		private bool CheckFunction(string cabinet, string function, string folder)
		{
			// Check function security

			int menuProc;
			if (!int.TryParse(function, out menuProc)) return false;
			if (!SetMenuSecurity(function)) return false;

			// Check folder rmb function presence

			cabinet = cabinet.ToUpperInvariant();

			var cab = ExplorerCache.GetCabinet(cabinet);
			if (cab == null) return false;

			int folderNum;
			if (!int.TryParse(folder, out folderNum)) return false;

			var expFolder = ExplorerCache.GetFolder(cabinet, folderNum);
			if (expFolder == null) return false;

			foreach (var rmb in expFolder.RmbList)
			{
				if (rmb.MenuProc.Equals(menuProc)) return true;
			}

			return false;
		}

		#endregion

		#region Context

		/// <summary>
		/// Gets the context query.
		/// </summary>
		/// <param name="function">The function.</param>
		/// <param name="cachedFolder">The cached folder.</param>
		/// <returns></returns>
		private Query GetContextQuery(string function, ExplorerFolderCache cachedFolder)
		{
			var query = GetFunctionQuery(function);

			// RMB Context

			foreach (var rmb in cachedFolder.RmbList)
			{
				if (rmb.MenuProc != int.Parse(function)) continue;

				switch (rmb.ContextOperator)
				{
					case OperatorEqual:
						{
							query.AddEquals(rmb.ContextField, rmb.ContextValue);
							break;
						}
					case OperatorGreaterEqual:
						{
							query.AddGreaterThanOrEquals(rmb.ContextField, rmb.ContextValue);
							break;
						}
					case OperatorGreaterThan:
						{
							query.AddGreaterThan(rmb.ContextField, rmb.ContextValue);
							break;
						}
					case OperatorIn:
						{
							query.AddIn(rmb.ContextField, new List<object>(rmb.ContextValue.Split(',')));
							break;
						}
					case OperatorLessEqual:
						{
							query.AddLessThanOrEquals(rmb.ContextField, rmb.ContextValue);
							break;
						}
					case OperatorLessThan:
						{
							query.AddLessThan(rmb.ContextField, rmb.ContextValue);
							break;
						}
					case OperatorLike:
						{
							query.AddLike(rmb.ContextField, rmb.ContextValue);
							break;
						}
					case OperatorNotEqual:
						{
							query.AddNotEquals(rmb.ContextField, rmb.ContextValue);
							break;
						}
					case OperatorNotIn:
						{
							query.AddNot();
							query.AddIn(rmb.ContextField, new List<object>(rmb.ContextValue.Split(',')));
							break;
						}
					case OperatorNotLike:
						{
							query.AddNot();
							query.AddLike(rmb.ContextField, rmb.ContextValue);
							break;
						}
				}

				break;
			}

			return query;
		}

		/// <summary>
		/// Gets the function query.
		/// </summary>
		/// <param name="function">The function.</param>
		/// <returns></returns>
		private Query GetFunctionQuery(string function)
		{
			int menuProc;
			if (string.IsNullOrWhiteSpace(function)) return null;
			if (!int.TryParse(function, out menuProc)) return null;

			var menu = ExplorerCache.GetMasterMenu(menuProc);
			if (menu == null) return null;

			var query = (Query) EntityManager.CreateQuery(menu.TableName);
			ExplorerAuxCache aux = ExplorerCache.GetExplorerAux(menu);

			if (aux != null)
			{
				string fromStatus = aux.FromStatus;
				if (!string.IsNullOrWhiteSpace(fromStatus))
				{
					if (menu.TableName == JobHeaderBase.StructureTableName)
					{
						query.AddIn("JOB_STATUS", new List<object>(fromStatus.Split(',')));
					}
					else
					{
						query.AddIn("STATUS", new List<object>(fromStatus.Split(',')));
					}
				}
			}

			return query;
		}

		#endregion
	}
}