using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.ImportExport;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the TEMPLATE_FIELDS entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class TemplateFields : TemplateFieldsBase
	{
		#region Export

		/// <summary>
		/// Gets the Properties that must be processed on the model.
		/// </summary>
		/// <returns></returns>
		public override List<string> GetCustomExportableProperties()
		{
			List<string> properties = base.GetCustomExportableProperties();
			properties.Add(TemplateFieldsPropertyNames.DefaultValue);
			return properties;
		}

		/// <summary>
		/// Gets Property's value linked data.
		/// </summary>
		/// <param name="propertyName">The property name to process</param>
		/// <param name="exportList">The Entity Export List</param>
		public override void GetLinkedData(string propertyName, EntityExportList exportList)
		{
			if (propertyName == TemplateFieldsPropertyNames.DefaultValue)
			{
				if (DefaultValue != null && DefaultType.PhraseId == PhraseMttypeLot.PhraseIdV)
				{
					ISchemaField field = Schema.Current.Tables[TableName].Fields[FieldName];

					if (field != null)
					{
						// Add referenced Phrase Type

						if (!string.IsNullOrEmpty(field.PhraseType))
						{
							exportList.AddPhraseType(field.PhraseType);
							return;
						}

						// Add referenced entity

						if (field.LinkField != null && field.LinkTable.KeyFields.Count == 1 && field.LinkField == field.LinkTable.KeyFields[0])
						{
							IEntity entity = EntityManager.Select(field.LinkTable.Name, new Identity(DefaultValue));
							exportList.AddEntity(entity);
						}
					}
				}
			}
		}

		#endregion
	}
}