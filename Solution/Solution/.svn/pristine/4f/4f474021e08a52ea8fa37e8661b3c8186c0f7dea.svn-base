using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the INCIDENTS entity.
	/// </summary>
	[SampleManagerEntity(IncidentsBase.EntityName)]
	public class Incidents : IncidentsBase
	{
		#region Properties

		/// <summary>
		/// Links to Type LocationBase
		/// </summary>
		/// <value></value>
		[PromptHierarchyLink(LocationBase.EntityName, true, Location.HierarchyPropertyName)]
		public override LocationBase LocationId
		{
			get
			{
				return base.LocationId;
			}
			set
			{
				base.LocationId = value;
			}
		}

		#endregion
	}
}
