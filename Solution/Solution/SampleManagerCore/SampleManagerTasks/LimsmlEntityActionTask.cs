using System.Collections.Generic;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// LIMSML Entity Action Task
	/// </summary>
	[SampleManagerTask("LimsmlEntityActionTask", "LABTABLE", "LIMSML_ACTION")]
	public class LimsmlEntityActionTask : GenericLabtableTask
	{
		#region Member Variables

		private FormLimsmlEntityAction m_LimsmlForm;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_LimsmlForm = (FormLimsmlEntityAction)MainForm;

			List<string> tableNames = new List<string>();

			tableNames.Add("SYSTEM");
			tableNames.Add("GENERIC");

			foreach (SchemaTable table in Schema.Current.Tables)
			{
				string tableName = table.Name.ToUpperInvariant();
				if (tableNames.Contains(tableName)) return;
				tableNames.Add(tableName);
			}

			tableNames.Sort();
			m_LimsmlForm.BrowseEntityTypes.Republish(tableNames);
		}

		#endregion
	}
}