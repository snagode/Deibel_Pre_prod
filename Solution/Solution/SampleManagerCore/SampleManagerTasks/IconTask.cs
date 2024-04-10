using System;
using System.IO;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of the Icon LTE
	/// </summary>
	[SampleManagerTask("IconTask", "LABTABLE", "ICON")]
	public class IconTask : GenericLabtableTask
	{
		#region Member Variables

		private string m_ClientIconFile;
		private FormIcon m_Form;
		private Icon m_Icon;
		IIconService m_IconService;

		#endregion

		#region Overridden Methods

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			m_Form = (FormIcon) MainForm;
			m_Icon = (Icon) MainForm.Entity;
			m_IconService = Library.GetService<IIconService>();

			if (Context.LaunchMode == DisplayOption)
			{
				// Make sure you can't press the choose icon button.

				m_Form.ChooseIcon.Enabled = false;
			}
			else
			{
				if (!m_Icon.Modifiable)
				{
					// This is an internal icon, allow it to change type

					m_Form.IconSource.ReadOnly = false;
				}

				m_Form.ChooseIcon.Click += ChooseIconClick;

				// See if we are dealing with old icons.

				if (Context.LaunchMode == ModifyOption && ConsiderMigratingResource())
				{
					if (Library.Utils.FlashMessageYesNo(m_Form.StringTable.TitleMigrateIcon, 
					                                    m_Form.StringTable.MessageMigrateIcon))
					{
						MigrateResource();
						return;
					}
				}
			}

			// Update the images to show the different icon sizes

			m_Form.PreviewIcon48.SetImageByIconName(new IconName(m_Icon.Identity));
			m_Form.PreviewIcon32.SetImageByIconName(new IconName(m_Icon.Identity));
			m_Form.PreviewIcon24.SetImageByIconName(new IconName(m_Icon.Identity));
			m_Form.PreviewIcon16.SetImageByIconName(new IconName(m_Icon.Identity));
		}

		#endregion

		#region Dealing with Old Icons

		/// <summary>
		/// Considers migrating the resource to the new name.
		/// </summary>
		private bool ConsiderMigratingResource()
		{
			if (string.IsNullOrEmpty(m_Icon.IconResource)) return false;

			string properName = GetIconFileName();

			if (m_Icon.IconResource == properName) return false;
			return File.Exists(m_Icon.IconResource);
		}

		/// <summary>
		/// Migrate the resource to the new name.
		/// </summary>
		private void MigrateResource()
		{
			if (string.IsNullOrEmpty(m_Icon.IconResource)) return;

			string properName = GetIconFileName();

			if (m_Icon.IconResource == properName) return;
			if (!File.Exists(m_Icon.IconResource)) return;

			string serverName = GetServerFileName();
			string clientName = GetClientFileName();

			// Copy the file to where it should be

			if (!File.Exists(serverName))
			{
				try
				{
					File.Copy(m_Icon.IconResource, serverName);
				}
				catch (Exception e)
				{
					Logger.Info("Unable to migrate icon file", e);
				}
			}

			// Send it to the client

			Library.File.TransferToClient(serverName, clientName);

			Library.Utils.ShowAlert(m_Form.StringTable.TitleMigratedIcon, "INT_IMPORT_SUCCESS",
									m_Form.StringTable.MessageMigratedIcon);

			SetClientFileIcon(clientName);
		}

		#endregion

		#region Choose Icons

		/// <summary>
		/// Choose Icon Button Click Handler
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void ChooseIconClick(object sender, EventArgs e)
		{
			string fileName = Library.Utils.PromptForFile(m_Form.StringTable.TitleChooseIcon,
			                                              m_Form.StringTable.MessageChooseIcon);

			if (!string.IsNullOrEmpty(fileName))
			{
				m_ClientIconFile = fileName;
				SetClientFileIcon(fileName);
			}
		}

		/// <summary>
		/// Sets the client file icon.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		private void SetClientFileIcon(string fileName)
		{
			m_Form.PreviewIcon48.SetImageByClientFile(fileName);
			m_Form.PreviewIcon32.SetImageByClientFile(fileName);
			m_Form.PreviewIcon24.SetImageByClientFile(fileName);
			m_Form.PreviewIcon16.SetImageByClientFile(fileName);

			m_Icon.SetIconSource(PhraseIconsource.PhraseIdRESOURCE);
			m_Icon.IconResource = GetIconFileName();

			Library.Task.StateModified();

			m_IconService.ResetIcon(new IconName(m_Icon.Identity));
		}

		#endregion

		#region Icon File Transfer

		/// <summary>
		/// Called after the property sheet or wizard is saved.
		/// </summary>
		protected override void OnPostSave()
		{
			if (!string.IsNullOrEmpty(m_ClientIconFile))
			{
				TransferIconFile();
			}

			// If we've set the icon type to internal, delete the resource file.

			if (m_Icon.IconSource.IsPhrase(PhraseIconsource.PhraseIdINTERNAL))
			{
				ResetIconToInternal();
			}

			// Reset the icon service.

			m_IconService.ResetIcon(new IconName(m_Icon.Identity));
			base.OnPostSave();
		}

		/// <summary>
		/// Resets the icon to internal
		/// </summary>
		private void ResetIconToInternal()
		{
			string serverFileName = GetServerFileName();

			if (File.Exists(serverFileName))
			{
				try
				{
					File.Delete(serverFileName);
					string clientFileName = GetClientFileName();
					Library.File.DeleteFileFromClient(clientFileName);
				}
				catch (Exception e)
				{
					Logger.Info("Unable to delete icon information", e);
				}
			}
		}

		/// <summary>
		/// Gets the name of the file.
		/// </summary>
		/// <returns></returns>
		private string GetIconFileName()
		{
			return Path.ChangeExtension(m_Icon.Identity, ".ico");
		}

		/// <summary>
		/// Transfers the icon file.
		/// </summary>
		private void TransferIconFile()
		{
			// Put it in the Server Resources

			string serverFullName = GetServerFileName();

			Library.File.TransferFromClient(m_ClientIconFile, serverFullName);

			// Make it available to this client immediately without refreshing the server resources.

			string clientFile = GetClientFileName();

			Library.File.TransferToClient(serverFullName, clientFile);

			Library.Utils.ShowAlert(m_Form.StringTable.TitleIconUploaded, "INT_IMPORT_SUCCESS",
			                        m_Form.StringTable.MessageIconUploaded);
		}

		/// <summary>
		/// Gets the name of the client file.
		/// </summary>
		/// <returns></returns>
		private string GetClientFileName()
		{
			string fileName = GetIconFileName();
			return Path.Combine(@"Resource\Icon\", fileName);
		}

		/// <summary>
		/// Gets the name of the server file.
		/// </summary>
		/// <returns></returns>
		private string GetServerFileName()
		{
			string fileName = GetIconFileName();
			FileInfo fileInfo = Library.File.GetWriteFile("smp$resource", "Icon", fileName);
			return fileInfo.FullName;
		}

		#endregion
	}
}