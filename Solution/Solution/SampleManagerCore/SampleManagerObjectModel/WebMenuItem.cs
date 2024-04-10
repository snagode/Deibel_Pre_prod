using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the WEB_MENU_ITEM entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class WebMenuItem : WebMenuItemBase
	{
		#region Overrides

		/// <summary>
		/// Gets or sets the option number.
		/// </summary>
		/// <value>The option number.</value>
		public override MasterMenuBase OptionNumber
		{
			get { return base.OptionNumber; }
			set
			{
				base.OptionNumber = value;
				SetDefaults();
			}
		}

		#endregion

		#region Default Values

		/// <summary>
		/// Sets the defaults.
		/// </summary>
		private void SetDefaults()
		{
			WebMenuItemName = OptionNumber.ShortText;
			Icon = OptionNumber.Icon;
		}

		#endregion
	}
}