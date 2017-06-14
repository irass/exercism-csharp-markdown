namespace MarkdownToHTML
{
    public static class MarkdownParser
    {
        // todo: remove this class as it doesn't do anything
        public static string Parse(string input)
        {
            return new MarkdownSectionParser(input).Parse();
        }
        
    }
}