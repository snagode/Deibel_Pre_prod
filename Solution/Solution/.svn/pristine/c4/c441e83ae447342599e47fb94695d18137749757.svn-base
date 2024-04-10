using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the INSPECTOR entity.
	/// </summary>
	[SampleManagerEntity(InspectorBase.EntityName)]
	public class Inspector : InspectorBase
	{
		#region Properties

		/// <summary>
		/// Gets the inspector icon.
		/// </summary>
		/// <value>The inspector icon.</value>
		[EntityIcon]
		public string InspectorIcon
		{
			get { return "INT_NOTE"; }
		}

		/// <summary>
		/// Gets a longer status value
		/// </summary>
		/// <value>The status long.</value>
		[PromptText]
		public string StatusLong
		{
			get
			{
				switch(Status)
				{
					case "I": return "Pending";
					case "V": return "Due Now";
					case "C": return "Completed";
					default: return Status;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is completed
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is pending; otherwise, <c>false</c>.
		/// </value>
		[PromptBoolean]
		public bool IsPending
		{
			get
			{
				return (Status == "V" || Status == "I");
			}
		}

		/// <summary>
		/// Gets a value indicating whether this inspection has been completed.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is completed; otherwise, <c>false</c>.
		/// </value>
		[PromptBoolean]
		public bool IsCompleted
		{
			get { return !IsPending;  }
		}

		#endregion
	}
}
