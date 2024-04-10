using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the MLP_LIMIT entity.
	/// </summary> 
	[SampleManagerEntity(MlpLimit.EntityName)]
	public class MlpLimit : BaseEntity
	{
		#region Public Constants

		/// <summary>
		/// Mlp Limit Virtual Entity Name.
		/// </summary>
		public const string EntityName = "MLP_LIMIT";

		#endregion
	}
}
