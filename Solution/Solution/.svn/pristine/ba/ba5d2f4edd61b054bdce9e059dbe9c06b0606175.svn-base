using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.ImportExport;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the MLP_COMPONENTS entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class MlpComponents : MlpComponentsBase
	{
		#region Member Variables

		private IEntityCollection m_MlpValues;

		#endregion

		#region Export Methods

		/// <summary>
		/// Gets the Properties that must be processed on the model.
		/// </summary>
		/// <returns></returns>
		public override List<string> GetCustomExportableProperties()
		{
			var l = base.GetCustomExportableProperties();
			l.Add("MlpValues");
			return l;
		}

		/// <summary>
		/// Gets Property's value linked data.
		/// </summary>
		/// <param name="propertyName">The property name to process</param>
		/// <param name="exportList">The Entity Export List</param>
		public override void GetLinkedData(string propertyName, EntityExportList exportList)
		{
			if (propertyName == "MlpValues")
			{
				foreach (IEntity o in MlpValues)
				{
					o.Parent = this;
					exportList.AddEntity(o);
				}
			}
			else
			{
				base.GetLinkedData(propertyName, exportList);
			}
		}

		#endregion

		#region Overrides

		/// <summary>
		/// OnDeleted
		/// </summary>
		protected override void OnDeleted()
		{
			foreach (IEntity mlpValue in MlpValues)
			{
				EntityManager.Delete(mlpValue);
			}
			base.OnDeleted();
		}

		/// <summary>
		/// Perform post copy processing.
		/// </summary>
		/// <param name="sourceEntity">The entity that was used to create this instance.</param>
		protected override void OnEntityCopied(IEntity sourceEntity)
		{
			base.OnEntityCopied(sourceEntity);
			MlpValues.CopyCollection(((MlpComponents) sourceEntity).MlpValues);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the MlpValues.
		/// </summary>
		/// <value>
		/// The MlpValues.
		/// </value>
		[PromptCollection(MlpValuesBase.EntityName, true)]
		public IEntityCollection MlpValues
		{
			get
			{
				if (m_MlpValues == null)
				{
					var q = EntityManager.CreateQuery("MLP_VALUES");
					q.AddEquals("ENTRY_CODE", EntryCode);
					m_MlpValues = EntityManager.Select(q);
					foreach (IEntity o in m_MlpValues)
					{
						o.Parent = this;
					}
				}
				return m_MlpValues;
			}
		}

		#endregion
	}
}
