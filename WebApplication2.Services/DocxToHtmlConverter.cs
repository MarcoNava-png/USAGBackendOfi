using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace WebApplication2.Services
{
    public static class DocxToHtmlConverter
    {
        public static string Convert(byte[] docxBytes)
        {
            using var ms = new MemoryStream(docxBytes);
            using var doc = WordprocessingDocument.Open(ms, false);

            var body = doc.MainDocumentPart?.Document?.Body;
            if (body == null) return "<p>Documento vacío</p>";

            var sb = new StringBuilder();
            sb.AppendLine("<div style=\"font-family: 'Calibri', 'Arial', sans-serif; font-size: 11pt; max-width: 700px; margin: 0 auto; padding: 40px 60px; background: white; box-shadow: 0 0 10px rgba(0,0,0,0.1);\">");

            foreach (var element in body.ChildElements)
            {
                if (element is Paragraph para)
                    sb.AppendLine(ConvertParagraph(para));
                else if (element is Table table)
                    sb.AppendLine(ConvertTable(table));
            }

            sb.AppendLine("</div>");
            return sb.ToString();
        }

        private static string ConvertParagraph(Paragraph para)
        {
            var text = new StringBuilder();
            var props = para.ParagraphProperties;
            var alignment = "left";
            var fontSize = "";
            var isBold = false;
            var marginTop = "0";
            var marginBottom = "4px";

            if (props != null)
            {
                if (props.Justification?.Val?.Value == JustificationValues.Center) alignment = "center";
                else if (props.Justification?.Val?.Value == JustificationValues.Right) alignment = "right";

                if (props.SpacingBetweenLines != null)
                {
                    if (props.SpacingBetweenLines.Before != null)
                        marginTop = $"{int.Parse(props.SpacingBetweenLines.Before) / 20}px";
                    if (props.SpacingBetweenLines.After != null)
                        marginBottom = $"{int.Parse(props.SpacingBetweenLines.After) / 20}px";
                }
            }

            foreach (var run in para.Elements<Run>())
            {
                var runText = string.Concat(run.Elements<Text>().Select(t => t.Text));
                if (string.IsNullOrEmpty(runText)) continue;

                var rp = run.RunProperties;
                var styles = new List<string>();

                if (rp?.Bold != null) styles.Add("font-weight:bold");
                if (rp?.Italic != null) styles.Add("font-style:italic");
                if (rp?.Underline != null && rp.Underline.Val != null && rp.Underline.Val != UnderlineValues.None)
                    styles.Add("text-decoration:underline");

                if (rp?.FontSize?.Val != null)
                {
                    var size = int.Parse(rp.FontSize.Val) / 2;
                    styles.Add($"font-size:{size}pt");
                }

                if (rp?.Color?.Val != null)
                    styles.Add($"color:#{rp.Color.Val}");

                if (styles.Count > 0)
                    text.Append($"<span style=\"{string.Join(";", styles)}\">{Encode(runText)}</span>");
                else
                    text.Append(Encode(runText));
            }

            var content = text.ToString();
            if (string.IsNullOrWhiteSpace(content))
                return $"<p style=\"text-align:{alignment}; margin:{marginTop} 0 {marginBottom} 0;\">&nbsp;</p>";

            return $"<p style=\"text-align:{alignment}; margin:{marginTop} 0 {marginBottom} 0;\">{content}</p>";
        }

        private static string ConvertTable(Table table)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<table style=\"width:100%; border-collapse:collapse; margin:8px 0;\">");

            foreach (var row in table.Elements<TableRow>())
            {
                sb.AppendLine("<tr>");
                foreach (var cell in row.Elements<TableCell>())
                {
                    var cellContent = new StringBuilder();
                    var bgColor = "";
                    var borderStyle = "border:1px solid #000;";

                    var tcp = cell.TableCellProperties;
                    if (tcp?.Shading?.Fill != null && tcp.Shading.Fill != "auto")
                        bgColor = $"background-color:#{tcp.Shading.Fill};";

                    foreach (var para in cell.Elements<Paragraph>())
                    {
                        var pText = ConvertParagraph(para);
                        cellContent.Append(pText);
                    }

                    sb.AppendLine($"<td style=\"{borderStyle} {bgColor} padding:4px 6px; vertical-align:middle;\">{cellContent}</td>");
                }
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");
            return sb.ToString();
        }

        private static string Encode(string text)
        {
            return System.Net.WebUtility.HtmlEncode(text);
        }
    }
}
