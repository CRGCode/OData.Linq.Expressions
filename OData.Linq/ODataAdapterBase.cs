namespace OData.Linq
{
    public abstract class ODataAdapterBase : IODataAdapter
    {
        public abstract AdapterVersion AdapterVersion { get; }


        public object Model { get; set; }

        public abstract IMetadata GetMetadata();

        public abstract string GetODataVersionString();
    }
}