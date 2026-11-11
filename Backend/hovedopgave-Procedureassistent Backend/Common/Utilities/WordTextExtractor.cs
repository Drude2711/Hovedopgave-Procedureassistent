using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Common.Utilities;

public static class WordTextExtractor
{
    public static string Extract(Stream docx)
    {
        using WordprocessingDocument document = WordprocessingDocument.Open(docx, false);
        Body? body = document.MainDocumentPart?.Document.Body;
        if (body is null)
        {
            return string.Empty;
        }

        IEnumerable<string> paragraphs = body.Descendants<Paragraph>()
            .Select(p => p.InnerText.Trim())
            .Where(t => t.Length > 0);

        return string.Join("\n", paragraphs);
    }
}
