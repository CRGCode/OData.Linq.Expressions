using OData.Linq.Cache;

namespace OData.Linq
{
    public class Session : ISession
    {
        private IODataAdapter _adapter;

        public Session()
        {
            Settings = new ODataClientSettings();
            TypeCache = new TypeCache(new TypeConverter(), Settings.NameMatchResolver);
            Metadata = new MetadataCache(new Metadata(null, Settings.NameMatchResolver, Settings.IgnoreUnmappedProperties, false));

            _adapter = new ODataAdapter(this);
        }

        public IODataAdapter Adapter
        {
            get
            {
                if (_adapter == null)
                {
                    lock (this)
                    {
                        if (_adapter == null)
                        {
                            _adapter = new ODataAdapter(this);
                        }
                    }
                }
                return _adapter;
            }
        }

        public IMetadata Metadata { get; }//  => Adapter.GetMetadata();

        public ODataClientSettings Settings { get; }

        public ITypeCache TypeCache { get; }
        
        public int ArgumentCounter { get; set; }
    }
}
