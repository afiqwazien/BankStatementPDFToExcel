using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Text;

namespace YourNamespace.Services
{
    public class PdfParserService
    {
        public string ExtractTextFromPdf(string filePath)
        {
            using (var pdfReader = new PdfReader(filePath))
            using (var pdfDoc = new PdfDocument(pdfReader))
            {
                var text = new StringBuilder();
                for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
                {
                    var pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page));
                    text.Append(pageText);
                }
                return text.ToString();
            }
        }
    }
}
