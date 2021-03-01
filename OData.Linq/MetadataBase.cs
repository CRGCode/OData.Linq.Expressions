using System.Collections.Generic;
using System.Linq;

namespace OData.Linq
{
    public abstract class MetadataBase : IMetadata
    {
        protected MetadataBase(INameMatchResolver nameMatchResolver, bool ignoreUnmappedProperties, bool unqualifiedNameCall)
        {
            IgnoreUnmappedProperties = ignoreUnmappedProperties;
            NameMatchResolver = nameMatchResolver;
            UnqualifiedNameCall = unqualifiedNameCall;
        }

        public bool IgnoreUnmappedProperties { get; }

        public INameMatchResolver NameMatchResolver { get; }

        public bool UnqualifiedNameCall { get; }

        public abstract string GetEntityCollectionExactName(string collectionName);

        public abstract string GetDerivedEntityTypeExactName(string collectionName, string entityTypeName);

        public abstract string GetQualifiedTypeName(string collectionName);

        public abstract bool IsOpenType(string collectionName);

        public abstract bool HasStructuralProperty(string collectionName, string propertyName);

        public abstract string GetStructuralPropertyExactName(string collectionName, string propertyName);
        public abstract string GetStructuralPropertyPath(string collectionName, params string[] propertyNames);
        public abstract IEnumerable<string> GetDeclaredKeyPropertyNames(string collectionName);
        public abstract IEnumerable<string> GetNavigationPropertyNames(string collectionName);
        public abstract IEnumerable<IEnumerable<string>> GetAlternateKeyPropertyNames(string collectionName);

        public abstract bool HasNavigationProperty(string collectionName, string propertyName);

        public abstract string GetNavigationPropertyExactName(string collectionName, string propertyName);

        public abstract string GetNavigationPropertyPartnerTypeName(string collectionName, string propertyName);

        public abstract bool IsNavigationPropertyCollection(string collectionName, string propertyName);
        public abstract string GetFunctionFullName(string functionName);
        public abstract EntityCollection GetFunctionReturnCollection(string functionName);
        public abstract string GetActionFullName(string actionName);
        public abstract EntityCollection GetActionReturnCollection(string functionName);

        public EntityCollection GetEntityCollection(string collectionPath)
        {
            var segments = collectionPath.Split('/');
            if (segments.Count() > 1)
            {
                if (SegmentsIncludeTypeSpecification(segments))
                {
                    var baseEntitySet = this.GetEntityCollection(Utils.ExtractCollectionName(segments[segments.Length - 2]));
                    return GetDerivedEntityCollection(baseEntitySet, Utils.ExtractCollectionName(segments.Last()));
                }
                else
                {
                    return new EntityCollection(GetEntityCollectionExactName(Utils.ExtractCollectionName(segments.Last())));
                }
            }
            else
            {
                return new EntityCollection(GetEntityCollectionExactName(Utils.ExtractCollectionName(collectionPath)));
            }
        }

        public EntityCollection GetDerivedEntityCollection(EntityCollection baseCollection, string entityTypeName)
        {
            var actualName = GetDerivedEntityTypeExactName(baseCollection.Name, entityTypeName);
            return new EntityCollection(actualName, baseCollection);
        }

        public EntityCollection NavigateToCollection(string path)
        {
            var segments = GetCollectionPathSegments(path);
            return IsSingleSegmentWithTypeSpecification(segments)
                ? GetEntityCollection(path)
                : NavigateToCollection(GetEntityCollection(segments.First()), segments.Skip(1));
        }

        public EntityCollection NavigateToCollection(EntityCollection rootCollection, string path)
        {
            return NavigateToCollection(rootCollection, GetCollectionPathSegments(path));
        }

        private EntityCollection NavigateToCollection(EntityCollection rootCollection, IEnumerable<string> segments)
        {
            if (!segments.Any())
                return rootCollection;

            var associationName = GetNavigationPropertyExactName(rootCollection.Name, segments.First());
            var typeName = IsSingleSegmentWithTypeSpecification(segments)
                ? segments.Last()
                : GetNavigationPropertyPartnerTypeName(rootCollection.Name, associationName);
            var entityCollection = GetEntityCollection(typeName);

            return segments.Count() == 1 || IsSingleSegmentWithTypeSpecification(segments)
                ? entityCollection
                : NavigateToCollection(entityCollection, segments.Skip(1));
        }

        protected bool SegmentsIncludeTypeSpecification(IEnumerable<string> segments)
        {
            return segments.Last().Contains(".");
        }

        protected bool IsSingleSegmentWithTypeSpecification(IEnumerable<string> segments)
        {
            return segments.Count() == 2 && SegmentsIncludeTypeSpecification(segments);
        }

        public IEnumerable<string> GetCollectionPathSegments(string path)
        {
            return path.Split('/').Select(x => x.Contains("(") ? x.Substring(0, x.IndexOf("(")) : x);
        }
    }
}