using Thermo.Framework.Server;
using Thermo.Framework.Utilities;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using System.Collections.Generic;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the FORM entity.
	/// </summary>
	[SampleManagerEntity(FormBase.EntityName)]
	public class Form : FormBase
	{
		#region Public Constants

		private Dictionary<string, string> m_Languages;

		#endregion

		#region IEntityMenu Members

		/// <summary>
		/// Creates extended menu items for the entity.
		/// </summary>
		/// <returns></returns>
		public override MenuItemCollection CreateMenu()
		{
			// Create a lookup table for the locale strings
			m_Languages = new Dictionary<string, string>();

			// Create a menu items collection
			MenuItemCollection menuItems = new MenuItemCollection();

			// Select all the languages
			IEntityCollection languages = EntityManager.Select("LANGUAGE");

			// Create a sub menu for internationalisation
			MenuItem internationalise =
				new MenuItem(Library.Message.GetMessage("CommonMessages", "FormMenuLocalization"),
							 Library.Message.GetMessage("CommonMessages", "FormMenuLocalizationMessage"), "",
							 true, false, null);
			menuItems.Add(internationalise);

			// Add each language as a menu item
			foreach (Language language in languages)
			{
				if (!string.IsNullOrEmpty(language.Locale) && !language.Removeflag)
				{
					menuItems.Add(language.Name, language.Description, language.Icon.Identity, false, false, internationalise, TranslateForm);
					m_Languages.Add(language.Name, language.Identity);
				}
			}

			// Remove the group if no items have been added
			if (menuItems.Count == 1)
			{
				menuItems.Remove(internationalise);
			}

			// Return the collection of menu items
			return menuItems;
		}

		/// <summary>
		/// Callback for the internationalisation menu. Calls the internationalise task with the language as a parameter.
		/// </summary>
		/// <param name="menuItem"></param>
		public void TranslateForm(MenuItem menuItem)
		{
			Library.Task.CreateTask(15797, this, m_Languages[menuItem.Name]);
		}

		#endregion

		/// <summary>
		/// Gets the available forms.
		/// </summary>
		[PromptCollection(EntityName, false)]
		public IEntityCollection AvailablePages
		{

			get
			{
				IQuery pageQuery = EntityManager.CreateQuery(FormBase.EntityName);
				pageQuery.AddEquals(FormPropertyNames.Type, "PAGE");
				pageQuery.AddAnd();
				pageQuery.AddNotEquals(FormPropertyNames.FormName,FormName);
				pageQuery.PushBracket();
				pageQuery.AddEquals(FormPropertyNames.FormEntityDefinition,FormEntityDefinition);
				pageQuery.AddOr();
				pageQuery.AddEquals(FormPropertyNames.FormEntityDefinition,string.Empty);
				pageQuery.PopBracket();
				return EntityManager.Select(FormBase.EntityName, pageQuery);

			}

		}

	}
}
