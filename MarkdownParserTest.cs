using Xunit;

namespace MarkdownToHTML
{

    public class MarkdownParserTest
    {
        #region Tests

        [Fact]
        public void Parses_normal_text_as_a_paragraph()
        {
            string input = "This will be a paragraph";
            string expected = "<p>This will be a paragraph</p>";

            Assert.Equal(expected, MarkdownParser.Parse(input));
        }

        [Fact]
        public void Parsing_italics()
        {
            string input = "_This will be italic_";
            string expected = "<p><em>This will be italic</em></p>";

            Assert.Equal(expected, MarkdownParser.Parse(input));
        }

        [Fact]
        public void Parsing_italics2()
        {
            string input = "*This will be italic*";
            string expected = "<p><em>This will be italic</em></p>";

            Assert.Equal(expected, MarkdownParser.Parse(input));
        }

        [Fact]
        public void Parsing_bold_text()
        {
            string input = "__This will be bold__";
            string expected = "<p><strong>This will be bold</strong></p>";

            Assert.Equal(expected, MarkdownParser.Parse(input));
        }

        [Fact]
        public void Parsing_bold_text2()
        {
            string input = "**This will be bold**";
            string expected = "<p><strong>This will be bold</strong></p>";

            Assert.Equal(expected, MarkdownParser.Parse(input));
        }

        [Fact]
        public void Mixed_normal_italics_and_bold_text()
        {
            string input = "This will _be_ __mixed__";
            string expected = "<p>This will <em>be</em> <strong>mixed</strong></p>";

            Assert.Equal(expected, MarkdownParser.Parse(input));
        }

        [Fact]
        public void With_h1_header_level()
        {
            string input = "# This will be an h1";
            string expected = "<h1>This will be an h1</h1>";

            Assert.Equal(expected, MarkdownParser.Parse(input));
        }

        [Fact]
        public void With_h2_header_level()
        {
            string input = "## This will be an h2";
            string expected = "<h2>This will be an h2</h2>";

            Assert.Equal(expected, MarkdownParser.Parse(input));
        }

        [Fact]
        public void With_h6_header_level()
        {
            string input = "###### This will be an h6";
            string expected = "<h6>This will be an h6</h6>";

            Assert.Equal(expected, MarkdownParser.Parse(input));
        }

        [Fact]
        public void Unordered_lists()
        {
            string input = "* Item 1\n* Item 2";
            string expected = "<ul><li>Item 1</li><li>Item 2</li></ul>";

            Assert.Equal(expected, MarkdownParser.Parse(input));
        }

        [Fact]
        public void With_a_little_bit_of_everything()
        {
            string input = "# Header!\n* __Bold Item__\n* _Italic Item_";
            string expected = "<h1>Header!</h1><ul><li><strong>Bold Item</strong></li><li><em>Italic Item</em></li></ul>";

            Assert.Equal(expected, MarkdownParser.Parse(input));
        }

        #endregion

        #region Helpers



        #endregion

    }
}