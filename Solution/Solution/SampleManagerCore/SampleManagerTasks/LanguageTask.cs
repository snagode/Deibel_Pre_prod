using System.Collections.Generic;
using System.Globalization;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of language LTE.
	/// </summary>
	[SampleManagerTask("LanguageTask", "LABTABLE", "LANGUAGE")]
	public class LanguageTask : GenericLabtableTask
	{
		#region Member Variables

		private FormLanguage m_Form;

		#endregion

		#region Overriden Functionality

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormLanguage) MainForm;

			LoadLocaleBrowse();
		}

		#endregion

		#region Browse

		/// <summary>
		/// Loads the locale browse.
		/// </summary>
		private void LoadLocaleBrowse()
		{
			// Get and enumerate all cultures.

			CultureInfo[] allCultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
			List<string> localeString = new List<string>();

			foreach (CultureInfo ci in allCultures)
			{
				localeString.Add(ci.Name);
			}

			localeString.Sort();

			m_Form.Locale.Browse.Republish(localeString);
		}

		#endregion
	}
}