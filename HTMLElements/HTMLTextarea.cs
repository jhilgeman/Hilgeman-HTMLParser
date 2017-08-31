using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hilgeman.HTMLElements
{
    public class HTMLTextarea : HTMLInput
    {
        public override string Value
        {
            get
            {
                return InnerText();
            }

            set
            {
                // Initialize attributes if we don't have a container yet (unusual for an input)
                if (Children == null) { Children = new List<DOMElement>(); }

                // Look for an existing value attribute and update it
                foreach (DOMElement child in Children)
                {
                    if(child is HTMLContent)
                    {
                        ((HTMLContent)child).Value = value;
                        return;
                    }
                }

                // Didn't find one - set it
                Children.Add(new HTMLContent(-1, value));
            }
        }

        public HTMLTextarea(int startPosition) : base(startPosition)
        {

        }
    }
}
