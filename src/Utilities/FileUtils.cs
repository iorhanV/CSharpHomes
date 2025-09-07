using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;
// Revit API
using Autodesk.Revit.UI;
using Form = System.Windows.Forms.Form;


namespace CSharpHomes.Utilities;

public class FileUtils
{
#region Access links

        /// <summary>
        /// Used to verify if a URL is valid (will open).
        /// </summary>
        /// <param name="linkPath"">The path, typically a URL.</param>
        /// <returns>A boolean.</returns>
        public static bool LinkIsAccessible(string linkPath)
        {
            return Uri.TryCreate(linkPath, UriKind.Absolute, out Uri uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp
                   || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// Attempts to open a link in the default browser.
        /// </summary>
        /// <param name="linkPath"">The path, typically a URL.</param>
        /// <returns>A result.</returns>
        public static Result OpenLinkPath(string linkPath)
        {
            if (LinkIsAccessible(linkPath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo { FileName = linkPath, UseShellExecute = true });
                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while trying to open the URL: {ex.Message} ({linkPath})");
                    return Result.Failed;
                }
            }
            else
            {
                Console.WriteLine($"ERROR: Link path could not be opened ({linkPath})");
                return Result.Failed;
            }
        }

        #endregion

        #region Access files

        /// <summary>
        /// Runs an accessibility check on a file path.
        /// </summary>
        /// <param name="filePath"">The path.</param>
        /// <returns>A boolean.</returns>
        public static bool FileIsAccessible(string filePath)
        {
            // If the file doesn't exist, we return true (to allow creation)
            if (!File.Exists(filePath))
            {
                return true;
            }

            // Try to open the file with exclusive access
            try
            {
                using (var stream = new FileStream(filePath,
                    FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    // If we managed to run a stream, we can just return true
                    return true;
                }
            }
            // Otherwise the file was not accessible
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Attempts to open a file path.
        /// </summary>
        /// <param name="filePath"">The file path.</param>
        /// <returns>A result.</returns>
        public static Result OpenFilePath(string? filePath)
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = filePath, UseShellExecute = true });
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR: File path could not be opened {ex.Message} ({filePath})");
                return Result.Failed;
            }
        }

        #endregion
    
        #region Get doc path
        
        public static string GetDocPath(Document doc, UIApplication uiapp)
        {
            // If no document passed, use the active document
            doc = doc ?? uiapp.ActiveUIDocument.Document;

            if (doc.IsWorkshared)
            {
                ModelPath modelPath = doc.GetWorksharingCentralModelPath();
                return ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
            }
            
            return doc.PathName;
        }
        
        #endregion
        
        public static void SetFormIcon(Form form)
        {
            var iconPath = "geeWiz.Resources.Icons16.IconList16.ico";

            using (var stream = Globals.Assembly.GetManifestResourceStream(iconPath))
            {
                if (stream != null)
                {
                    form.Icon = new Icon(stream);
                }
            }
        }
        
        
}