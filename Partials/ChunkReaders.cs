using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hilgeman
{
    public partial class HTMLParser
    {
        private string _readUntil(int startPosition, char untilChar, char? escapeChar = null, bool includeUntilChar = true)
        {
            // Where to begin our "IndexOf" searches
            int indexOfStartingOffset = startPosition;

            // Optional escape character
            char realEscapeChar = (char)0;
            if (escapeChar != null)
            {
                realEscapeChar = (char)escapeChar;
            }

            // Find unescaped target character
            string result = null;
            while (true)
            {
                int toPosition = _HTML.IndexOf(untilChar, indexOfStartingOffset);
                if (toPosition == -1)
                {
                    result = _HTML.Substring(startPosition);
                    break;
                }
                else
                {
                    // Check to see if this target char is escaped
                    if (_HTML[toPosition - 1] == escapeChar)
                    {
                        // Escape character detected - keep searching
                        indexOfStartingOffset = toPosition;
                    }
                    else
                    {
                        result = _HTML.Substring(startPosition, (toPosition - startPosition) + (includeUntilChar ? 1 : 0));
                        break;
                    }
                }
            }

            return result;
        }

        private string _readUntil(int startPosition, string untilString, bool includeTargetString = true)
        {
            // Find next character
            string result = null;
            int toPosition = _HTML.IndexOf(untilString, startPosition);
            if (toPosition == -1)
            {
                result = _HTML.Substring(startPosition);
            }
            else
            {
                result = _HTML.Substring(startPosition, (toPosition - startPosition) + (includeTargetString ? untilString.Length : 0));
            }
            return result;
        }

        private string _readWord(int startPosition)
        {
            int currentPosition = startPosition;
            while ((currentPosition < _HTML.Length) && isAlphaChar(_HTML[currentPosition]))
            {
                currentPosition++;
            }
            return _HTML.Substring(startPosition, (currentPosition - startPosition));
        }

        private string _readAlphaNumericWord(int startPosition)
        {
            int currentPosition = startPosition;
            while ((currentPosition < _HTML.Length) && isAlphaNumericChar(_HTML[currentPosition]))
            {
                currentPosition++;
            }
            return _HTML.Substring(startPosition, (currentPosition - startPosition));
        }

        



        private int _indexOfNextNonWhitespaceChar(int startPosition)
        {
            int currentPosition = startPosition;
            while ((currentPosition < _HTML.Length) && char.IsWhiteSpace(_HTML[currentPosition]))
            {
                currentPosition++;
            }
            return currentPosition;
        }


        private char _peekAtNextNonWhitespaceChar(int startPosition)
        {
            char nextNonWhitespaceChar = _HTML[_indexOfNextNonWhitespaceChar(startPosition)];
            return nextNonWhitespaceChar;
        }

        private string _readValue(int startPosition)
        {
            // Are we reading a quoted value?
            char QuoteChar = _HTML[startPosition];
            if ((QuoteChar == '"') || (QuoteChar == '\''))
            {
                // Yes, start with the quoted character and read the rest
                return QuoteChar + _readUntil(startPosition + 1, QuoteChar, '\\');
            }
            else
            {
                // Unquoted value
                return _readAlphaNumericWord(startPosition);
            }
        }

        private bool isAlphaChar(char chr)
        {
            return ((chr >= 65) && (chr <= 90)) || ((chr >= 97) && (chr <= 122));
        }

        private bool isAlphaNumericChar(char chr)
        {
            return ((chr >= 65) && (chr <= 90)) || ((chr >= 97) && (chr <= 122) || ((chr >= 48) && (chr <= 57)) || (chr == '-') || (chr == '!'));
        }
    }
}
