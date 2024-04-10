using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core;
using System.IO;

namespace Thermo.SampleManager.Tasks
{
    /// <summary>
    /// Printer Destination Task
    /// </summary>
    [SampleManagerTask("PrinterDestinationTask", "LABTABLE", "PRINTER")]
    public class PrinterDestinationTask : GenericLabtableTask
    {
        #region Member Variables

        private FormPrinterDestination m_Form;
        private PrinterBase m_Printer;
        private Panel m_LastPanel;
        private List<string> m_PrinterCodeTypes;
		private PhraseBase m_LastDevice;

        #endregion

        #region Overridden Methods

        /// <summary>
        /// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
        /// </summary>
        protected override void MainFormCreated()
        {
            m_Form = (FormPrinterDestination)MainForm;
            m_Printer = (PrinterBase)MainForm.Entity;
            m_Printer.PropertyChanged += new PropertyEventHandler(m_Printer_PropertyChanged);
        }

        /// <summary>
        /// Called when the <see cref="GenericLabtableTask.MainForm"/> has been loaded.
        /// </summary>
        protected override void MainFormLoaded()
        {
            SetInitialPrinterType();
            PopulatePrinterQueueBrowse();
        }

        #endregion

        #region Events

        /// <summary>
        /// m_Printer_PropertyChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_Printer_PropertyChanged(object sender, PropertyEventArgs e)
        {
            if (e.PropertyName == PrinterPropertyNames.DeviceType)
            {
                SetDestinationPanels(m_Printer.DeviceType);
				m_LastDevice = m_Printer.DeviceType;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Set destination panels
        /// </summary>
        /// <param name="deviceType"></param>
        private void SetDestinationPanels(PhraseBase deviceType)
        {
            if (deviceType != null)
            {
                ResetPrinter(deviceType);

                if (m_Printer.DeviceType.PhraseId == PhrasePrintType.PhraseIdMAIL_OPER)
                {
                    TogglePanelVisibility(m_Form.PanelOperator);
                }
                else if (m_Printer.DeviceType.PhraseId == PhrasePrintType.PhraseIdDIST)
                {
                    TogglePanelVisibility(m_Form.PanelDestinations);
                }
                else if (m_Printer.DeviceType.PhraseId == PhrasePrintType.PhraseIdLOCAL)
                {
                    TogglePanelVisibility(m_Form.PanelLocal);
                }
                else if (m_Printer.DeviceType.PhraseId == PhrasePrintType.PhraseIdDEVICE)
                {
                    TogglePanelVisibility(m_Form.PanelDevice);
                }
                else if (m_Printer.DeviceType.PhraseId == PhrasePrintType.PhraseIdQUEUE)
                {
                    TogglePanelVisibility(m_Form.PanelQueue);
                }
                else if (m_Printer.DeviceType.PhraseId == PhrasePrintType.PhraseIdMAIL)
                {
                    TogglePanelVisibility(m_Form.PanelMail);
                }
                else if (m_Printer.DeviceType.PhraseId == PhrasePrintType.PhraseIdEDIT)
                {
                    TogglePanelVisibility(m_Form.PanelEdit);
                }

                PopulatePrinterCodeBrowse();
            }
        }
        
        /// <summary>
        /// Set Initial Printer Type
        /// </summary>
        private void SetInitialPrinterType()
        {
            if (m_Printer.DeviceType == null)
            {
                TogglePanelVisibility(m_Form.PanelLocal);
            }
            else
            {
                SetDestinationPanels(m_Printer.DeviceType);
            }
			m_LastDevice = m_Printer.DeviceType;
        }

        /// <summary>
        /// Make the current panel visible and hide the last panel.
        /// </summary>
        /// <param name="currentPanel"></param>
        private void TogglePanelVisibility(Panel currentPanel)
        {
            if (currentPanel != m_LastPanel)
            {
                currentPanel.Visible = true;
                if (m_LastPanel != null)
                    m_LastPanel.Visible = false;
                m_LastPanel = currentPanel;
            }
        }

        #endregion

        #region Browse Population Methods

        /// <summary>
        /// Populate printer queue browse
        /// </summary>
        private void PopulatePrinterQueueBrowse()
        {
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                m_Form.BrowseStringCollectionQueue.AddItem(printer);
            }
        }

        /// <summary>
        /// Populate printer code browse
        /// </summary>
        private void PopulatePrinterCodeBrowse()
        {
            m_PrinterCodeTypes = new List<string>();

            if (m_Printer.DeviceType != null && 
                (  m_Printer.DeviceType.PhraseId == PhrasePrintType.PhraseIdDEVICE || m_Printer.DeviceType.PhraseId == PhrasePrintType.PhraseIdLOCAL))
            {
                FolderList datafiles = Library.Environment.GetFolderList("smp$datafiles");

                foreach ( DirectoryInfo folder in datafiles.Folders )
                {
                    string[] files = Directory.GetFiles(folder.FullName, "*.ddf", SearchOption.AllDirectories);
                    foreach ( string file in files )
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        if (!m_PrinterCodeTypes.Contains(fileName))
                        {
                            m_PrinterCodeTypes.Add(fileName);
                        }
                    }
                }
            }
            else if (m_Printer.DeviceType != null && m_Printer.DeviceType.PhraseId == PhrasePrintType.PhraseIdQUEUE)
            {
                m_PrinterCodeTypes.Add("WINDOWS");
            }

            m_Form.BrowseStringCollectionPrinterCodes.Republish(m_PrinterCodeTypes);
            
        }

        /// <summary>
        /// Reset printer values when the device is changed.
        /// </summary>
        private void ResetPrinter(PhraseBase deviceType)
        {
			if (m_LastDevice != null && m_LastDevice != deviceType)
			{
				m_Printer.DeviceName = string.Empty;
				m_Printer.LogicalName = string.Empty;
				m_Printer.PrinterCodeType = string.Empty;
				m_Printer.PrinterParameter = string.Empty;
				m_Printer.GraphicsType = null;
				m_Printer.DistributionPersonnels.Clear();
				m_Printer.Distributions.Clear();
			}
        }
        #endregion
    }
}
