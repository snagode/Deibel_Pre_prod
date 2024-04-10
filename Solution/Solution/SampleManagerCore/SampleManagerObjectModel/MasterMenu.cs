using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the MASTER_MENU entity.
	/// </summary>
	[SampleManagerEntity(MasterMenuBase.EntityName)]
	public class MasterMenu : MasterMenuBase
	{
		#region Overrides

		/// <summary>
		/// Property : Name
		/// </summary>
		/// <value></value>
		public override string Name
		{
			get { return ProcedureNum.ToString(); }
		}

		/// <summary>
		/// Gets the menu usages.
		/// </summary>
		/// <value>
		/// The menu usages.
		/// </value>
		[PromptCollection(Barmenu.EntityName, true)]
		public IEntityCollection MenuUsages
		{
			get
			{
				IQuery query = EntityManager.CreateQuery(Barmenu.EntityName);
				query.AddEquals(BarmenuPropertyNames.MenuNumber, ProcedureNum);
				return EntityManager.Select(query);
			}
		}

		#endregion
	}
}
