using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.CommandLine;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks.BackgroundTasks
{
	/// <summary>
	/// Background task to update active flags
	/// </summary>
	[SampleManagerTask("ActiveRecord")]
	public class ActiveRecordTask : SampleManagerTask, IBackgroundTask
	{
		#region Constants

		private const string ApprovalSingleActiveVersion = "APPROVAL_SINGLE_ACTIVE_VERSION";

		#endregion

		#region Member Variables

		private StringBuilder m_Output = new StringBuilder();

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the output.
		/// </summary>
		/// <value>
		/// The output.
		/// </value>
		public StringBuilder Output
		{
			get { return m_Output; }
			set { m_Output = value; }
		}

		/// <summary>
		/// Gets or sets the criteria
		/// </summary>
		/// <value>The criteria.</value>
		[CommandLineSwitch("table", "Table to process, if left blank all appropriate tables will be processed", false)]
		public string Table { get; set; }

		#endregion

		#region Study Specific Statics

		internal static readonly string StudyTableName = "STB_STUDY";

		internal static readonly List<object> StudyStatusToActivate = new List<object> { "A", "C", "D", "F", "R", "S", "X" };
		internal static readonly List<object> StudyStatusToDeactivate = new List<object> { "V", "I" };

		#endregion

		#region IBackgroundTask Members

		/// <summary>
		/// Launches this background task.
		/// </summary>
		public void Launch()
		{
			OutputInfo(GetMessage("ActiveRecord_Starting"));

			// Process All Tables

			if (string.IsNullOrEmpty(Table))
			{
				ProcessAllTables();
			}
			else
			{
				// Process a specific table

				ProcessTable(Table);
			}

			OutputInfo(GetMessage("ActiveRecord_Finishing"));
		}

		/// <summary>
		/// Processes all tables.
		/// </summary>
		private void ProcessAllTables()
		{
			OutputInfo(GetMessage("ActiveRecord_ProcessingAll"));

			foreach (ISchemaTable table in Library.Schema.Tables)
			{
				if (!ActiveTable(table)) continue;
				ProcessTable(table);
			}
		}

		/// <summary>
		/// Process the table.
		/// </summary>
		/// <param name="table">The table.</param>
		private void ProcessTable(string table)
		{
			table = table.ToUpperInvariant();

			if (!Library.Schema.Tables.Contains(table))
			{
				throw new SampleManagerError(Library.Message.GetMessage("CommonMessages", "ErrorInvalidTableName", table));
			}

			ISchemaTable schemaTable = Library.Schema.Tables[table];
			ProcessTable(schemaTable);
		}

		/// <summary>
		/// Process the table.
		/// </summary>
		/// <param name="table">The table.</param>
		private void ProcessTable(ISchemaTable table)
		{
			CheckTable(table);

			if (SingleActiveVersion && (table.VersionField!=null))
			{
				// Should be active but isnt.

				OutputInfo(GetMessage("ActiveRecord_ProcessingInactive"), table.Name);

				foreach (IEntity entity in ShouldBeActiveSingleVersion(table))
				{
					Activate(entity, table);
				}

				// Is active and shouldn't be

				OutputInfo(GetMessage("ActiveRecord_ProcessingActive"), table.Name);

				foreach (IEntity entity in ShouldBeInactiveSingleVersion(table))
				{
					Deactivate(entity, table);
				}
			}
			else
			{
				// Should be active but isnt.

				OutputInfo(GetMessage("ActiveRecord_ProcessingInactive"), table.Name);

				foreach (IEntity entity in ShouldBeActive(table))
				{
					Activate(entity, table);
				}

				// Is active and shouldn't be

				OutputInfo(GetMessage("ActiveRecord_ProcessingActive"), table.Name);

				foreach (IEntity entity in ShouldBeInactive(table))
				{
					Deactivate(entity, table);
				}
			}

			EntityManager.Commit();
		}

		/// <summary>
		/// Activates the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="table">The table.</param>
		private void Activate(IEntity entity, ISchemaTable table)
		{
			NullableDateTime startDate = (NullableDateTime) entity.Get(table.ActiveStartDateField.Name);
			NullableDateTime endDate = (NullableDateTime) entity.Get(table.ActiveEndDateField.Name);

			if (table.ApprovalStatusField == null)
			{
				OutputInfo(GetMessage("ActiveRecord_Activating"), entity.Identity, startDate, endDate);
			}
			else
			{
				if (SingleActiveVersion)
				{
					if ( entity.GetString(table.ApprovalStatusField.Name) == "P")
						entity.Set(table.ApprovalStatusField.Name, "A");
				}

				OutputInfo(GetMessage("ActiveRecord_ActivatingApprStat"),
					entity.Identity, startDate, endDate, entity.Get(table.ApprovalStatusField.Name));
			}

			entity.Set(table.ActiveFlagField.Name, true);
			EntityManager.Transaction.Add(entity);
		}

		/// <summary>
		/// Deactivates the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="table">The table.</param>
		private void Deactivate(IEntity entity, ISchemaTable table)
		{
			NullableDateTime startDate = (NullableDateTime) entity.Get(table.ActiveStartDateField.Name);
			NullableDateTime endDate = (NullableDateTime) entity.Get(table.ActiveEndDateField.Name);

			if (table.ApprovalStatusField == null)
			{
				OutputInfo(GetMessage("ActiveRecord_Deactivating"), entity.Identity, startDate,
					endDate);
			}
			else
			{

				OutputInfo(GetMessage("ActiveRecord_DectivatingApprStat"), entity.Identity, startDate,
					endDate, entity.Get(table.ApprovalStatusField.Name));
			}

			entity.Set(table.ActiveFlagField.Name, false);
			EntityManager.Transaction.Add(entity);
		}

		/// <summary>
		/// Get a collection of records that should be active
		/// </summary>
		/// <param name="table">The table.</param>
		/// <returns></returns>
		private IEntityCollection ShouldBeActive(ISchemaTable table)
		{
			IQuery query = EntityManager.CreateQuery(table.Name);

			query.AddEquals(table.ActiveFlagField.Name, false);

			query.AddAnd();

			query.PushBracket();
			query.AddLessThanOrEquals(table.ActiveStartDateField.Name, Library.Environment.ClientNow);
			query.AddOr();
			query.AddEquals(table.ActiveStartDateField.Name, null);
			query.PopBracket();

			query.AddAnd();

			query.PushBracket();
			query.AddGreaterThanOrEquals(table.ActiveEndDateField.Name, Library.Environment.ClientNow);
			query.AddOr();
			query.AddEquals(table.ActiveEndDateField.Name, null);
			query.PopBracket();

			if (table.ApprovalStatusField != null)
			{
				// Special Case for Stability (if it's installed)

				if (table.Name == StudyTableName)
				{
					query.AddAnd();
					query.AddIn(table.ApprovalStatusField.Name, StudyStatusToActivate);

				}
				else
				{
					query.AddAnd();
					query.AddEquals(table.ApprovalStatusField.Name, PhraseApprStat.PhraseIdA);
				}
			}

			return EntityManager.Select(table.Name, query);
		}

		/// <summary>
		/// Get a collection of records that should be inactive
		/// </summary>
		/// <param name="table">The table.</param>
		/// <returns></returns>
		private IEntityCollection ShouldBeInactive(ISchemaTable table)
		{
			IQuery query = EntityManager.CreateQuery(table.Name);

			query.AddEquals(table.ActiveFlagField.Name, true);

			query.AddAnd();

			query.PushBracket();
			query.PushBracket();
			query.AddGreaterThanOrEquals(table.ActiveStartDateField.Name, Library.Environment.ClientNow);
			query.AddOr();
			query.AddLessThanOrEquals(table.ActiveEndDateField.Name, Library.Environment.ClientNow);
			query.PopBracket();

			if (table.ApprovalStatusField != null)
			{
				// Special Case for Stability (if it's installed)

				if (table.Name == StudyTableName)
				{
					query.AddOr();
					query.AddIn(table.ApprovalStatusField.Name, StudyStatusToDeactivate);
				}
				else
				{
					query.AddOr();
					query.AddNotEquals(table.ApprovalStatusField.Name, PhraseApprStat.PhraseIdA);
				}
			}

			query.PopBracket();

			return EntityManager.Select(table.Name, query);
		}

		/// <summary>
		/// Checks the table.
		/// </summary>
		/// <param name="table">The table.</param>
		private static void CheckTable(ISchemaTable table)
		{
			if (ActiveTable(table)) return;

			throw new SampleManagerError(ServerMessageManager.Current.GetMessage("CommonMessages", "ErrorTableDoesNotContainActive", table.Name));
		}

		/// <summary>
		/// Determines if the table is an active table type.
		/// </summary>
		/// <param name="table">The table.</param>
		/// <returns></returns>
		private static bool ActiveTable(ISchemaTable table)
		{
			if (table.IsView) return false;
			if (table.ActiveStartDateField == null) return false;
			if (table.ActiveEndDateField == null) return false;
			if (table.ActiveFlagField == null) return false;
			return true;
		}

		/// <summary>
		/// Returns true if single active version mode is enabled.
		/// </summary>
		/// <returns></returns>
		private bool SingleActiveVersion
		{
			get
			{
				if (Library.Environment.CheckGlobalExists(ApprovalSingleActiveVersion))
					return Library.Environment.GetGlobalBoolean(ApprovalSingleActiveVersion);

				return false;
			}
		}

		/// <summary>
		/// Get a collection of records that should be active when in Single Active Version mode
		/// </summary>
		/// <param name="table">The table.</param>
		/// <param name="dynamicSql">The dynamic SQL.</param>
		/// <returns></returns>
		private IEntityCollection GetCollectionSingleVersion(ISchemaTable table, string dynamicSql)
		{
			// 0 = table name
			// 1 = version field name
			// 2 = join between identity fields
			// 3 = active from field name
			// 4 = now in correct db format
			// 5 = approval status field name
			// 6 = remove flag name
			// 7 = active flag name

			StringBuilder idEquals = new StringBuilder();
			bool firstField = true;

			foreach (ISchemaField keyField in table.KeyFields)
			{
				if (keyField != table.VersionField)
				{
					if (!firstField)
						idEquals.Append(" AND ");
					else
						firstField = false;
		
					idEquals.Append(" T1.[");
					idEquals.Append(keyField.Name);
					idEquals.Append("]=T2.[");
					idEquals.Append(keyField.Name);
					idEquals.Append("]");
				}
			}

			string nowSqlS = string.Format("convert(datetime,'{0}',126)",
										   Library.Environment.ClientNow.Value.ToString("s"));
			string nowOra = string.Format("TO_DATE('{0}', 'DDMMYYYYHH24MISS')",
										  Library.Environment.ClientNow.Value.ToString("ddMMyyyyHHmmss"));

			string sqlServerSelect = string.Format(dynamicSql,
												   table.Name,
												   table.VersionField.Name,
												   idEquals,
												   table.ActiveStartDateField.Name,
												   nowSqlS,
												   table.ApprovalStatusField == null
													   ? ""
													   : "AND ((T2.[" + table.ApprovalStatusField.Name + "]='A') OR (T2.[" + table.ApprovalStatusField.Name + "]='P'))",
												   table.RemoveField == null ? "" : "AND T2.[" + table.RemoveField.Name + "]='F'",
												   table.ActiveFlagField.Name);

			string oracleSelect = string.Format(dynamicSql,
												table.Name,
												table.VersionField.Name,
												idEquals,
												table.ActiveStartDateField.Name,
												nowOra,
												table.ApprovalStatusField == null
													? ""
													   : "AND ((T2.[" + table.ApprovalStatusField.Name + "]='A') OR (T2.[" + table.ApprovalStatusField.Name + "]='P'))",
												table.RemoveField == null ? "" : "AND T2.[" + table.RemoveField.Name + "]='F'",
												table.ActiveFlagField.Name);

			oracleSelect = oracleSelect.Replace("[", "");
			oracleSelect = oracleSelect.Replace("]", "");

			return EntityManager.SelectDynamic(table.Name, oracleSelect, sqlServerSelect);
		}

		/// <summary>
		/// Get a collection of records that should be active when in Single Active Version mode
		/// </summary>
		/// <param name="table">The table.</param>
		/// <returns></returns>
		private IEntityCollection ShouldBeActiveSingleVersion(ISchemaTable table)
		{
			// 0 = table name
			// 1 = version field name
			// 2 = join between identity fields
			// 3 = active from field name
			// 4 = now in correct db format
			// 5 = approval status field name
			// 6 = remove flag name
			// 7 = active flag name

			const string dynamicSql =
				"SELECT * FROM [{0}] T1 WHERE T1.[{1}] = (SELECT MAX([{1}]) FROM [{0}] T2 WHERE {2} AND ((T2.[{3}] IS NULL) OR (T2.[{3}] <= {4})) {5} {6}) AND T1.[{7}]='F'";

			return (GetCollectionSingleVersion(table, dynamicSql));
		}

		/// <summary>
		/// Get a collection of records that should be active when in Single Active Version mode
		/// </summary>
		/// <param name="table">The table.</param>
		/// <returns></returns>
		private IEntityCollection ShouldBeInactiveSingleVersion(ISchemaTable table)
		{
			// 0 = table name
			// 1 = version field name
			// 2 = join between identity fields
			// 3 = active from field name
			// 4 = now in correct db format
			// 5 = approval status field name
			// 6 = remove flag name
			// 7 = active flag name

			const string dynamicSql =
				"SELECT * FROM [{0}] T1 WHERE T1.[{1}] <> (SELECT MAX([{1}]) FROM [{0}] T2 WHERE {2} AND ((T2.[{3}] IS NULL) OR (T2.[{3}] <= {4})) {5} {6}) AND T1.[{7}]='T'";

			return (GetCollectionSingleVersion(table, dynamicSql));
		}

		#endregion

		#region Info output

		/// <summary>
		/// Outputs the information.
		/// </summary>
		/// <param name="formatString">The format string.</param>
		/// <param name="args">The arguments.</param>
		private void OutputInfo(string formatString, params object[] args)
		{
			var output = string.Format(formatString, args);
			Logger.InfoFormat(output);

			Output.AppendLine(output);
		}

		/// <summary>
		/// Gets the message.
		/// </summary>
		/// <param name="messageIdentity">The message identity.</param>
		/// <returns></returns>
		private string GetMessage(string messageIdentity)
		{
			return Library.Message.GetMessage("CommonMessages", messageIdentity);
		}

		#endregion

		#region Task Items

		/// <summary>
		/// Sets the task items.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <param name="entityManager">The entity manager.</param>
		internal void SetTaskItems(StandardLibrary library, IEntityManager entityManager)
		{
			Library = library;
			EntityManager = entityManager;
		}

		#endregion
	}

	/// <summary>
	/// User interface to the task to update active flags
	/// </summary>
	[SampleManagerTask("ActiveRecordUiTask")]
	public class ActiveRecordUiTask : DefaultFormTask
	{
		#region Member Variables

		private ActiveRecordTask m_ActiveRecordTask;
		private FormStatusOutput m_Form;
		private BackgroundWorker m_Worker;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm" /> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			base.MainFormLoaded();

			m_ActiveRecordTask = new ActiveRecordTask();
			m_ActiveRecordTask.SetTaskItems(Library, EntityManager);

			m_Form = (FormStatusOutput) MainForm;

			m_Worker = new BackgroundWorker
			{
				WorkerReportsProgress = true
			};

			m_Worker.DoWork += (s, e) => DoTask();

			m_Worker.RunWorkerCompleted += (s, e) =>
			{
				m_Form.StatusTextbox.Text = m_ActiveRecordTask.Output.ToString();
				RefreshApplication();
			};

			m_Worker.RunWorkerAsync();
		}

		#endregion

		#region Task Execution

		/// <summary>
		/// Refreshes the application.
		/// </summary>
		private void RefreshApplication()
		{
			var appWindow = (IMainWindowService) Library.GetService(typeof (IMainWindowService));
			appWindow.Refresh();
		}

		/// <summary>
		/// Does the task.
		/// </summary>
		private void DoTask()
		{
			m_ActiveRecordTask.Launch();
		}

		#endregion
	}
}