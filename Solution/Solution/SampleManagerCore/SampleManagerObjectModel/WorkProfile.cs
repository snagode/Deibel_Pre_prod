using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the WORK_PROFILE entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class WorkProfile : WorkProfileBase
	{
		/// <summary>
		/// Gets the work profile components list.
		/// </summary>
		/// <value>
		/// The work profile components list.
		/// </value>
		[PromptCollection(RWorkProfileComponentsListBase.EntityName, false)]
		public IEntityCollection WorkProfileComponentsList
		{
			get
			{
				var q = EntityManager.CreateQuery(RWorkProfileComponentsListBase.EntityName);
				q.AddEquals(RWorkProfileComponentsListPropertyNames.Identity, Identity);
				return EntityManager.Select(q);
			}
		}
	}
}