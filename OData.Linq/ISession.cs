using OData.Linq.Cache;

namespace OData.Linq
{
    public interface ISession
    {
        ITypeCache TypeCache { get; }

        bool EnumPrefixFree { get; }

        int ArgumentCounter { get; set; }
    }
}