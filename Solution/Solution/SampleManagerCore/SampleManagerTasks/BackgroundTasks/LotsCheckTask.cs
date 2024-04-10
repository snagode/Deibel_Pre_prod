using System;
using System.Globalization;
using Thermo.SampleManager.Common.CommandLine;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks.BackgroundTasks
{
	/// <summary>
	/// Background task which updates the state of lots
	/// </summary>
	[SampleManagerTask("LotsCheck")]
	public class LotsCheckTask : SampleManagerTask, IBackgroundTask
	{
		#region IBackgroundTask Members

		/// <summary>
		/// Launches this background task.
		/// </summary>
		public void Launch()
		{
			Logger.Debug("Starting Lots Check Task...");

			// Select all Lots

			IQuery lotsQuery = EntityManager.CreateQuery(TableNames.LotDetails);
			lotsQuery.AddEquals("STATUS", PhraseLotStat.PhraseIdV);
			lotsQuery.AddOrder("LOT_ID", true);

			IEntityCollection lotCollection = EntityManager.Select(TableNames.LotDetails, lotsQuery);

			foreach (LotDetails lot in lotCollection)
			{
				try
				{
					if (!lot.ExpiryDate.IsNull)
					{
						DateTime expiry = lot.ExpiryDate.ToDateTime(CultureInfo.CurrentCulture);

						if (expiry <= (DateTime) Library.Environment.ClientNow)
						{
							Logger.DebugFormat("Expired Trigger run for lot '{0}'", lot);
							// Fire Event to action expiry within workflow
							lot.TriggerExpired();
						}
					}

					if (lot.IsModified())
					{
						EntityManager.Transaction.Add(lot);
						EntityManager.Commit();
					}
				}
				catch (Exception exception)
				{
					Logger.Fatal(string.Format("Exception on lot '{0}'", lot), exception);
				}
			}

			Logger.Debug("Finished Lots Check Task...");
		}

		#endregion
	}
}