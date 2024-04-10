using Thermo.SampleManager.Library;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Custom Startup Task - place holder for end user custom code during startup
	/// </summary>
	[SampleManagerTask("CustomStartupTask")]
	public class CustomStartupTask : SampleManagerTask
	{
		/// <summary>
		/// Sets up the task.
		/// </summary>
		protected override void SetupTask()
		{
			// Library.Utils.FlashMessage("Custom Startup Task", "Customization");

			Exit();
		}
	}
}