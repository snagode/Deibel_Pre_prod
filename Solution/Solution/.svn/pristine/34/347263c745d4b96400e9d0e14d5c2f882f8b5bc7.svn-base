using System;

using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel;
using Thermo.SampleManager.Tasks.BusinessObjects;

namespace Thermo.SampleManager.Tasks.ChromeleonLinkTasks.HelperFunctions
{
	/// <summary>
	/// Sequence Helper
	/// </summary>
	public class SequenceGetResultsHelper : LogMessaging
	{
		#region Properties

		/// <summary>
		/// Gets or sets the current sequence.
		/// </summary>
		/// <value>
		/// The current sequence.
		/// </value>
		public ChromeleonSequenceEntity CurrentSequence { get; protected set; }

		#endregion

		#region Update Sequence

		/// <summary>
		/// Updates the sequence.
		/// </summary>
		/// <param name="sequence">The sequence.</param>
		public bool UpdateSequence(ChromeleonSequenceEntity sequence)
		{
			// Turn on Logging

			StartLogging();
			ClearLogging();

			// Refresh the Sequence Entry

			bool ok = RefreshSequence(sequence);

			// Stop Logging

			StopLogging();

			return ok;
		}

		/// <summary>
		/// Processes the sequence.
		/// </summary>
		/// <param name="sequence">The sequence.</param>
		/// <returns></returns>
		private bool RefreshSequence(ChromeleonSequenceEntity sequence)
		{
			// Check the parameters

			CurrentSequence = sequence;

			if (!BaseEntity.IsValid(sequence))
			{
				Logger.Error("Null or Invalid Sequence. Update skipped.");
				return false;
			}

			// Process Message

			Logger.InfoFormat("Refreshing Sequence {0}", CurrentSequence.ChromeleonSequenceName);

			// Check the Status

			if (CurrentSequence.Status.IsPhrase(PhraseChromStat.PhraseIdA) ||
				CurrentSequence.Status.IsPhrase(PhraseChromStat.PhraseIdX))
			{
				Logger.ErrorFormat("Invalid Sequence Status = {0}. Processing skipped.", CurrentSequence.Status);
				return false;
			}

			// Refresh sequence

			try
			{
				CurrentSequence.RefreshFromChromeleon();
			}
			catch (SampleManagerError error)
			{
				// Generally a connection error.

				Logger.Error(error.Message, error);
				return false;
			}
			catch (Exception ex)
			{
				Logger.Error(ex.Message, ex);
				return false;
			}

			// Done

			Logger.InfoFormat("Successfully Refreshed Sequence {0}", CurrentSequence.ChromeleonSequenceName);
			return true;
		}

		#endregion

		#region Result Retrieval (by Sequence)

		/// <summary>
		/// Retrieves the results.
		/// </summary>
		/// <param name="sequence">The sequence.</param>
		public bool RetrieveResults(ChromeleonSequenceEntity sequence)
		{
			// Turn on Logging

			StartLogging();
			ClearLogging();

			// Process the Sequence Entry

			bool ok = ProcessSequence(sequence);

			// Stop Logging

			StopLogging();

			return ok;
		}

		/// <summary>
		/// Processes the sequence.
		/// </summary>
		/// <param name="sequence">The sequence.</param>
		/// <returns></returns>
		private bool ProcessSequence(ChromeleonSequenceEntity sequence)
		{
			// Hook up memory logging

			CurrentSequence = sequence;
			CurrentSequence.Logger = Logger;

			if (CurrentSequence.ChromeleonMapping != null)
			{
				CurrentSequence.ChromeleonMapping.Logger = Logger;
			}

			CurrentSequence.InjectionResultsProcessed -= SequenceInjectionResultsProcessed;
			CurrentSequence.InjectionResultsProcessed += SequenceInjectionResultsProcessed;

			// Check the parameters

			if (!BaseEntity.IsValid(sequence))
			{
				Logger.Error("Null or Invalid Sequence. Processing skipped.");
				return false;
			}

			// Force a Refresh

			RefreshSequence(sequence);

			// Process Message

			Logger.InfoFormat("Processing Sequence {0}", CurrentSequence.ChromeleonSequenceName);

			// Check the Status

			if (CurrentSequence.Status.IsPhrase(PhraseChromStat.PhraseIdA) ||
				CurrentSequence.Status.IsPhrase(PhraseChromStat.PhraseIdX))
			{
				Logger.ErrorFormat("Invalid Sequence Status = {0}. Processing skipped.", CurrentSequence.Status);
				return false;
			}

			// Retrieve Results

			try
			{
				CurrentSequence.RetrieveResults();
			}
			catch (SampleManagerError error)
			{
				// Generally a connection error.

				Logger.Error(error.Message, error);
				return false;
			}
			catch (Exception ex)
			{
				Logger.Error(ex.Message, ex);
				return false;
			}

			// Done

			Logger.InfoFormat("Finished Processing Sequence {0}", CurrentSequence.ChromeleonSequenceName);
			return true;
		}

		#endregion

		#region Events 

		/// <summary>
		/// Sequence injection results retrieved.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void SequenceInjectionResultsProcessed(object sender, EntryEventArgs e)
		{
			Logger.DebugFormat("Processed Injection {0}", e.Entry.ChromeleonSequenceEntryName);
			OnInjectionResultsProcessed(e.Entry);
		}

		/// <summary>
		/// Occurs when injection results retrieved.
		/// </summary>
		public event EventHandler<EntryEventArgs> InjectionResultsProcessed;

		/// <summary>
		/// Called when injection results retrieved.
		/// </summary>
		protected void OnInjectionResultsProcessed(ChromeleonSequenceEntryEntity entry)
		{
			if (InjectionResultsProcessed != null)
			{
				var eventArgs = new EntryEventArgs(entry);

				StopLogging();
				InjectionResultsProcessed(this, eventArgs);
				StartLogging();
			}
		}

		#endregion
	}
}
