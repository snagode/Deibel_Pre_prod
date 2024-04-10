using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.FormDefinition;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Generic User Prompt Task. A task which contains a collection of prompts
	/// and passes the values of the prompts back to the caller.
	/// </summary>
	[SampleManagerTask("FormulaTask")]
	public class FormulaTask : DefaultFormTask
	{

		protected override void MainFormCreated()
		{
			base.MainFormCreated();
			FormFormula form = (FormFormula)MainForm;
			form.ButtonEdit1.Click += new EventHandler(ButtonEdit1_Click);
		}

		void ButtonEdit1_Click(object sender, EventArgs e)
		{
			string tableName = ((FormFormula)MainForm).PromptTableNameBrowse1.TableName;
			object returnFormula = Library.Formula.EditFormula(tableName,((FormFormula)MainForm).Formula.Text);
			if (returnFormula != null)
			{
				((FormFormula)MainForm).Formula.Text = (string)returnFormula;
			}
		}
	}
}