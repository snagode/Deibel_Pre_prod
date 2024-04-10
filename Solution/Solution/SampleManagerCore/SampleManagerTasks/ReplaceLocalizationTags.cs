using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Task to replace localised tags in the database using the current translation
	/// </summary>
	[SampleManagerTask("ReplaceLocalizationTags")]
	public class ReplaceLocalizationTags : SampleManagerTask
	{
		#region Member Variables

		#endregion

		#region Overrides

		/// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			UndoAll();
		}

		#endregion

		#region Undo Database Tags

		/// <summary>
		/// Repopulate the database fields using the current translation
		/// </summary>
		private void UndoAll()
		{
			UndoField(ExplorerCabinetBase.EntityName, ExplorerCabinetPropertyNames.ExplorerCabinetName);
			UndoField(ExplorerCabinetBase.EntityName, ExplorerCabinetPropertyNames.Description);

			UndoField(ExplorerFolderBase.EntityName, ExplorerFolderPropertyNames.ExplorerFolderName);
			UndoField(ExplorerFolderBase.EntityName, ExplorerFolderPropertyNames.Description);

			UndoField(ExplorerRmbBase.EntityName, ExplorerRmbPropertyNames.ExplorerRmbName);
			UndoField(ExplorerRmbBase.EntityName, ExplorerRmbPropertyNames.Description);

			UndoField(ExplorerGroupBase.EntityName, ExplorerGroupPropertyNames.ExplorerGroupName);
			UndoField(ExplorerGroupBase.EntityName, ExplorerGroupPropertyNames.Description);

			UndoField(EntityTemplatePropertyBase.EntityName, EntityTemplatePropertyPropertyNames.Title);

			EntityManager.Commit();		
		}

		private void UndoField(string tableName, string fieldName)
		{
			var items = EntityManager.Select(tableName);

			foreach (IEntity item in items)
			{
				string currentValue = item.GetString(fieldName);

				if (currentValue.StartsWith("${") && currentValue.EndsWith("}"))
				{
					string translation = Library.Message.ConvertTaggedField(currentValue);

					if (translation.StartsWith("[") && translation.EndsWith("]"))
					{
						item.Set(fieldName, translation);
					}
				}
			}
		}

		#endregion
	}
}