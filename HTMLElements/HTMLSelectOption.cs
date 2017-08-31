using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hilgeman.HTMLElements
{
    public class HTMLSelectOption : HTMLTag
    {
        public string Text
        {
            get
            {
                return InnerText();
            }
        }

        public bool Selected
        {
            get
            {
                if (Attributes == null) { return false; }
                foreach (HTMLTagAttribute a in Attributes)
                {
                    if (a.Name.ToLower() == "selected")
                    {
                        return true;
                    }
                }
                return false;
            }
            set
            {
                if (Attributes == null) { Attributes = new List<HTMLTagAttribute>();  }
                foreach (HTMLTagAttribute a in Attributes)
                {
                    // Is the option already selected?
                    if (a.Name.ToLower() == "selected")
                    {
                        // Trying to select what's already selected?
                        if (value)
                        {
                            return;
                        }
                        else
                        {
                            // Trying to de-select? Just remove the attribute.
                            Attributes.Remove(a);
                            break;
                        }
                    }
                }

                // Not yet selected but we want it to be...
                if (value)
                {
                    Attributes.Add(new HTMLTagAttribute(-1, "selected"));
                }
            }
        }

        public string Value
        {
            get
            {
                if (Attributes == null) { return null; }
                foreach (HTMLTagAttribute a in Attributes)
                {
                    if (a.Name.ToLower() == "value")
                    {
                        return a.Value;
                    }
                }
                return null;
            }

            set
            {
                // Initialize attributes if we don't have a container yet (unusual for an input)
                if (Attributes == null) { Attributes = new List<HTMLTagAttribute>(); }

                // Look for an existing value attribute and update it
                foreach (HTMLTagAttribute a in Attributes)
                {
                    if (a.Name.ToLower() == "value")
                    {
                        a.Value = value;
                        return;
                    }
                }

                // Didn't find one - set it
                Attributes.Add(new HTMLTagAttribute(-1, "value", value, "\""));
            }
        }

        public HTMLSelectOption(int startPosition) : base(startPosition)
        {

        }
    }
}
