using System;
using Thermo.SampleManager.Common.CommandLine;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.Tasks.BackgroundTasks
{
	/// <summary>
	/// Background task to update inactive users flags
	/// </summary>
	[SampleManagerTask("InactiveUsers")]
	public class InactiveUsersTask : SampleManagerTask, IBackgroundTask
	{
		#region Properties

		/// <summary>
		/// Number of days a user can be inactive
		/// </summary>
		/// <value>The criteria.</value>
		[CommandLineSwitch("days", "Number of days of inactivity before the user account is frozen.", false)]
		public int Days { get; set; }

		#endregion

		#region IBackgroundTask Members

		/// <summary>
		/// Launches this background task.
		/// </summary>
		public void Launch()
		{
			Logger.Info("Starting Inactive Users Task...");

			if (Days == 0)
				Days = 30;

			ProcessUsers(Days);

			Logger.Info("Finished Inactive Users Task.");

		}

		/// <summary>
		/// Freeze accounts for inactive users.
		/// </summary>
		/// <param name="days"></param>
		void ProcessUsers(int days)
		{
			IEntityCollection inactiveUsers = ShouldBeInactive(days);

			foreach (PasswordBase user in inactiveUsers)
			{
				Logger.InfoFormat("Freezing user account {0}, Last Login Date = {1}", user.Identity, user.LastLogin);
				user.Frozen = true;
				EntityManager.Transaction.Add(user);
			}

			EntityManager.Commit();
		}

		/// <summary>
		/// Get users which have not been logged in for n days.
		/// </summary>
		/// <param name="days"></param>
		/// <returns></returns>
		private IEntityCollection ShouldBeInactive(int days)
		{
			DateTime inactiveDate = DateTime.Now.AddDays(-days);
			IQuery query = EntityManager.CreateQuery(PasswordBase.EntityName);

			query.AddLessThan(PasswordPropertyNames.LastLogin, inactiveDate);
			query.AddAnd();
			query.AddEquals(PasswordPropertyNames.Frozen, false);
			query.AddAnd();
			query.AddNotEquals(PasswordPropertyNames.Identity, "SYSTEM");
			query.AddAnd();
			query.AddNotEquals(PasswordPropertyNames.Identity, "BACKGROUND");
			query.AddAnd();
			query.AddNotEquals(PasswordPropertyNames.Identity, "BATCH");

			return EntityManager.Select(PasswordBase.EntityName, query);
		}

		#endregion
	}
}