namespace forgetmenot.Data;
using Microsoft.AspNetCore.Components;

public class ChecklistService
{
    IChecklistParser Parser {get; set;}
    List<Checklist> Checklists { get; set; }

    private const string ChecklistPath = @"C:\src\forgetmenot\";
    private bool Initialized { get; set; }

    public ChecklistService(IChecklistParser parser)
    {
        this.Parser = parser;
    }

    public async Task<List<Checklist>> GetAllChecklistsAsync()
    {
        await this.EnsureInitialized();
        return this.Checklists;
    }

    public async Task<Checklist> GetChecklistAsync(string topicId, int version = -1)
    {
        await this.EnsureInitialized();
        var matchingTopic = this.Checklists.Where(n => n.Id.TopicId == topicId);
        if (version == -1)
        {
            return matchingTopic.OrderByDescending(n => n.Id.Version).First();
        }
        else
        {
            return matchingTopic.Single(n => n.Id.Version == version);
        }
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

    internal void ToggleItem(Checklist checklist, ChecklistItem item)
    {
        item.Done = !item.Done;

        FindAndUpdateParents(checklist, item);
        checklist.ModifiedDate = DateTime.Now;
        SaveChecklist(checklist);
    }

    private bool FindAndUpdateParents(Checklist checklist, ChecklistItem targetItem)
    {
        bool hasTarget = false;
        foreach (var item in checklist.Items.Where(n => n is EmbeddedChecklistItem))
        {
            var embeddedItem = (EmbeddedChecklistItem)item;
            var thisHasTarget = FindAndUpdateParents(embeddedItem.Checklist, targetItem);
            if (thisHasTarget)
            {
                embeddedItem.Done = embeddedItem.Checklist.Items.All(n => n.Done);
            }
            hasTarget |= thisHasTarget;
        }

        return hasTarget || checklist.Items.Contains(targetItem);
    }
}
