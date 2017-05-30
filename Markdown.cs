using System;
using System.Text.RegularExpressions;

public static class Markdown
{
    private static string Wrap(string text, string tag) => "<" + tag + ">" + text + "</" + tag + ">";

    private static bool IsTag(string text, string tag) => text.StartsWith("<" + tag + ">");

    private static string Parse(string markdown, string delimiter, string tag)
    {
        string pattern = delimiter + "(.+)" + delimiter;
        string replacement = "<" + tag + ">$1</" + tag + ">";
        return Regex.Replace(markdown, pattern, replacement);
    }

    private static string Parse__(string markdown) => Parse(markdown, "__", "strong");

    private static string Parse_(string markdown) => Parse(markdown, "_", "em");

    private static string ParseText(string markdown, bool list)
    {
        string parsedText = Parse_(Parse__((markdown)));

        if (list)
        {
            return parsedText;
        }
        else
        {
            return Wrap(parsedText, "p");
        }
    }

    private static string ParseHeader(string markdown, bool list, out bool inListAfter)
    {
        int count = 0;

        for (int i = 0; i < markdown.Length; i++)
        {
            if (markdown[i] == '#')
            {
                count += 1;
            }
            else
            {
                break;
            }
        }

        if (count == 0)
        {
            inListAfter = list;
            return null;
        }

        string headerTag = "h" + count;
        string headerHtml = Wrap(markdown.Substring(count + 1), headerTag);

        inListAfter = false;

        if (list)
            return "</ul>" + headerHtml;
        else
            return headerHtml;
    }

    private static string ParseLineItem(string markdown, bool list, out bool inListAfter)
    {
        if (markdown.StartsWith("*"))
        {
            string innerHtml = Wrap(ParseText(markdown.Substring(2), true), "li");
            inListAfter = true;

            if (list)
                return innerHtml;
            else
                return "<ul>" + innerHtml;
        }

        inListAfter = list;
        return null;
    }

    private static string ParseParagraph(string markdown, bool list, out bool inListAfter)
    {
        inListAfter = false;

        if (!list)
            return ParseText(markdown, list);
        else
            return "</ul>" + ParseText(markdown, list);
    }

    private static string ParseLine(string markdown, bool list, out bool inListAfter)
    {
        string result = ParseHeader(markdown, list, out inListAfter);

        if (result == null)
        {
            result = ParseLineItem(markdown, list, out inListAfter);
        }

        if (result == null)
        {
            result = ParseParagraph(markdown, list, out inListAfter);
        }

        if (result == null)
        {
            throw new ArgumentException("Invalid markdown");
        }

        return result;
    }

    public static string Parse(string markdown)
    {
        string[] lines = markdown.Split('\n');
        string result = "";
        bool list = false;

        for (int i = 0; i < lines.Length; i++)
        {
            string lineResult = ParseLine(lines[i], list, out list);
            result += lineResult;
        }

        if (list)
        {
            return result + "</ul>";
        }
        else
        {
            return result;
        }
    }
}