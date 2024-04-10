using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using Thermo.SampleManager.Common;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Basic form task to open a form using the context selection list.
	/// </summary>
	[SampleManagerTask("TestingTask")]
	public class TestingTask : DefaultFormTask
	{
		#region Overrides

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			FormAA_Test form = (FormAA_Test)MainForm;
			form.ButtonEdit1.Click += new System.EventHandler(ButtonEdit1_Click);
		}

		void ButtonEdit1_Click(object sender, System.EventArgs e)
		{
			////IEntityCollection allHazards = EntityManager.Select(Hazard.EntityName);

			////Library.Reporting.PrintReportToServerFile("HAZARD", allHazards[0], null, PrintFileType.Pdf, @"C:\MyReport.pdf");

			////StreamReader streamReader = new StreamReader(@"C:\MyReport.pdf");

			////try 
			////{
			////    Font printFont = new Font("Arial", 10);
			////    PrintDocument pd = new PrintDocument(); 
			////    //pd.PrintPage += new PrintPageEventHandler(pd_PrintPage);
     
			////    // Specify the printer to use.
			////    pd.PrinterSettings.PrinterName = "HP Photosmart 2570 series";

			////    if (pd.PrinterSettings.IsValid) 
			////    {
			////        pd.Print();
			////    } 
			////} 
			////finally 
			////{
			////    streamReader.Close();
			////}

			//// Set Acrobat Reader EXE, e.g.:
			////PdfPrinter.AdobeReaderPath = @"C:\Program Files\Adobe\Adobe Acrobat 7.0\Acrobat\Acrobat.exe";
			//// -or-
			////PdfPrinter.AdobeReaderPath = @"C:\Program Files\Adobe\[...]\AcroRd32.exe";

			//// On my computer (running Windows Vista 64) it is here:
			//PdfFilePrinter.AdobeReaderPath = @"C:\Program Files (x86)\Adobe\Acrobat 8.0\Acrobat\Acrobat.exe";

			//// Set the file to print and the Windows name of the printer.
			//// At my home office I have an old Laserjet 6L under my desk.
			//PdfFilePrinter printer = new PdfFilePrinter(@"..\..\..\..\..\PDFS\HelloWorld.pdf", "HP LaserJet 6L");

			//try
			//{
			//    printer.Print();
			//}
			//catch (Exception ex)
			//{

			//}


		}		

		#endregion
	}
}