using System;
using Thermo.SampleManager.Common.Data;
using Thermo.Framework.Core;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the INSTRUMENT_PART_LINK_HISTORY entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class InstrumentPartLinkHistory : BaseEntity
	{
		#region Public Constants

		/// <summary>
		/// INSTRUMENT_PART_LINK_HISTORY virtual Entity
		/// </summary>
		public const string EntityName = "INSTRUMENT_PART_LINK_HISTORY";

		#endregion

		#region Member Variables

		private NullableDateTime m_EventDate;
		private string m_EventType;
		private InstrumentPartBase m_InstrumentPart;
		private InstrumentPartTemplateBase m_InstrumentPartTemplate;
		private bool m_Mandatory;

		#endregion

		#region Properties

		/// <summary>
		/// Structure Field EVENT_DATE
		/// </summary>
		[PromptDate]
		public virtual NullableDateTime EventDate
		{
			get { return m_EventDate; }
			set { m_EventDate = value; }
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
		/// Links to Type InstrumentPartBase
		/// </summary>
		[PromptLink]
		public virtual InstrumentPartBase InstrumentPart
		{
			get { return m_InstrumentPart; }
			set { m_InstrumentPart = value; }
		}

		/// <summary>
		/// Links to Type InstrumentPartTemplateBase
		/// </summary>
		[PromptLink]
		public virtual InstrumentPartTemplateBase InstrumentPartTemplate
		{
			get { return m_InstrumentPartTemplate; }
			set { m_InstrumentPartTemplate = value; }
		}

		/// <summary>
		/// Gets a value indicating whether the part is mandatory
		/// </summary>
		/// <value><c>true</c> if mandatory; otherwise, <c>false</c>.</value>
		[PromptBoolean]
		public bool Mandatory
		{
			get { return m_Mandatory; }
			set { m_Mandatory = value; }
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
		static public IEntityCollection BuildHistory(IEntityManager entityManager, string tableName, string identity)
		{
			// Select the audit records
			IQuery auditEventQuery = entityManager.CreateQuery(TableNames.AuditValues);

			auditEventQuery.AddEquals("TABLE_NAME", tableName);
			auditEventQuery.AddLike("RECORD_KEY0", identity + "%");

			// Only select the fields we're interested in
			auditEventQuery.PushBracket();

			auditEventQuery.AddEquals("FIELD_NAME", "INSTRUMENT_PART");
			auditEventQuery.AddOr();
			auditEventQuery.AddEquals("FIELD_NAME", "INSTRUMENT_PART_TEMP");
			auditEventQuery.AddOr();
			auditEventQuery.AddEquals("FIELD_NAME", "MANDATORY");

			auditEventQuery.PopBracket();

			auditEventQuery.AddOrder("EVENT", true);
			auditEventQuery.AddOrder("ORDER_NUM", true);

			IEntityCollection auditValuesCollection = entityManager.Select(TableNames.AuditValues, auditEventQuery);

			// Build the history collection from the audit tables
			IEntityCollection historyCollection = entityManager.CreateEntityCollection(EntityName);

			PackedDecimal currentEvent = 0;
			InstrumentPartLinkHistory ipHistory = null;

			foreach (AuditValues auditValue in auditValuesCollection)
			{
				if ((auditValue.Event.Event != currentEvent) || (ipHistory == null))
				{
					if (currentEvent != 0)
					{
						historyCollection.Add(ipHistory);
					}

					currentEvent = auditValue.Event.Event;
					ipHistory = (InstrumentPartLinkHistory)entityManager.CreateEntity(EntityName);
					ipHistory.m_EventDate = auditValue.AuditDate;
					ipHistory.m_EventType = auditValue.AuditAction;
				}

				switch (auditValue.FieldName.Trim())
				{
					case "INSTRUMENT_PART":
						if (!BlankValue(auditValue.ValueAfter))
							ipHistory.m_InstrumentPart = (InstrumentPart)entityManager.Select(TableNames.InstrumentPart, auditValue.ValueAfter);
						else if (!BlankValue(auditValue.ValueBefore))
							ipHistory.m_InstrumentPart = (InstrumentPart)entityManager.Select(TableNames.InstrumentPart, auditValue.ValueBefore);
						break;

					case "INSTRUMENT_PART_TEMP":
						if (!BlankValue(auditValue.ValueAfter))
							ipHistory.m_InstrumentPartTemplate = (InstrumentPartTemplate)entityManager.Select(TableNames.InstrumentPartTemplate, auditValue.ValueAfter);
						else if (!BlankValue(auditValue.ValueBefore))
							ipHistory.m_InstrumentPartTemplate = (InstrumentPartTemplate)entityManager.Select(TableNames.InstrumentPartTemplate, auditValue.ValueBefore);
						break;

					case "MANDATORY":
						if (!BlankValue(auditValue.ValueAfter))
							ipHistory.m_Mandatory = (auditValue.ValueAfter.ToUpper().CompareTo("TRUE") == 0);
						else if (!BlankValue(auditValue.ValueBefore))
							ipHistory.m_Mandatory = (auditValue.ValueBefore.ToUpper().CompareTo("TRUE") == 0);
						break;

					default:
						break;
				}
			}

			if (currentEvent != 0)
			{
				historyCollection.Add(ipHistory);
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
