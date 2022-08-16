namespace forgetmenot.Data;
using Microsoft.AspNetCore.Components;

public class ChecklistService
{
    ChecklistParser Parser {get; set;}
    List<Checklist> Checklists { get; set; }
    private const string ChecklistPath = @"C:\temp\checklists\";
    private bool Initialized { get; set; }

    public ChecklistService(ChecklistParser parser)
    {
        this.Parser = parser;
    }

    public async Task<List<Checklist>> GetAllChecklistsAsync()
    {
        await this.EnsureInitialized();
        return this.Checklists;
    }

    public async Task<Checklist> GetChecklistAsync(string title)
    {
        await this.EnsureInitialized();
        return this.Checklists.Single(n => n.Title == title);
    }

    public async void SaveChecklist(Checklist checklist)
    {
        var serialized = this.Parser.Serialize(checklist);
        await File.WriteAllTextAsync(Path.Combine(ChecklistPath, checklist.Title + ".md"), serialized);
    }

    private async Task EnsureInitialized()
    {
        if (!this.Initialized)
        {
            this.Checklists = await ReadChecklistsAsync();
            this.Initialized = true;
        }
    }

    private async Task<List<Checklist>> ReadChecklistsAsync()
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
}
