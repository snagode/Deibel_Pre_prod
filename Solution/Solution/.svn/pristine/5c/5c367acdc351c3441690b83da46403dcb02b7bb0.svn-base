using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the LANGUAGE entity.
	/// </summary>
	[SampleManagerEntity(LanguageBase.EntityName)]
	public class Language : LanguageBase
	{
		#region Member Variables

		private IEntityCollection m_Forms;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the forms implemented in this language.
		/// </summary>
		/// <value>The list of forms indicated whether they have been implemented in the language.</value>
		[PromptCollection("LANGUAGE_FORM", false)]
		public IEntityCollection Forms
		{
			get
			{
				if (m_Forms == null)
				{
					m_Forms = EntityManager.Select("LANGUAGE_FORM");
				}

				foreach (LanguageForm form in m_Forms)
				{
					form.Language = this;
				}

				return m_Forms;
			}
		}

		#endregion
	}
}
