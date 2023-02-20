using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace forgetmenot.Data;
public class SimpleStateRepositoryProvider : IStateRepositoryProvider
{
    private readonly static string StatePath = Path.Combine(ChecklistService.ChecklistPath, "state.json");

    public async Task<ChecklistRepository> GetStateAsync()
    {
        var stateString = await File.ReadAllTextAsync(StatePath);
        var state = JsonSerializer.Deserialize<ChecklistRepository>(stateString);
        return state;
    }

    public async Task InitializeAsync(IList<Checklist> checklists)
    {
        if (File.Exists(StatePath))
        {
            return;
        }
        // Initializes state from scratch, when no existing state is available

        var newState = new ChecklistRepository();
        foreach (var checklist in checklists)
        {
            newState.Add(new ChecklistWithState()
            {
                Identifier = checklist.Id,
                IsPrototype = false, // this information should be in the checklist.
                IsFeatured = false,
            });
        }

        await this.SetStateAsync(newState);
    }

    private async Task UpdateStateAsync(IList<Checklist> currentChecklists)
    {
        var newState = new ChecklistRepository();
        foreach (var checklist in currentChecklists)
        {
            var existingState = await this.GetStateAsync();
            var matchingExistingChecklist = existingState.FirstOrDefault(n => n.Identifier.Equals(checklist.Id));
            if (matchingExistingChecklist is not null)
            {
                newState.Add(matchingExistingChecklist);
            }
            else
            {
                newState.Add(new ChecklistWithState()
                {
                    Identifier = checklist.Id,
                    IsPrototype = false, // this information should be in the checklist.
                    IsFeatured = false,
                });
            }
        }

        await this.SetStateAsync(newState);
    }

    public Task SetStateAsync(ChecklistRepository state)
    {
        var stateString = JsonSerializer.Serialize(state);
        return File.WriteAllTextAsync(StatePath, stateString);
    }
}