using System.IO;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the LABEL_TEMPLATE entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class LabelTemplate : LabelTemplateBase
	{
		#region Delete

		/// <summary>
		/// Delete the File
		/// </summary>
		protected override void OnDeleted()
		{
			DeleteFile(LabelFileExtension);
			DeleteFile(LabelPrintExtension);
		}

		/// <summary>
		/// Deletes the file.
		/// </summary>
		/// <param name="extension">The extension.</param>
		private void DeleteFile(string extension)
		{
			string fileName = Path.ChangeExtension(Name, extension);
			FileInfo fileInfo = Library.File.GetWriteFile("smp$resource", LabelFolder, fileName);
			if (fileInfo.Exists) fileInfo.Delete();
		}

		#endregion
	}
}