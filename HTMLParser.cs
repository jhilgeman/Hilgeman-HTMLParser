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
        // Parsing config
        private string _HTML;
        private bool _CaptureWhitespace;
        private Transformations _transforms;

        // Outputs
        public List<DOMElement> RawResults;
        public DOMContainer Results;

        // Options
        public enum Transformations
        {
            None,
            LowercaseNames,
            UppercaseNames
        }

        private HTMLParser(string HTML, bool CaptureWhitespace, Transformations transforms, bool processRaw)
        {
            _HTML = HTML;
            _CaptureWhitespace = CaptureWhitespace;
            _transforms = transforms;

            // Parse into raw results first
            RawResults = _ParseRaw();

            // Now process raw results into a hierarchy
            if (processRaw)
            {
                Results = Raw2Hierarchy(RawResults);
            }
        }


        

        #region Public Methods
        public static List<DOMElement> ParseRaw(string HTML, bool CaptureWhitespace = false, Transformations transforms = Transformations.None)
        {
            HTMLParser hsr = new HTMLParser(HTML, CaptureWhitespace, transforms, false);
            return hsr.RawResults;
        }

        public static DOMContainer Parse(string HTML, bool CaptureWhitespace = false, Transformations transforms = Transformations.None)
        {
            HTMLParser hsr = new HTMLParser(HTML, CaptureWhitespace, transforms, true);
            return hsr.Results;
        }
        #endregion

        #region Parsing
        private List<DOMElement> _ParseRaw()
        {
            // Top collection
            List<DOMElement> collection = new List<DOMElement>();
            DOMElement parsedElement = null;

            // Initial position
            int currentPosition = 0;

            // Loop through HTML
            while (currentPosition < _HTML.Length)
            {
                // Get next character
                char chr = _HTML[currentPosition];
                if (chr == '<')
                {
                    parsedElement = _ParseTag(currentPosition);
                    collection.Add(parsedElement);

                    if (parsedElement is HTMLContent)
                    {
                        currentPosition = parsedElement.EndPosition + 0;
                    }
                    else
                    {
                        currentPosition = parsedElement.EndPosition + 1;
                    }
                }
                else
                {
                    parsedElement = _ParseContent(currentPosition);
                    if (!_CaptureWhitespace && string.IsNullOrWhiteSpace(((HTMLContent)parsedElement).Value))
                    {
                        // Don't add the whitespace element if we're not capturing it
                    }
                    else
                    {
                        // Non-whitespace or we're okay with capturing whitespace
                        collection.Add(parsedElement);
                    }
                    currentPosition = parsedElement.EndPosition + 0;
                }

                // Advance cursor to next element
            }
            return collection;
        }


        private HTMLContent _ParseContent(int startPosition)
        {
            return new HTMLContent(startPosition, _readUntil(startPosition, '<', null, false));
        }
        #endregion


    }
}
