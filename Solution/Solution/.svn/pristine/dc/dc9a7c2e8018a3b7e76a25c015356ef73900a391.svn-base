using Thermo.Framework.Core;
using Thermo.SampleManager.Common.CommandLine;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel;
using Thermo.SampleManager.Tasks.ChromeleonLinkTasks.HelperFunctions;

namespace Thermo.SampleManager.Tasks.ChromeleonLinkTasks
{
	/// <summary>
	/// Poll for results to be entered from Chromeleon.
	/// </summary>
	[SampleManagerTask("ChromeleonResultsEntryPoll")]
	public class ChromeleonResultsEntryPollTask : SampleManagerTask, IBackgroundTask
	{
		#region IBackgroundTask Members

		/// <summary>
		/// This task runs and looks for entries in Chromeleon to be entered into SampleManager.
		/// </summary>
		public void Launch()
		{
			// Get list of sequences for to be checked for available results.

			Logger.Info("Selecting Status V/P Sequences read for result retrieval.");

			var sequences = GetSubmittedSequencesSortByServer();

			Logger.InfoFormat("{0} sequences returned", sequences.Count);
			
			// Spin through retrieving results for each

			foreach (ChromeleonSequenceEntity sequence in sequences)
			{
				SequenceGetResultsHelper helper = new SequenceGetResultsHelper {LogMessageLevel = LoggerLevel.Off};

				bool ok = helper.RetrieveResults(sequence);

				// Save immediately if processed.

				if (ok)
				{
					Logger.InfoFormat("Saving Changes to Sequence {0}", sequence.ChromeleonSequenceName);

					EntityManager.Transaction.Add(sequence);
					EntityManager.Commit();
				}
				else
				{
					Logger.InfoFormat("Error Processing Sequence {0}", sequence.ChromeleonSequenceName);
				}
			}
		}

		#endregion

		#region Sequence Selection

		/// <summary>
		/// Return a list of submitted sequences, sorted by server.
		/// </summary>
		/// <returns>Collection of sequences submitted to Chromeleon but not marked complete.</returns>
		public IEntityCollection GetSubmittedSequencesSortByServer()
		{
			IQuery qry = EntityManager.CreateQuery(ChromeleonSequenceBase.EntityName);
			qry.AddEquals(ChromeleonSequencePropertyNames.Removeflag, false);

			// Get sequences that have been submitted but not completed.

			qry.AddEquals(ChromeleonSequencePropertyNames.Status, PhraseChromStat.PhraseIdV);
			qry.AddOr();
			qry.AddEquals(ChromeleonSequencePropertyNames.Status, PhraseChromStat.PhraseIdP);

			// Sort by Chromeleon server instance so that a server timing out can be skipped.

			qry.AddOrder(ChromeleonSequencePropertyNames.ChromeleonId, true);

			// Select the entities and return.

			var sequences = EntityManager.Select(ChromeleonSequenceBase.EntityName, qry);
			return sequences;
		}

		#endregion

	}
}
