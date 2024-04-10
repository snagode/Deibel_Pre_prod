using System;
using System.Collections.Generic;
using System.Text;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.CommandLine;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks.BackgroundTasks
{
	/// <summary>
	/// Background task which updates the state of Training Records
	/// </summary>
	[SampleManagerTask("TrainingRecordCheck")]
	public class TrainingRecordCheckTask : SampleManagerTask, IBackgroundTask
	{
		#region Member Variables 

		private readonly IDictionary<string, string> m_MailMessages = new Dictionary<string, string>();
		private readonly IDictionary<string, PersonnelBase> m_OperatorsToMail = new Dictionary<string, PersonnelBase>();

		#endregion

		#region IBackgroundTask Members

		/// <summary>
		/// Launches this background task.
		/// </summary>
		public void Launch()
		{
			Logger.Debug("Starting TrainingRecord Check Task...");

			// Select all Training Courses
			IEntityCollection trainingCourseCollection = EntityManager.Select(TableNames.TrainingCourse);

			foreach (TrainingCourse trainingCourse in trainingCourseCollection)
			{
				if (MailableUser(trainingCourse.OperatorId))
				{
					StringBuilder mailMessage = new StringBuilder();

					if (!m_OperatorsToMail.ContainsKey(trainingCourse.OperatorId.Identity))
						m_OperatorsToMail.Add(trainingCourse.OperatorId.Identity, trainingCourse.OperatorId);

					if (!m_MailMessages.ContainsKey(trainingCourse.OperatorId.Identity))
						m_MailMessages.Add(trainingCourse.OperatorId.Identity, "");
					else
						mailMessage.Append(m_MailMessages[trainingCourse.OperatorId.Identity]);

					CheckTrainedOperators(trainingCourse, ref mailMessage);

					m_MailMessages[trainingCourse.OperatorId.Identity] = mailMessage.ToString();
				}
			}

			MailResponsibleOperators();
		}

		#endregion

		#region Changed Operators

		/// <summary>
		/// Checks the trained operators.
		/// </summary>
		/// <param name="trainingCourse">The training course.</param>
		/// <param name="mailMessage">The mail message.</param>
		private void CheckTrainedOperators(TrainingCourseBase trainingCourse, ref StringBuilder mailMessage)
		{
			int idLength = 0;

			List<PersonnelTraining> retestNow = new List<PersonnelTraining>();
			List<PersonnelTraining> retestSoon = new List<PersonnelTraining>();
			List<PersonnelTraining> notCompleted = new List<PersonnelTraining>();

			IQuery persTrainingQuery = EntityManager.CreateQuery(TableNames.PersonnelTraining);

			persTrainingQuery.AddEquals("TRAINING_COURSE", trainingCourse);
			persTrainingQuery.AddOrder("PERSONNEL", true);

			IEntityCollection persTrainings = EntityManager.Select(TableNames.PersonnelTraining, persTrainingQuery);

			foreach (PersonnelTraining persTraining in persTrainings)
			{
				if (persTraining.DateCompleted.IsNull)
				{
					idLength = persTraining.ParentPersonnel.Identity.Length > idLength
					           	? persTraining.ParentPersonnel.Identity.Length
					           	: idLength;
					notCompleted.Add(persTraining);
				}
				else if (trainingCourse.RetestInterval.TotalMilliseconds != 0)
				{
					NullableDateTime warningDate = persTraining.RetestDate.Value.Subtract(trainingCourse.RetestWarningPeriodInterval);

					if (persTraining.RetestDate <= Library.Environment.ClientNow)
					{
						idLength = persTraining.ParentPersonnel.Identity.Length > idLength
						           	? persTraining.ParentPersonnel.Identity.Length
						           	: idLength;
						retestNow.Add(persTraining);
					}
					else if (warningDate <= Library.Environment.ClientNow)
					{
						idLength = persTraining.ParentPersonnel.Identity.Length > idLength
						           	? persTraining.ParentPersonnel.Identity.Length
						           	: idLength;
						retestSoon.Add(persTraining);
					}
				}
			}

			if (idLength > 0)
			{

				mailMessage.AppendFormat(Library.Message.GetMessage("LaboratoryMessages", "TrainingCheckRequirements",
				                         trainingCourse.Identity,
				                         trainingCourse.Description));

				if (notCompleted.Count > 0)
				{

					mailMessage.Append(Library.Message.GetMessage("LaboratoryMessages", "TrainingCheckTrain"));

					foreach (PersonnelTraining personnelTraining in notCompleted)
					{
						mailMessage.AppendFormat("\t\t{0}, {1}\n",
						                         personnelTraining.ParentPersonnel.Identity,
						                         personnelTraining.ParentPersonnel.Description);
					}
				}

				if (retestNow.Count > 0)
				{
					mailMessage.Append(Library.Message.GetMessage("LaboratoryMessages", "TrainingCheckRetrainPast"));

					foreach (PersonnelTraining personnelTraining in retestNow)
					{
						mailMessage.AppendFormat("\t\t{0}, {1}\n",
						                         personnelTraining.ParentPersonnel.Identity,
						                         personnelTraining.ParentPersonnel.Description);
					}
				}

				if (retestSoon.Count > 0)
				{
					mailMessage.Append(Library.Message.GetMessage("LaboratoryMessages", "TrainingCheckRetrain"));

					foreach (PersonnelTraining personnelTraining in retestSoon)
					{
						mailMessage.AppendFormat("\t\t{0}, {1}\n",
						                         personnelTraining.ParentPersonnel.Identity,
						                         personnelTraining.ParentPersonnel.Description);
					}
				}
			}
		}

		#endregion

		#region Mail

		/// <summary>
		/// Mailable user.
		/// </summary>
		/// <param name="oper">The oper.</param>
		/// <returns></returns>
		private static bool MailableUser(PersonnelBase oper)
		{
			if (oper.IsNull()) return false;
			Personnel person = (Personnel)oper;
			return person.IsMailable;
		}

		/// <summary>
		/// Mails the responsible operators.
		/// </summary>
		private void MailResponsibleOperators()
		{
			foreach (KeyValuePair<string, PersonnelBase> operToMail in m_OperatorsToMail)
			{
				if (m_MailMessages[operToMail.Key].Length > 0)
					MailToUser((Personnel)operToMail.Value, m_MailMessages[operToMail.Key]);
			}
		}

		/// <summary>
		/// Mails to user.
		/// </summary>
		/// <param name="oper">The oper.</param>
		/// <param name="mailMessage">The mail message.</param>
		private void MailToUser(Personnel oper, string mailMessage)
		{
			StringBuilder mailBody = new StringBuilder();

			mailBody.Append(Library.Message.GetMessage("LaboratoryMessages", "TrainingCheckIntro"));
			mailBody.Append(mailMessage);

			try
			{
				string subject = Library.Message.GetMessage("LaboratoryMessages", "TrainingCheckSubject");
				oper.Mail(subject, mailBody.ToString());
				Logger.DebugFormat("Mail sent to {0}", oper.Email);
			}
			catch (Exception e)
			{
				Logger.DebugFormat("Error Sending mail {0} - {1}", e.Message, e.InnerException.Message);
			}
		}

		#endregion
	}
}