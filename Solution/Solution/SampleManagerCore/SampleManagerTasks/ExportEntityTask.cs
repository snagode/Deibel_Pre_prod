using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.ImportExport;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel.Import_Helpers;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Export Entity Task
	/// </summary>
	[SampleManagerTask("ExportEntityTask")]
	public class ExportEntityTask : SampleManagerTask
	{
		#region Member Variables

		private string m_XmlFileName;


		#endregion

		#region Overrides

		/// <summary>
		///     Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			m_XmlFileName = Library.Utils.PromptForFile("Xml filename", "XML Files|*.xml", true);

			if (string.IsNullOrEmpty(m_XmlFileName))
			{
				Exit();
				return;
			}

			ExportData();
		}

		#endregion

		#region Private Background Worker

		/// <summary>
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ExportBackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
		{
			var referencedEntityTypes = new List<string> {Context.EntityType};

			var options = new ExportEntityOptions
			{
				FileFormat = ExportFormat.Xml,
				IncludeAllReferences = false,
				IncludeAttachments = false,
				IncludePhraseReferences = false,
				IncludeReferencesForEntityTypes = referencedEntityTypes,
				IncludeSchemaInformation = true,
				IncludeNonModifiable = false
			};

			var tempDirectory = Path.GetTempPath();
			var guid = Guid.NewGuid().ToString();
			var path = Path.Combine(tempDirectory, "ImportExport", guid);

			// Perform the export.
			FileInfo[] files;

			try
			{
				PreprocessEntities(Context.SelectedItems);

				files = Library.ExportData.Export(Context.SelectedItems, options, path, Context.EntityType);
			}
			catch (Exception ex)
			{
				Library.Utils.FlashMessage(ex.Message, "");
				return;
			}

			if (files != null && files.Length > 0)
			{
				var file = files[0];

				Library.File.TransferToClient(file.FullName, m_XmlFileName, Library.Environment.GetGlobalInt("CLIENT_TYPE") == 1);
				Directory.Delete(path, true);

				Library.Utils.SetStatusBar("");
				var caption = Library.Message.GetMessage("LaboratoryMessages", Context.SelectedItems.Count == 1 ? "ExportSuccessCaptionSingular" : "ExportSuccessCaption");
				var title = Library.Message.GetMessage("LaboratoryMessages", "ExportSuccessTitle");

				Library.Utils.FlashMessage(string.Format(caption, Context.SelectedItems.Count), title);
			}
			else
			{
				var caption = Library.Message.GetMessage("LaboratoryMessages", "ExportNoFilesError");
				var title = Library.Message.GetMessage("LaboratoryMessages", "ExportError");

				Library.Utils.FlashMessage(caption, title);
			}
		}

		/// <summary>
		/// Pre-processes the entities.
		/// </summary>
		/// <param name="selectedItems">The selected items.</param>
		private void PreprocessEntities(IEntityCollection selectedItems)
		{
			foreach (IEntity entity in selectedItems)
			{
				if (entity is IImportableEntity)
				{
					((IImportableEntity) entity).ExportPreprocess(entity);
				}
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		///     Performs the export.
		/// </summary>
		private void ExportData()
		{
			var exportBackgroundWorker = new BackgroundWorker();
			exportBackgroundWorker.DoWork += ExportBackgroundWorkerDoWork;
			exportBackgroundWorker.RunWorkerAsync();
		}

		#endregion
	}
}