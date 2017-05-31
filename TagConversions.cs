using System;
using System.Collections.Generic;
using System.Text;

namespace MarkdownToHTML
{
    public class TagConversions
    {
        public static readonly Dictionary<string, string> _dictForLineTags = new Dictionary<string, string> {
                                                                                { "# ", "<h1>" },
                                                                                { "## ", "<h2>" },
                                                                                { "### ", "<h3>" },
                                                                                { "#### ", "<h4>" },
                                                                                { "##### ", "<h5>" },
                                                                                { "###### ", "<h6>" },
                                                                                { "* ", "<li>" }
                                                                             };
        public static readonly Dictionary<string, string> _dictForWordTags = new Dictionary<string, string> {
                                                                                { "_", "<em>" },
                                                                                { "*", "<em>" },
                                                                                { "__", "<strong>"},
                                                                                { "**", "<strong>"}
                                                                             };

        public static readonly Dictionary<string, string> _dictForParentTags = new Dictionary<string, string> {
                                                                                { "* ", "<ul>" }
                                                                             };

        public static readonly Dictionary<string, string> _dictForParagraphTag = new Dictionary<string, string> {
                                                                                { "", "<p>" }
                                                                             };

    }
}
