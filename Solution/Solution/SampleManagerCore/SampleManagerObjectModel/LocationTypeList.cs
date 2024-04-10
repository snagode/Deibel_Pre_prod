using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Defines extended business logic and manages access to the LOCATION entity.
	/// </summary>
	[SampleManagerEntity(LocationTypeListBase.EntityName)]
	public class LocationTypeList : LocationTypeListBase
	{
		#region Properties

		/// <summary>
		/// Gets the location icon. As the location type list is used within the explorer to
		/// show hierarchy, it needs to have an icon property to display icons.
		/// </summary>
		/// <value>The location icon.</value>
		[EntityIcon]
		public string LocationIcon
		{
			get { return LocationType.DefaultIcon.Identity; }
		}

		#endregion
	}
}
