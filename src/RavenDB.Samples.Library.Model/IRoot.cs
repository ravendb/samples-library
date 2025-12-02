namespace RavenDB.Samples.Library.Model;

public interface IRoot
{
    public string Id { get; }

    public static abstract string BuildId(string value);
}