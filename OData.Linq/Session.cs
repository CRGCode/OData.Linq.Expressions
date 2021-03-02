using OData.Linq.Cache;

namespace OData.Linq
{
    public class Session : ISession
    {
        public Session()
        {
            Settings = new ODataClientSettings();
            TypeCache = new TypeCache(new TypeConverter(), Settings.NameMatchResolver);
        }

        public ODataClientSettings Settings { get; }

        public ITypeCache TypeCache { get; }
        
        public int ArgumentCounter { get; set; }
    }
}
