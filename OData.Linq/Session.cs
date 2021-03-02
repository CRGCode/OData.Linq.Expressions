using OData.Linq.Cache;

namespace OData.Linq
{
    public class Session : ISession
    {
        public Session(bool enumPrefixFree = false)
        {
            EnumPrefixFree = enumPrefixFree;
            TypeCache = new TypeCache(new TypeConverter(), new BestMatchResolver());
        }

        public bool EnumPrefixFree { get; set; }

        public ITypeCache TypeCache { get; }

        public int ArgumentCounter { get; set; }
    }
}
