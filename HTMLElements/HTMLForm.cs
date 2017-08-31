using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hilgeman.HTMLElements
{
    public class HTMLForm : HTMLTag
    {
        public string Action
        {
            get
            {
                return GetAttributeValueByName("action");
            }
        }

        public string Method
        {
            get
            {
                return GetAttributeValueByName("method");
            }
        }


        public List<HTMLInput> Inputs = new List<HTMLInput>();
        public List<HTMLInput> NamedInputs = new List<HTMLInput>();

        public HTMLForm(int startPosition) : base(startPosition)
        {

        }
    }
}
