
using OData.Linq.Fluent;

namespace OData.Linq
{
    public interface IODataAdapter
    {
        AdapterVersion AdapterVersion { get; }

        object Model { get; set; }

        IMetadata GetMetadata();

        ICommandFormatter GetCommandFormatter();

        string GetODataVersionString();
    }
}