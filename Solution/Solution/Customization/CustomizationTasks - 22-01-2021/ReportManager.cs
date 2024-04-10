using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.ObjectModel;


namespace Customization.Tasks
{
    public class ReportManager
    {
        IEntityCollection _entities;
        ReportTemplate _template;
        StandardLibrary Library;


        public ReportManager(StandardLibrary library)
        {
            Library = library;
        }

        public void PreviewReport(IEntityCollection entities, ReportTemplate template)
        {
            Library.Reporting.PreviewReport(template, entities, new ReportOptions(ReportOutput.Preview));
        }

        public void SendReport(IEntityCollection entities, ReportTemplate template, PrinterInternal printer, string subject = "Report attached", string body = "")
        {
            _entities = entities;
            _template = template;

            SendReport(printer, subject, body);
        }

        public void SendReport(IEntityCollection entities, ReportTemplate template, List<PrinterInternal> printers, string subject = "Report attached", string body = "")
        {
            _entities = entities;
            _template = template;

            foreach (var printer in printers)
            {
                SendReport(printer);
            }
        }

        void SendReport(PrinterInternal printer, string subject = "Report attached", string body = "")
        {
            if (printer.IsMailable)
            {
                SendEmail(printer, subject, body);
            }
            if (printer.IsServerPrinterQueue)
            {
                SendPrintJob(printer);
            }
        }


        void SendEmail(PrinterInternal printer, string subject, string body)
        {
            if (_entities == null || _template == null)
                return;

            MailMessage message;
            message = printer.BuildMail(subject, body);

            System.Net.Mail.Attachment reportAttachment = null;
            string tempFileName = Path.GetTempFileName();
            string fileName = string.Empty;

            if (_entities.Count > 0)
            {
                Library.Reporting.PrintReportToServerFile(_template, _entities, new ReportOptions(ReportOutput.Background), PrintFileType.Pdf, tempFileName);
                fileName = Path.ChangeExtension(tempFileName, "pdf");
                reportAttachment = new System.Net.Mail.Attachment(fileName);

                if (reportAttachment != null)
                    message.Attachments.Add(reportAttachment);
            }
            else
            {
                message.Body = "No data available for report.";
            }
            Library.Utils.Mail(message);

            // Clean up the temp files
            if (reportAttachment != null)
                reportAttachment.Dispose();
            if (!string.IsNullOrEmpty(tempFileName) && File.Exists(tempFileName))
                File.Delete(tempFileName);
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
                File.Delete(fileName);
        }

        void SendPrintJob(PrinterInternal printer)
        {
            List<string> printers = new List<string>();
            printer.GetQueuePrinterList(printers);
            foreach (string printerName in printers)
            {
                Library.Reporting.PrintReportBackground(_template, _entities, new ReportOptions(), printerName);
            }
        }
    }
}
