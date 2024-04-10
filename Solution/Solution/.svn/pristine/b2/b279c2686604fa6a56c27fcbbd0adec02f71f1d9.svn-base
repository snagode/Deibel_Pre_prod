using System.Collections.Generic;
using System.ComponentModel;
using Thermo.Framework.Core;
using Thermo.Informatics.Common.Forms.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Utilities;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.ClientControls.Browse;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Server;
using EntityDefinition = Thermo.SampleManager.Server.EntityDefinition;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Page to display template fields.
	/// </summary>
	[SampleManagerPage("LinksPage")]
	public class LinksPage : PageBase
	{
		#region Member Variables
		private SimpleTreeList m_SimpleTreeList;
		private EntityBrowse m_EntityBrowse;
	    private ExplorerGrid m_ExplorerGrid;
		private bool m_TreeLoaded;

		#endregion

        /// <summary>
        /// Page Selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void PageSelected(object sender, RuntimeFormsEventArgs e)
        {
            base.PageSelected(sender, e);
            if (!m_TreeLoaded)
            {
                m_ExplorerGrid = (ExplorerGrid)MainForm.Controls[FormLinksPage.LinkExplorerGridControlName];
                m_SimpleTreeList = (SimpleTreeList)MainForm.Controls[FormLinksPage.LinksTreeListControlName];
                m_SimpleTreeList.FocusedNodeChanged += m_SimpleTreeList_FocusedNodeChanged;

                var currentEntityType = new FormsEntityType(MainForm.Entity.EntityType);

                if (currentEntityType.Name != "PHRASE_HEADER")
                {

                    foreach (EntityDefinition definition in Schema.Current.EntityDefinitions)
                    {
                        if (!string.IsNullOrEmpty(definition.DataSource))
                        {
                            List<IFormsPropertyType> propertiesFound = GetLinkedProperties(definition);

                            if (propertiesFound.Count == 1)
                            {

                                m_SimpleTreeList.AddNode(null, GetTreeName(definition.Name),
                                                               new IconName(definition.DefaultIcon),
                                                               propertiesFound[0]);


                            }
                            else if (propertiesFound.Count > 1)
                            {
                                bool first = true;
                                SimpleTreeListNodeProxy table = null;
                                foreach (IFormsPropertyType formsPropertyType in propertiesFound)
                                {
                                    if (first)
                                    {
                                        table = m_SimpleTreeList.AddNode(null,
                                                                         GetTreeName(
                                                                             formsPropertyType.EntityType.Name),
                                                                         new IconName(
                                                                             formsPropertyType.EntityType.Name),
                                                                         null);
                                        first = false;
                                    }

                                    if (table != null)
                                    {
                                        m_SimpleTreeList.AddNode(table, formsPropertyType.Name, formsPropertyType);
                                    }
                                }
                            }

                        }
                    }
                }
                else
                {
                    var entityType = new FormsEntityType("PHRASE");
                    IFormsPropertyType phraseType = entityType.GetProperty("PhraseType");
                    string display = TextUtils.GetDisplayText(entityType.Name);
                    string icon = Library.Utils.GetDefaultIcon(phraseType.EntityType.Name);
                    m_SimpleTreeList.AddNode(null, display, new IconName(icon), phraseType);
                }

                if (m_SimpleTreeList.Nodes.Count > 0)
                {
                    m_SimpleTreeList.FocusNode(m_SimpleTreeList.Nodes[0]);
                    m_SimpleTreeList_FocusedNodeChanged(null,
                                                        new SimpleFocusedNodeChangedEventArgs(null,
                                                                                              m_SimpleTreeList.Nodes[0]));
                }

                m_TreeLoaded = true;
            }

        }

        /// <summary>
        /// Get the linked properties
        /// </summary>
        /// <param name="definition"></param>
        /// <returns></returns>
	    private List<IFormsPropertyType> GetLinkedProperties(EntityDefinition definition)
	    {
	        var propertiesFound = new List<IFormsPropertyType>();
	        var entityType = new FormsEntityType(definition.DataSource);
	        foreach (IFormsPropertyType property in entityType.GetProperties())
	        {
                if (property.IsLink && !property.IsLinkToMany &&
                    property.LinkedEntityType.Name == MainForm.Entity.EntityType)
                {
                    string reverseProperty = EntityType.ReversePropertyName(property.LinkedEntityType.Name, property.Name);
                    EntityProperty propertyFound = definition.Properties[reverseProperty];
                    if (propertyFound != null)
                    {
                        propertiesFound.Add(property);
                    }
                }
	        }
	        return propertiesFound;
	    }

        /// <summary>
        /// GetTreeName
        /// </summary>
        /// <param name="linkType"></param>
        /// <returns></returns>
        private static string GetTreeName(string linkType)
        {
            return TextUtils.GetDisplayText(linkType);
        }

        /// <summary>
        /// GetTreeSelectedName
        /// </summary>
        /// <param name="linkType"></param>
        /// <param name="linkedPropertyName"></param>
        /// <returns></returns>
        private static string GetTreeSelectedName(string linkType, string linkedPropertyName)
        {
            return string.Format("{0} ({1})",  TextUtils.GetDisplayText(linkType),  linkedPropertyName);
        }

	    #region Events

		/// <summary>
		/// Handles the FocusedNodeChanged event of the m_SimpleTreeList control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.SimpleFocusedNodeChangedEventArgs"/> instance containing the event data.</param>
        void m_SimpleTreeList_FocusedNodeChanged(object sender, SimpleFocusedNodeChangedEventArgs e)
		{
		    if (e.OldNode != null && e.OldNode.ParentNode == null)
		    {
		        if ( e.OldNode.Data is IFormsPropertyType)
		        {
		            var oldProperty = (IFormsPropertyType) e.OldNode.Data;
		            e.OldNode.DisplayText = GetTreeName(oldProperty.EntityType.Name);
		        }
		    }

		    if (e.NewNode.Data is IFormsPropertyType)
		    {
		        var currentProperty = (IFormsPropertyType) e.NewNode.Data;
		        IQuery query = EntityManager.CreateQuery(currentProperty.EntityType.Name);
		        query.AddEquals(currentProperty.Name, MainForm.Entity);

		        IEntityCollection collection = EntityManager.Select(query.TableName, query);
		        m_EntityBrowse = BrowseFactory.CreateEntityBrowse(collection);
                m_EntityBrowse.AddColumnsFromTableDefaults();
		        m_ExplorerGrid.Browse = m_EntityBrowse;

		        if (e.NewNode.ParentNode == null)
		        {
		            e.NewNode.DisplayText = GetTreeSelectedName(currentProperty.EntityType.Name, currentProperty.Name);
		        }
		    }
		}

	    #endregion
	}

}
