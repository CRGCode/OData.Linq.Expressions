using OData.Linq.Extensions;
using Xunit;

namespace OData.Linq.Tests.Extensions
{
    public class StringExtensionTests
    {
        [Fact]
        public void EnsureStartsWith_should_prefix_string()
        {
            var actual = "bar".EnsureStartsWith("foo");

            Assert.Equal("foobar", actual);
        }

        [Fact]
        public void EnsureStartsWith_should_not_prefix_string()
        {
            var actual = "foobar".EnsureStartsWith("foo");

            Assert.Equal("foobar", actual);
        }
    }
}
