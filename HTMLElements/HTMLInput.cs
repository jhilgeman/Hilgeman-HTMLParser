using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hilgeman.HTMLElements
{
    public class HTMLInput : HTMLTag
    {
        public override string ToString()
        {
            return Name + "=" + Value;
        }

        public string Name
        {
            get
            {
                HTMLTagAttribute attr = GetAttributeByName("name");
                if (attr != null)
                {
                    return attr.Value;
                }
                return null;
            }
        }

        public virtual string Value
        {
            get
            {
                HTMLTagAttribute attr = GetAttributeByName("value");
                if(attr != null)
                {
                    return attr.Value;
                }
                return null;
            }

            set
            {
                // Look for an existing value attribute and update it
                HTMLTagAttribute attr = GetAttributeByName("value", true);
                if (attr != null)
                {
                    attr.Value = value;
                    return;
                }

                // Didn't find one - set it
                Attributes.Add(new HTMLTagAttribute(-1, "value", value, "\""));
            }
        }

        

        public HTMLInput(int startPosition) : base(startPosition)
        {

        }
    }
}
