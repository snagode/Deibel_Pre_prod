using Thermo.Framework.Server;
using Thermo.Framework.Utilities;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the UNIT_HEADER entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class UnitHeader : UnitHeaderBase
	{
		#region Static Validation

		/// <summary>
		/// Checks that a value is valid for a property on this entity type.
		/// </summary>
		/// <param name="parent">The parent entity.</param>
		/// <param name="propertyName">The property.</param>
		/// <param name="propertyValue">The value.</param>
		/// <param name="errorMessage">The error message.</param>
		/// <returns></returns>
		/// <remarks>
		/// In certain cases, free text can be entered in reference fields.
		/// This method is called in such cases and allows validation of these
		/// free text values to take place at the EntityType level.
		/// </remarks>
		public new static bool CheckValue(IEntity parent, string propertyName, object propertyValue, out string errorMessage)
		{
			if (parent != null)
			{
				BaseEntity parentBase = (BaseEntity) parent;

				if (propertyValue is string)
				{
					bool isValid = parentBase.Library.Utils.UnitValidate((string) propertyValue);
					errorMessage = (isValid)
					               	? string.Empty
					               	: ServerMessageManager.Current.GetMessage("CommonMessages", "UnitHeaderCheckValue");
					return isValid;
				}
			}

			errorMessage = string.Empty;
			return false;
		}

		#endregion
	}
}