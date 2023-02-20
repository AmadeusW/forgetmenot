namespace forgetmenot.Data;

using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

public class ChecklistService
{
    private readonly IChecklistParser Parser;
    private readonly IStateRepositoryProvider RepositoryProvider;

    List<Checklist> AllChecklists { get; set; }

    internal const string ChecklistPath = @"C:\src\forgetmenot\checklists";
    private bool Initialized { get; set; }
    public IList<Checklist> Archetypes { get; private set; }
    public IList<Checklist> FeaturedLists { get; private set; }

    public ChecklistService(IChecklistParser parser, IStateRepositoryProvider repositoryProvider)
    {
        this.Parser = parser;
        this.RepositoryProvider = repositoryProvider;
    }

    public async Task<IList<Checklist>> GetPrototypeChecklistsAsync()
    {
        await this.EnsureInitialized();
        return this.Archetypes;
    }

    public async Task<IList<Checklist>> GetFeaturedChecklistsAsync()
    {
        await this.EnsureInitialized();
        return this.FeaturedLists;
    }

    public async Task<IList<Checklist>> GetAllChecklistsAsync()
    {
        await this.EnsureInitialized();
        return this.AllChecklists;
    }

    public async Task<Checklist> GetChecklistAsync(string topicId, bool useArchetype, int version = -1)
    {
        await this.EnsureInitialized();
        var source = useArchetype ? this.Archetypes : this.FeaturedLists;
        var matchingTopic = source.Where(n => n.Id.TopicId == topicId);
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
        // Assuming this is not a prototype
        // note: the Checklist needs to declare within itself what it is, e.g. a prototype
        var serialized = this.Parser.Serialize(checklist);
        var filePath = string.IsNullOrEmpty(checklist.Id.FilePath)
            ? "lists\\" + checklist.Id.TopicId + "-" + checklist.Id.Version + ".md"
            : checklist.Id.FilePath;
        await File.WriteAllTextAsync(Path.Combine(ChecklistPath, filePath), serialized);
    }

    private async Task EnsureInitialized()
    {
        if (!this.Initialized)
        {
            this.AllChecklists = await ReadChecklistsAsync();
            await this.RepositoryProvider.InitializeAsync(this.AllChecklists);
            var checklistState = await this.RepositoryProvider.GetStateAsync();
            var prototypeIds = checklistState.Where(n => n.IsPrototype);
            var listIds = checklistState.Where(n => !n.IsPrototype);
            // TODO: this information should be embedded within checklists, and not in state.
            this.Archetypes = this.AllChecklists.Where(n => prototypeIds.Any(s => s.Identifier.Equals(n.Id))).ToList();
            this.FeaturedLists = this.AllChecklists.Where(n => listIds.Any(s => s.Identifier.Equals(n.Id))).ToList();
            this.Initialized = true;
        }
    }

    private async Task<List<Checklist>> ReadChecklistsAsync()
    {
        var list = new List<Checklist>();
        var files = Directory.EnumerateFiles(ChecklistPath, "*.md", SearchOption.AllDirectories);
        foreach (var filePath in files)
        {
            var raw = await File.ReadAllTextAsync(filePath);
            var checklist = await this.Parser.ParseAsync(raw);
            checklist.Id.FilePath = Path.GetRelativePath(ChecklistPath, filePath);
            list.Add(checklist);
        }
        this.AllChecklists = list;
        return list;
    }

    public Checklist GetChecklist(ChecklistId identifier)
    {
        if (!string.IsNullOrEmpty(identifier.FilePath))
        {
            var matchingFile = this.AllChecklists.SingleOrDefault(n => n.Id.FilePath == identifier.FilePath);
            if (matchingFile is not null)
            {
                return matchingFile;
            }
        }

        var matchingTopic = this.AllChecklists.Where(n => n.Id.TopicId == identifier.TopicId);
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
