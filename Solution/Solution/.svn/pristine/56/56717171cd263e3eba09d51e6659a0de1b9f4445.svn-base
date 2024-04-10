using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.ImportExport;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.ObjectModel.Import_Helpers
{
	/// <summary>
	/// Explorer Folder Import Helper
	/// </summary>
	public class ExplorerFolderImportHelper:BaseImportHelper
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ExplorerFolderImportHelper"/> class.
		/// </summary>
		/// <param name="entityManager">The entity manager.</param>
		/// <param name="library">The library.</param>
		public ExplorerFolderImportHelper(IEntityManager entityManager, StandardLibrary library) : base(entityManager, library)
		{
		}

		/// <summary>
		/// Initializes the helper.
		/// </summary>
		public override void InitializeHelper()
		{
			base.InitializeHelper();
			IgnoreEntityFields.Add("PARENT_NUMBER");
			IgnoreEntityFields.Add("TABLE_NAME");
		}

		/// <summary>
		/// Checks the import validity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="primitiveEntities">The primitive entities.</param>
		/// <returns></returns>
		public override ImportValidationResult CheckImportValidity(IEntity entity, List<ExportDataEntity> primitiveEntities)
		{
			var result = new ImportValidationResult(entity);


			result.Errors = CrossCheckEntityLinks(entity, primitiveEntities);

			if (result.Errors.Count > 0)
			{
				result.Result = ImportValidationResult.ValidityResult.Error;
			}
			else
			{
				//check if entity exists

				var explorerFolder = entity as ExplorerFolder;

				var query = m_EntityManager.CreateQuery("EXPLORER_FOLDER");
				query.AddLike("CABINET", explorerFolder.Cabinet);
				query.AddLike("NAME", explorerFolder.Name);

				var existingEntities = m_EntityManager.Select(query);

				if (existingEntities != null && existingEntities.Count>0)
				{
					var existingEntity = existingEntities[0];

					if (!m_OverrideNonModifiablePrivilege && !existingEntity.IsModifiable())
					{
						result.Result = ImportValidationResult.ValidityResult.Error;
						result.Errors.Add(string.Format(m_Library.Message.GetMessage("LaboratoryMessages", "ImportNonModifiableOverwriteNoPriviledge"), entity.EntityType, entity.Identity));
						return result;
					}

					//exists
					result.Result = ImportValidationResult.ValidityResult.Warning;
					result.AvailableActions.Add(ImportValidationResult.ImportActions.Overwrite);

					if (explorerFolder.Cabinet.Name != "TABLE_DETAILS")
					{
						result.AvailableActions.Add(ImportValidationResult.ImportActions.New_Folder_Number);
					}

					result.AvailableActions.Add(ImportValidationResult.ImportActions.Skip);
					result.DefaultAction = ImportValidationResult.ImportActions.Skip;
					result.DisplayName = entity.Name;
					result.AlreadyExists = true;
				}
				else
				{
					result.Result = ImportValidationResult.ValidityResult.Ok;
					result.AvailableActions.Add(ImportValidationResult.ImportActions.Add);
					result.AvailableActions.Add(ImportValidationResult.ImportActions.Skip);
					result.DisplayName = entity.Name;
					result.DefaultAction = ImportValidationResult.ImportActions.Add;
				}

			}
			return result;
		}

		/// <summary>
		/// Imports the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="result">The result.</param>
		/// <returns></returns>
		public override ImportCommitResult Import(IEntity entity, ImportValidationResult result)
		{
		
			if (result.SelectedImportAction == ImportValidationResult.ImportActions.Overwrite)
			{
				var query = m_EntityManager.CreateQuery("EXPLORER_FOLDER");

				query.AddLike("CABINET", ((ExplorerFolder)entity).Cabinet);
				query.AddLike("NAME", ((ExplorerFolder)entity).Name);

				var existingFolder = m_EntityManager.Select(query);
				m_EntityManager.Delete(existingFolder[0]);
				m_EntityManager.Commit();
				result.SelectedImportAction = ImportValidationResult.ImportActions.Add;
			}

			if (result.SelectedImportAction == ImportValidationResult.ImportActions.Add || result.SelectedImportAction == ImportValidationResult.ImportActions.New_Folder_Number)
			{
				var folderEntity = entity as ExplorerFolder;
				var query = m_EntityManager.CreateQuery("EXPLORER_FOLDER");
				query.AddEquals("CABINET", folderEntity.Cabinet);
				var max = m_EntityManager.SelectMax(query, "FOLDER_NUMBER");
				var number = new PackedDecimal(max ?? 1);
				if (string.IsNullOrEmpty(number.String))
				{
					number = new PackedDecimal(1);
				}
				else
				{
					number.Value += 1;
				}

				folderEntity.FolderNumber = folderEntity.OrderNumber = number;
				
		
				entity = folderEntity;

				if (result.SelectedImportAction == ImportValidationResult.ImportActions.New_Folder_Number)
				{
					MakeFolderNameUnique(entity as ExplorerFolder);
				}
			}

			
			var rVal = base.Import(entity, result);
			ConvertFolderLinks(entity);
			m_EntityManager.Commit();

			return rVal;

		}

		/// <summary>
		/// Converts the folder links.
		/// </summary>
		/// <param name="entity">The entity.</param>
		private void ConvertFolderLinks(IEntity entity)
		{
			var folder = entity as ExplorerFolder;
			if (folder != null)
			{
				var oldToNew = new Dictionary<int, int>();	//<old,new>

		
				
				foreach (ExplorerRmb rmb in folder.Rmbs)
				{
					var match = Regex.Match(rmb.Using, @"<rmbNum>(.*)<\/rmbNum>");

					var oldNum = int.Parse(match.Groups[1].ToString());
					oldToNew.Add(oldNum, rmb.OrderNumber.Value);
				}

				//alloc new parent links
				foreach (ExplorerRmb rmb in folder.Rmbs)
				{
					if (rmb.ParentNumber != 0)
					{
						rmb.ParentNumber = oldToNew[(int)rmb.ParentNumber];
					}
					rmb.Using = Regex.Replace(rmb.Using, @"<rmbNum>.*<\/rmbNum>", "");
				}
				
				
				//foreach (ExplorerRmb rmb in folder.Rmbs)
				//{
				//	var match = Regex.Match(rmb.Using, @"<rmbNum>(.*)<\/rmbNum>");

				//	var oldNum = int.Parse(match.Groups[1].ToString());

				//	rmb.RmbNumber = rmb.OrderNumber = oldNum;
				//	rmb.Using = Regex.Replace(rmb.Using, @"<rmbNum>.*<\/rmbNum>", "");
				//}
			}
			
			m_EntityManager.Transaction.Add(folder);
			
		}

		/// <summary>
		/// Makes the folder name unique.
		/// </summary>
		/// <param name="folder">The folder.</param>
		private void MakeFolderNameUnique(ExplorerFolder folder)
		{
			var originalName = folder.Name;
			var newName = folder.Name;
			var count = 2;
			const string appendSuffix = " ({0})";

			while (true)
			{
				folder.ExplorerFolderName = newName;

				var query = m_EntityManager.CreateQuery("EXPLORER_FOLDER");
				query.AddLike("NAME", folder.Name);
				var existingFolder = m_EntityManager.Select(query);

				if (existingFolder.Count != 0) 
				{
					newName = ((originalName.Length == 100) ? originalName.Substring(0, 100 - appendSuffix.Length) : originalName) + string.Format(appendSuffix, count++);
				}
				else
				{
					break;
				}
			}
		}
	}
}
