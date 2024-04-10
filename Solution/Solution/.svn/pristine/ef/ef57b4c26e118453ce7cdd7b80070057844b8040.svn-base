using Thermo.Informatics.Common.Forms.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core.Definition;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls.Data;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Basic form task to open a form using the context selection list.
	/// </summary>
	[SampleManagerTask("DefaultPropertyTask")]
	public class DefaultPropertyTask : DefaultFormTask
    {

        #region Constants

        const string Group = "GroupId";
		const string ModifiedBy = "ModifiedBy";
		const string ModifiedOn = "ModifiedOn";
		const string Modifiable = "Modifiable";

        #endregion

        #region Member Variables

        private string m_EntityType;
        private bool IsSpecific { get; set; }

        #endregion

        #region Overrides

        /// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			Context.TaskParameters[0] = "GenericPropertySheet";
		    m_EntityType = Context.EntityType;

            if (Context.SelectedItems.Count == 0)
            {
                // Deal with Menu Proc with multiple keys.
                if ( Context.TaskParameters.Length > 2)
                {
                    var identities = new object[Context.TaskParameters.Length - 2];

                    for (int i = 2; i < Context.TaskParameters.Length; i++)
                    {
                        identities[i - 2] = Context.TaskParameters[i];
                    }

                    IEntity entity = EntityManager.Select(Context.TaskParameters[1], new Identity(identities));
                    if (!entity.IsNull())
                    {
                        Context.SelectedItems.Add(entity);
                        m_EntityType = entity.EntityType;
                    }
                }
            }

			IQuery propertySheetQuery = EntityManager.CreateQuery("FORM");
			propertySheetQuery.AddEquals("Category", "PROPERTY");
            propertySheetQuery.AddEquals("FormEntityDefinition", m_EntityType);

			// Check to see if there is a specific property sheet for the entity type
			IEntityCollection collection = EntityManager.Select("FORM", propertySheetQuery);

			if (collection.Count > 0)
			{
				Context.TaskParameters[0] = collection[0].Name;
				IsSpecific = true;
			}
			else
			{
				// If it is a GenericPropertySheet clear the cache for it as prompts can be different on the general page
				IFormsUserInterfaceService m_UserInterfaceService = (IFormsUserInterfaceService)Library.GetService(typeof(IFormsUserInterfaceService));
				m_UserInterfaceService.ClearCache(Context.TaskParameters[0]);
			}
			
			FormFactory.InterfaceLoaded += FormFactoryInterfaceLoaded;

			base.SetupTask();
		}

		/// <summary>
		/// Handles the InterfaceLoaded event of the FormFactory control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.InterfaceEventArgs"/> instance containing the event data.</param>
		void FormFactoryInterfaceLoaded(object sender, InterfaceEventArgs e)
		{
			LayoutDefinition layout = e.UserInterface.DesignDefinition;

			DataEntityDefinition dataEntityDefinition =
				(DataEntityDefinition) layout.FindControlDefinition("GenericEntity");

			if (dataEntityDefinition != null)
			{
                dataEntityDefinition.Entity = m_EntityType;
			}

			if (!IsSpecific)
			{
				TextEditDefinition identityPrompt =
					(TextEditDefinition) layout.FindControlDefinition(FormGenericPropertySheet.IdentityControlName);

                ISchemaTable table = Schema.Current.Tables[m_EntityType];

				if (table != null)
				{
					if (table.BrowseField != null)
						identityPrompt.Property = table.BrowseField.Name;
					else if (table.KeyFields.Count > 0)
						identityPrompt.Property = table.KeyFields[0].Name;
					else if (table.Fields.Count > 0)
						identityPrompt.Property = table.Fields[0].Name;
				}

				SetPromptVisible(layout, Group);
				SetPromptVisible(layout, ModifiedBy);
				SetPromptVisible(layout, ModifiedOn);
				SetPromptVisible(layout, Modifiable);

				LineDefinition line =
					(LineDefinition) layout.FindControlDefinition(FormGenericPropertySheet.LineUnderModifiedByControlName);
                line.Visible = EntityType.ContainsProperty(m_EntityType, Modifiable);

			}
		}

		/// <summary>
		/// Sets the prompt visible.
		/// </summary>
		/// <param name="layout">The layout.</param>
		/// <param name="promptDefinitionName">Name of the prompt definition.</param>
		void SetPromptVisible(LayoutDefinition layout, string promptDefinitionName)
		{
			PromptDefinition prompt = (PromptDefinition)layout.FindControlDefinition(promptDefinitionName);
            prompt.Visible = EntityType.ContainsProperty(m_EntityType, prompt.Property);
		}

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			if (!IsSpecific)
			{
				if (MainForm.NonVisualControls.Contains("GenericEntity"))
				{
					DataEntity entitySource = (DataEntity)MainForm.NonVisualControls["GenericEntity"];
					entitySource.Publish(MainForm.Entity);
				}
			}
		}

		#endregion
	}
}