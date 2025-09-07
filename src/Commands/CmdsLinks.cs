// Revit API

using System.IO;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using CSharpHomes.Extensions;
using cFil = CSharpHomes.Utilities.FileUtils;

namespace CSharpHomes.CmdsLinks;

[Transaction(TransactionMode.Manual)]
public class CmdResCode : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        // Open the Url
        string linkPath = @"https://www.planning.vic.gov.au/guides-and-resources/guides/all-guides/residential-development";
        return cFil.OpenLinkPath(linkPath);
    }
}

[Transaction(TransactionMode.Manual)]
public class CmdNCC : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        // Open the Url
        string linkPath = @"https://ncc.abcb.gov.au/editions/ncc-2022";
        return cFil.OpenLinkPath(linkPath);
    }
}

[Transaction(TransactionMode.Manual)]
public class CmdFolder : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        // Open the Url
        var uiApp = commandData.Application;
        var uiDoc = uiApp.ActiveUIDocument;
        var doc = uiDoc.Document;

        string filePath = doc.GetDocPath();
        string? cleanPath = Path.GetDirectoryName(filePath);
        return cFil.OpenFilePath(cleanPath);
    }
}

