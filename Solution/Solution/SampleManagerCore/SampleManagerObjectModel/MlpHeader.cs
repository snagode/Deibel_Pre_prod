using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.ImportExport;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the MLP_HEADER entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class MlpHeader : MlpHeaderBase
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
		/// Called when an entity is marked for deletion.
		/// </summary>
		protected override void OnPrepareDelete()
		{
			foreach (IEntity mlpValue in MlpValues)
			{
				EntityManager.Delete(mlpValue);
			}
			
			base.OnPrepareDelete();
		}

		/// <summary>
		/// Perform post copy processing.
		/// </summary>
		/// <param name="sourceEntity">The entity that was used to create this instance.</param>
		protected override void OnEntityCopied(IEntity sourceEntity)
		{
			base.OnEntityCopied(sourceEntity);
			MlpValues.CopyCollection(((MlpHeader)sourceEntity).MlpValues);
		}

		/// <summary>
		/// Creates a copy of the entity as a new version
		/// </summary>
		/// <returns>
		/// The copy.
		/// </returns>
		public override IEntity CreateNewVersion()
		{
			var oldEntryCode = EntryCode;
			var newEntity = (MlpHeader)base.CreateNewVersion();
			PackedDecimal newEntryCode = Library.Increment.GetIncrement("MLP_COMPS", "ENTRY_CODE");

			UpdateValues(newEntity.MlpValues, oldEntryCode, newEntryCode);
			foreach (MlpComponents mlpComponent in newEntity.MlpComponents)
			{
				oldEntryCode = mlpComponent.EntryCode;
				var newCompEntryCode =  Library.Increment.GetIncrement("MLP_COMPS", "ENTRY_CODE");
				mlpComponent.ProductVersion = newEntity.ProductVersion;
				UpdateValues(mlpComponent.MlpValues, oldEntryCode, newCompEntryCode);
				mlpComponent.EntryCode = newCompEntryCode;
			}
			newEntity.EntryCode = newEntryCode;

			return newEntity;
		}

		/// <summary>
		/// Updates the values.
		/// </summary>
		/// <param name="values">The values.</param>
		/// <param name="oldEntryCode">The old entry code.</param>
		/// <param name="newEntryCode">The new entry code.</param>
		private void UpdateValues(IEntityCollection values, PackedDecimal oldEntryCode, PackedDecimal newEntryCode)
		{
			foreach (MlpValues mlpValue in values)
			{
				if (mlpValue.EntryCode == oldEntryCode)
				{
					mlpValue.EntryCode = newEntryCode;
				}
			}
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

		/// <summary>
		/// Gets the MlpValues.
		/// </summary>
		/// <value>
		/// The MlpValues.
		/// </value>
		[PromptCollection(MlpLevelBase.EntityName, true)]
		public IEntityCollection MlpLevels
		{
			get
			{
				IEntityCollection collection = EntityManager.CreateEntityCollection(MlpLevelBase.EntityName, false);
				foreach (MlpValues mlpValue in MlpValues)
				{
					collection.Add(mlpValue.LevelId);
				}
				return collection;
			}
		}

		#endregion
	}
}
