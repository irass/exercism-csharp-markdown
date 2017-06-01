namespace MarkdownToHTML
{
    public static class MarkdownParser
    {
        public static string Parse(string input)
        {
            string wordTag = string.Empty;
            string lineTag = string.Empty;
            string parentTag = string.Empty;
            bool parentTagExists = false;

            return new MarkdownSectionParser(input, wordTag, lineTag, parentTag, parentTagExists).Parse();
        }
        
    }
}