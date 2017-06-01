using System;
using static MarkdownToHTML.Enums;
using static MarkdownToHTML.Utils;

namespace MarkdownToHTML
{
    public class MarkdownSectionParser
    {
        private string _openWordTag;
        private string _openLineTag;
        private string _openParentTag;
        private string _markdownInput;
        private bool _parentTagExists;


        public MarkdownSectionParser(string markdownInput, string openWordTag, string openLineTag, string openParentTag, bool parentTagExists)
        {
            _markdownInput = markdownInput;
            _openWordTag = openWordTag;
            _openLineTag = openLineTag;
            _openParentTag = openParentTag;
            _parentTagExists = parentTagExists;
        }


        private string ParseNextSection(int startingIndex, NewTagType newTagType, string tagToInsert, string currentLineTag)
        {
            try
            {
                string nextSection = GetNextSection(_markdownInput, startingIndex);
                SetUpTagsForNextSection(newTagType, tagToInsert, currentLineTag, out string newWordTag, out string newLineTag, out string newParentTag);

                return new MarkdownSectionParser(nextSection, newWordTag, newLineTag, newParentTag, _parentTagExists).Parse();
            }
            catch (Exception ex)
            {
                throw new MarkdownParserException(_markdownInput, ex.Message);
            }
        }


        //if newline - close currently open word and line tags and parse beyond
        private string DealWithNewLine(string result, int currentIndex)
        {
            result = AddClosingTag(result, _openWordTag);
            result = AddClosingTag(result, _openLineTag);
            return result + ParseNextSection(currentIndex + 1, NewTagType.None, null, null);
        }


        private string DealWithClosingWordTag(string result, int currentIndex, int lengthToSkip)
        {
            result = AddClosingTag(result, _openWordTag);
            return result + ParseNextSection(currentIndex + lengthToSkip, NewTagType.Line, _openLineTag, _openLineTag);
        }


        //if a new tag has been inserted - close any previous tags if applicable and parse beyond
        private string DealWithOpeningTag(string result, int currentIndex, int lengthToSkip, NewTagType newTagType, string stringToInsert)
        {
            if (newTagType == NewTagType.Parent)
                _parentTagExists = true;

            AddAppropriateClosingTag(result, newTagType, _openLineTag, _openParentTag);
            result += ParseNextSection(currentIndex + lengthToSkip, newTagType, stringToInsert, _openLineTag);

            if (newTagType == NewTagType.Word)
                ResetTags(_openWordTag, "", _openParentTag, out _openWordTag, out _openLineTag, out _openParentTag);

            return result;
        }


        private string DealWithEndOfSection(string result)
        {
            result = AddClosingTag(result, _openLineTag);
            result = AddClosingTag(result, _openParentTag);
            _parentTagExists = false;

            return result;
        }


        public string Parse()
        {
            string result = "";
            int currentIndex = 0;

            while (currentIndex < _markdownInput.Length)
            {
                int lengthToSkip = 0;

                if (_markdownInput[currentIndex] == '\n')
                    return DealWithNewLine(result, currentIndex);

                else if (CheckIfClosingWordTag(currentIndex, _markdownInput, _openWordTag, out lengthToSkip))
                    return DealWithClosingWordTag(result, currentIndex, lengthToSkip);

                else
                {
                    NewTagType newTagType;
                    string stringToInsert = GetStringToInsert(_markdownInput.Substring(currentIndex), _openLineTag, _openParentTag, _parentTagExists, out newTagType, out lengthToSkip);
                    result += stringToInsert;

                    if (newTagType != NewTagType.None)
                    {
                        result = DealWithOpeningTag(result, currentIndex, lengthToSkip, newTagType, stringToInsert);
                        break;
                    }
                }
                currentIndex++;
            }
            return DealWithEndOfSection(result);
        }

    }
}
