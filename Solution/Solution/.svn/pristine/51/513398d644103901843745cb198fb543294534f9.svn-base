using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the AUDIT_EVENT entity.
	/// </summary>
	[SampleManagerEntity(AuditEventBase.EntityName)]
	public class AuditEvent : AuditEventBase
	{
		/// <summary>
		/// Gets the audit data collection.
		/// </summary>
		/// <value>
		/// The audit data collection.
		/// </value>
		[PromptCollection(AuditData.EntityName, false)]
		public IEntityCollection AuditDataCollection
		{
			get
			{
				IQuery query = EntityManager.CreateQuery(AuditData.EntityName);
				query.AddEquals(AuditDataPropertyNames.Event
					, this);
				return EntityManager.Select(query);
			}
		}
	}
}
