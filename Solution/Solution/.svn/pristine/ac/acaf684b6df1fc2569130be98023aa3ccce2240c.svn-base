using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.ServiceModel.Web;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;
using Thermo.SampleManager.WebApiTasks.Mobile.Data;
using Thermo.SM.LIMSML.Helper.Low;

namespace Thermo.SampleManager.WebApiTasks.Mobile
{
	/// <summary>
	///  Auxiliary Functions
	/// </summary>
	[SampleManagerWebApi("mobile.auxiliary")]
	[MobileFeature(FunctionAuxiliary.FeatureName)]
	public class AuxiliaryTask : WebApiLimsmlBaseTask
	{
		#region Properties

		/// <summary>
		/// Gets or sets the Explorer cache.
		/// </summary>
		/// <value>
		/// The cache.
		/// </value>
		protected IExplorerCacheService ExplorerCache { get; set; }

		#endregion

		#region Overrides

		/// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			ExplorerCache = Library.GetService<IExplorerCacheService>();
			base.SetupTask();
		}

		#endregion

		#region Auxiliary Function

		/// <summary>
		/// All available auxiliary functions
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/auxiliary", Method = "GET")]
		[Description("List of all supported auxiliary functions")]
		public List<FunctionAuxiliary> Auxiliary()
		{
			var functions = new List<FunctionAuxiliary>();

			var query = EntityManager.CreateQuery(MasterMenuBase.EntityName);
			query.AddEquals(MasterMenuPropertyNames.Type, PhraseMenuType.PhraseIdAUXILIARY);
			query.AddDefaultOrder();
			query.HideRemoved();

			var procedures = EntityManager.Select(query);

			foreach (MasterMenuBase menu in procedures)
			{
				if (!ValidateAuxiliary(menu.ProcedureNum)) continue;
				var function = new FunctionAuxiliary(menu);
				functions.Add(function);
			}

			if (functions.Count == 0) return null;
			return functions;
		}

		/// <summary>
		/// Auxiliary Function Information
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/auxiliary/{function}", Method = "GET")]
		[Description("Auxiliary Function Information")]
		public FunctionAuxiliary AuxiliarySpecific(string function)
		{
			if (!ValidateAuxiliary(function)) return null;
			var menu = (MasterMenuBase)EntityManager.Select(MasterMenuBase.EntityName, new Identity(function));
			if (!BaseEntity.IsValid(menu)) return null;
			return new FunctionAuxiliary(menu);
		}

		#endregion

		#region Aux Processing

		/// <summary>
		/// Auxiliary Function Execution
		/// </summary>
		/// <param name="function">The function.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/auxiliary/{function}", Method = "POST")]
		[Description("Auxiliary Function Execution")]
		public List<Uri> AuxiliaryExecute(string function, AuxiliaryValues values)
		{
			var menu = (MasterMenuBase)EntityManager.Select(MasterMenuBase.EntityName, new Identity(function));
			if (!BaseEntity.IsValid(menu)) return null;

			if (!ValidateAuxiliary(function)) return null;
			SetMenuSecurity(function, checkSignature: true);

			// Build up LIMSML request

			var limsml = LimsmlGet();
			var trans = LimsmlGetTransaction(limsml);
			var entity = trans.AddEntity(ExplorerAuxBase.StructureTableName);
			var action = entity.AddAction("EXECUTE");
			action.AddParameter("TABLE", menu.TableName);
			action.AddParameter("IDENTITY", menu.AuxReport);

			foreach (KeyValuePair<string,object> prompt in values.Values)
			{
				if (prompt.Value is DateTime)
				{
					entity.AddField(prompt.Key, (DateTime) prompt.Value, Direction.In);
				}
				else if (prompt.Value is TimeSpan)
				{
					entity.AddField(prompt.Key, (TimeSpan)prompt.Value, Direction.In);
				}
				else
				{
					Field field = new Field(prompt.Key);
					field.Value = prompt.Value.ToString();
					field.Direction = Direction.In;
					entity.Fields.Add(field);
				}
			}

			foreach (var item in values.Selected)
			{
				var child = entity.AddChild(menu.TableName);
				child.AddField("_KEY0", item, Direction.InOut);
			}
			
			// Process 

			var response = LimsmlProcess(limsml);

			// Check everything went well and return links

			if (LimsmlCheckOk(response, HttpStatusCode.Conflict))
			{
				var links = new List<Uri>();

				foreach (var item in values.Selected)
				{
					links.Add(Data.Entity.MakeEntityLink(menu.TableName, item));
				}

				return links;
			}

			return null;
		}

		#endregion

		#region Utilities

		/// <summary>
		/// Validates the auxiliary menu item;
		/// </summary>
		/// <param name="menuproc">The menuproc.</param>
		/// <returns></returns>
		private bool ValidateAuxiliary(int menuproc)
		{
			if (menuproc == FunctionResultEntry.FunctionResultEntryNumber) return false;
			return CheckMenuSecurity(menuproc);
		}

		/// <summary>
		/// Validates the auxiliary menu item;
		/// </summary>
		/// <param name="function">The function.</param>
		/// <returns></returns>
		private bool ValidateAuxiliary(string function)
		{
			int menuproc;
			if (!int.TryParse(function, NumberStyles.Any, CultureInfo.InvariantCulture, out menuproc)) return false;
			return ValidateAuxiliary(menuproc);
		}

		#endregion

		#region Function Support

		/// <summary>
		/// Gets the function.
		/// </summary>
		/// <param name="rmb">The RMB.</param>
		/// <returns></returns>
		[MobileFunction]
		public static Function GetFunction(ExplorerRmb rmb)
		{
			if (!FunctionAuxiliary.IsFunction(rmb)) return null;
			return new FunctionAuxiliary(rmb);
		}

		#endregion
	}
}

