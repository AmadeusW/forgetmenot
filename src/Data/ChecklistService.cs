namespace forgetmenot.Data;

using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

public class ChecklistService
{
    private readonly IChecklistParser Parser;
    private readonly IStateRepositoryProvider RepositoryProvider;

    List<Checklist> Checklists { get; set; }

    internal const string ChecklistPath = @"C:\src\forgetmenot\";
    private bool Initialized { get; set; }
    public IList<Checklist> Prototypes { get; private set; }
    public IList<Checklist> Featured { get; private set; }

    public ChecklistService(IChecklistParser parser, IStateRepositoryProvider repositoryProvider)
    {
        this.Parser = parser;
        this.RepositoryProvider = repositoryProvider;
    }

    public async Task<IList<Checklist>> GetPrototypeChecklistsAsync()
    {
        await this.EnsureInitialized();
        return this.Prototypes;
    }

    public async Task<IList<Checklist>> GetFeaturedChecklistsAsync()
    {
        await this.EnsureInitialized();
        return this.Featured;
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
        await File.WriteAllTextAsync(Path.Combine(ChecklistPath, checklist.Id.TopicId + "-" + checklist.Id.Version + ".md"), serialized);
    }

    private async Task EnsureInitialized()
    {
        if (!this.Initialized)
        {
            this.Checklists = await ReadChecklistsAsync();
            await this.RepositoryProvider.InitializeAsync(this.Checklists);
            var checklistState = await this.RepositoryProvider.GetStateAsync();
            var prototypeIds = checklistState.Where(n => n.IsPrototype);
            var featuredIds = checklistState.Where(n => n.IsFeatured);
            this.Prototypes = this.Checklists.Where(n => prototypeIds.Any(s => s.Identifier.Equals(n.Id))).ToList();
            this.Featured = this.Checklists.Where(n => featuredIds.Any(s => s.Identifier.Equals(n.Id))).ToList();
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

    public Checklist GetChecklist(ChecklistId identifier)
    {
        var matchingTopic = this.Checklists.Where(n => n.Id.TopicId == identifier.TopicId);
        if (identifier.Version == -1)
        {
            var largestVersion = matchingTopic.Max(n => n.Id.Version);
            return matchingTopic.Single(n => n.Id.Version == largestVersion);
        }
        else
        {
            return matchingTopic.First(n => n.Id.Version == identifier.Version);
        }
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
            var embeddedChecklist = GetChecklist(((EmbeddedChecklistItem)item).ChecklistId);
            var thisHasTarget = FindAndUpdateParents(embeddedChecklist, targetItem);
            if (thisHasTarget)
            {
                item.Done = embeddedChecklist.Items.All(n => n.Done);
            }
            hasTarget |= thisHasTarget;
        }

        return hasTarget || checklist.Items.Contains(targetItem);
    }
}
