using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Active Desktop Task
	/// </summary>
	[SampleManagerTask("ActiveDesktopTask")]
	public class ActiveDesktopTask : DefaultFormTask
	{
		#region Member Variables

		private FormActiveDesktop m_Form;
		private string m_CurrentURL;

		#endregion

		#region Properties

		

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm" /> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			base.MainFormCreated();

			m_Form = (FormActiveDesktop)MainForm;
		}

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm" /> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			base.MainFormLoaded();

			m_Form.Title = "Sample Status";
			m_Form.ActiveDesktop.AllowNavigation = true;
			m_Form.ActiveDesktop.URL = m_CurrentURL;
			m_Form.ActiveDesktop.Refresh();
		}

		/// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			m_CurrentURL = GetDefaultURI();
			base.SetupTask();
		}

		/// <summary>
		/// Gets the default URI.
		/// </summary>
		/// <returns></returns>
		public string GetDefaultURI()
		{
			string currentOperator = Library.Environment.GetGlobalString("OPERATOR");
			IEntity defaultURI = EntityManager.Select("USER_SETTING", new Identity(currentOperator, "ACTIVE_DESKTOP", "HOME_PAGE", "URL"));

			if (defaultURI == null)
			{
				// If we cant get the users settings, get the default one instead.
				defaultURI = EntityManager.Select("USER_SETTING", new Identity("DEFAULT", "ACTIVE_DESKTOP", "HOME_PAGE", "URL"));
			}

			string URI = "";
			if (defaultURI != null)
			{
				URI = defaultURI.GetString("VALUE");
			}

			return URI;
		}

		#endregion
	}
}