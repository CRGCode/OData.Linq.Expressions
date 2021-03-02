using Microsoft.OData.Edm;

namespace OData.Linq
{
    public class ODataAdapter : ODataAdapterBase
    {
        private readonly ISession _session;
        private IMetadata _metadata;

        public override AdapterVersion AdapterVersion => AdapterVersion.V4;

        public ODataAdapter(ISession session)
        {
            _session = session;
        }

        public new IEdmModel Model
        {
            get => base.Model as IEdmModel;
            set
            {
                base.Model = value;
                _metadata = null;
            }
        }

        public override string GetODataVersionString()
        {
            return "V4";
        }

        public override IMetadata GetMetadata()
        {
            // TODO: Should use a MetadataFactory here 
            return _metadata ?? (_metadata = new MetadataCache(new Metadata(Model, _session.Settings.NameMatchResolver, _session.Settings.IgnoreUnmappedProperties, false)));//  _session.Settings.UnqualifiedNameCall)));
        }
    }
}