using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the STANDARD_VERSIONS entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class StandardVersions : StandardVersionsBase
	{
		/// <summary>
		/// Gets the remaining usage.
		/// </summary>
		/// <value>
		/// The remaining usage.
		/// </value>
		[PromptInteger]
		public int RemainingUsage
		{
			get { return MaximumUsage - UsageCount; }
		}
	}
}