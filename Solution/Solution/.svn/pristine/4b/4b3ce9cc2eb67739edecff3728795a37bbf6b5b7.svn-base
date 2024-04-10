using System.Runtime.Serialization;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Search Column
	/// </summary>
	[DataContract(Name="searchColumn")]
	public class SearchColumn : Column
	{
		#region Utility Statics

		/// <summary>
		/// Builds the column.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <param name="node">The node.</param>
		/// <param name="explorerColumn">The explorer column.</param>
		/// <returns></returns>
		public static SearchColumn LoadColumn(StandardLibrary library, ExplorerDataNode node, ExplorerColumnInfo explorerColumn)
		{
			var col = new SearchColumn();

			col.Id = explorerColumn.FieldName.Trim('"');
			col.Name = GetLocalizedString(library, explorerColumn.Name);
			col.AllowSort = explorerColumn.AllowSort;
			col.Width = explorerColumn.Width;
			col.Hidden = explorerColumn.Hidden;

			string propertyName = EntityType.DeducePropertyName(node.TableName, explorerColumn.FieldName);
			col.SetDataType(node.TableName, propertyName);
			return col;
		}

		#endregion
	}
}
