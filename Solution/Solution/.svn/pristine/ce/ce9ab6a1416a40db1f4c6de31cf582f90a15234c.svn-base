using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the JOB_HEADER entity.
	/// </summary>
	[SampleManagerEntity(JobHeaderBase.EntityName)]
	public class JobHeader : JobHeaderInternal
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
				query.AddEquals(TemplateFieldsPropertyNames.TableName, EntityName);
				query.AddEquals(TemplateFieldsPropertyNames.TemplateId, TemplateId);
				return EntityManager.Select(query);
			}
		}

		/// <summary>
		/// Gets the fields.
		/// </summary>
		/// <value>
		/// The fields.
		/// </value>
		[PromptCollection(SampTestResultBase.EntityName, false)]
		public IEntityCollection JobSampTestResult
		{
			get
			{
				IQuery query = EntityManager.CreateQuery(SampTestResultBase.EntityName);
				query.AddEquals(SampTestResultPropertyNames.JobName, JobName);
				return EntityManager.Select(query);
			}
		}

		#endregion
	}
}
