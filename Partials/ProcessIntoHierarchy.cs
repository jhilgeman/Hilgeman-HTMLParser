using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hilgeman.HTMLElements;

namespace Hilgeman
{
    public partial class HTMLParser
    {
        private DOMContainer Raw2Hierarchy(List<DOMElement> rawResults)
        {
            // Create a DOM container
            DOMContainer domObject = new DOMContainer();

            // Define the current parent element
            HTMLTag currentParent = domObject;

            // Define our starting stack
            List<HTMLTag> openTagStack = new List<HTMLTag>()
            {
                currentParent
            };

            // Keep a running reference to any active <form> tags and <select> tags
            HTMLForm currentForm = null;
            HTMLSelect currentSelect = null;

            // Pre-processing - find and update self-closing tags like <IMG> and <BR> and eliminate any 
            // unnecessary closing tags like </IMG> or </BR>
            List<DOMElement> elementsToRemove = new List<DOMElement>();
            foreach (DOMElement element in rawResults)
            {
                if (element is HTMLTag)
                {
                    HTMLTag elementTag = (HTMLTag)element;
                    string elementTagName = elementTag.TagName.ToUpper();
                    switch (elementTagName)
                    {
                        case "AREA":
                        case "BASE":
                        case "BR":
                        case "COL":
                        case "COMMAND":
                        case "EMBED":
                        case "HR":
                        case "IMG":
                        case "INPUT":
                        case "KEYGEN":
                        case "LINK":
                        case "META":
                        case "PARAM":
                        case "SOURCE":
                        case "TRACK":
                        case "WBR":
                            if (elementTag.IsClosingTag)
                            {
                                // </IMG> and </BR> and so on are invalid closing tags...
                                domObject.Warnings.Add("Marking " + element + " to be deleted (REASON: Self-closed tag)");
                                elementsToRemove.Add(elementTag);
                            }
                            else if (!elementTag.SelfClosed)
                            {
                                // <IMG> and <BR> and so on are self-closed tags...
                                domObject.Warnings.Add("Marking " + element + " as self-closed (REASON: Self-closed tag)");
                                elementTag.SelfClosed = true;
                            }
                            break;
                    }
                }
            }

            // Remove bad tags
            if (elementsToRemove.Count > 0)
            {
                foreach (DOMElement element in elementsToRemove)
                {
                    Console.WriteLine("Removing " + element);
                    rawResults.Remove(element);
                }
            }

            List<string> domErrors = new List<string>();
            List<string> domWarnings = new List<string>();

            while (rawResults.Count > 0)
            {
                // Shift off the beginning
                DOMElement nextElement = rawResults[0];
                rawResults.RemoveAt(0);

                // string indent = "".PadLeft(openTagStack.Count,'\t');

                // If it's an opening tag, let's update the parentElement
                if (nextElement is HTMLTag)
                {
                    // Cast once
                    HTMLTag nextElementTag = (HTMLTag)nextElement;

                    // Console.Write(indent + nextElementTag + ": ");

                    // If it's an opening tag
                    if (nextElementTag.IsClosingTag)
                    {
                        // Closing tag - try to match to parent
                        // Console.Write("Closing tag, trying to match to current parent " + currentParent + "... ");

                        // If this is a closing </form>, then null out currentForm
                        if(nextElementTag.TagName == "form")
                        {
                            currentForm = null;
                        }
                        // Else If this is a closing </select>, then null out currentSelect
                        else if (nextElementTag.TagName == "select")
                        {
                            currentSelect = null;
                        }

                        // Check to see if the current parent matches the current element (<td><p></p></td> and not malformed HTML like <p><td></p></td>)
                        if (nextElementTag.TagName == currentParent.TagName)
                        {
                            // Mark current parent as successfully closing
                            // currentParent.Closes = true;

                            // Closing tag - pop the stack
                            openTagStack.Remove(currentParent);
                            currentParent = openTagStack.Last();

                            // Console.WriteLine("Match - popped the stack and adding to end of new currentParent " + currentParent + ".");

                            // REMOVED - So the hierarchy only lists the open node and the closed can be 
                            //           inferred from the hierarchy.
                            // Move to current parent
                            // currentParent.Children.Add(nextElement);

                        }
                        else
                        {
                            // Console.WriteLine("Not a match - searching stack for a match...");

                            // Malformed HTML detected, try to find a matching open parent from the bottom to the top
                            bool foundStackSearchMatch = false;
                            for (int j = openTagStack.Count - 1; j >= 0; j--)
                            {
                                // Found it!
                                // Console.Write(indent + "  " + nextElementTag + " == " + openTagStack[j] + " ? ");
                                if (openTagStack[j].TagName == nextElementTag.TagName)
                                {
                                    domObject.Warnings.Add(nextElementTag + " was out of sequence. Current parent tag is " + currentParent + " but matching " + openTagStack[j] + " was found further outside.");

                                    // Console.WriteLine("Match! Moving to its parent.");
                                    foundStackSearchMatch = true;

                                    // REMOVED - See above reason.
                                    // Add to parent
                                    // openTagStack[j - 1].Children.Add(nextElement);
                                    // openTagStack[j - 1].Closes = true;

                                    // Remove that element from the open stack
                                    openTagStack.RemoveAt(j);
                                    break;
                                }
                            }
                            if (!foundStackSearchMatch)
                            {
                                // Uh-oh.... add it to the current parent
                                domObject.Errors.Add(nextElementTag + " did not match up to any open tag! Position in HTML: " + nextElementTag.StartPosition);
                                // currentParent.Children.Add(nextElement);
                                // Console.Write(indent + "  No matches found - adding to the currentParent " + currentParent);
                            }
                        }
                    }
                    else if (nextElementTag.SelfClosed)
                    {
                        // Self-closed tag
                        // Console.WriteLine("Self-closed tag, adding to current parent " + currentParent + "");

                        // Move to current parent
                        currentParent.Children.Add(nextElement);

                        // <input>s
                        if((nextElement is HTMLInput) && (currentForm != null))
                        {
                            currentForm.Inputs.Add((HTMLInput)nextElement);
                            if(((HTMLInput)nextElement).Name != null)
                            {
                                currentForm.NamedInputs.Add((HTMLInput)nextElement);
                            }
                        }

                        // <option />s
                        else if ((nextElement is HTMLSelectOption) && (currentSelect != null))
                        {
                            currentSelect.Options.Add((HTMLSelectOption)nextElement);
                        }
                    }
                    else
                    {
                        // Open tag - push onto the stack
                        // Console.WriteLine("Open tag, adding to currentParent " + currentParent + ", adding to stack, and setting as new currentParent.");

                        // Move to current parent
                        currentParent.Children.Add(nextElementTag);

                        // <select>s and <textarea>s
                        if ((nextElement is HTMLInput) && (currentForm != null))
                        {
                            currentForm.Inputs.Add((HTMLInput)nextElement);
                            if (((HTMLInput)nextElement).Name != null)
                            {
                                currentForm.NamedInputs.Add((HTMLInput)nextElement);
                            }

                            // Indicate we're in a <select> (for easier <option> association)
                            if(nextElement is HTMLSelect)
                            {
                                currentSelect = (HTMLSelect)nextElement;
                            }
                        }
                        // <option />s
                        else if ((nextElement is HTMLSelectOption) && (currentSelect != null))
                        {
                            currentSelect.Options.Add((HTMLSelectOption)nextElement);
                        }

                        // Make this the new currentParent
                        openTagStack.Add(nextElementTag);
                        currentParent = nextElementTag;

                        // Initialize children list
                        currentParent.Children = new List<DOMElement>();

                        // Update current form
                        if(currentParent is HTMLForm)
                        {
                            currentForm = (HTMLForm)currentParent;
                        }
                    }
                }
                else
                {
                    // Content goes into current parent
                    currentParent.Children.Add(nextElement);
                }
            }

            // Return final result
            return domObject;
        }
    }
}
