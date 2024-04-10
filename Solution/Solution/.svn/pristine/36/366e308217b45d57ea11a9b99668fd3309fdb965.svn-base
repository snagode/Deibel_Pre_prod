using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Timers;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SM.LIMSML.Helper.Low;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Task to generate localised tags in the database and generate resx files
	/// </summary>
	[SampleManagerTask("GenerateLocalizationTags")]
	public class GenerateLocalizationTags : SampleManagerTask
	{
		#region Member Variables

		FormGenerateLocalizationTags m_Form;

		private const string Tag = "${{{0}.{1}}}";

		private const string ExplorerFolderName = @"Replace(ToName([Cabinet.LocalName])+[ExplorerFolderName]+'FolderName', ' ', '')";
		private const string ExplorerFolderDesc = @"Replace(ToName([Cabinet.LocalName])+[ExplorerFolderName]+'FolderDescription', ' ', '')";
		private const string ExplorerCabinetName = @"Replace([ExplorerCabinetName] + 'CabinetName', ' ', '')";
		private const string ExplorerCabinetDesc = @"Replace([ExplorerCabinetName] + 'CabinetDescription', ' ', '')";
		private const string ExplorerRmbName = @"Replace(ToName([Folder.TableName]) + [LocalName] + 'RmbName', ' ', '')";
		private const string ExplorerRmbDesc = @"Replace(ToName([Folder.TableName]) + [LocalName] + 'RmbDescription', ' ', '')";
		private const string ExplorerGroupName = @"Replace(ToName([Folder.TableName]) + [LocalName] + 'GroupName', ' ', '')";
		private const string ExplorerGroupDesc = @"Replace(ToName([Folder.TableName]) + [LocalName] + 'GroupDescription', ' ', '')";
		private const string EntityPropertyTitle = @"Replace(ToName([EntityTemplate.TableName]) + ToName([Title]) + 'Title', ' ', '')";

		private readonly Dictionary<string, Dictionary<string, string>> m_Resources = new Dictionary<string, Dictionary<string, string>>();

		private string m_ProgressMessage;
		private string m_CabinetMessageFile = "";
		private string m_FolderMessageFile = "";
		private string m_RmbMessageFile = "";
		private string m_GroupMessageFile = "";
		private string m_TemplateMessageFile = "";

		private Timer m_Timer = new Timer(100);

		#endregion

		#region Overrides

		/// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			m_Form = (FormGenerateLocalizationTags)FormFactory.CreateForm("GenerateLocalizationTags");
			m_Form.Loaded += FormLoaded;
			m_Form.Closed += FormClosed;
			m_Form.Show();
		}

		void FormClosed(object sender, EventArgs e)
		{
			m_Timer.AutoReset = false;
			m_Timer.Start();

			if (!string.IsNullOrEmpty(m_CabinetMessageFile))
			{
				m_ProgressMessage = Library.Message.GetMessage("ExplorerMessages", "UpdatingCabinetTag");
				LocaliseExplorerCabinets(UpdateProgress);
			}

			if (!string.IsNullOrEmpty(m_FolderMessageFile))
			{
				m_ProgressMessage = Library.Message.GetMessage("ExplorerMessages", "UpdatingFoldersTag");
				LocaliseExplorerFolders(UpdateProgress);
			}

			if (!string.IsNullOrEmpty(m_RmbMessageFile))
			{
				m_ProgressMessage = Library.Message.GetMessage("ExplorerMessages", "UpdatingRMBsTag");
				LocaliseExplorerRmbs(UpdateProgress);
			}

			if (!string.IsNullOrEmpty(m_GroupMessageFile))
			{
				m_ProgressMessage = Library.Message.GetMessage("ExplorerMessages", "UpdatingGroupsTag");
				LocaliseExplorerGroups(UpdateProgress);
			}

			if (!string.IsNullOrEmpty(m_TemplateMessageFile))
			{
				m_ProgressMessage = Library.Message.GetMessage("ExplorerMessages", "UpdatingTemplatePropertiesTag");
				LocaliseTemplateProperties(UpdateProgress);
			}

			BuildResources(UpdateProgress);

			EntityManager.Commit();

			Library.Utils.SetStatusBar("");
		}

		private void FormLoaded(object sender, EventArgs e)
		{
			m_Form.ButtonConvert.Click += ButtonConvertClick;
		}

		private void ButtonConvertClick(object sender, EventArgs e)
		{
			if (m_Form.ExplorerCabinetsCheck.Checked) m_CabinetMessageFile = m_Form.ExplorerFileName.Text;
			if (m_Form.ExplorerFoldersCheck.Checked) m_FolderMessageFile = m_Form.ExplorerFileName.Text;
			if (m_Form.ExplorerRMBsCheck.Checked) m_RmbMessageFile = m_Form.ExplorerFileName.Text;
			if (m_Form.ExplorerGroupsCheck.Checked) m_GroupMessageFile = m_Form.ExplorerFileName.Text;
			if (m_Form.EntityTemplateCheck.Checked) m_TemplateMessageFile = m_Form.TemplatesFileName.Text;

			m_Form.Close();
		}

		private void UpdateProgress(int position, int count)
		{
			if (m_Timer.Enabled) return;

			if (count == 0)
			{
				Library.Utils.SetStatusBar(m_ProgressMessage);
			}
			else
			{
				int percent = (int)Math.Floor((decimal)position / count * 100.0m);
				string message = string.Format(m_ProgressMessage, percent);

				Library.Utils.SetStatusBar(message);
			}

			m_Timer.Enabled = true;
		}

		#endregion

		#region Create Database Tags

		private void LocaliseAll()
		{
			LocaliseExplorerCabinets();
			LocaliseExplorerFolders();
			LocaliseExplorerRmbs();
			LocaliseExplorerGroups();
			LocaliseTemplateProperties();

			BuildResources();

			EntityManager.Commit();
		}

		private void LocaliseTemplateProperties(Action<int, int> progress = null)
		{
			IEntityCollection items = EntityManager.Select(EntityTemplatePropertyBase.EntityName);

			LocaliseField(m_TemplateMessageFile, items, EntityTemplatePropertyPropertyNames.Title, EntityPropertyTitle, progress);
		}

		private void LocaliseExplorerGroups(Action<int, int> progress = null)
		{
			IEntityCollection items = EntityManager.Select(ExplorerGroupBase.EntityName);

			LocaliseField(m_GroupMessageFile, items, ExplorerGroupPropertyNames.Description, ExplorerGroupDesc, progress);
			LocaliseField(m_GroupMessageFile, items, ExplorerGroupPropertyNames.ExplorerGroupName, ExplorerGroupName, progress);
		}

		private void LocaliseExplorerRmbs(Action<int, int> progress = null)
		{
			IQuery query = EntityManager.CreateQuery(ExplorerRmbBase.EntityName);
			query.AddNotEquals(ExplorerRmbPropertyNames.Type, PhraseRmbType.PhraseIdSEPARATOR);
			IEntityCollection items = EntityManager.Select(ExplorerRmbBase.EntityName, query);

			LocaliseField(m_RmbMessageFile, items, ExplorerRmbPropertyNames.Description, ExplorerRmbDesc, progress);
			LocaliseField(m_RmbMessageFile, items, ExplorerRmbPropertyNames.ExplorerRmbName, ExplorerRmbName, progress);
		}

		private void LocaliseExplorerFolders(Action<int, int> progress = null)
		{
			IEntityCollection items = EntityManager.Select(ExplorerFolderBase.EntityName);

			LocaliseField(m_FolderMessageFile, items, ExplorerFolderPropertyNames.Description, ExplorerFolderDesc, progress);
			LocaliseField(m_FolderMessageFile, items, ExplorerFolderPropertyNames.ExplorerFolderName, ExplorerFolderName, progress);
		}

		private void LocaliseExplorerCabinets(Action<int, int> progress = null)
		{
			IEntityCollection items = EntityManager.Select(ExplorerCabinetBase.EntityName);

			LocaliseField(m_CabinetMessageFile, items, ExplorerCabinetPropertyNames.Description, ExplorerCabinetDesc, progress);
			LocaliseField(m_CabinetMessageFile, items, ExplorerCabinetPropertyNames.ExplorerCabinetName, ExplorerCabinetName, progress);
		}

		private void LocaliseField(string messageFile, IEntityCollection items, string fieldName, string formula, Action<int, int> progress = null)
		{
			int count = items.Count;
			int pos = 0;

			foreach (IEntity item in items)
			{
				// Check the value isn't already localised
				string currentValue = item.GetString(fieldName);

				if (currentValue.StartsWith("${") && currentValue.EndsWith("}"))
				{
					continue;
				}

				// Update the progress position
				if (progress != null) progress(pos++, count);

				// Build the tag
				string messageName = Library.Formula.Evaluate(item, formula).ToString();
				messageName = new string(messageName.Where(Char.IsLetterOrDigit).ToArray());

				string messageTag = string.Format(Tag, messageFile, messageName);
				item.Set(fieldName, messageTag);

				EntityManager.Transaction.Add(item);

				CreateResxEntry(messageFile, messageName, currentValue);
			}
		}

		private void CreateResxEntry(string messageFile, string messageTag, string currentValue)
		{
			Dictionary<string, string> messages;

			if (!m_Resources.TryGetValue(messageFile, out messages))
			{
				messages = new Dictionary<string, string>();
				m_Resources.Add(messageFile, messages);
			}

			messages[messageTag] = currentValue;
		}

		#endregion

		#region Build .resx and .resources Files

		private void BuildResources(Action<int, int> progress = null)
		{
			foreach (var resource in m_Resources)
			{
				MergeExistingResources(resource.Key, resource.Value);
				BuildResourceFiles(resource.Key, resource.Value, progress);
			}
		}

		private void MergeExistingResources(string fileName, Dictionary<string, string> entries)
		{
			FolderList resourceDir = Library.Environment.GetFolderList("smp$root", "Localization\\neutral");

			if (resourceDir.Count == 0) return;

			string fullName = Path.Combine(resourceDir[0].ToString(), fileName + ".resx");

			if (!File.Exists(fullName)) return;

			using (ResXResourceReader resxReader = new ResXResourceReader(fullName))
			{
				foreach (DictionaryEntry entry in resxReader)
				{
					entries[entry.Key.ToString()] = entry.Value.ToString();
				}
			}
		}

		private void BuildResourceFiles(string fileName, Dictionary<string, string> entries, Action<int, int> progress = null)
		{
			FolderList resourceDir = Library.Environment.GetFolderList("smp$resource", "Localization");
			FolderList rootDir = Library.Environment.GetFolderList("smp$root", "Localization\\neutral");

			if (resourceDir.Count == 0) return;
			if (rootDir.Count == 0) return;

			m_ProgressMessage = Library.Message.GetMessage("ExplorerMessages", "BuildingResourceFile", fileName);
			if (progress != null) progress(0, 0);

			var errors = new StringBuilder();

			using (ResXResourceWriter resxWriter = new ResXResourceWriter(Path.Combine(rootDir[0].ToString(), fileName + ".resx")))
			{
				ResourceWriter resourceWriter = null;
				var resourceFileName = fileName + ".resources";

				try
				{
					resourceWriter = new ResourceWriter(Path.Combine(resourceDir[0].ToString(), resourceFileName));
				}
				catch (IOException)
				{
					//message
					errors.AppendLine(string.Format(m_Form.FormMessages.LockedFileMessage, resourceFileName));
				}
				catch (Exception exception)
				{
					//message
					errors.AppendLine(exception.Message);
					resourceWriter = null;

					try
					{
						File.Delete(Path.Combine(resourceDir[0].ToString(), resourceFileName));
					}
					// ReSharper disable once EmptyGeneralCatchClause
					catch
					{
					}
				}

				foreach (var entry in entries)
				{
					resxWriter.AddResource(entry.Key, entry.Value);

					if (resourceWriter != null) resourceWriter.AddResource(entry.Key, entry.Value);
				}

				if (resourceWriter != null)
				{
					resourceWriter.Generate();
					resourceWriter.Close();
				}
			}

			if (errors.Length > 0)
			{
				errors.AppendLine(m_Form.FormMessages.LockedFileInstruction);
				Library.Utils.FlashMessage(errors.ToString(), "");
			}
		}

		#endregion
	}
}