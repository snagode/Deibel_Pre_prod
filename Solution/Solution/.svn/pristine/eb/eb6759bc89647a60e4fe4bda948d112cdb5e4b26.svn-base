using System.ComponentModel;
using System.ServiceModel.Web;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Server;
using Thermo.SampleManager.WebApiTasks.Mobile.Data;

namespace Thermo.SampleManager.WebApiTasks.Mobile
{
	/// <summary>
	/// Search Task
	/// </summary>
	[SampleManagerWebApi("mobile.prompt")]
	public class PromptTask : SampleManagerWebApiTask
	{
		#region Prompt Information

		/// <summary>
		/// Prompt
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/prompts/{entity}/{property}", Method = "GET")]
		[Description("Prompt definition for the specified entity property")]
		public Prompt PromptGet(string entity, string property)
		{
			if (string.IsNullOrEmpty(entity) || string.IsNullOrEmpty(property)) return null;

			entity = entity.ToUpperInvariant();
			property = EntityType.GetPropertyName(entity, property);

			// Default Values

			string defaultValue = null;
			var field = EntityType.GetFieldnameFromProperty(entity, property);
			if (field != null)
			{
				object def = Prompt.GetDefaultValue(Library, entity, field);
				if (def != null)
				{
					var att = EntityType.GetPromptAttribute(entity, property);
					if (att != null)
					{
						defaultValue = att.FormatString(def);
					}
				}
			}

			// Create the prompt

			var prompt = Prompt.CreateByProperty(Library, entity, property, defaultValue);

			string propertyName = MobileObject.GetLocalizedPropertyName(Library, entity, property);
			string entityName = MobileObject.GetLocalizedEntityName(Library, entity);

			prompt.Id = property;
			prompt.Label = propertyName;
			prompt.Tooltip = string.Format("{0}.{1}", entityName, propertyName);

			return prompt;
		}

		#endregion
	}
}