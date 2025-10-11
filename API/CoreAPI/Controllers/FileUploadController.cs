using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using YourNamespace.Engine;
using YourNamespace.Services;
using System.Collections.Generic;
using System.Linq;

namespace BankStatementPDFToExcel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileUploadController : ControllerBase
    {
        private readonly PdfParserService _pdfParserService;

        public FileUploadController(PdfParserService pdfParserService)
        {
            _pdfParserService = pdfParserService;
        } 

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File not uploaded.");
            }

            var filePath = Path.GetTempFileName();
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var extractedText = _pdfParserService.ExtractTextFromPdf(filePath);
            return Ok(new { Text = extractedText });
        }

        [HttpPost("exportPdfToExcel")]
        public async Task<IActionResult> ExportPdfToExcel(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("File not uploaded.");
                }

                var filePath = Path.GetTempFileName();
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var extractedText = _pdfParserService.ExtractTextFromPdf(filePath);

                if (DataProcessingEngine.ValidateSignature(extractedText)){
                    // get the file stream
                    var fileStream = file.OpenReadStream();
                    
                    // whether to split the pages
                    bool splitPdf = true;

                    if (splitPdf)
                    {
                        var splitPdfs = DataProcessingEngine.SplitPdf(fileStream);
                        List<MemoryStream> excelDataStreams = new List<MemoryStream>();
                        foreach (var inputPdf in splitPdfs)
                        {
                            Spire.Pdf.PdfDocument pdf = new Spire.Pdf.PdfDocument();

                            pdf.LoadFromStream(inputPdf);

                            MemoryStream excelDataStream = new MemoryStream();
                            pdf.SaveToStream(excelDataStream, Spire.Pdf.FileFormat.XLSX);
                            excelDataStreams.Add(excelDataStream);
                        }


                        MemoryStream outputStream = new MemoryStream();
                        ExcelPackage combinedPackage = new ExcelPackage();
                        int count = 1;
                        foreach (var excelDataStream in excelDataStreams)
                        {
                            var newStream = new MemoryStream(excelDataStream.GetBuffer());
                            using (var sourcePackage = new ExcelPackage(newStream))
                            {
                                foreach (var sheet in sourcePackage.Workbook.Worksheets)
                                {
                                    combinedPackage.Workbook.Worksheets.Add(sheet.Name + "_" + count.ToString(), sheet);
                                }
                            }
                            count++;
                        }

                        await combinedPackage.SaveAsAsync(outputStream);

                        DataProcessingEngine.ExtractTable(combinedPackage);
                    }
                    else
                    {
                        Spire.Pdf.PdfDocument pdf = new Spire.Pdf.PdfDocument();

                        pdf.LoadFromStream(fileStream);

                        MemoryStream excelDataStream = new MemoryStream();
                        pdf.SaveToStream(excelDataStream, Spire.Pdf.FileFormat.XLSX);
                        pdf.SaveToFile("Output-Bank 1.xlsx", Spire.Pdf.FileFormat.XLSX);
                    }
                }
                else{
                    return ValidationProblem("File provided not a valid maybank original PDF");
                }
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ValidationProblem(ex.Message.ToString());
            }
        }

        [HttpPost("exportPdfToExcelDownload")]
        public async Task<IActionResult> ExportPdfToExcelDownload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // 🕓 Create new filename with timestamp
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var originalName = Path.GetFileNameWithoutExtension(file.FileName);
            var newFileName = $"{originalName}_{timestamp}.xlsx";

            var filePath = Path.GetTempFileName();
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var extractedText = _pdfParserService.ExtractTextFromPdf(filePath);

            if (DataProcessingEngine.ValidateSignature(extractedText)){
                // get the file stream
                var fileStream = file.OpenReadStream();
                
                // whether to split the pages
                bool splitPdf = true;

                if (splitPdf)
                {
                    var splitPdfs = DataProcessingEngine.SplitPdf(fileStream);
                    List<MemoryStream> excelDataStreams = new List<MemoryStream>();
                    foreach (var inputPdf in splitPdfs)
                    {
                        Spire.Pdf.PdfDocument pdf = new Spire.Pdf.PdfDocument();

                        pdf.LoadFromStream(inputPdf);

                        MemoryStream excelDataStream = new MemoryStream();
                        pdf.SaveToStream(excelDataStream, Spire.Pdf.FileFormat.XLSX);
                        excelDataStreams.Add(excelDataStream);
                    }


                    MemoryStream outputStream = new MemoryStream();
                    ExcelPackage combinedPackage = new ExcelPackage();
                    int count = 1;
                    foreach (var excelDataStream in excelDataStreams)
                    {
                        var newStream = new MemoryStream(excelDataStream.GetBuffer());
                        using (var sourcePackage = new ExcelPackage(newStream))
                        {
                            foreach (var sheet in sourcePackage.Workbook.Worksheets)
                            {
                                combinedPackage.Workbook.Worksheets.Add(sheet.Name + "_" + count.ToString(), sheet);
                            }
                        }
                        count++;
                    }

                    await combinedPackage.SaveAsAsync(outputStream);
                    byte[] convertedBytes;
                    convertedBytes = DataProcessingEngine.ExtractTableAndReturnByte(combinedPackage);

                    // 🧾 Return the file as a download
                    return File(
                        convertedBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // or appropriate MIME
                        newFileName
                    );
                }
                else
                {
                    Spire.Pdf.PdfDocument pdf = new Spire.Pdf.PdfDocument();

                    // pdf.LoadFromStream(fileStream);

                    // MemoryStream excelDataStream = new MemoryStream();
                    // pdf.SaveToStream(excelDataStream, Spire.Pdf.FileFormat.XLSX);
                    // pdf.SaveToFile("Output-Bank 1.xlsx", Spire.Pdf.FileFormat.XLSX);
                    return Ok();
                }
            }
            else{
                return ValidationProblem("File provided not a valid maybank original PDF");
            }
        }


       
        [HttpPost("ExtractTable")]
        public async Task<IActionResult> ExtractTable(IFormFile file)
        {
            // Load the Excel file
            using (var stream = file.OpenReadStream())
            {
                using (var package = new ExcelPackage(stream))
                {
                    // Get the workbook
                    await package.LoadAsync(stream);

                    DataProcessingEngine.ExtractTable(package);

                }
            }

            return Ok();
        }  
    }
}


