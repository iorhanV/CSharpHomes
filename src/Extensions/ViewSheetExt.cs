// Autodesk
using Autodesk.Revit.UI;
using CSharpHomes.Utilities;
using cView = CSharpHomes.Utilities.ViewUtils;
namespace CSharpHomes.Extensions;

public static class ViewSheetExt
{
    public static string Ext_ToSheetKey(this ViewSheet sheet, bool includeId = false)
    {
        // Null catch
        if (sheet is null) { return "???"; }
            
        // Return key with Id
        if (includeId)
        {
            return $"{sheet.SheetNumber}: {sheet.Name} [{sheet.Id.ToString()}]";
        }
        // Return key without Id
        else
        {
            return $"{sheet.SheetNumber}: {sheet.Name}";
        }
    }

    /// <summary>
    /// Constructs a name key for exporting.
    /// </summary>
    /// <param name="sheet">A Revit Sheet (extended).</param>
    /// <returns>A string.</returns>
    public static string Ext_ToExportKey(this ViewSheet sheet)
    {
        // Null catch
        if (sheet is null) { return "ERROR (-) - ERROR"; }

        // Get current revision
        string revisionNumber;

        if (sheet.GetCurrentRevision() != ElementId.InvalidElementId)
        {
            revisionNumber = sheet.GetRevisionNumberOnSheet(sheet.GetCurrentRevision());
        }
        else
        {
            revisionNumber = "-";
        }

        // Return sheet key
        var sheetKey = $"{sheet.SheetNumber} ({revisionNumber}) - {sheet.Name}";
        return StringUtils.MakeStringValid(sheetKey);
    }
    

    /// <summary>
    /// Exports a sheet to DWG.
    /// </summary>
    /// <param name="sheet">A revit sheet (extended).</param>
    /// <param name="fileName">The file name to use (do not include extension).</param>
    /// <param name="directoryPath">The directory to export to.</param>
    /// <param name="doc">The document (optional).</param>
    /// <param name="options">The export options (optional).</param>
    /// <returns>A Result.</returns>
    public static Result Ext_ExportToDwg(this ViewSheet sheet, string fileName, string directoryPath,
        Document doc = null, DWGExportOptions options = null)
    {
        // Ensure we have a sheet
        if (sheet is null) { return Result.Failed; }

        // Set document and/or options if not provided
        doc ??= sheet.Document;
        options ??= cView.DefaultDwgExportOptions();

        // Create the sheet list
        var sheetIds = new List<ElementId>() { sheet.Id };

        // Try to export to Dwg
        try
        {
            doc.Export(directoryPath, fileName, sheetIds, options);
            return Result.Succeeded;
        }
        catch
        {
            return Result.Failed;
        }
    }
    
}
