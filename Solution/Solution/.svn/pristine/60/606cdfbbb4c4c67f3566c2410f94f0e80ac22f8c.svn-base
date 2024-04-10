using System;
using Thermo.SampleManager.Common.Data;
using Thermo.Framework.Core;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the INSTRUMENT_HISTORY entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class InstrumentHistory : BaseEntity
	{
		#region Public Constants

		/// <summary>
		/// INSTRUMENT_HISTORY virtual Entity
		/// </summary>
		public const string EntityName = "INSTRUMENT_HISTORY";

		#endregion

		#region Member Variables

		private NullableDateTime m_EventDate;
		private string m_EventType;
		private bool m_Available;
		private bool m_Retired;
		private PhraseBase m_Status;
		private bool m_RequiresCalibration;
		private string m_CalibrationDate;
		private bool m_RequiresService;
		private string m_ServiceDate;

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
		/// Gets a value indicating whether the instrument is available
		/// </summary>
		/// <value><c>true</c> if available; otherwise, <c>false</c>.</value>
		[PromptBoolean]
		public bool Available
		{
			get { return m_Available; }
			set { m_Available = value; }
		}

		/// <summary>
		/// Gets a value indicating whether the instrument is retired
		/// </summary>
		/// <value><c>true</c> if retired; otherwise, <c>false</c>.</value>
		[PromptBoolean]
		public bool Retired
		{
			get { return m_Retired; }
			set { m_Retired = value; }
		}

		/// <summary>
		/// Gets the status of the instrument
		/// </summary>
		/// <value>The instruments status.</value>
		[PromptValidPhrase(PhraseInstStat.Identity)]
		public PhraseBase Status
		{
			get { return m_Status; }
			set { m_Status = value; }
		}

		/// <summary>
		/// Gets a value indicating whether the instrument requires calibration
		/// </summary>
		/// <value><c>true</c> if requires calibration; otherwise, <c>false</c>.</value>
		[PromptBoolean]
		public bool RequiresCalibration
		{
			get { return m_RequiresCalibration; }
			set { m_RequiresCalibration = value; }
		}

		/// <summary>
		/// Gets the date as a string of the last calibration
		/// </summary>
		/// <value>The date as a string of the last calibration.</value>
		[PromptText]
		public string CalibrationDate
		{
			get { return m_CalibrationDate; }
			set { m_CalibrationDate = value; }
		}

		/// <summary>
		/// Gets a value indicating whether the instrument requires servicing
		/// </summary>
		/// <value><c>true</c> if requires servicing; otherwise, <c>false</c>.</value>
		[PromptBoolean]
		public bool RequiresService
		{
			get { return m_RequiresService; }
			set { m_RequiresService = value; }
		}

		/// <summary>
		/// Gets the date as a string of the last service
		/// </summary>
		/// <value>The date as a string of the last service.</value>
		[PromptText]
		public string ServiceDate
		{
			get { return m_ServiceDate; }
			set { m_ServiceDate = value; }
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
			auditEventQuery.AddEquals("RECORD_KEY0", identity);

			// Only select the fields we're interested in
			auditEventQuery.PushBracket();

			auditEventQuery.AddEquals("FIELD_NAME", "AVAILABLE");
			auditEventQuery.AddOr();
			auditEventQuery.AddEquals("FIELD_NAME", "RETIRED");
			auditEventQuery.AddOr();
			auditEventQuery.AddEquals("FIELD_NAME", "STATUS");
			auditEventQuery.AddOr();
			auditEventQuery.AddEquals("FIELD_NAME", "REQUIRES_CALIBRATION");
			auditEventQuery.AddOr();
			auditEventQuery.AddEquals("FIELD_NAME", "LAST_CALIB_DATE");
			auditEventQuery.AddOr();
			auditEventQuery.AddEquals("FIELD_NAME", "REQUIRES_SERVICING");
			auditEventQuery.AddOr();
			auditEventQuery.AddEquals("FIELD_NAME", "LAST_SERVICE_DATE");

			auditEventQuery.PopBracket();

			auditEventQuery.AddOrder("EVENT", true);
			auditEventQuery.AddOrder("ORDER_NUM", true);

			IEntityCollection auditValuesCollection = entityManager.Select(TableNames.AuditValues, auditEventQuery);

			// Build the history collection from the audit tables
			IEntityCollection historyCollection = entityManager.CreateEntityCollection(EntityName);

			PackedDecimal currentEvent = 0;
			InstrumentHistory iHistory = null;

			foreach (AuditValues auditValue in auditValuesCollection)
			{
				if ((auditValue.Event.Event != currentEvent) || (iHistory == null))
				{
					if (currentEvent != 0)
					{
						historyCollection.Add(iHistory);
					}

					currentEvent = auditValue.Event.Event;
					iHistory = (InstrumentHistory)entityManager.CreateEntity(EntityName);
					iHistory.m_EventDate = auditValue.AuditDate;
					iHistory.m_EventType = auditValue.AuditAction;
				}

				switch (auditValue.FieldName.Trim())
				{
					case "AVAILABLE":
						if (!BlankValue(auditValue.ValueAfter))
							iHistory.m_Available = (auditValue.ValueAfter.ToUpper().CompareTo("TRUE") == 0);
						else if (!BlankValue(auditValue.ValueBefore))
							iHistory.m_Available = (auditValue.ValueBefore.ToUpper().CompareTo("TRUE") == 0);
						break;

					case "RETIRED":
						if (!BlankValue(auditValue.ValueAfter))
							iHistory.m_Retired = (auditValue.ValueAfter.ToUpper().CompareTo("TRUE") == 0);
						else if (!BlankValue(auditValue.ValueBefore))
							iHistory.m_Retired = (auditValue.ValueBefore.ToUpper().CompareTo("TRUE") == 0);
						break;

					case "STATUS":
						if (!BlankValue(auditValue.ValueAfter))
							iHistory.m_Status = (PhraseBase)entityManager.SelectPhrase(PhraseInstStat.Identity, auditValue.ValueAfter);
						else if (!BlankValue(auditValue.ValueBefore))
							iHistory.m_Status = (PhraseBase)entityManager.SelectPhrase(PhraseInstStat.Identity, auditValue.ValueBefore);
						break;

					case "REQUIRES_CALIBRATION":
						if (!BlankValue(auditValue.ValueAfter))
							iHistory.m_RequiresCalibration = (auditValue.ValueAfter.ToUpper().CompareTo("TRUE") == 0);
						else if (!BlankValue(auditValue.ValueBefore))
							iHistory.m_RequiresCalibration = (auditValue.ValueBefore.ToUpper().CompareTo("TRUE") == 0);
						break;

					case "LAST_CALIB_DATE":
						if (!BlankValue(auditValue.ValueAfter))
							iHistory.m_CalibrationDate = auditValue.ValueAfter;
						else if (!BlankValue(auditValue.ValueBefore))
							iHistory.m_CalibrationDate = auditValue.ValueBefore;
						break;

					case "REQUIRES_SERVICING":
						if (!BlankValue(auditValue.ValueAfter))
							iHistory.m_RequiresService = (auditValue.ValueAfter.ToUpper().CompareTo("TRUE") == 0);
						else if (!BlankValue(auditValue.ValueBefore))
							iHistory.m_RequiresService = (auditValue.ValueBefore.ToUpper().CompareTo("TRUE") == 0);
						break;

					case "LAST_SERVICE_DATE":
						if (!BlankValue(auditValue.ValueAfter))
							iHistory.m_ServiceDate = auditValue.ValueAfter;
						else if (!BlankValue(auditValue.ValueBefore))
							iHistory.m_ServiceDate = auditValue.ValueBefore;
						break;

					default:
						break;
				}
			}

			if (currentEvent != 0)
			{
				historyCollection.Add(iHistory);
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
