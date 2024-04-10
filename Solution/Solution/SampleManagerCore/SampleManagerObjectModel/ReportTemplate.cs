using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the REPORT_TEMPLATE entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class ReportTemplate : ReportTemplateBase
	{
		/// <summary>
		/// The specific parameters
		/// </summary>
		public List<ReportSpecificParameter> SpecificParameters = new List<ReportSpecificParameter>();

		/// <summary>
		/// Shallow Clones this instance - FOR SpecificParameters USE ONLY.
		/// </summary>
		/// <returns></returns>
		public ReportTemplate ShallowClone()
		{
			ReportTemplate returnEntity = (ReportTemplate) this.MemberwiseClone();
			returnEntity.SpecificParameters=new List<ReportSpecificParameter>();
			return returnEntity;
		}
	}
}