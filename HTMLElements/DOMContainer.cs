using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hilgeman.HTMLElements
{
    public class DOMContainer : HTMLTag
    {
        public List<string> Warnings = new List<string>();
        public List<string> Errors = new List<string>();

        public DOMContainer() : base(-1)
        {
            this.TagName = "DOM";
            this.Children = new List<DOMElement>();
        }
    }
}
