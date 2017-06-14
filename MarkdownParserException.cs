using System;
using System.Collections.Generic;
using System.Text;

namespace MarkdownToHTML
{
    public class MarkdownParserException : Exception
    {
        public MarkdownParserException(string input, string message) : base($"Could not parse {input}{Environment.NewLine}{message}")
        {
        }
    }
}
