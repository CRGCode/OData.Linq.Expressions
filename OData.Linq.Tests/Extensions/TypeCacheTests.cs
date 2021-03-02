using System.Linq;
using OData.Linq.Cache;
using Xunit;

namespace OData.Linq.Tests.Extensions
{
    public class TypeCacheTests
    {
        private ITypeCache TypeCache => new TypeCache(new TypeConverter(), null);

        [Fact]
        public void GetDerivedTypes_BaseType()
        {
            Assert.Single(TypeCache.GetDerivedTypes(typeof(Transport)));
        }

        [Fact]
        public void GetAllProperties_BaseType()
        {
            Assert.Single(TypeCache.GetAllProperties(typeof(Transport)));
        }

        [Fact]
        public void GetAllProperties_DerivedType()
        {
            Assert.Equal(2, TypeCache.GetAllProperties(typeof(Ship)).Count());
        }

        [Fact]
        public void GetDeclaredProperties_ExcludeExplicitInterface()
        {
            Assert.Equal(5, TypeCache.GetAllProperties(typeof(Address)).Count());
        }

        [Fact]
        public void GetDeclaredProperties_BaseType()
        {
            Assert.Single(TypeCache.GetDeclaredProperties(typeof(Transport)));
        }

        [Fact]
        public void GetDeclaredProperties_DerivedType()
        {
            Assert.Single(TypeCache.GetDeclaredProperties(typeof(Ship)));
        }

        [Fact]
        public void GetNamedProperty_BaseType()
        {
            Assert.NotNull(TypeCache.GetNamedProperty(typeof(Transport), "TransportId"));
        }

        [Fact]
        public void GetNamedProperty_DerivedType()
        {
            Assert.NotNull(TypeCache.GetNamedProperty(typeof(Ship), "TransportId"));
            Assert.NotNull(TypeCache.GetNamedProperty(typeof(Ship), "ShipName"));
        }

        [Fact]
        public void GetDeclaredProperty_BaseType()
        {
            Assert.NotNull(TypeCache.GetDeclaredProperty(typeof(Transport), "TransportId"));
        }

        [Fact]
        public void GetDeclaredProperty_DerivedType()
        {
            Assert.Null(TypeCache.GetDeclaredProperty(typeof(Ship), "TransportId"));
            Assert.NotNull(TypeCache.GetDeclaredProperty(typeof(Ship), "ShipName"));
        }
    }
}