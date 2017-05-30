using System;
using System.Text.RegularExpressions;

namespace MarkdownToHTML
{
    public class MarkdownParser
    {
        private string _markdown { get; set; }

        public MarkdownParser(string markdown)
        {
            _markdown = markdown;
        }

        private string Parse(string markdown, string delimiter, string tag)
        {
            string pattern = delimiter + "(.+)" + delimiter;
            string replacement = "<" + tag + ">$1</" + tag + ">";
            return Regex.Replace(markdown, pattern, replacement);
        }

        private string Parse__(string markdown) => Parse(markdown, "__", "strong");

        private string Parse_(string markdown) => Parse(markdown, "_", "em");

        private string ParseText(string markdown)
        {
            string parsedText = Parse_(Parse__((markdown)));

            if (_previousLineIsList)
            {
                return parsedText;
            }
            else
            {
                return Utils.Wrap(parsedText, "p");
            }
        }


        private static int GetHeaderLevel(string line)
        {
            int count = 0;

            foreach (char character in line)
            {
                if (character == '#')
                    count++;
                else
                    break;
            }

            return count;
        }


        private string ParseHeader(string markdown)
        {
            int headerLevel = GetHeaderLevel(markdown);
            string headerTag = "h" + headerLevel;
            string html = Utils.Wrap(markdown.Substring(headerLevel + 1), headerTag);

            return html;
        }

        private string ParseListItem(string markdown)
        {
            string html = Utils.Wrap(ParseText(markdown.Substring(2)), "li");

            if (_previousLineIsList)
                return html;
            else
                return "<ul>" + html;
        }

        private string ParseParagraph(string markdown)
        {
            if (!_previousLineIsList)
                return ParseText(markdown);
            else
                return "</ul>" + ParseText(markdown);
        }

        private bool _previousLineIsList;

        private string ParseLine(string markdown)
        {
            if (Utils.MarkdownLineIsHeader(markdown))
            {
                _previousLineIsList = false;
                return ParseHeader(markdown);
            }

            else if (Utils.MarkdownLineIsListItem(markdown))
            {
                _previousLineIsList = true;
                return ParseListItem(markdown);
            }

            else
            {
                _previousLineIsList = false;
                return ParseParagraph(markdown);
            }

            throw new ArgumentException("Invalid markdown");
        }

        public string Parse()
        {
            if (!string.IsNullOrEmpty(_markdown))
            {
                string[] lines = _markdown.Split('\n');
                string result = "";

                foreach (var line in lines)
                {
                    string lineResult = ParseLine(line);
                    result += lineResult;
                }

                if (_previousLineIsList)
                    return result + "</ul>";
                else
                    return result;
            }

            return null;
        }
    }
}