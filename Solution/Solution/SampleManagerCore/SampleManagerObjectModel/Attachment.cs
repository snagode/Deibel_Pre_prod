using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.ObjectModel;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the Attachment entity.
	/// </summary> 
	[SampleManagerEntity(EntityName)]
	public class Attachment : AttachmentBase
	{
		#region Constants

		/// <summary>
		/// The default Rich Text used to create an attachment comment.
		/// </summary>
		public const string DefaultRichText = @"{\rtf1\ansi\ansicpg1252\deff0\deflang2057{\fonttbl{\f0\fnil\fcharset0 Verdana;}}\viewkind4\uc1\pard\f0\fs20\par}";

		#endregion

		#region Member Variables

		private IEntityCollection m_DocumentPages;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the display name.
		/// </summary>
		/// <value>The display name.</value>
		[PromptText]
		public string DisplayName
		{
			get
			{
				if (Version == 1) return AttachmentName;
				string format = "{0} (v{1})";
				return string.Format( format, AttachmentName, Version.ToInt32(CultureInfo.CurrentCulture));
			}
		}

		#endregion

		#region Page Generation

		/// <summary>
		/// Populates attachment pages from PDF.
		/// </summary>
		/// <param name="pdfFileName">Name of the PDF file.</param>
		private void PopulatePagesFromPdf(string pdfFileName)
		{
			// Setup output folder for images

			string documentImagesPath = TemporaryDirectory;
			documentImagesPath = Path.Combine(documentImagesPath, "PdfConversions");
			documentImagesPath = Path.Combine(documentImagesPath, Guid.NewGuid().ToString());

			try
			{
				// Do the conversion

				PdfConverter converter = new PdfConverter();
				List<string> imagefiles = converter.ConvertPdfToImage(pdfFileName, documentImagesPath);

				// Create Document pages collection

				m_DocumentPages = EntityManager.CreateEntityCollection(AttachmentPageInternal.EntityName);

				// Create pages

				foreach (string imageFileName in imagefiles)
				{
					// Load image and create a page

					// We need to save the file to a stream and then dispose of 
					// the file on disk otherwise the directory cannot be deleted 
					// i.e. SampleManager retains a handle to the file on disk

					Image image = Image.FromFile(imageFileName);
					MemoryStream stream = new MemoryStream();
					image.Save(stream, ImageFormat.Png);
					image.Dispose();
					AttachmentPageInternal attachmentPage = new AttachmentPageInternal(Image.FromStream(stream));
					m_DocumentPages.Add(attachmentPage);
				}
			}
			finally
			{
				if (Directory.Exists(documentImagesPath))
				{
					Directory.Delete(documentImagesPath, true);
				}
			}
		}

		/// <summary>
		/// Gets the Attachment pages.
		/// </summary>
		/// <value>The Attachment pages.</value>
		[PromptCollection("ATTACHMENT_PAGE", false, StopAutoPublish = true)]
		public IEntityCollection Pages
		{
			get
			{
				// Only load stuff once.

				if (m_DocumentPages != null) return m_DocumentPages;

				// Drop out with no pages

				m_DocumentPages = EntityManager.CreateEntityCollection(AttachmentPageInternal.EntityName);
				if (IsNoteType) return m_DocumentPages;

				// Make a set of pages

				if (IsPdf)
				{
					LoadPdfPages();
				}
				else if (IsWord || IsExcel || IsPowerPoint || IsText || IsHtml)
				{
					LoadOfficeDocument();
				}
				else
				{
					// All of the Main file types have been attempted, try image

					LoadImage();
				}

				return m_DocumentPages;
			}
		}

		/// <summary>
		/// Loads the images.
		/// </summary>
		private void LoadImage()
		{
			// Assume that we have an image but catch exceptions

			try
			{
				string fileName = IsFileType ? ServerFileName : Attachment;
				if (string.IsNullOrEmpty(fileName)) return;

				Image image = Image.FromFile(fileName);
				AttachmentPageInternal page = new AttachmentPageInternal(image);

				m_DocumentPages = EntityManager.CreateEntityCollection(AttachmentPageInternal.EntityName);
				m_DocumentPages.Add(page);
			}
			catch (Exception)
			{
				// Ignore errors
			}
		}

		/// <summary>
		/// Loads the PDF pages.
		/// </summary>
		private void LoadPdfPages()
		{
			string pdfFileName = IsFileType ? ServerFileName : AttachmentTextInternal;
			if (string.IsNullOrEmpty(pdfFileName)) return;

			PopulatePagesFromPdf(pdfFileName);
		}

		/// <summary>
		/// Loads the Word pages.
		/// </summary>
		private void LoadOfficeDocument()
		{
			string docFileName = IsFileType ? ServerFileName : AttachmentTextInternal;
			if (string.IsNullOrEmpty(docFileName)) return;

			// Generate PDF file name

			string pdfFileName = TemporaryDirectory;
			pdfFileName = Path.Combine(pdfFileName, "OfficeConversions");
			pdfFileName = Path.Combine(pdfFileName, Guid.NewGuid().ToString());
			pdfFileName = Path.Combine(pdfFileName, Path.GetFileName(docFileName));
			pdfFileName = Path.ChangeExtension(pdfFileName, "pdf");

			try
			{
				// Do the conversion

				OfficeToPdfConverter officeConverter = new OfficeToPdfConverter();
				bool converted = officeConverter.ConvertToPdf(docFileName, pdfFileName);

				if (converted)
				{
					// Load attachment pages

					PopulatePagesFromPdf(pdfFileName);
				}
			}
			finally
			{
				// Clear temp files

				string outputDir = Path.GetDirectoryName(pdfFileName);
				if (outputDir != null && Directory.Exists(outputDir))
				{
					Directory.Delete(outputDir, true);
				}
			}
		}

		#endregion
	}
}