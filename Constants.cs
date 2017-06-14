using System.Collections.Generic;

namespace MarkdownToHTML
{
    public static class Constants
    {
        public const char NEW_LINE = '\n';

        public static readonly Dictionary<string, string> DictForLineTags = 
            new Dictionary<string, string> {
                { "# ", "<h1>" },
                { "## ", "<h2>" },
                { "### ", "<h3>" },
                { "#### ", "<h4>" },
                { "##### ", "<h5>" },
                { "###### ", "<h6>" },
                { "* ", "<li>" }
            };

        public static readonly Dictionary<string, string> DictForWordTags = 
            new Dictionary<string, string> {
                { "_", "<em>" },
                { "*", "<em>" },
                { "__", "<strong>"},
                { "**", "<strong>"}
            };

        public static readonly Dictionary<string, string> DictForParentTags =
            new Dictionary<string, string> {
                { "* ", "<ul>" }
            };

        public static readonly Dictionary<string, string> DictForParagraphTag = 
            new Dictionary<string, string> {
                { "", "<p>" }
            };

        public static readonly List<char> CharactersThatCannotFollowAnOpeningWordTag = 
            new List<char> { ' ', '_', '*' };

    }
}
