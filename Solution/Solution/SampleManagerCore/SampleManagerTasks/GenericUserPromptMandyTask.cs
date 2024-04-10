using Thermo.SampleManager.Library;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Mandatory Prompt Task
	/// </summary>
	[SampleManagerTask("GenericUserPromptMandyTask", "USERPROMPT")]
	public class GenericUserPromptMandyTask : GenericUserPromptTask
	{
		#region Overrides

		/// <summary>
		/// Setup the SampleManager LTE task
		/// </summary>
		protected override void SetupTask()
		{
			m_Mandatory = true;
		    m_ForceValid = true;
			base.SetupTask();
		}

		#endregion
	}
}