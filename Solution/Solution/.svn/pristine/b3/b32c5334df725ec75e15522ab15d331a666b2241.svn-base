using System.Collections.Generic;
using System.Globalization;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.ImportExport;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.ImportExport;
using Thermo.SampleManager.ObjectModel.Import_Helpers;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the EXPLORER_FOLDER entity.
	/// </summary>
	[SampleManagerEntity(ExplorerFolderBase.EntityName)]
	public class ExplorerFolder : ExplorerFolderBase,IImportableEntity
	{
		#region Member Variables

		/// <summary>
		/// The criteria linked to the explorer folder.
		/// </summary>
		private CriteriaSaved m_CriteriaSaved;

        #endregion

		#region RMB Numbers

		/// <summary>
		/// Perform pre commit processing.
		/// </summary>
		protected override void OnPreCommit()
		{
			foreach (ExplorerRmb rmb in Rmbs.ActiveItems)
			{
				rmb.UpdateRMBNumber();
			}


			
			base.OnPreCommit();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets and sets the criteria saved.
		/// </summary>
		/// <value>The criteria saved.</value>
		[PromptLink]
		public CriteriaSaved CriteriaSaved
		{
			get
			{
				if (m_CriteriaSaved == null)
				{
					IEntity criteriaFound = EntityManager.Select("CRITERIA_SAVED", new Identity(TableName, CriteriaName));
					if (criteriaFound != null)
					{
						m_CriteriaSaved = (CriteriaSaved)criteriaFound;
					}
					else
					{
						m_CriteriaSaved =
							(CriteriaSaved)EntityManager.CreateEntity("CRITERIA_SAVED", new Identity(TableName, CriteriaName));
						CriteriaSavedIdentity = m_CriteriaSaved.Identity;
					}

					m_CriteriaSaved.PublicCriteria = false;
				}
				
				return m_CriteriaSaved;
			}

			set
			{
				m_CriteriaSaved = value;
				EntityManager.Transaction.Add(m_CriteriaSaved);
			}
		}

        /// <summary>
        /// Assign the criteria table.
        /// </summary>
        /// <param name="newTableName"></param>
        public void ResetCriteriaTable(string newTableName)
        {
            if ( m_CriteriaSaved != null && m_CriteriaSaved.TableName != newTableName )
            {
                EntityManager.Delete(m_CriteriaSaved);
                m_CriteriaSaved = null;
            }
        }

		/// <summary>
		/// Gets or sets the default report link.
		/// </summary>
		/// <value>
		/// The default report link.
		/// </value>
		[PromptLink]
		public ReportTemplate DefaultReportLink
		{
			get
			{
				return (ReportTemplate)EntityManager.SelectLatestVersion(ReportTemplateBase.EntityName, DefaultReport);
			}
			set
			{
				DefaultReport = IsValid(value) ? ((IEntity)value).GetString(ReportTemplatePropertyNames.Identity) : string.Empty;
				Library.Task.StateModified();
			}
		}
		#endregion

		#region Saving

		/// <summary>
		/// Called when the entity has been included in a transaction using an Transaction.Add() call.
		/// </summary>
		protected override void OnEnterTransaction()
		{
			// As the created entity is not a child and is just a linked entity
			// we need to add the entity to the transaction so it saves.
			if (m_CriteriaSaved != null)
			{
				EntityManager.Transaction.Add(m_CriteriaSaved);
			}

			base.OnEnterTransaction();
		}

		/// <summary>
		/// Called when the entity has been removed from a transaction using an Transaction.Remove() call.
		/// </summary>
		protected override void OnLeaveTransaction()
		{
			if (m_CriteriaSaved != null)
			{
				EntityManager.Transaction.Remove(m_CriteriaSaved);
			}

			base.OnLeaveTransaction();
		}

		#endregion

		#region Custom Menus

		/// <summary>
		/// Defines whether a menu item is included in the context menu.
		/// </summary>
		/// <param name="menuProc">The menu proc.</param>
		/// <param name="items">The items.</param>
		/// <param name="folderPath">The folder path.</param>
		/// <param name="groupValue">The group value.</param>
		/// <returns>
		/// 	<c>true</c> to include the item in the menu; otherwise, <c>false</c>.
		/// </returns>
		public override bool IncludeMenuItem(int menuProc, ICollection<IExtendedObject> items, string folderPath, object groupValue)
		{
			if (TableName == TableNames.Workflow)
			{
				if (!Workflow.IncludeMenuItem(menuProc, items, folderPath, groupValue)) return false;
			}

			return base.IncludeMenuItem(menuProc, items, folderPath, groupValue);
		}

		#endregion

		#region IImportableEntity Implementation

		/// <summary>
		/// Validates the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="primitiveEntities">The primitive entities.</param>
		/// <returns></returns>
		public ImportValidationResult CheckImportValidity(IEntity entity, List<ExportDataEntity> primitiveEntities)
		{
			var helper = new ExplorerFolderImportHelper(EntityManager, Library);	
			return helper.CheckImportValidity(entity, primitiveEntities);
		}


		/// <summary>
		/// Imports the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="result">The result.</param>
		/// <returns></returns>
		public ImportCommitResult Import(IEntity entity, ImportValidationResult result)
		{
			((ExplorerFolder) entity).CriteriaSavedIdentity = "";
			((ExplorerFolder) entity).CriteriaSaved =null ;
			((ExplorerFolder) entity).ColumnUsers = null;


			var helper = new ExplorerFolderImportHelper(EntityManager, Library);
			return helper.Import(entity, result);
		}

		/// <summary>
		/// Export preprocessing
		/// </summary>
		/// <param name="entity"></param>
		public void ExportPreprocess(IEntity entity)
		{
			var folder = entity as ExplorerFolder;
			if (folder != null)
			{
				foreach (ExplorerRmb explorerRmb in folder.Rmbs)
				{
					explorerRmb.Using += string.Format(@"<rmbNum>{0}</rmbNum>", explorerRmb.OrderNumber.ToInt32(CultureInfo.CurrentCulture.NumberFormat));
				}
			}
		}

		#endregion
	}
}
