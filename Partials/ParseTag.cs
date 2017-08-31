using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hilgeman.HTMLElements;

namespace Hilgeman
{
    public partial class HTMLParser
    {
        private enum TagParserState
        {
            None,
            ExpectingTagName,
            ExpectingTagContentsOrEnd,
            TagEnded
        }
        private TagParserState tagParserState = TagParserState.None;

        // Return DOMElement instead of Tag, since we -could- return 
        private DOMElement _ParseTag(int startPosition)
        {
            // Initialize new Tag and empty Attribute
            HTMLTag tag = new HTMLTag(startPosition);
            HTMLTagAttribute currentAttribute = null;

            // Start looping through the HTML (skip 1 char since we're already at the '<'
            tagParserState = TagParserState.ExpectingTagName;
            int currentPosition = startPosition + 1;
            while (currentPosition < _HTML.Length)
            {
                // Read char and advance
                char chr = _HTML[currentPosition];

                switch (tagParserState)
                {
                    #region TagParserState.ExpectingTagName - Look for an optional '/' and/or a tag name and possibly an ending '>' (if there's a '/' found)

                    /* 
                     * MATCHES:
                     * <DIV ATTRIBUTE="FOO" ATTR = 'BAR'> or </DIV>
                     *  ‾‾‾                                   ‾‾‾‾‾
                     */

                    // When we're start a tag and waiting for the tag name...
                    case TagParserState.ExpectingTagName:
                        {
                            if (isAlphaNumericChar(chr))
                            {
                                // A letter in the tag name - add it to sbTemp and read the rest of the tag name
                                tag.TagName = _readAlphaNumericWord(currentPosition);

                                if (tag.TagName.StartsWith("!--"))
                                {
                                    // HTML comment
                                    HTMLContent comment = new HTMLContent(startPosition, _readUntil(startPosition, "-->"));
                                    return comment;
                                }
                                else
                                {
                                    // Any tag conversions?
                                    switch(tag.TagName.ToLower())
                                    {
                                        case "form":
                                            tag = new HTMLForm(tag.StartPosition) { TagName = tag.TagName };
                                            break;
                                        case "input":
                                            tag = new HTMLInput(tag.StartPosition) { TagName = tag.TagName };
                                            break;
                                        case "select":
                                            tag = new HTMLSelect(tag.StartPosition) { TagName = tag.TagName };
                                            break;
                                        case "option":
                                            tag = new HTMLSelectOption(tag.StartPosition) { TagName = tag.TagName };
                                            break;
                                        case "textarea":
                                            tag = new HTMLTextarea(tag.StartPosition) { TagName = tag.TagName };
                                            break;
                                    }

                                    // Advance position by name length
                                    currentPosition += tag.TagName.Length;
                                    tagParserState = TagParserState.ExpectingTagContentsOrEnd;
                                }
                            }
                            else if (chr == '/')
                            {
                                // This is a closing tag like </div> - read the tag name and close it
                                tag.IsClosingTag = true;

                                // Advance to the start of the tag name and read it
                                currentPosition = this._indexOfNextNonWhitespaceChar(currentPosition + 1);
                                tag.TagName = _readAlphaNumericWord(currentPosition);
                                currentPosition += tag.TagName.Length;

                                // Advance to end of tag '>'
                                currentPosition += _readUntil(currentPosition, '>').Length - 1;
                                tagParserState = TagParserState.TagEnded;
                            }
                        }
                        break;
                    #endregion

                    #region TagParserState.ExpectingAttributeNameOrTagEnd - Inside the tag, looking for either alpha chars (start of an attribute), or a '/' self-closing flag, or the closing '>' character
                    case TagParserState.ExpectingTagContentsOrEnd:

                        // Advance to the next non-whitespace char
                        currentPosition = _indexOfNextNonWhitespaceChar(currentPosition);
                        chr = _HTML[currentPosition];

                        if (chr == '/')
                        {
                            /* MATCHES: <IMG />
                             *               ‾‾
                             */

                            // Self-closing tag
                            tag.SelfClosed = true;

                            // Advance to end of tag '>'
                            currentPosition += _readUntil(currentPosition, '>').Length - 1;
                            tagParserState = TagParserState.TagEnded;
                        }
                        else if (chr == '>')
                        {
                            /* MATCHES: <DIV>
                             *              ‾
                             */

                            // End of tag
                            tagParserState = TagParserState.TagEnded;
                        }
                        else if ((chr == '"') || (chr == '\''))
                        {
                            // Unnamed, quoted attribute value, like a DOCTYPE dtd path <!DOCTYPE html "blah blah">

                            // Read the quoted value
                            string attributeValue = _readValue(currentPosition);

                            // Build a new attribute
                            currentAttribute = new HTMLTagAttribute(currentPosition, null, attributeValue, chr.ToString());

                            // Advance the position
                            currentPosition += attributeValue.Length;

                            // Finish the attribute and clear it
                            currentAttribute.EndPosition = currentPosition;
                            tag.Attributes.Add(currentAttribute);
                            currentAttribute = null;
                        }
                        else if (isAlphaChar(chr))
                        {
                            /* 
                             * MATCHES:
                             * <DIV ATTRIBUTE="FOO" ATTR = 'BAR'>
                             *      ‾‾‾‾‾‾‾‾‾       ‾‾‾‾
                             */
                            // A letter in the attribute name - read the rest of the attribute
                            string attributeName = _readAlphaNumericWord(currentPosition);
                            currentAttribute = new HTMLTagAttribute(currentPosition, attributeName);

                            // Advance position to the end of the name
                            currentPosition += attributeName.Length;

                            // Do we have an attribute value?
                            int nextNonWhitespaceChar = _indexOfNextNonWhitespaceChar(currentPosition);
                            if (_HTML[nextNonWhitespaceChar] == '=')
                            {
                                // tagParserState = TagParserState.ExpectingAttributeValue;
                                currentPosition = nextNonWhitespaceChar + 1;

                                // Advance to the next non-whitespace char (in case of space-separated values like 'foo = "bar"'
                                nextNonWhitespaceChar = _indexOfNextNonWhitespaceChar(currentPosition);
                                string rawAttributeValue = _readValue(currentPosition);
                                currentAttribute.Value = rawAttributeValue;

                                // Advance position to end of the value
                                currentPosition += rawAttributeValue.Length;
                            }
                            else
                            {
                                // A standalone attributelike <!DOCTYPE html "foobar">
                                //                                      ‾‾‾‾
                            }

                            // End of attribute - mark the end position and add to the tag
                            currentAttribute.EndPosition = currentPosition;
                            tag.Attributes.Add(currentAttribute);

                            // Reset attribute
                            currentAttribute = null;
                        }
                        break;
                        #endregion
                }

                // End the tag?
                if (tagParserState == TagParserState.TagEnded)
                {
                    // Apply transformations?
                    if(_transforms == Transformations.LowercaseNames)
                    {
                        tag.TagName = tag.TagName.ToLower();
                        foreach(HTMLTagAttribute attr in tag.Attributes)
                        {
                            attr.Name = attr.Name.ToLower();
                        }
                    }
                    else if (_transforms == Transformations.UppercaseNames)
                    {
                        tag.TagName = tag.TagName.ToUpper();
                        foreach (HTMLTagAttribute attr in tag.Attributes)
                        {
                            attr.Name = attr.Name.ToUpper();
                        }
                    }

                    // Remove empty attributes list
                    if(tag.Attributes.Count == 0)
                    {
                        tag.Attributes = null;
                    }

                    // Mark the end position of the tag and return it
                    tag.MarkEndPosition(currentPosition);
                    return tag;
                }
            }

            // Shouldn't really get here...
            return tag;
        }
    }
}
