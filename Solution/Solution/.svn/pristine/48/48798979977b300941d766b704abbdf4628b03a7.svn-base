using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the WORKSHEET entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class Worksheet : WorksheetBase
	{
		/// <summary>
		/// Gets the tests.
		/// </summary>
		/// <value>
		/// The tests.
		/// </value>
		[PromptCollection(TestBase.EntityName, false)]
		public IEntityCollection Tests
		{
			get
			{
				var q = EntityManager.CreateQuery(TestBase.EntityName);
				q.AddEquals(TestPropertyNames.Worksheet, LinkNumber);
				return EntityManager.Select(q);
			}
		}
		
		/// <summary>
		/// Gets the preparation.
		/// </summary>
		/// <value>
		/// The preparation.
		/// </value>
		[PromptHierarchyLink(Preparation.EntityName)]
		public Preparation Preparation
		{
			get
			{
				if (Tests.Count > 0)
				{
					return ((Test) (Tests.GetFirst())).Preparation as Preparation;
				}

				return null;
			}
		}
	}
}