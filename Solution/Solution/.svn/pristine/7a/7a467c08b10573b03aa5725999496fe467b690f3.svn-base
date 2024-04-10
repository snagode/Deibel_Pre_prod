using System.Collections.Generic;
using System.ComponentModel;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Basic form task to open a form using the context selection list.
	/// </summary>
	[SampleManagerTask("DefaultPromptFormTask")]
	public class DefaultPromptFormTask : DefaultFormTask
	{
		#region Overrides

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			MainForm.Closing += MainFormClosing;
		}

		/// <summary>
		/// Handles the Closing event of the form control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		protected virtual void MainFormClosing(object sender, CancelEventArgs e)
		{
			if (MainForm.FormResult == FormResult.OK)
			{
				// Pass back all prompt values in a dictionary
				Dictionary<string, object> promptValues = new Dictionary<string, object>();

				foreach (ClientProxyControl control in MainForm.Controls)
				{
					if (control is Prompt)
					{
						promptValues.Add(control.Name, ((Prompt) control).ValueForQuery);
					}
				}

				Context.ReturnValue = promptValues;

				OnAfterGenerateReturnValue();
			}
			else
			{
				Context.ReturnValue = null;
			}
		}

		/// <summary>
		/// Called after the task's return value has been generated.
		/// </summary>
		protected virtual void OnAfterGenerateReturnValue()
		{
			// Nothing at this level
		}

		#endregion

	}
}