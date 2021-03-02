﻿using System.Collections.Generic;

namespace OData.Linq.Tests.Entities
{
    public class Animal
    {
        public Animal()
        {
            DynamicProperties = new Dictionary<string, object>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public IDictionary<string, object> DynamicProperties { get; set; }
    }
}
