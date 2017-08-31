using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hilgeman.HTMLElements
{
    public class HTMLTag : DOMElement
    {
        public static string[] SelfClosingTags = new string[] { "!DOCTYPE", "AREA", "BASE", "BR", "COL", "COMMAND", "EMBED", "HR", "IMG", "INPUT", "KEYGEN", "LINK", "META", "PARAM", "SOURCE", "TRACK", "WBR" };
        public string ID
        {
            get
            {
                return GetAttributeValueByName("id");
            }
        }
        public string TagName;
        public bool SelfClosed = false;
        public bool IsClosingTag = false;
        public bool Closes
        {
            get
            {
                return !SelfClosingTags.Contains(this.TagName.ToUpper());
            }
        }
        public List<HTMLTagAttribute> Attributes = new List<HTMLTagAttribute>();
        public List<DOMElement> Children; // = new List<DOMElement>();

        public HTMLTag(int startPosition) : base(ElementTypes.Tag, startPosition)
        {
        }

        public void MarkEndPosition(int Position)
        {
            setEndPosition(Position);
        }

        public string GetAttributeValueByName(string attributeName, bool trimQuotes = true, string defaultValue = "")
        {
            HTMLTagAttribute attr = GetAttributeByName(attributeName, false);
            if (attr == null) { return defaultValue; }
            if(trimQuotes && (!string.IsNullOrEmpty(attr.QuoteChar)))
            {
                return attr.Value.Substring(1, attr.Value.Length - 2);
            }
            return attr.Value;
        }

        public HTMLTagAttribute GetAttributeByName(string attributeName, bool initializeAttributes = false)
        {
            if (Attributes == null)
            {
                if (initializeAttributes)
                {
                    Attributes = new List<HTMLTagAttribute>();
                }
                return null;
            }
            foreach (HTMLTagAttribute a in Attributes)
            {
                if (a.Name.ToLower() == attributeName.ToLower())
                {
                    return a;
                }
            }
            return null;
        }

        public List<HTMLForm> FindAllForms()
        {
            return this.FindAll("form", false, 0).Cast<HTMLForm>().ToList();
        }

        public List<HTMLTag> FindAll(string tagName, bool recurseIntoMatches = true, int depth = 0)
        {
            // Initialize empty return
            List<HTMLTag> result = new List<HTMLTag>();

            // If we don't have any children return the empty list
            if(Children == null)
            {
                return result;
            }

            // Normalize the name for searching purposes
            if (depth == 0)
            {
                tagName = tagName.Trim(new char[] { '<', '>' }).ToLower();
            }

            // Search children and recurse
            foreach(DOMElement child in Children)
            {
                if(child is HTMLTag)
                {
                    HTMLTag childTag = (HTMLTag)child;
                    if(childTag.TagName.ToLower() == tagName)
                    {
                        // Matching tag
                        result.Add(childTag);

                        // Recurse only if we've specified the flag (e.g. don't bother searching for <form> instead another <form>
                        if(recurseIntoMatches)
                        {
                            result.AddRange(childTag.FindAll(tagName, recurseIntoMatches, depth + 1));
                        }
                    }
                    else
                    {
                        // Non-matching tag, recurse
                        result.AddRange(childTag.FindAll(tagName, recurseIntoMatches, depth + 1));
                    }
                }
            }

            // Return the merged result
            return result;
        }

        public string InnerText()
        {
            if((Children == null) || (Children.Count == 0)) { return ""; }
            StringBuilder sb = new StringBuilder();
            foreach(DOMElement child in Children)
            {
                if(child is HTMLContent)
                {
                    sb.Append(((HTMLContent)child).Value);
                }
            }
            return sb.ToString();
        }

        #region HTML Output
        public override string ToString()
        {
            return "<" + (IsClosingTag ? "/" : "") + (this.TagName == null ? "UNNAMED TAG" : this.TagName) + (SelfClosed ? " /" : "") + ">";
        }

        private string GetOpenTag()
        {
            StringBuilder sb = new StringBuilder("<" + (this.TagName == null ? "UNNAMED TAG" : this.TagName));

            // Add attributes
            if ((Attributes != null) && (Attributes.Count > 0))
            {
                foreach (HTMLTagAttribute a in Attributes)
                {
                    sb.Append(" " + a.ToString());
                }
            }

            if (this.SelfClosed)
            {
                sb.Append(" /");
            }

            sb.Append(">");
            return sb.ToString();
        }

        private string GetCloseTag()
        {
            return "</" + (this.TagName == null ? "UNNAMED TAG" : this.TagName) + ">";
        }

        public string ToHTML(int Depth = -1, bool renderingOnNewLine = true)
        {
            // Normally we won't have closing tags in the final hierarchy, but just in case...
            if (IsClosingTag)
            {
                return GetCloseTag();
            }

            // Determine different factors that will weigh in on rendering
            int childCount = (Children == null ? 0 : Children.Count);
            bool onlyHasOneContentChild = ((childCount == 1) && (Children[0] is HTMLContent));
            bool hasGrandchildren = false;
            if (childCount > 0)
            {
                foreach (DOMElement child in Children)
                {
                    if(child is HTMLTag)
                    {
                        HTMLTag childTag = (HTMLTag)child;
                        if(childTag.Children != null)
                        {
                            hasGrandchildren = true;
                            break;
                        }
                    }
                }
            }

            // Normal render
            StringBuilder sb = new StringBuilder();
            string indent = (renderingOnNewLine ? "".PadLeft((Depth < 0 ? 0 : Depth), '\t') : "");
            bool childrenAreOnNewLine = ((hasGrandchildren) || (childCount > 1));

            // Don't render fake elements like our DOM container
            if (Depth >= 0)
            {
                if (childrenAreOnNewLine)
                {
                    sb.AppendLine(indent + GetOpenTag());
                }
                else
                {
                    sb.Append(indent + GetOpenTag());
                }
            }

            if ((Children != null) && (Children.Count > 0))
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    DOMElement child = Children[i];

                    if (child is HTMLTag)
                    {
                        // Child Tag
                        if (childrenAreOnNewLine)
                        {
                            sb.AppendLine(((HTMLTag)child).ToHTML(Depth + 1, childrenAreOnNewLine));
                        }
                        else
                        {
                            sb.Append(((HTMLTag)child).ToHTML(Depth + 1, childrenAreOnNewLine));
                        }
                    }
                    else
                    {
                        // Raw content
                        if (childrenAreOnNewLine)
                        {
                            sb.Append(((HTMLContent)child).Value); // Line
                        }
                        else
                        {
                            sb.Append(((HTMLContent)child).Value);
                        }
                    }
                }

            }

            if ((Depth >= 0) && Closes)
            {
                sb.Append((childrenAreOnNewLine ? indent : "") + GetCloseTag());
            }

            return sb.ToString();
        }
        #endregion
    }
}
