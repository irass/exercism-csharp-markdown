using System;
using static MarkdownToHTML.Enums;
using static MarkdownToHTML.Utils;
using static MarkdownToHTML.Constants;

namespace MarkdownToHTML
{
    // todo: interesting general case solution. 
    // This class is longer than ideal.
    // I think that it would probably be easier to understand and debug and extend if the solution was more specific.
    public class MarkdownSectionParser
    {
        // I would remove the "private" keywords throughout to improve the signal to noise ratio
        private readonly string _markdownInput;
        private string _openWordTag;
        private string _openLineTag;
        private string _openParentTag;
        private bool _parentTagExists;


        public MarkdownSectionParser(string markdownInput, string openWordTag, string openLineTag, string openParentTag, bool parentTagExists)
        {
            _markdownInput = markdownInput;
            _openWordTag = openWordTag;
            _openLineTag = openLineTag;
            _openParentTag = openParentTag;
            _parentTagExists = parentTagExists;
        }

        public MarkdownSectionParser(string markdownInput) : this(markdownInput, string.Empty, string.Empty, string.Empty, false)
        {
        }

        private string ParseNextSection(int startingIndex, NewTagType newTagType, string tagToInsert, string currentLineTag)
        {
            // todo: in try / except, it is best to have just one line of code in the try and one in the except, ie, by create another method
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


        //if closing word tag - close currently open word tags and parse beyond
        private string DealWithClosingWordTag(string result, int currentIndex, int lengthToSkip)
        {
            result = AddClosingTag(result, _openWordTag);
            return result + ParseNextSection(currentIndex + lengthToSkip, NewTagType.Line, _openLineTag, _openLineTag);
        }


        //if a new tag has been inserted - close any previous tags if applicable and parse beyond
        private string DealWithOpeningTag(string result, int currentIndex, int lengthToSkip, NewTagType newTagType, string stringToInsert)
        {
            //todo: a method should all be at the same level of abstraction ideally. This one mixes some higher level concepts (ParseNextSection) and some lower ones (_parentTagExists)
            if (newTagType == NewTagType.Parent)
                _parentTagExists = true;

            AddAppropriateClosingTag(result, newTagType, _openLineTag, _openParentTag);
            result += ParseNextSection(currentIndex + lengthToSkip, newTagType, stringToInsert, _openLineTag);

            if (newTagType == NewTagType.Word)
                ResetTags(_openWordTag, string.Empty, _openParentTag, out _openWordTag, out _openLineTag, out _openParentTag);

            return result;
        }


        private string DealWithEndOfSection(string result)
        {
            result = AddClosingTag(result, _openLineTag);
            result = AddClosingTag(result, _openParentTag);
            _parentTagExists = false;

            return result;
        }

        // todo: methods should generally have just one control structure (try / except, loop, conditional) and method calls. This was the control logic stands out and is obvious, and is not confused by the non control logic
        public string Parse()
        {
            string result = string.Empty;
            int currentIndex = 0;

            while (currentIndex < _markdownInput.Length)
            {
                int lengthToSkip = 0;

                if (_markdownInput[currentIndex] == NEW_LINE)
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
