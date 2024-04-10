using Thermo.SampleManager.Common.CommandLine;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks.BackgroundTasks
{
	/// <summary>
	/// Background task to email reports
	/// </summary>
	[SampleManagerTask("MailReport")]
	public class MailReportTask : SampleManagerTask, IBackgroundTask
	{
		#region Member Variables

		private string m_Report;
		private string m_Operator;
		private string m_Criteria;
		private string m_Destination;
		private bool m_Html;

		private Personnel m_Personnel;
		private CriteriaSaved m_SavedCriteria;
		private ReportTemplate m_ReportTemplate;
		private Printer m_Printer;
        private Printer m_BackgroundPrinter;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the criteria
		/// </summary>
		/// <value>The criteria.</value>
		[CommandLineSwitch("criteria", "Criteria to execute", false)]
		public string Criteria
		{
			get { return m_Criteria; }
			set { m_Criteria = value; }
		}

		/// <summary>
		/// Gets or sets the report.
		/// </summary>
		/// <value>The report.</value>
		[CommandLineSwitch("report", "Report to Run", true)]
		public string Report
		{
			get { return m_Report; }
			set { m_Report = value; }
		}

		/// <summary>
		/// Gets or sets the email operator
		/// </summary>
		/// <value>The email.</value>
		[CommandLineSwitch("operator", "Operator to Email the Report to", false)]
		public string Operator
		{
			get { return m_Operator; }
			set { m_Operator = value; }
		}

		/// <summary>
		/// Gets or sets the email operator
		/// </summary>
		/// <value>The email.</value>
		[CommandLineSwitch("destination", "Printer Destination - this should be a MAIL destination, or distribution list of MAIL destinations", false)]
		public string Destination
		{
			get { return m_Destination; }
			set { m_Destination = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this email should be sent as HTML.
		/// </summary>
		/// <value><c>true</c> if HTML; otherwise, <c>false</c>.</value>
		[CommandLineSwitch("html", "Email as HTML body", false)]
		public bool Html
		{
			get { return m_Html; }
			set { m_Html = value; }
		}

		#endregion

		#region IBackgroundTask Members

		/// <summary>
		/// Launches this background task.
		/// </summary>
		public void Launch()
		{
			Logger.Debug("Starting Mail Report Task...");

			// Report to Execute

			ProcessParameters();

			// Do the work.

			Logger.Debug("Selecting the Data...");

			if (m_SavedCriteria == null)
			{
				IEntityCollection data = EntityManager.Select(m_ReportTemplate.DataEntityDefinition);
				GenerateReport(data);
			}
			else
			{
				ICriteriaTaskService criteriaService = (ICriteriaTaskService) Library.GetService(typeof (ICriteriaTaskService));
				criteriaService.QueryPopulated += CriteriaServiceQueryPopulated;

				// Note that code execution will "continue" in the event handler for query populated

				criteriaService.GetPopulatedCriteriaQuery(m_SavedCriteria);
			}
		}

		/// <summary>
		/// Criteria query populated.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Server.CriteriaTaskQueryPopulatedEventArgs"/> instance containing the event data.</param>
		void CriteriaServiceQueryPopulated(object sender, CriteriaTaskQueryPopulatedEventArgs e)
		{
			IEntityCollection data = EntityManager.Select(m_ReportTemplate.DataEntityDefinition, e.PopulatedQuery);
			GenerateReport(data);
		}

		/// <summary>
		/// Generates the report.
		/// </summary>
		/// <param name="data">The data.</param>
		private void GenerateReport(IEntityCollection data)
		{
			// Send to the Person

			if (BaseEntity.IsValid(m_Personnel))
			{
				if (Html) m_Personnel.MailHtmlReport(m_ReportTemplate, data);
				else m_Personnel.MailReport(m_ReportTemplate, data);
			}

			// Send to the Operator

			if (BaseEntity.IsValid(m_Printer))
			{
				if (Html) m_Printer.MailHtmlReport(m_ReportTemplate, data);
				else m_Printer.MailReport(m_ReportTemplate, data);
			}

            // Send to the back ground Printer

            if (BaseEntity.IsValid(m_BackgroundPrinter))
            {
                if (m_BackgroundPrinter.IsMailable)
                {
                    if (Html) m_BackgroundPrinter.MailHtmlReport(m_ReportTemplate, data);
                    else m_BackgroundPrinter.MailReport(m_ReportTemplate, data);
                }
            }

			// Tell the Task it should die

			Exit();
		}

		#endregion

		#region Parameters

		/// <summary>
		/// Processes the parameters.
		/// </summary>
		private void ProcessParameters()
		{
			m_ReportTemplate = (ReportTemplate) EntityManager.Select(ReportTemplate.EntityName, m_Report);
			if (! BaseEntity.IsValid(m_ReportTemplate))
			{
				string message = Library.Message.GetMessage("LaboratoryMessages","InvalidReportTemplate", m_Report);
				string messageTitle = Library.Message.GetMessage("LaboratoryMessages", "InvalidReportTemplateTitle");
				throw new SampleManagerError(messageTitle, message);
			}

			// Get hold of the Criteria if specified

			if (string.IsNullOrEmpty(m_Criteria)) m_SavedCriteria = null;
			else
			{
				Identity criteriaId = new Identity( m_ReportTemplate.DataEntityDefinition, m_Criteria);
				m_SavedCriteria = (CriteriaSaved)EntityManager.Select(CriteriaSaved.EntityName, criteriaId);

				if (!BaseEntity.IsValid(m_SavedCriteria))
				{
					string message = Library.Message.GetMessage("LaboratoryMessages","InvalidCriteria", m_Criteria, m_ReportTemplate.DataEntityDefinition);
					string messageTitle = Library.Message.GetMessage("LaboratoryMessages", "InvalidCriteriaTitle");
					throw new SampleManagerError(messageTitle, message);
				}
			}

			// Operator to email stuff to

			if (string.IsNullOrEmpty(m_Operator)) m_Personnel = null;
			else
			{
				m_Personnel = (Personnel) EntityManager.Select(Personnel.EntityName, m_Operator);
				if (!BaseEntity.IsValid(m_Personnel))
				{
					string message = Library.Message.GetMessage("LaboratoryMessages", "InvalidEmailOperator", m_Operator);
					string messageTitle = Library.Message.GetMessage("LaboratoryMessages", "InvalidEmailOperatorTitle");
					throw new SampleManagerError(messageTitle, message);
				}

				if (!m_Personnel.IsMailable)
				{
					string message = Library.Message.GetMessage("LaboratoryMessages", "InvalidOperatorEmail", m_Personnel.PersonnelName);
					string messageTitle = Library.Message.GetMessage("LaboratoryMessages", "InvalidOperatorEmailTitle");
					throw new SampleManagerError(messageTitle, message);
				}
			}

			// Distribution to email stuff to

			if (string.IsNullOrEmpty(m_Destination)) m_Printer = null;
			else
			{
				m_Printer = (Printer)EntityManager.Select(Printer.EntityName, m_Destination);
				if (!BaseEntity.IsValid(m_Printer))
				{
					string message = Library.Message.GetMessage("LaboratoryMessages","InvalidDestination", m_Destination);
					string messageTitle = Library.Message.GetMessage("LaboratoryMessages", "InvalidDestinationTitle");
					throw new SampleManagerError(messageTitle, message);
				}

				if (!m_Printer.IsMailable && !m_Printer.IsMailableDistribution)
				{
					string message = Library.Message.GetMessage("LaboratoryMessages", "InvalidPrinterEmail", m_Destination);
					string messageTitle = Library.Message.GetMessage("LaboratoryMessages", "InvalidPrinterEmailTitle");
					throw new SampleManagerError(messageTitle, message);
				}
			}

            // Distrubute to the background printer

            if (!string.IsNullOrEmpty(Context.BackgroundPrinter))
            {
                m_BackgroundPrinter = (Printer)EntityManager.Select(Printer.EntityName, Context.BackgroundPrinter);
            }

			// Make sure we have at least one valid email

			if (!BaseEntity.IsValid(m_Printer) && !BaseEntity.IsValid(m_Personnel))
			{
				string message = Library.Message.GetMessage("LaboratoryMessages", "InvalidOperatorOrDestination");
				string messageTitle = Library.Message.GetMessage("LaboratoryMessages", "InvalidOperatorOrDestinationTitle");
				throw new SampleManagerError(messageTitle, message);
			}
		}

		#endregion
	}
}