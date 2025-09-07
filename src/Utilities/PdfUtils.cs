using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSharpHomes.Utilities;

public static class PdfUtils
{
    public static Dictionary<string, List<string>> ExtractProjectData(string pdfText)
    {
        var projectData = new Dictionary<string, List<string>>();

        // Helper to add value to dictionary
        void AddField(string key, string value)
        {
            if (!projectData.ContainsKey(key))
                projectData[key] = new List<string>();
            projectData[key].Add(value.Trim());
        }

        // Extract main fields
        var projectNumberMatch = Regex.Match(pdfText, @"Project Number:\s*(\d+)");
        if (projectNumberMatch.Success) AddField("Project Number", projectNumberMatch.Groups[1].Value);

        var projectNameMatch = Regex.Match(pdfText, @"Project Name:\s*([^\r\n]+?)\s*(?:Project Type|Address|Client|Project Overview)");
        if (projectNameMatch.Success) AddField("Project Name", projectNameMatch.Groups[1].Value);

        var addressMatch = Regex.Match(pdfText, @"Address:\s*([^\r\n]+?)\s*(?:Client|Project Overview)");
        if (addressMatch.Success) AddField("Project Address", addressMatch.Groups[1].Value);

        var clientMatch = Regex.Match(pdfText, @"Client:\s*([^\r\n]+?)\s*(?:Project Overview|Design Options)");
        if (clientMatch.Success) AddField("Client Name", clientMatch.Groups[1].Value);

        // Extract Design Options (list)
        var designOptionsMatch = Regex.Match(pdfText, @"Design Options\s*(.+)", RegexOptions.Singleline);
        if (designOptionsMatch.Success)
        {
            string optionsText = designOptionsMatch.Groups[1].Value;
            var options = Regex.Split(optionsText, @"•")
                .Select(o => o.Trim())
                .Where(o => !string.IsNullOrEmpty(o));
            foreach (var option in options)
            {
                AddField("Design Option", option);
            }
        }

        return projectData;
    }
}