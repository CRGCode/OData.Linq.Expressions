namespace OData.Linq
{
    public interface INameMatchResolver
    {
        bool IsMatch(string actualName, string requestedName);
    }
}