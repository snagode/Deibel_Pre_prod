using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the AUDIT_VIEW entity.
	/// </summary>
	[SampleManagerEntity(AuditViewBase.EntityName)]
	public class AuditView : AuditViewBase
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
					, Event);
				return EntityManager.Select(query);
			}
		}

	}
}
