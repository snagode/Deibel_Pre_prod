using System;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Base reporting task to share common functionality to derived classes.
	/// </summary>
	public abstract class BaseReportingTask : SampleManagerTask
	{
		#region Constants

		/// <summary>
		/// User parameter name.
		/// </summary>
		protected const string UserParam = "paramUser";

		/// <summary>
		/// TimeStamp parameter name.
		/// </summary>
		protected const string TimeStampParam = "paramTimeStamp";

		/// <summary>
		/// TimeStamp parameter name.
		/// </summary>
		protected const string PromptKeyWord = "[PROMPT]";

		#endregion

		#region Member Variables

		/// <summary>
		/// Report Template Entity.
		/// </summary>
		protected ReportTemplate m_ReportTemplate;

		/// <summary>
		/// Report Template Entity.
		/// </summary>
		protected ReportTemplate[] m_ReportTemplateCollection;

		/// <summary>
		/// Data that is to be presented within the report. By default, this is the selected 
		/// items from Explorer. If there is no selection, the selected items within Explorer are used.
		/// </summary>
		protected IEntityCollection m_ReportData;

		/// <summary>
		/// Report display options.
		/// </summary>
		protected ReportOptions m_ReportOptions;

		/// <summary>
		/// Report master template.
		/// </summary>
		protected IEntity m_MasterTemplate;

		/// <summary>
		/// Execute without data
		/// </summary>
		protected bool m_ExecuteWithoutData = false;

		#endregion

		#region Report Utilities
		
		/// <summary>
		/// Produces the report.
		/// </summary>
		public virtual void ProduceReport()
		{
			if (m_ReportData.Count <= 0 && !m_ExecuteWithoutData) 
			{
				Library.Utils.FlashMessage(Library.Message.GetMessage("ReportTemplateMessages", "NoDataSelectedBody"),
					Library.Message.GetMessage("ReportTemplateMessages", "NoDataSelectedHead"));
				return;
			}
			
			//if single report create array of one
			if (m_ReportTemplateCollection == null)
				m_ReportTemplateCollection = new ReportTemplate[] {m_ReportTemplate};

			//add order numbers to parameters
			int i=0;
			foreach (ReportTemplate reportTemplate in m_ReportTemplateCollection)
			{
				foreach (ReportSpecificParameter specificParameter in reportTemplate.SpecificParameters)
				{
					specificParameter.OrderNumber = i;
					m_ReportOptions.ReportSpecificParameters.Add(specificParameter);
				}
				i++;
			}

			if (m_ReportTemplateCollection != null && m_ReportTemplateCollection.Length > 0)
			{
				//Make sure data is correct
				if (m_ReportData.EntityType != m_ReportTemplateCollection[0].DataEntityDefinition)
				{
					throw new SampleManagerError(string.Format(GetMessage("ReportTemplateMessages", "InvalidDataException"),
						m_ReportTemplate.DataEntityDefinition));
				}

				//Generate report on client
				switch (m_ReportOptions.Output)
				{
					case ReportOutput.Preview:
						//Display print preview screen on client
						Library.Reporting.PreviewReport(m_ReportTemplateCollection, m_ReportData, m_ReportOptions, m_MasterTemplate);
						break;

					case ReportOutput.Print:
						//Send to printer dialog on client
						Library.Reporting.PrintReport(m_ReportTemplateCollection, m_ReportData, m_ReportOptions, m_MasterTemplate);
						break;

					case ReportOutput.PrintExcel:

						if (m_ReportOptions.PrintToServerFile)
						{
							//Save to Excel File on server
							Library.Reporting.PrintReportToServerFile(m_ReportTemplateCollection, m_ReportData, m_ReportOptions,
								Common.PrintFileType.Excel, m_ReportOptions.FileName, m_MasterTemplate);
							break;
						}

						//Save to Excel File on client
						Library.Reporting.PrintReportToClientFile(m_ReportTemplateCollection, m_ReportData, m_ReportOptions,
							Common.PrintFileType.Excel, m_ReportOptions.FileName, m_ReportOptions.OpenFile, m_MasterTemplate);
						break;

					case ReportOutput.PrintHtml:

						if (m_ReportOptions.PrintToServerFile)
						{
							//Save to HTML File on server
							Library.Reporting.PrintReportToServerFile(m_ReportTemplateCollection, m_ReportData, m_ReportOptions,
								Common.PrintFileType.Html, m_ReportOptions.FileName, m_MasterTemplate);
							break;
						}

						//Save to HTML File on client
						Library.Reporting.PrintReportToClientFile(m_ReportTemplateCollection, m_ReportData, m_ReportOptions,
							Common.PrintFileType.Html, m_ReportOptions.FileName, m_ReportOptions.OpenFile, m_MasterTemplate);
						break;

					case ReportOutput.PrintMht:

						if (m_ReportOptions.PrintToServerFile)
						{
							//Save to MHT File on server
							Library.Reporting.PrintReportToServerFile(m_ReportTemplateCollection, m_ReportData, m_ReportOptions,
								Common.PrintFileType.Mht, m_ReportOptions.FileName, m_MasterTemplate);
							break;
						}

						//Save to MHT File on client
						Library.Reporting.PrintReportToClientFile(m_ReportTemplateCollection, m_ReportData, m_ReportOptions,
							Common.PrintFileType.Mht, m_ReportOptions.FileName, m_ReportOptions.OpenFile, m_MasterTemplate);
						break;

					case ReportOutput.PrintPdf:

						if (m_ReportOptions.PrintToServerFile)
						{
							//Save to PDF File on server
							Library.Reporting.PrintReportToServerFile(m_ReportTemplateCollection, m_ReportData, m_ReportOptions,
								Common.PrintFileType.Pdf, m_ReportOptions.FileName, m_MasterTemplate);
							break;
						}

						//Save to PDF File on client
						Library.Reporting.PrintReportToClientFile(m_ReportTemplateCollection, m_ReportData, m_ReportOptions,
							Common.PrintFileType.Pdf, m_ReportOptions.FileName, m_ReportOptions.OpenFile, m_MasterTemplate);
						break;

					case ReportOutput.PrintRtf:

						if (m_ReportOptions.PrintToServerFile)
						{
							//Save to RTF File on server
							Library.Reporting.PrintReportToServerFile(m_ReportTemplateCollection, m_ReportData, m_ReportOptions,
								Common.PrintFileType.Rtf, m_ReportOptions.FileName, m_MasterTemplate);
							break;
						}

						//Save to RTF File on client
						Library.Reporting.PrintReportToClientFile(m_ReportTemplateCollection, m_ReportData, m_ReportOptions,
							Common.PrintFileType.Rtf, m_ReportOptions.FileName, m_ReportOptions.OpenFile, m_MasterTemplate);
						break;
					case ReportOutput.PrintDoc:

						if (m_ReportOptions.PrintToServerFile)
						{
							//Save to Dpoc File on server
							Library.Reporting.PrintReportToServerFile(m_ReportTemplateCollection, m_ReportData, m_ReportOptions,
								Common.PrintFileType.Doc, m_ReportOptions.FileName, m_MasterTemplate);
							break;
						}

						//Save to Doc File on client
						Library.Reporting.PrintReportToClientFile(m_ReportTemplateCollection, m_ReportData, m_ReportOptions,
							Common.PrintFileType.Doc, m_ReportOptions.FileName, m_ReportOptions.OpenFile, m_MasterTemplate);
						break;
					case ReportOutput.PrintEpub:

						if (m_ReportOptions.PrintToServerFile)
						{
							//Save to Dpoc File on server
							Library.Reporting.PrintReportToServerFile(m_ReportTemplateCollection, m_ReportData, m_ReportOptions,
								Common.PrintFileType.Epub, m_ReportOptions.FileName, m_MasterTemplate);
							break;
						}

						//Save to Doc File on client
						Library.Reporting.PrintReportToClientFile(m_ReportTemplateCollection, m_ReportData, m_ReportOptions,
							Common.PrintFileType.Epub, m_ReportOptions.FileName, m_ReportOptions.OpenFile, m_MasterTemplate);
						break;
					case ReportOutput.PrintTxt:

						if (m_ReportOptions.PrintToServerFile)
						{
							//Save to Dpoc File on server
							Library.Reporting.PrintReportToServerFile(m_ReportTemplateCollection, m_ReportData, m_ReportOptions,
								Common.PrintFileType.Txt, m_ReportOptions.FileName, m_MasterTemplate);
							break;
						}

						//Save to Doc File on client
						Library.Reporting.PrintReportToClientFile(m_ReportTemplateCollection, m_ReportData, m_ReportOptions,
							Common.PrintFileType.Txt, m_ReportOptions.FileName, m_ReportOptions.OpenFile, m_MasterTemplate);
						break;
					case ReportOutput.PrintOdt:

						if (m_ReportOptions.PrintToServerFile)
						{
							//Save to Dpoc File on server
							Library.Reporting.PrintReportToServerFile(m_ReportTemplateCollection, m_ReportData, m_ReportOptions,
								Common.PrintFileType.Odt, m_ReportOptions.FileName, m_MasterTemplate);
							break;
						}

						//Save to Doc File on client
						Library.Reporting.PrintReportToClientFile(m_ReportTemplateCollection, m_ReportData, m_ReportOptions,
							Common.PrintFileType.Odt, m_ReportOptions.FileName, m_ReportOptions.OpenFile, m_MasterTemplate);
						break;
					case ReportOutput.PrintPng:

						if (m_ReportOptions.PrintToServerFile)
						{
							//Save to Dpoc File on server
							Library.Reporting.PrintReportToServerFile(m_ReportTemplateCollection, m_ReportData, m_ReportOptions,
								Common.PrintFileType.Png, m_ReportOptions.FileName, m_MasterTemplate);
							break;
						}

						//Save to Doc File on client
						Library.Reporting.PrintReportToClientFile(m_ReportTemplateCollection, m_ReportData, m_ReportOptions,
							Common.PrintFileType.Png, m_ReportOptions.FileName, m_ReportOptions.OpenFile, m_MasterTemplate);
						break;
					case ReportOutput.PrintJpeg:

						if (m_ReportOptions.PrintToServerFile)
						{
							//Save to Dpoc File on server
							Library.Reporting.PrintReportToServerFile(m_ReportTemplateCollection, m_ReportData, m_ReportOptions,
								Common.PrintFileType.Jpeg, m_ReportOptions.FileName, m_MasterTemplate);
							break;
						}

						//Save to Doc File on client
						Library.Reporting.PrintReportToClientFile(m_ReportTemplateCollection, m_ReportData, m_ReportOptions,
							Common.PrintFileType.Jpeg, m_ReportOptions.FileName, m_ReportOptions.OpenFile, m_MasterTemplate);
						break;
				}
			}
		}

		#endregion

		#region Message Utilities

		/// <summary>
		/// Get a message
		/// </summary>
		/// <returns></returns>
		protected string GetMessage(string category, string messageIdentity)
		{
			return Library.Message.GetMessage(category, messageIdentity);
		}

		/// <summary>
		/// Get a message
		/// </summary>
		/// <returns></returns>
		protected string GetMessage(string category, string messageIdentity, params string[] param)
		{
			return Library.Message.GetMessage(category, messageIdentity, param);
		}

		#endregion

		#region Obsolete Methods

		/// <summary>
		/// Assigns the default report option parameters.
		/// </summary>
		[Obsolete("No longer needed")]
		public void AssignDefaultReportOptionParameters()
		{
		}

		#endregion
	}
}
