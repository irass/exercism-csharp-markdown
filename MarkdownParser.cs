using System.Collections.Generic;
using static MarkdownToHTML.Enums;
using static MarkdownToHTML.Utils;
using static MarkdownToHTML.TagConversions;
using System;

namespace MarkdownToHTML
{
    public static class MarkdownParser
    {
        //Entry method
        public static string Parse(string input)
        {
            string wordTag = "";
            string lineTag = "";
            string parentTag = "";
            bool parentTagExists = false;

            return ParseSection(wordTag, lineTag, parentTag, input, parentTagExists);
        }

        //Determines whether the next string in the input matches the currently open word tag
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

        //Gets the matching tag from the supplied dictionary for the string provided
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

        //Gets the appropriate tag to be inserted based on input and currently open tags
        private static string GetTagToInsert(string input, string lineTag, string parentTag, bool parentTagExists, out NewTagType newTagType, out int numberOfCharactersToSubstitute)
        {
            string tag = null;
            newTagType = NewTagType.None;
            numberOfCharactersToSubstitute = 1;

            if (!parentTagExists)
                if (GetMatchingTagFromDictionary(_dictForParentTags, input, TagDictionaryType.Parent, out tag, out numberOfCharactersToSubstitute))
                    newTagType = NewTagType.Parent;

            if (tag == null && GetMatchingTagFromDictionary(_dictForLineTags, input, TagDictionaryType.Line, out tag, out numberOfCharactersToSubstitute))
                newTagType = NewTagType.Line;

            if (tag == null && string.IsNullOrEmpty(lineTag) && string.IsNullOrEmpty(parentTag) && !parentTagExists)
                if (GetMatchingTagFromDictionary(_dictForParagraphTag, input, TagDictionaryType.Paragraph, out tag, out numberOfCharactersToSubstitute))
                    newTagType = NewTagType.Parent;

            if (tag == null && GetMatchingTagFromDictionary(_dictForWordTags, input, TagDictionaryType.Word, out tag, out numberOfCharactersToSubstitute))
                newTagType = NewTagType.Word;

            if (tag != null)
                return tag;
            else
                return input[0].ToString();
        }
        
        //Adds appropriate closing tag based on the most recently inserted tag
        private static string AddAppropriateClosingTag(string result, NewTagType newTagType, string lineTag, string parentTag)
        {
            if (newTagType == NewTagType.Line)
                return AddClosingTag(result, lineTag);
            else if (newTagType == NewTagType.Parent)
                return AddClosingTag(result, parentTag);
            else
                return result;
        }

        //Sets up currently open tags for parsing the next section
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

        //Main worker method (recursive in conjunction with ParseNextSection())
        private static string ParseSection(string wordTag, string lineTag, string parentTag, string input, bool parentTagExists)
        {
            string result = "";
            int currentIndex = 0;           
            int lengthToSkip = 0;
            NewTagType newTagType;

            while (currentIndex < input.Length)
            {   
                //if newline - close currently open word and line tags and parse beyond
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
                    string stringToInsert = GetTagToInsert(input.Substring(currentIndex), lineTag, parentTag, parentTagExists, out newTagType, out lengthToSkip);
                    result += stringToInsert;

                    //if a new tag has been inserted - close any previous tags if applicable and parse beyond
                    if (newTagType != NewTagType.None)
                    {
                        if (newTagType == NewTagType.Parent)
                            parentTagExists = true;

                        AddAppropriateClosingTag(result, newTagType, lineTag, parentTag);
                        result += ParseNextSection(input, currentIndex + lengthToSkip, parentTagExists, newTagType, stringToInsert, lineTag);

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