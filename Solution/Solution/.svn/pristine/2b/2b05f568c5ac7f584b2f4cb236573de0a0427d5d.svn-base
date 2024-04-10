using System;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the TRAINING_HISTORY entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class TrainingHistory : BaseEntity
	{
		#region Public Constants

		/// <summary>
		/// TRAINING_HISTORY virtual Entity
		/// </summary>
		public const string EntityName = "TRAINING_HISTORY";

		#endregion

		#region Member Variables

		private string m_EventType;
		private PhraseBase m_MinimumCompetence;
		private NullableDateTime m_ModificationDate;
		private TrainingCourseBase m_TrainingCourse;

		#endregion

		#region Properties

		/// <summary>
		/// Structure Field MODIFICATION_DATE
		/// </summary>
		[PromptDate]
		public virtual NullableDateTime ModificationDate
		{
			get { return m_ModificationDate; }
			set { m_ModificationDate = value; }
		}

		/// <summary>
		/// Return the event type - INSERT, MODIFY, DELETE.
		/// </summary>
		[PromptText]
		public virtual string EventType
		{
			get { return m_EventType; }
			set { m_EventType = value; }
		}

		/// <summary>
		/// Links to Type TrainingCourseBase
		/// </summary>
		[PromptLink]
		public virtual TrainingCourseBase TrainingCourse
		{
			get { return m_TrainingCourse; }
			set { m_TrainingCourse = value; }
		}

		/// <summary>
		/// Phrase of Type TRAIN_COMP
		/// </summary>
		[PromptValidPhrase(PhraseTrainComp.Identity, false)]
		public virtual PhraseBase MinimumCompetence
		{
			get { return m_MinimumCompetence; }
			set { m_MinimumCompetence = value; }
		}

		#endregion

		#region Build History

		/// <summary>
		/// Select all the audit records for passed entry and build up its history records,
		/// identity must be formatted for use in audit record_key0 field
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="tableName"></param>
		/// <param name="identity"></param>
		/// <returns>Collection with one entry per audit event</returns>
		public static IEntityCollection BuildHistory(IEntityManager entityManager, string tableName, string identity)
		{
			// Select the audit records
			IQuery auditEventQuery = entityManager.CreateQuery(TableNames.AuditValues);

			auditEventQuery.AddEquals("TABLE_NAME", tableName);
			auditEventQuery.AddLike("RECORD_KEY0", identity + "%");

			// Only select the fields we're interested in
			auditEventQuery.PushBracket();

			auditEventQuery.AddEquals("FIELD_NAME", "TRAINING_COURSE");
			auditEventQuery.AddOr();
			auditEventQuery.AddEquals("FIELD_NAME", "MINIMUM_COMPETENCE");

			auditEventQuery.PopBracket();

			auditEventQuery.AddOrder("EVENT", true);
			auditEventQuery.AddOrder("ORDER_NUM", true);

			IEntityCollection auditValuesCollection = entityManager.Select(TableNames.AuditValues, auditEventQuery);

			// Build the history collection from the audit tables
			IEntityCollection historyCollection = entityManager.CreateEntityCollection(EntityName);

			PackedDecimal currentEvent = 0;
			TrainingHistory ptHistory = null;

			foreach (AuditValues auditValue in auditValuesCollection)
			{
				if ((auditValue.Event.Event != currentEvent) || (ptHistory == null))
				{
					if (currentEvent != 0)
						historyCollection.Add(ptHistory);

					currentEvent = auditValue.Event.Event;
					ptHistory = (TrainingHistory) entityManager.CreateEntity(EntityName);
					ptHistory.m_ModificationDate = auditValue.AuditDate;
					ptHistory.m_EventType = auditValue.AuditAction;
				}

				switch (auditValue.FieldName.Trim())
				{
					case "TRAINING_COURSE":
						if (!BlankValue(auditValue.ValueAfter))
						{
							ptHistory.m_TrainingCourse =
								(TrainingCourse) entityManager.Select(TableNames.TrainingCourse, auditValue.ValueAfter);
						}
						else if (!BlankValue(auditValue.ValueBefore))
						{
							ptHistory.m_TrainingCourse =
								(TrainingCourse) entityManager.Select(TableNames.TrainingCourse, auditValue.ValueBefore);
						}
						break;

					case "MINIMUM_COMPETENCE":
						if (!BlankValue(auditValue.ValueAfter))
						{
							ptHistory.m_MinimumCompetence =
								(PhraseBase) entityManager.SelectPhrase(PhraseTrainComp.Identity, auditValue.ValueAfter);
						}
						break;

					default:
						break;
				}
			}

			if (currentEvent != 0)
				historyCollection.Add(ptHistory);

			return (historyCollection);
		}

		#endregion

		#region Blank Value

		/// <summary>
		/// Returns true if the audit field has no value
		/// </summary>
		/// <param name="auditValue"></param>
		/// <returns></returns>
		private static bool BlankValue(string auditValue)
		{
			return (string.IsNullOrEmpty(auditValue)) || (auditValue.Trim() == String.Empty);
		}

		#endregion
	}
}