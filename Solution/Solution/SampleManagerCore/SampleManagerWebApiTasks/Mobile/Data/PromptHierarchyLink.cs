using System;
using System.Runtime.Serialization;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Prompt Hierarchy
	/// </summary>
	[DataContract(Name = "promptHierarchyLink", Namespace = "")]
	public class PromptHierarchyLink : Prompt
	{
		#region Member Variables

		/// <summary>
		/// The prompt type
		/// </summary>
		public const string PromptType = "hierarchy";

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
		public string BrowseUri{ get; set; }

		/// <summary>
		/// Gets or sets the hierarchy browse URI.
		/// </summary>
		/// <value>
		/// The hierarchy browse URI.
		/// </value>
		[DataMember(Name = "hierarchyBrowseUri")]
		public string HierarchyBrowseUri { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptHierarchyLink"/> class.
		/// </summary>
		public PromptHierarchyLink()
		{
			Datatype = PromptType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptHierarchyLink" /> class.
		/// </summary>
		/// <param name="attribute">The attribute.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="property">The property.</param>
		/// <param name="value">The value.</param>
		public PromptHierarchyLink(PromptHierarchyLinkAttribute attribute, string entity, string property, string value = null) : this()
		{
			var propertyInfo = EntityType.GetProperty(entity, property);
			var linkType = attribute.GetLinkType(propertyInfo);

			Uri uri = new Uri(string.Format("/mobile/browses/{0}", linkType), UriKind.Relative);
			BrowseUri = Uri.EscapeUriString(uri.ToString().ToLowerInvariant());

			Uri hier = new Uri(string.Format("/mobile/browses/{0}/{1}", linkType, attribute.HierarchyProperty), UriKind.Relative);
			HierarchyBrowseUri = Uri.EscapeUriString(hier.ToString().ToLowerInvariant());

			if (value != null)
			{
				object val;
				if (attribute.TryParse(value, out val))
				{
					Value = (string)val;
				}
			}
		}

		#endregion
	}
}
