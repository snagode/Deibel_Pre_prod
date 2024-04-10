using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the ENTITY_TEMPLATE_PROPERTY entity.
	/// </summary>
	[SampleManagerEntity(EntityTemplatePropertyBase.EntityName)]
	public class EntityTemplateProperty : EntityTemplatePropertyInternal
	{
		#region Overrides

		/// <summary>
		/// Called when when a property changes.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged(PropertyEventArgs e)
		{
			if (e.PropertyName == EntityTemplatePropertyPropertyNames.PropertyName)
			{
				DefaultValue = null;
			}

			base.OnPropertyChanged(e);
		}

		/// <summary>
		/// Called when a property is about to change.  Allows the update to be cancelled and
		/// an error to be displayed in the UI on the relevant control which has attepted the update.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="newValue">The new value.</param>
		/// <param name="errorMessage">The error message.</param>
		/// <returns>
		/// True when <paramref name="newValue"/> is valid, otherwise False.
		/// </returns>
		protected override bool OnValidateProperty(string propertyName, object newValue, out string errorMessage)
		{
			if (propertyName == EntityTemplatePropertyPropertyNames.FilterBy)
			{
				string filterByValue = newValue as string;

				if (!string.IsNullOrEmpty(filterByValue))
				{
					// Make sure the filter value is able to filter the property assigned to this instance

					string linkType = EntityType.GetLinkedEntityType(EntityTemplate.TableName, filterByValue);
					string propertyType = EntityType.GetLinkedEntityType(EntityTemplate.TableName, PropertyName);

					IList<string> properties = EntityType.GetReflectedPropertyNames(propertyType);

					// Loop through each property to find a link 

					foreach (var property in properties)
					{
						PromptAttribute attribute = EntityType.GetPromptAttribute(propertyType, property);

						if (attribute.IsLink && EntityType.GetLinkedEntityType(propertyType, property) == linkType)
						{
							errorMessage = string.Empty;
							return true;
						}
					}

					// The selected FilterBy value is invalid for this property
					errorMessage = ServerMessageManager.Current.GetMessage("CommonMessages", "EntityTemplateInvalidFilterBy");
					errorMessage = string.Format(errorMessage, PropertyName, filterByValue);
					return false;
				}
			}

			errorMessage = string.Empty;
			return true;
		}

		#endregion
	}
}
