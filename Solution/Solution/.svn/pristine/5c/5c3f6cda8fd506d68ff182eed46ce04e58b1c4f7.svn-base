using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the SAMPLE_PREP_WORKSHEET entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class SamplePrepWorksheet:SamplePrepWorksheetBase
	{
		/// <summary>
		/// Gets the samples.
		/// </summary>
		/// <value>
		/// The samples.
		/// </value>
		[PromptCollection(Sample.EntityName, false, StopAutoPublish = true)]
		public IEntityCollection Samples
		{
			get
			{
				var q = EntityManager.CreateQuery(Sample.EntityName);
				q.AddEquals(SamplePropertyNames.LinkNumber, LinkNumber);
				return EntityManager.Select(q);
			}
		}


		/// <summary>
		/// Gets the analysis.
		/// </summary>
		/// <value>
		/// The analysis.
		/// </value>
		[PromptLink(Preparation.EntityName, false)]
		public Preparation Preparation
		{
			get
			{
				if (Samples.Count == 0) return null;
				return (Preparation)((Sample)(Samples.GetFirst())).Preparation;
			}
		}


		/// <summary>
		/// Gets the Results.
		/// </summary>
		/// <value>
		/// The Results.
		/// </value>
		[PromptCollection(Result.EntityName, false, StopAutoPublish = true)]
		public IEntityCollection Results
		{
			get
			{
				var collection = EntityManager.CreateEntityCollection(Result.EntityName);
				foreach (Sample sample in Samples)
				{
					foreach (Test test in sample.Tests)
					{
						if (test.Results.Count == 0)
						{
							foreach (VersionedComponent component in test.Analysis.Components)
							{
								Result result = (Result)EntityManager.CreateEntity(Result.EntityName);
								result.ResultName = component.Name;
								result.ResultType = component.ResultType.PhraseText;
								result.Units = component.Units;
								result.TestNumber = test;
								collection.Add(result);
							}
						}

						foreach (Result result in test.Results)
						{
							collection.Add(result);
						}
					}
				}
				return collection;
			}
		}
	}
}
