using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the LOT_TEMPLATE entity.
	/// </summary>
	[SampleManagerEntity(LotTemplateBase.EntityName)]
	public class LotTemplate : LotTemplateBase
	{
		#region Template Fields

		/// <summary>
		/// Gets the fields.
		/// </summary>
		/// <value>
		/// The fields.
		/// </value>
		[PromptCollection(TemplateFieldsBase.EntityName, false)]
		public IEntityCollection Fields
		{
			get
			{
				IQuery query = EntityManager.CreateQuery(TemplateFieldsBase.EntityName);
				query.AddEquals(TemplateFieldsPropertyNames.TableName, LotDetailsBase.EntityName);
				query.AddEquals(TemplateFieldsPropertyNames.TemplateId, Identity);
				return EntityManager.Select(query);
			}
		}

		#endregion
	}
}
