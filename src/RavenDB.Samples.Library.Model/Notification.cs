namespace RavenDB.Samples.Library.Model;

public class Notification : IRoot
{
    private const string IdPrefix = "Notifications";

    public string Id { get; set; }

    public string UserId { get; set; }
    
    public string ReferencedItemId { get; set; }

    public string Text { get; set; }

    public static string BuildId(string value) => $"{IdPrefix}/{value}";

    public static string GetNewId() => $"{IdPrefix}/{Guid.CreateVersion7()}";
}