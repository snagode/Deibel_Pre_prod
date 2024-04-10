using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Search Data Row
	/// </summary>
	[DataContract(Name="searchDataRow")]
	public class SearchDataRow : DataCollectionItem
	{
		#region Properties

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="SearchDataRow"/> is selectable.
		/// </summary>
		/// <value>
		///   <c>true</c> if selectable; otherwise, <c>false</c>.
		/// </value>
		[DataMember(Name = "selectable")]
		[DefaultValue(true)]
		public bool Selectable { get; set; }

		/// <summary>
		/// Gets or sets the child browse URI.
		/// </summary>
		/// <value>
		/// The child browse URI.
		/// </value>
		[DataMember(Name = "childBrowseUri")]
		public Uri ChildBrowseUri { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="SearchDataRow"/> class.
		/// </summary>
		public SearchDataRow()
		{
			Selectable = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SearchDataRow"/> class.
		/// </summary>
		/// <param name="initialise">if set to <c>true</c> initialise.</param>
		public SearchDataRow(bool initialise = false) : this ()
		{
			if (!initialise) return;
			Data = new KeyValues<object>();
		}

		#endregion

		#region Set Link Information

		/// <summary>
		/// Sets the child Browse URI.
		/// </summary>
		/// <param name="baseUri">The base URI.</param>
		/// <param name="entity">The entity.</param>
		public void SetChildBrowseUri(string baseUri, IEntity entity)
		{
			ChildBrowseUri = MakeCaseSpecificLink("{0}/{1}", baseUri, entity.IdentityString.Trim());
		}

		#endregion

		#region Utility Statics

		/// <summary>
		/// Loads the data row.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		public static SearchDataRow LoadDataRow(ExplorerDataNode node, IEntity item)
		{
			SearchDataRow row = new SearchDataRow(initialise:true);

			row.TableName = item.EntityType;
			row.Key0 = item.IdentityString.TrimEnd();

			row.SetUri(item);

			foreach (var col in node.Columns)
			{
				object val = item.Get(col.FieldName);
				row.Data.Add(col.FieldName.Trim('"'), val);
			}

			return row;
		}

		#endregion
	}
}
