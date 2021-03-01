using System.Collections.Concurrent;
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

            //MetadataCache = new EdmMetadataCache("X", "....", TypeCache);
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

        //public EdmMetadataCache MetadataCache { get; }

        public ODataClientSettings Settings { get; }

        public ITypeCache TypeCache { get; }
    }

    //internal static class TypeCaches
    //{
    //    private static readonly ConcurrentDictionary<string, ITypeCache> _typeCaches;

    //    static TypeCaches()
    //    {
    //        // TODO: Have a global switch whether we use the dictionary or not
    //        _typeCaches = new ConcurrentDictionary<string, ITypeCache>();
    //    }

    //    internal static ITypeCache TypeCache(string uri, INameMatchResolver nameMatchResolver)
    //    {
    //        return _typeCaches.GetOrAdd(uri, new TypeCache(CustomConverters.Converter(uri), nameMatchResolver));
    //    }
    //}

}
