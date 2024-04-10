using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the BATCH_TMPL_HEADER entity.
	/// </summary>
	[SampleManagerEntity(BatchTmplHeaderBase.EntityName)]
	public class BatchTmplHeader : BatchTmplHeaderBase
	{
		#region Properties

		/// <summary>
		/// Gets the fields.
		/// </summary>
		/// <value>
		/// The fields.
		/// </value>
		[PromptCollection(TemplateFieldsBase.EntityName, false)]
		public IEntityCollection Fields
		{
			get
			{
				IQuery query = EntityManager.CreateQuery(TemplateFieldsBase.EntityName);
				query.AddEquals(TemplateFieldsPropertyNames.TableName, BatchHeaderBase.EntityName);
				query.AddEquals(TemplateFieldsPropertyNames.TemplateId, Identity);
				return EntityManager.Select(query);
			}
		}

		/// <summary>
		/// Gets the component list.
		/// </summary>
		/// <value>
		/// The component list.
		/// </value>
		[PromptCollection(RBatchTmplComponentsListBase.EntityName, false)]
		public IEntityCollection ComponentList
		{
			get
			{
				IQuery query = EntityManager.CreateQuery(RBatchTmplComponentsListBase.EntityName);
				query.AddEquals(RBatchTmplComponentsListPropertyNames.Identity, Identity);
				return EntityManager.Select(query);
			}
		}

		/// <summary>
		/// Gets the entries.
		/// </summary>
		/// <value>
		/// The entries.
		/// </value>
		[PromptCollection(BatchTmplEntryBase.EntityName, false)]
		public IEntityCollection Entries
		{
			get
			{
				IQuery query = EntityManager.CreateQuery(BatchTmplEntryBase.EntityName);
				query.AddEquals(BatchTmplEntryPropertyNames.Identity, Identity);
				return EntityManager.Select(query);
			}
		}

		/// <summary>
		/// Gets the SQC headers.
		/// </summary>
		/// <value>
		/// The SQC headers.
		/// </value>
		[PromptCollection(BatchSqcHeaderBase.EntityName, false)]
		public IEntityCollection SqcHeaders
		{
			get
			{
				IEntityCollection headers = EntityManager.CreateEntityCollection(BatchSqcHeaderBase.EntityName);
				foreach (BatchTmplSqc batchTmplSqc in this.BatchTmplSqcs)
				{
					headers.Add(batchTmplSqc.Chart);
				}

				return headers;
			}
		}

		#endregion
	}
}