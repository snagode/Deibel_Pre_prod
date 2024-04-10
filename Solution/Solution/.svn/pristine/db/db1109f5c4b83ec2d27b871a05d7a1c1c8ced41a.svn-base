using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Web Menu Section Task
	/// </summary>
	[SampleManagerTask("WebMenuSectionTask", "LABTABLE", "WEB_MENU_SECTION")]
	public class WebMenuSectionTask : GenericLabtableTask
	{
		#region Overrides

		/// <summary>
		/// Forces user to specify menu items before saving the entity
		/// </summary>
		/// <returns></returns>
		protected override bool OnPreSave()
		{
			WebMenuSection webMenuSection = (WebMenuSection) MainForm.Entity;
			FormWebMenuSection webForm = (FormWebMenuSection) MainForm;

			if (webMenuSection.WebMenuItems.ActiveCount == 0)
			{
				Library.Utils.FlashMessage(webForm.StringTable.NoMenuItemsMessage, webForm.StringTable.NoMenuItemsTitle);
				return false;
			}

			return base.OnPreSave();
		}

		#endregion
	}
}