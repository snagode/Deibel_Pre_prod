using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Form used to build an entity.
	/// </summary>
	/// <remarks>
	/// TaskParameters expected are:
	///	[0] Form Name
	/// [1] Entity type
	/// [2] New name
	/// </remarks>
	[SampleManagerTask("BuildEntityTask")]
	public class BuildEntityTask : DefaultPromptFormTask
	{
		#region Member Variables

		private FormBuild m_Form;
		private string m_Identity;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormBuild) MainForm;
		}

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			base.MainFormLoaded();

			// Get the type of entity being built

			string entityType = Context.TaskParameters[1];
			m_Identity = Context.TaskParameters[2];

			// Setup prompts

			m_Form.CopyEntityPrompt.Browse = BrowseFactory.CreateEntityBrowse(entityType);
		}

		/// <summary>
		/// Called after the task's return value has been generated.
		/// </summary>
		protected override void OnAfterGenerateReturnValue()
		{
			Dictionary<string, object> promptValues = Context.ReturnValue as Dictionary<string, object>;
			if (promptValues != null)
			{
				promptValues.Add("Identity", m_Identity);
			}
		}

		#endregion
	}
}