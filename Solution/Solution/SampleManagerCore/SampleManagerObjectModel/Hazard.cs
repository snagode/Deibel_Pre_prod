using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.ImportExport;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.ImportExport;
using Thermo.SampleManager.ObjectModel.Import_Helpers;


namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the HAZARD entity.
	/// </summary>
	[SampleManagerEntity(HazardBase.EntityName)]
	public class Hazard : HazardBase
	{
		/* for future reference, simple IImportableEntity implementation
		 * above would read: public class Hazard : HazardBase,IImportableEntity
		 
		#region IImportableEntity Implementation

		/// <summary>
		/// Validates the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="primitiveEntities">The primitive entities.</param>
		/// <returns></returns>
		public ImportValidationResult CheckImportValidity(IEntity entity, List<ExportDataEntity> primitiveEntities)
		{
			var helper = new BaseImportHelper(EntityManager, Library);
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
			var helper = new BaseImportHelper(EntityManager, Library);
			return helper.Import(entity, result);
		}

		#endregion

		 
		*/
	}
}
