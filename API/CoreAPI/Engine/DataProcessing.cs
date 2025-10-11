using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using OfficeOpenXml;
using System.Text;

namespace YourNamespace.Engine
{
    public class DataProcessingEngine
    {
        public static List<Stream> SplitPdf(Stream pdfStream, int pagesPerPdf = 10)
        {
            List<Stream> splitPdfs = new List<Stream>();

            // Open the file
            PdfReader reader = new PdfReader(pdfStream);
            reader.SetUnethicalReading(true);
            using (var inputDocument = new PdfDocument(reader))
            {
                int totalPages = inputDocument.GetNumberOfPages();

                if (totalPages <= pagesPerPdf)
                {
                    //skip split
                    splitPdfs.Add(pdfStream);
                }
                else
                {
                    for (int pageNumber = 1; pageNumber <= totalPages; pageNumber += pagesPerPdf)
                    {
                        int endPage = Math.Min(pageNumber + pagesPerPdf - 1, totalPages);

                        using (var stream = new MemoryStream())
                        {
                            PdfDocument outputDocument = new PdfDocument(new PdfWriter(stream));

                            for (int i = pageNumber; i <= endPage; i++)
                            {
                                // Add the page and save it
                                var newPage = inputDocument.GetPage(i).CopyTo(outputDocument);
                                outputDocument.AddPage(newPage);
                            }

                            outputDocument.Close();

                            var newStream = new MemoryStream(stream.GetBuffer());
                            splitPdfs.Add(newStream);
                        }
                    }
                }
            }
            reader.Close();

            return splitPdfs;
        }

        public static bool ValidateSignature(string extractedText)
        {
            // Check text, only if its original pdf maybank/malayan bank statement is accepted, check which product doesnt work
            // expand for other bank statements if possible, but as of now, maintain for maybank original pdf first
            bool isOriginalMaybankFile = false;
            List<string> keywords = new List<string>(["Malayan Banking Berhad (3813-K)","TARIKH PENYATA","PROTECTED BY PIDM UP TO RM250,000"]);
            List<int> keywordIndexList = new List<int>();

            foreach (string keyword in keywords){
                keywordIndexList.Add(extractedText.IndexOf(keyword));
            }

            for (int j = 0; j < keywordIndexList.Count; j++){
                if (j > 0){
                    if (keywordIndexList[j] > keywordIndexList[j - 1]){
                        isOriginalMaybankFile = true;
                    }
                    else{
                        isOriginalMaybankFile = false;
                        break;
                    }
                }
            }
            return isOriginalMaybankFile;
        }

        public static string ExtractTable(ExcelPackage package)
        {
            try {
                // Get the workbook
                var workbook = package.Workbook;

                List<List<string>> masterTable = new List<List<string>>();

                var columnRanges = new List<(int, int)>
                {
                    (2, 3), // Date
                    (7, 16), // Description
                    (17, 20), // Amount
                    (21, 23) // Balance
                };

                string[] startingRowText = { "ENTRY DATE", "VALUE DATE" };
                string stopText = "BAKI LEGAR";
                
                // Iterate over each sheet
                foreach (var sheet in workbook.Worksheets)
                {
                    // Find the starting row
                    var startRow = FindStartingRow(sheet, startingRowText);

                    if (startRow > -1){
                        // Extract the table data
                        var tableData = ExtractTableData(sheet, startRow, stopText, columnRanges);

                        masterTable.AddRange(tableData);

                        // Print the table data
                        PrintTableData(tableData);
                    }
                    else{
                        Console.WriteLine("Starting row not found for worksheet : " + sheet);
                    }
                    
                }
                // later output and store in db/server folder, then just notify front end, 
                // then that link for front end will just download from that path
                string path = @"C:\Users\WazienTAB\Documents\Output bank table.xlsx"; 
                
                // this is only for maybank columns
                List<string> columnNames = new List<string>(["Date","Desc.1","Desc.2","Desc.3","Desc.4","Debit","Credit","Balance"]);

                masterTable = TransformTableData(masterTable, columnNames);

                OutputTableDataToExcel(masterTable, path, "BankTable", columnNames);

                return "Success"; 
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            return String.Empty;
        }

        public static byte[] ExtractTableAndReturnByte(ExcelPackage package)
        {
            try {
                // Get the workbook
                var workbook = package.Workbook;

                List<List<string>> masterTable = new List<List<string>>();

                var columnRanges = new List<(int, int)>
                {
                    (2, 3), // Date
                    (7, 16), // Description
                    (17, 20), // Amount
                    (21, 23) // Balance
                };

                string[] startingRowText = { "ENTRY DATE", "VALUE DATE" };
                string stopText = "BAKI LEGAR";
                
                // Iterate over each sheet
                foreach (var sheet in workbook.Worksheets)
                {
                    // Find the starting row
                    var startRow = FindStartingRow(sheet, startingRowText);

                    if (startRow > -1){
                        // Extract the table data
                        var tableData = ExtractTableData(sheet, startRow, stopText, columnRanges);

                        masterTable.AddRange(tableData);

                        // Print the table data
                        PrintTableData(tableData);
                    }
                    else{
                        Console.WriteLine("Starting row not found for worksheet : " + sheet);
                    }
                    
                }
                
                // this is only for maybank columns
                List<string> columnNames = new List<string>(["Date","Desc.1","Desc.2","Desc.3","Desc.4","Debit","Credit","Balance"]);

                masterTable = TransformTableData(masterTable, columnNames);

                // OutputTableDataToExcel(masterTable, path, "BankTable", columnNames);
                byte[] bytes = OutputTableDataToExcelAndReturnByte(masterTable, "BankTable", columnNames);

                return bytes; 
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            return [];
        }

        private static int FindStartingRow(ExcelWorksheet sheet, string[] textSequence)
        {
            // Iterate over each row
            for (int row = 1; row <= sheet.Dimension.End.Row; row++)
            {
                // Get the cells in the row
                var cells = sheet.Cells[row, 1, row, sheet.Dimension.End.Column];

                // Get the text values of the cells
                var textValues = cells.Select(cell => cell.Text.Trim()).ToArray();

                // Check if the text sequence is contained in the text values in the correct order
                int index = -1;
                foreach (var seq in textSequence)
                {
                    index = Array.IndexOf(textValues, textValues.FirstOrDefault(t => t.Contains(seq)));
                    if (index == -1)
                    {
                        break;
                    }
                }

                if (index != -1)
                {
                    return row + 1;
                }
            }

            // Return -1 if no matching row is found
            return -1;
        }

        // Method to extract the table data
        private static List<List<string>> ExtractTableData(ExcelWorksheet sheet, int startRow, string stopText, List<(int, int)> columnRanges)
        {
            // Initialize the table data
            var tableData = new List<List<string>>();

            // Iterate over each row
            for (int row = startRow; row <= sheet.Dimension.End.Row; row++)
            {
                // Get the cells in the row
                var cells = sheet.Cells[row, 1, row, sheet.Dimension.End.Column];

                // Get the text values of the cells
                var textValues = cells.Select(cell => cell.Text.Trim()).ToArray();

                // Check if the stop text is found
                if (textValues.Any(t => t.Contains(stopText)))
                {
                    break;
                }

                // Extract the column ranges
                var columnData = new List<string>();
                foreach (var (startColumn, endColumn) in columnRanges)
                {
                    var columnValues = cells.Where(cell => cell.Start.Column >= startColumn && cell.Start.Column <= endColumn).Select(cell => cell.Text.Trim()).ToArray();
                    columnData.Add(string.Join(" ", columnValues));
                }

                // Add the column data to the table data
                tableData.Add(columnData);
            }

            return tableData;
        }

        // Method to print the table data
        private static void PrintTableData(List<List<string>> tableData)
        {
            // Iterate over each row
            foreach (var row in tableData)
            {
                // Print the text values
                Console.WriteLine(string.Join("\t", row));
            }
        }

        private static void OutputTableDataToExcel(List<List<string>> tableData, string filePath, string worksheetName, List<string> columnNames)
        {
            try {
                // Load the existing Excel file
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    // Get the workbook
                    var workbook = package.Workbook;

                    // Check if the worksheet already exists
                    if (workbook.Worksheets.Any(ws => ws.Name == worksheetName))
                    {
                        // Delete the existing worksheet
                        workbook.Worksheets.Delete(worksheetName);
                    }

                    // Add a new worksheet
                    var worksheet = workbook.Worksheets.Add(worksheetName);

                    for (int x = 0; x < columnNames.Count; x++){
                        worksheet.Cells[1, x + 1].Value = columnNames[x];
                    }

                    // Write the table data to the worksheet
                    for (int row = 0; row < tableData.Count; row++)
                    {
                        for (int col = 0; col < tableData[row].Count; col++)
                        {
                            worksheet.Cells[row + 2, col + 1].Value = tableData[row][col];
                        }
                    }

                    // Format the output table data as a table
                    var table = worksheet.Tables.Add(worksheet.Cells[1, 1, tableData.Count + 1, tableData[0].Count], "BankTable");
                    table.TableStyle = OfficeOpenXml.Table.TableStyles.Light12;
                    table.ShowHeader = true;


                    // Save the Excel file
                    package.Save();
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error reading or writing file: " + ex.Message);
            }
            
        }
        
        private static byte[] OutputTableDataToExcelAndReturnByte(List<List<string>> tableData, string worksheetName, List<string> columnNames)
        {
            try {
                // EPPlus license requirement
                // ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add(worksheetName);

                    // Write column headers
                    for (int x = 0; x < columnNames.Count; x++)
                    {
                        worksheet.Cells[1, x + 1].Value = columnNames[x];
                    }

                    // Write table data
                    for (int row = 0; row < tableData.Count; row++)
                    {
                        for (int col = 0; col < tableData[row].Count; col++)
                        {
                            worksheet.Cells[row + 2, col + 1].Value = tableData[row][col];
                        }
                    }

                    // Format the data as a styled table
                    var table = worksheet.Tables.Add(
                        worksheet.Cells[1, 1, tableData.Count + 1, tableData[0].Count],
                        "BankTable"
                    );
                    table.TableStyle = OfficeOpenXml.Table.TableStyles.Light12;
                    table.ShowHeader = true;

                    // Auto-fit columns
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // Return Excel as byte array (in-memory)
                    return package.GetAsByteArray();
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error reading or writing file: " + ex.Message);
                return Array.Empty<byte>();
            }
            
        }

        private static List<List<string>> TransformTableData(List<List<string>> tableData, List<string> columnNames)
        {
            List<List<string>> newTableData = new List<List<string>>();

            for (int i = 0; i < tableData.Count; i++)
            {
                // Create a new row in newTableData
                var newRow = new List<string>();

                string stopKeyword = "ENDING BALANCE";

                if (tableData[i][1].Trim().Contains(stopKeyword))
                {
                    break;
                }

                if (!String.IsNullOrEmpty(tableData[i][0].Trim()))
                {
                    // Add the row to newRow
                    newRow.Add(tableData[i][0]);
                    newRow.Add(tableData[i][1]);

                    // Check the next lines to see if the line continues
                    for (int j = i + 1; j < tableData.Count; j++)
                    {
                        // If the next line is not empty, add it to newRow
                        if (string.IsNullOrEmpty(tableData[j][0].Trim()) && !string.IsNullOrEmpty(tableData[j][1].Trim()))
                        {
                            if (tableData[j][1].Trim().Contains(stopKeyword))
                            {
                                break;
                            }
                            newRow.Add(tableData[j][1]);
                        }
                        else if (String.IsNullOrEmpty(tableData[j][1].Trim()))
                        {
                            //continue
                        }
                        else
                        {
                            var rowCount = newRow.Count();
                            for (int z = rowCount; z < 5; z++)
                            {
                                newRow.Add(String.Empty);
                            }
                            // If the next line is empty, break the loop
                            break;
                        }
                    }

                    if (tableData[i][2].Contains('-'))
                    {
                        newRow.Add(tableData[i][2].Split('-')[0].Trim());
                        newRow.Add(String.Empty);
                    }
                    else
                    {
                        newRow.Add(String.Empty);
                        newRow.Add(tableData[i][2].Split('+')[0].Trim());
                    }
                    newRow.Add(tableData[i][3]);

                    newTableData.Add(newRow);
                }
            }
            return newTableData;
        }
    }
}
