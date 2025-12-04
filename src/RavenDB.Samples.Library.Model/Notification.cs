namespace RavenDB.Samples.Library.Model;

public class Notification : IRoot
{
    public string Id { get; set; }
    
    public string UserId { get; set; }
    
    public string Text { get; set; }
    
    public static string BuildId(string value) => $"Notifications/{value}";
}