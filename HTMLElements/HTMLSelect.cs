using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hilgeman.HTMLElements
{
    public class HTMLSelect : HTMLInput
    {
        public override string InputType
        {
            get
            {
                return "select";
            }
        }


        public HTMLSelectOption GetSelectedOption()
        {
            if (Children == null) { return null; }
            foreach (DOMElement child in Children)
            {
                if (child is HTMLSelectOption)
                {
                    HTMLSelectOption childOption = (HTMLSelectOption)child;
                    if (childOption.Selected)
                    {
                        return childOption;
                    }
                }
            }
            return null;
        }

        public HTMLSelectOption GetOptionByValue(string Value)
        {
            if (Children == null) { return null; }
            foreach (DOMElement child in Children)
            {
                if (child is HTMLSelectOption)
                {
                    HTMLSelectOption childOption = (HTMLSelectOption)child;
                    if (childOption.Value == Value)
                    {
                        return childOption;
                    }
                }
            }
            return null;
        }

        public string Text
        {
            get
            {
                HTMLSelectOption childOption = GetSelectedOption();
                if (childOption != null)
                {
                    return childOption.Text;
                }
                else
                {
                    return null;
                }
            }
        }

        public override string Value
        {
            get
            {
                HTMLSelectOption childOption = GetSelectedOption();
                if (childOption != null)
                {
                    return childOption.Value;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                HTMLSelectOption childOption = GetSelectedOption();
                if (childOption != null)
                {
                    if (value == childOption.Value)
                    {
                        // It's already selected
                        return;
                    }
                    else
                    {
                        // Start by de-selecting the currently-selected option
                        childOption.Selected = false;
                    }
                }

                // Now select the desired one
                childOption = GetOptionByValue(value);
                if(childOption == null)
                {
                    // We don't have an <option> with that value!
                    throw new Exception("Trying to set " + this + " to " + value + ", but there's no <option> with that value!");
                }
                childOption.Selected = true;
            }
        }

        public List<HTMLSelectOption> Options = new List<HTMLSelectOption>();

        public HTMLSelect(int startPosition) : base(startPosition)
        {

        }
    }
}
