using System;
using System.Globalization;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the SAMPLE entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class Sample : SampleInternal
	{
		#region Properties

		/// <summary>
		/// Gets a value indicating whether this instance is trained for preparation with regards for the current user.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is trained for preparation; otherwise, <c>false</c>.
		/// </value>
		[PromptBoolean]
		public bool IsTrainedForPreparation
		{
			get
			{
				if (!Preparation.IsNull())
				{
					return ((Preparation) Preparation).IsTrained;
				}
				return true;
			}
		}

		/// <summary>
		/// Links to Type LocationBase
		/// </summary>
		/// <value></value>
		[PromptHierarchyLink(Location.EntityName, true, Location.HierarchyPropertyName)]
		public override LocationBase LocationId
		{
			get
			{
				return base.LocationId;
			}
			set
			{
				base.LocationId = value;
			}
		}

		/// <summary>
		/// Gets the results.
		/// </summary>
		/// <value>
		/// The results.
		/// </value>
		[PromptCollection(Result.EntityName,false)]
		public IEntityCollection Results
		{
			get
			{
				IEntityCollection returnCollection = EntityManager.CreateEntityCollection(Result.EntityName, false);
				foreach (Test test in Tests)
				{
					foreach (Result result in test.Results)
					{
						returnCollection.Add(result);
					}
				}
				return returnCollection;
			}
		}

		/// <summary>
		/// Gets the results.
		/// </summary>
		/// <value>
		/// The results.
		/// </value>
		[PromptInteger]
		public int AgeDays
		{
			get
			{
				return (DateTime.Today - LoginDate.ToDateTime(CultureInfo.CurrentCulture)).Days;
			}
		}

		/// <summary>
		/// Gets the state of the preperation.
		/// </summary>
		/// <value>
		/// The state of the preperation.
		/// </value>
		[PromptText]
		public string PreparationState
		{
			get
			{
				if (Status.PhraseId != PhraseSampStat.PhraseIdW)
					return Library.Message.GetMessage("ReportTemplateMessages", "PreparationComplete");
				return string.Empty;
			}
		}
		#endregion
		
		#region Tests

		/// <summary>
		/// Determines whether the specified test is assigned to this sample.
		/// </summary>
		/// <param name="analysis">The analysis.</param>
		/// <returns>
		/// 	<c>true</c> if the test is assigned, otherwise, <c>false</c>.
		/// </returns>
		public bool IsTestAssigned(string analysis)
		{
			foreach (TestInternal test in Tests)
			{
				if (test.Analysis.VersionedAnalysisName == analysis)
				{
					return true;
				}
			}

			return false;
		}

		#endregion
	}
}