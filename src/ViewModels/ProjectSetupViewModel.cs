using System.Collections.ObjectModel;
using System.ComponentModel;
using CSharpHomes.Models;

namespace CSharpHomes.ViewModels;
public class ProjectSetupViewModel
{
    public ObservableCollection<ProjectSetupModel> Setup { get; set; }
    public ProjectSetupViewModel(IEnumerable<DesignOption> designOptions, Dictionary<string, List<string>> pdfData)
    {
        Setup = new ObservableCollection<ProjectSetupModel>();

        // Add non-design option fields from PDF
        foreach (var kvp in pdfData)
        {
            if (kvp.Key == "Design Option")
            {
                // Each PDF Design Option value becomes its own row
                foreach (var option in kvp.Value)
                {
                    Setup.Add(new ProjectSetupModel
                    {
                        Description = "Design Option",
                        OriginalValue = option,
                        Input = option,
                        Apply = false,
                        // ComboBox source comes from Revit designOptions
                        Options = new List<string?>(
                            designOptions.Select(d => d.get_Parameter(BuiltInParameter.OPTION_NAME).AsString())
                        )
                    });
                }
            }
            else
            {
                // Regular single-value rows
                Setup.Add(new ProjectSetupModel
                {
                    Description = kvp.Key,
                    OriginalValue = kvp.Value.FirstOrDefault(), // keep PDF value
                    Input = kvp.Value.FirstOrDefault(),
                    Apply = false
                });
            }
        }
    }
    
}
