using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the INSTRUMENT_PROPERTY entity.
	/// </summary>
	[SampleManagerEntity(InstrumentPropertyBase.EntityName)]
	public class InstrumentProperty : InstrumentPropertyBase
	{
		#region Constants

		private const string PropertyTypeCalibrationResult = "CALIBRES";

		#endregion

		#region Overrides

		/// <summary>
		/// Called when when a property changes.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged(PropertyEventArgs e)
		{
			switch (e.PropertyName)
			{
				case InstrumentPropertyPropertyNames.PropertyType:
					if (PropertyType.PhraseId == PropertyTypeCalibrationResult)
					{
						Value = string.Empty;
						Units = null;
					}

					break;
			}
		}

		#endregion
	}
}
