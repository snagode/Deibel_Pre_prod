using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel;
using Thermo.SampleManager.Tasks.ChromeleonLinkTasks.HelperFunctions;

namespace Thermo.SampleManager.Tasks.ChromeleonLinkTasks
{
	/// <summary>
	/// Chromeleon Sequence Get Results Task
	/// </summary>
	[SampleManagerTask("ChromeleonSequenceGetResultsTask")]
	public class ChromeleonSequenceGetResultsTask : DefaultSingleEntityTask
	{
		#region Member Variables

		private FormChromeleonSequenceGetResults m_Form;
		private ChromeleonSequenceEntity m_Sequence;
		private BackgroundWorker m_ResultWorker;
		private string m_MessageText;

		private bool m_Closed;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the helper.
		/// </summary>
		/// <value>
		/// The helper.
		/// </value>
		protected SequenceGetResultsHelper Helper { get; private set; }

		#endregion

		#region Overrides

		/// <summary>
		/// Tell the base class to allow multiple entries from explorer
		/// </summary>
		protected override void SetupTask()
		{
			bool first = true;
			AllowMultiple = true;

			// Just spin off additional task for remaining items

			foreach (IEntity entity in Context.SelectedItems)
			{
				if (first)
				{
					first = false;
					continue;
				}

				if (Context.MenuProcedureNumber == 0) break;

				Library.Task.CreateTask(Context.MenuProcedureNumber, entity);
			}

			// Start the Form etc

			base.SetupTask();
		}

		/// <summary>
		/// Entity Validation
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		protected override bool FindSingleEntityValidate(IEntity entity)
		{
			ChromeleonSequenceEntity found = (ChromeleonSequenceEntity)entity;

			if (found.Status.IsPhrase(PhraseChromStat.PhraseIdA) ||
				found.Status.IsPhrase(PhraseChromStat.PhraseIdX))
			{
				string title = Library.Message.GetMessage("ChromeleonLinkMessages", "InvalidStatusTitle");
				string message = Library.Message.GetMessage("ChromeleonLinkMessages", "InvalidStatusMessage");

				Library.Utils.FlashMessage(message, title, MessageButtons.OK, MessageIcon.Error, MessageDefaultButton.Button1);

				return false;
			}

			return base.FindSingleEntityValidate(entity);
		}

		/// <summary>
		/// Called to allow the consumer to extend the query
		/// </summary>
		/// <param name="query"></param>
		protected override void FindSingleEntityQuery(IQuery query)
		{
			query.AddEquals(ChromeleonSequencePropertyNames.Status, PhraseChromStat.PhraseIdV);
			query.AddOr();
			query.AddEquals(ChromeleonSequencePropertyNames.Status, PhraseChromStat.PhraseIdP);
			query.AddOr();
			query.AddEquals(ChromeleonSequencePropertyNames.Status, PhraseChromStat.PhraseIdC);
		}

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormChromeleonSequenceGetResults)MainForm;
			m_Sequence = (ChromeleonSequenceEntity)MainForm.Entity;

			m_Form.Closed += Form_Closed;
			m_Form.Closing += Form_Closing;

			m_ResultWorker = new BackgroundWorker();

			m_ResultWorker.DoWork += ResultWorker_DoWork;
			m_ResultWorker.ProgressChanged += ResultWorker_ProgressChanged;
			m_ResultWorker.RunWorkerCompleted += ResultWorker_RunWorkerCompleted;
			m_ResultWorker.WorkerReportsProgress = true;
			m_ResultWorker.WorkerSupportsCancellation = true;

			base.MainFormCreated();
		}

		/// <summary>
		/// Main form loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			Helper = new SequenceGetResultsHelper();

			m_Form.GetResultsButton.Click += GetResultsButton_Click;
			m_Form.UpdateButton.Click += UpdateButton_Click;
			m_Form.CloseButton.Click += CloseButton_Click;

			m_Form.Title = string.Format(m_Form.StringTable.FormTitle, m_Sequence.ChromeleonSequenceName);

			base.MainFormLoaded();
		}

		#endregion

		#region Close Button

		/// <summary>
		/// Handles the Click event of the CloseButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void CloseButton_Click(object sender, EventArgs e)
		{
			m_Closed = true;
			m_Form.Close();
		}

		/// <summary>
		/// Handles the Closing event of the Form control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void Form_Closing(object sender, CancelEventArgs e)
		{
			m_Closed = true;
			m_ResultWorker.CancelAsync();
		}

		/// <summary>
		/// Handles the Closed event of the Form control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Form_Closed(object sender, EventArgs e)
		{
			m_Closed = true;
		}

		#endregion

		#region Update

		/// <summary>
		/// Handles the ClickAndWait event of the UpdateButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		private void UpdateButton_Click(object sender, EventArgs e)
		{
			ClearMessages();
			Helper.LogMessageLevel = GetLoggerLevel();

			if (Helper.UpdateSequence(m_Sequence))
			{
				SaveChanges();
			}

			if (m_Closed) return;

			ShowMessages(Helper.LogMessages);
		}

		#endregion

		#region Get Results

		/// <summary>
		/// Handles the ClickAndWait event of the GetResultsButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void GetResultsButton_Click(object sender, EventArgs e)
		{
			m_ResultWorker.RunWorkerAsync();
		}

		/// <summary>
		/// Handles the DoWork event of the ResultWorker control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
		private void ResultWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			StartProcessing(m_Form.StringTable.StartRetrieval);

			Helper.LogMessageLevel = GetLoggerLevel();

			Helper.InjectionResultsProcessed -= Helper_InjectionResultsProcessed;
			Helper.InjectionResultsProcessed += Helper_InjectionResultsProcessed;

			if (Helper.RetrieveResults(m_Sequence))
			{
				if (m_Closed) return;
				SaveChanges();
			}
		}

		/// <summary>
		/// Handles the RunWorkerCompleted event of the ResultWorker control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RunWorkerCompletedEventArgs"/> instance containing the event data.</param>
		private void ResultWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			EndProcessing();
		}

		/// <summary>
		/// Handles the InjectionResultsProcessed event of the Helper control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EntryEventArgs"/> instance containing the event data.</param>
		private void Helper_InjectionResultsProcessed(object sender, EntryEventArgs e)
		{
			if (m_Closed) return;

			int position = m_Sequence.ChromeleonSequenceEntries.IndexOf(e.Entry) + 1;
			int total = m_Sequence.ChromeleonSequenceEntries.Count;
			double percent = (Convert.ToDouble(position)/Convert.ToDouble(total))*100;

			var logState = new List<LoggerMessage>(Helper.LogMessages);
			m_ResultWorker.ReportProgress(Convert.ToInt16(percent), logState);
		}

		/// <summary>
		/// Handles the ProgressChanged event of the ResultWorker control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="ProgressChangedEventArgs"/> instance containing the event data.</param>
		private void ResultWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (m_Closed) return;

			// Update the status window

			var logState = e.UserState as List<LoggerMessage>;
			if (logState != null)
			{
				ShowMessages(logState);
			}

			// Show Position

			m_Form.ProgressBar.Position = e.ProgressPercentage - 1;
		}

		/// <summary>
		/// Starts the processing.
		/// </summary>
		private void StartProcessing(string message)
		{
			if (m_Closed) return;

			// Put a processing message on the screen

			SetMessage(message);

			// Disable the buttons

			m_Form.UpdateButton.Enabled = false;
			m_Form.GetResultsButton.Enabled = false;
			m_Form.CloseButton.Enabled = false;
			m_Form.ProgressBar.Position = 1;
			m_Form.ProgressBar.Visible = true;
		}

		/// <summary>
		/// Ends the processing.
		/// </summary>
		private void EndProcessing()
		{
			if (m_Closed) return;

			// Show any remaining messages

			ShowMessages(Helper.LogMessages);

			// Enable Everything

			m_Form.UpdateButton.Enabled = true;
			m_Form.GetResultsButton.Enabled = true;
			m_Form.CloseButton.Enabled = true;
			m_Form.ProgressBar.Visible = false;
		}

		#endregion

		#region Save Changes

		/// <summary>
		/// Saves the changes.
		/// </summary>
		private void SaveChanges()
		{
			EntityManager.Transaction.Add(m_Sequence);
			EntityManager.Commit();
		}

		#endregion

		#region Logging

		/// <summary>
		/// Gets the logger level.
		/// </summary>
		/// <returns></returns>
		private LoggerLevel GetLoggerLevel()
		{
			if (m_Form.DebugError.Checked) return LoggerLevel.Error;
			if (m_Form.DebugWarn.Checked) return LoggerLevel.Warn;
			if (m_Form.DebugInfo.Checked) return LoggerLevel.Info;
			return LoggerLevel.Debug;
		}

		/// <summary>
		/// Clears the messages.
		/// </summary>
		private void ClearMessages()
		{
			SetMessage(string.Empty);
		}

		/// <summary>
		/// Shows the messages.
		/// </summary>
		/// <param name="messages">The messages.</param>
		private void ShowMessages(IEnumerable<LoggerMessage> messages)
		{
			StringBuilder builder = new StringBuilder();
			var localMessage = new List<LoggerMessage>(messages);

			bool first = true;

			foreach (LoggerMessage message in localMessage)
			{
				if (first) first = false; else builder.AppendLine();
				string format = m_Form.StringTable.LogMessageFormat.Replace("\\t", "\t");
				builder.AppendFormat(format, message.TimeStamp, message.Level, message.Message);
			}

			SetMessage(builder.ToString());
		}

		/// <summary>
		/// Sets the message.
		/// </summary>
		/// <param name="message">The message.</param>
		private void SetMessage(string message)
		{
			if (m_Closed) return;
			m_MessageText = string.Empty;

			m_MessageText = string.IsNullOrEmpty(m_MessageText) ? message : string.Concat(m_MessageText, "\r\n", message);
			m_Form.DebugComments.Text = m_MessageText;
		}

		#endregion
	}
}