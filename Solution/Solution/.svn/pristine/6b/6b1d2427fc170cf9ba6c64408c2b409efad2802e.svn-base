using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Schedule Base Support
	/// </summary>
	public class SchedulePointEventBase : Dictionary<string, object>
	{
		#region Field Update

		/// <summary>
		/// Updates the fields.
		/// </summary>
		protected void UpdateFields(BaseEntity entity, string tableName)
		{
			ISchemaTable master = entity.FindSchemaTable();
			ISchemaTable slave = entity.Library.Schema.Tables[tableName];

			foreach (ISchemaField field in master.Fields)
			{
				if (slave.Fields.Contains(field.Name))
				{
					Add(field.Name, ((IEntity) entity).Get(field.Name));
				}
			}
		}

		#endregion
	}
}