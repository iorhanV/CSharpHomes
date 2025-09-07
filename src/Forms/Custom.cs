// Revit API
using Autodesk.Revit.UI;
// CSharpHomes libraries
using cFrm = CSharpHomes.Forms;
// using gCnv = CSharpHomes.Utilities.Convert_Utils;

// The class belongs to the forms namespace
// using gFrm = CSharpHomes.Forms (+ .Custom)
namespace CSharpHomes.Forms
{
    // These classes all form the front end selection forms in Revit
    public static class Custom
    {
        #region File filter constants

        // File filter constant values
        public static string FILTER_TSV = "TSV Files (*.tsv)|*.tsv";
        public static string FILTER_EXCEL = "Excel Files (*.xls;*.xlsx;*.xlsm)|*.xls;*.xlsx;*.xlsm";
        public static string FILTER_RFA = "Family Files|*.rfa";
        public static string FILTER_TXT = "Text Files (*.txt)|*.txt";

        #endregion
        
        #region Message (+ variants)

        /// <summary>
        /// Processes a generic message to the user.
        /// </summary>
        /// <param name="title">An optional title to display.</param>
        /// <param name="message">An optional message to display.</param>
        /// <param name="yesNo">Show Yes and No options instead of OK and Cancel.</param>
        /// <param name="noCancel">Does not offer a cancel button.</param>
        /// <param name="icon">The icon type to display.</param>
        /// <returns>A FormResult object.</returns>
        public static FormResult<bool> Message(string title = null, string message = null,
            bool yesNo = false, bool noCancel = false, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            // Establish the form result to return
            var formResult = new FormResult<bool>(valid: false);

            // Default values if not provided
            title ??= "Message";
            message ??= "No description provided.";

            // Set the question icon
            if (yesNo) { icon = MessageBoxIcon.Question; }

            // Set the available buttons
            MessageBoxButtons buttons;

            if (noCancel)
            {
                buttons = MessageBoxButtons.OK;
            }
            else
            {
                if (yesNo) { buttons = MessageBoxButtons.YesNo; }
                else { buttons = MessageBoxButtons.OKCancel; }
            }

            // Create a messagebox, process its dialog result
            var dialogResult = MessageBox.Show(message, title, buttons, icon);

            // Process the outcomes
            if (dialogResult == DialogResult.Yes || dialogResult == DialogResult.OK)
            {
                formResult.Validate();
            }

            // Return the outcome
            return formResult;
        }

        /// <summary>
        /// Displays a generic completed message.
        /// </summary>
        /// <param name="message">An optional message to display.</param>
        /// <returns>Result.Succeeded.</returns>
        public static Result Completed(string message = null)
        {
            // Default message
            message ??= "Task completed.";

            // Show form to user
            Message(message: message,
                title: "Task completed",
                noCancel: true,
                icon: MessageBoxIcon.Information);

            // Return a succeeded result
            return Result.Succeeded;
        }

        /// <summary>
        /// Displays a generic cancelled message.
        /// </summary>
        /// <param name="message">An optional message to display.</param>
        /// <returns>Result.Cancelled.</returns>
        public static Result Cancelled(string message = null)
        {
            // Default message
            message ??= "Task cancelled.";

            // Show form to user
            Message(message: message,
                title: "Task cancelled",
                noCancel: true,
                icon: MessageBoxIcon.Warning);

            // Return a cancelled result
            return Result.Cancelled;
        }

        /// <summary>
        /// Displays a generic error message.
        /// </summary>
        /// <param name="message">An optional message to display.</param>
        /// <returns>Result.Failed.</returns>
        public static Result Error(string message = null)
        {
            // Default message
            message ??= "Error encountered.";

            // Show form to user
            Message(message: message,
                title: "Error",
                noCancel: true,
                icon: MessageBoxIcon.Error);

            // Return a cancelled result
            return Result.Failed;
        }

        #endregion

        #region Select files / directory

        /// <summary>
        /// Select file path(s) from a browser dialog.
        /// </summary>
        /// <param name="title">An optional title to display.</param>
        /// <param name="filter">An optional file type filter.</param>
        /// <param name="multiSelect">If we want to select more than one file path.</param>
        /// <returns>A FormResult object.</returns>
        public static FormResult<string> SelectFilePaths(string title = null, string filter = null, bool multiSelect = true)
        {
            // Establish the form result to return
            var formResult = new FormResult<string>(valid: false);

            // Using a dialog object
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Default title and filter
                title ??= multiSelect ? "Select file(s)" : "Select a file";
                if (filter is not null) { openFileDialog.Filter = filter; }

                // Set the typical settings
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Title = title;
                openFileDialog.Multiselect = multiSelect;

                // Process the results
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var filePaths = openFileDialog.FileNames.ToList();

                    if (multiSelect) { formResult.Validate(filePaths); }
                    else { formResult.Validate(filePaths.First()); }
                }
            }

            // Return the outcome
            return formResult;
        }

        /// <summary>
        /// Select a directory path from a browser dialog.
        /// </summary>
        /// <param name="title">An optional title to display.</param>
        /// <returns>A FormResult object.</returns>
        public static FormResult<string> SelectDirectoryPath(string title = null)
        {
            // Establish the form result to return
            var formResult = new FormResult<string>(valid: false);

            // Default title
            title ??= "Select folder";

            // Using a dialog object
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog() { Description = title })
            {
                // Process the result
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    formResult.Validate(folderBrowserDialog.SelectedPath);
                }
            }

            // Return the outcome
            return formResult;
        }

        #endregion

        #region SelectFromList

        /// <summary>
        /// Processes a generic form for showing objects in a list with a text filter.
        /// </summary>
        /// <param name="keys">A list of keys to display.</param>
        /// <param name="values">A list of values to pass by key.</param>
        /// <param name="title">An optional title to display.</param>
        /// <param name="multiSelect">If we want to select more than one item.</param>
        /// <typeparam name="T">The type of object being stored.</typeparam>
        /// <returns>A FormResult object.</returns>
        public static FormResult<T> SelectFromList<T>(List<string> keys, List<T> values,
            string title = null, bool multiSelect = true)
        {
            // Establish the form result to return
            var formResult = new FormResult<T>(valid: false);

            // Default title
            title ??= multiSelect ? "Select object(s) from list:" : "Select object from list:";

            // Using a select items form
            using (var form = new cFrm.Bases.BaseListView<T>(keys, values, title: title, multiSelect: multiSelect))
            {
                // Process the outcome
                if (form.ShowDialog() == DialogResult.OK)
                {
                    if (multiSelect) { formResult.Validate(form.Tag as List<T>); ; }
                    else { formResult.Validate((T)form.Tag); } 
                }
            }

            // Return the result
            return formResult;
        }
        #endregion
    }

    #region FormResult class

    /// <summary>
    /// A class for holding form outcomes, used by custom forms.
    /// </summary>
    /// <typeparam name="T">The type of object being stored.</typeparam>
    public class FormResult<T>
    {
        // These properties hold the resulting object or objects from the form
        public List<T> Objects { get; set; }
        public T Object { get; set; }

        // These properties allow us to verify the outcome of the form
        public bool Cancelled { get; set; }
        public bool Valid { get; set; }
        public bool Affirmative { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public FormResult() { }

        /// <summary>
        /// Constructor to begin a FormResult.
        /// </summary>
        /// <param name="valid">Should the result begin as valid.</param>
        public FormResult(bool valid)
        {
            Objects = new List<T>();
            Object = default;
            Cancelled = !valid;
            Valid = valid;
            Affirmative = valid;
        }

        /// <summary>
        /// Sets all dialog related results to valid.
        /// </summary>
        public void Validate()
        {
            Cancelled = false;
            Valid = true;
            Affirmative = true;
        }

        /// <summary>
        /// Sets the dialog result to valid, passing an object.
        /// </summary>
        /// <param name="obj">Object to pass to result.</param>
        public void Validate(T obj)
        {
            this.Validate();
            this.Object = obj;
        }

        /// <summary>
        /// Sets the dialog result to valid, passing a list of objects.
        /// </summary>
        /// <param name="objs">Objects to pass to result.</param>
        public void Validate(List<T> objs)
        {
            this.Validate();
            this.Objects = objs;
        }
    }

    #endregion
}