using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Search Data
	/// </summary>
	[DataContract(Name="searchData")]
	public class SearchData : DataCollection
	{
		#region Properties

		/// <summary>
		/// Gets or sets the parent URI.
		/// </summary>
		/// <value>
		/// The parent URI.
		/// </value>
		[DataMember(Name = "parentUri")]
		public Uri ParentUri { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="SearchData"/> class.
		/// </summary>
		public SearchData()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SearchData"/> class.
		/// </summary>
		/// <param name="initialise">if set to <c>true</c> initialise.</param>
		public SearchData(bool initialise = false) : base (initialise)
		{
		}

		#endregion

		#region Column Definition

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

			Columns = new List<Column>();

			foreach (var explorerColumn in node.Columns)
			{
				var col = SearchColumn.LoadColumn(library, node, explorerColumn);
				Columns.Add(col);
			}
		}

		#endregion

		#region Data

		/// <summary>
		/// Loads the data.
		/// </summary>
		/// <param name="node">The node.</param>
		public void LoadData(ExplorerDataNode node)
		{
			Count = node.DisplayCount;

			int start = (PageSize * (Page - 1));
			var data = (IEntityCollection)node.ReadDataBlock(start, start + PageSize - 1);

			foreach (IEntity item in data)
			{
				SearchDataRow row = SearchDataRow.LoadDataRow(node, item);
				Rows.Add(row);
			}
		}

		/// <summary>
		/// Loads the hierarchy data.
		/// </summary>
		/// <param name="entityManager">The entity manager.</param>
		/// <param name="node">The node.</param>
		/// <param name="hierarchy">The hierarchy.</param>
		/// <param name="entity">The entity.</param>
		public void LoadHierarchy(IEntityManager entityManager, EntityHierarchyNode node, string hierarchy, string entity)
		{
			IEntityCollection data = entityManager.CreateEntityCollection(node.TableName);
			
			hierarchy = EntityType.GetPropertyName(node.TableName, hierarchy);
			if (hierarchy == null) return;
			
			string baseUri = string.Format("mobile/browses/{0}/{1}", node.TableName, hierarchy);

			if (string.IsNullOrWhiteSpace(entity))
			{
				var dataQuery = GetHierarchyRootQuery(entityManager, node, hierarchy);
				data = entityManager.Select(dataQuery);
			}
			else
			{
				entity = entity.ToUpperInvariant();
				IEntity entityObject = entityManager.Select(node.TableName, new Identity(entity));
				if (entityObject != null)
				{
					data = entityObject.GetEntityCollection(hierarchy);
				}
			}

			foreach (IEntity item in data)
			{
				var row = SearchDataRow.LoadDataRow(node, item);

				row.Selectable = node.Selectable;
				row.SetChildBrowseUri(baseUri, item);

				Rows.Add(row);
			}

			// Paging not supported

			Count = data.Count;
			Page = 1;
			PageSize = Count;
		}

		/// <summary>
		/// Loads the hierarchy root.
		/// </summary>
		/// <param name="entityManager">The entity manager.</param>
		/// <param name="node">The node.</param>
		/// <param name="hierarchy">The hierarchy.</param>
		/// <returns></returns>
		private Query GetHierarchyRootQuery(IEntityManager entityManager, EntityHierarchyNode node, string hierarchy)
		{
			ISchemaField primaryBrowseField = Schema.Current.Tables[node.TableName].BrowseField;

			string parentLinkField = EntityType.GetChildCollectionLinkField(node.TableName, hierarchy);
			if (string.IsNullOrEmpty(parentLinkField)) return null;

			IQuery query = entityManager.CreateQuery(node.TableName);
			query.AddEquals(parentLinkField, null);
			query.HideRemoved();
			query.AddOrder(primaryBrowseField.Name, true);

			return (Query)query;
		}

		#endregion
	}
}
