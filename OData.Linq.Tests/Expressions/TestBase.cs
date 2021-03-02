using System;

#pragma warning disable 3008

namespace OData.Linq.Tests.Expressions
{
    public abstract class TestBase : IDisposable
    {
        internal ISession Session;

        protected TestBase()
        {
            Session = new Session()
            {

            };
        }

        public abstract IFormatSettings FormatSettings { get; }

        public void Dispose()
        {
        }

    }
}
