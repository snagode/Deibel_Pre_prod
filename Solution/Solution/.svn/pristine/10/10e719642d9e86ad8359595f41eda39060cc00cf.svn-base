using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the VERSIONED_C_L_ENTRY entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class VersionedCLEntry : VersionedCLEntryBase
	{
		#region Public Constants

		/// <summary>
		/// Selected Property Name
		/// </summary>
		public const string SelectedProperty = "Selected";

		#endregion

		#region Member Variables

		private bool m_Selected = true;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="VersionedCLEntry"/> is selected.
		/// </summary>
		/// <value><c>true</c> if selected; otherwise, <c>false</c>.</value>
		[PromptBoolean]
		public bool Selected
		{
			get { return m_Selected; }
			set
			{
				m_Selected = value;
				NotifyPropertyChanged(SelectedProperty);
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Update using the values of another V CL Entry.
		/// </summary>
		/// <param name="other">The other.</param>
		public void UpdateFromOther(VersionedCLEntry other)
		{
			VersionedCLEntryName = other.VersionedCLEntryName;
			DefaultValue = other.DefaultValue;
			ReplicateCount = other.ReplicateCount;
			SetSelectedQuietly(other.Selected);
		}

		/// <summary>
		/// Sets the selected quietly.
		/// </summary>
		/// <param name="selected">if set to <c>true</c> [selected].</param>
		public void SetSelectedQuietly(bool selected)
		{
			m_Selected = selected;
		}

		#endregion
	}
}