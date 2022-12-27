namespace forgetmenot.Data
{
    public interface IChecklistParser
    {
        string Serialize(Checklist checklist);
        Task<Checklist> ParseAsync(string serialized); 
    }
}