using System;
using System.Collections.Generic;
using System.Text;

namespace MarkdownToHTML
{
    public static class Utils
    {
        public static string Wrap(string text, string tag) => "<" + tag + ">" + text + "</" + tag + ">";
        public static bool IsTag(string text, string tag) => text.StartsWith("<" + tag + ">");
        public static bool MarkdownLineIsHeader(string line) => line.StartsWith("#");
        public static bool MarkdownLineIsListItem(string line) => line.StartsWith("*");

    }
}
