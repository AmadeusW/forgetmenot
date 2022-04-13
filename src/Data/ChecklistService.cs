namespace forgetmenot.Data;
using forgetmenot.Model;
using Microsoft.AspNetCore.Components;

public class ChecklistService
{
    ChecklistParser Parser {get; set;}

    public ChecklistService(ChecklistParser parser)
    {
        this.Parser = parser;
    }

    public Task<ChecklistQuickView[]> GetChecklistsAsync()
    {
        return Task.FromResult(new[] { 
            new ChecklistQuickView()
            {
                Title = "Max",
                Summary = "What to bring for Max",
            },
            new ChecklistQuickView()
            {
                Title = "Vacation",
                Summary = "Vacation todo",
            },
        });
    }

    public async Task<Checklist> GetChecklistAsync(string title)
    {
        var rawData = await Parser.LoadAsync(title);
        var parsed = await Parser.ParseAsync(rawData);
        return parsed;
    }
}
