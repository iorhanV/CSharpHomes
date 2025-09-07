using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using CSharpHomes.Extensions;
using CSharpHomes.Utilities;
using CSharpHomes.ViewModels;
using CSharpHomes.Views;
using CSharpHomes.Views.Utils;
using OperationCanceledException = Autodesk.Revit.Exceptions.OperationCanceledException;
using cFil = CSharpHomes.Utilities.FileUtils;
using cFrm = CSharpHomes.Forms;
using cXcl = CSharpHomes.Utilities.ExcelUtils;
using UglyToad.PdfPig;

namespace CSharpHomes.CmdsTools;

[Transaction(TransactionMode.Manual)]
public class CmdRotate : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        // Collect the document
        var uiApp = commandData.Application;
        var uiDoc = uiApp.ActiveUIDocument;
        var doc = uiDoc.Document;
        Double angle = 45;
        ICollection<Element> selElement = new List<Element>();
        
        try
        {
            var options = SetupOptionsBar();
            selElement = PickToElements(uiDoc, doc);  
            angle = options.Angle;
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
        finally
        {
            using var transaction = new Transaction(doc);
            transaction.Start("Rotate Elements");
            foreach (Element elem in selElement)
            {
                RotateElements(elem, doc, angle);
            }
            transaction.Commit();
            RibbonController.HideOptionsBar();
        }

        RibbonController.HideOptionsBar();
        
        return Result.Succeeded;
    }

    private RotateViewModel SetupOptionsBar()
    {
        var options = new RotateViewModel
        {
            Angle = 0.0
        };
        
        var view = new RotateView(options);
        
        RibbonController.ShowOptionsBar(view);
        
        return options;
    }
    
    private static ICollection<Element> PickToElements(UIDocument uiDoc, Document doc)
    {
        ICollection<Element> selectedElements = new List<Element>();
        
        IList<Reference> selRef = uiDoc.Selection.PickObjects(ObjectType.Element, "Pick elements to rotate");
        if (selRef != null)
        {
            selectedElements = selRef.Select(doc.GetElement).ToList();
        }
        return selectedElements;
    }
    
    public static void RotateElements(Element elem, Document doc,  double angle)
    {
        BoundingBoxXYZ bBox = elem.get_BoundingBox(doc.ActiveView);
        XYZ pointBox = (bBox.Min + bBox.Max) / 2;
        
        // Create Vertical Axis Line
        Line axisLine = Line.CreateBound(pointBox, pointBox + XYZ.BasisZ);
        
        // Convert to radians
        double radians = (Math.PI / 180) * angle;
        
        ElementTransformUtils.RotateElement(doc, elem.Id, axisLine, radians);
    }
}


[Transaction(TransactionMode.Manual)]
public class CmdExpCAD : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        // Get the document
        var uiApp = commandData.Application;
        var uiDoc = uiApp.ActiveUIDocument;
        var doc = uiDoc.Document;

        // Select sheets to export
        var formResults = doc.Ext_SelectSheets(title: "Select sheets to export", sorted: true);
        if (formResults.Cancelled) { return Result.Cancelled; }
        var sheets = formResults.Objects;

        // Select directory to export to
        string? directoryPath = null;
        using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog() { Description = @"Select Folder" })
        {
            // Process the result
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                directoryPath = folderBrowserDialog.SelectedPath;
            }
        }

        // Dwg export options
        var options = new DWGExportOptions();

        // Configure the settings
        options.MergedViews = true;
        options.FileVersion = ACADVersion.R2013;
        
        
        // Using a transaction
        using (var t = new Transaction(doc, "Export sheets"))
        {
            // Start the transaction
            t.Start();

            // For each sheet
            foreach (var sheet in sheets)
            {
                // Export the sheet to Dwg
                sheet.Ext_ExportToDwg(
                    fileName: sheet.Ext_ToExportKey(),
                    directoryPath: directoryPath,
                    doc: doc,
                    options: options);
            }

            // Commit the transaction
            t.Commit();
        }

        // Finish by opening the directory path
        return cFil.OpenFilePath(directoryPath);
    }
}

/// <summary>
/// Creates a document transmittal.
/// </summary>
[Transaction(TransactionMode.Manual)]
public class CmdDocTrans : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        // Get the document
        var uiApp = commandData.Application;
        var uiDoc = uiApp.ActiveUIDocument;
        var doc = uiDoc.Document;

        // Select a revision
        var formResultRevision = doc.Ext_SelectRevisions(sorted: true);
        if (formResultRevision.Cancelled) { return Result.Cancelled; }
        var revisions = formResultRevision.Objects;

        // Select sheets
        var formResultSheets = doc.Ext_SelectSheets(sorted: true);
        if (formResultSheets.Cancelled) { return Result.Cancelled; }
        var sheets = formResultSheets.Objects;

        // Construct doctans, header row
        var matrix = new List<List<string>>();
        var header = new List<string>() { "Number", "Name", "Current"};
        var revisionIds = new List<ElementId>();

        foreach (var revision in revisions)
        {
            header.Add(revision.Ext_ToRevisionKey());
            revisionIds.Add(revision.Id);
        }

        // Add header row to matrix
        matrix.Add(header);

        // For each sheet
        foreach (var sheet in sheets)
        {
            // New row with sheet number and name
            var row = new List<string>()
            {
                sheet.SheetNumber,
                sheet.Name
            };

            // Get current revision
            if (sheet.GetCurrentRevision() != ElementId.InvalidElementId)
            {
                row.Add(sheet.GetRevisionNumberOnSheet(sheet.GetCurrentRevision()));
            }
            else
            {
                row.Add("-");
            }

            // For each revision
            foreach (var id in revisionIds)
            {
                // Add its number to the row for the sheet
                var revisionNumber = sheet.GetRevisionNumberOnSheet(id);
                revisionNumber ??= "";
                row.Add(revisionNumber);
            }

            // Add row to matrix
            matrix.Add(row);
        }

        // Select directory
        var directoryResult = cFrm.Custom.SelectDirectoryPath("Select where to save the transmittal");
        if (directoryResult.Cancelled) { return Result.Cancelled; }
        var directoryPath = directoryResult.Object;
        var filePath = Path.Combine(directoryPath, "Doctrans.xlsx");

        // Accessibility check if it exists
        if (File.Exists(filePath))
        {
            if (!cFil.FileIsAccessible(filePath))
            {
                return cFrm.Custom.Cancelled(
                    "File exists and is not editable.\n\n" +
                    "Ensure it is closed and try again.");
            }
        }

        // Using a workbook object
        using (var workbook = cXcl.CreateWorkbook(filePath))
        {
            // Establish workbook variable
            ClosedXML.Excel.IXLWorksheet worksheet = null;

            // If the file exists, clear its contents
            if (File.Exists(filePath))
            {
                worksheet = cXcl.GetWorkSheet(workbook: workbook,
                    worksheetName: "Doctrans", getFirstOtherwise: true);
                worksheet.Clear();
            }
            else
            {
                // Otherwise, add the worksheet
                worksheet = workbook.AddWorksheet("Doctrans");
            }

            // Write the matrix to the workbook
            cXcl.WriteToWorksheet(worksheet, matrix);

            // Make the header row taller
            worksheet.Row(1).Height = 150;

            // For each column...
            for (int i = 1; i <= revisionIds.Count + 3; i++)
            {
                // First 3 columns set to 30 wide
                if (i < 3)
                {
                    worksheet.Column(i).Width = 30;
                }
                // Remainder set to 5, roated 90 degrees (revision columns)
                else
                {
                    worksheet.Cell(1, i).Style.Alignment.TextRotation = 90;
                    worksheet.Column(i).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                    worksheet.Column(i).Width = 5;
                }
            }

            // If the workbook exists...
            if (File.Exists(filePath))
            {
                // Save it
                workbook.Save();
            }
            else
            {
                // Otherwise save it to the file path
                workbook.SaveAs(filePath);
            }
        }
        
        return Result.Succeeded;
    }
}

/// <summary>
/// Project Setup Utility.
/// </summary>
[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class CmdProjSetup : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        // Get the document
        var uiApp = commandData.Application;
        var uiDoc = uiApp.ActiveUIDocument;
        var doc = uiDoc.Document;
        
        // Select a file
        var openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = @"PDF files (*.pdf)|*.pdf";
        openFileDialog.Title = @"Select PDF File";
        string pdfPath = "";

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            pdfPath = openFileDialog.FileName;
        }
        
        // Read PDF Data
        string pdfText = "";

        using (var document = PdfDocument.Open(pdfPath))
        {
            foreach (var page in document.GetPages())
            {
                pdfText += page.Text;
            }
        }

        var pdfData = PdfUtils.ExtractProjectData(pdfText);
        
        // // Get parameters and design options
        // var projInfo = doc.ProjectInformation;
        // var clientName = projInfo.get_Parameter(BuiltInParameter.CLIENT_NAME);
        // var projAddress = projInfo.get_Parameter(BuiltInParameter.PROJECT_ADDRESS);
        // var projName = projInfo.get_Parameter(BuiltInParameter.PROJECT_NAME);
        // var projNumber = projInfo.get_Parameter(BuiltInParameter.PROJECT_NUMBER);
        // var designOptions = doc.Ext_GetElementsOfClass<DesignOption>();
        //
        
        // Get parameters and design options
        var projInfo = doc.ProjectInformation;
        var designOptions = doc.Ext_GetElementsOfClass<DesignOption>();
        
        // Launch WPF
        var viewModel = new ProjectSetupViewModel(designOptions, pdfData);
        var view = new ProjectSetupView(viewModel);
        bool? result = view.ShowDialog();
        
        if (result == true)
        {
            using (var t = new Transaction(doc, "Project Setup"))
            {
                t.Start();
        
                // Apply changes the user approved
                foreach (var item in viewModel.Setup.Where(s => s.Apply))
                {
                    switch (item.Description)
                    {
                        case "Project Number":
                            projInfo.get_Parameter(BuiltInParameter.PROJECT_NUMBER).Set(item.Input);
                            break;
                        case "Project Name":
                            projInfo.get_Parameter(BuiltInParameter.PROJECT_NAME).Set(item.Input);
                            break;
                        case "Client Name":
                            projInfo.get_Parameter(BuiltInParameter.CLIENT_NAME).Set(item.Input);
                            break;
                        case "Project Address":
                            projInfo.get_Parameter(BuiltInParameter.PROJECT_ADDRESS).Set(item.Input);
                            break;
                    }
                }
                
                // Commit Design Option selections
                // Get the list of selected design option names from the user
                var selectedOptions = viewModel.Setup
                    .Where(s => s.Apply && s.Description == "Design Option")
                    .Select(s => s.Input)
                    .ToList();

                foreach (var option in designOptions)
                {
                    string optionName = option.get_Parameter(BuiltInParameter.OPTION_NAME).AsString();
                    if (!selectedOptions.Contains(optionName))
                    {
                        doc.Delete(option.Id);
                    }
                }
        
                t.Commit();
            }
        }
        
        else
        {
            // User cancelled or closed window
            TaskDialog.Show("Cancelled", "No changes were applied to the project.");
            return Result.Cancelled;
        }
        
        // After applying changes, show Design Options dialog
        uiApp.PostCommand(RevitCommandId.LookupPostableCommandId(PostableCommand.DesignOptions));

        
        return Result.Succeeded;
    }
}
