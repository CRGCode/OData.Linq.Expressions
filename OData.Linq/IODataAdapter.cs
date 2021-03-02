namespace OData.Linq
{
    public interface IODataAdapter
    {
        AdapterVersion AdapterVersion { get; }

        object Model { get; set; }

        IMetadata GetMetadata();

        string GetODataVersionString();
    }
}