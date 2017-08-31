# Hilgeman-HTMLParser
A very basic, tiny HTML parser for C#. It was built primarily to extract form and input information from pages that used broken, inconsistent, or unreliable HTML code.

Examples:

    using Hilgeman;
    using Hilgeman.HTMLElements;
    
    ...
    
    try
    {
        // Read an HTML file
        DOMContainer results = HTMLParser.Parse(System.IO.File.ReadAllText(@"C:\path\to\somefile.html"));

        // Dump HTML from any HTMLTag node with ToHTML()
        string regeneratedHTML = results.ToHTML();

        // Get a list of all top-level tables
        List<HTMLTag> topTables = results.FindAll("table", false);

        // Get a list of all tables, including tables within tables
        List<HTMLTag> allTables = results.FindAll("table");

        // Get all HTMLForm elements and loop through the HTML inputs
        List<HTMLForm> allForms = results.FindAllForms();
        foreach (HTMLForm form in allForms)
        {
            // Output some basic form info
            Console.WriteLine("Form:\r\n  ACTION: " + form.Action + "\r\n  METHOD: " + form.Method + "\r\n  INPUTS:");

            // Loop through the named inputs
            foreach (HTMLInput namedInput in form.NamedInputs)
            {
                // For radio buttons and checkboxes, get the checked state
                bool isCheckableInput = (namedInput.InputType == "checkbox" || namedInput.InputType == "radio");
                string checkedState = (!isCheckableInput ? "" : (namedInput.Checked ? "Checked" : "Unchecked") + " ");

                // Output content like "myRadioButton = 123 (Checked radio)"
                Console.WriteLine("    " + namedInput.Name + " = " + namedInput.Value + 
                    " (" + (checkedState + namedInput.InputType) + ")");
            }

            // Ending blank space
            Console.WriteLine();
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show(ex.Message);
    }