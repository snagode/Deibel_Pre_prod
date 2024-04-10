using System;
using System.Collections.Generic;
using PdfToImage;
using Thermo.SampleManager.Core.Exceptions;
using System.IO;
using Thermo.SampleManager.Library.ObjectModel;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Converts PDF files into images
	/// </summary>
	internal class PdfConverter
	{
		#region Conversion Methods

		/// <summary>
		/// Converts a PDF file into images (one image per page).
		/// </summary>
		/// <param name="pdfFileName">Name of the PDF file.</param>
		/// <param name="outputFolder">The output folder into which the images will be placed.</param>
		internal List<string> ConvertPdfToImage(string pdfFileName, string outputFolder)
		{
			if(!File.Exists(pdfFileName))
				return new List<string>();

			const string outputExtension = "png";
			const string outputFormat = "png256";

			PDFConvert converter = new PDFConvert();

			converter.OutputToMultipleFile = true;	//	)
			converter.FirstPageToConvert = -1;		//	>All Pages
			converter.LastPageToConvert = -1;		//	)
			converter.FitPage = false;
			converter.JPEGQuality = 10;
			converter.OutputFormat = outputFormat;
			
			FileInfo input = new FileInfo(pdfFileName);

			if (input == null || input.Directory == null)
			{
				throw new SampleManagerError(string.Format("Pdf file '{0}' does not exist!", pdfFileName));
			}

			// Make sure output folder is ready

			if (Directory.Exists(outputFolder))
			{
				Directory.Delete(outputFolder, true);
			}

			Directory.CreateDirectory(outputFolder);

			// Do the conversion

			string output = string.Format("{0}\\{1}", outputFolder, Path.ChangeExtension(input.Name, outputExtension));

			try
			{
				bool converted = converter.Convert(input.FullName, output);

				if (!converted)
				{
					// Cannot proceed...

					throw new SampleManagerError(string.Format("Error converting PDF file '{0}' to an image", pdfFileName));
				}

				// Sort filenames

				List<string> imageFiles = new List<string>(Directory.GetFiles(outputFolder));
				
				if (imageFiles.Count > 1)
				{
					string fileNamePrefix = Path.GetFileName(pdfFileName).ToUpper().Replace(".PDF", "");
					imageFiles.Sort(new ImageFileComparer(fileNamePrefix, ".png"));
				}

				return imageFiles;
			}
			catch (Exception e)
			{
				// Display error

				throw new SampleManagerError(string.Format("Error converting PDF file '{0}' to an image. {1}", pdfFileName, e.Message));
			}
		}

		#endregion

	}
}
