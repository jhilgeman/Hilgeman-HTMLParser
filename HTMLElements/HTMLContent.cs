using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hilgeman.HTMLElements
{
    public class HTMLContent : DOMElement
    {
        public string Value;

        public HTMLContent(int startPosition, string Value) : base(ElementTypes.Content, startPosition)
        {
            this.Value = Value;
            setEndPosition(startPosition + Value.Length);
        }

        public override string ToString()
        {
            if (Value.Length <= 20)
            {
                return Value;
            }
            else
            {
                return Value.Substring(0, 20) + "... (" + (Value.Length - 20) + " more)";
            }
        }
    }
}
