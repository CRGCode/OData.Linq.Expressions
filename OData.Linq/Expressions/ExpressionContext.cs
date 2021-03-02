﻿namespace OData.Linq.Expressions
{
    internal class ExpressionContext
    {
        public ISession Session { get; set; }
        public string ScopeQualifier { get; set; }
        public string DynamicPropertiesContainerName { get; set; }
        public bool IsQueryOption { get; set; }

        public ExpressionContext(ISession session)
        {
            this.Session = session;
        }

        public ExpressionContext(ISession session,string scopeQualifier, string dynamicPropertiesContainerName)
        {
            this.Session = session;
            this.ScopeQualifier = scopeQualifier;
            this.DynamicPropertiesContainerName = dynamicPropertiesContainerName;
        }

        public ExpressionContext(ISession session, bool isQueryOption)
        {
            this.Session = session;
            this.IsQueryOption = isQueryOption;
        }
    }
}
