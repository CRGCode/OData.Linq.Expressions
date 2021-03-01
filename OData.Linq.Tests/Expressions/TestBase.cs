using System;

#pragma warning disable 3008

namespace OData.Linq.Tests.Expressions
{
    public abstract class TestBase : IDisposable
    {
        internal ISession _session;

        protected TestBase()
        {
            _session = new Session()
            {

            };
        }

        public abstract IFormatSettings FormatSettings { get; }

        public void Dispose()
        {
        }

    }
}
