using System.Text;
using System.Text.RegularExpressions;

namespace YourCompany.UnityCopilot.Editor
{
    /// <summary>
    /// Converts markdown syntax to Unity's rich text format.
    /// Supports: bold, italic, code blocks, inline code, headers, and lists.
    /// </summary>
    public static class MarkdownToRichText
    {
        public static string Convert(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return markdown;
            }

            var sb = new StringBuilder();
            var lines = markdown.Split('\n');
            var inCodeBlock = false;
            var codeBlockContent = new StringBuilder();

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                // Handle code blocks
                if (line.TrimStart().StartsWith("```"))
                {
                    if (inCodeBlock)
                    {
                        // End code block
                        sb.Append("<color=#D4D4D4><i>");
                        sb.Append(EscapeRichText(codeBlockContent.ToString().TrimEnd('\n')));
                        sb.Append("</i></color>\n");
                        codeBlockContent.Clear();
                        inCodeBlock = false;
                    }
                    else
                    {
                        // Start code block
                        inCodeBlock = true;
                    }
                    continue;
                }

                if (inCodeBlock)
                {
                    codeBlockContent.Append(line);
                    codeBlockContent.Append('\n');
                    continue;
                }

                // Process the line for inline markdown
                line = ProcessInlineMarkdown(line);

                // Handle headers
                if (line.StartsWith("### "))
                {
                    line = "<size=16><b>" + line.Substring(4) + "</b></size>";
                }
                else if (line.StartsWith("## "))
                {
                    line = "<size=18><b>" + line.Substring(3) + "</b></size>";
                }
                else if (line.StartsWith("# "))
                {
                    line = "<size=20><b>" + line.Substring(2) + "</b></size>";
                }
                // Handle unordered lists
                else if (line.TrimStart().StartsWith("- ") || line.TrimStart().StartsWith("* "))
                {
                    var indent = line.Length - line.TrimStart().Length;
                    var indentStr = new string(' ', indent);
                    line = indentStr + "• " + line.TrimStart().Substring(2);
                }
                // Handle ordered lists
                else if (Regex.IsMatch(line.TrimStart(), @"^\d+\.\s"))
                {
                    // Keep numbered lists as-is, they render fine
                }

                sb.Append(line);
                if (i < lines.Length - 1)
                {
                    sb.Append('\n');
                }
            }

            // Close any remaining code block
            if (inCodeBlock && codeBlockContent.Length > 0)
            {
                sb.Append("<color=#D4D4D4><i>");
                sb.Append(EscapeRichText(codeBlockContent.ToString().TrimEnd('\n')));
                sb.Append("</i></color>");
            }

            return sb.ToString();
        }

        private static string ProcessInlineMarkdown(string text)
        {
            // Inline code (backticks) - handle before bold/italic to avoid conflicts
            text = Regex.Replace(text, @"`([^`]+)`", match =>
            {
                var code = EscapeRichText(match.Groups[1].Value);
                return $"<color=#D4D4D4><i>{code}</i></color>";
            });

            // Bold (** or __)
            text = Regex.Replace(text, @"\*\*(.+?)\*\*", "<b>$1</b>");
            text = Regex.Replace(text, @"__(.+?)__", "<b>$1</b>");

            // Italic (* or _)
            text = Regex.Replace(text, @"\*(.+?)\*", "<i>$1</i>");
            text = Regex.Replace(text, @"_(.+?)_", "<i>$1</i>");

            return text;
        }

        private static string EscapeRichText(string text)
        {
            // Escape Unity rich text tags so they display literally
            return text
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");
        }
    }
}

