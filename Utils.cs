using System;
using System.Collections.Generic;
using System.Text;

namespace MarkdownToHTML
{
    public static class Utils
    {
        public static string GetClosingTag(string openingTag) => openingTag.Replace("<", "</");

        public static string AddClosingTag(string result, string openingTag) => result += GetClosingTag(openingTag);

        public static string GetNextSection(string text, int startingIndex) => text.Substring(startingIndex);

        public static void ResetTags(string newWordTag, string newLineTag, string newParentTag, out string wordTag, out string lineTag, out string parentTag)
        {
            wordTag = newWordTag;
            lineTag = newLineTag;
            parentTag = newParentTag;
            return;
        }

        public static bool CheckStringIndexEqualsChar(string text, int index, List<char> characters) => (index >= 0 &&
                                                                                                     index < text.Length &&
                                                                                                     characters.Contains(text[index]));
    }
}
