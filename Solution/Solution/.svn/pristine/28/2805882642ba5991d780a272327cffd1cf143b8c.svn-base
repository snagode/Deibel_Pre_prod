using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the SAMPLE_POINT entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class SamplePoint : SamplePointBase
	{
		#region Properties

		/// <summary>
		/// Links to Type LocationBase
		/// </summary>
		/// <value></value>
		[PromptHierarchyLink(Location.EntityName, true, Location.HierarchyPropertyName)]
		public override LocationBase PointLocation
		{
			get
			{
				return base.PointLocation;
			}
			set
			{
				base.PointLocation = value;
			}
		}

		/// <summary>
		/// Gets the samples.
		/// </summary>
		/// <value>
		/// The samples.
		/// </value>
		[PromptCollection(SampTestViewBase.EntityName, false)]
		public IEntityCollection Samples
		{
			get
			{
				var q = EntityManager.CreateQuery(SampTestViewBase.EntityName);
				q.AddEquals(SampTestViewPropertyNames.SamplingPoint, Identity);
				return EntityManager.Select(q);
			}
		}

		#endregion
	}
}