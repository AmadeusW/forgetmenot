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

        var newState = new ChecklistRepository();
        foreach (var checklist in checklists)
        {
            newState.Add(new ChecklistWithState()
            {
                Identifier = checklist.Id,
                IsPrototype = true,
                IsFeatured = false,
            });
        }
        await this.SetStateAsync(newState);
    }

    public Task SetStateAsync(ChecklistRepository state)
    {
        var stateString = JsonSerializer.Serialize(state);
        return File.WriteAllTextAsync(StatePath, stateString);
    }
}