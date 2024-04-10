using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the ANALYSIS_WORKSHEET entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class AnalysisWorksheet : AnalysisWorksheetBase
	{
		/// <summary>
		/// Gets the tests.
		/// </summary>
		/// <value>
		/// The tests.
		/// </value>
		[PromptCollection(Test.EntityName, false, StopAutoPublish = true)]
		public IEntityCollection Tests
		{
			get
			{
				var q = EntityManager.CreateQuery(Test.EntityName);
				q.AddEquals(TestPropertyNames.Worksheet, LinkNumber);
				q.AddOrder(TestPropertyNames.WorksheetPosition,true);
				return EntityManager.Select(q);
			}
		}

		/// <summary>
		/// Gets the analysis.
		/// </summary>
		/// <value>
		/// The analysis.
		/// </value>
		[PromptLink(VersionedAnalysis.EntityName, false)]
		public VersionedAnalysis Analysis
		{
			get
			{
				if (Tests.Count == 0) return null;
				return  (VersionedAnalysis) ((Test) (Tests.GetFirst())).Analysis;
			}
		}

		/// <summary>
		/// Gets the components.
		/// </summary>
		/// <value>
		/// The components.
		/// </value>
		[PromptCollection(VersionedComponent.EntityName, false, StopAutoPublish = true)]
		public IEntityCollection Components
		{
			get
			{
				if (Analysis == null) return EntityManager.CreateEntityCollection(VersionedComponent.EntityName);
				return ((VersionedAnalysis) Analysis).Components;
			}
		}

		/// <summary>
		/// Gets the tests.
		/// </summary>
		/// <value>
		/// The tests.
		/// </value>
		[PromptCollection(Result.EntityName, false, StopAutoPublish = true)]
		public IEntityCollection Results
		{
			get
			{
				var collection = EntityManager.CreateEntityCollection(Result.EntityName);
				foreach (Test test in Tests)
				{
					if (test.Results.Count == 0)
					{
						foreach (VersionedComponent component in test.Analysis.Components)
						{
							Result result = (Result) EntityManager.CreateEntity(Result.EntityName);
							result.ResultName = component.Name;
							result.ResultType = component.ResultType.PhraseText;
							result.TestNumber = test;
							collection.Add(result);
						}
					}

					foreach (Result result in test.Results)
					{
						collection.Add(result);
					}
				}
				return collection;
			}
		}

	}
}
