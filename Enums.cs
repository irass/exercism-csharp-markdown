using System;
using System.Collections.Generic;
using System.Text;

namespace MarkdownToHTML
{
    public class Enums
    {
        public enum TagDictionaryType
        {
            Parent = 0,
            Line = 1,
            Word = 2,
            Paragraph = 3
        }

        public enum NewTagType
        {
            Parent = 0,
            Line = 1,
            Word = 2,
            None = 3
        }

    }
}
