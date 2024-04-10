using System;
using System.Collections.Generic;
using System.Linq;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.ImportExport;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.ObjectModel.Import_Helpers
{
	/// <summary>
	/// Base import helper
	/// </summary>
	public class BaseImportHelper
	{
		/// <summary>
		/// The m_ entity manager
		/// </summary>
		protected readonly IEntityManager m_EntityManager;
		/// <summary>
		/// The m_ library
		/// </summary>
		protected readonly StandardLibrary m_Library;

		/// <summary>
		/// Users override privileges on non modifiable data 
		/// </summary>
		protected readonly bool m_OverrideNonModifiablePrivilege;

		/// <summary>
		/// Gets or sets the ignore entity fields.
		/// </summary>
		/// <value>
		/// The ignore entity fields.
		/// </value>
		public List<string> IgnoreEntityFields { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseImportHelper" /> class.
		/// </summary>
		/// <param name="entityManager">The entity manager.</param>
		/// <param name="library">The library.</param>
		public BaseImportHelper(IEntityManager entityManager,StandardLibrary library)
		{

			m_EntityManager = entityManager;
			m_Library = library;

			var securityService = (ISecurityService)m_Library.GetService(typeof(ISecurityService));
			m_OverrideNonModifiablePrivilege= securityService.CheckPrivilege(SMPrivilege.AccessNoneModifiable);

			IgnoreEntityFields = new List<string>();
			InitializeHelper();
		}


		/// <summary>
		/// Initializes the helper.
		/// </summary>
		public virtual void InitializeHelper()
		{
			AddIgnoreField("REMOVEFLAG");
			AddIgnoreField("MODIFIABLE");
		}

		/// <summary>
		/// Checks the import validity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="primitiveEntities">The primitive entities.</param>
		/// <returns></returns>
		public virtual ImportValidationResult CheckImportValidity(IEntity entity,List<ExportDataEntity> primitiveEntities)
		{
			var result = new ImportValidationResult(entity);


			result.Errors = CrossCheckEntityLinks(entity, primitiveEntities);

			if (result.Errors.Count>0)
			{
				result.Result = ImportValidationResult.ValidityResult.Error;
			}
			else
			{
				//check if entity exists
				var existingEntity = m_EntityManager.Select(entity.EntityType, entity.Identity);
				if (existingEntity != null)
				{

					if (!m_OverrideNonModifiablePrivilege && !existingEntity.IsModifiable())
					{
						result.Result = ImportValidationResult.ValidityResult.Error;
						result.Errors.Add(string.Format(m_Library.Message.GetMessage("LaboratoryMessages", "ImportNonModifiableOverwriteNoPriviledge"),entity.EntityType,entity.Identity));
						return result;
					}

					//exists
					result.Result = ImportValidationResult.ValidityResult.Warning;
					result.AvailableActions.Add(ImportValidationResult.ImportActions.Overwrite);
					result.AvailableActions.Add(ImportValidationResult.ImportActions.Skip);
					result.DefaultAction = ImportValidationResult.ImportActions.Skip;
					result.AlreadyExists = true;
				}
				else
				{
					result.Result = ImportValidationResult.ValidityResult.Ok;
					result.AvailableActions.Add(ImportValidationResult.ImportActions.Add);
					result.AvailableActions.Add(ImportValidationResult.ImportActions.Skip);
					result.DefaultAction = ImportValidationResult.ImportActions.Add; 
				}
			
			}
			return result;
		}

		/// <summary>
		/// Crosses the check entity links.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="primitiveEntities">The primitive entities.</param>
		/// <returns></returns>
		public virtual List<string> CrossCheckEntityLinks(IEntity entity, List<ExportDataEntity> primitiveEntities)
		{
			var errorList = new List<string>();
			try
			{
				var exemptValues = new[] {"TRUE", "FALSE","UNITS"};

				foreach (var exportDataEntity in primitiveEntities)
				{
					if (exportDataEntity.Identity.ToString() != entity.Identity.ToString()) continue;
					CheckFields(entity, exportDataEntity, exemptValues, errorList,false);
				}
			}
			catch
				(Exception ex)
			{
				errorList.Add(ex.Message);
			}

			return errorList;
		}

		/// <summary>
		/// Checks the fields.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="exportDataEntity">The export data entity.</param>
		/// <param name="exemptValues">The exempt values.</param>
		/// <param name="errorList">The error list.</param>
		/// <param name="isChild">if set to <c>true</c> [is child].</param>
		/// <exception cref="System.Exception"></exception>
		private void CheckFields(IEntity entity, ExportDataEntity exportDataEntity, string[] exemptValues, List<string> errorList,bool isChild)
		{
			foreach (var field in exportDataEntity.Fields)
			{
				
				if (IgnoreEntityFields.Contains(field.Key.ToUpper()))
				{
					continue;
				}

				if (field.Value != null)
				{
					if (exemptValues.Contains(field.Value.ToString()) || string.IsNullOrEmpty(field.Value.ToString()))
					{
						continue;
					}
					if (entity.GetString(field.Key) != field.Value.ToString() && !isChild)
					{
						errorList.Add(string.Format(m_Library.Message.GetMessage("LaboratoryMessages", "ImportErrorField"), field.Key, entity.Name, field.Value));
						continue;
					}

					var schemaField = Schema.Current.Tables[exportDataEntity.EntityType].Fields[field.Key];

					if (schemaField.LinkField != null && schemaField.LinkTable.KeyFields.Count > 0 && !schemaField.LinksToParent && !schemaField.IsKey && !schemaField.IsIdentity && !exemptValues.Contains(schemaField.Name.ToUpper()))
					{
						try
						{
							var fEntity = m_EntityManager.Select(schemaField.LinkTable.Name, field.Value.ToString());
							if (fEntity == null)
							{
								throw new Exception();
							}

							if (fEntity.IsRemoved() || fEntity.IsDeleted())
							{
								throw new Exception();
							}
						}
						catch
						{
							errorList.Add(string.Format(m_Library.Message.GetMessage("LaboratoryMessages", "ImportErrorLinkedField"), field.Key, exportDataEntity.EntityType,exportDataEntity.Identity, field.Value));
						}
					}
				}

				
			}

			foreach (var collectionItem in exportDataEntity.Collections.SelectMany(collection => collection.Value))
			{
				CheckFields(entity, collectionItem, exemptValues, errorList, true);
			}
		}

		/// <summary>
		/// Adds the ignore field.
		/// </summary>
		/// <param name="fieldName">Name of the field.</param>
		public void AddIgnoreField(string fieldName)
		{
			IgnoreEntityFields.Add(fieldName.ToUpper());
		}

		/// <summary>
		/// Imports the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="result">The result.</param>
		/// <returns></returns>
		public virtual ImportCommitResult Import(IEntity entity, ImportValidationResult result)
		{
			try
			{
				switch (result.SelectedImportAction)
				{
					case ImportValidationResult.ImportActions.Add:
						m_EntityManager.Transaction.Add(entity);
						break;

					case ImportValidationResult.ImportActions.Overwrite:
						var existing = m_EntityManager.Select(entity.EntityType, entity.Identity);
						m_EntityManager.Delete(existing);
						m_EntityManager.Commit();
						goto case ImportValidationResult.ImportActions.Add;
						
					case ImportValidationResult.ImportActions.New_Version:
						goto case ImportValidationResult.ImportActions.Add;
						
					case ImportValidationResult.ImportActions.New_Folder_Number:
						goto case ImportValidationResult.ImportActions.Add;

					case ImportValidationResult.ImportActions.New_Identity:
						while (true)
						{
							var dialogMessage = new string(' ', 7)+m_Library.Message.GetMessage("LaboratoryMessages", "Import_EnterNewIdentity");
							var iconService = (IIconService) m_Library.GetService(typeof (IIconService));

							var image = iconService.LoadImage(new IconName(entity.DefaultFolderIcon), 16);

							var newIdentity = m_Library.Utils.PromptForGenericType(typeof (String),
								dialogMessage, "",
								new Dictionary<string, object>()
								{
									{"Identity",true},
									{"InitialText",entity.Identity.ToString()},
									{"Image",image}
								});

							if (newIdentity == null) throw new Exception(m_Library.Message.GetMessage("LaboratoryMessages", "ImportCancelled"));

							var query = m_EntityManager.CreateQuery(entity.EntityType);
							query.AddEquals("IDENTITY",newIdentity.ToString());
							var checkEntity = m_EntityManager.Select(query);
							if (checkEntity != null && checkEntity.ActiveItems.Count>0)
							{
								m_Library.Utils.FlashMessage(m_Library.Message.GetMessage("LaboratoryMessages", "Import_IdentityAlreadyExists"), "");
							}
							else
							{
								entity.SetField("IDENTITY", newIdentity);
								goto case ImportValidationResult.ImportActions.Add; 
							}

						}


					case ImportValidationResult.ImportActions.Skip:
						return new ImportCommitResult(m_Library.Message.GetMessage("LaboratoryMessages", "ImportSkipped"), ImportCommitResult.ImportCommitResultState.Skipped);
				}

				m_EntityManager.Commit();

				return new ImportCommitResult(m_Library.Message.GetMessage("LaboratoryMessages", "ImportSuccess"), ImportCommitResult.ImportCommitResultState.Ok);
			}
			catch(Exception ex)
			{
				return new ImportCommitResult(string.Format("{0}: {1}", m_Library.Message.GetMessage("LaboratoryMessages", "ImportError"), ex.Message), ImportCommitResult.ImportCommitResultState.Error);
			}


		}
	}
}
