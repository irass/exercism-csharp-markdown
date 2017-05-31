using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MarkdownToHTML
{
    public static class MarkdownParser
    {
        private enum TagDictionary
        {
            Parent = 0,
            Line = 1,
            Word = 2,
            Paragraph = 3
        }

        private enum NewTag
        {
            Parent = 0,
            Line = 1,
            Word = 2,
            None = 3
        }

        private static readonly Dictionary<string, string> _dictForLineTags = new Dictionary<string, string> {
                                                                                { "# ", "<h1>" },
                                                                                { "## ", "<h2>" },
                                                                                { "### ", "<h3>" },
                                                                                { "#### ", "<h4>" },
                                                                                { "##### ", "<h5>" },
                                                                                { "###### ", "<h6>" },
                                                                                { "* ", "<li>" }
                                                                             };
        private static readonly Dictionary<string, string> _dictForWordTags = new Dictionary<string, string> {
                                                                                { "_", "<em>" },
                                                                                { "*", "<em>" },
                                                                                { "__", "<strong>"},
                                                                                { "**", "<strong>"}
                                                                             };

        private static readonly Dictionary<string, string> _dictForParentTags = new Dictionary<string, string> {
                                                                                { "* ", "<ul>" }
                                                                             };

        private static readonly Dictionary<string, string> _dictForParagraphTag = new Dictionary<string, string> {
                                                                                { "", "<p>" }
                                                                             };

        private static bool CheckStringIndexEqualsChar(string text, int index, int offset, List<char> characters) => (index + offset >= 0 && 
                                                                                                     index + offset < text.Length &&
                                                                                                     characters.Contains(text[index + offset]));

        private static bool CheckIfClosingWordTag(int currentIndex, string input, string openingTag, out int lengthToSkip)
        {
            string currentString = "";
            lengthToSkip = 0;

            while (currentIndex < input.Length)
            {
                currentString += input[currentIndex];

                if (_dictForWordTags.ContainsKey(currentString) && 
                    _dictForWordTags[currentString] == openingTag && 
                    !CheckStringIndexEqualsChar(input, currentIndex, -1, new List<char> { ' ' }))
                {
                    if (currentIndex + 1 >= input.Length || CheckStringIndexEqualsChar(input, currentIndex, 1, new List<char> { ' ' , '\n'}))
                    {
                        lengthToSkip = currentString.Length;
                        return true;
                    }
                }
                currentIndex++;
            }

            return false;
        }

        private static bool GetMatchingTagFromDictionary(Dictionary<string, string> dict, string text, TagDictionary dictionaryUsed, out string tag, out int numberOfCharactersToSubstitute)
        {
            string stringToSubstitute = "";
            int counter = 0;
            numberOfCharactersToSubstitute = 0;
            tag = null;

            if (dictionaryUsed == TagDictionary.Paragraph)
            {
                tag = dict[""];
                return true;
            }

            while (counter < text.Length)
            {
                stringToSubstitute += text[counter];
                if (dict.ContainsKey(stringToSubstitute))
                {
                    if (dictionaryUsed != TagDictionary.Word || !CheckStringIndexEqualsChar(text, counter, 1, new List<char> { ' ', '_' }))
                    {
                        tag = dict[stringToSubstitute];
                        if (dictionaryUsed != TagDictionary.Parent)
                            numberOfCharactersToSubstitute = stringToSubstitute.Length;
                        return true;
                    }
                }
                counter++;
            }
            return false;
        }

        private static string GetTagToInsert(string input, string lineTag, string parentTag, bool parentTagExists, out NewTag newTagType, out int numberOfCharactersToSubstitute)
        {
            string tag = null;

            if (!parentTagExists)
            {
                if (GetMatchingTagFromDictionary(_dictForParentTags, input, TagDictionary.Parent, out tag, out numberOfCharactersToSubstitute))
                {
                    newTagType = NewTag.Parent;
                    return tag;
                }
            }

            if (GetMatchingTagFromDictionary(_dictForLineTags, input, TagDictionary.Line, out tag, out numberOfCharactersToSubstitute))
            {
                newTagType = NewTag.Line;
                return tag;
            }

            if (string.IsNullOrEmpty(lineTag) && string.IsNullOrEmpty(parentTag) && !parentTagExists)
            {
                if (GetMatchingTagFromDictionary(_dictForParagraphTag, input, TagDictionary.Paragraph, out tag, out numberOfCharactersToSubstitute))
                {
                    newTagType = NewTag.Parent;
                    return tag;
                }
            }

            if (GetMatchingTagFromDictionary(_dictForWordTags, input, TagDictionary.Word, out tag, out numberOfCharactersToSubstitute))
            {
                newTagType = NewTag.Word;
                return tag;
            }

            newTagType = NewTag.None;
            numberOfCharactersToSubstitute = 1;
            return input[0].ToString();
        }

        private static string GetClosingTag(string openingTag) => openingTag.Replace("<", "</");

        private static string AddClosingTag(string result, string openingTag) => result += GetClosingTag(openingTag);


        public static string Parse(string input)
        {
            string wordTag = "";
            string lineTag = "";
            string parentTag = "";
            bool parentTagExists = false;

            return ParseSection(wordTag, lineTag, parentTag, input, parentTagExists);
        }

        private static string AddAppropriateClosingTag(string result, NewTag newTagType, string lineTag, string parentTag)
        {
            if (newTagType == NewTag.Line)
                return AddClosingTag(result, lineTag);
            else if (newTagType == NewTag.Parent)
                return AddClosingTag(result, parentTag);
            else
                return result;
        }

        private static void ResetTags(string newWordTag, string newLineTag, string newParentTag, out string wordTag, out string lineTag, out string parentTag)
        {
            wordTag = newWordTag;
            lineTag = newLineTag;
            parentTag = newParentTag;
            return;
        }

        private static void SetUpTagsForNextSection(NewTag newTagType, string tagToInsert, string currentLineTag, out string wordTag, out string lineTag, out string parentTag)
        {
            if (newTagType == NewTag.Line)
                ResetTags("", tagToInsert, "", out wordTag, out lineTag, out parentTag);

            else if (newTagType == NewTag.Word)
                ResetTags(tagToInsert, currentLineTag, "", out wordTag, out lineTag, out parentTag);

            else if (newTagType == NewTag.Parent)
                ResetTags("", "", tagToInsert, out wordTag, out lineTag, out parentTag);

            else
                ResetTags("", "", "", out wordTag, out lineTag, out parentTag);
        }

        private static string GetNextSection(string text, int startingIndex) => text.Substring(startingIndex);


        private static string ParseNextSection(string input, int startingIndex, bool parentTagExists, NewTag newTagType, string tagToInsert, string currentLineTag)
        {
            string nextSection = GetNextSection(input, startingIndex);
            SetUpTagsForNextSection(newTagType, tagToInsert, currentLineTag, out string newWordTag, out string newLineTag, out string newParentTag);

            return ParseSection(newWordTag, newLineTag, newParentTag, nextSection, parentTagExists);
        }

        private static string ParseSection(string wordTag, string lineTag, string parentTag, string input, bool parentTagExists)
        {
            string result = "";
            int currentIndex = 0;           
            int lengthToSkip = 0;
            NewTag newTagType;

            while (currentIndex < input.Length)
            {
                if (input[currentIndex] == '\n')
                {
                    result = AddClosingTag(result, wordTag);
                    result = AddClosingTag(result, lineTag);
                    result += ParseNextSection(input, currentIndex + 1, parentTagExists, NewTag.None, null, null);
                    ResetTags(wordTag, "", "", out wordTag, out lineTag, out parentTag);

                    break;                 
                }
                else if (CheckIfClosingWordTag(currentIndex, input, wordTag, out lengthToSkip))
                {
                    result = AddClosingTag(result, wordTag);
                    result += ParseNextSection(input, currentIndex + lengthToSkip, parentTagExists, NewTag.Line, lineTag, lineTag);
                    return result;
                }
                else
                {
                    string tagToInsert = GetTagToInsert(input.Substring(currentIndex), lineTag, parentTag, parentTagExists, out newTagType, out lengthToSkip);
                    result += tagToInsert;

                    if (newTagType != NewTag.None)
                    {
                        if (newTagType == NewTag.Parent)
                            parentTagExists = true;

                        AddAppropriateClosingTag(result, newTagType, lineTag, parentTag);
                        result += ParseNextSection(input, currentIndex + lengthToSkip, parentTagExists, newTagType, tagToInsert, lineTag);

                        if (newTagType == NewTag.Word)
                            ResetTags(wordTag, "", parentTag, out wordTag, out lineTag, out parentTag);

                        break;
                    }
                }
                currentIndex++;
            }

            result = AddClosingTag(result, lineTag);
            result = AddClosingTag(result, parentTag);
            parentTagExists = false;

            return result;
        }
    }
}