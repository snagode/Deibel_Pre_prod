using System;
using System.Runtime.Serialization;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Entity
	/// </summary>
	[DataContract(Name="entity")]
	public class Entity : MobileObject
	{
		#region Properties

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		[DataMember(Name = "name")]
		public string Name { get; set; }

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
		/// Gets or sets the icon.
		/// </summary>
		/// <value>
		/// The icon.
		/// </value>
		[DataMember(Name = "icon")]
		public string Icon { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Entity"/> is expanded.
		/// </summary>
		/// <value>
		///   <c>true</c> if expanded; otherwise, <c>false</c>.
		/// </value>
		[DataMember(Name = "expanded")]
		public bool Expanded { get; set; }

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

		#region Utility Statics

		/// <summary>
		/// Loads the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="expand">if set to <c>true</c> expand sub-entities</param>
		/// <returns></returns>
		public static Entity Load(IEntity item, bool expand = false)
		{
			var entity = new Entity();
			entity.TableName = item.EntityType;
			entity.Key0 = item.IdentityString.Trim();
			entity.Expanded = expand;
			entity.Icon = item.Icon;
			entity.Name = item.Name;

			entity.EntityUri = MakeEntityLink(item);

			if (!expand) return entity;

			entity.Data = new KeyValues<object>();
			entity.Data.FlattenLinks = false;

			foreach (string property in EntityType.GetProperties(item.EntityType))
			{
				var val = item.Get(property);
				entity.Data.Add(property, val);
			}

			return entity;
		}

		/// <summary>
		/// Makes the link.
		/// </summary>
		/// <param name="table">The table.</param>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public static Uri MakeEntityLink(string table, string key)
		{
			return MakeCaseSpecificLink("/mobile/entities/{0}/{1}", table.ToLowerInvariant(), key.Trim());
		}

		/// <summary>
		/// Makes the link.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		public static Uri MakeEntityLink(IEntity entity)
		{
			string table = entity.EntityType;
			string key = string.Empty;

			foreach (object val in entity.Identity.Fields)
			{
				key = string.Concat(key, val.ToString().Trim(), "/");
			}

			return MakeEntityLink(table, key.TrimEnd('/'));
		}

		#endregion
	}
}
