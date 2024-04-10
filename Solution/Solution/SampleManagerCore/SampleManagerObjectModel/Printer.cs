using System.Net.Mail;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the PRINTER entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class Printer : PrinterInternal
	{
		#region Properties

		/// <summary>
		/// Links to Type LocationBase
		/// </summary>
		/// <value></value>
		[PromptHierarchyLink(LocationBase.EntityName, true, ObjectModel.Location.HierarchyPropertyName)]
		public override LocationBase Location
		{
			get
			{
				return base.Location;
			}
			set
			{
				base.Location = value;
			}
		}

		#endregion
	}
}
