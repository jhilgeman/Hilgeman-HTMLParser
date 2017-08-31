using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hilgeman.HTMLElements
{
    public class HTMLForm : HTMLTag
    {
        public List<HTMLInput> Inputs = new List<HTMLInput>();
        public List<HTMLInput> NamedInputs = new List<HTMLInput>();

        public HTMLForm(int startPosition) : base(startPosition)
        {

        }
    }
}
