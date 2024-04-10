using System;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Syntax Task
	/// </summary>
	[SampleManagerTask("SyntaxTask", "LABTABLE", "SYNTAX")]
	public class SyntaxTask : GenericLabtableTask
	{
		#region Member Variables

		private FormSyntax m_Form;
		private Syntax m_Entity;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormSyntax)MainForm;
			m_Entity = (Syntax)MainForm.Entity;

			m_Form.RadioButtonFormula.CheckedChanged += RadioButtonFormulaCheckedChanged;
			m_Form.RadioButtonLiteral.CheckedChanged += RadioButtonLiteralCheckedChanged;
			m_Form.RadioButtonUserWritten.CheckedChanged += RadioButtonUserWrittenCheckedChanged;
			m_Form.TableName.TableNameChanged += TableNameTableNameChanged;
			m_Form.ButtonEditTest.Click += ButtonEditTestClick;
			m_Form.Formula.ButtonClick += FormulaButtonClick;

			if (m_Entity.Type == Syntax.SyntaxType.Formula)
			{
				m_Form.RadioButtonFormula.Checked = true;
			}
			else if (m_Entity.Type == Syntax.SyntaxType.Literal)
			{
				m_Form.RadioButtonLiteral.Checked = true;
			}
			else
			{
				m_Form.RadioButtonUserWritten.Checked = true;
			}
		}

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm" /> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			SetTestButton();

			base.MainFormLoaded();
		}

		/// <summary>
		/// Called before the property sheet or wizard is saved.
		/// </summary>
		/// <returns>
		/// true to allow the save to continue, false to abort the save.
		/// Please also ensure that you call the base.OnPreSave when continuing
		/// successfully.
		/// </returns>
		protected override bool OnPreSave()
		{
			if (!string.IsNullOrEmpty(m_Entity.Syntax) && !m_Entity.UserWritten)
			{
				object returnValue = Library.VGL.RunVGLRoutine("$SYNTAX",
															   "convert_syntax_string",
															   new object[] { m_Entity.Identity, m_Entity.Syntax });

				if ((returnValue is string) && (!string.IsNullOrWhiteSpace((string)returnValue)))
				{
					Library.Utils.FlashMessage((string)returnValue, "Syntax Conversion Error");
					return false;
				}
			}

			return base.OnPreSave();
		}

		#endregion

		#region Events

		/// <summary>
		/// Handles the Click event of the ButtonEditTest control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void ButtonEditTestClick(object sender, EventArgs e)
		{
			bool readyToTest = true;

			if (m_Form.PromptEntityBrowseTest.Entity == null || m_Form.PromptEntityBrowseTest.Entity.IsNull())
			{
				readyToTest = Library.Utils.FlashMessageYesNo(m_Form.StringTableSyntax.NoEntity,
															  m_Form.StringTableSyntax.NoEntityTitle);
			}

			if (readyToTest)
			{
				try
				{
					if (m_Entity.Type == Syntax.SyntaxType.Formula)
					{
						var entity = m_Form.PromptEntityBrowseTest.Entity;
						object val = Library.Formula.Test(entity, m_Entity.Formula, m_Entity.Identity);
						if (val == null) m_Form.TextEditTest.Text = string.Empty;
						else m_Form.TextEditTest.Text = val.ToString();
					}
					else
					{
						m_Form.TextEditTest.Text = Library.Syntax.GenerateSyntax(m_Entity, m_Form.PromptEntityBrowseTest.Entity);
					}
				}
				catch (Exception exception)
				{
					Library.Utils.ShowAlert(exception.Message);
				}
			}
		}

		/// <summary>
		/// Handles the TableNameChanged event of the TableName control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.TextChangedEventArgs"/> instance containing the event data.</param>
		void TableNameTableNameChanged(object sender, TextChangedEventArgs e)
		{
			string table = m_Form.TableName.TableName;

			if (!string.IsNullOrEmpty(table))
			{
				m_Form.EntityBrowseTest.Republish(table);
			}

			m_Form.TextEditTest.Text = string.Empty;
		}

		/// <summary>
		/// Handles the ButtonClick event of the Formula control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		void FormulaButtonClick(object sender, EventArgs e)
		{
			m_Entity.Formula = Library.Formula.EditFormula(m_Entity.TableName, m_Entity.Formula);
		}

		/// <summary>
		/// Handles the CheckedChanged event of the RadioButtonUserWritten control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.CheckedChangedEventArgs"/> instance containing the event data.</param>
		void RadioButtonUserWrittenCheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			m_Form.RadioButtonUserWritten.Checked = e.Checked;
			EnableUserWritten(e.Checked);
		}

		/// <summary>
		/// Handles the CheckedChanged event of the RadioButtonLiteral control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.CheckedChangedEventArgs"/> instance containing the event data.</param>
		void RadioButtonLiteralCheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			m_Form.RadioButtonLiteral.Checked = e.Checked;
			EnableLiteral(e.Checked);
		}

		/// <summary>
		/// Handles the CheckedChanged event of the RadioButtonFormula control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.CheckedChangedEventArgs"/> instance containing the event data.</param>
		void RadioButtonFormulaCheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			m_Form.RadioButtonFormula.Checked = e.Checked;
			EnableFormula(e.Checked);
		}

		/// <summary>
		/// Enables the formula.
		/// </summary>
		void EnableFormula(bool enabled)
		{
			if (enabled)
			{
				m_Form.Formula.Enabled = true;
				m_Form.Literal.Enabled = false;

				m_Entity.Syntax = string.Empty;

				SetTestButton();
			}
		}

		/// <summary>
		/// Enables the literal.
		/// </summary>
		void EnableLiteral(bool enabled)
		{
			if (enabled)
			{
				m_Form.Formula.Enabled = false;
				m_Form.Literal.Enabled = true;

				m_Entity.UserWritten = false;
				m_Entity.Formula = string.Empty;

				SetTestButton();
			}
		}

		/// <summary>
		/// Enables the user written.
		/// </summary>
		void EnableUserWritten(bool enabled)
		{
			if (enabled)
			{
				m_Form.Formula.Enabled = false;
				m_Form.Literal.Enabled = false;

				m_Entity.UserWritten = true;
				m_Entity.Formula = string.Empty;
				m_Entity.Syntax = string.Empty;

				SetTestButton();
			}
		}

		/// <summary>
		/// Sets the test button.
		/// </summary>
		private void SetTestButton()
		{
			if (Context.LaunchMode == DisplayOption)
				m_Form.ButtonEditTest.Enabled = false;
			else if (m_Entity.IsNew() &&
					 (m_Form.RadioButtonLiteral.Checked ||
					  m_Form.RadioButtonUserWritten.Checked))
				m_Form.ButtonEditTest.Enabled = false;
			else
				m_Form.ButtonEditTest.Enabled = true;
		}

		#endregion
	}
}
