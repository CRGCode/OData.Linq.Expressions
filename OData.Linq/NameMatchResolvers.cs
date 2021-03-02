using System;
using System.Linq;
using OData.Linq.Extensions;

namespace OData.Linq
{
    public static class ODataNameMatchResolver
    {
        public static readonly INameMatchResolver Strict = new ExactMatchResolver();
    }

    public static class Pluralizers
    {
        private static readonly IPluralizer Simple = new SimplePluralizer();
        public static readonly IPluralizer Cached = new CachedPluralizer(Simple);
    }

    public class ExactMatchResolver : INameMatchResolver
    {
        private readonly StringComparison stringComparison;
        private readonly bool alphanumComparison;

        public ExactMatchResolver(bool alphanumComparison = false, StringComparison stringComparison = StringComparison.InvariantCulture)
        {
            this.alphanumComparison = alphanumComparison;
            this.stringComparison = stringComparison;
        }

        public bool IsMatch(string actualName, string requestedName)
        {
            actualName = actualName.Split('.').Last();
            requestedName = requestedName.Split('.').Last();
            if (!alphanumComparison) 
                return actualName.Equals(requestedName, stringComparison);
            actualName = actualName.Homogenize();
            requestedName = requestedName.Homogenize();

            return actualName.Equals(requestedName, stringComparison);
        }
    }

    public class BestMatchResolver : INameMatchResolver
    {
        private readonly IPluralizer pluralizer;

        public BestMatchResolver()
        {
            pluralizer = Pluralizers.Cached;
        }

        public bool IsMatch(string actualName, string requestedName)
        {
            actualName = actualName.Split('.').Last().Homogenize();
            requestedName = requestedName.Split('.').Last().Homogenize();

            return actualName == requestedName || 
                   (actualName == pluralizer.Singularize(requestedName) ||
                    actualName == pluralizer.Pluralize(requestedName) ||
                    pluralizer.Singularize(actualName) == requestedName ||
                    pluralizer.Pluralize(actualName) == requestedName);
        }
    }
}