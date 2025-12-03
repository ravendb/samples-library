namespace RavenDB.Samples.Library.Model;

public class Author : IRoot
{
    public string Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }
    public static string BuildId(string value) => $"Authors/{value}";
}