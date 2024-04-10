using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the CHROMELEON_PROPERTY entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class ChromeleonPropertyEntity : ChromeleonPropertyBase
	{
		#region Overrides

		/// <summary>
		/// Entity Loaded Event
		/// </summary>
		protected override void OnEntityLoaded()
		{
			base.OnEntityLoaded();
			PropertyChanged += ChromeleonProperty_PropertyChanged;
		}

		#endregion

		#region Property Change Events

		/// <summary>
		/// Handles the PropertyChanged event of the ChromeleonProperty control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PropertyEventArgs"/> instance containing the event data.</param>
		private void ChromeleonProperty_PropertyChanged(object sender, PropertyEventArgs e)
		{
			if (e.PropertyName == ChromeleonPropertyPropertyNames.TableName)
			{
				FieldName = string.Empty;
				Property = string.Empty;
				SetEntity(PhraseChromEnt.PhraseIdINJECTION);
			}
		}

		#endregion
	}
}
