using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended modular report
	/// </summary>
	[SampleManagerEntity(ModularReportBase.EntityName)]
	public class ModularReport : ModularReportBase
	{
		/// <summary>
		/// Gets the children report links.
		/// </summary>
		/// <value>The children report links.</value>
		public IEntity[] ReportTemplates
		{
		    get
		    {
				IEntity[] collection = new IEntity[ModularReportItems.Count];
			    int i = 0;
		        foreach (ModularReportItemBase reportLink in ModularReportItems)
		        {
		            collection[i]=((ReportTemplate)reportLink.ReportTemplate).ShallowClone();
			        i++;
		        }
		        return collection;
		    }
		}
	}
}
