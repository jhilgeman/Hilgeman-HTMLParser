using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hilgeman.HTMLElements
{
    public class DOMElement
    {
        public readonly ElementTypes Type;
        public readonly int StartPosition;
        protected int _endPosition;
        public int EndPosition
        {
            get
            {
                return _endPosition;
            }
        }

        public int Length
        {
            get
            {
                return EndPosition - StartPosition;
            }
        }

        public enum ElementTypes
        {
            Content,
            Tag,
            DOM
        }
        
        public DOMElement(ElementTypes type, int startPosition)
        {
            Type = type;
            StartPosition = startPosition;
        }

        protected void setEndPosition(int endPosition)
        {
            _endPosition = endPosition;
        }
    }
}
