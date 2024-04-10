using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.ClientControls.Browse;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;
using Thermo.SampleManager.Server.Workflow.Helpers;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Workflow State Task
	/// </summary>
	[SampleManagerTask("WorkflowStateTask", "LABTABLE", "WORKFLOW_STATE")]
	public class WorkflowStateTask : GenericLabtableTask
	{
		#region Member Variables

		private WorkflowState m_Entity;
		private FormWorkflowState m_Form;
		private PhraseBrowse m_GlobalCellBrowse;
		private PhraseBrowse m_TypeDateBrowse;
		private PhraseBrowse m_TypeBrowse;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Entity = (WorkflowState)MainForm.Entity;
			m_Entity.WorkflowStateConditions.ItemPropertyChanged += new EntityCollectionEventHandler(WorkflowStateConditionsItemPropertyChanged);
			m_Form = (FormWorkflowState)MainForm;
			m_Form.Loaded += FormLoaded;
		}

		#endregion

		#region Events

		/// <summary>
		/// Handles the Loaded event of the MainForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void FormLoaded(object sender, System.EventArgs e)
		{
			SetEntity(m_Entity.TableName);

			m_Form.PromptTable.TableNameChanged += PromptTableTableNameChanged;
			m_Form.GridWorkflowStateConditions.CellEditor += GridWorkflowStateConditionsCellEditor;
			m_Form.GridWorkflowStateConditions.CellEnabled += new System.EventHandler<CellEnabledEventArgs>(GridWorkflowStateConditionsCellEnabled);

			m_GlobalCellBrowse = BrowseFactory.CreatePhraseBrowse(PhraseWflowGlob.Identity);
			BuildTypeBrowse();
		}

		/// <summary>
		/// Grids the workflow state conditions cell editor.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.CellEditorEventArgs"/> instance containing the event data.</param>
		private void GridWorkflowStateConditionsCellEditor(object sender, CellEditorEventArgs e)
		{
			if (e.PropertyName == WorkflowStateConditionPropertyNames.Operator)
			{
				WorkflowStateCondition condition = (WorkflowStateCondition)e.Entity;

				PromptAttribute promptDetails = GetPromptDetails(condition);

				if (promptDetails == null) return; 

				if (promptDetails is PromptLinkAttribute)
					e.Browse = m_Form.BrowseStringSimpleOperators;

				else if (promptDetails is PromptBooleanAttribute)
					e.Browse = m_Form.BrowseStringSimpleOperators;

				else if (promptDetails is PromptPhraseAttribute)
					e.Browse = m_Form.BrowseStringSetOperators;

				else
					e.Browse = m_Form.BrowseStringOperators;

				return;
			}

			if (e.PropertyName == WorkflowStateConditionPropertyNames.Type)
			{
				WorkflowStateCondition condition = (WorkflowStateCondition)e.Entity;

				PromptAttribute promptDetails = GetPromptDetails(condition);

				if (promptDetails == null) return; 

				if (promptDetails is PromptDateAttribute)
				{
					e.Browse = m_TypeDateBrowse;
				}
				else
				{
                    e.Browse = m_TypeBrowse;
				}

				return;
			}

			if (e.PropertyName == WorkflowStateConditionPropertyNames.Value)
			{
				WorkflowStateCondition condition = (WorkflowStateCondition)e.Entity;

				PromptAttribute promptDetails = GetPromptDetails(condition);

				if (promptDetails == null) return;

				if ((condition.IsValue) || (condition.IsDate))
				{
					// This condition is using a value
					e.BrowseAllowMultiple = (condition.Operator == Condition.OperatorIn || condition.Operator == Condition.OperatorNotIn);
					e.SetFromPromptAttribute(promptDetails);

					EntityBrowse browse = e.Browse as EntityBrowse;
					if (browse != null) browse.ReturnProperty = "IdentityString";

					PhraseBrowse phrase = e.Browse as PhraseBrowse;
					if (phrase != null) e.PhraseUseDescription = false;
				}
				else if (condition.IsInterval)
				{
					e.DataType = SMDataType.Interval;
				}
				else
				{
					// This is a global condition, assign the global phrase browse
					e.Browse = m_GlobalCellBrowse;
				}

				return;
			}
		}

		/// <summary>
		/// Handles the CellEnabled event of the GridWorkflowStateConditions control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.CellEnabledEventArgs"/> instance containing the event data.</param>
		private static void GridWorkflowStateConditionsCellEnabled(object sender, CellEnabledEventArgs e)
		{
			if (e.PropertyName == WorkflowStateConditionPropertyNames.Type)
			{
				WorkflowStateCondition condition = (WorkflowStateCondition)e.Entity;
				e.Enabled = condition.AllowGlobal;
				e.DisabledMode = DisabledCellDisplayMode.GreyShowContents;
			}
		}

		/// <summary>
		/// Table Name Changed
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.TextChangedEventArgs"/> instance containing the event data.</param>
		private void PromptTableTableNameChanged(object sender, TextChangedEventArgs e)
		{
			SetEntity(e.Text);
		}

		/// <summary>
		/// Handles the ItemPropertyChanged event of the WorkflowStateConditions control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void WorkflowStateConditionsItemPropertyChanged(object sender, EntityCollectionEventArgs e)
		{
			if (e.PropertyName == WorkflowStateConditionPropertyNames.Property)
			{
				WorkflowStateCondition condition = (WorkflowStateCondition) e.Entity;

				PromptAttribute promptDetails = GetPromptDetails(condition);

				if (promptDetails is PromptDateAttribute)
				{
					if (condition.IsValue)
						condition.Type = (PhraseBase) EntityManager.SelectPhrase(PhraseWfCondTy.Identity, PhraseWfCondTy.PhraseIdD);
				}
				else if (promptDetails != null)
				{
					if ((condition.IsDate) || (condition.IsInterval))
						condition.Type = (PhraseBase) EntityManager.SelectPhrase(PhraseWfCondTy.Identity, PhraseWfCondTy.PhraseIdV);
				}
			}

			if (e.PropertyName == WorkflowStateConditionPropertyNames.Type ||
			    e.PropertyName == WorkflowStateConditionPropertyNames.Property)
			{
				m_Form.GridWorkflowStateConditions.RefreshRow(e.Entity);
			}
		}

		#endregion

		#region Browse

		private void BuildTypeBrowse()
		{
			// Type browse for dates

			IQuery datePhraseQuery = EntityManager.CreateQuery(TableNames.Phrase);

			datePhraseQuery.AddEquals(PhrasePropertyNames.Phrase, PhraseWfCondTy.Identity);
			datePhraseQuery.AddNotEquals(PhrasePropertyNames.PhraseId, PhraseWfCondTy.PhraseIdV);

			m_TypeDateBrowse = BrowseFactory.CreatePhraseBrowse(datePhraseQuery);

			// Type browse for all other data types

			IQuery phraseQuery = EntityManager.CreateQuery(TableNames.Phrase);

			phraseQuery.AddEquals(PhrasePropertyNames.Phrase, PhraseWfCondTy.Identity);
			phraseQuery.AddNotEquals(PhrasePropertyNames.PhraseId, PhraseWfCondTy.PhraseIdD);
			phraseQuery.AddNotEquals(PhrasePropertyNames.PhraseId, PhraseWfCondTy.PhraseIdI);

			m_TypeBrowse = BrowseFactory.CreatePhraseBrowse(phraseQuery);
            //m_GlobalCellBrowse = BrowseFactory.CreatePhraseBrowse(phraseQuery); //!!
		}

		private PromptAttribute GetPromptDetails(WorkflowStateCondition condition)
		{
			string propertyName = condition.Property;

			if (string.IsNullOrEmpty(propertyName)) return null;

			string entityName = m_Entity.TableName;

			if (string.IsNullOrEmpty(entityName)) return null;
			if (!EntityType.ContainsProperty(entityName, propertyName)) return null;

			return EntityType.GetPromptAttribute(entityName, propertyName);
		}

		#endregion

		#region Entity Stuff

		/// <summary>
		/// Sets the entity.
		/// </summary>
		/// <param name="entityName">Name of the entity.</param>
		private void SetEntity(string entityName)
		{
			// Drop out if this is a display only operation

			bool modifyOption = (Context.LaunchMode == AddOption || Context.LaunchMode == ModifyOption || Context.LaunchMode == TestOption);
			if (!modifyOption) return;

			// Make the Grid read only until a user fills in a table

			if (string.IsNullOrEmpty(entityName))
			{
				m_Form.GridWorkflowStateConditions.ReadOnly = true;
				return;
			}

			// Publish the list of available values

			m_Form.GridWorkflowStateConditions.ReadOnly = false;

			IList<string> propertyNames = EntityType.GetReflectedPropertyNames(entityName, false);
            propertyNames=(from r in propertyNames
                            orderby r
                            select r).ToList();
            
			m_Form.BrowseStringCollection.Republish((List<string>)propertyNames);
		}

		#endregion

	}
}