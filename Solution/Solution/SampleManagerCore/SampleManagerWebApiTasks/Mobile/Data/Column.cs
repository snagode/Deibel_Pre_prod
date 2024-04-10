using System;
using System.Runtime.Serialization;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Column
	/// </summary>
	[DataContract(Name="column")]
	public class Column : MobileObject
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
		/// Gets or sets the type.
		/// </summary>
		/// <value>
		/// The type.
		/// </value>
		[DataMember(Name = "type")]
		public string Type { get; set; }

		/// <summary>
		/// Gets or sets the name of the property.
		/// </summary>
		/// <value>
		/// The name of the property.
		/// </value>
		public string PropertyName { get; set; }

		/// <summary>
		/// Gets or sets the name of the table.
		/// </summary>
		/// <value>
		/// The name of the table.
		/// </value>
		public string TableName { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to allow server sorting.
		/// </summary>
		/// <value>
		///   <c>true</c> if allow sort; otherwise, <c>false</c>.
		/// </value>
		[DataMember(Name = "allowSort")]
		public bool AllowSort { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="SearchColumn"/> is hidden.
		/// </summary>
		/// <value>
		///   <c>true</c> if hidden; otherwise, <c>false</c>.
		/// </value>
		[DataMember(Name = "hidden")]
		public bool Hidden { get; set; }

		/// <summary>
		/// Gets or sets the width.
		/// </summary>
		/// <value>
		/// The width.
		/// </value>
		[DataMember(Name = "width")]
		public int Width { get; set; }

		/// <summary>
		/// Gets or sets the Prompt Uri
		/// </summary>
		/// <value>
		/// The Prompt Uri
		/// </value>
		[DataMember(Name = "promptUri")]
		public Uri PromptUri { get; set; }

		#endregion

		#region Setting Data Types

		/// <summary>
		/// Sets the type of the data.
		/// </summary>
		/// <param name="entityType">Type of the entity.</param>
		/// <param name="property">The property.</param>
		public void SetDataType(string entityType, string property)
		{
			Type = SMDataType.Text.ToString();
			var att = EntityType.GetPromptAttribute(entityType, property);

			if (att != null)
			{
				if (att is PromptPackedDecimalAttribute) Type = "int";
				else if (att is PromptIntegerAttribute) Type = "int";
				else if (att is PromptIntervalAttribute) Type = "timespan";
				else if (att is PromptDateAttribute) Type = "datetime";
				else Type = "string";

				PromptUri = MakeLink("/mobile/prompts/{0}/{1}", entityType, property);
			}
		}

		#endregion

		#region Utility Statics

		/// <summary>
		/// Loads the column.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <param name="entityType">Type of the entity.</param>
		/// <param name="property">The property.</param>
		/// <returns></returns>
		public static Column LoadColumn(StandardLibrary library, string entityType, string property)
		{
			var col = new Column();

			col.Id = string.Format("{0}{1}", entityType, property);
			col.Name = property;
			col.PropertyName = property;
			col.SetDataType(entityType, property);
			col.TableName = entityType;

			return col;
		}

		#endregion
	}
}
