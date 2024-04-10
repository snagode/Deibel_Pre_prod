using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.ClientControls.Browse;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Library.ImportExport;
using Thermo.SampleManager.ObjectModel.Import_Helpers;
using Thermo.SampleManager.Server;
using Thermo.SampleManager.Server.ImportExportService;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	///     Entity Import Client Task
	/// </summary>
	[SampleManagerTask("ImportEntityResultsTask")]
	public class ImportEntityResultsTask : DefaultFormTask
	{
		#region Icon Enum

		/// <summary>
		///     Status icons
		/// </summary>
		private enum
			StatusIcon
		{

			INT_INFORMATION,

			DM_EVENT_ERROR,
		}

		#endregion

		#region Overrides

		/// <summary>
		///     Called when the <see cref="DefaultFormTask.MainForm" /> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			base.MainFormLoaded();
			m_Form = (FormImportEntityResults) MainForm;
			m_Form.saveResultsButton.Click += SaveResults;
			m_Form.runImport.Click += runImport_Click;

			if (SelectXmlFiles())
			{
				BuildImport();
			}
			else
			{
				Exit();
			}
		}

		private void runImport_Click(object sender, EventArgs e)
		{
			if (NothingToDo())
			{
				return;
			}
			if (!ConfirmOverwrites())
			{
				return;
			}

			//do import:
			foreach (var row in m_Form.entityGrid.Rows)
			{
				var action = ActionFromSelectedOption(row["ActionColumn"].ToString());
				var rowResult = row.Tag as ImportValidationResult;

				if (action!=ImportValidationResult.ImportActions.Unset)
				{
					if (rowResult != null)
					{
						rowResult.SelectedImportAction = action;
						var commitResult = ((IImportableEntity)rowResult.Entity).Import(rowResult.Entity, rowResult);
						row.SetIcon(GetCommitResultIcon(commitResult));
						row["ImportResult"] = string.Format("'{0}' '{1}': {2}", rowResult.Entity.EntityType, rowResult.DisplayName, commitResult.Result);

						if (commitResult.State != ImportCommitResult.ImportCommitResultState.Skipped)
						{
							m_Form.entityGrid.Columns[1].SetCellBrowse(row, m_NoActionsBrowse);
							row["ActionColumn"] = "";
						}
					} 
				}
			}

			Action a = () =>
			{
				m_MainWindowService.Refresh();
				Library.Utils.FlashMessage(Library.Message.GetMessage("LaboratoryMessages", "ImportFinished"), "");
				m_MainWindowService.Refresh();
			};
			
			a.BeginInvoke(null, null);



			

		}

		private bool ConfirmOverwrites()
		{
			var overwrites = false;
			foreach (var row in m_Form.entityGrid.Rows)
			{
				var action = ActionFromSelectedOption(row["ActionColumn"].ToString());
				if (action == ImportValidationResult.ImportActions.Overwrite)
				{
					overwrites = true;
					break;
				}
			}
			if (overwrites)
			{
				if (!Library.Utils.FlashMessageYesNo(Library.Message.GetMessage("LaboratoryMessages", "ImportOverwriteMessage"), Library.Message.GetMessage("LaboratoryMessages", "ImportOverwriteCaption")))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		///     Checks if theres nothing to do.
		/// </summary>
		/// <returns></returns>
		private bool NothingToDo()
		{
			var nothingToDo = true;
			foreach (var row in m_Form.entityGrid.Rows)
			{
				var action = (row["ActionColumn"]!=null) ? ActionFromSelectedOption(row["ActionColumn"].ToString()) : ImportValidationResult.ImportActions.Unset;
			
				if (action == ImportValidationResult.ImportActions.Skip || action == ImportValidationResult.ImportActions.Unset)
				{
					nothingToDo = true;
				}
				else
				{
					nothingToDo = false;
					break;
				}
			}

			if (nothingToDo)
			{
				Library.Utils.FlashMessage(Library.Message.GetMessage("LaboratoryMessages", "ImportNothingToDo"), "");
				return true;
			}

			return false;
		}

		#endregion

		#region Private Members

		private IEntityXmlImportInspectionService m_ImportService;
		private ISecurityService m_SecurityService;
		private IMainWindowService m_MainWindowService;


		private List<string> m_XmlFileNames;
		private FormImportEntityResults m_Form;
		private List<Tuple<string, string>> m_Results;
		private StringBrowse m_NoActionsBrowse;

		#endregion

		#region Private Methods

		/// <summary>
		///     Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			base.SetupTask();
			m_ImportService = (IEntityXmlImportInspectionService) Library.GetService(typeof (IEntityXmlImportInspectionService));
			m_SecurityService = (ISecurityService) Library.GetService(typeof (ISecurityService));
			m_MainWindowService = (IMainWindowService)Library.GetService(typeof(IMainWindowService));
		
			m_Results = new List<Tuple<string, string>>();
			m_NoActionsBrowse = BrowseFactory.CreateStringBrowse(new List<string>());
		}

		/// <summary>
		///     Selects the XML file.
		/// </summary>
		/// <returns></returns>
		private bool SelectXmlFiles()
		{
			var clientFiles = Library.Utils.PromptForFiles(Library.Message.GetMessage("LaboratoryMessages", "ImportPleaseSelectFile"), "XML Files|*.xml");

			if (clientFiles==null || clientFiles.Count==0)
			{
				AddResult(Library.Message.GetMessage("LaboratoryMessages", "ImportCancelled"), StatusIcon.INT_INFORMATION);
				return false;
			}
			m_XmlFileNames = new List<string>();
			foreach (var clientFile in clientFiles)
			{
				var fileInfo = Library.File.GetWriteFile("smp$resource", "", Path.GetFileName(clientFile));

				Library.File.TransferFromClient(clientFile, fileInfo.FullName);

				if (!IsXMLFileValid(fileInfo.FullName))
				{
					Library.Utils.FlashMessage(string.Format("{0}: {1}", clientFile, Library.Message.GetMessage("LaboratoryMessages", "ImportInvalidFile")), Library.Message.GetMessage("LaboratoryMessages", "ImportInvalidFile"));
					return false;
				}

				m_XmlFileNames.Add(fileInfo.FullName);
			}

			return true;
		}

		/// <summary>
		///     Determines whether [is XML file valid] [the specified filename].
		/// </summary>
		/// <param name="filename">The filename.</param>
		/// <returns></returns>
		private bool IsXMLFileValid(string filename)
		{
			var validFile = true;

			try
			{
				var document = new XmlDocument();
				document.Load(filename);

				// One single root of type "export-entity"
				var roots = document.SelectNodes("export-entity");
				if (roots == null)
				{
					validFile = false;
				}
				else if (roots.Count != 1)
				{
					validFile = false;
				}
			}
			catch
			{
				return false;
			}

			return validFile;
		}

		/// <summary>
		///     Runs the import.
		/// </summary>
		private void BuildImport()
		{
			m_Results.Clear();
			foreach (var file in m_XmlFileNames)
			{
				try
				{
					var importType = GetImportType(file);
					var importEntitiesIdentities = GetImportEntityNames(file);



					var xmlImportInformation = m_ImportService.AnalyseFile(file, importType, importEntitiesIdentities, EntityManager);

					File.Delete(file);

					IEntityCollection entities = null;
					string buildException = "";

					try
					{
						entities = m_ImportService.BuildEntities(xmlImportInformation, EntityManager);
					}
					catch (Exception ex)
					{
						buildException = ex.Message;
					}

					if (!string.IsNullOrEmpty(buildException))
					{
						var continueMessage = Library.Message.GetMessage("LaboratoryMessages", "ConfirmContinue");
						if (!Library.Utils.FlashMessageYesNo(buildException + System.Environment.NewLine + continueMessage, continueMessage))
						{
							throw new Exception(buildException);
						}
					}

					if (entities != null)
					{
						foreach (var e in entities)
						{
							if (e is IImportableEntity)
							{
								var validityResults = ((IImportableEntity) e).CheckImportValidity((IEntity) e, xmlImportInformation.ImportEntities as List<ExportDataEntity>);
								if (validityResults.Result == ImportValidationResult.ValidityResult.Error)
								{
									foreach (var error in validityResults.Errors)
									{
										m_Form.entityGrid.AddRow(string.Format("{0}: {1}", validityResults.DisplayName, error), "");
										var row = m_Form.entityGrid.Rows.Count - 1;
										m_Form.entityGrid.Columns[1].SetCellBrowse(m_Form.entityGrid.Rows[row], m_NoActionsBrowse);
										m_Form.entityGrid.Rows[row].SetIcon(IconFromStatus(ImportValidationResult.ValidityResult.Error));
										m_Form.entityGrid.Rows[row].Tag = validityResults;
									}
								}
								else
								{
									var displayResult = new StringBuilder();
									displayResult.AppendFormat("'{0}' '{1}'", validityResults.Entity.EntityType, validityResults.DisplayName);

									if (validityResults.AlreadyExists)
									{
										displayResult.AppendFormat(": {0}", Library.Message.GetMessage("LaboratoryMessages", "ImportAlreadyExists"));

										if (validityResults.IsRemoved)
										{
											displayResult.AppendFormat("({0})", Library.Message.GetMessage("LaboratoryMessages", "ImportIsRemoved"));
										}
									}
									else
									{
										displayResult.AppendFormat(": {0} '{1}'", Library.Message.GetMessage("LaboratoryMessages", "ImportNew"),
											validityResults.Entity.EntityType);
									}

									if (validityResults.AdditionalInformation != "") displayResult.AppendFormat(": {0}", validityResults.AdditionalInformation);

									m_Form.entityGrid.AddRow(displayResult.ToString(), "");
									var row = m_Form.entityGrid.Rows.Count - 1;
									m_Form.entityGrid.Columns[1].SetCellBrowse(m_Form.entityGrid.Rows[row], StringBrowseFromValidityActions(validityResults.AvailableActions));
									m_Form.entityGrid.Rows[row].SetIcon(IconFromStatus(validityResults.Result));
									m_Form.entityGrid.Rows[row]["ActionColumn"] = LocalizedOptionFromValidationOption(validityResults.DefaultAction);
									m_Form.entityGrid.Rows[row].Tag = validityResults;
								}
							}
							else
							{
								throw new Exception("Entity does not implement IImportableEntity");
							}
						}
					}
					else
					{
						if (buildException == "") throw new Exception("Entities are null");
					}
				}
				catch (Exception ex)
				{
					m_Form.entityGrid.AddRow(string.Format("{0}", ex.Message), "");

					var row = m_Form.entityGrid.Rows.Count - 1;
					m_Form.entityGrid.Columns[1].SetCellBrowse(m_Form.entityGrid.Rows[row], m_NoActionsBrowse);
					m_Form.entityGrid.Rows[row].SetIcon(IconFromStatus(ImportValidationResult.ValidityResult.Error));
					m_Form.entityGrid.Rows[row].Tag = null;
				}
			}
		}


		/// <summary>
		///     Gets the type of the import.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">Could not identify import type</exception>
		private string GetImportType(string fileName)
		{
			var rMatch = Regex.Match(File.ReadAllText(fileName), "<!--EXPORT_TYPE:(.*)-->");
			if (rMatch == null || string.IsNullOrEmpty(rMatch.ToString()) || rMatch.Groups.Count != 2)
			{
				throw new Exception("Could not identify import type");
			}
			return rMatch.Groups[1].ToString();
		}

		private List<string> GetImportEntityNames(string fileName)
		{
			var rMatch = Regex.Match(File.ReadAllText(fileName), "<!--EXPORT_ENTITIES:(.*)-->");
			if (rMatch == null || string.IsNullOrEmpty(rMatch.ToString()) || rMatch.Groups.Count != 2)
			{
				throw new Exception("Could not identify import entity meta data");
			}

			return rMatch.Groups[1].ToString().Split(',').ToList();
		}

		/// <summary>
		///     Adds the result.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="icon">The icon.</param>
		private void AddResult(string message, StatusIcon icon)
		{
			m_Results.Add(new Tuple<string, string>(message, icon.ToString()));
		}

		/// <summary>
		///     Saves the results.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		private void SaveResults(object sender, EventArgs e)
		{
			var clientFileName = Library.Utils.PromptForFile("Save", "Text File|*.txt", true);
			if (!string.IsNullOrEmpty(clientFileName))
			{
				var tempFile = Library.File.GetWriteFile("smp$resource", Guid.NewGuid() + ".txt");

				using (var sw = tempFile.CreateText())
				{
					foreach (UnboundGridRow row in m_Form.entityGrid.Rows)
					{
						sw.WriteLine(row["ImportResult"].ToString());
					}
				}
				Library.File.TransferToClient(tempFile.FullName, clientFileName);
				tempFile.Delete();
				Library.Utils.FlashMessage(Library.Message.GetMessage("LaboratoryMessages", "GeneralDoneMessage"), "");
			}
		}

		#endregion

		#region IImportable Utilities

		/// <summary>
		///     Icons from status.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <returns></returns>
		public IconName IconFromStatus(ImportValidationResult.ValidityResult result)
		{
			switch (result)
			{
				case ImportValidationResult.ValidityResult.Unset:

					break;
				case ImportValidationResult.ValidityResult.Ok:
					return new IconName("INT_ADD_SIGN");
				case ImportValidationResult.ValidityResult.Warning:
					return new IconName("DM_EVENT_WARNING");
				case ImportValidationResult.ValidityResult.Error:
					return new IconName("DELETE");
				default:
					throw new ArgumentOutOfRangeException("result");
			}

			return new IconName("NULL_ICON");
		}

		/// <summary>
		///     Commits the result icon.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentOutOfRangeException">result</exception>
		public IconName GetCommitResultIcon(ImportCommitResult result)
		{
			switch (result.State)
			{
				case ImportCommitResult.ImportCommitResultState.Ok:
					return new IconName("CHECK");
				case ImportCommitResult.ImportCommitResultState.Skipped:
					return new IconName("WELL_EMPTY");
				case ImportCommitResult.ImportCommitResultState.Error:
					return new IconName("DELETE");
				default:
					throw new ArgumentOutOfRangeException("result");
			}
		}

		/// <summary>
		///     Strings the browse from validity actions.
		/// </summary>
		/// <param name="actions">The actions.</param>
		/// <returns></returns>
		public StringBrowse StringBrowseFromValidityActions(List<ImportValidationResult.ImportActions> actions)
		{
			return BrowseFactory.CreateStringBrowse(actions.Select(LocalizedOptionFromValidationOption).ToList());
		}

		/// <summary>
		///     Localizeds the option from validation option.
		/// </summary>
		/// <param name="action">The action.</param>
		/// <returns></returns>
		public string LocalizedOptionFromValidationOption(ImportValidationResult.ImportActions action)
		{
			return Library.Message.GetMessage("LaboratoryMessages", "ImportAction_" + action);
		}

		/// <summary>
		///     Actions from selected option.
		/// </summary>
		/// <param name="selectedOption">The selected option.</param>
		/// <returns></returns>
		public ImportValidationResult.ImportActions ActionFromSelectedOption(string selectedOption)
		{
			if (string.IsNullOrEmpty(selectedOption))
			{
				return ImportValidationResult.ImportActions.Unset;
			}

			selectedOption = selectedOption.Replace(" ", "_");

			//build items
			var items = new Dictionary<string, ImportValidationResult.ImportActions>();
			foreach (var enumItem in Enum.GetNames(typeof (ImportValidationResult.ImportActions)))
			{
				items.Add(Library.Message.GetMessage("LaboratoryMessages", "ImportAction_" + enumItem).Replace(" ", "_"), (ImportValidationResult.ImportActions) Enum.Parse(typeof (ImportValidationResult.ImportActions), enumItem));
			}

			return items[selectedOption];
		}

		#endregion
	}

}