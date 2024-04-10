using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	///     Defines extended business logic and manages access to the EXPLORER_CABINET entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class ExplorerCabinet : ExplorerCabinetBase
	{
		/// <summary>
		///     Flag for if the cabinet is a copy
		/// </summary>
		private bool IsCopy { get; set; }

		/// <summary>
		///     Called when cabinet is copied
		/// </summary>
		/// <param name="sourceEntity"></param>
		protected override void OnEntityCopied(IEntity sourceEntity)
		{
			IsCopy = true;
			base.OnEntityCopied(sourceEntity);
		}

		/// <summary>
		/// Perform post creation processing.
		/// </summary>
		protected override void OnEntityCreated()
		{
			base.OnEntityCreated();

			var max = EntityManager.SelectMax(TableNames.ExplorerCabinet, "ORDER_NUMBER");
			var number = new PackedDecimal(max ?? 1);

			if (string.IsNullOrEmpty(number.String))
				number = new PackedDecimal(1);
			else
				number.Value += 1;

			OrderNumber = number;
		}

		/// <summary>
		///     Precommitting explorer cabinet entity
		/// </summary>
		protected override void OnPreCommit()
		{
			if (IsCopy)
			{
				//create a copy of each folder criteria
				foreach (ExplorerFolder folder in Folders)
				{
					//workaround: for some reason the copying of criteria creates a corrupt identity: following creates proper identity
					var cEntity = folder.CriteriaSaved.CreateCopy();

					var newFolderName = folder.ExplorerPrefix + Identity;

					var newIdentity = string.Format("{0}_{1}_{2}", newFolderName, folder.FolderNumber.ToString().Trim(), newFolderName.Length).ToUpper();

					folder.CriteriaSaved = (CriteriaSaved)cEntity;
					folder.CriteriaSavedIdentity = folder.CriteriaSaved.Identity = newIdentity;
				}
			}

			base.OnPreCommit();
		}
	}
}