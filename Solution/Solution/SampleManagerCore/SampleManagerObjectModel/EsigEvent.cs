using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;
namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the ESIG_EVENT entity.
	/// </summary>
	[SampleManagerEntity(EsigEventBase.EntityName)]
	public class EsigEvent : EsigEventBase
	{
		/// <summary>
		/// Links to Type LocationBase
		/// </summary>
		/// <value></value>
		[PromptCollection(EsigData.EntityName, false)]
		public IEntityCollection Data
		{
			get
			{
				IQuery query = EntityManager.CreateQuery(EsigData.EntityName);
				query.AddEquals(EsigDataPropertyNames.EsigEventId,this.EsigEventId);
				return EntityManager.Select(query);
			}
		}
	}
}
