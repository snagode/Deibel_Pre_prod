using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Server;
using Thermo.SampleManager.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.IO;
using System.Globalization;
using Thermo.SampleManager.ObjectModel;
using OfficeOpenXml;
using System.Drawing;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(BillCustomerPreviewTask))]
    public class BillCustomerPreviewTask : SampleManagerTask
    {
        private FormBillCustomerPreview _form;
        UnboundGridColumn _jobName;
        UnboundGridColumn _labId;
        UnboundGridColumn _PONumber;
        UnboundGridColumn _billingMonth;
        UnboundGridColumn _dateRecieved;
        UnboundGridColumn _customerId;
        UnboundGridColumn _customerName;
        UnboundGridColumn _MAScustomer;
        UnboundGridColumn _analysis;

        private string customerid;
        private string type;
        private static IEntityManager _entityManager;
        private IEntityCollection _MAScustomerCollection;
        private DeibelLabCustomerBase _deibelLabCustomer;
        private List<row> _rows;
        private List<column> Headers;
        DateTime _startdate;
        DateTime _enddate;
        private static string filename;
        private string maspath = @"C://test_folder//";
        private string destination = @"C://test_folder//";
        private string ImageFile = @"C:\Thermo\SampleManager\Server\SMUAT\Imprint\excel_logo.jpg";
        private string templateFilePath = @"C:\Thermo\SampleManager\Server\SMUAT\Imprint\monthlyBilling2.xlsx";


        public IEntityCollection MAScustomerCollection
        {
            get
            {
                return _MAScustomerCollection;
            }

            set
            {
                _MAScustomerCollection = value;
            }
        }

        public DeibelLabCustomerBase DeibelLabCustomer
        {
            get
            {
                return _deibelLabCustomer;
            }

            set
            {
                _deibelLabCustomer = value;
            }
        }

        public List<row> Rows
        {
            get
            {
                return _rows;
            }

            set
            {
                _rows = value;
            }
        }

        protected override void SetupTask()
        {
            _form = (FormBillCustomerPreview)FormFactory.CreateForm(typeof(FormBillCustomerPreview));
            _form.Closed += _form_Closed;
            _form.Loaded += _form_Loaded;
            _form.ButtonEdit1.Click += ButtonEdit1_Click;
            _form.ButtonEdit2.Click += ButtonEdit2_Click;
            _form.Show();
        }

        private void _form_Closed(object sender, EventArgs e)
        {
            if (_form != null)
            {
                _form.Close();
            }
        }
        public class column
        {
            public int id;
            public string name;

        }
        public class row
        {
            public int id;
            public string PO;
            public string Lab;
            public string Description;
            public List<item> Items;
        }

        public class item
        {
            public string column;
            public int val;
        }

        public class masrow
        {
            public string InvDate;
            public string Division;
            public string CustNo;
            public string PO;
            public string ColName;
            public string Totals;
        }

        private void _form_Loaded(object sender, EventArgs e)
        {
            var i = this.Context.TaskParameters;
            customerid = this.Context.TaskParameters[0]; //((CustomerToBilledVwBase)this.Context.SelectedItems.ActiveItems.First()).CustomerId;
            var startdate = this.Context.TaskParameters[1];
            var enddate = this.Context.TaskParameters[2];
            _startdate = DateTime.ParseExact(startdate, "M/d/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
            _enddate = DateTime.ParseExact(enddate, "M/d/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
            string SERVER_PATH = Library.Environment.GetFolderList("smp$textreports").ToString();
            string MAS_PATH = Library.Environment.GetFolderList("smp$masdestination").ToString();

            var query = EntityManager.CreateQuery(DeibelLabCustomerBase.EntityName);
            query.AddEquals(DeibelLabCustomerPropertyNames.CustomerId, customerid);
            DeibelLabCustomer = EntityManager.Select(query).ActiveItems.Cast<DeibelLabCustomerBase>().FirstOrDefault();

            filename = GetFileName(DeibelLabCustomer);

            BindGrid2();
        }

        private void BindGrid()
        {
            var grid = _form.UnboundGridDesign1;
            grid.BeginUpdate();
            grid.ClearRows();
            BuildColumns();

            var q = EntityManager.CreateQuery(MonthlyBillingViewBase.EntityName);
            q.AddEquals(MonthlyBillingViewPropertyNames.CustomerId, customerid);
            //q.AddGreaterThan(MonthlyBillingViewPropertyNames.DateReceived, _startdate.ToUniversalTime());
            q.AddLessThan(MonthlyBillingViewPropertyNames.DateReceived, _enddate.ToUniversalTime());
            MAScustomerCollection = EntityManager.Select(q);

            var id = "";
            foreach (MonthlyBillingViewBase item in MAScustomerCollection.ActiveItems.ToList())
            {
                UnboundGridRow row = grid.AddRow();
                row.Tag = item;
                row.SetValue(_jobName, item.JobName);
                row.SetValue(_labId, item.LabId);
                ////row.SetValue(_PONumber, item.PoNumber);
                row.SetValue(_dateRecieved, item.DateReceived);
                row.SetValue(_customerId, item.CustomerId);
                row.SetValue(_customerName, item.CustomerName);
                row.SetValue(_MAScustomer, item.MasCustomer);
                row.SetValue(_analysis, item.Analysis);
            }
            grid.EndUpdate();
            _form.ClearBusy();
        }


        private void BindGrid2()
        {
            var grid = _form.UnboundGridDesign1;

            var q = EntityManager.CreateQuery(MonthlyBillingViewBase.EntityName);
            q.AddEquals(MonthlyBillingViewPropertyNames.CustomerId, customerid);
            //q.AddGreaterThan(MonthlyBillingViewPropertyNames.DateReceived, _startdate.ToUniversalTime());
            q.AddLessThan(MonthlyBillingViewPropertyNames.DateReceived, _enddate.ToUniversalTime());
            MAScustomerCollection = EntityManager.Select(q);

            grid.BeginUpdate();
            grid.ClearRows();
            CreateMasCustomerHeadersAndRows();
            BuildColumns2();

            foreach (var item in Rows)
            {
                UnboundGridRow row = grid.AddRow();
                //row.Tag = item;
                foreach (var item2 in grid.Columns)
                {
                    if (item2.Caption == "PO")
                        row.SetValue(item2, item.PO);
                    else if (item2.Caption == "LAB NO")
                        row.SetValue(item2, item.Lab);
                    else if (item2.Caption == "Description")
                        row.SetValue(item2, item.Description);
                    else
                    {
                        var val = item.Items.Where(m => m.column == item2.Name).Select(n => n.val).FirstOrDefault();
                        row.SetValue(item2, val);
                        //item2.wi
                    }
                }
            }
            UnboundGridRow rowfooter = grid.AddRow();

            foreach (var item2 in grid.Columns)
            {
                var val = Rows.SelectMany(n => n.Items.Where(m => m.column == item2.Caption).Select(c => c.val)).Sum();
                rowfooter.SetValue(item2, val);
            }

            var coldescription = grid.Columns.Where(m => m.Caption == "Description").FirstOrDefault();
            var colLABNO = grid.Columns.Where(m => m.Caption == "LAB NO").FirstOrDefault();
            var colPO= grid.Columns.Where(m => m.Caption == "PO").FirstOrDefault();
            rowfooter.SetValue(coldescription, "Totals");
            rowfooter.SetValue(colLABNO, "");
            rowfooter.SetValue(colPO, "");

            grid.EndUpdate();
            _form.ClearBusy();
        }


        void BuildColumns2()
        {
            var grid = _form.UnboundGridDesign1;
            _jobName = grid.AddColumn(MonthlyBillingViewPropertyNames.JobName, "PO");
            _labId = grid.AddColumn(MonthlyBillingViewPropertyNames.LabId, "LAB NO");
            _PONumber = grid.AddColumn(MonthlyBillingViewPropertyNames.PoNumber, "Description");
            foreach (var item in Headers)
            {
                var maxLength = Headers.Max(x => x.name.Length);
                var columnWidth = (maxLength * 4) < 80 ? 80 : maxLength * 4;
                grid.AddColumn(item.name, item.name, width: columnWidth);
            }
        }

        void BuildColumns()
        {
            var grid = _form.UnboundGridDesign1;
            _jobName = grid.AddColumn(MonthlyBillingViewPropertyNames.JobName, "Job Name");
            _labId = grid.AddColumn(MonthlyBillingViewPropertyNames.LabId, "Lab Id");
            _PONumber = grid.AddColumn(MonthlyBillingViewPropertyNames.PoNumber, "PO Number");
            _billingMonth = grid.AddColumn(MonthlyBillingViewPropertyNames.BillingMonth, "Billing Month");
            _dateRecieved = grid.AddColumn(MonthlyBillingViewPropertyNames.DateReceived, "Date Recieved");
            _customerId = grid.AddColumn(MonthlyBillingViewPropertyNames.CustomerId, "Customer Id");
            _customerName = grid.AddColumn(MonthlyBillingViewPropertyNames.CustomerName, "Customer Name");
            _MAScustomer = grid.AddColumn(MonthlyBillingViewPropertyNames.MasCustomer, "MAS Customer");
            _analysis = grid.AddColumn(MonthlyBillingViewPropertyNames.Analysis, "Aanalysis");
        }

        private string GetFileName(DeibelLabCustomerBase deibelLabCustomerBase)
        {
            return "MAS-" + deibelLabCustomerBase.LabCode + "-" + deibelLabCustomerBase.CustomerId + "-" + _enddate.ToString("MMM") + "-" + _enddate.ToString("yyyyMMddHHmmss");
        }

        private void ButtonEdit1_Click(object sender, EventArgs e)
        {
            //CreateMasCustomerHeadersAndRows();
            //ExportData(Rows, templateFilePath);
            string excelFile = getCopyExcelTemplateFile();
            var res = WriteExcel(Rows, Headers, excelFile);
            if (res)
                Library.Utils.FlashMessage("File Generated succefully", "");
            else
                Library.Utils.FlashMessage("File Could not be created", "");
        }


        private void CreateMasCustomerHeadersAndRows()
        {
            var data = MAScustomerCollection.ActiveItems.Cast<MonthlyBillingViewBase>();
            Headers = new List<column>();

            foreach (var i in data.Select(m => new { analysis = m.Analysis }).Distinct())
            {
                Headers.Add(new column { id = 0, name = i.analysis });
            }

            Rows = new List<row>();
            row newrow;

            foreach (MonthlyBillingViewBase i in data)
            {
                newrow = new row();
                newrow.Description = i.Description;
                newrow.Lab = i.IdText;
                newrow.Items = new List<item>();

                foreach (var column in Headers)
                {
                    var rec = data.Where(m => m.Analysis == column.name && m.IdText == i.IdText).FirstOrDefault();
                    var Item = new item();
                    Item.column = column.name;
                    if (rec != null)
                    {
                        Item.val = rec.TestCount;
                    }
                    newrow.Items.Add(Item);
                }
                if (!Rows.Any(n => n.Lab == i.IdText))
                    Rows.Add(newrow);
            }

        }

        private void ButtonEdit2_Click(object sender, EventArgs e)
        {
            var query = EntityManager.CreateQuery(DeibelLabCustomerBase.EntityName);
            query.AddEquals(DeibelLabCustomerPropertyNames.CustomerId, customerid);
            DeibelLabCustomerBase deibelcustomer = EntityManager.Select(query).ActiveItems.Cast<DeibelLabCustomerBase>().FirstOrDefault();

            var q = EntityManager.CreateQuery(MasBillingViewBase.EntityName);
            q.AddEquals(MasBillingViewPropertyNames.CustomerId, customerid);
            //q.AddGreaterThan(MonthlyBillingViewPropertyNames.DateReceived, _startdate.ToUniversalTime());
            q.AddLessThan(MasBillingViewPropertyNames.DateReceived, _enddate.ToUniversalTime());
            var masview = EntityManager.Select(q);

            var data = masview.ActiveItems.Cast<MasBillingViewBase>();
            List<masrow> MASRows = new List<masrow>();

            MASRows = data.GroupBy(p => new { p.PoNumber, p.Analysis, p.CustomerId }).
                Select(b => new masrow
                {
                    CustNo = customerid,
                    Division = customerid,
                    ColName = b.Key.Analysis,
                    //, InvDate = b.Key.DateReceived.Value.ToShortDateString(),
                    PO = b.Key.PoNumber,
                    Totals = b.Count().ToString()
                }).ToList();

            var res = false;
            if (MASRows.Count > 0)
            {
                res = DownloadReport(MASRows, filename);

                if (res)
                {
                    //upcate mas_billing
                    var id = new PackedDecimal(Library.Increment.GetIncrement(TableNames.MasBilling, MasBillingPropertyNames.RecordId).ToString());
                    MasBillingBase record = (MasBillingBase)EntityManager.CreateEntity(MasBillingBase.EntityName, new Identity(id));
                    var currentUser = (Personnel)Library.Environment.CurrentUser;
                    //record.RecordId  = Library.Increment.GetIncrement(TableNames.MasBilling, MasBillingPropertyNames.RecordId);
                    record.DateCreated = DateTime.Now;
                    record.CreatedBy = currentUser;
                    record.CustomerId = deibelcustomer.Deibellabcustomer;
                    record.LabCode = deibelcustomer.LabCode;
                    record.FileName = filename;
                    record.XlFileName = destination + filename + ".xlsx";
                    record.Mode = "SINGLE";
                    record.Status = "P";
                    record.Tries = 1;
                    record.StatusMessage = "Transfer Pending";
                    record.GroupId = ((Personnel)Library.Environment.CurrentUser).GroupId.GroupId;
                    EntityManager.Transaction.Add(record);

                    //update job_header
                    foreach (var item in MAScustomerCollection.Cast<MonthlyBillingViewBase>().Select(m => m.JobName).Distinct())
                    {
                        var q1 = EntityManager.CreateQuery(JobHeader.EntityName);
                        q1.AddEquals(JobHeaderPropertyNames.JobName, item);
                        JobHeader job = EntityManager.Select(q1).ActiveItems.Cast<JobHeader>().FirstOrDefault();
                        job.BillingStatus = (PhraseBase)EntityManager.SelectPhrase(PhraseBillStat.Identity, PhraseBillStat.PhraseIdB);
                        EntityManager.Transaction.Add(job);
                    }
                    EntityManager.Commit();
                    Library.Utils.FlashMessage("File Generated succefully", "");
                    _form.Close();
                }
            }

        }

        public bool DownloadReport(List<masrow> lstData, string filename)
        {
            var sb = new StringBuilder();
            foreach (var data in lstData)
            {
                sb.AppendLine(_enddate.ToString("yyyyMMdd") + "," + DeibelLabCustomer.MasCustomer.Split('-')[0] + "," + DeibelLabCustomer.MasCustomer.Split('-')[1] + "," + data.PO + data.ColName + ", " + data.Totals);
            }
            File.WriteAllText(maspath + filename + ".csv", sb.ToString(), Encoding.UTF8);
            return true;
        }


        private string getCopyExcelTemplateFile()
        {
            string
                newFile = GetFileName(DeibelLabCustomer),
                rootPath = destination,
                templateFile = templateFilePath,
                tempFile = destination + newFile + ".xlsx";

            File.Copy(templateFile, tempFile, true);
            return tempFile;
        }


        private bool WriteExcel(List<row> Rows, List<column> Headers, string excelFile)
        {
            //using EP Plus library

            var template = new FileInfo(excelFile);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            try
            {
                FileInfo fileInfo = new FileInfo(excelFile);
                ExcelPackage p = new ExcelPackage(fileInfo);
                ExcelWorksheet myWorksheet = p.Workbook.Worksheets["Report"];

                myWorksheet.Cells[2, 3].Value = "PO";
                myWorksheet.Cells[2, 4].Value = "LAB NO";
                myWorksheet.Cells[2, 5].Value = "Description";

                for (int i = 0; i < Headers.Count; i++)
                {
                    myWorksheet.Cells[2, i + 6].Value = Headers[i].name;
                }

                for (int j = 0; j < Rows.Count; j++)
                {
                    if (!string.IsNullOrEmpty(Rows[j].PO))
                        myWorksheet.Cells[j + 3, 3].Value = Rows[j].PO;
                    else
                        myWorksheet.Cells[j + 3, 3].Value = "'";
                    myWorksheet.Cells[j + 3, 4].Value = Rows[j].Lab;
                    myWorksheet.Cells[j + 3, 5].Value = "'" + Rows[j].Description;
                    for (int k = 0; k < Rows[j].Items.Count; k++)
                    {
                        if (Rows[j].Items[k].val != 0)
                            myWorksheet.Cells[j + 3, k + 6].Value = Rows[j].Items[k].val;
                        else
                            myWorksheet.Cells[j + 3, k + 6].Value = "";
                    }
                }

                myWorksheet.Cells[Rows.Count + 3, 5].Value = "Totals";

                for (int i = 0; i < Headers.Count; i++)
                {
                    myWorksheet.Cells[Rows.Count + 3, i + 6].Value = Rows.Select(n => n.Items[i].val).Sum();
                }
                myWorksheet.Cells[Rows.Count + 3, 2, Rows.Count + 3, Headers.Count + 5].Style.Font.Bold = true;

                //myWorksheet.PrinterSettings.FitToPage = true;
                //string columnName = OfficeOpenXml.ExcelCellAddress.GetColumnLetter(myWorksheet.Dimension.End.Column);
                //string printArea = string.Format("A1:{0}{1}", columnName, myWorksheet.Dimension.End.Row);

                string one = myWorksheet.Cells[1, 2].Address;
                string two = myWorksheet.Cells[Rows.Count + 3, Headers.Count + 5].Address;
                myWorksheet.PrinterSettings.PrintArea = myWorksheet.Cells[one + ":" + two];
                Image img = Image.FromFile(ImageFile);
                myWorksheet.HeaderFooter.OddHeader.InsertPicture(img, PictureAlignment.Centered);
                myWorksheet.HeaderFooter.OddHeader.CenteredText += string.Format("\n\n&10&\"Arial,Bold\" Lab Report Summary" +
                    "\n&10&\"Arial,Regular\" {0} - {1}-{2}", DeibelLabCustomer.Deibellabcustomer.CompanyName, DeibelLabCustomer.MasCustomer.Split('-')[0], DeibelLabCustomer.MasCustomer.Split('-')[1]);
                myWorksheet.HeaderFooter.EvenHeader.InsertPicture(img, PictureAlignment.Centered);
                myWorksheet.HeaderFooter.EvenHeader.CenteredText += string.Format("\n\n&10&\"Arial,Bold\" Lab Report Summary" +
                    "\n&10&\"Arial,Regular\" {0} - {1}-{2}", DeibelLabCustomer.Deibellabcustomer.CompanyName, DeibelLabCustomer.MasCustomer.Split('-')[0], DeibelLabCustomer.MasCustomer.Split('-')[1]);

                myWorksheet.HeaderFooter.differentOddEven = false;

                //    header_txt= "&" : ASCII(34) : "Arial,Bold" :
                //           ASCII(34) : "Lab Report Summary&" : ASCII(34) :
                //           "Arial,Regular" : ASCII(34)
                //    full_header = "&G " : Chr(10) :  Chr(10) : header_txt: Chr(10) : 
                //      self.customer_name : Chr(10) : bill_month
                //&D Current date
                //&T Current time
                //&F Workbook name
                //&A Worksheet name (from the worksheet tab)
                //& P Current page number
                //& P + x Current page number plus x
                //& P - x Current page number minus x                
                //& N Total pages in the workbook
                //&& Ampersand character

                //myWorksheet.PrinterSettings.PrintArea = myWorksheet.Cells["A:1,G:30"];
                p.Save();
            }
            catch (Exception e)
            {
                return false;
            }

            //****** using open xml
            //using (var spreadSheetDocument = SpreadsheetDocument.Open(excelFile, true))
            //{
            //    //WorkbookPart workbookPart = spreadSheetDocument.WorkbookPart;
            //    //Sheet sheet = spreadSheetDocument.WorkbookPart.Workbook.Sheets.GetFirstChild<Sheet>();
            //    //DocumentFormat.OpenXml.Spreadsheet.Worksheet worksheet = (spreadSheetDocument.WorkbookPart.GetPartById(sheet.Id.Value) as WorksheetPart).Worksheet;
            //    //string relId = workbookPart.Workbook.Descendants<Sheet>().First(s => sheetName.Equals(s.Name)).Id;
            //    //return (WorksheetPart)workbookPart.GetPartById(relId);

            //    //var workSheet = ((WorksheetPart)workbookPart.GetPartById(sheetID)).Worksheet;

            //    WorkbookPart workbookPart1 = spreadSheetDocument.WorkbookPart;
            //    IEnumerable<Sheet> Sheets = spreadSheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>().Where(s => s.Name == "Report");
            //    if (Sheets.Count() == 0)
            //    {
            //        // The specified worksheet does not exist.
            //        return false;
            //    }

            //    string relationshipId = Sheets.First().Id.Value;
            //    WorksheetPart worksheetPart = (WorksheetPart)spreadSheetDocument.WorkbookPart.GetPartById(relationshipId);
            //    SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
            //    var sharedstringtablepart = workbookPart1.SharedStringTablePart;

            //    // SheetData sheetdata = worksheet.GetFirstChild<SheetData>();
            //    ExportData2(Rows, "", sheetData);
            //    // ExportData2(Rows, "", sheetData);
            //    spreadSheetDocument.WorkbookPart.Workbook.Save();
            //    spreadSheetDocument.Close();
            //}

            return true;
        }


        private void ExportData(List<row> Rows, string filename)
        {

            using (var workbook = SpreadsheetDocument.Open(templateFilePath, false))
            {
                var workbookPart = workbook.AddWorkbookPart();
                workbook.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();
                workbook.WorkbookPart.Workbook.Sheets = new DocumentFormat.OpenXml.Spreadsheet.Sheets();

                var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
                sheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);

                DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
                string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                uint sheetId = 1;
                if (sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() > 0)
                {
                    sheetId =
                        sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                }

                DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = relationshipId, SheetId = sheetId, Name = "test" };
                sheets.Append(sheet);

                DocumentFormat.OpenXml.Spreadsheet.Row blankkrow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                DocumentFormat.OpenXml.Spreadsheet.Row blankkrow1 = new DocumentFormat.OpenXml.Spreadsheet.Row();
                DocumentFormat.OpenXml.Spreadsheet.Row blankkrow2 = new DocumentFormat.OpenXml.Spreadsheet.Row();
                DocumentFormat.OpenXml.Spreadsheet.Row blankkrow3 = new DocumentFormat.OpenXml.Spreadsheet.Row();
                sheetData.Append(blankkrow);
                sheetData.Append(blankkrow1);
                sheetData.Append(blankkrow2);
                sheetData.Append(blankkrow3);

                //insert Image by specifying two range
                OpenXmlHelper.InsertImage(sheetPart, 1, 1, 3, 3, new FileStream(ImageFile, FileMode.Open));

                DocumentFormat.OpenXml.Spreadsheet.Row headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                DocumentFormat.OpenXml.Spreadsheet.Cell cellh1 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                cellh1.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                cellh1.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue("PO"); //
                headerRow.AppendChild(cellh1);
                DocumentFormat.OpenXml.Spreadsheet.Cell cellh2 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                cellh2.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                cellh2.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue("LAB NO"); //
                headerRow.AppendChild(cellh2);
                DocumentFormat.OpenXml.Spreadsheet.Cell cellh3 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                cellh3.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                cellh3.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue("Description"); //
                headerRow.AppendChild(cellh3);
                List<String> columns = new List<string>();

                foreach (var column in Headers)
                {
                    columns.Add(column.name);
                    DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                    cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                    cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(column.name);
                    double width = OpenXmlHelper.GetWidth("Calibri", 11, column.name);
                    headerRow.AppendChild(cell);
                }


                sheetData.AppendChild(headerRow);
                foreach (var row in Rows)
                {
                    DocumentFormat.OpenXml.Spreadsheet.Row newRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                    DocumentFormat.OpenXml.Spreadsheet.Cell cell1 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                    cell1.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                    cell1.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(row.PO); //
                    newRow.AppendChild(cell1);
                    DocumentFormat.OpenXml.Spreadsheet.Cell cell2 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                    cell2.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                    cell2.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(row.Lab); //
                    newRow.AppendChild(cell2);

                    DocumentFormat.OpenXml.Spreadsheet.Cell cell3 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                    cell3.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                    cell3.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(row.Description); //
                    newRow.AppendChild(cell3);

                    foreach (String col in columns)
                    {

                        var i = row.Items.Where(m => m.column == col).FirstOrDefault();
                        DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                        cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                        cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(i.val.ToString()); //
                        newRow.AppendChild(cell);
                    }

                    sheetData.AppendChild(newRow);
                }

                DocumentFormat.OpenXml.Spreadsheet.Row footerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                DocumentFormat.OpenXml.Spreadsheet.Cell cellf1 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                cellf1.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                cellf1.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(); //
                footerRow.AppendChild(cellf1);
                DocumentFormat.OpenXml.Spreadsheet.Cell cellf2 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                cellf2.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                cellf2.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(); //
                footerRow.AppendChild(cellf2);
                DocumentFormat.OpenXml.Spreadsheet.Cell cellf3 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                cellf3.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                cellf3.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue("Totals"); //
                footerRow.AppendChild(cellf3);

                foreach (String col in columns)
                {
                    var sum = Rows.SelectMany(n => n.Items.Where(m => m.column == col).Select(c => c.val)).Sum();
                    DocumentFormat.OpenXml.Spreadsheet.Cell cellf = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                    cellf.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                    cellf.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(sum.ToString()); //
                    footerRow.AppendChild(cellf);
                }

                sheetData.AppendChild(footerRow);
                var tete = sheetData.Elements<Column>();
                foreach (var column in tete.Cast<Column>().ToList())
                {
                    double width2 = OpenXmlHelper.GetWidth("Calibri", 11, column.LocalName);
                }
            }
        }

        private void ExportData2(List<row> Rows, string filename, DocumentFormat.OpenXml.Spreadsheet.SheetData sheetData)
        {
            var cell = OpenXmlHelper.FindCell(sheetData, "C4");

            cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
            cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue("PO"); //
        }
    }
}
