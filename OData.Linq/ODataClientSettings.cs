namespace OData.Linq
{
    public class ODataClientSettings
    {
        private readonly INameMatchResolver _defaultNameMatchResolver = new BestMatchResolver();
        private INameMatchResolver _nameMatchResolver;
        /// <summary>
        /// Gets or sets a value indicating whether unmapped structural or navigation properties should be ignored or cause <see cref="UnresolvableObjectException"/>.
        /// </summary>
        /// <value>
        /// <c>true</c> to ignore unmapped properties; otherwise, <c>false</c>.
        /// </value>
        public bool IgnoreUnmappedProperties { get; set; }

        /// <summary>
        /// Gets or sets the OData service metadata document. If not set, service metadata is downloaded prior to the first call to the OData service and stored in an in-memory cache.
        /// </summary>
        /// <value>
        /// The content of the service metadata document.
        /// </value>
        public string MetadataDocument { get; set; }

        public bool EnumPrefixFree { get; set; }

        /// <summary>
        /// Gets or sets a name resolver for OData resources, types and properties.
        /// </summary>
        /// <value>
        /// If not set, a built-in word pluralizer is used to resolve resource, type and property names.
        /// </value>
        public INameMatchResolver NameMatchResolver
        {
            get => _nameMatchResolver ?? _defaultNameMatchResolver;
            set => _nameMatchResolver = value;
        }

        public ODataClientSettings()
        {
        }

        internal ODataClientSettings(ISession session)
        {
            this.IgnoreUnmappedProperties = session.Settings.IgnoreUnmappedProperties;
            this.MetadataDocument = session.Settings.MetadataDocument;
            this.EnumPrefixFree = session.Settings.EnumPrefixFree;
            this.NameMatchResolver = session.Settings.NameMatchResolver;
        }
    }
}