using System;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the INSPECTION_RECORD entity.
	/// </summary>
	[SampleManagerEntity(InspectionRecordBase.EntityName)]
	public class InspectionRecord : InspectionRecordBase
	{
		#region Public Constants

		/// <summary>
		/// The Phrase ID that contains the Inspection Record set
		/// </summary>
		public const string InspectionRecordTypePhrase = "INSP_USER";

		/// <summary>
		/// The Phrase associated with an instrument part that is Out of Calibration
		/// </summary>
		public const string InspectionRecordTypePhraseUser = "USER";
		/// <summary>
		/// The Phrase associated with an instrument part that is Out of Service
		/// </summary>
		public const string InspectionRecordTypePhraseRole = "ROLE";

		#endregion

		#region Member Variables

		string m_identityColumn = "";
		string m_userType = "";
		string m_personnel = "";
		string m_role = "";

		#endregion

		#region Properties

		/// <summary>
		/// Property : IdentityColumn
		/// </summary>
		/// <value></value>
		[PromptText(30)]
		public String IdentityColumn
		{
			get
			{
				m_userType = InspectionUserType.PhraseId;
				m_personnel = PersonnelId.Identity;
				m_role = RoleId.Identity;

				switch (m_userType)
				{
					case InspectionRecordTypePhraseUser:
						m_identityColumn = InspectionUserType.PhraseText + ": " + m_personnel;
						break;
					case InspectionRecordTypePhraseRole:
						m_identityColumn = InspectionUserType.PhraseText + ": " + m_role;
						break;
					default:
						break;
				}

				return m_identityColumn;
			}
		}

		#endregion
	}
}
