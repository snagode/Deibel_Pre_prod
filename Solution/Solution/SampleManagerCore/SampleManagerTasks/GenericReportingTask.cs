using System;
using System.Collections.Generic;
using System.Text;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Base class for all reporting tasks.
	/// </summary>
	[SampleManagerTask("GenericReportingTask")]
    public class GenericReportingTask : BaseReportingTask
	{
		#region Setup

		/// <summary>
		/// Setup the SampleManager task
		/// </summary>
		protected override void SetupTask()
		{
			try
			{
				//Make sure setup of task is correct
				if (Context.TaskParameters.Length == 0 || string.IsNullOrEmpty(Context.TaskParameters[0]))
				{
					throw new SampleManagerError(string.Format(GetMessage("GeneralMessages", "EditorModeException", "")));
				}
				//Load Report Template
				m_ReportTemplate = (ReportTemplate)EntityManager.SelectLatestVersion("REPORT_TEMPLATE", Context.TaskParameters[0]);

				//Create default settings
				m_ReportOptions = new ReportOptions(ReportOutput.Preview);

				//Setup default data
				if (Context.SelectedItems.Count == 0)
				{
					if (string.IsNullOrEmpty(Context.EntityType))
					{
						// No Table name has been defined so the parameters are recieved in their raw format.
						// Parse the task parameters
						m_ReportData = ParseRawCommand();
					}
					else
					{
						//Entity type is supplied, select all items for this EntityType
						m_ReportData = EntityManager.Select(Context.EntityType);
					}
				}
				else
				{
					//Use the selected items within Explorer
					m_ReportData = Context.SelectedItems;
				}

				SetupReport();
				ProduceReport();
			}
			finally
			{
				//Exit the task
				Exit();
			}
		}

		/// <summary>
		/// Parses the raw command.
		/// </summary>
		/// <returns></returns>
		private IEntityCollection ParseRawCommand()
		{
			string entityType = m_ReportTemplate.DataEntityDefinition;

			// Composite Keys not supoorted here
			ISchemaTable table = Library.Schema.Tables[entityType];
			if (table.KeyFields.Count > 1)
			{
				throw new SampleManagerError(Library.Message.GetMessage("LaboratoryMessages", "ParseRawCommandError"));
			}

			string keyField = table.KeyFields[0].Name;
			string inputString = Context.TaskParameters[1];

			string[] identities;

			if (inputString.Contains("\""))
			{
				// Input string has quotes, parse this accordingly
				identities = ParseIdentitiesWithQuotes(inputString);
			}
			else
			{
				// No quotes are present, just split the string
				identities = inputString.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
			}
				
			// Generate a query for the identies provided
			IQuery query = EntityManager.CreateQuery(entityType);
			bool addOr = false;
			foreach(string identity in identities)
			{
				if (addOr)
				{
					// There is a preceeding clause, add an Or
					query.AddOr();
				}

				// Add clause for this entity
				query.AddEquals(keyField, identity);
				addOr = true;
			}

			// Select report data and return
			return EntityManager.Select(entityType, query);
		}

		/// <summary>
		/// Parses the identities with quotes.
		/// </summary>
		/// <param name="inputString">The input string.</param>
		/// <returns></returns>
		private static string[] ParseIdentitiesWithQuotes(string inputString)
		{
			List<string> identityList = new List<string>();

			// Identities have spaces so are wrapped with quotes
			// Walk along the string parsing each identity from within the quotes
			StringBuilder identityBuilder = new StringBuilder();
			bool withinQuotes = false;
			foreach (char c in inputString)
			{
				switch (c)
				{
					case '"':

						// Is this character a quote?
						withinQuotes = !withinQuotes;

						// If we are no longer within quotes, add the text parsed between the quotes to the result
						if (!withinQuotes)
						{
							// Add identity to the result and clear the string builder in preperation for the next identity
							identityList.Add(identityBuilder.ToString());
							identityBuilder.Clear();
						}

						break;

					case ' ':

						// This is a space, add it if we are not within quotes
						if (withinQuotes)
						{
							identityBuilder.Append(c);
						}

						break;

					default:

						// For all other characters, add them to the identity
						identityBuilder.Append(c);

						break;
				}
			}

			// Add the last ID into the results if needed
			if (identityBuilder.Length != 0)
			{
				identityList.Add(identityBuilder.ToString());
			}

			// Assign identities for query generation
			return identityList.ToArray();
		}

		/// <summary>
		/// Sets up report data and output options.
		/// </summary>
		protected virtual void SetupReport()
		{
			//Nothing at this level
		}

		#endregion



	}
}