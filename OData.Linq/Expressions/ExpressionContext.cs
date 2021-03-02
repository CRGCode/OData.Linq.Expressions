namespace OData.Linq.Expressions
{
    internal class ExpressionContext
    {
        public ISession Session { get; set; }
        public string ScopeQualifier { get; set; }
        public string DynamicPropertiesContainerName { get; set; }
        public bool IsQueryOption { get; set; }

        public ExpressionContext(ISession session)
        {
            Session = session;
        }

        public ExpressionContext(ISession session,string scopeQualifier, string dynamicPropertiesContainerName)
        {
            Session = session;
            ScopeQualifier = scopeQualifier;
            DynamicPropertiesContainerName = dynamicPropertiesContainerName;
        }

        public ExpressionContext(ISession session, bool isQueryOption)
        {
            Session = session;
            IsQueryOption = isQueryOption;
        }
    }
}
