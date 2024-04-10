using Thermo.SampleManager.Library;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	///     Basic form task to open a form using the context selection list.
	/// </summary>
	[SampleManagerTask("DefaultModalFormTask")]
	public class DefaultModalFormTask : DefaultFormTask
	{
		#region Overrides

		/// <summary>
		///     Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			var entity = GetEntity();

			TaskForm = FormFactory.CreateForm(Context.TaskParameters[0], entity);

			TaskForm.Created += FormCreated;
			TaskForm.Loaded += FormLoaded;
			TaskForm.Closed += (s, e) => { Exit(); };
			TaskForm.ShowDialog();
		}

		#endregion
	}
}