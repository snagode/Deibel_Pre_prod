using System;
using System.Collections.Generic;
using System.Text;
using Thermo.SampleManager.Common.CommandLine;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks.BackgroundTasks
{
	/// <summary>
	/// Background task which updates the state of instruments and instrument parts
	/// </summary>
	[SampleManagerTask("InstrumentCheck")]
	public class InstrumentCheckTask : SampleManagerTask, IBackgroundTask
	{
		#region Member Variables

		private readonly List<Instrument> m_CalibList = new List<Instrument>();
		private readonly List<Instrument> m_InCalibList = new List<Instrument>();
		private readonly List<InstrumentPart> m_PartCalibList = new List<InstrumentPart>();
		private readonly List<InstrumentPart> m_PartServiceList = new List<InstrumentPart>();
		private readonly List<Instrument> m_ServiceList = new List<Instrument>();

		#endregion

		#region IBackgroundTask Members

		/// <summary>
		/// Launches this background task.
		/// </summary>
		public void Launch()
		{
			Logger.Debug("Starting Instrument Check Task...");

			// Select all Instruments

			IEntityCollection instrumentCollection = EntityManager.Select(TableNames.Instrument);

			foreach (Instrument inst in instrumentCollection)
				CheckMaintenance(inst);

			EntityManager.Commit();

			// Select all Instruments parts

			IEntityCollection instrumentPartCollection = EntityManager.Select(TableNames.InstrumentPart);

			foreach (InstrumentPart instPart in instrumentPartCollection)
				CheckPartMaintenance(instPart);

			EntityManager.Commit();

			// Send an email to the resposible operator

			MailResponsibleOperators();
		}

		#endregion

		#region Check Maintenance

		/// <summary>
		/// Checks the maintenance.
		/// </summary>
		/// <param name="inst">The inst.</param>
		private void CheckMaintenance(Instrument inst)
		{
			if (MailableUser(inst.OperatorId))
			{
				if ((!inst.Retired) && (inst.Available))
				{
					if (inst.InCalibration)
						m_InCalibList.Add(inst);
					else if (inst.OutOfService)
						m_ServiceList.Add(inst);
					else if (inst.OutOfCalibration)
						m_CalibList.Add(inst);
				}
			}

			inst.SetInstrumentStatus();

			if (inst.IsModified())
				EntityManager.Transaction.Add(inst);
		}

		/// <summary>
		/// Checks the part maintenance.
		/// </summary>
		/// <param name="instPart">The inst part.</param>
		private void CheckPartMaintenance(InstrumentPart instPart)
		{
			if (MailableUser(instPart.OperatorId))
			{
				if ((!instPart.Retired) && (instPart.Available))
				{
					if (instPart.OutOfService)
						m_PartServiceList.Add(instPart);
					else if (instPart.OutOfCalibration)
						m_PartCalibList.Add(instPart);
				}
			}

			instPart.SetInstrumentPartStatus();

			if (instPart.IsModified())
				EntityManager.Transaction.Add(instPart);
		}

		#endregion

		#region Mailing

		/// <summary>
		/// Mailable user.
		/// </summary>
		/// <param name="oper">The oper.</param>
		/// <returns></returns>
		private static bool MailableUser(PersonnelBase oper)
		{
			if (oper.IsNull()) return false;
			Personnel person = (Personnel) oper;
			return person.IsMailable;
		}

		/// <summary>
		/// Mails the responsible operators.
		/// </summary>
		private void MailResponsibleOperators()
		{
			while (m_InCalibList.Count > 0)
				MailToUser((Personnel)m_InCalibList[0].OperatorId);

			while (m_ServiceList.Count > 0)
				MailToUser((Personnel)m_ServiceList[0].OperatorId);

			while (m_CalibList.Count > 0)
				MailToUser((Personnel)m_CalibList[0].OperatorId);
		}

		/// <summary>
		/// Mails to user.
		/// </summary>
		/// <param name="oper">The oper.</param>
		private void MailToUser(Personnel oper)
		{
			StringBuilder mailBody = new StringBuilder();

			mailBody.Append(Library.Message.GetMessage("LaboratoryMessages", "InstrumentCheckIntro"));

			AddInstInCalib(oper, mailBody);
			AddInstCalib(oper, mailBody);
			AddInstService(oper, mailBody);
			AddInstPartCalib(oper, mailBody);
			AddInstPartService(oper, mailBody);

			try
			{
				string subject = Library.Message.GetMessage("LaboratoryMessages", "InstrumentCheckSubject");
				oper.Mail(subject, mailBody.ToString());
				Logger.DebugFormat("Mail sent to {0}", oper.Email);
			}
			catch (Exception e)
			{
				Logger.DebugFormat("Error Sending mail {0} - {1}", e.Message, e.InnerException.Message);
			}
		}

		/// <summary>
		/// Adds the inst part service.
		/// </summary>
		/// <param name="oper">The oper.</param>
		/// <param name="mailBody">The mail body.</param>
		private void AddInstPartService(PersonnelBase oper, StringBuilder mailBody)
		{
			bool first = true;
			int idLength = 0;
			List<InstrumentPart> tempInstParts = new List<InstrumentPart>();

			for (int i = m_PartServiceList.Count - 1; i >= 0; i--)
			{
				if (m_PartServiceList[i].OperatorId == oper)
				{
					idLength = m_PartServiceList[i].Identity.Length > idLength ? m_PartServiceList[i].Identity.Length : idLength;
					tempInstParts.Insert(0, m_PartServiceList[i]);
					m_PartServiceList.RemoveAt(i);
				}
			}

			foreach (InstrumentPart instPart in tempInstParts)
			{
				// Header if required
				if (first)
				{
					mailBody.Append(Library.Message.GetMessage("LaboratoryMessages", "InstrumentCheckPartsService"));
					first = false;
				}

				// Each instrument
				mailBody.Append("\t");
				mailBody.Append(instPart.Identity.PadRight(idLength));
				mailBody.Append("\t");
				mailBody.Append(instPart.Description);
				mailBody.Append("\n");
			}
		}

		/// <summary>
		/// Adds the inst part calib.
		/// </summary>
		/// <param name="oper">The oper.</param>
		/// <param name="mailBody">The mail body.</param>
		private void AddInstPartCalib(PersonnelBase oper, StringBuilder mailBody)
		{
			bool first = true;
			int idLength = 0;
			List<InstrumentPart> tempInstParts = new List<InstrumentPart>();

			for (int i = m_PartCalibList.Count - 1; i >= 0; i--)
			{
				if (m_PartCalibList[i].OperatorId == oper)
				{
					idLength = m_PartCalibList[i].Identity.Length > idLength ? m_PartCalibList[i].Identity.Length : idLength;
					tempInstParts.Insert(0, m_PartCalibList[i]);
					m_PartCalibList.RemoveAt(i);
				}
			}

			foreach (InstrumentPart instPart in tempInstParts)
			{
				// Header if required
				if (first)
				{
					mailBody.Append(Library.Message.GetMessage("LaboratoryMessages", "InstrumentCheckPartsCalib"));
					first = false;
				}

				// Each instrument
				mailBody.Append("\t");
				mailBody.Append(instPart.Identity.PadRight(idLength));
				mailBody.Append("\t");
				mailBody.Append(instPart.Description);
				mailBody.Append("\n");
			}
		}

		/// <summary>
		/// Adds the inst service.
		/// </summary>
		/// <param name="oper">The oper.</param>
		/// <param name="mailBody">The mail body.</param>
		private void AddInstService(PersonnelBase oper, StringBuilder mailBody)
		{
			bool first = true;
			int idLength = 0;
			List<Instrument> tempInsts = new List<Instrument>();

			for (int i = m_ServiceList.Count - 1; i >= 0; i--)
			{
				if (m_ServiceList[i].OperatorId == oper)
				{
					idLength = m_ServiceList[i].Identity.Length > idLength ? m_ServiceList[i].Identity.Length : idLength;
					tempInsts.Insert(0, m_ServiceList[i]);
					m_ServiceList.RemoveAt(i);
				}
			}

			foreach (Instrument inst in tempInsts)
			{
				// Header if required
				if (first)
				{
					mailBody.Append(Library.Message.GetMessage("LaboratoryMessages", "InstrumentCheckService"));
					first = false;
				}

				// Each instrument
				mailBody.Append("\t");
				mailBody.Append(inst.Identity.PadRight(idLength));
				mailBody.Append("\t");
				mailBody.Append(inst.Description);
				mailBody.Append("\n");
			}
		}

		/// <summary>
		/// Adds the inst calib.
		/// </summary>
		/// <param name="oper">The oper.</param>
		/// <param name="mailBody">The mail body.</param>
		private void AddInstCalib(PersonnelBase oper, StringBuilder mailBody)
		{
			bool first = true;
			int idLength = 0;
			List<Instrument> tempInsts = new List<Instrument>();

			for (int i = m_CalibList.Count - 1; i >= 0; i--)
			{
				if (m_CalibList[i].OperatorId == oper)
				{
					idLength = m_CalibList[i].Identity.Length > idLength ? m_CalibList[i].Identity.Length : idLength;
					tempInsts.Insert(0, m_CalibList[i]);
					m_CalibList.RemoveAt(i);
				}
			}

			foreach (Instrument inst in tempInsts)
			{
				// Header if required
				if (first)
				{
					mailBody.Append(Library.Message.GetMessage("LaboratoryMessages", "InstrumentCheckCalib"));
					first = false;
				}

				// Each instrument
				mailBody.Append("\t");
				mailBody.Append(inst.Identity.PadRight(idLength));
				mailBody.Append("\t");
				mailBody.Append(inst.Description);
				mailBody.Append("\n");
			}
		}

		/// <summary>
		/// Adds the inst in calib.
		/// </summary>
		/// <param name="oper">The oper.</param>
		/// <param name="mailBody">The mail body.</param>
		private void AddInstInCalib(PersonnelBase oper, StringBuilder mailBody)
		{
			bool first = true;
			int idLength = 0;
			List<Instrument> tempInsts = new List<Instrument>();

			for (int i = m_InCalibList.Count - 1; i >= 0; i--)
			{
				if (m_InCalibList[i].OperatorId == oper)
				{
					idLength = m_InCalibList[i].Identity.Length > idLength ? m_InCalibList[i].Identity.Length : idLength;
					tempInsts.Insert(0, m_InCalibList[i]);
					m_InCalibList.RemoveAt(i);
				}
			}

			foreach (Instrument inst in tempInsts)
			{
				// Header if required
				if (first)
				{
					mailBody.Append(Library.Message.GetMessage("LaboratoryMessages", "InstrumentCheckCalibComplete"));
					first = false;
				}

				// Each instrument
				mailBody.Append("\t");
				mailBody.Append(inst.Identity.PadRight(idLength));
				mailBody.Append("\t");
				mailBody.Append(inst.Description);
				mailBody.Append("\n");
			}
		}

		#endregion
	}
}