using System;
using System.Text;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Manages the approved and qualified personnel for an entity
	/// </summary>
	public sealed class TrainingApproval
	{
		#region Static for Trained Operators

		/// <summary>
		/// Collection of operators that have the correct training for the parent entity
		/// </summary>
		/// <param name="parentEntity"></param>
		/// <param name="clientNow"></param>
		/// <returns></returns>
		public static IEntityCollection TrainedOperators(IEntity parentEntity, NullableDateTime clientNow)
		{
			string selectOracle = null;
			string selectSqlServer = null;

			// Get the appropriate dynamic SQL command

			if (parentEntity is Preparation)
				BuildSelect(clientNow, ((Preparation) parentEntity).PreparationTrainings, out selectOracle, out selectSqlServer);
			else if (parentEntity is Instrument)
				BuildSelect(clientNow, ((Instrument)parentEntity).InstrumentTrainings, out selectOracle, out selectSqlServer);
			else if (parentEntity is VersionedAnalysis)
				BuildSelect(clientNow, ((VersionedAnalysis)parentEntity).AnalysisTrainings, out selectOracle, out selectSqlServer);

			// Execute the command if we have one, otherwise return an empty collection

			if (selectOracle != null || selectSqlServer != null)
				return parentEntity.EntityManager.SelectDynamic(TableNames.Personnel, selectOracle, selectSqlServer);

			return parentEntity.EntityManager.CreateEntityCollection(TableNames.Personnel);
		}

		#endregion

		#region Utility Methods

		/// <summary>
		/// Builds the select.
		/// </summary>
		/// <param name="clientNow">The current client time.</param>
		/// <param name="entityTrainings">The entity trainings.</param>
		/// <param name="selectOracle">The select oracle.</param>
		/// <param name="selectSqlServer">The select SQL server.</param>
		private static void BuildSelect(NullableDateTime clientNow, 
		                                IEntityCollection entityTrainings,
		                                out string selectOracle,
		                                out string selectSqlServer)
		{
			StringBuilder conditionsOracle = new StringBuilder();
			StringBuilder conditionsSqlServer = new StringBuilder();

			selectOracle = null;
			selectSqlServer = null;

			if (entityTrainings.ActiveCount > 0)
			{
				conditionsOracle.Append(" WHERE ");
				conditionsSqlServer.Append(" WHERE ");

				foreach (IEntity course in entityTrainings.ActiveItems)
				{
					TrainingCourse trainingCourse = null;
					string courseIdentity = string.Empty;
					string competencyIdentity = string.Empty;

					if (course is PreparationTraining)
					{
						trainingCourse = (TrainingCourse)(((PreparationTraining) course).TrainingCourse);
						courseIdentity = ((PreparationTraining) course).TrainingCourse.Identity;
						competencyIdentity = ((PreparationTraining) course).MinimumCompetence.PhraseId;
					}
					else if (course is InstrumentTraining)
					{
						trainingCourse = (TrainingCourse)(((InstrumentTraining) course).TrainingCourse);
						courseIdentity = ((InstrumentTraining) course).TrainingCourse.Identity;
						competencyIdentity = ((InstrumentTraining) course).MinimumCompetence.PhraseId;
					}
					else if (course is VersionedAnalysisTraining)
					{
						trainingCourse = (TrainingCourse)(((VersionedAnalysisTraining) course).TrainingCourse);
						courseIdentity = ((VersionedAnalysisTraining) course).TrainingCourse.Identity;
						competencyIdentity = ((VersionedAnalysisTraining) course).MinimumCompetence.PhraseId;
					}

					if ((trainingCourse == null) || string.IsNullOrEmpty(courseIdentity) || string.IsNullOrEmpty(competencyIdentity))
						return;

					if (trainingCourse.RetestInterval.TotalSeconds == 0)
					{
						conditionsOracle.AppendFormat("{0}((TRAINING_COURSE = '{1}') AND (COMPETENCE >= {2})){3}",
													  entityTrainings.IsFirst(course) ? "(" : "",
													  courseIdentity,
													  competencyIdentity,
													  entityTrainings.IsLast(course) ? ") " : "OR");

						conditionsSqlServer.AppendFormat("{0}((TRAINING_COURSE = '{1}') AND (COMPETENCE >= {2})){3}",
														 entityTrainings.IsFirst(course) ? "(" : "",
														 courseIdentity,
														 competencyIdentity,
														 entityTrainings.IsLast(course) ? ") " : "OR");
					}
					else
					{
						DateTime earliestTrainedDate = clientNow.Value - trainingCourse.RetestInterval - trainingCourse.RetestGracePeriodInterval;

						conditionsOracle.AppendFormat("{0}((TRAINING_COURSE = '{1}') AND (COMPETENCE >= {2}) AND (DATE_COMPLETED > TO_DATE('{3}', 'DDMMYYYYHH24MISS'))){4}",
													  entityTrainings.IsFirst(course) ? "(" : "",
													  courseIdentity,
													  competencyIdentity,
													  earliestTrainedDate.ToString("ddMMyyyyHHmmss"),
													  entityTrainings.IsLast(course) ? ") " : "OR");

						conditionsSqlServer.AppendFormat("{0}((TRAINING_COURSE = '{1}') AND (COMPETENCE >= {2}) AND (DATE_COMPLETED > convert(datetime,'{3}',126))){4}",
														 entityTrainings.IsFirst(course) ? "(" : "",
														 courseIdentity,
														 competencyIdentity,
														 earliestTrainedDate.ToString("s"),
														 entityTrainings.IsLast(course) ? ") " : "OR");
					}
				}

				conditionsOracle.AppendFormat("GROUP BY pt.PERSONNEL HAVING (COUNT(*) >= {0})))",
				                              entityTrainings.ActiveCount);

				conditionsSqlServer.AppendFormat("GROUP BY pt.PERSONNEL HAVING (COUNT(*) >= {0})))",
				                                 entityTrainings.ActiveCount);

				selectOracle =
					string.Format(
						"SELECT * FROM PERSONNEL WHERE ([IDENTITY] IN (SELECT DISTINCT PERSONNEL FROM PERSONNEL_TRAINING pt {0}",
						conditionsOracle.Length > 0 ? conditionsOracle.ToString() : "))");

				selectSqlServer =
					string.Format(
						"SELECT * FROM PERSONNEL WHERE ([IDENTITY] IN (SELECT DISTINCT PERSONNEL FROM PERSONNEL_TRAINING pt {0}",
						conditionsSqlServer.Length > 0 ? conditionsSqlServer.ToString() : "))");
			}
		}

		#endregion
	}
}