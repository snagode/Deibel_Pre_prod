using System.Globalization;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel;

namespace Thermo.SampleManager.Tasks.ChromeleonLinkTasks
{
	/// <summary>
	/// Chromeleon Refresh Sequence Task
	/// </summary>
	[SampleManagerTask("ChromeleonRefreshSequenceTask")]
	public class ChromeleonRefreshSequenceTask : SampleManagerTask
	{
		#region Member Variables

		private IEntityCollection m_Sequences;

		#endregion

		#region Setup

		/// <summary>
		/// Setups the task.
		/// </summary>
		protected override void SetupTask()
		{
			base.SetupTask();
			LoadSequences();
			RefreshSequences();

			if (m_Sequences.Count > 0)
			{
				string message = Library.Message.GetMessage("ChromeleonLinkMessages", "RefreshedSequenceMessage", m_Sequences.Count.ToString(CultureInfo.InvariantCulture));
				string caption = Library.Message.GetMessage("ChromeleonLinkMessages", "RefreshedSequenceTitle");

				Library.Utils.FlashMessage(message, caption);
			}

			Exit();
		}

		#endregion

		#region Data Processing

		/// <summary>
		/// Loads the sample data.
		/// </summary>
		private void LoadSequences()
		{
			m_Sequences = EntityManager.CreateEntityCollection(ChromeleonSequenceBase.EntityName);

			if (Context.SelectedItems != null)
			{
				foreach (ChromeleonSequenceEntity sequence in Context.SelectedItems)
				{
					m_Sequences.Add(sequence);
				}
			}

			// Prompt for one if none selected.

			if (m_Sequences.Count == 0)
			{
				IEntity seq;

				string caption = Library.Message.GetMessage("ChromeleonLinkMessages", "RefreshSequenceTitle");
				string message = Library.Message.GetMessage("ChromeleonLinkMessages", "RefreshSequenceMessage");

				IQuery query = EntityManager.CreateQuery(ChromeleonSequenceBase.EntityName);
				query.AddNotEquals(ChromeleonSequencePropertyNames.Status, PhraseChromStat.PhraseIdC);
				query.AddNotEquals(ChromeleonSequencePropertyNames.Status, PhraseChromStat.PhraseIdA);

				if (Library.Utils.PromptForEntity(message, caption, query, out seq) == FormResult.OK)
				{
					m_Sequences.Add(seq);
				}
			}
		}

		/// <summary>
		/// Gets the data for browses
		/// </summary>
		private void RefreshSequences()
		{
			foreach (ChromeleonSequenceEntity sequence in m_Sequences)
			{
				sequence.RefreshFromChromeleon();

				// Update as we go, there may be a lot of data changes.
				// Each selected sequence done in a separate transaction.

				string message = Library.Message.GetMessage("ChromeleonLinkMessages", "UpdatedSequenceMessage", sequence.ChromeleonSequenceName);
				string caption = Library.Message.GetMessage("ChromeleonLinkMessages", "UpdatedSequenceTitle");

				Library.Utils.ShowAlert(caption, "CHROMELEON_SEQUENCE", message);

				EntityManager.Transaction.Add(sequence);
				EntityManager.Commit();
			}
		}

		#endregion
	}
}