using System;
using System.Collections.Generic;
using System.Linq;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.ObjectModel.Import_Helpers
{
	/// <summary>
	///     Various utils for import export
	/// </summary>
	public class ImportExportUtilities
	{
		/// <summary>
		///     Gets the importable entitiy types.
		/// </summary>
		/// <value>
		///     The importable entitiy types.
		/// </value>
		public static IDictionary<string, Type> ImportableEntityTypes
		{
			get
			{
				var entities = Schema.Current.EntityTypes;
				var importableTypes = new SortedDictionary<string, Type>();
				foreach (var entity in entities)
				{
					var type = typeof (IImportableEntity);
					if (entity.Value.GetInterfaces().Any(@interface => @interface == type))
					{
						importableTypes.Add(entity.Key, entity.Value);
					}
				}
				return importableTypes;
			}
		}
	}
}