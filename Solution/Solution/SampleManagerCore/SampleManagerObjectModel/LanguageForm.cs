using Thermo.SampleManager.Common.Data;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Manages the forms which have been localised for a parent language.
	/// </summary> 
	[SampleManagerEntity("LANGUAGE_FORM")]
	public class LanguageForm : Form
	{
		#region Member Variables

		private Language m_Language;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the state icon.
		/// </summary>
		/// <value>The state icon.</value>
		[EntityIcon]
		public string StateIcon
		{
			get
			{
				if (Language != null && CheckTranslation(Language.Locale))
				{
					return "INT_IMPORT_SUCCESS";
				}

				return "INT_IMPORT_ERROR";
			}
		}

		/// <summary>
		/// Gets or sets the language.
		/// </summary>
		/// <value>The language.</value>
		[PromptLink(TableNames.Language, false)]
		public Language Language
		{
			get { return m_Language; }
			set { m_Language = value; }
		}

		#endregion
	}
}
