using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using A = DocumentFormat.OpenXml.Drawing;
using Xdr = DocumentFormat.OpenXml.Drawing.Spreadsheet;
using A14 = DocumentFormat.OpenXml.Office2010.Drawing;
//using DocumentFormat.OpenXml.Extensions;
using System.Text.RegularExpressions;
using System.IO;

namespace Customization.Tasks
{
    public static class OpenXmlHelper
    {
        public static Cell FindCell(SheetData sheetData, string addressName)
        {
            return sheetData.Descendants<Cell>().
              Where(c => c.CellReference == addressName).FirstOrDefault();
        }

        // Retrieve the value of a cell, given a file name, sheet name, https://docs.microsoft.com/en-us/office/open-xml/how-to-retrieve-the-values-of-cells-in-a-spreadsheet
        // and address name.
        public static string GetCellValue(string fileName,
            string sheetName,
            string addressName)
        {
            string value = null;

            // Open the spreadsheet document for read-only access.
            using (SpreadsheetDocument document =
                SpreadsheetDocument.Open(fileName, false))
            {
                // Retrieve a reference to the workbook part.
                WorkbookPart wbPart = document.WorkbookPart;

                // Find the sheet with the supplied name, and then use that 
                // Sheet object to retrieve a reference to the first worksheet.
                Sheet theSheet = wbPart.Workbook.Descendants<Sheet>().
                  Where(s => s.Name == sheetName).FirstOrDefault();

                // Throw an exception if there is no sheet.
                if (theSheet == null)
                {
                    throw new ArgumentException("sheetName");
                }

                // Retrieve a reference to the worksheet part.
                WorksheetPart wsPart =
                    (WorksheetPart)(wbPart.GetPartById(theSheet.Id));

                // Use its Worksheet property to get a reference to the cell 
                // whose address matches the address you supplied.
                Cell theCell = wsPart.Worksheet.Descendants<Cell>().
                  Where(c => c.CellReference == addressName).FirstOrDefault();

                // If the cell does not exist, return an empty string.
                if (theCell.InnerText.Length > 0)
                {
                    value = theCell.InnerText;

                    // If the cell represents an integer number, you are done. 
                    // For dates, this code returns the serialized value that 
                    // represents the date. The code handles strings and 
                    // Booleans individually. For shared strings, the code 
                    // looks up the corresponding value in the shared string 
                    // table. For Booleans, the code converts the value into 
                    // the words TRUE or FALSE.
                    if (theCell.DataType != null)
                    {
                        switch (theCell.DataType.Value)
                        {
                            case CellValues.SharedString:

                                // For shared strings, look up the value in the
                                // shared strings table.
                                var stringTable =
                                    wbPart.GetPartsOfType<SharedStringTablePart>()
                                    .FirstOrDefault();

                                // If the shared string table is missing, something 
                                // is wrong. Return the index that is in
                                // the cell. Otherwise, look up the correct text in 
                                // the table.
                                if (stringTable != null)
                                {
                                    value =
                                        stringTable.SharedStringTable
                                        .ElementAt(int.Parse(value)).InnerText;
                                }
                                break;

                            case CellValues.Boolean:
                                switch (value)
                                {
                                    case "0":
                                        value = "FALSE";
                                        break;
                                    default:
                                        value = "TRUE";
                                        break;
                                }
                                break;
                        }
                    }
                }
            }
            return value;
        }

        // https://csharp.hotexamples.com/examples/DocumentFormat.OpenXml.Packaging/SharedStringTablePart/-/php-sharedstringtablepart-class-examples.html
        private static int InsertSharedStringItem(string text, SharedStringTablePart shareStringPart)
        {
            // If the part does not contain a SharedStringTable, create one.
            if (shareStringPart.SharedStringTable == null)
            {
                shareStringPart.SharedStringTable = new SharedStringTable();
            }

            int i = 0;

            // Iterate through all the items in the SharedStringTable. If the text already exists, return its index.
            foreach (SharedStringItem item in shareStringPart.SharedStringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == text)
                {
                    return i;
                }

                i++;
            }

            // The text does not exist in the part. Create the SharedStringItem and return its index.
            shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new DocumentFormat.OpenXml.Spreadsheet.Text(text)));
            shareStringPart.SharedStringTable.Save();

            return i;
        }



        //}


        //private Row GetRow(DocumentFormat.OpenXml.Spreadsheet.Worksheet worksheet, uint rowIndex)
        //{
        //    Row row = worksheet.GetFirstChild<Row>().
        //    Elements().FirstOrDefault(r => r.in == rowIndex);
        //    if (row == null)
        //    {
        //        throw new ArgumentException(String.Format("No row with index {0} found in spreadsheet", rowIndex));
        //    }
        //    return row;
        //}

        public static void CreateSpreadsheetWorkbook(string filepath)
        {
            // Create a spreadsheet document by supplying the filepath.
            // By default, AutoSave = true, Editable = true, and Type = xlsx.
            SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.
                Create(filepath, SpreadsheetDocumentType.Workbook);

            // Add a WorkbookPart to the document.
            WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
            workbookpart.Workbook = new Workbook();

            // Add a WorksheetPart to the WorkbookPart.
            WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(new SheetData());

            // Add Sheets to the Workbook.
            Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.
                AppendChild<Sheets>(new Sheets());

            // Append a new worksheet and associate it with the workbook.
            Sheet sheet = new Sheet()
            {
                Id = spreadsheetDocument.WorkbookPart.
                GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "mySheet"
            };
            sheets.Append(sheet);


            DocumentFormat.OpenXml.Spreadsheet.Row headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();

            //List<String> columns = new List<string>();
            //foreach (var item in  MAScustomerCollection.ActiveItems)
            //{
            //    columns.Add();

            //    DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
            //    cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
            //    cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(column.ColumnName);
            //    headerRow.AppendChild(cell);
            //}


            workbookpart.Workbook.Save();

            // Close the document.
            spreadsheetDocument.Close();
        }

        public static double GetWidth(string font, int fontSize, string text)
        {
            System.Drawing.Font stringFont = new System.Drawing.Font(font, fontSize);
            return GetWidth(stringFont, text);
        }

        public static double GetWidth(System.Drawing.Font stringFont, string text)
        {
            // This formula is based on this article plus a nudge ( + 0.2M )
            // http://msdn.microsoft.com/en-us/library/documentformat.openxml.spreadsheet.column.width.aspx
            // Truncate(((256 * Solve_For_This + Truncate(128 / 7)) / 256) * 7) = DeterminePixelsOfString

            Size textSize = TextRenderer.MeasureText(text, stringFont);
            double width = (double)(((textSize.Width / (double)7) * 256) - (128 / 7)) / 256;
            width = (double)decimal.Round((decimal)width + 0.2M, 2);

            return width;
        }

        private static Column CreateColumnData(UInt32 StartColumnIndex, UInt32 EndColumnIndex, double ColumnWidth)
        {
            Column column;
            column = new Column();
            column.Min = StartColumnIndex;
            column.Max = EndColumnIndex;
            column.Width = ColumnWidth;
            column.CustomWidth = true;
            return column;
        }

        // Given a Worksheet and a cell name, verifies that the specified cell exists.
        // If it does not exist, creates a new cell. 
        private static Cell CreateSpreadsheetCellIfNotExist(DocumentFormat.OpenXml.Spreadsheet.Worksheet worksheet, string cellName)
        {
            string columnName = GetColumnName(cellName);
            uint rowIndex = GetRowIndex(cellName);

            IEnumerable<Row> rows = worksheet.Descendants<Row>().Where(r => r.RowIndex.Value == rowIndex);
            Cell cell;

            // If the Worksheet does not contain the specified row, create the specified row.
            // Create the specified cell in that row, and insert the row into the Worksheet.
            if (rows.Count() == 0)
            {
                Row row = new Row() { RowIndex = new UInt32Value(rowIndex) };
                cell = new Cell() { CellReference = new StringValue(cellName) };
                row.Append(cell);
                worksheet.Descendants<SheetData>().First().Append(row);
                worksheet.Save();
            }
            else
            {
                Row row = rows.First();

                IEnumerable<Cell> cells = row.Elements<Cell>().Where(c => c.CellReference.Value == cellName);

                // If the row does not contain the specified cell, create the specified cell.
                if (cells.Count() == 0)
                {
                    cell = new Cell() { CellReference = new StringValue(cellName) };
                    row.Append(cell);
                    worksheet.Save();
                }
                else
                    cell = cells.First();
            }

            return cell;
        }

        // Given a cell name, parses the specified cell to get the column name.
        private static string GetColumnName(string cellName)
        {
            // Create a regular expression to match the column name portion of the cell name.
            Regex regex = new Regex("[A-Za-z]+");
            Match match = regex.Match(cellName);

            return match.Value;
        }

        // Given a cell name, parses the specified cell to get the row index.
        private static uint GetRowIndex(string cellName)
        {
            // Create a regular expression to match the row index portion the cell name.
            Regex regex = new Regex(@"\d+");
            Match match = regex.Match(cellName);

            return uint.Parse(match.Value);
        }

        public static void InsertImage(WorksheetPart sheet1, int startRowIndex, int startColumnIndex, int endRowIndex, int endColumnIndex, Stream imageStream)
        {
            //Inserting a drawing element in worksheet
            //Make sure that the relationship id is same for drawing element in worksheet and its relationship part
            int drawingPartId = GetNextRelationShipID(sheet1);
            Drawing drawing1 = new Drawing() { Id = "rId" + drawingPartId.ToString() };

            //Check whether the WorksheetPart contains VmlDrawingParts (LegacyDrawing element)
            if (sheet1.VmlDrawingParts == null)
            {
                //if there is no VMLDrawing part (LegacyDrawing element) exists, just append the drawing part to the sheet
                sheet1.Worksheet.Append(drawing1);
            }
            else
            {
                //if VmlDrawingPart (LegacyDrawing element) exists, then find the index of legacy drawing in the sheet and inserts the new drawing element before VMLDrawing part
                int legacyDrawingIndex = GetIndexofLegacyDrawing(sheet1);
                if (legacyDrawingIndex != -1)
                    sheet1.Worksheet.InsertAt<OpenXmlElement>(drawing1, legacyDrawingIndex);
                else
                    sheet1.Worksheet.Append(drawing1);
            }
            //Adding the drawings.xml part
            DrawingsPart drawingsPart1 = sheet1.AddNewPart<DrawingsPart>("rId" + drawingPartId.ToString());
            GenerateDrawingsPart1Content(drawingsPart1, startRowIndex, startColumnIndex, endRowIndex, endColumnIndex);
            //Adding the image
            ImagePart imagePart1 = drawingsPart1.AddNewPart<ImagePart>("image/jpeg", "rId1");
            imagePart1.FeedData(imageStream);
        }

        /// <summary>
        /// Get the index of legacy drawing element in the specified WorksheetPart
        /// </summary>
        /// <param name="sheet1">The worksheetPart</param>
        /// <returns>Index of legacy drawing</returns>
        private static int GetIndexofLegacyDrawing(WorksheetPart sheet1)
        {
            for (int i = 0; i < sheet1.Worksheet.ChildElements.Count; i++)
            {
                OpenXmlElement element = sheet1.Worksheet.ChildElements[i];
                if (element is LegacyDrawing)
                    return i;
            }
            return -1;
        }
        /// <summary>
        /// Returns the WorksheetPart for the specified sheet name
        /// </summary>
        /// <param name="workbookpart">The WorkbookPart</param>
        /// <param name="sheetName">The name of the worksheet</param>
        /// <returns>Returns the WorksheetPart for the specified sheet name</returns>
        private static WorksheetPart GetSheetByName(WorkbookPart workbookpart, string sheetName)
        {
            foreach (WorksheetPart sheetPart in workbookpart.WorksheetParts)
            {
                string uri = sheetPart.Uri.ToString();
                if (uri.EndsWith(sheetName + ".xml"))
                    return sheetPart;
            }
            return null;
        }
        /// <summary>
        /// Returns the next relationship id for the specified WorksheetPart
        /// </summary>
        /// <param name="sheet1">The worksheetPart</param>
        /// <returns>Returns the next relationship id </returns>
        private static int GetNextRelationShipID(WorksheetPart sheet1)
        {
            int nextId = 0;
            List<int> ids = new List<int>();
            foreach (IdPartPair part in sheet1.Parts)
            {
                ids.Add(int.Parse(part.RelationshipId.Replace("rId", string.Empty)));
            }
            if (ids.Count > 0)
                nextId = ids.Max() + 1;
            else
                nextId = 1;
            return nextId;
        }

        // Generates content of drawingsPart1.
        private static void GenerateDrawingsPart1Content(DrawingsPart drawingsPart1, int startRowIndex, int startColumnIndex, int endRowIndex, int endColumnIndex)
        {
            Xdr.WorksheetDrawing worksheetDrawing1 = new Xdr.WorksheetDrawing();
            worksheetDrawing1.AddNamespaceDeclaration("xdr", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
            worksheetDrawing1.AddNamespaceDeclaration("a", "http://schemas.openxmlformats.org/drawingml/2006/main");

            Xdr.TwoCellAnchor twoCellAnchor1 = new Xdr.TwoCellAnchor() { EditAs = Xdr.EditAsValues.OneCell };

            Xdr.FromMarker fromMarker1 = new Xdr.FromMarker();
            Xdr.ColumnId columnId1 = new Xdr.ColumnId();
            columnId1.Text = startColumnIndex.ToString();
            Xdr.ColumnOffset columnOffset1 = new Xdr.ColumnOffset();
            columnOffset1.Text = "38100";
            Xdr.RowId rowId1 = new Xdr.RowId();
            rowId1.Text = startRowIndex.ToString();
            Xdr.RowOffset rowOffset1 = new Xdr.RowOffset();
            rowOffset1.Text = "0";

            fromMarker1.Append(columnId1);
            fromMarker1.Append(columnOffset1);
            fromMarker1.Append(rowId1);
            fromMarker1.Append(rowOffset1);

            Xdr.ToMarker toMarker1 = new Xdr.ToMarker();
            Xdr.ColumnId columnId2 = new Xdr.ColumnId();
            columnId2.Text = endColumnIndex.ToString();
            Xdr.ColumnOffset columnOffset2 = new Xdr.ColumnOffset();
            columnOffset2.Text = "542925";
            Xdr.RowId rowId2 = new Xdr.RowId();
            rowId2.Text = endRowIndex.ToString();
            Xdr.RowOffset rowOffset2 = new Xdr.RowOffset();
            rowOffset2.Text = "161925";

            toMarker1.Append(columnId2);
            toMarker1.Append(columnOffset2);
            toMarker1.Append(rowId2);
            toMarker1.Append(rowOffset2);

            Xdr.Picture picture1 = new Xdr.Picture();

            Xdr.NonVisualPictureProperties nonVisualPictureProperties1 = new Xdr.NonVisualPictureProperties();
            Xdr.NonVisualDrawingProperties nonVisualDrawingProperties1 = new Xdr.NonVisualDrawingProperties() { Id = (UInt32Value)2U, Name = "Picture 1" };

            Xdr.NonVisualPictureDrawingProperties nonVisualPictureDrawingProperties1 = new Xdr.NonVisualPictureDrawingProperties();
            A.PictureLocks pictureLocks1 = new A.PictureLocks() { NoChangeAspect = true };

            nonVisualPictureDrawingProperties1.Append(pictureLocks1);

            nonVisualPictureProperties1.Append(nonVisualDrawingProperties1);
            nonVisualPictureProperties1.Append(nonVisualPictureDrawingProperties1);

            Xdr.BlipFill blipFill1 = new Xdr.BlipFill();

            A.Blip blip1 = new A.Blip() { Embed = "rId1", CompressionState = A.BlipCompressionValues.Print };
            blip1.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");

            A.BlipExtensionList blipExtensionList1 = new A.BlipExtensionList();

            A.BlipExtension blipExtension1 = new A.BlipExtension() { Uri = "{28A0092B-C50C-407E-A947-70E740481C1C}" };

            A14.UseLocalDpi useLocalDpi1 = new A14.UseLocalDpi() { Val = false };
            useLocalDpi1.AddNamespaceDeclaration("a14", "http://schemas.microsoft.com/office/drawing/2010/main");

            blipExtension1.Append(useLocalDpi1);

            blipExtensionList1.Append(blipExtension1);

            blip1.Append(blipExtensionList1);

            A.Stretch stretch1 = new A.Stretch();
            A.FillRectangle fillRectangle1 = new A.FillRectangle();

            stretch1.Append(fillRectangle1);

            blipFill1.Append(blip1);
            blipFill1.Append(stretch1);

            Xdr.ShapeProperties shapeProperties1 = new Xdr.ShapeProperties();

            A.Transform2D transform2D1 = new A.Transform2D();
            A.Offset offset1 = new A.Offset() { X = 1257300L, Y = 762000L };
            A.Extents extents1 = new A.Extents() { Cx = 2943225L, Cy = 2257425L };

            transform2D1.Append(offset1);
            transform2D1.Append(extents1);

            A.PresetGeometry presetGeometry1 = new A.PresetGeometry() { Preset = A.ShapeTypeValues.Rectangle };
            A.AdjustValueList adjustValueList1 = new A.AdjustValueList();

            presetGeometry1.Append(adjustValueList1);

            shapeProperties1.Append(transform2D1);
            shapeProperties1.Append(presetGeometry1);

            picture1.Append(nonVisualPictureProperties1);
            picture1.Append(blipFill1);
            picture1.Append(shapeProperties1);
            Xdr.ClientData clientData1 = new Xdr.ClientData();

            twoCellAnchor1.Append(fromMarker1);
            twoCellAnchor1.Append(toMarker1);
            twoCellAnchor1.Append(picture1);
            twoCellAnchor1.Append(clientData1);

            worksheetDrawing1.Append(twoCellAnchor1);

            drawingsPart1.WorksheetDrawing = worksheetDrawing1;
        }


        //private void ExportData(List<row> Rows, string filename)
        //{

        //    using (var workbook = SpreadsheetDocument.Open(templateFilePath, false))
        //    {
        //        var workbookPart = workbook.AddWorkbookPart();
        //        workbook.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();
        //        workbook.WorkbookPart.Workbook.Sheets = new DocumentFormat.OpenXml.Spreadsheet.Sheets();

        //        var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
        //        var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
        //        sheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);

        //        DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
        //        string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

        //        uint sheetId = 1;
        //        if (sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() > 0)
        //        {
        //            sheetId =
        //                sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Select(s => s.SheetId.Value).Max() + 1;
        //        }

        //        DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = relationshipId, SheetId = sheetId, Name = "test" };
        //        sheets.Append(sheet);

        //        DocumentFormat.OpenXml.Spreadsheet.Row blankkrow = new DocumentFormat.OpenXml.Spreadsheet.Row();
        //        DocumentFormat.OpenXml.Spreadsheet.Row blankkrow1 = new DocumentFormat.OpenXml.Spreadsheet.Row();
        //        DocumentFormat.OpenXml.Spreadsheet.Row blankkrow2 = new DocumentFormat.OpenXml.Spreadsheet.Row();
        //        DocumentFormat.OpenXml.Spreadsheet.Row blankkrow3 = new DocumentFormat.OpenXml.Spreadsheet.Row();
        //        sheetData.Append(blankkrow);
        //        sheetData.Append(blankkrow1);
        //        sheetData.Append(blankkrow2);
        //        sheetData.Append(blankkrow3);

        //        //insert Image by specifying two range
        //        OpenXmlHelper.InsertImage(sheetPart, 1, 1, 3, 3, new FileStream(ImageFile, FileMode.Open));

        //        DocumentFormat.OpenXml.Spreadsheet.Row headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
        //        DocumentFormat.OpenXml.Spreadsheet.Cell cellh1 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //        cellh1.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //        cellh1.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue("PO"); //
        //        headerRow.AppendChild(cellh1);
        //        DocumentFormat.OpenXml.Spreadsheet.Cell cellh2 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //        cellh2.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //        cellh2.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue("LAB NO"); //
        //        headerRow.AppendChild(cellh2);
        //        DocumentFormat.OpenXml.Spreadsheet.Cell cellh3 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //        cellh3.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //        cellh3.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue("Description"); //
        //        headerRow.AppendChild(cellh3);
        //        List<String> columns = new List<string>();

        //        foreach (var column in Headers)
        //        {
        //            columns.Add(column.name);
        //            DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //            cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //            cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(column.name);
        //            double width = OpenXmlHelper.GetWidth("Calibri", 11, column.name);
        //            headerRow.AppendChild(cell);
        //        }


        //        sheetData.AppendChild(headerRow);
        //        foreach (var row in Rows)
        //        {
        //            DocumentFormat.OpenXml.Spreadsheet.Row newRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
        //            DocumentFormat.OpenXml.Spreadsheet.Cell cell1 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //            cell1.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //            cell1.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(row.PO); //
        //            newRow.AppendChild(cell1);
        //            DocumentFormat.OpenXml.Spreadsheet.Cell cell2 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //            cell2.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //            cell2.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(row.Lab); //
        //            newRow.AppendChild(cell2);

        //            DocumentFormat.OpenXml.Spreadsheet.Cell cell3 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //            cell3.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //            cell3.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(row.Description); //
        //            newRow.AppendChild(cell3);

        //            foreach (String col in columns)
        //            {

        //                var i = row.Items.Where(m => m.column == col).FirstOrDefault();
        //                DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //                cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //                cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(i.val.ToString()); //
        //                newRow.AppendChild(cell);
        //            }

        //            sheetData.AppendChild(newRow);
        //        }

        //        DocumentFormat.OpenXml.Spreadsheet.Row footerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
        //        DocumentFormat.OpenXml.Spreadsheet.Cell cellf1 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //        cellf1.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //        cellf1.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(); //
        //        footerRow.AppendChild(cellf1);
        //        DocumentFormat.OpenXml.Spreadsheet.Cell cellf2 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //        cellf2.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //        cellf2.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(); //
        //        footerRow.AppendChild(cellf2);
        //        DocumentFormat.OpenXml.Spreadsheet.Cell cellf3 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //        cellf3.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //        cellf3.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue("Totals"); //
        //        footerRow.AppendChild(cellf3);

        //        foreach (String col in columns)
        //        {
        //            var sum = Rows.SelectMany(n => n.Items.Where(m => m.column == col).Select(c => c.val)).Sum();
        //            DocumentFormat.OpenXml.Spreadsheet.Cell cellf = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //            cellf.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //            cellf.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(sum.ToString()); //
        //            footerRow.AppendChild(cellf);
        //        }

        //        sheetData.AppendChild(footerRow);
        //        var tete = sheetData.Elements<Column>();
        //        foreach (var column in tete.Cast<Column>().ToList())
        //        {
        //            double width2 = OpenXmlHelper.GetWidth("Calibri", 11, column.LocalName);
        //        }
        //    }
        //}


        //private void ExportData2(List<row> Rows, string filename, DocumentFormat.OpenXml.Spreadsheet.SheetData sheetData)
        //{

        //    var cell = OpenXmlHelper.FindCell(sheetData, "C4");

        //    cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //    cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue("PO"); //

        //}
    }
}
