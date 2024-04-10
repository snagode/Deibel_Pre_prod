using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the BATCH_HEADER entity.
	/// </summary>
	[SampleManagerEntity(BatchHeaderBase.EntityName)]
	public class BatchHeader : BatchHeaderBase
	{
		/// <summary>
		/// Gets the batch failures.
		/// </summary>
		/// <value>
		/// The batch failures.
		/// </value>
		[PromptCollection(RBatchFailures.EntityName, false)]
		public IEntityCollection BatchFailures
		{
			get
			{
				IQuery query = EntityManager.CreateQuery(RBatchFailures.EntityName);
				query.AddEquals(RBatchFailuresPropertyNames.Identity
					, this);
				return EntityManager.Select(query);
			}
		}

		/// <summary>
		/// Gets the batch failures.
		/// </summary>
		/// <value>
		/// The batch failures.
		/// </value>
		[PromptCollection(BatchEntry.EntityName, false)]
		public IEntityCollection BatchEntries
		{
			get
			{
				IQuery query = EntityManager.CreateQuery(BatchEntry.EntityName);
				query.AddEquals(BatchEntryPropertyNames.Identity
					, this);
				return EntityManager.Select(query);
			}
		}

		/// <summary>
		/// Gets the QA status.
		/// </summary>*
		/// <value>
		/// The QA status.
		/// </value>
		[PromptText()]
		public string QaStatus
		{
			get
			{
				IQuery query = EntityManager.CreateQuery(BatchEntry.EntityName);
				query.AddEquals(BatchEntryPropertyNames.Identity
					, this);
				query.AddEquals(BatchEntryPropertyNames.ReviewStatus
					, PhraseLimitComp.PhraseIdF);
				if (EntityManager.SelectCount(query) > 0) return PhrasePassFail.PhraseIdFAIL;
				return PhrasePassFail.PhraseIdPASS;
			}
		}
	}
}
