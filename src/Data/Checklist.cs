using System.Diagnostics;

namespace forgetmenot.Data;

[DebuggerDisplay("{Title}")]
public class Checklist
{
    public ChecklistId Id {get; set;}
    public string Title {get; set;}
    public string Summary { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string ModifiedBy { get; set; }
    public List<ChecklistItem> Items { get; set; }

    // Changed since last save?
    public bool ItemsChanged { get; set; }
    public bool NextItemChangeWillIncrementVersion { get; set; }
}

[DebuggerDisplay("{Name}")]
public class ChecklistItem
{
    public string Name { get; set; }
    public bool Done {get; set;}
    public DateTime ModifiedTime {get; set;}
    public string ModifiedId {get; set;}
}

[DebuggerDisplay("+{Name}")]
public class ParentChecklistItem : ChecklistItem
{
    public List<ChecklistItem> Items { get; set; }
}

[DebuggerDisplay(">{ChecklistId.Title}")]
public class EmbeddedChecklistItem : ChecklistItem
{
    public ChecklistId ChecklistId {get; set;}
}

[DebuggerDisplay("Checklist {TopicId}@{Version}")]
public class ChecklistId
{
    public string TopicId {get; set;}
    public int Version {get;set;}
    public string FilePath { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not ChecklistId other)
        {
            return false;
        }
        if (this.FilePath is not null && other.FilePath is not null)
        {
            return this.FilePath.Equals(other.FilePath);
        }
        return this.TopicId == other.TopicId && this.Version == other.Version;
    }
}

[DebuggerDisplay("Checklist+ {Identifier}")]
public class ChecklistWithState
{
    public bool IsFeatured { get; set; }
    public bool IsPrototype { get; set; }
    public ChecklistId Identifier { get; set; }
}

public class ChecklistRepository : List<ChecklistWithState>
{

}
