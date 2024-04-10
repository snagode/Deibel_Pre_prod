using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Generic User Prompt Task. A task which contains a collection of prompts
	/// and passes the values of the prompts back to the caller.
	/// </summary>
	[SampleManagerTask("GenericUserPromptTask", "USERPROMPT")]
	public class GenericUserPromptTask : SampleManagerTask
	{
		#region Member Variables

		/// <summary>
		/// The default name of the prompt form
		/// </summary>
		private const string UserPromptForm = "Criteria User Prompt Template";

		/// <summary>
		/// The form name of the user prompts.
		/// </summary>
		private string m_FormName;

		private string m_FormTitle;

		/// <summary>
		/// List of properties needed to prompt for.
		/// </summary>
		private List<string> m_PromptCaptions;
		private List<string> m_PromptProperties;

		private List<string> m_PromptsExpected;

		/// <summary>
		/// An array of return values retrieved from the prompts.
		/// </summary>
		protected ArrayList returnValues;

		/// <summary>
		/// All Prompts must be filled in
		/// </summary>
		protected bool m_Mandatory;

		/// <summary>
		/// Promtp must be completed with a valid value
		/// </summary>
		protected bool m_ForceValid;

        private Form m_Form;
        private IEntity m_Entity;

		#endregion

		#region Setup

		/// <summary>
		/// Setup the SampleManager LTE task
		/// </summary>
		protected override void SetupTask()
		{
			CreateUserPromptForm();
		}

		/// <summary>
		/// Creates the form to prompt for values. If the first task paramter is specified that
		/// specific form is used. If empty the default template is used. The second parameter is an override
		/// for the form title and the remaining parameters are expected to be properties to prompt for.
		/// </summary>
		public void CreateUserPromptForm()
		{
			// Example parameter should look like 
			//'FormName,FormTitle,PropertyName:VariableName:LabelForPrompt,PropertyName:VariableName:LabelForPrompt,etc...'

			returnValues = new ArrayList();

			if (Context.TaskParameters.Length > 0)
			{
				m_FormName = Context.TaskParameters[0];
			}

			if (Context.TaskParameters.Length > 1)
			{
				m_FormTitle = Context.TaskParameters[1];
			}

			if (Context.TaskParameters.Length > 2)
			{
				m_PromptProperties = new List<string>();
				m_PromptCaptions = new List<string>();
				m_PromptsExpected = new List<string>();
				for (int propertyCount = 2; propertyCount < Context.TaskParameters.Length; propertyCount++)
				{
					string parameter = Context.TaskParameters[propertyCount];
					string[] propertyAndPrompt = parameter.Split(':');

					if (propertyAndPrompt.Length > 1 && !string.IsNullOrEmpty(propertyAndPrompt[1]))
					{
						m_PromptsExpected.Add(propertyAndPrompt[1]);
					}
					m_PromptProperties.Add(propertyAndPrompt[0]);

					string name = Library.Message.ConvertTaggedField(propertyAndPrompt[2]);
					m_PromptCaptions.Add(name);
				}
			}		

			// Create an entity just to base the form on, this will not be saved.

			if (!string.IsNullOrEmpty(Context.EntityType))
			{
				m_Entity = EntityManager.CreateEntity(Context.EntityType);
			}

			if (m_FormName == string.Empty || m_FormName == UserPromptForm)
			{
				string template =
					Library.Utils.Library.Environment.GetGlobalString("CRITERIA_USER_PROMPT_TEMPLATE") ??
					UserPromptForm;
				m_FormName = template;

                m_Form = FormFactory.CreatePromptForm(m_FormName, m_PromptCaptions, m_PromptProperties, m_Entity, m_Mandatory, m_ForceValid);
			}
			else
			{
                m_Form = FormFactory.CreateForm(m_FormName, m_Entity);
			}

            m_Form.Loaded += MainForm_Loaded;
            m_Form.Show();
            m_Form.Closing += form_Closing;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the main form of the labtable.
		/// </summary>
		/// <value>The main form.</value>
		protected Form MainForm
		{
			get { return FormFactory[m_FormName]; }
		}

		#endregion

		#region Events

		/// <summary>
		/// Handles the Loaded event of the MainForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void MainForm_Loaded(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(m_FormTitle))
            {
                MainForm.Title = m_FormTitle;
            }

            // Update values so data is save when enter is pressed.
            foreach (ClientProxyControl control in m_Form.Controls)
            {
                if (control is TextEdit)
                {
                    ((TextEdit)control).EditValueChanged += new EventHandler<TextChangedEventArgs>(GenericUserPromptTask_EditValueChanged);
                }
            }


            TaskLoaded();
        }

        /// <summary>
        /// As the controls are not bound update the text values.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GenericUserPromptTask_EditValueChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextEdit)
            {
                ((TextEdit)sender).Text = e.Text;
            }
        }

		/// <summary>
		/// Tasked Loaded method
		/// </summary>
		protected virtual void TaskLoaded()
		{
			// Nothing at this level
		}

		/// <summary>
		/// Handles the Closing event of the form control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		protected virtual void form_Closing(object sender, CancelEventArgs e)
		{
			Form form = (Form)sender;
			if (form.FormResult == FormResult.OK)
			{
				// We need to pass the values back in the same order expected at the start
				// Therefore step through the prompts and try and find the value from the form.


				if (m_PromptsExpected.Count != 0)
				{
					foreach (string prompt in m_PromptsExpected)
					{
						foreach (ClientProxyControl control in form.Controls)
						{
							if (control is Prompt)
							{
								if (control.Name == prompt)
								{
									if (control is IValueForQuery)
										returnValues.Add(((IValueForQuery)control).ValueForQuery);
								}
							}
						}
					}
				}
				else
				{
					foreach (ClientProxyControl control in form.Controls)
					{
						if (control is Prompt)
						{
							if (control is IValueForQuery)
								returnValues.Add(((IValueForQuery)control).ValueForQuery);
						}
					}
				}
				Context.ReturnValue = returnValues;
			}
			else
				Context.ReturnValue = null;
		}
		#endregion
	}
}