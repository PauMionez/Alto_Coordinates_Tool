using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace Alto_Coordinates_Viewer.Services
{
    class TagChangeColor : DocumentColorizingTransformer
    {
        // Match URLs (adjust the pattern to match more types of URLs)
        private readonly Regex sLinkRegex = new Regex(@"https?:\/\/[^\s""<>]+", RegexOptions.Compiled);

        // Match XML tags
        private readonly Regex sTagRegex = new Regex(@"<\??/?[\w:.-]+", RegexOptions.Compiled);

        // Match XML tags with attributes endtag
        private readonly Regex sTagEndRegex = new Regex(@"(\?>|/>|>)", RegexOptions.Compiled);

        // Match attribute names (before = )
        private readonly Regex sAttributeRegex = new Regex(@"[\w:.-]+(?=\s*=)", RegexOptions.Compiled);

        // Match attribute values (inside quotes)
        private readonly Regex sAttributeValueRegex = new Regex("\"([^\"]*)\"", RegexOptions.Compiled);

        // Match the ="
        private readonly Regex sEqualQuoteRegex = new Regex(@"=\s*[""']", RegexOptions.Compiled);

        // Match text content inside tags
        private readonly Regex sTextContentRegex = new Regex(@">(.*?)<", RegexOptions.Compiled);

        protected override void ColorizeLine(DocumentLine line)
        {
            string text = CurrentContext.Document.GetText(line);

            // Tag names
            foreach (Match match in sTagRegex.Matches(text))
                SetColor(line, match, Color.FromRgb(86, 156, 214));

            // Tag endings
            foreach (Match match in sTagEndRegex.Matches(text))
                SetColor(line, match, Color.FromRgb(86, 156, 214));

            // Attribute names
            foreach (Match match in sAttributeRegex.Matches(text))
            {
                SetColor(line, match, Color.FromRgb(156, 220, 254));
            }

            // Attribute values (inside quotes)
            foreach (Match match in sAttributeValueRegex.Matches(text))
            {

                // Check if the attribute value is a URL
                if (sLinkRegex.IsMatch(match.Value))
                {
                    //Console.WriteLine("Found URL: " + match.Value); // Debugging the matched URL
                    SetColor(line, match, Color.FromRgb(255, 192, 0)); // Yellowish-Orange
                }
                else
                {
                    SetColor(line, match, Color.FromRgb(206, 145, 120));
                }
            }

            // Equal-Quote signs (=")
            foreach (Match match in sEqualQuoteRegex.Matches(text))
                SetColor(line, match, Color.FromRgb(206, 145, 120));

            // Text inside tags (content)
            foreach (Match match in sTextContentRegex.Matches(text))
                SetColor(line, match, Color.FromRgb(212, 212, 212));
        }

        // Helper to set the color
        private void SetColor(DocumentLine line, Match match, Color color)
        {
            if (match.Success)
            {
                int start = line.Offset + match.Index;
                int end = start + match.Length;
                ChangeLinePart(start, end, (visualElement) =>
                {
                    visualElement.TextRunProperties.SetForegroundBrush(new SolidColorBrush(color));
                    visualElement.TextRunProperties.SetTextDecorations(null); // no underline
                });
            }
        }
    }
}
