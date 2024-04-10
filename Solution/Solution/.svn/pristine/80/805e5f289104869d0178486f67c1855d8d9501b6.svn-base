using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the INSTRUMENT_PART_TEMPLATE entity.
	/// </summary>
	[SampleManagerEntity(InstrumentPartTemplateBase.EntityName)]
	public class InstrumentPartTemplate : InstrumentPartTemplateBase
	{
		#region Member Variables

		private IEntityCollection m_AssignedInstrumentTemplates;

		#endregion

		#region Overrides

		/// <summary>
		/// Called before the entity is committed as part of a transaction.
		/// </summary>
		protected override void OnPreCommit()
		{
			m_AssignedInstrumentTemplates = null;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the instrument templates to which this part template is assigned.
		/// </summary>
		/// <value>The instrument templates.</value>
		[PromptCollection(TableNames.InstrumentTemplate)]
		public IEntityCollection AssignedInstrumentTemplates
		{
			get
			{
				if (m_AssignedInstrumentTemplates == null)
				{
					//Select the assigned instrument templates
					IQuery query = EntityManager.CreateQuery(TableNames.InstrumentPartLinkTemplate);
					query.AddEquals(InstrumentPartLinkTemplatePropertyNames.InstrumentPartTemplate, Identity);
					List<object> instrumentTemplates = EntityManager.SelectDistinct(query, "INSTRUMENT_TEMPLATE");

					if (instrumentTemplates.Count > 0)
					{
						IQuery instQuery = EntityManager.CreateQuery(TableNames.InstrumentTemplate);
						instQuery.AddIn("IDENTITY", instrumentTemplates);
						instQuery.AddOrder("IDENTITY", true);
						m_AssignedInstrumentTemplates = EntityManager.Select(TableNames.InstrumentTemplate, instQuery);
					}
					else
					{
						m_AssignedInstrumentTemplates = EntityManager.CreateEntityCollection(TableNames.InstrumentTemplate);
					}
				}
				return m_AssignedInstrumentTemplates;
			}
		}

		#endregion

	}
}
