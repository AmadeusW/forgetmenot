namespace forgetmenot.Data;

public class Checklist
{
    public DateTime DateCreated { get; set; }

    public string Title {get; set;}
    public string Author { get; set; }
    public string Summary { get; set; }
    public List<ChecklistItem> Items {get; set;}
}

public class ChecklistQuickView
{
    public string Title {get; set;}
    public string Summary { get; set; }
}

public class ChecklistItem
{
    public string Name { get; set; }
    public List<ChecklistItem>? Items {get;set;} // TODO: abstract this away to support Any and All constructs
    public bool Done {get; set;}
    public DateTime LastModified {get; set;}
    public string LastModifiedBy {get; set;}
}