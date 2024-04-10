using System;
using System.Threading;
using Thermo.SampleManager.Common.CommandLine;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks.BackgroundTasks
{
	/// <summary>
	/// Generic Background reporting task
	/// </summary>
	[SampleManagerTask("GenericBackgroundReportingTask")]
	public class GenericBackgroundReportingTask : GenericReportingTask, IBackgroundTask
	{
		#region Member Variables

		/// <summary>
		/// Name of the report
		/// </summary>
		private string m_ReportName;

		/// <summary>
		/// Print Queue
		/// </summary>
		private string m_PrintQueue;

		/// <summary>
		/// Number of copies for the generated report;
		/// </summary>
		private int m_NumberOfcopies = 1;

		/// <summary>
		/// The criteria to use to select data for the report
		/// </summary>
		private string m_Criteria;

		/// <summary>
		/// The file name on the server to output the report to
		/// </summary>
		private string m_File;

		/// <summary>
		/// The report version
		/// -1 is latest version
		/// </summary>
		private int m_Version = -1;

		#endregion

		#region Task Setup

		/// <summary>
		/// Setup the SampleManager task
		/// </summary>
		protected override void SetupTask()
		{
			//Load Report Template

			if (m_Version < 0)
			{
				m_ReportTemplate = (ReportTemplate)EntityManager.SelectLatestVersion("REPORT_TEMPLATE", m_ReportName);
			}
			else
			{
				var identity = new Identity(m_ReportName, Version);

				try
				{
					m_ReportTemplate = (ReportTemplate)EntityManager.Select("REPORT_TEMPLATE", identity);

					if (!BaseEntity.IsValid(m_ReportTemplate))
						throw new Exception("Invalid version");
				}
				catch
				{
					throw new SampleManagerError(string.Format(Library.Message.GetMessage("ReportTemplateMessages", "ErrorFindingReportVersion"), Version, m_ReportName));
				}

				//throw new SampleManagerError(string.Format("SELECTED VERSION IS {0}", m_ReportTemplate.Version));
			}

			if (!BaseEntity.IsValid(m_ReportTemplate))
				throw new SampleManagerError(string.Format(Library.Message.GetMessage("ReportTemplateMessages", "ErrorFindingReportId"), m_ReportName));

			//Setup default data

			if (!string.IsNullOrWhiteSpace(m_Criteria))
			{
				IQuery query = EntityManager.CreateQuery(TableNames.CriteriaSaved);

				query.AddEquals(CriteriaSavedPropertyNames.TableName, m_ReportTemplate.DataEntityDefinition);
				query.AddAnd();
				query.AddEquals(CriteriaSavedPropertyNames.Identity, m_Criteria);

				IEntityCollection criteriaCollection = EntityManager.Select(query);

				if (criteriaCollection.Count > 0)
				{
					ICriteriaTaskService criteriaTaskService = (ICriteriaTaskService)Library.GetService(typeof(ICriteriaTaskService));
					IQuery dquery = criteriaTaskService.GetDefaultQueryByCriteria(criteriaCollection[0]);
					m_ReportData = EntityManager.Select(dquery);
				}
				else
				{
					throw new SampleManagerError(string.Format(Library.Message.GetMessage("ReportTemplateMessages", "ErrorFindingCriteriaId"), m_Criteria));
				}
			}
			else
			{
				if ((Context == null) || (Context.SelectedItems == null))
					throw new SampleManagerError(Library.Message.GetMessage("ReportTemplateMessages", "ErrorNoData"));

				if (Context.SelectedItems.Count == 0)
				{
					//Select all items for this EntityType
					m_ReportData = EntityManager.Select(m_ReportTemplate.DataEntityDefinition);
				}
				else
				{
					//Use the selected items within Explorer
					m_ReportData = Context.SelectedItems;
				}
			}

			//Create default settings

			m_ReportOptions = new ReportOptions(ReportOutput.Background);

			SetupReport();
			ProduceReport();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the name of the report.
		/// </summary>
		/// <value>The name of the report.</value>
		[CommandLineSwitch("report", "Name of the report to be run.", true)]
		public string ReportName
		{
			get { return m_ReportName; }
			set { m_ReportName = value; }
		}

		/// <summary>
		/// Gets or sets the print queue.
		/// </summary>
		/// <value>The print queue.</value>
		[CommandLineSwitch("printqueue", "Name of a Print Queue to which this background task will print to.", false)]
		public string PrintQueue
		{
			get { return m_PrintQueue; }
			set { m_PrintQueue = value; }
		}

		/// <summary>
		/// Gets or sets the number ofcopies.
		/// </summary>
		/// <value>The number ofcopies.</value>
		[CommandLineSwitch("numcopies", "Number of copies that will be generated.", false)]
		public int NumberOfcopies
		{
			get { return m_NumberOfcopies; }
			set { m_NumberOfcopies = value; }
		}

		/// <summary>
		/// Gets or sets the criteria.
		/// </summary>
		/// <value>
		/// The criteria.
		/// </value>
		[CommandLineSwitch("criteria", "Name of saved criteria for the report data", false)]
		public string Criteria
		{
			get { return m_Criteria; }
			set { m_Criteria = value; }
		}

		/// <summary>
		/// Gets or sets the file to output to
		/// </summary>
		/// <value>
		/// The file.
		/// </value>
		[CommandLineSwitch("file", "Name of the output file on the server for the report", false)]
		public string File
		{
			get { return (m_File); }
			set { m_File = value; }
		}

		/// <summary>
		/// Gets or sets the file to output to
		/// </summary>
		/// <value>
		/// The file.
		/// </value>
		[CommandLineSwitch("version", "Version of report to print", false)]
		public int Version
		{
			get { return (m_Version); }
			set { m_Version = value; }
		}


		#endregion

		#region Report Generation

		/// <summary>
		/// Produces the report.
		/// </summary>
		public override void ProduceReport()
		{
			// Nothing here, we will run the report in the Launch method
		}

		#endregion

		#region IBackgroundTask Implementation

		/// <summary>
		/// Launches this background task.
		/// </summary>
		public virtual void Launch()
		{

			bool reportProduced = false;

			if (!string.IsNullOrEmpty(m_File))
			{
				Logger.DebugFormat("Output PDF file {0}", m_File);
				Library.Reporting.PrintReportToServerFile(m_ReportTemplate, m_ReportData, m_ReportOptions,
					Common.PrintFileType.Pdf, m_File);
				reportProduced = true;
			}

			if (!reportProduced && !string.IsNullOrEmpty(m_PrintQueue))
			{
				//Send the report to the specified print queue
				Library.Reporting.PrintReportBackground(m_ReportTemplate, m_ReportData, m_ReportOptions, m_PrintQueue,
					m_NumberOfcopies);
				reportProduced = true;
			}

			if (!reportProduced && !string.IsNullOrEmpty(Context.BackgroundPrinter))
			{
				//Send the report to the specified SM background printer
				Library.Reporting.PrintReportBackgroundUsingSMPrinter(m_ReportTemplate, m_ReportData, m_ReportOptions,
					Context.BackgroundPrinter, m_NumberOfcopies);
				reportProduced = true;
			}

			if (!reportProduced)
				throw new SampleManagerError(Library.Message.GetMessage("ReportTemplateMessages", "ErrorNoDestination"));
		}

		#endregion
	}
}