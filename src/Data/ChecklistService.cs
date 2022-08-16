namespace forgetmenot.Data;
using Microsoft.AspNetCore.Components;

public class ChecklistService
{
    ChecklistParser Parser {get; set;}
    List<Checklist> Checklists { get; set; }
    private const string ChecklistPath = @"C:\temp\checklists\";

    public ChecklistService(ChecklistParser parser)
    {
        this.Parser = parser;
    }

    internal async Task<List<Checklist>> ReadChecklistsAsync()
    {
        var list = new List<Checklist>();
        var files = Directory.EnumerateFiles(ChecklistPath, "*.md");
        foreach (var filePath in files)
        {
            var raw = await File.ReadAllTextAsync(filePath);
            var checklist = await this.Parser.ParseAsync(raw);
            list.Add(checklist);
        }
        this.Checklists = list;
        return list;
    }

    public async Task<Checklist> GetChecklistAsync(string title)
    {
        return this.Checklists.Single(n => n.Title == title);
    }
}
