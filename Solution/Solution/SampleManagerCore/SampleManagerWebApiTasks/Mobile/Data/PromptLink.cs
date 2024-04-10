using System;
using System.Runtime.Serialization;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Prompt Text
	/// </summary>
	[DataContract(Name = "promptLink", Namespace = "")]
	public class PromptLink : Prompt
	{
		#region Member Variables

		/// <summary>
		/// The prompt type
		/// </summary>
		public const string PromptType = "entity";

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>
		/// The value.
		/// </value>
		[DataMember(Name = "value")]
		public string Value { get; set; }

		/// <summary>
		/// Gets or sets the browse URI.
		/// </summary>
		/// <value>
		/// The browse URI.
		/// </value>
		[DataMember(Name = "browseUri")]
		public Uri BrowseUri{ get; set; }

		/// <summary>
		/// Gets or sets the criteria.
		/// </summary>
		/// <value>
		/// The criteria.
		/// </value>
		[DataMember(Name = "criteria")]
		public string Criteria { get; set; }

		/// <summary>
		/// Gets the type of the entity.
		/// </summary>
		/// <value>
		/// The type of the entity.
		/// </value>
		[DataMember(Name = "linkType")]
		public string LinkType { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptLink"/> class.
		/// </summary>
		public PromptLink()
		{
			Datatype = PromptType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptLink" /> class.
		/// </summary>
		/// <param name="linkType">Type of the link.</param>
		/// <param name="value">The value.</param>
		/// <param name="criteria">The criteria.</param>
		public PromptLink(string linkType, string value = null, string criteria = null) : this (new PromptLinkAttribute(linkType), value)
		{
			SetBrowseUri(linkType, criteria);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptLink" /> class.
		/// </summary>
		/// <param name="attribute">The attribute.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="property">The property.</param>
		/// <param name="value">The value.</param>
		/// <param name="criteria">The criteria.</param>
		public PromptLink(PromptLinkAttribute attribute, string entity, string property, string value = null, string criteria = null) : this(attribute, value)
		{
			var propertyInfo = EntityType.GetProperty(entity, property);
			var linkType = attribute.GetLinkType(propertyInfo);
			SetBrowseUri(linkType, criteria);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptLink" /> class.
		/// </summary>
		/// <param name="attribute">The attribute.</param>
		/// <param name="value">The value.</param>
		public PromptLink(PromptLinkAttribute attribute, string value) : this()
		{
			if (attribute == null) return;
			if (value == null) return;

			object val;
			if (attribute.TryParse(value, out val))
			{
				Value = (string) val;
			}
		}

		#endregion

		#region Uri

		/// <summary>
		/// Sets the browse URI.
		/// </summary>
		/// <param name="linkType">Type of the link.</param>
		/// <param name="criteria">The criteria.</param>
		/// <returns></returns>
		private void SetBrowseUri(string linkType, string criteria)
		{
			LinkType = linkType;
			Criteria = criteria;

			var path = string.Format("/mobile/browses/{0}", LinkType);

			if (!string.IsNullOrWhiteSpace(criteria))
			{
				path = string.Concat(path, string.Format("?criteria={0}", Criteria));
			}

			BrowseUri = MakeLink(path);
		}

		#endregion
	}
}
