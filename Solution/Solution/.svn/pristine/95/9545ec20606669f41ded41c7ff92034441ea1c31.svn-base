using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Selected Values
	/// </summary>
	[DataContract(Name="selectedValues")]
	[KnownType(typeof(Interval))]
	public class SelectedValues : MobileObject
	{
		#region Properties

		/// <summary>
		/// Gets or sets the selected entities.
		/// </summary>
		/// <value>
		/// The entities.
		/// </value>
		[DataMember(Name = "selected")]
		public List<string> Selected { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="SelectedValues"/> class.
		/// </summary>
		public SelectedValues()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SelectedValues"/> class.
		/// </summary>
		/// <param name="initialise">if set to <c>true</c> [initialise].</param>
		public SelectedValues(bool initialise)
		{
			if (!initialise) return;
			Selected = new List<string>();
		}

		#endregion
	}
}
