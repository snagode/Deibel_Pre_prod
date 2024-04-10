using System;
using System.IO;
using System.Linq;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;


namespace Customization.Tasks
{
    [SampleManagerTask(nameof(SampleImageTask))]
    public class SampleImageTask : SampleManagerTask
    {
        Sample _sample;

        protected override void SetupTask()
        {
            if (Context.SelectedItems.ActiveCount == 0)
                return;

            _sample = Context.SelectedItems.ActiveItems[0] as Sample;
            if (_sample == null || _sample.IsNull())
            {
                Library.Utils.FlashMessage("Invalid sample selection", "Selection");
                return;
            }

            var fileMode = Context.TaskParameters[0];
            if (fileMode == null || (fileMode != "VIEW" && fileMode != "SAVE" && fileMode != "DELETE"))
            {
                Library.Utils.FlashMessage("Invalid menu item configuration.  Task parameter VIEW, SAVE, or DELETE is required.", "Warning");
                return;
            }

            if (fileMode == "SAVE")
                AddFile();

            if (fileMode == "VIEW")
                ViewFile();

            if (fileMode == "DELETE")
                DeleteFile();
        }

        void AddFile()
        {
            string fileName = Library.Utils.PromptForFile("Select sample image", "All Files(*.*)|*.*");
            if (fileName == null)
                return;
            if (fileName != string.Empty)
            {
                // Name of the file we're going to save
                string newFile = string.Format("SampleImage_{0}", _sample.IdNumeric.ToString().Trim());

                // Path of image files
                string path = Library.File.GetFolderList("smp$sampleimages") + "\\";

                // Quietly delete existing files
                var dir = new DirectoryInfo(path);
                FileInfo[] files = dir.GetFiles(newFile + "*");
                if(files.Length > 0)
                {
                    foreach(var file in files)
                    {
                        try
                        {
                            File.Delete(file.FullName);
                        }
                        catch { }
                    }
                }

                // Save the new file
                string fileExt = System.IO.Path.GetExtension(fileName);
                string serverFileName = string.Format("{0}\\{1}{2}", Library.File.GetFolderList("smp$sampleimages"), newFile, fileExt);
                Library.File.TransferFromClient(fileName, serverFileName);
                if (!File.Exists(serverFileName))
                {
                    Library.Utils.FlashMessage("File attachment '{0}' not saved.", fileName);
                    return;
                }
                else  // File save successful, update image path
                {
                    _sample.ImagePath = newFile + fileExt;
                    EntityManager.Transaction.Add(_sample);
                    EntityManager.Commit();              
                }
            }
        }

        void ViewFile()
        {
            var fileName = _sample.ImagePath;
            string serverFileName = string.Format("{0}\\{1}", Library.File.GetFolderList("smp$sampleimages"), fileName);
            string clientFileName = fileName;
            if (File.Exists(serverFileName))
            {
                Library.File.TransferToClientTemp(serverFileName, clientFileName);
                Library.File.OpenClientTempFile(clientFileName);
            }
            else
                Library.Utils.FlashMessage("File not found.", "File missing");
        }

        void DeleteFile()
        {
            var fileName = _sample.ImagePath;
            var serverFilename = string.Format("{0}\\{1}", Library.File.GetFolderList("smp$sampleimages"), fileName);
            if (File.Exists(serverFilename))
            {
                if (Library.Utils.FlashMessageYesNo($"Are you sure you want to permanently delete sample image file {fileName}?", "Confirm"))
                {
                    try
                    {
                        File.Delete(serverFilename);
                        string confirm = $"File {fileName} has been deleted.";
                        _sample.ImagePath = "";
                        EntityManager.Transaction.Add(_sample);
                        EntityManager.Commit();
                    }
                    catch(Exception e)
                    {
                        Library.Utils.FlashMessage($"System has failed to delete file {fileName} with error: {e.Message}", "Error");
                    }
                }
            }
            else
                Library.Utils.FlashMessage("File not found.", "File missing");
        }
    }
}
