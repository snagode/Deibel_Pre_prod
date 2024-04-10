using Thermo.SampleManager.Library;
using Thermo.SampleManager.Common.Data;

namespace Thermo.SampleManager.Tasks
{

    /// <summary>
    /// Task to launch the formula editor task, with the ability to browse on records.
    /// </summary>
    [SampleManagerTask("FormulaEditorTableTask")]
	public class FormulaEditorTableTask : SampleManagerTask
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
            string tableName = string.Empty;
            string criteria = string.Empty;

            string[] taskParameters = Context.TaskParameterString.Split(';');
            if (taskParameters.Length == 3)
            {
                if (Library.Schema.Tables.Contains(taskParameters[1]))
                {
                    m_InputValue = taskParameters[0];
                    tableName = taskParameters[1];
                    criteria = taskParameters[2];
                }
            }          
            
			// Launch the Formula Editor
            m_ReturnValue = Library.Formula.EditFormula(Context.EntityType, m_InputValue, tableName, criteria);

			// Return the Formula to the caller
			Exit(m_ReturnValue);
		}

		#endregion
	}
}
