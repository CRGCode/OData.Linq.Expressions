using OData.Linq.Cache;

namespace OData.Linq
{
    public interface ISession
    {
        ODataClientSettings Settings { get; }

        ITypeCache TypeCache { get; }

        int ArgumentCounter { get; set; }
    }
}