using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.ImportExport;
using Thermo.SampleManager.Common.Utilities;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the EXPLORER_AUX_FIELDS entity.
	/// </summary>
	[SampleManagerEntity(ExplorerAuxFieldsBase.EntityName)]
	public class ExplorerAuxFields : ExplorerAuxFieldsBase
	{
		#region Field Names

		/// <summary>
		/// Gets or sets the name of the field.
		/// </summary>
		/// <value>The name of the field.</value>
		public override string FieldName
		{
			get { return base.FieldName; }
			set
			{
				ExplorerAuxFieldsName = TextUtils.GetDisplayText(value);
				base.FieldName = value;
			}
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
            properties.Add(ExplorerAuxFieldsPropertyNames.DefaultValue);
            return properties;
        }

        /// <summary>
        /// Gets Property's value linked data.
        /// </summary>
        /// <param name="propertyName">The property name to process</param>
        /// <param name="exportList">The Entity Export List</param>
        public override void GetLinkedData(string propertyName, EntityExportList exportList)
        {
            if (propertyName == ExplorerAuxFieldsPropertyNames.DefaultValue)
            {
                if (!string.IsNullOrEmpty(DefaultValue))
                {
                    ISchemaField schemaField = Schema.Current.Tables[ExplorerAux.TableName].Fields[FieldName];

                    if (!string.IsNullOrEmpty(schemaField.PhraseType))
                    {
                        exportList.AddPhraseType(schemaField.PhraseType);
                        return;
                    }
                    if (schemaField.LinkField != null && schemaField.LinkTable.KeyFields.Count == 1 && schemaField.LinkField == schemaField.LinkTable.KeyFields[0])
                    {
                        IEntity entity = EntityManager.Select(schemaField.LinkTable.Name, new Identity(DefaultValue));
                        exportList.AddEntity(entity);
                    }
                }
            }
            else
            {
                base.GetLinkedData(propertyName, exportList);
            }
        }
        
        #endregion
    }
}
