namespace RavenDB.Samples.Library.Model;

public class User
{
    public string Id { get; set; }

    public static string BuildId(string value) => $"Users/{value}";
}
