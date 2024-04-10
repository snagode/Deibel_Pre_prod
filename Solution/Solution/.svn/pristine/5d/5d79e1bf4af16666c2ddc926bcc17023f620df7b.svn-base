using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Reporting task for handling a single entity basded report.
	/// Will process each passed in record individually or prompt is entities are available.
	/// </summary>
	[SampleManagerTask("SingleEntityReportingTask")]
	public class SingleEntityReportingTask : BaseReportingTask
	{
		#region Setup

		/// <summary>
		/// Setup the SampleManager task
		/// </summary>
		protected override void SetupTask()
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
				IEntity entityToPrint;

				Library.Utils.PromptForEntity(Library.Message.GetMessage("GeneralMessages", "FindEntity"),
											  Context.MenuItem.Description,
											  Context.EntityType,
											  out entityToPrint);

				m_ReportData = EntityManager.CreateEntityCollection(Context.EntityType);
				m_ReportData.Add(entityToPrint);

				SetupReport();
				ProduceReport();
			}
			else if (Context.SelectedItems.Count == 1)
			{
				m_ReportData = Context.SelectedItems;

				SetupReport();
				ProduceReport();
			}
			else
			{
				foreach (IEntity entity in Context.SelectedItems)
				{
					m_ReportData = EntityManager.CreateEntityCollection(Context.EntityType);
					m_ReportData.Add(entity);

					SetupReport();
					ProduceReport();
				}
			}

			//Exit the task
			Exit();
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