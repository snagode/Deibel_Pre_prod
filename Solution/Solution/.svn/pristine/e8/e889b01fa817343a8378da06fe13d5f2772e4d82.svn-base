using Thermo.Informatics.Common.Forms.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Utilities;
using Thermo.SampleManager.Core.Definition;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.ClientControls.Data;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Inspection Page
	/// </summary>
	[SampleManagerPage("InspectionPage")]
	public class InspectionPage : PageBase
	{
		#region Member Variables

		private IEntityCollection m_Inspections;
		private IEntityCollection m_InspectionsPending;
		private IEntityCollection m_InspectionsHistory;
		private bool m_Setup;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the inspections.
		/// </summary>
		public IEntityCollection Inspections
		{
			get
			{
				if (m_Inspections == null)
				{
					m_Inspections = EntityManager.CreateEntityCollection(InspectorBase.EntityName);
					ISchemaTable table = MainForm.Entity.FindSchemaTable();
					if (table == null) return m_Inspections;

					IQuery query = EntityManager.CreateQuery(InspectorBase.EntityName);
					query.AddEquals(InspectorPropertyNames.TableName, table.Name);
					query.AddEquals(InspectorPropertyNames.RecordKey0, MainForm.Entity.IdentityString);

					m_Inspections = EntityManager.Select(InspectorBase.EntityName, query);
					m_Inspections.Owner = MainForm.Entity;
				}

				return m_Inspections;
			}
		}

		/// <summary>
		/// Gets the inspections history.
		/// </summary>
		public IEntityCollection InspectionsHistory
		{
			get
			{
				if (m_InspectionsHistory == null)
				{
					m_InspectionsHistory = EntityManager.CreateEntityCollection(InspectorBase.EntityName);
					foreach (Inspector inspector in Inspections)
					{
						if (inspector.IsCompleted) m_InspectionsHistory.Add(inspector);
					}
				}

				return m_InspectionsHistory;
			}
		}

		/// <summary>
		/// Gets the inspections pending.
		/// </summary>
		public IEntityCollection InspectionsPending
		{
			get
			{
				if (m_InspectionsPending == null)
				{
					m_InspectionsPending = EntityManager.CreateEntityCollection(InspectorBase.EntityName);
					foreach (Inspector inspector in Inspections)
					{
						if (inspector.IsPending) m_InspectionsPending.Add(inspector);
					}
				}
				return m_InspectionsPending;
			}
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Page Interface Loaded called after corresponding Form event. Multiple pages
		/// placing code here will result slower property sheet loading.
		/// </summary>
		/// <param name="formsUserInterface"></param>
		public override void PageInterfaceLoaded(IFormsUserInterface formsUserInterface)
		{
			base.PageInterfaceLoaded(formsUserInterface);

			// Prompts on the page are entity specific. Retrieve these prompts and assign Inspection Fields.
			// This override happens before the form is built.
            if (formsUserInterface.EntityType != null)
            {
                var inspectionControl =
                    (PromptEntityBrowseDefinition)
                    formsUserInterface.DesignDefinition.FindControlDefinition(FormInspectionPage.InspectionInspectionPlanControlName);


                var approvalControl =
                    (PromptPhraseBrowseDefinition)
                    formsUserInterface.DesignDefinition.FindControlDefinition(FormInspectionPage.InspectionApprovalStatusControlName);

                ISchemaTable table = Library.Schema.Tables[formsUserInterface.EntityType.Name];

                if (table.InspectionField != null)
                {
                    inspectionControl.Property = TextUtils.MakePascalCase(table.InspectionField.Name);
                    inspectionControl.Mandatory = Library.Environment.GetGlobalString("APPROVAL_INSPECT_SECURITY") != "LOW";
                }

                if (table.ApprovalStatusField != null)
                {
                    approvalControl.Property = TextUtils.MakePascalCase(table.ApprovalStatusField.Name);
                }
                else if (table.Name == TableNames.Sample)
                {
                    approvalControl.Property = SamplePropertyNames.Status;
                }

                var topPanel =
                    (PanelDefinition)
                    formsUserInterface.DesignDefinition.FindControlDefinition(
                        FormInspectionPage.InsTopSectionControlName);

                topPanel.Visible = true;
                var topPanelRead =
                    (PanelDefinition)
                    formsUserInterface.DesignDefinition.FindControlDefinition(
                        FormInspectionPage.InsTopSectionReadOnlyControlName);

                topPanelRead.Visible = false;

            }
            else
            {

                var topPanelRead =
                    (PanelDefinition)
                    formsUserInterface.DesignDefinition.FindControlDefinition(
                        FormInspectionPage.InsTopSectionReadOnlyControlName);

                topPanelRead.Visible = true;

                var topPanel =
                    (PanelDefinition)
                    formsUserInterface.DesignDefinition.FindControlDefinition(
                        FormInspectionPage.InsTopSectionControlName);

                topPanel.Visible = false;

            }
           
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
	        if (m_Setup) return;

            DataEntityCollection entrySource = (DataEntityCollection)MainForm.NonVisualControls["InspectionEntries"];
			entrySource.Publish(InspectionsHistory);
			DataEntityCollection pendingSource = (DataEntityCollection)MainForm.NonVisualControls["InspectionPendingEntries"];
			pendingSource.Publish(InspectionsPending);
            
			ISchemaTable table = Schema.Current.Tables[MainForm.Entity.EntityType];
	        var topPanel = (Panel) MainForm.Controls[FormInspectionPage.InsTopSectionReadOnlyControlName];
	        if (topPanel.Visible)
	        {
	            var inspectionPlan =
	                (PromptEntityBrowse)
	                MainForm.Controls[
	                    FormInspectionPage.InspectionInspectionPlanReadOnlyControlName];

	            inspectionPlan.Entity = MainForm.Entity.GetEntity(table.InspectionField.Name);


	            var approvalStatus =
	                (TextEdit)
	                MainForm.Controls[
	                    FormInspectionPage.InspectionApprovalStatusReadOnlyControlName];

	            var approvalEntity = (PhraseBase) MainForm.Entity.GetEntity(table.ApprovalStatusField.Name);
                approvalStatus.Text = approvalEntity.PhraseText;
	        }
	        m_Setup = true;
		}

		/// <summary>
		/// Method to decide if page should be added. False will not add the page to the property sheet
		/// or call the extension page methods.
		/// </summary>
		/// <param name="entityType">Type of the entity.</param>
		/// <returns></returns>
		public override bool AddPage(string entityType)
		{
			ISchemaTable table = Schema.Current.Tables[entityType];

			if (table.InspectionField == null || table.ApprovalStatusField == null && table.Name != TableNames.Sample)
			{
				m_ParentSampleManagerTask.Library.Utils.ShowAlert(Library.Message.GetMessage("ControlMessages",
				                                                                             "InspectionPageError"));
				return false;
			}

			return true;
		}
		
		#endregion
		
	}
}
