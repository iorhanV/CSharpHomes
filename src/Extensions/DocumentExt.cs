using Autodesk.Revit.UI;
using View = Autodesk.Revit.DB.View;
// CSharpHomes
using cFrm = CSharpHomes.Forms;
using Autodesk.Revit.DB;

namespace CSharpHomes.Extensions;
public static class DocumentExt
{
    public static string GetDocPath(this Document doc)
    {
        if (doc.IsWorkshared)
        {
            ModelPath modelPath = doc.GetWorksharingCentralModelPath();
            return ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
        }
            
        return doc.PathName;
    }
    
    public static FilteredElementCollector Ext_Collector(this Document doc)
    {
        return new FilteredElementCollector(doc);
    }
    
    public static FilteredElementCollector Ext_Collector(this Document doc, View? view)
    {
        if (view is null) { return doc.Ext_Collector(); }
        return new FilteredElementCollector(doc, view.Id);
    }
    
    public static List<T> Ext_GetElementsOfClass<T>(this Document doc, View? view = null)
    {
        return doc.Ext_Collector(view)
            .OfClass(typeof(T))
            .WhereElementIsNotElementType()
            .Cast<T>()
            .ToList();
    }
    
    public static List<ViewSheet> Ext_GetSheets(this Document doc, bool sorted = false, bool includePlaceholders = false)
    {
        // Collect all viewsheets in document
        var sheets = doc.Ext_GetElementsOfClass<ViewSheet>();

        // Filter our placeholders if not desired
        if (!includePlaceholders)
        {
            sheets = sheets
                .Where(s => !s.IsPlaceholder)
                .ToList();
        }

        // Return the elements sorted or unsorted
        if (sorted)
        {
            return sheets
                .OrderBy(s => s.SheetNumber)
                .ToList();
        }
        else
        {
            return sheets;
        }
    }
    
    public static cFrm.FormResult<ViewSheet> Ext_SelectSheets(this Document doc, List<ViewSheet> sheets = null, string title = null,
        bool multiSelect = true, bool sorted = false, bool includePlaceholders = false, bool includeId = false)
    {
        // Set the default form title if not provided
        title ??= multiSelect ? "Select Sheet(s):" : "Select a sheet";

        // Get all Sheets in document if none provided
        sheets ??= doc.Ext_GetSheets(sorted: sorted, includePlaceholders: includePlaceholders);

        // Process into keys (to return)
        var keys = sheets
            .Select(s => s.Ext_ToSheetKey(includeId))
            .ToList();

        // Run the selection from list
        return cFrm.Custom.SelectFromList<ViewSheet>(keys: keys,
            values: sheets,
            title: title,
            multiSelect: multiSelect);
    }

    #region Revision collector/selection

    /// <summary>
    /// Gets all revisions in the given document.
    /// </summary>
    /// <param name="doc">A Revit document (extended).</param>
    /// <param name="sorted">Sort the revisions by sequence number.</param>
    /// <returns>A list of Revisions.</returns>
    public static List<Revision> Ext_GetRevisions(this Document doc, bool sorted = false)
    {
        // Collect all revisions in document
        var revisions = doc.Ext_Collector()
            .OfClass(typeof(Revision))
            .Cast<Revision>()
            .ToList();

        // Return the revisions sorted or unsorted
        if (sorted)
        {
            return revisions
                .OrderBy(r => r.SequenceNumber)
                .ToList();
        }
        else
        {
            return revisions;
        }
    }

    /// <summary>
    /// Select revision(s) from the document.
    /// </summary>
    /// <param name="doc">The Document (extended).</param>
    /// <param name="title">The form title (optional).</param>
    /// <param name="multiSelect">Select more than one item.</param>
    /// <param name="sorted">Sort the revisions by sequence.</param>
    /// <returns>A FormResult object.</returns>
    public static cFrm.FormResult<Revision> Ext_SelectRevisions(this Document doc, string title = null, bool multiSelect = true, bool sorted = false)
    {
        // Set the default form title if not provided
        title ??= multiSelect ? "Select Revision(s):" : "Select a Revision:";

        // Get all revisions in document
        var revisions = doc.Ext_GetRevisions(sorted: sorted);

        // Process into keys (to return)
        var keys = revisions
            .Select(r => r.Ext_ToRevisionKey())
            .ToList();

        // Run the selection from list
        return cFrm.Custom.SelectFromList<Revision>(keys: keys,
            values: revisions,
            title: title,
            multiSelect: multiSelect);
    }

    #endregion
    
}

