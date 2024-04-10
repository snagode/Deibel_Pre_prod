using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Internal.ObjectModel;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the LOT_DETAILS entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class LotDetails : LotDetailsInternal
	{
		#region Inventory Properties

		/// <summary>
		/// Gets or sets the inventory comment.
		/// </summary>
		/// <value>
		/// The inventory comment.
		/// </value>
		[PromptText(255)]
		public string InventoryComment { get; set; }

		/// <summary>
		/// Gets or sets the inventory quantity.
		/// </summary>
		/// <value>
		/// The inventory quantity.
		/// </value>
		[PromptText(255)]
		public string InventoryDisplayQuantityReserved { get; set; }

		#endregion

	}
}
