using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Data Collection
	/// </summary>
	[DataContract(Name="dataCollection")]
	public class DataCollection : MobileObject
	{
		#region Properties

		/// <summary>
		/// Gets or sets the page.
		/// </summary>
		/// <value>
		/// The page.
		/// </value>
		[DataMember(Name = "page")]
		public int Page { get; set; }

		/// <summary>
		/// Gets or sets the size of the page.
		/// </summary>
		/// <value>
		/// The size of the page.
		/// </value>
		[DataMember(Name = "pagesize")]
		public int PageSize { get; set; }

		/// <summary>
		/// Gets or sets the total count of records.
		/// </summary>
		/// <value>
		/// The count.
		/// </value>
		[DataMember(Name = "count")]
		public int Count { get; set; }

		/// <summary>
		/// Gets or sets the next page.
		/// </summary>
		/// <value>
		/// The next page.
		/// </value>
		[DataMember(Name = "nextUri")]
		public Uri NextPage { get; set; }

		/// <summary>
		/// Gets or sets the previous page.
		/// </summary>
		/// <value>
		/// The previous page.
		/// </value>
		[DataMember(Name = "previousUri")]
		public Uri PreviousPage { get; set; }

		/// <summary>
		/// Gets or sets the start filter
		/// </summary>
		/// <value>
		/// The start filter value
		/// </value>
		[DataMember(Name = "findFilter")]
		public string FindFilter { get; set; }

		/// <summary>
		/// Gets or sets the criteria filter
		/// </summary>
		/// <value>
		/// The criteria filter value
		/// </value>
		[DataMember(Name = "criteria")]
		public string Criteria { get; set; }

		/// <summary>
		/// Gets or sets the columns.
		/// </summary>
		/// <value>
		/// The columns.
		/// </value>
		[DataMember(Name = "columns")]
		public List<Column> Columns { get; set; }

		/// <summary>
		/// Gets or sets the rows.
		/// </summary>
		/// <value>
		/// The rows.
		/// </value>
		[DataMember(Name = "rows")]
		public List<DataCollectionItem> Rows { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="DataCollection"/> class.
		/// </summary>
		public DataCollection()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DataCollection"/> class.
		/// </summary>
		/// <param name="initialise">if set to <c>true</c> [initialise].</param>
		public DataCollection(bool initialise = false)
		{
			if (!initialise) return;
			Rows = new List<DataCollectionItem>();
		}

		#endregion

		#region Set Page Information

		/// <summary>
		/// Sets the Previous/Next URI.
		/// </summary>
		/// <param name="baseUri">The base URI.</param>
		/// <param name="alwaysNext">if set to <c>true</c> always next uri.</param>
		public virtual void SetUri(string baseUri, bool alwaysNext = false)
		{
			SetNextPage(baseUri, alwaysNext);
			SetPreviousPage(baseUri);
		}

		/// <summary>
		/// Sets the previous page.
		/// </summary>
		/// <param name="baseUri">The base URI.</param>
		private void SetPreviousPage(string baseUri)
		{
			if (Page == 1) return;
			int page = Page - 1;
			string path = AddParams(baseUri, page);
			PreviousPage = MakeLink(path);
		}

		/// <summary>
		/// Sets the next page.
		/// </summary>
		/// <param name="baseUri">The base URI.</param>
		/// <param name="alwaysNext">if set to <c>true</c> always next uri.</param>
		private void SetNextPage(string baseUri, bool alwaysNext)
		{
			int currentCount = Page * PageSize;
			if (currentCount >= Count && !alwaysNext) return;

			int next = Page + 1;
			string path = AddParams(baseUri, next);
			NextPage = MakeLink(path);
		}

		/// <summary>
		/// Adds the parameters.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="page">The page.</param>
		/// <returns></returns>
		private string AddParams(string path, int page)
		{
			path = string.Concat(path, string.Format("?page={0}", page));

			if (PageSize!=100)
			{
				path = string.Concat(path, string.Format("&pageSize={0}", PageSize));
			}

			if (!string.IsNullOrWhiteSpace(FindFilter))
			{
				path = string.Concat(path, string.Format("&filter={0}", FindFilter));
			}

			if (!string.IsNullOrWhiteSpace(Criteria))
			{
				path = string.Concat(path, string.Format("&criteria={0}", Criteria));
			}

			return path;
		}

		#endregion
	}
}
