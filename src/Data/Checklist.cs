using System.Diagnostics;

namespace forgetmenot.Data;

[DebuggerDisplay("{Title}")]
public class Checklist
{
    public string Title {get; set;}
    public string Summary { get; set; }
    public List<ChecklistItem> Items {get; set;}
    public DateTime LastModified { get; set; } // not serialized ATM
    public string LastModifiedBy { get; set; } // not serialized ATM
    //public DateTime DateCreated { get; set; }
    //public string Author { get; set; }
}

[DebuggerDisplay("{Name}")]
public class ChecklistItem
{
    public string Name { get; set; }
    public bool Done {get; set;}
    public ChecklistItem? ParentItem { get; set; }
    public bool HasChildItems { get; set; }
    public int IndentSize { get; set; }
}