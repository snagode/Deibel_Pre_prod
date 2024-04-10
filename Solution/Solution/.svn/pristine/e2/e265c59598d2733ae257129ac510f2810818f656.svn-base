using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	///     NamedLicenseGroupEditorTask
	/// </summary>
	[SampleManagerTask("NamedLicenseGroupEditorTask")]
	public class NamedLicenseGroupEditorTask : DefaultFormTask
	{
		#region Utilities

		/// <summary>
		///     Tests the writable.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <returns></returns>
		private bool TestWritable(string fileName)
		{
			try
			{
				// Attempt to get a list of security permissions from the folder. 
				// Will fail if path is readonly or no permissions

				var fileInfo = new FileInfo(fileName);

				if (fileInfo.Directory != null)
				{
					// ReSharper disable once UnusedVariable
					var ds = Directory.GetAccessControl(fileInfo.Directory.ToString());
				}

				if (fileInfo.Exists)
				{
					if (fileInfo.IsReadOnly)
					{
						throw new Exception();
					}

					// ReSharper disable once UnusedVariable
					var fs = fileInfo.GetAccessControl();
				}

				return true;
			}
			catch
			{
				return false;
			}
		}

		#endregion

		#region Structured class for options file management

		/// <summary>
		/// </summary>
		private class OptionsFileItem
		{
			/// <summary>
			///     Initializes a new instance of the <see cref="OptionsFileItem" /> class.
			/// </summary>
			/// <param name="identity">The identity.</param>
			/// <param name="groupName">Name of the group.</param>
			public OptionsFileItem(string identity, string groupName)
			{
				Identity = identity;
				GroupName = groupName;
				UsersInGroup = new List<string>();
				IsNamed = groupName.ToLower().EndsWith("named");
			}

			/// <summary>
			///     Gets or sets the identity.
			/// </summary>
			/// <value>
			///     The identity.
			/// </value>
			public string Identity { get; set; }

			/// <summary>
			///     Gets or sets the text.
			/// </summary>
			/// <value>
			///     The text.
			/// </value>
			public string GroupName { get; private set; }

			/// <summary>
			///     Gets or sets the count.
			/// </summary>
			/// <value>
			///     The count.
			/// </value>
			public int Count { get; set; }

			/// <summary>
			///     Gets or sets the maximum.
			/// </summary>
			/// <value>
			///     The maximum.
			/// </value>
			public int Max { get; set; }

			/// <summary>
			///     Gets or sets the users in group.
			/// </summary>
			/// <value>
			///     The users in group.
			/// </value>
			public List<string> UsersInGroup { get; set; }

			/// <summary>
			///     Gets or sets a value indicating whether this instance is named.
			/// </summary>
			/// <value>
			///     <c>true</c> if this instance is named; otherwise, <c>false</c>.
			/// </value>
			public bool IsNamed { get; private set; }

			/// <summary>
			///     Gets a value indicating whether [exceeding maximum].
			/// </summary>
			/// <value>
			///     <c>true</c> if [exceeding maximum]; otherwise, <c>false</c>.
			/// </value>
			public bool ExceedingMax
			{
				get { return Count > Max; }
			}

			/// <summary>
			///     Resets the counts.
			/// </summary>
			public void Reset()
			{
				Count = 0;
				UsersInGroup.Clear();
			}

		}

		#endregion

		#region Features Enumerated

		/// <summary>
		///     LicenseFeatures
		/// </summary>
		private enum LicenseFeature
		{
			/// <summary>
			///     Fully featured
			/// </summary>
			// ReSharper disable once InconsistentNaming
			FF,

			/// <summary>
			///     Fully featured named
			/// </summary>
			// ReSharper disable once InconsistentNaming
			FFN,

			/// <summary>
			///     Read only
			/// </summary>
			// ReSharper disable once InconsistentNaming
			RO,

			/// <summary>
			///     Read only named
			/// </summary>
			// ReSharper disable once InconsistentNaming
			RON

		}

		/// <summary>
		///     License feature from string.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <returns></returns>
		private LicenseFeature LicenseFeatureFromString(string input)
		{
			return (LicenseFeature)Enum.Parse(typeof(LicenseFeature), input);
		}

		/// <summary>
		///     Translates the phrase identifier to license feature.
		///     Do not change these values
		/// </summary>
		/// <param name="phraseId">The phrase identifier.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentOutOfRangeException"></exception>
		private string TranslatePhraseIdToLicenseFeature(string phraseId)
		{
			switch (LicenseFeatureFromString(phraseId))
			{
				case LicenseFeature.FF:
					return "Full";
				case LicenseFeature.FFN:
					return "FullNamed";
				case LicenseFeature.RO:
					return "ReadOnly";
				case LicenseFeature.RON:
					return "ReadOnlyNamed";
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#endregion

		#region Constants

		private const string OptionsFileName = "THERMOCORP.opt";
		private const string OptionsFileFeaturePrefix = "SampleManager";
		private const string LicensePhrase = "LIC_TYPE";

		private const string LicenceFlexnetMainProductFullCount = "FF_COUNT";
		private const string LicenceFlexnetMainProductFullNamedCount = "FN_COUNT";
		private const string LicenceFlexnetMainProductReadonlyCount = "RO_COUNT";
		private const string LicenceFlexnetMainProductReadonlyNamedCount = "RON_COUNT";

		#endregion

		#region Private members

		private FormNamedLicenseGroupEditor m_Form;
		private Dictionary<LicenseFeature, OptionsFileItem> m_OptionsFileItems;
		private bool m_OptionsFileWritable;
		private string m_OptionsFilePath;
		private string m_OptionsFileDir;
		private string m_InstanceName;
		private bool m_UpdatingUi;

		private IEntityCollection m_PersonnelCollection;
		private readonly IDictionary<string, string> m_LicenseTypeDictionary= new Dictionary<string, string>();

		#endregion

		#region Private methods

		/// <summary>
		/// Loads the options file.
		/// </summary>
		private void LoadOptionsFile()
		{
			//load structure:
			m_OptionsFileItems = new Dictionary<LicenseFeature, OptionsFileItem>();
			foreach (var phraseEntryDefinition in Schema.Current.PhraseDefinitions[LicensePhrase].Entries)
			{
				var groupName = string.Format("{0}{1}{2}", m_InstanceName, OptionsFileFeaturePrefix, TranslatePhraseIdToLicenseFeature(phraseEntryDefinition.Identity)).Replace(" ", "").Replace("-", "");

				m_OptionsFileItems.Add(LicenseFeatureFromString(phraseEntryDefinition.Identity), new OptionsFileItem(phraseEntryDefinition.Identity,
					groupName));
			}

			//file check

			m_OptionsFileDir = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\LabSystems\SampleManager Server",
				"smp$license", @"C:\Program Files (x86)\Thermo\Thermo Licensing Server").ToString();

			m_OptionsFilePath = Path.Combine(m_OptionsFileDir, OptionsFileName);
			m_OptionsFileWritable = TestWritable(m_OptionsFilePath);

			if (m_OptionsFileWritable)
			{
				m_Form.FilePathLabel.Caption = string.Format(m_Form.GeneralMessages.ModifyingFile, m_OptionsFilePath);
			}
			else
			{
				m_Form.FilePathLabel.Caption = m_Form.GeneralMessages.OptionsNotWritable;
			}

			m_OptionsFileItems[LicenseFeature.FF].Max = m_Form.FFCount.Number = Library.Environment.GetGlobalInt(LicenceFlexnetMainProductFullCount);
			m_OptionsFileItems[LicenseFeature.FFN].Max = m_Form.FNCount.Number = Library.Environment.GetGlobalInt(LicenceFlexnetMainProductFullNamedCount);
			m_OptionsFileItems[LicenseFeature.RO].Max = m_Form.ROCount.Number = Library.Environment.GetGlobalInt(LicenceFlexnetMainProductReadonlyCount);
			m_OptionsFileItems[LicenseFeature.RON].Max = m_Form.RONCount.Number = Library.Environment.GetGlobalInt(LicenceFlexnetMainProductReadonlyNamedCount);

		}

		/// <summary>
		///     Setups the UI events.
		/// </summary>
		private void SetupUiEvents()
		{
			m_Form.OKVisibleButton.Click += (s, e) =>
			{
				if (FormOk())
				{
					Save();
					CloseForm();
				}
			};

			m_Form.CancelButton.Click += (s, e) => CloseForm();

			m_Form.FFCount.NumberChanged += SpinEditChanged;
			m_Form.FNCount.NumberChanged += SpinEditChanged;
			m_Form.ROCount.NumberChanged += SpinEditChanged;
			m_Form.RONCount.NumberChanged += SpinEditChanged;

		}

		private void PublishPersonnel()
		{
			IQuery query = EntityManager.CreateQuery(PersonnelBase.EntityName);

			query.HideRemoved();

			IEntityCollection personnelCollection = EntityManager.Select(PersonnelBase.EntityName, query);

			if (Library.Environment.GetGlobalBoolean("LOGIN_LDAP_NEEDSPASSWORD"))
			{
				m_PersonnelCollection = EntityManager.CreateEntityCollection(PersonnelBase.EntityName);

				foreach (var person in personnelCollection)
				{
					Personnel personnel = person as Personnel;

					if (personnel != null && personnel.HasPassword)
						m_PersonnelCollection.Add(personnel);
				}
			}
			else
			{
				m_PersonnelCollection = personnelCollection;
			}

			m_Form.PersonnelBrowse.Republish(m_PersonnelCollection);
		}

		private void SetContextMenu()
		{
			m_Form.PersonnelGrid.ShowDefaultMenu = false;

			foreach (var phraseEntryDefinition in Schema.Current.PhraseDefinitions[LicensePhrase].Entries)
			{
				m_LicenseTypeDictionary[phraseEntryDefinition.Text] = phraseEntryDefinition.Identity;

				ContextMenuItem contextMenuItem = new ContextMenuItem(phraseEntryDefinition.Text, null);
				contextMenuItem.ItemClicked += ContextMenuItemItemClicked;

				m_Form.PersonnelGrid.ContextMenu.AddItem(contextMenuItem);
			}
		}

		private void ContextMenuItemItemClicked(object sender, ContextMenuItemEventArgs e)
		{
			ContextMenuItem contextMenuItem = sender as ContextMenuItem;

			if (contextMenuItem == null) return;

			bool updateNeeded = false;
			string newLicensePhraseId = m_LicenseTypeDictionary[contextMenuItem.Caption];

			foreach (IEntity entity in e.EntityCollection.ActiveItems)
			{
				Personnel personnel = entity as Personnel;

				if (personnel == null) continue;

				if (personnel.LicenseType.PhraseText != contextMenuItem.Caption)
				{
					personnel.LicenseType = (PhraseBase) EntityManager.SelectPhrase(PhraseLicType.Identity, newLicensePhraseId);
					updateNeeded = true;
				}
			}

			if (updateNeeded) UpdateUi();
		}

		void SpinEditChanged(object sender, NumberChangedEventArgs e)
		{
			SpinEdit spinEdit = sender as SpinEdit;

			if ((spinEdit == null) || (e.Number != spinEdit.Number))
			{
				UpdateUi();
			}
		}

		/// <summary>
		/// Updates the UI.
		/// </summary>
		private void UpdateUi()
		{
			if (!m_UpdatingUi)
			{
				m_UpdatingUi = true;

				foreach (var optionsFileItem in m_OptionsFileItems)
				{
					optionsFileItem.Value.Reset();
				}

				//update maxs
				m_OptionsFileItems[LicenseFeature.FF].Max = m_Form.FFCount.Number;
				m_OptionsFileItems[LicenseFeature.FFN].Max = m_Form.FNCount.Number;
				m_OptionsFileItems[LicenseFeature.RO].Max = m_Form.ROCount.Number;
				m_OptionsFileItems[LicenseFeature.RON].Max = m_Form.RONCount.Number;

				m_Form.PersonnelBrowse.Republish(m_PersonnelCollection);

				//count license types selected
				foreach (Personnel personnel in m_PersonnelCollection)
				{
					if (!personnel.LicenseType.IsNull())
					{
						m_OptionsFileItems[LicenseFeatureFromString(personnel.LicenseType.PhraseId)].Count++;
						m_OptionsFileItems[LicenseFeatureFromString(personnel.LicenseType.PhraseId)].UsersInGroup.Add(personnel.Identity);
					}
				}

				//update progress bars
				var ff = m_OptionsFileItems[LicenseFeature.FF];
				m_Form.FFProgressBar.Maximum = ff.ExceedingMax ? ff.Count : ff.Max;
				m_Form.FFProgressBar.Position = ff.Count;
				m_Form.FFProgressBar.Visible = ff.Max != 0;

				var fn = m_OptionsFileItems[LicenseFeature.FFN];
				m_Form.FNProgressBar.Maximum = fn.ExceedingMax ? fn.Count : fn.Max;
				m_Form.FNProgressBar.Position = fn.Count;
				m_Form.FNIconError.Visible = m_Form.FNLabelError.Visible = fn.ExceedingMax;
				m_Form.FNProgressBar.Visible = fn.Max != 0;

				var ro = m_OptionsFileItems[LicenseFeature.RO];
				m_Form.ROProgressBar.Maximum = ro.ExceedingMax ? ro.Count : ro.Max;
				m_Form.ROProgressBar.Position = ro.Count;
				m_Form.ROProgressBar.Visible = ro.Max != 0;

				var ron = m_OptionsFileItems[LicenseFeature.RON];
				m_Form.RONProgressBar.Maximum = ron.ExceedingMax ? ron.Count : ron.Max;
				m_Form.RONProgressBar.Position = ron.Count;
				m_Form.RONIconError.Visible = m_Form.RONLabelError.Visible = ron.ExceedingMax;
				m_Form.RONProgressBar.Visible = ron.Max != 0;

				m_UpdatingUi = false;
			}
		}

		#region End of form behavior

		/// <summary>
		///     Closes the form.
		/// </summary>
		private void CloseForm()
		{
			m_Form.CloseButton.PerformClick();
		}

		/// <summary>
		///     Checks the form is ok
		/// </summary>
		/// <returns></returns>
		private bool FormOk()
		{
			if (m_PersonnelCollection.Cast<Personnel>().Any(p => p.LicenseType.IsNull()))
			{
				Library.Utils.FlashMessage(m_Form.GeneralMessages.NoLicenseError, "");
				return false;
			}

			foreach (var optionsFileItem in m_OptionsFileItems)
			{
				if (optionsFileItem.Value.IsNamed)
				{
					if (optionsFileItem.Value.IsNamed && optionsFileItem.Value.ExceedingMax)
					{
						Library.Utils.FlashMessage(string.Format(m_Form.GeneralMessages.LicenseExceededError, optionsFileItem.Value.GroupName), "");
						return false;
					}
				}
			}
			return true;
		}

		/// <summary>
		///     Saves this instance.
		/// </summary>
		private void Save()
		{
			UpdateUi();

			var buffer = BuildOptionsFile();

			if (m_OptionsFileWritable)
			{
				try
				{
					var backupFile = BackupFile(m_OptionsFilePath);

					File.WriteAllLines(m_OptionsFilePath, buffer);

					var backupMessage = backupFile != "" ? string.Format(m_Form.GeneralMessages.BackupMessage, backupFile) : "";

					Library.Utils.FlashMessage(
						string.Format(m_Form.GeneralMessages.OptionFileWritten, m_OptionsFilePath, backupMessage).Replace(@"\r\n",
						                                                                                                  "\r\n\r\n"),
						"");
				}
				catch
				{
					m_OptionsFileWritable = false;
				}
			}

			if (!m_OptionsFileWritable)
			{
				bool fileSaved = false;

				do
				{
					Library.Utils.FlashMessage(m_Form.GeneralMessages.SaveMessage, "");

					var clientOutputFolder = Library.Utils.PromptForFolder(m_Form.GeneralMessages.SaveFileCaption, @"C:\", true);

					if (!string.IsNullOrEmpty(clientOutputFolder))
					{
						var outputPath = Path.Combine(clientOutputFolder, OptionsFileName);
						var localFile = Library.File.GetWriteFile("smp$textreports", OptionsFileName).FullName;

						File.WriteAllLines(localFile, buffer);

						if (Library.File.TransferToClient(localFile, outputPath))
						{
							File.Delete(localFile);
							Library.Utils.FlashMessage(string.Format(m_Form.GeneralMessages.FileWrittenMessage, outputPath), "");
							fileSaved = true;
						}
					}
					else
					{
						fileSaved = true;
					}
				} while (!fileSaved);
			}

			//save to entities
			EntityManager.Transaction.Add(m_PersonnelCollection);
			EntityManager.Commit();
		}

		/// <summary>
		///     Builds the options file.
		/// </summary>
		/// <returns></returns>
		private List<string> BuildOptionsFile()
		{
			//save options file
			var buffer = new List<string>();

			var itemsToPutInOptFile = (from optionsFileItem in m_OptionsFileItems
									   where optionsFileItem.Value.IsNamed
									   select optionsFileItem.Value).ToList();

			if (File.Exists(m_OptionsFilePath) && m_OptionsFileWritable)
			{
				buffer = File.ReadAllLines(m_OptionsFilePath).ToList();

				foreach (var optionsFileItem in itemsToPutInOptFile)
				{
					var groupName = optionsFileItem.GroupName;

					for (var i = 0; i < buffer.Count; i++)
					{
						if (Regex.IsMatch(buffer[i], string.Format("(GROUP {0})", groupName)))
						{
							buffer.RemoveAt(i);
							i--;
						}
					}

					var groupLine = BuildGroupLine(optionsFileItem);
					var includeLine = BuildIncludeLine(optionsFileItem);

					buffer.Add(groupLine);
					buffer.Add(includeLine);
				}
			}
			else
			{
				buffer.Add("GROUPCASEINSENSITIVE ON");
				//groups
				buffer.AddRange(itemsToPutInOptFile.Select(BuildGroupLine));
				buffer.AddRange(itemsToPutInOptFile.Select(BuildIncludeLine));
			}
			return buffer;
		}

		private string BackupFile(string optionsFilePath)
		{
			var fx = new FileInfo(optionsFilePath);
			if (fx.Exists)
			{
				if (fx.DirectoryName != null)
				{
					var backupFolder = Path.Combine(fx.DirectoryName, "opt backups");
					if (!Directory.Exists(backupFolder))
					{
						Directory.CreateDirectory(backupFolder);
					}
					var backupFile = Path.Combine(backupFolder, fx.Name + Regex.Replace(string.Format(".{0:U}", DateTime.Now), "[^a-zA-Z0-9_.]|-", "_"));
					fx.CopyTo(backupFile);

					return backupFile;
				}
			}
			return "";
		}

		/// <summary>
		///     Builds the include line.
		/// </summary>
		/// <param name="optionsFileItem">The options file item.</param>
		/// <returns></returns>
		private string BuildIncludeLine(OptionsFileItem optionsFileItem)
		{
			return string.Format("{0}INCLUDE {1} GROUP {2}", optionsFileItem.Count == 0 ? "#" : "",
				optionsFileItem.GroupName.Replace("Group", "").Replace(m_InstanceName, ""), optionsFileItem.GroupName);
		}

		/// <summary>
		///     Builds the group line.
		/// </summary>
		/// <param name="optionsFileItem">The options file item.</param>
		/// <returns></returns>
		private static string BuildGroupLine(OptionsFileItem optionsFileItem)
		{
			return string.Format("{0}GROUP {1} {2}", optionsFileItem.Count == 0 ? "#" : "",
				optionsFileItem.GroupName, string.Join(" ", optionsFileItem.UsersInGroup));
		}

		#endregion

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm" /> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			base.MainFormCreated();

			m_Form = (FormNamedLicenseGroupEditor) MainForm;
			m_InstanceName = Library.Environment.InstanceName;

			PublishPersonnel();
		}

		/// <summary>
		///     Called when the <see cref="DefaultFormTask.MainForm" /> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			base.MainFormLoaded();

			SetContextMenu();

			LoadOptionsFile();
			SetupUiEvents();
			UpdateUi();
		}

		#endregion

	}
}