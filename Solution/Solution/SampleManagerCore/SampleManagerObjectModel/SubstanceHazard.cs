using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the SUBSTANCE_HAZARD entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
    public class SubstanceHazard : SubstanceHazardBase
	{
		#region Properties

		/// <summary>
		/// Return the icon for the hazard symbol
		/// </summary>
		[EntityIcon]
		public string SubstanceHazardIcon
		{
			get
			{
				if (Hazard.Icon.Identity == "")
					return "ICON_BLANK";

				return Hazard.Icon.Identity;
			}
		}

		#endregion
	}
}