using System.Collections.Generic;
using static MarkdownToHTML.Enums;
using static MarkdownToHTML.Constants;

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

        public static bool CheckStringIndexInCharList(string text, int index, List<char> characters) => (index >= 0 &&
                                                                                                         index < text.Length &&
                                                                                                         characters.Contains(text[index]));


        //Determines whether the next string in the input matches the currently open word tag
        public static bool CheckIfClosingWordTag(int currentIndex, string input, string openWordTag, out int lengthToSkip)
        {
            string currentString = string.Empty;
            lengthToSkip = 0;

            while (currentIndex < input.Length)
            {
                currentString += input[currentIndex];

                if (DictForWordTags.ContainsKey(currentString) &&
                    DictForWordTags[currentString] == openWordTag &&
                    !CheckStringIndexInCharList(input, currentIndex - 1, new List<char> { ' ' }))
                {
                    if (currentIndex + 1 >= input.Length || CheckStringIndexInCharList(input, currentIndex + 1, new List<char> { ' ', NEW_LINE }))
                    {
                        lengthToSkip = currentString.Length;
                        return true;
                    }
                }
                currentIndex++;
            }
            return false;
        }


        //Gets the matching tag from the supplied dictionary for the string provided
        public static bool GetMatchingTagFromDictionary(Dictionary<string, string> dict, string text, TagDictionaryType dictionaryUsed, out string tag, out int numberOfCharactersToSubstitute)
        {
            string stringToSubstitute = string.Empty;
            int index = 0;
            numberOfCharactersToSubstitute = 0;
            tag = null;

            if (dictionaryUsed == TagDictionaryType.Paragraph)
            {
                tag = dict[string.Empty];
                return true;
            }

            while (index < text.Length)
            {
                stringToSubstitute += text[index];
                if (dict.ContainsKey(stringToSubstitute))
                {
                    if (dictionaryUsed != TagDictionaryType.Word || !CheckStringIndexInCharList(text, index + 1, CharactersThatCannotFollowAnOpeningWordTag))
                    {
                        tag = dict[stringToSubstitute];
                        if (dictionaryUsed != TagDictionaryType.Parent)
                            numberOfCharactersToSubstitute = stringToSubstitute.Length;
                        return true;
                    }
                }
                index++;
            }
            return false;
        }


        //Gets the appropriate string to be inserted based on input and currently open tags
        public static string GetStringToInsert(string input, string lineTag, string parentTag, bool parentTagExists, out NewTagType newTagType, out int numberOfCharactersToSubstitute)
        {
            string tag = null;
            newTagType = NewTagType.None;
            numberOfCharactersToSubstitute = 1;

            if (!parentTagExists)
                if (GetMatchingTagFromDictionary(DictForParentTags, input, TagDictionaryType.Parent, out tag, out numberOfCharactersToSubstitute))
                    newTagType = NewTagType.Parent;

            if (tag == null && GetMatchingTagFromDictionary(DictForLineTags, input, TagDictionaryType.Line, out tag, out numberOfCharactersToSubstitute))
                newTagType = NewTagType.Line;

            if (tag == null && string.IsNullOrEmpty(lineTag) && string.IsNullOrEmpty(parentTag) && !parentTagExists)
                if (GetMatchingTagFromDictionary(DictForParagraphTag, input, TagDictionaryType.Paragraph, out tag, out numberOfCharactersToSubstitute))
                    newTagType = NewTagType.Parent;

            if (tag == null && GetMatchingTagFromDictionary(DictForWordTags, input, TagDictionaryType.Word, out tag, out numberOfCharactersToSubstitute))
                newTagType = NewTagType.Word;

            if (tag != null)
                return tag;
            else
                return input[0].ToString();
        }


        //Sets up currently open tags for parsing the next section
        public static void SetUpTagsForNextSection(NewTagType newTagType, string tagToInsert, string currentLineTag, out string wordTag, out string lineTag, out string parentTag)
        {
            if (newTagType == NewTagType.Line)
                ResetTags(string.Empty, tagToInsert, string.Empty, out wordTag, out lineTag, out parentTag);

            else if (newTagType == NewTagType.Word)
                ResetTags(tagToInsert, currentLineTag, string.Empty, out wordTag, out lineTag, out parentTag);

            else if (newTagType == NewTagType.Parent)
                ResetTags(string.Empty, string.Empty, tagToInsert, out wordTag, out lineTag, out parentTag);

            else
                ResetTags(string.Empty, string.Empty, string.Empty, out wordTag, out lineTag, out parentTag);
        }


        //Adds appropriate closing tag based on the most recently inserted tag
        public static string AddAppropriateClosingTag(string result, NewTagType tagType, string lineTag, string parentTag)
        {
            if (tagType == NewTagType.Line)
                return AddClosingTag(result, lineTag);
            else if (tagType == NewTagType.Parent)
                return AddClosingTag(result, parentTag);
            else
                return result;
        }
    }
}
