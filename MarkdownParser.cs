using System.Collections.Generic;
using static MarkdownToHTML.Enums;
using static MarkdownToHTML.Utils;
using static MarkdownToHTML.TagConversions;
using System;

namespace MarkdownToHTML
{
    public static class MarkdownParser
    {
        public static string Parse(string input)
        {
            string wordTag = "";
            string lineTag = "";
            string parentTag = "";
            bool parentTagExists = false;

            return new MarkdownSectionParser(input, wordTag, lineTag, parentTag, parentTagExists).Parse();
        }
        
    }
}