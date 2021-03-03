# Welcome to the OData.Linq lambda expression to OData filter

## Introduction

OData.Linq.Expressions is OData filter generator for Linq style lambda expressions

eg. 
 Expression<Func<TestEntity, bool>> filter = x => ids.Contains(x.ProductId);

OData $filter="ProductId in (1,2,3)";

## License
<a href="http://opensource.org/licenses/MIT">MIT-licensed</a>.
