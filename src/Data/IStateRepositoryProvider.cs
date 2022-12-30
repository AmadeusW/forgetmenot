namespace forgetmenot.Data
{
    public interface IStateRepositoryProvider
    {
        Task<ChecklistRepository> GetStateAsync();
        Task InitializeAsync(IList<Checklist> checklists);
        Task SetStateAsync(ChecklistRepository state); 
    }
}