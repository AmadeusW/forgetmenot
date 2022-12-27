using System.Diagnostics;

namespace forgetmenot.Data;

[DebuggerDisplay("{Title}")]
public class Checklist
{
    public string Title {get; set;}
    public string Summary { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string ModifiedBy { get; set; }
    public List<ChecklistItem> Items { get; set; } // Serialized as flat list
}

[DebuggerDisplay("{Name}")]
public class ChecklistItem
{
    public string Name { get; set; }
    public bool Done {get; set;}
}

public class ChecklistNode
{
    public ChecklistItem Item { get; set; }
    public ChecklistNode Parent { get; set; }
    public List<ChecklistNode> Children { get; set; }
}
