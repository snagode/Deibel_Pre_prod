using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.ImportExport;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the EXPLORER_FOLDER entity.
	/// </summary>
	[SampleManagerEntity(ExplorerFolderIconBase.EntityName)]
	public class ExplorerFolderIcon : ExplorerFolderIconBase
	{
		#region Overrides

		/// <summary>
		/// Perform post creation processing.
		/// </summary>
		protected override void OnEntityCreated()
		{
			ApplyKeyIncrements("ITEM_NUMBER");
		}

		#endregion

        #region Export

        /// <summary>
        /// Gets the Properties that must be processed on the model.
        /// </summary>
        /// <returns></returns>
        public override List<string> GetCustomExportableProperties()
        {
            List<string> properties = base.GetCustomExportableProperties();
            properties.Add(ExplorerFolderIconPropertyNames.Value);
            return properties;
        }

        /// <summary>
        /// Gets Property's value linked data.
        /// </summary>
        /// <param name="propertyName">The property name to process</param>
        /// <param name="exportList">The Entity Export List</param>
        public override void GetLinkedData(string propertyName, EntityExportList exportList)
        {
            if (propertyName == ExplorerFolderIconPropertyNames.Value)
            {
                if (Value != null)
                {
                    ISchemaField field = Schema.Current.Tables[Folder.TableName].Fields[FieldName];

                    if (!string.IsNullOrEmpty(field.PhraseType))
                    {
                        exportList.AddPhraseType(field.PhraseType);
                        return;
                    }

                    if (field.LinkField != null && field.LinkTable.KeyFields.Count == 1 && field.LinkField == field.LinkTable.KeyFields[0])
                    {
                        IEntity entity = EntityManager.Select(field.LinkTable.Name, new Identity(Value));
                        exportList.AddEntity(entity);
                    }
                }
            }
        }

        #endregion
    }
}
