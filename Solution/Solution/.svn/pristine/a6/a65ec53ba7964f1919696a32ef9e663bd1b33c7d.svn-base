using System;
using System.Runtime.Serialization;
using Thermo.SampleManager.Common.Data;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Data Item
	/// </summary>
	[DataContract(Name="dataItem")]
	public class DataCollectionItem : MobileObject
	{
		#region Properties

		/// <summary>
		/// Gets or sets the key0
		/// </summary>
		/// <value>
		/// The key0
		/// </value>
		[DataMember(Name = "key0")]
		public string Key0 { get; set; }

		/// <summary>
		/// Gets or sets the name of the table.
		/// </summary>
		/// <value>
		/// The name of the table.
		/// </value>
		[DataMember(Name = "tablename")]
		public string TableName { get; set; }

		/// <summary>
		/// Gets or sets the entity URI.
		/// </summary>
		/// <value>
		/// The entity URI.
		/// </value>
		[DataMember(Name = "entityUri")]
		public Uri EntityUri { get; set; }

		/// <summary>
		/// Gets or sets the data.
		/// </summary>
		/// <value>
		/// The data.
		/// </value>
		[DataMember(Name = "values")]
		public KeyValues<object> Data { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="DataCollectionItem"/> class.
		/// </summary>
		public DataCollectionItem()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DataCollectionItem"/> class.
		/// </summary>
		/// <param name="initialise">if set to <c>true</c> initialise data list.</param>
		public DataCollectionItem(bool initialise = false) : this ()
		{
			if (!initialise) return;
			Data = new KeyValues<object>();
		}

		#endregion

		#region Uri

		/// <summary>
		/// Sets the URI.
		/// </summary>
		/// <param name="entity">The entity.</param>
		public void SetUri(IEntity entity)
		{
			EntityUri = MakeCaseSpecificLink("mobile/entities/{0}/{1}", entity.EntityType.ToLowerInvariant(), entity.IdentityString.Trim());
		}

		#endregion
	}
}
