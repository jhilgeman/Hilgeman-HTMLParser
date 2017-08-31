using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hilgeman.HTMLElements
{
    public class HTMLTagAttribute
    {
        public string Name;
        private string _Value;
        public string Value
        {
            get
            {
                return _Value;
            }
            set
            {
                // Handle null values
                if (value == null)
                {
                    _Value = value;
                }
                else
                {
                    // Remove surrounding quotes
                    if ((value[0] == '"') || (value[0] == '\''))
                    {
                        _Value = value.Substring(1, value.Length - 2);
                    }
                    else
                    {
                        // Unquoted attribute value like colspan=2
                        _Value = value;
                    }
                }
            }
        }
        public string QuoteChar;
        public int StartPosition;
        public int EndPosition;

        public HTMLTagAttribute(int startPosition, string Name, string Value = null, string QuoteChar = "")
        {
            this.Name = Name;
            this.Value = Value;
            this.QuoteChar = QuoteChar;
            this.StartPosition = startPosition;
        }

        public override string ToString()
        {
            if (this.Name == null)
            {
                // Doesn't have a name
                if (this.Value == null)
                {
                    // Shouldn't happen
                    return "<NEW ATTRIBUTE>";
                }
                else
                {
                    return QuoteChar + this.Value + QuoteChar;
                }
            }
            else
            {
                // Has a name
                if (this.Value == null)
                {
                    return this.Name;
                }
                else
                {
                    return this.Name + "=" + QuoteChar + this.Value + QuoteChar;
                }
            }
        }
    }
}
