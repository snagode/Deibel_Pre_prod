using System;
using System.Globalization;
using System.IO;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.ClientControls.Validation;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Library.ObjectModel;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of the Label LTE
	/// </summary>
	[SampleManagerTask("LabelTemplateTask", "LABTABLE", "LABEL_TEMPLATE")]
	public class LabelTemplateTask : GenericLabtableTask
	{
		#region Member Variables

		private bool m_DesignClicked;
		private FormLabelTemplate m_FormLabelTemplate;
		private LabelTemplate m_LabelTemplate;
		private string m_TempTemplate;

		#endregion

		#region Overridden Methods

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_LabelTemplate = (LabelTemplate) MainForm.Entity;
			m_FormLabelTemplate = (FormLabelTemplate) MainForm;

			m_FormLabelTemplate.PrintTestButton.Click += PrintTestButtonClick;
			m_FormLabelTemplate.DesignButton.Click += DesignButtonClick;

			m_FormLabelTemplate.PromptEntityBrowseTest.EntityChanged += PromptEntityBrowseTestEntityChanged;
			m_LabelTemplate.PropertyChanged += LabelTemplatePropertyChanged;
			m_FormLabelTemplate.DesignValidator.Validate += DesignValidatorValidate;
		}

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			if (Context.LaunchMode == CopyOption)
			{
				m_TempTemplate = CreateClientTempFiles(CopiedEntity.Identity);
			}

			if (Library.Environment.GetGlobalInt("CLIENT_TYPE") == 1)
			{
				m_FormLabelTemplate.LabelLine.Visible = false;
				m_FormLabelTemplate.LabelPicture.Visible = false;
				m_FormLabelTemplate.Label2.Visible = false;
				m_FormLabelTemplate.DesignClicked.Visible = false;
				m_FormLabelTemplate.DesignButton.Visible = false;
			}
			else
			{
				EnableDesignButton();
			}

			EnablePrintButton();
			SetupTestDataBrowse();

			m_FormLabelTemplate.promptTableName.ReadOnly = !(Context.LaunchMode == AddOption || Context.LaunchMode == TestOption);
		}

		#endregion

		#region Button Events

		/// <summary>
		/// Handles the Click event of the m_DesignButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void DesignButtonClick(object sender, EventArgs e)
		{
			DesignLabel();

			Library.Task.StateModified();

			m_LabelTemplate.ModifiedOn = Library.Environment.ClientNow;
			m_FormLabelTemplate.promptTableName.ReadOnly = true;
			m_DesignClicked = true;
		}

		/// <summary>
		/// Handles the Click event of the testButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void PrintTestButtonClick(object sender, EventArgs e)
		{
			PrintLabel();
		}

		/// <summary>
		/// Prints the label.
		/// </summary>
		private void PrintLabel()
		{
			if (m_FormLabelTemplate.PromptEntityBrowseTest.Entity == null) return;

			if (string.IsNullOrEmpty(m_TempTemplate))
			{
				Library.Utils.PrintLabel(m_LabelTemplate, m_FormLabelTemplate.PromptEntityBrowseTest.Entity);
			}
			else
			{
				Library.Utils.PrintLabel(m_LabelTemplate, m_TempTemplate, m_FormLabelTemplate.PromptEntityBrowseTest.Entity);
			}
		}

		/// <summary>
		/// Designs the label.
		/// </summary>
		private void DesignLabel()
		{
			if (m_LabelTemplate == null) return;

			if (string.IsNullOrEmpty(m_TempTemplate))
			{
				m_TempTemplate = CreateClientTempFiles(m_LabelTemplate.Identity);
			}

			Library.Utils.DesignLabel(m_LabelTemplate, m_TempTemplate);
		}

		/// <summary>
		/// Make sure a design is present
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The <see cref="Thermo.SampleManager.Library.ClientControls.Validation.ServerValidatorEventArgs"/> instance containing the event data.</param>
		private void DesignValidatorValidate(Object sender, ServerValidatorEventArgs args)
		{
			args.Valid = (m_DesignClicked || Context.LaunchMode != AddOption);

			if (!args.Valid)
			{
				args.ErrorMessage = m_FormLabelTemplate.StringTable.MissingDesignMessage;

				Library.Utils.FlashMessage(args.ErrorMessage, m_FormLabelTemplate.StringTable.MissingDesignCaption,
				                           MessageButtons.OK, MessageIcon.Exclamation, MessageDefaultButton.Button1);
			}
		}

		#endregion

		#region Value Changing Events

		/// <summary>
		/// Labels the template property changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.PropertyEventArgs"/> instance containing the event data.</param>
		private void LabelTemplatePropertyChanged(object sender, PropertyEventArgs e)
		{
			if (e.PropertyName == LabelTemplatePropertyNames.DataEntityDefinition ||
			    e.PropertyName == LabelTemplatePropertyNames.Identity)
			{
				DefinitionChanged();
			}
		}

		/// <summary>
		/// Definition Changed
		/// </summary>
		private void DefinitionChanged()
		{
			EnableDesignButton();
			SetupTestDataBrowse();

			m_FormLabelTemplate.PromptEntityBrowseTest.Entity = null;
		}

		/// <summary>
		/// Test Entity Changed
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.EntityChangedEventArgs"/> instance containing the event data.</param>
		private void PromptEntityBrowseTestEntityChanged(object sender, EntityChangedEventArgs e)
		{
			EnablePrintButton();
		}

		#endregion

		#region Browses

		/// <summary>
		/// Sets the browse by label property.
		/// </summary>
		private void SetupTestDataBrowse()
		{
			if (m_LabelTemplate == null) return;
			if (string.IsNullOrEmpty(m_LabelTemplate.DataEntityDefinition)) return;

			m_FormLabelTemplate.EntityBrowseTest.Republish(m_LabelTemplate.DataEntityDefinition);
		}

		#endregion

		#region Button Control

		/// <summary>
		/// Enables the design button.
		/// </summary>
		private void EnableDesignButton()
		{
			// Display mode - no designing

			if (Context.LaunchMode == DisplayOption || Library.Environment.GetGlobalInt("CLIENT_TYPE") == 1)
			{
				m_FormLabelTemplate.DesignButton.Enabled = false;
				return;
			}

			// No Entity/Identity

			string entityType = m_LabelTemplate.DataEntityDefinition;
			string identity = m_LabelTemplate.Identity;

			if (string.IsNullOrEmpty(entityType) || string.IsNullOrEmpty(identity))
			{
				m_FormLabelTemplate.DesignButton.Enabled = false;
				return;
			}

			// Everything is good, allow edit.

			m_FormLabelTemplate.DesignButton.Enabled = true;
		}

		/// <summary>
		/// Enables the print button.
		/// </summary>
		private void EnablePrintButton()
		{
			bool entitySelected = m_FormLabelTemplate.PromptEntityBrowseTest.Entity != null;
			bool enabled = entitySelected && (m_DesignClicked || Context.LaunchMode != AddOption);
			m_FormLabelTemplate.PrintTestButton.Enabled = enabled;
		}

		#endregion

		#region Commit

		/// <summary>
		/// Called after the property sheet or wizard is saved.
		/// </summary>
		protected override void OnPostSave()
		{
			if (!string.IsNullOrEmpty(m_TempTemplate))
			{
				TransferClientFile(LabelTemplateInternal.LabelFileExtension);
				TransferClientFile(LabelTemplateInternal.LabelPrintExtension);

				DeleteTemporaryFiles();
			}

			base.OnPostSave();
		}

		#endregion

		#region File Management

		/// <summary>
		/// Creates the client temp files.
		/// </summary>
		/// <param name="template">The template.</param>
		/// <returns></returns>
		private string CreateClientTempFiles(string template)
		{
			CreateTempClientFile(template, LabelTemplateInternal.LabelPrintExtension);
			return CreateTempClientFile(template, LabelTemplateInternal.LabelFileExtension);
		}

		/// <summary>
		/// Creates the temp file.
		/// </summary>
		/// <returns></returns>
		public string CreateTempClientFile(string template, string extension)
		{
			string fileName = Path.ChangeExtension(template, extension);
			FileInfo fileInfo = Library.File.GetWriteFile("smp$resource", LabelTemplateInternal.LabelFolder, fileName);
			if (!fileInfo.Exists) return template;

			// Create Temp File

			string stamp = Library.Environment.ClientNow.ToDateTime(CultureInfo.CurrentCulture).ToString("ddMMyyHHmmss");
			string tempFile = string.Format("_{0}_{1}", stamp, fileName);

			string clientFile = Path.Combine(LabelTemplateInternal.LabelClientFolder, tempFile);
			Library.File.TransferToClient(fileInfo.FullName, clientFile);

			return tempFile;
		}

		/// <summary>
		/// Transfers the client.
		/// </summary>
		protected void TransferClientFile(string extension)
		{
			string tempFile = Path.Combine(LabelTemplateInternal.LabelClientFolder, m_TempTemplate);
			tempFile = Path.ChangeExtension(tempFile, extension);
			string fileName = Path.ChangeExtension(m_LabelTemplate.Identity, extension);

			// Put it in the Server Resources

			FileInfo fileInfo = Library.File.GetWriteFile("smp$resource", LabelTemplateInternal.LabelFolder, fileName);
			Library.File.TransferFromClient(tempFile, fileInfo.FullName);
			if (!fileInfo.Exists) return;

			// Make it available to this client immediately without refreshing the server resources.

			string clientFile = Path.Combine(LabelTemplateInternal.LabelClientFolder, fileName);
			Library.File.TransferToClient(fileInfo.FullName, clientFile);
		}

		/// <summary>
		/// Deletes the temporary files.
		/// </summary>
		private void DeleteTemporaryFiles()
		{
			DeleteTemporaryFile(LabelTemplateInternal.LabelPrintExtension);
			DeleteTemporaryFile(LabelTemplateInternal.LabelFileExtension);
			DeleteTemporaryFile(LabelTemplateInternal.LabelValueExtension);
			DeleteTemporaryFile(LabelTemplateInternal.LabelTempExtension);

			m_TempTemplate = null;
		}

		/// <summary>
		/// Deletes the temporary file.
		/// </summary>
		/// <param name="extension">The extension.</param>
		private void DeleteTemporaryFile(string extension)
		{
			string tempFile = Path.Combine(LabelTemplateInternal.LabelClientFolder, m_TempTemplate);
			tempFile = Path.ChangeExtension(tempFile, extension);
			Library.File.DeleteFileFromClient(tempFile);
		}

		#endregion
	}
}