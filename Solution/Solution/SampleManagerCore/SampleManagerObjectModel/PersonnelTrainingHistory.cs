using System;
using Thermo.SampleManager.Common.Data;
using Thermo.Framework.Core;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the PERSONNEL_TRAINING_HISTORY entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class PersonnelTrainingHistory : BaseEntity
	{
		#region Public Constants

		/// <summary>
		/// PERSONNEL_TRAINING_HISTORY virtual Entity
		/// </summary>
		public const string EntityName = "PERSONNEL_TRAINING_HISTORY";

		#endregion

		#region Member Variables

		private NullableDateTime m_ModificationDate;
		private PhraseBase m_Competence;
		private NullableDateTime m_DateCompleted;
		private TrainingCourseBase m_TrainingCourse;
		private string m_EventType;

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
		/// Phrase of Type TRAIN_COMP
		/// </summary>
		[PromptValidPhrase(PhraseTrainComp.Identity, false)]
		public virtual PhraseBase Competence
		{
			get { return m_Competence; }
			set { m_Competence = value; }
		}

		/// <summary>
		/// Structure Field DATE_COMPLETED
		/// </summary>
		[PromptDate]
		public virtual NullableDateTime DateCompleted
		{
			get { return m_DateCompleted; }
			set { m_DateCompleted = value; }
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
		/// Return the event type - INSERT, MODIFY, DELETE.
		/// </summary>
		[PromptText]
		public virtual string EventType
		{
			get { return m_EventType; }
			set { m_EventType = value; }
		}

		#endregion

		#region Build History

		/// <summary>
		/// Select all the audit records for passed personnel entry and build up its history records.
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="personnelEntry"></param>
		/// <returns>Collection with one entry per audit event</returns>
		static public IEntityCollection BuildHistory(IEntityManager entityManager, Personnel personnelEntry)
		{
			// Select the audit records
			IQuery auditEventQuery = entityManager.CreateQuery(TableNames.AuditValues);

			auditEventQuery.AddEquals("TABLE_NAME", TableNames.PersonnelTraining);
			auditEventQuery.AddLike("RECORD_KEY0", personnelEntry.Identity.PadRight(10) + "%");

			// Only select the fields we're interested in
			auditEventQuery.PushBracket();

			auditEventQuery.AddEquals("FIELD_NAME", "TRAINING_COURSE");
			auditEventQuery.AddOr();
			auditEventQuery.AddEquals("FIELD_NAME", "COMPETENCE");
			auditEventQuery.AddOr();
			auditEventQuery.AddEquals("FIELD_NAME", "DATE_COMPLETED");

			auditEventQuery.PopBracket();

			auditEventQuery.AddOrder("EVENT", true);
			auditEventQuery.AddOrder("ORDER_NUM", true);

			IEntityCollection auditValuesCollection = entityManager.Select(TableNames.AuditValues, auditEventQuery);

			// Build the history collection from the audit tables
			IEntityCollection historyCollection = entityManager.CreateEntityCollection(EntityName);

			PackedDecimal currentEvent = 0;
			PersonnelTrainingHistory ptHistory = null;

			foreach (AuditValues auditValue in auditValuesCollection)
			{
				if ((auditValue.Event.Event != currentEvent) || (ptHistory == null))
				{
					if (currentEvent != 0)
					{
						historyCollection.Add(ptHistory);
					}

					currentEvent = auditValue.Event.Event;
					ptHistory = (PersonnelTrainingHistory)entityManager.CreateEntity(EntityName);
					ptHistory.m_ModificationDate = auditValue.AuditDate;
					ptHistory.m_EventType = auditValue.AuditAction;
				}

				switch (auditValue.FieldName.Trim())
				{
					case "TRAINING_COURSE":
						if (!BlankValue(auditValue.ValueAfter))
							ptHistory.m_TrainingCourse = (TrainingCourse)entityManager.Select(TableNames.TrainingCourse, auditValue.ValueAfter);
						else if (!BlankValue(auditValue.ValueBefore))
							ptHistory.m_TrainingCourse = (TrainingCourse)entityManager.Select(TableNames.TrainingCourse, auditValue.ValueBefore);
						break;

					case "COMPETENCE":
						if (!BlankValue(auditValue.ValueAfter))
							ptHistory.m_Competence = (PhraseBase)entityManager.SelectPhrase(PhraseTrainComp.Identity, auditValue.ValueAfter);
						break;

					case "DATE_COMPLETED":
						if (!BlankValue(auditValue.ValueAfter))
							ptHistory.m_DateCompleted = DateTime.Parse(auditValue.ValueAfter);
						break;
				}
			}

			if (currentEvent != 0)
			{
				historyCollection.Add(ptHistory);
			}

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
