using System;
using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core.Definition;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.ClientControls.Browse;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Server.Workflow.Definition;
using Thermo.SampleManager.Tasks.BusinessObjects;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Workflow Journal page allows journal entries to be displayed on a property sheet.
	/// </summary>
	[SampleManagerPage("WorkflowJournalPage")]
	public class WorkflowJournalPage : PageBase
	{
		#region Private Members

		private Calendar m_JournalCalendar;
		private EntityBrowse m_JournalBrowse;
		private ExplorerGrid m_ExplorerGrid;
		private IEntity m_Entity;

		#endregion

		#region Page Override Methods

		/// <summary>
		/// Method to decide if page should be added. False will not add the page to the property sheet
		/// or call the extension page methods.
		/// </summary>
		/// <param name="entityType">Type of the entity.</param>
		/// <returns></returns>
		public override bool AddPage(string entityType)
		{
			if (Context.SelectedItems.Count == 0) return true;
			IEntity item = Context.SelectedItems[0];
			return (item.GetWorkflowNode() != null);
		}

		/// <summary>
		/// Page Selected is called once the user selects this page and therefore will not
		/// effect property sheet loading. Labour intensive code should be place here or
		/// on a background task.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="T:Thermo.SampleManager.Library.RuntimeFormsEventArgs"/> instance containing the event data.</param>
		public override void PageSelected(object sender, RuntimeFormsEventArgs e)
		{
			base.PageSelected(sender, e);
			if (Context.SelectedItems.Count != 1) return;
			if (m_ExplorerGrid != null) return;

			lock (this)
			{
				JournalPageSelected();
			}
		}

		/// <summary>
		/// Journal page selected.
		/// </summary>
		private void JournalPageSelected()
		{
			m_ExplorerGrid = (ExplorerGrid)MainForm.Controls[FormWorkflowJournalPage.JournalEntryGridControlName];
			m_Entity = Context.SelectedItems[0];

			// Created Workflow 

			PromptEntityBrowse flow = (PromptEntityBrowse)MainForm.Controls[FormWorkflowJournalPage.JournalWorkflowControlName];
			PromptEntityBrowse flowNode = (PromptEntityBrowse)MainForm.Controls[FormWorkflowJournalPage.JournalWorkflowNodeControlName];

			WorkflowNodeInternal node = (WorkflowNodeInternal)m_Entity.GetWorkflowNode();

			IEntityCollection nodes = EntityManager.CreateEntityCollection(WorkflowNodeBase.EntityName);
			nodes.Add(node);
			flowNode.Browse = BrowseFactory.CreateEntityBrowse(nodes);
			flowNode.Entity = node;

			IEntityCollection flows = EntityManager.CreateEntityCollection(WorkflowBase.EntityName);
			flows.Add(node.Workflow);
			flow.Browse = BrowseFactory.CreateEntityBrowse(flows);
			flow.Entity = node.Workflow;

			// Timeline

			m_JournalCalendar = (Calendar)MainForm.Controls[FormWorkflowJournalPage.JournalCalendarControlName];

			IJournal journal = new JournalWorkflow(m_Entity);
			journal.Fill();

			// Columns

			var columns = new EntityBrowseColumnCollection();

			AddColumn(columns, WorkflowJournalPropertyNames.PerformedOn, 10, "WorkflowJournalPerformedOnColumn");
			AddColumn(columns, WorkflowJournalInternal.DescriptionPropertyName, 30, "WorkflowJournalDescriptionColumn");
			AddColumn(columns, WorkflowJournalPropertyNames.PerformedBy, 10, "WorkflowJournalPerformedByColumn");

			m_JournalBrowse = BrowseFactory.CreateEntityBrowse(journal.JournalCollection, columns);
			m_ExplorerGrid.Browse = m_JournalBrowse;

			PublishOnCalendar(journal);
		}

		/// <summary>
		/// Publish the calendar event of the journal
		/// </summary>
		/// <param name="journal">The journal</param>
		private void PublishOnCalendar(IJournal journal)
		{
			IList<UnboundCalendarEvent> unboundEvents = journal.GetJournalAsCalendarEvents();

			List<UnboundCalendarEvent> list = new List<UnboundCalendarEvent>();
			list.AddRange(unboundEvents);

			m_JournalCalendar.AddUnboundEvents(list);

			if (!journal.Start.IsNull)
			{
				TimeSpan difference = journal.End.Value - journal.Start.Value;

				m_JournalCalendar.SetTimeScaleForTimeLineView(difference);
				m_JournalCalendar.GoToDate(journal.Start.Value);
			}
		}

		/// <summary>
		/// Adds the column.
		/// </summary>
		/// <param name="collection">The collection.</param>
		/// <param name="property">The property.</param>
		/// <param name="width">The width.</param>
		/// <param name="titleMessage">The title message.</param>
		private void AddColumn(EntityBrowseColumnCollection collection, string property, int width, string titleMessage)
		{
			string title = Library.Message.GetMessage("WorkflowMessages", titleMessage);

			var columnDefinition = new EntityBrowseColumnDefinition
			{
				Column = property,
				Title = title,
				Width = width
			};

			collection.Add(columnDefinition);
		}

		#endregion
	}
}
