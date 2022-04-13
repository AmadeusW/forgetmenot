namespace forgetmenot.Data;

public class ChecklistService
{
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

    public Task<Checklist> GetChecklistAsync(string title)
    {
        return Task.FromResult(new Checklist() 
        {
            Title = title,
            Author = "n/a",
            DateCreated = DateTime.Now,
            Summary = "auto generated",
            Items = new List<ChecklistItem>() {
                new ChecklistItem() {
                    Name = "task A",
                    Done = true,
                },
                new ChecklistItem() {
                    Name = "task B",
                    Done = false,
                },
                new ChecklistItem() {
                    Name = "task C",
                    Done = false,
                },
            },
        });
    }
}
