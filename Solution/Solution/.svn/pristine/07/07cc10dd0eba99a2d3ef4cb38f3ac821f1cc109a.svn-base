using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the INCIDENT_TEMPLATE entity.
	/// </summary>
	[SampleManagerEntity(IncidentTemplateBase.EntityName)]
	public class IncidentTemplate : IncidentTemplateBase
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
				query.AddEquals(TemplateFieldsPropertyNames.TableName, IncidentsBase.EntityName);
				query.AddEquals(TemplateFieldsPropertyNames.TemplateId, TemplateId);
				return EntityManager.Select(query);
			}
		}

		#endregion
	}
}
