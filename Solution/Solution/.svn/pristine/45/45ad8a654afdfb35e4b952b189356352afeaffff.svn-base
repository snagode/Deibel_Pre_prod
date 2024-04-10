using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Security;
using System.Security.Permissions;
using System.ServiceModel.Web;
using Thermo.SampleManager.Core;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.WebApiTasks.Data;

namespace Thermo.SampleManager.WebApiTasks
{
	/// <summary>
	///  Files
	/// </summary>
	[SampleManagerWebApi("files")]
	public class FileTask : SampleManagerWebApiTask
	{
		#region Constants

		/// <summary>
		/// The forbidden extensions
		/// </summary>
		public readonly List<string> ForbiddenExtensions = new List<string> { ".dll", ".exe", ".config", ".sec", ".bat" };

		private const int PrivilegeFileUpload = 9041;
		private const int PrivilegeFileAccess = 9039;

		#endregion

		#region File Upload

		/// <summary>
		/// Uploads the file.
		/// </summary>
		/// <param name="logical">The logical.</param>
		/// <param name="filePath">The file path.</param>
		/// <param name="stream">The stream.</param>
		[WebInvoke(UriTemplate = "files/{logical}/{*filePath}", Method = "PUT")]
		[Description("Upload the file to the specified logical folder (9041)")]
		[MenuSecurity(PrivilegeFileUpload)]
		public void PutLogicalFile(string logical, string filePath, Stream stream)
		{
			try
			{
				if (InvalidFile(filePath)) return;

				string fileName = Path.GetFileName(filePath);
				string subFolder = Path.GetDirectoryName(filePath);
				FileInfo file;

				if (string.IsNullOrEmpty(subFolder))
				{
					file = Library.File.GetWriteFile(logical, fileName);
				}
				else
				{
					file = Library.File.GetWriteFile(logical, subFolder, fileName);
				}

				bool existed = file.Exists;

				SaveFile(file, stream);

				if (!existed)
				{
					SetHttpStatus(HttpStatusCode.Created);
				}
			}
			catch (UnauthorizedAccessException ex)
			{
				SetHttpStatus(HttpStatusCode.Forbidden, ex.Message);
			}
			catch (KeyNotFoundException)
			{
				SetHttpStatus(HttpStatusCode.NotFound);
			}
		}

		/// <summary>
		/// Saves the file.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <param name="inStream">The in stream.</param>
		public static void SaveFile(FileInfo file, Stream inStream)
		{
			using (var saveStream = file.OpenWrite())
			{
				const int bufferLen = 4096;
				byte[] buffer = new byte[bufferLen];
				int count;

				while ((count = inStream.Read(buffer, 0, bufferLen)) > 0)
				{
					saveStream.Write(buffer, 0, count);
				}
			}
		}

		/// <summary>
		/// Determines whether this instance can write the specified file.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <returns></returns>
		public static bool CanWrite(FileInfo file)
		{
			var permissionSet = new PermissionSet(PermissionState.None);
			var writePermission = new FileIOPermission(FileIOPermissionAccess.Write, file.FullName);
			permissionSet.AddPermission(writePermission);
			return (permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet));
		}

		#endregion

		#region File Listings

		/// <summary>
		/// Gets the logicals.
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "files", Method = "GET")]
		[Description("Retrieve a list of available folder logicals (9039)")]
		[MenuSecurity(PrivilegeFileAccess)]
		public List<Uri> GetLogicals()
		{
			var logicals = new List<Uri>();

			logicals.Add(GetLink(EnvironmentLibrary.SMP.ArchiveFiles));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.Calculations));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.Code));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.ComFiles));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.Criteria));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.DataFiles));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.Forms));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.GraphStyles));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.Imprint));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.Labels));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.License));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.LimitCalculations));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.ListResults));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.Listing));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.LogFiles));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.Messages));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.Programs));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.Reports));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.Resource));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.Root));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.SQLFiles));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.SampleText));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.SigFigs));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.Syntaxes));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.Tabulator));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.TextReports));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.UserFiles));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.WebServer));
			logicals.Add(GetLink(EnvironmentLibrary.SMP.Worksheets));

			return logicals;
		}

		/// <summary>
		/// Gets the link.
		/// </summary>
		/// <param name="logical">Name of the logical.</param>
		/// <returns></returns>
		private Uri GetLink(string logical)
		{
			return DataUtils.MakeLink("files/{logical}", logical);
		}

		/// <summary>
		/// Gets the logical file list
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "files/{logical}", Method = "GET")]
		[Description("Retrieve a list of available files from a logical folder (9039)")]
		[MenuSecurity(PrivilegeFileAccess)]
		public List<Uri> GetLogicalFiles(string logical)
		{
			var links = new List<Uri>();

			try
			{
				var folders = Library.Environment.GetFolderList(logical);
				foreach (DirectoryInfo directory in folders.Folders)
				{
					GetFolderFiles(logical, directory, links);
				}
			}
			catch (DirectoryNotFoundException)
			{
				return null;
			}
			catch (KeyNotFoundException)
			{
				return null;
			}

			return links;
		}

		/// <summary>
		/// Gets the folder files.
		/// </summary>
		/// <param name="logical">The logical.</param>
		/// <param name="directory">The directory.</param>
		/// <param name="links">The links.</param>
		private void GetFolderFiles(string logical, DirectoryInfo directory, List<Uri> links)
		{
			foreach (var item in directory.EnumerateFiles())
			{
				links.Add(GetLink(logical, item.Name));
			}

			foreach (var subFolder in directory.EnumerateDirectories())
			{
				GetFolderFiles(Path.Combine(logical, subFolder.Name), subFolder, links);
			}
		}

		/// <summary>
		/// Gets the link.
		/// </summary>
		/// <param name="logical">Name of the logical.</param>
		/// <param name="filePath">The file path.</param>
		/// <returns></returns>
		private Uri GetLink(string logical, string filePath)
		{
			return DataUtils.MakeLink("files/{logical}/{*filePath}", logical, filePath);
		}

		#endregion

		#region File Download

		/// <summary>
		/// Downloads the file.
		/// </summary>
		/// <param name="logical">The logical.</param>
		/// <param name="filePath">The file path.</param>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "files/{logical}/{*filePath}", Method = "GET")]
		[Description("Retrieve the file from the specified logical sub folder (9039)")]
		[MenuSecurity(PrivilegeFileAccess)]
		public Stream GetLogicalFile(string logical, string filePath)
		{
			try
			{
				string fileName = Path.GetFileName(filePath);
				string subFolder = Path.GetDirectoryName(filePath);
				FolderList folders;

				if (string.IsNullOrEmpty(subFolder))
				{
					folders = Library.Environment.GetFolderList(logical);
				}
				else
				{
					folders = Library.Environment.GetFolderList(logical, subFolder);
				}

				var file = folders.FindFile(fileName);
				return GetFile(file);
			}
			catch (KeyNotFoundException)
			{
				return null;
			}
			catch (FileNotFoundException)
			{
				return null;
			}
		}

		#endregion

		#region File Utilities

		/// <summary>
		/// Gets the file.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <returns></returns>
		public Stream GetFile(string fileName)
		{
			var file = new FileInfo(fileName);
			return GetFile(file);
		}

		/// <summary>
		/// Gets the file.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <returns></returns>
		public Stream GetFile(FileInfo file)
		{
			if (!file.Exists) return null;

			// Don't allow certain files to be downloaded.

			if (InvalidFile(file.FullName)) return null;

			// Stream back the requested file.

			string mimeType = GetMimeType(file.Name);
			SetContentDispositionFile(file.Name, mimeType);
			return File.OpenRead(file.FullName);
		}

		/// <summary>
		/// Determines if the file is invalid.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <returns></returns>
		public bool InvalidFile(string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName))
			{
				SetHttpStatus(HttpStatusCode.BadRequest);
				return true;
			}

			string extension = Path.GetExtension(fileName);

			if (ForbiddenExtensions.Contains(extension))
			{
				SetHttpStatus(HttpStatusCode.Forbidden);
				return true;
			}

			if (fileName.Contains(".."))
			{
				SetHttpStatus(HttpStatusCode.Forbidden);
				return true;
			}

			return false;
		}

		#endregion
	}
}