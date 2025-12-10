namespace RavenDB.Samples.Library.Model;

public class Book : IRoot
{
    public string Id { get; set; }
    public static string BuildId(string value) => $"Books/{value}";

    public string Title { get; set; }

    public string AuthorId { get; set; }
    
    public string? Description { get; set; }
}
