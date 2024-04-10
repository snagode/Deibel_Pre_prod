using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.ImportExport;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.ImportExport;

namespace Thermo.SampleManager.ObjectModel.Import_Helpers
{
	class VersionedAnalysisImportHelper:BaseImportHelper
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="VersionedAnalysisImportHelper"/> class.
		/// </summary>
		/// <param name="entityManager">The entity manager.</param>
		/// <param name="library">The library.</param>
		public VersionedAnalysisImportHelper(IEntityManager entityManager, StandardLibrary library) : base(entityManager, library)
		{
		}

		/// <summary>
		/// Checks the import validity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="primitiveEntities">The primitive entities.</param>
		/// <returns></returns>
		public override ImportValidationResult CheckImportValidity(IEntity entity, List<ExportDataEntity> primitiveEntities)
		{
			var result =base.CheckImportValidity(entity, primitiveEntities);
			if (result.AlreadyExists) result.AvailableActions.Add(ImportValidationResult.ImportActions.New_Version);
			return result;
		}

		public override ImportCommitResult Import(IEntity entity, ImportValidationResult result)
		{
			if (result.SelectedImportAction == ImportValidationResult.ImportActions.New_Version)
			{
				var analysisEntity = entity as VersionedAnalysis;
				entity = analysisEntity.CreateNewVersion();
			}
			return base.Import(entity, result);
		}

	}
}
