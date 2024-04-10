using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Thermo.Framework.Utilities;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Aux Function Information
	/// </summary>
	[DataContract(Name="auxiliary")]
	public class FunctionAuxiliary : Function
	{
		#region Constants

		/// <summary>
		/// Auxiliary Functionality
		/// </summary>
		public const string FeatureName = "Auxiliary";

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the name of the table.
		/// </summary>
		/// <value>
		/// The name of the table.
		/// </value>
		[DataMember(Name = "tableName")]
		public string TableName { get; set; }

		/// <summary>
		/// Gets or sets the prompts.
		/// </summary>
		/// <value>
		/// The prompts.
		/// </value>
		[DataMember(Name = "prompts")]
		public List<Prompt> Prompts { get; set; }

		/// <summary>
		/// Gets or sets the execute URI.
		/// </summary>
		/// <value>
		/// The execute URI.
		/// </value>
		[DataMember(Name="executeUri")]
		public Uri ExecuteUri { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionAuxiliary"/> class.
		/// </summary>
		public FunctionAuxiliary()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionAuxiliary"/> class.
		/// </summary>
		/// <param name="rmb">The RMB.</param>
		public FunctionAuxiliary(ExplorerRmb rmb) : this(rmb.Menuproc)
		{
			Feature = FeatureName;
			SearchesUri = MakeLink("/mobile/searches/{0}/{1}", rmb.Cabinet, Name);
		}

		/// <summary>
		/// Loads the menu.
		/// </summary>
		/// <param name="menu">The menu.</param>
		public FunctionAuxiliary(MasterMenuBase menu) : base(menu)
		{
			var auxReport = new Identity(menu.TableName, menu.AuxReport);
			var aux = (ExplorerAux) menu.EntityManager.Select(ExplorerAuxBase.EntityName, auxReport);
			if (!BaseEntity.IsValid(aux)) return;

			ExecuteUri = MakeLink("/mobile/auxiliary/{0}", menu.ProcedureNum);

			TableName = menu.TableName;
			Prompts = new List<Prompt>();

			foreach (ExplorerAuxFields field in aux.AuxFields)
			{
				if (!field.Visible) continue;
				if (field.Type == "ROUTINE") continue;

				var prompt = Prompt.Create(menu.Library, field.TableName, field.FieldName, field.DefaultValue);
				if (prompt == null) continue;

				prompt.Label = GetLocalizedString(menu.Library, field.ExplorerAuxFieldsName);
				prompt.Id = field.FieldName;

				prompt.Tooltip = GetLocalizedFieldName(menu.Library, field.TableName, field.FieldName);
				prompt.Mandatory = true;
				prompt.ReadOnly = (field.Type != "FORMAT");

				Prompts.Add(prompt);
			}

			// Clear Empty Prompts

			if (Prompts.Count == 0)
			{
				Prompts = null;
			}
		}

		#endregion

		#region Feature Support

		/// <summary>
		/// Determines whether the specified RMB is this function.
		/// </summary>
		/// <param name="rmb">The RMB.</param>
		/// <returns></returns>
		public static bool IsFunction(ExplorerRmb rmb)
		{
			if (rmb.Menuproc == null) return false;
			if (rmb.Menuproc.Type == null) return false;
			if (rmb.Menuproc.ProcedureNum == FunctionResultEntry.FunctionResultEntryNumber) return false;
			if (rmb.Menuproc.ProcedureNum == FunctionResultEntryByTest.FunctionResultEntryByTestNumber) return false;

			if (rmb.Menuproc.Type.IsPhrase(PhraseMenuType.PhraseIdAUXILIARY))
			{
				return true;
			}

			return false;
		}

		#endregion
	}
}
