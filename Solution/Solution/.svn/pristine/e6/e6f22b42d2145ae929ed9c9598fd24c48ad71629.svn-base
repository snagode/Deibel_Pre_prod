using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.ImportExport;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the EXPLORER_HIERARCHY_LEVEL entity.
	/// </summary>
	[SampleManagerEntity(ExplorerHierarchyLevelBase.EntityName)]
	public class ExplorerHierarchyLevel : ExplorerHierarchyLevelBase
    {
        #region Export
        
        /// <summary>
        /// Gets the Properties that must be processed on the model.
        /// </summary>
        /// <returns></returns>
        public override List<string> GetCustomExportableProperties()
        {
            List<string> properties = base.GetCustomExportableProperties();
            properties.Add(ExplorerHierarchyLevelPropertyNames.Criteria);
            return properties;
        }

        /// <summary>
        /// Gets Property's value linked data.
        /// </summary>
        /// <param name="propertyName">The property name to process</param>
        /// <param name="exportList">The Entity Export List</param>
        public override void GetLinkedData(string propertyName, EntityExportList exportList)
        {
            if (propertyName == ExplorerHierarchyLevelPropertyNames.Criteria)
            {
                if (string.IsNullOrEmpty(Criteria))
                {
                    return;
                }

                IEntity criteria = EntityManager.Select(CriteriaSaved.EntityName, new Identity(LinkTableName, Criteria));
                exportList.AddEntity(criteria);
            }
        }

        #endregion
    }
}
