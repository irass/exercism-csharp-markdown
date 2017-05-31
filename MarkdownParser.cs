using System.Collections.Generic;
using static MarkdownToHTML.Enums;
using static MarkdownToHTML.Utils;
using static MarkdownToHTML.TagConversions;
using System;

namespace MarkdownToHTML
{
    public static class MarkdownParser
    {
        private static bool CheckIfClosingWordTag(int currentIndex, string input, string openingTag, out int lengthToSkip)
        {
            string currentString = "";
            lengthToSkip = 0;

            while (currentIndex < input.Length)
            {
                currentString += input[currentIndex];

                if (_dictForWordTags.ContainsKey(currentString) && 
                    _dictForWordTags[currentString] == openingTag && 
                    !CheckStringIndexEqualsChar(input, currentIndex - 1, new List<char> { ' ' }))
                {
                    if (currentIndex + 1 >= input.Length || CheckStringIndexEqualsChar(input, currentIndex + 1, new List<char> { ' ' , '\n'}))
                    {
                        lengthToSkip = currentString.Length;
                        return true;
                    }
                }
                currentIndex++;
            }

            return false;
        }

        private static bool GetMatchingTagFromDictionary(Dictionary<string, string> dict, string text, TagDictionaryType dictionaryUsed, out string tag, out int numberOfCharactersToSubstitute)
        {
            string stringToSubstitute = "";
            int index = 0;
            numberOfCharactersToSubstitute = 0;
            tag = null;

            if (dictionaryUsed == TagDictionaryType.Paragraph)
            {
                tag = dict[""];
                return true;
            }

            while (index < text.Length)
            {
                stringToSubstitute += text[index];
                if (dict.ContainsKey(stringToSubstitute))
                {
                    if (dictionaryUsed != TagDictionaryType.Word || !CheckStringIndexEqualsChar(text, index + 1, new List<char> { ' ', '_', '*' }))
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

        private static string GetTagToInsert(string input, string lineTag, string parentTag, bool parentTagExists, out NewTagType newTagType, out int numberOfCharactersToSubstitute)
        {
            string tag = null;

            if (!parentTagExists)
            {
                if (GetMatchingTagFromDictionary(_dictForParentTags, input, TagDictionaryType.Parent, out tag, out numberOfCharactersToSubstitute))
                {
                    newTagType = NewTagType.Parent;
                    return tag;
                }
            }

            if (GetMatchingTagFromDictionary(_dictForLineTags, input, TagDictionaryType.Line, out tag, out numberOfCharactersToSubstitute))
            {
                newTagType = NewTagType.Line;
                return tag;
            }

            if (string.IsNullOrEmpty(lineTag) && string.IsNullOrEmpty(parentTag) && !parentTagExists)
            {
                if (GetMatchingTagFromDictionary(_dictForParagraphTag, input, TagDictionaryType.Paragraph, out tag, out numberOfCharactersToSubstitute))
                {
                    newTagType = NewTagType.Parent;
                    return tag;
                }
            }

            if (GetMatchingTagFromDictionary(_dictForWordTags, input, TagDictionaryType.Word, out tag, out numberOfCharactersToSubstitute))
            {
                newTagType = NewTagType.Word;
                return tag;
            }

            newTagType = NewTagType.None;
            numberOfCharactersToSubstitute = 1;
            return input[0].ToString();
        }
      
        public static string Parse(string input)
        {
            string wordTag = "";
            string lineTag = "";
            string parentTag = "";
            bool parentTagExists = false;

            return ParseSection(wordTag, lineTag, parentTag, input, parentTagExists);
        }

        private static string AddAppropriateClosingTag(string result, NewTagType newTagType, string lineTag, string parentTag)
        {
            if (newTagType == NewTagType.Line)
                return AddClosingTag(result, lineTag);
            else if (newTagType == NewTagType.Parent)
                return AddClosingTag(result, parentTag);
            else
                return result;
        }

        private static void SetUpTagsForNextSection(NewTagType newTagType, string tagToInsert, string currentLineTag, out string wordTag, out string lineTag, out string parentTag)
        {
            if (newTagType == NewTagType.Line)
                ResetTags("", tagToInsert, "", out wordTag, out lineTag, out parentTag);

            else if (newTagType == NewTagType.Word)
                ResetTags(tagToInsert, currentLineTag, "", out wordTag, out lineTag, out parentTag);

            else if (newTagType == NewTagType.Parent)
                ResetTags("", "", tagToInsert, out wordTag, out lineTag, out parentTag);

            else
                ResetTags("", "", "", out wordTag, out lineTag, out parentTag);
        }

        private static string ParseNextSection(string input, int startingIndex, bool parentTagExists, NewTagType newTagType, string tagToInsert, string currentLineTag)
        {
            try
            {
                string nextSection = GetNextSection(input, startingIndex);
                SetUpTagsForNextSection(newTagType, tagToInsert, currentLineTag, out string newWordTag, out string newLineTag, out string newParentTag);

                return ParseSection(newWordTag, newLineTag, newParentTag, nextSection, parentTagExists);
            }
            catch (Exception ex)
            {
                throw new MarkdownParserException(input, ex.Message);
            }
        }

        private static string ParseSection(string wordTag, string lineTag, string parentTag, string input, bool parentTagExists)
        {
            string result = "";
            int currentIndex = 0;           
            int lengthToSkip = 0;
            NewTagType newTagType;

            while (currentIndex < input.Length)
            {
                if (input[currentIndex] == '\n')
                {
                    result = AddClosingTag(result, wordTag);
                    result = AddClosingTag(result, lineTag);
                    result += ParseNextSection(input, currentIndex + 1, parentTagExists, NewTagType.None, null, null);
                    ResetTags(wordTag, "", "", out wordTag, out lineTag, out parentTag);

                    break;                 
                }
                else if (CheckIfClosingWordTag(currentIndex, input, wordTag, out lengthToSkip))
                {
                    result = AddClosingTag(result, wordTag);
                    result += ParseNextSection(input, currentIndex + lengthToSkip, parentTagExists, NewTagType.Line, lineTag, lineTag);
                    return result;
                }
                else
                {
                    string tagToInsert = GetTagToInsert(input.Substring(currentIndex), lineTag, parentTag, parentTagExists, out newTagType, out lengthToSkip);
                    result += tagToInsert;

                    if (newTagType != NewTagType.None)
                    {
                        if (newTagType == NewTagType.Parent)
                            parentTagExists = true;

                        AddAppropriateClosingTag(result, newTagType, lineTag, parentTag);
                        result += ParseNextSection(input, currentIndex + lengthToSkip, parentTagExists, newTagType, tagToInsert, lineTag);

                        if (newTagType == NewTagType.Word)
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