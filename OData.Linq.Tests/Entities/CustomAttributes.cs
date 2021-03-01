using System;

namespace OData.Linq.Tests.Entities
{
    public class NotMappedAttribute : Attribute
    {
    }

    public class DataAttribute : Attribute
    {
        public string Name { get; set; }
    }

    public class ColumnAttribute : DataAttribute
    {
    }
}