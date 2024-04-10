using Thermo.SampleManager.Library;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Generic User Prompt Task. A task which contains a collection of prompts
	/// and passes the values of the prompts back to the caller.
	/// </summary>
	[SampleManagerTask("FormulaEditorTask")]
	public class FormulaEditorTask : SampleManagerTask
	{
		#region Member Variables

		private string m_ReturnValue;
		private string m_InputValue = null;

		#endregion

		#region Setup

		/// <summary>
		/// Setup the SampleManager LTE task
		/// </summary>
		protected override void SetupTask()
		{
			// Get the Input

			m_InputValue = Context.TaskParameterString;

			// Launch the Formula Editor
			m_ReturnValue = Library.Formula.EditFormula(Context.EntityType, m_InputValue);

			// Return the Formula to the caller
			Exit(m_ReturnValue);
		}

		#endregion
	}
}