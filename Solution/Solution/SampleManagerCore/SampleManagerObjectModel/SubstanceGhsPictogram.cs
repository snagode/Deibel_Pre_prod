using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the SUBSTANCE_GHS_PICTOGRAM entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class SubstanceGhsPictogram : SubstanceGhsPictogramBase
	{
		#region Properties

		/// <summary>
		/// Return the icon for the hazard symbol
		/// </summary>
		[EntityIcon]
		public string SubstanceGhsPictogramIcon
		{
			get
			{
				if (Pictogram.Icon.Identity == "")
					return "ICON_BLANK";

				return Pictogram.Icon.Identity;
			}
		}

		#endregion
	}
}

