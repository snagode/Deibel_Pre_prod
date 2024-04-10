using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.ImportExport;
using Thermo.SampleManager.Library.ImportExport;

namespace Thermo.SampleManager.ObjectModel.Import_Helpers
{
	/// <summary>
	/// Interface for importable entities
	/// </summary>
	public interface IImportableEntity
	{

		/// <summary>
		/// Preprocess export
		/// </summary>
		void ExportPreprocess(IEntity entity);

		/// <summary>
		/// Validates the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="primitiveEntities">The primitive entities.</param>
		/// <returns></returns>
		ImportValidationResult CheckImportValidity(IEntity entity,List<ExportDataEntity> primitiveEntities);

		/// <summary>
		/// Imports the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="result">The result.</param>
		/// <returns></returns>
		ImportCommitResult Import(IEntity entity,ImportValidationResult result);
	}

}
