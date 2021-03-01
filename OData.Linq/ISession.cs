using OData.Linq.Cache;

namespace OData.Linq
{
    /// <summary>
    /// Provide access to session-specific details.
    /// </summary>
    public interface ISession
    {
        ODataClientSettings Settings { get; }

        /// <summary>
        /// Gets OData client adapter.
        /// </summary>
        IODataAdapter Adapter { get; }

        IMetadata Metadata { get; }

        /// <summary>
        /// Gets type information for this session.
        /// </summary>
        ITypeCache TypeCache { get; }
    }
}