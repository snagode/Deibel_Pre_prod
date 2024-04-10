using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the SAMPLE_WORKSHEET entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class SampleWorksheet : SampleWorksheetBase
	{
		/// <summary>
		/// Gets the approvals.
		/// </summary>
		/// <value>
		/// The approvals.
		/// </value>
		[PromptCollection(RSampleWorksheet.EntityName, false)]
		public IEntityCollection Entries
		{
			get
			{
				var q = EntityManager.CreateQuery(RSampleWorksheet.EntityName);
				q.AddEquals(RSampleWorksheetPropertyNames.Worksheet, Identity);
				return EntityManager.Select(q);
			}
		}
	}
}