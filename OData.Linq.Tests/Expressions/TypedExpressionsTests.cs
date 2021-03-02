using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using OData.Linq.Expressions;
using Xunit;

namespace OData.Linq.Tests.Expressions
{
    public class TypedExpressionV4Tests : TypedExpressionTests
    {
        public override IFormatSettings FormatSettings => new ODataV4Format();

        [Fact]
        public void FilterEntitiesWithContains()
        {
            var ids = new List<int> { 1, 2, 3 };
            Expression<Func<TestEntity, bool>> filter = x => ids.Contains(x.ProductId);
            Assert.Equal("ProductId in (1,2,3)", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void FilterEntitiesByStringPropertyWithContains()
        {
            var names = new List<string> { "Chai", "Milk", "Water" };
            Expression<Func<TestEntity, bool>> filter = x => names.Contains(x.ProductName);
            Assert.Equal("ProductName in ('Chai','Milk','Water')", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void FilterEntitiesByNestedPropertyWithContains()
        {
            var categories = new List<string> { "Chai", "Milk", "Water" };
            Expression<Func<TestEntity, bool>> filter = x => categories.Contains(x.Nested.ProductName);
            Assert.Equal("Nested/ProductName in ('Chai','Milk','Water')", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void FilterEntitiesByComplexConditionWithContains()
        {
            var categories = new List<string> { "chai", "milk", "water" };
            Expression<Func<TestEntity, bool>> filter = x => categories.Contains(x.ProductName.ToLower());
            Assert.Equal("tolower(ProductName) in ('chai','milk','water')", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }
    }

    public abstract class TypedExpressionTests : TestBase
    {
        class DataAttribute : Attribute
        {
            public string Name { get; set; }
            public string PropertyName { get; set; }
        }
        class DataMemberAttribute : Attribute
        {
            public string Name { get; set; }
            public string PropertyName { get; set; }
        }
        class OtherAttribute : Attribute
        {
            public string Name { get; set; }
            public string PropertyName { get; set; }
        }

        internal class TestEntity
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public Guid LinkId { get; set; }
            public decimal Price { get; set; }
            public Address Address { get; set; }
            public DateTime CreationTime { get; set; }
            public DateTimeOffset Updated { get; set; }
            public TimeSpan Period { get; set; }
            public TestEntity Nested { get; set; }
            public TestEntity[] Collection { get; set; }

            [Column(Name = "Name")]
            public string MappedNameUsingColumnAttribute { get; set; }
            [Data(Name = "Name", PropertyName = "OtherName")]
            public string MappedNameUsingDataAttribute { get; set; }
            [DataMember(Name = "Name", PropertyName = "OtherName")]
            public string MappedNameUsingDataMemberAttribute { get; set; }
            [Other(Name = "Name", PropertyName = "OtherName")]
            public string MappedNameUsingOtherAttribute { get; set; }
            [DataMember(Name = "Name", PropertyName = "OtherName")]
            [Other(Name = "OtherName", PropertyName = "OtherName")]
            public string MappedNameUsingDataMemberAndOtherAttribute { get; set; }
            [JsonProperty("Name")]
            public string MappedNameUsingJsonPropertyAttribute { get; set; }
            [System.Text.Json.Serialization.JsonPropertyName("Name")]
            public string MappedNameUsingJsonPropertyNameAttribute { get; set; }
        }

        [Fact]
        public void And()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductId == 1 && x.ProductName == "Chai";
            Assert.Equal("ProductId eq 1 and ProductName eq 'Chai'", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void Or()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductName == "Chai" || x.ProductId == 1;
            Assert.Equal("ProductName eq 'Chai' or ProductId eq 1", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void Not()
        {
            Expression<Func<TestEntity, bool>> filter = x => !(x.ProductName == "Chai");
            Assert.Equal("not (ProductName eq 'Chai')", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void Precedence()
        {
            Expression<Func<TestEntity, bool>> filter = x => (x.ProductId == 1 || x.ProductId == 2) && x.ProductName == "Chai";
            Assert.Equal("(ProductId eq 1 or ProductId eq 2) and ProductName eq 'Chai'", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void EqualString()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductName == "Chai";
            Assert.Equal("ProductName eq 'Chai'", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void EqualFieldToString()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductName.ToString() == "Chai";
            Assert.Equal("ProductName eq 'Chai'", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void EqualValueToString()
        {
            var name = "Chai";
            Expression<Func<TestEntity, bool>> filter = x => x.ProductName.ToString() == name.ToString();
            Assert.Equal("ProductName eq 'Chai'", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void EqualNumeric()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductId == 1;
            Assert.Equal("ProductId eq 1", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void NotEqualNumeric()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductId != 1;
            Assert.Equal("ProductId ne 1", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void GreaterNumeric()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductId > 1;
            Assert.Equal("ProductId gt 1", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void GreaterOrEqualNumeric()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductId >= 1.5;
            Assert.Equal($"ProductId ge 1.5{FormatSettings.DoubleNumberSuffix}", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void LessNumeric()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductId < 1;
            Assert.Equal("ProductId lt 1", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void LessOrEqualNumeric()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductId <= 1;
            Assert.Equal("ProductId le 1", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void AddEqualNumeric()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductId + 1 == 2;
            Assert.Equal("ProductId add 1 eq 2", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void SubEqualNumeric()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductId - 1 == 2;
            Assert.Equal("ProductId sub 1 eq 2", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void MulEqualNumeric()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductId * 1 == 2;
            Assert.Equal("ProductId mul 1 eq 2", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void DivEqualNumeric()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductId / 1 == 2;
            Assert.Equal("ProductId div 1 eq 2", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void ModEqualNumeric()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductId % 1 == 2;
            Assert.Equal("ProductId mod 1 eq 2", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void EqualLong()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductId == 1L;
            Assert.Equal($"ProductId eq 1{FormatSettings.LongNumberSuffix}", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void EqualDecimal()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.Price == 1M;
            Assert.Equal($"Price eq 1{FormatSettings.DecimalNumberSuffix}", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void EqualDecimalWithFractionalPart()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.Price == 1.23M;
            Assert.Equal($"Price eq 1.23{FormatSettings.DecimalNumberSuffix}", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void EqualGuid()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.LinkId == Guid.Empty;
            Assert.Equal($"LinkId eq {FormatSettings.GetGuidFormat("00000000-0000-0000-0000-000000000000")}", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void EqualDateTime()
        {
            if (FormatSettings.ODataVersion < 4)
            {
                Expression<Func<TestEntity, bool>> filter = x => x.CreationTime == new DateTime(2013, 1, 1);
                Assert.Equal("CreationTime eq datetime'2013-01-01T00:00:00'", ODataExpression.FromLinqExpression(filter).AsString(Session));
            }
        }

        [Fact]
        public void EqualDateTimeOffset()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.Updated == new DateTimeOffset(new DateTime(2013, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.Equal($"Updated eq {FormatSettings.GetDateTimeOffsetFormat("2013-01-01T00:00:00Z")}", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void EqualTimeSpan()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.Period == new TimeSpan(1, 2, 3);
            Assert.Equal($"Period eq {FormatSettings.TimeSpanPrefix}'PT1H2M3S'", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void LengthOfStringEqual()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductName.Length == 4;
            Assert.Equal("length(ProductName) eq 4", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void StringToLowerEqual()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductName.ToLower() == "chai";
            Assert.Equal("tolower(ProductName) eq 'chai'", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void StringToUpperEqual()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductName.ToUpper() == "CHAI";
            Assert.Equal("toupper(ProductName) eq 'CHAI'", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void StringStartsWithEqual()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductName.StartsWith("Ch") == true;
            Assert.Equal("startswith(ProductName,'Ch') eq true", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void StringEndsWithEqual()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductName.EndsWith("Ch") == true;
            Assert.Equal("endswith(ProductName,'Ch') eq true", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void StringContainsEqualTrue()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductName.Contains("ai") == true;
            Assert.Equal($"{FormatSettings.GetContainsFormat("ProductName", "ai")} eq true", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void StringContainsEqualFalse()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductName.Contains("ai") == false;
            Assert.Equal($"{FormatSettings.GetContainsFormat("ProductName", "ai")} eq false", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void StringContains()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductName.Contains("ai");
            Assert.Equal(FormatSettings.GetContainsFormat("ProductName", "ai"), ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void StringContainedIn()
        {
            Expression<Func<TestEntity, bool>> filter = x => "Chai".Contains(x.ProductName);
            Assert.Equal(FormatSettings.GetContainedInFormat("ProductName", "Chai"), ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void StringNotContains()
        {
            Expression<Func<TestEntity, bool>> filter = x => !x.ProductName.Contains("ai");
            Assert.Equal($"not {FormatSettings.GetContainsFormat("ProductName", "ai")}", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void StringToLowerAndContains()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductName.ToLower().Contains("Chai");
            Assert.Equal(FormatSettings.GetContainsFormat("tolower(ProductName)", "Chai"), ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void IndexOfStringEqual()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductName.IndexOf("ai") == 1;
            Assert.Equal("indexof(ProductName,'ai') eq 1", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void SubstringWithPositionEqual()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductName.Substring(1) == "hai";
            Assert.Equal("substring(ProductName,1) eq 'hai'", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void SubstringWithPositionAndLengthEqual()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductName.Substring(1, 2) == "ha";
            Assert.Equal("substring(ProductName,1,2) eq 'ha'", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void ReplaceStringEqual()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductName.Replace("a", "o") == "Choi";
            Assert.Equal("replace(ProductName,'a','o') eq 'Choi'", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void TrimEqual()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.ProductName.Trim() == "Chai";
            Assert.Equal("trim(ProductName) eq 'Chai'", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void ConcatEqual()
        {
            Expression<Func<TestEntity, bool>> filter = x => string.Concat(x.ProductName, "Chai") == "ChaiChai";
            Assert.Equal("concat(ProductName,'Chai') eq 'ChaiChai'", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void DayEqual()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.CreationTime.Day == 1;
            Assert.Equal("day(CreationTime) eq 1", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void MonthEqual()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.CreationTime.Month == 2;
            Assert.Equal("month(CreationTime) eq 2", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void YearEqual()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.CreationTime.Year == 3;
            Assert.Equal("year(CreationTime) eq 3", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void HourEqual()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.CreationTime.Hour == 4;
            Assert.Equal("hour(CreationTime) eq 4", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void MinuteEqual()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.CreationTime.Minute == 5;
            Assert.Equal("minute(CreationTime) eq 5", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void SecondEqual()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.CreationTime.Second == 6;
            Assert.Equal("second(CreationTime) eq 6", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void RoundEqual()
        {
            Expression<Func<TestEntity, bool>> filter = x => decimal.Round(x.Price) == 1;
            Assert.Equal($"round(Price) eq 1{FormatSettings.DecimalNumberSuffix}", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void FloorEqual()
        {
            Expression<Func<TestEntity, bool>> filter = x => decimal.Floor(x.Price) == 1;
            Assert.Equal($"floor(Price) eq 1{FormatSettings.DecimalNumberSuffix}", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void CeilingEqual()
        {
            Expression<Func<TestEntity, bool>> filter = x => decimal.Ceiling(x.Price) == 2;
            Assert.Equal($"ceiling(Price) eq 2{FormatSettings.DecimalNumberSuffix}", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void EqualNestedProperty()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.Nested.ProductId == 1;
            Assert.Equal("Nested/ProductId eq 1", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void EqualNestedPropertyLengthOfStringEqual()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.Nested.ProductName.Length == 4;
            Assert.Equal("length(Nested/ProductName) eq 4", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void ConvertEqual()
        {
            var id = "1";
            Expression<Func<TestEntity, bool>> filter = x => x.Nested.ProductId == Convert.ToInt32(id);
            Assert.Equal("Nested/ProductId eq 1", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void FilterWithMappedPropertiesUsingColumnAttribute()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.MappedNameUsingColumnAttribute == "Milk";
            Assert.Equal("Name eq 'Milk'", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void FilterWithMappedPropertiesUsingDataAttribute()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.MappedNameUsingDataAttribute == "Milk";
            Assert.Equal("Name eq 'Milk'", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void FilterWithMappedPropertiesUsingDataMemberAttribute()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.MappedNameUsingDataMemberAttribute == "Milk";
            Assert.Equal("Name eq 'Milk'", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void FilterWithMappedPropertiesUsingOtherAttribute()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.MappedNameUsingOtherAttribute == "Milk";
            Assert.Equal("Name eq 'Milk'", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void FilterWithMappedPropertiesUsingDataMemberAndOtherAttribute()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.MappedNameUsingDataMemberAndOtherAttribute == "Milk";
            Assert.Equal("Name eq 'Milk'", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void FilterWithMappedPropertiesUsingJsonPropertyAttribute()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.MappedNameUsingJsonPropertyAttribute == "Milk";
            Assert.Equal("Name eq 'Milk'", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void FilterWithMappedPropertiesUsingJsonPropertyNameAttribute()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.MappedNameUsingJsonPropertyNameAttribute == "Milk";
            Assert.Equal("Name eq 'Milk'", ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void FilterWithEnum()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.Address.Type == AddressType.Corporate;
            var result = ODataExpression.FromLinqExpression(filter).AsString(Session);
            var expected = $"Address/Type eq {FormatSettings.GetEnumFormat(AddressType.Corporate, typeof(AddressType), typeof(AddressType).Namespace)}";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void FilterWithEnum_LocalVar()
        {
            var addressType = AddressType.Corporate;
            Expression<Func<TestEntity, bool>> filter = x => x.Address.Type == addressType;
            Assert.Equal($"Address/Type eq {FormatSettings.GetEnumFormat(AddressType.Corporate, typeof(AddressType), typeof(AddressType).Namespace)}",
                ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        private AddressType addressType = AddressType.Corporate;

        [Fact]
        public void FilterWithEnum_MemberVar()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.Address.Type == addressType;
            Assert.Equal($"Address/Type eq {FormatSettings.GetEnumFormat(AddressType.Corporate, typeof(AddressType), typeof(AddressType).Namespace)}",
                ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void FilterWithEnum_Const()
        {
            const AddressType addressType = AddressType.Corporate;
            Expression<Func<TestEntity, bool>> filter = x => x.Address.Type == addressType;
            Assert.Equal($"Address/Type eq {FormatSettings.GetEnumFormat(AddressType.Corporate, typeof(AddressType), typeof(AddressType).Namespace)}",
                ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void FilterWithEnum_PrefixFree()
        {
            var enumPrefixFree = Session.EnumPrefixFree;
            Session = new Session(true);
            try
            {
                Expression<Func<TestEntity, bool>> filter = x => x.Address.Type == AddressType.Corporate;
                var actual = ODataExpression.FromLinqExpression(filter).AsString(Session);
                var enumFormat = FormatSettings.GetEnumFormat(AddressType.Corporate, typeof(AddressType), typeof(AddressType).Namespace, true);
                var expected = $"Address/Type eq {enumFormat}";
                Assert.Equal(expected, actual);
            }
            finally
            {
                Session = new Session(enumPrefixFree);
            }
        }

        [Fact]
        public void FilterWithEnum_HasFlag()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.Address.Type.HasFlag(AddressType.Corporate);
            Assert.Equal($"Address/Type has {FormatSettings.GetEnumFormat(AddressType.Corporate, typeof(AddressType), typeof(AddressType).Namespace)}",
                ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void FilterWithEnum_ToString()
        {
            Expression<Func<TestEntity, bool>> filter = x => x.Address.Type.ToString() == AddressType.Corporate.ToString();
            Assert.Equal($"Address/Type eq {FormatSettings.GetEnumFormat(AddressType.Corporate, typeof(AddressType), typeof(AddressType).Namespace)}",
                ODataExpression.FromLinqExpression(filter).AsString(Session));
        }

        [Fact]
        public void FilterDateTimeRange()
        {
            var beforeDt = new DateTime(2013, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var afterDt = new DateTime(2014, 2, 2, 0, 0, 0, DateTimeKind.Utc);
            Expression<Func<TestEntity, bool>> filter = x => (x.CreationTime >= beforeDt) && (x.CreationTime < afterDt);
            if (FormatSettings.ODataVersion < 4)
            {
                Assert.Equal("CreationTime ge datetime'2013-01-01T00:00:00Z' and CreationTime lt datetime'2014-02-02T00:00:00Z'",
                    ODataExpression.FromLinqExpression(filter).AsString(Session));
            }
            else
            {
                Assert.Equal("CreationTime ge 2013-01-01T00:00:00Z and CreationTime lt 2014-02-02T00:00:00Z",
                    ODataExpression.FromLinqExpression(filter).AsString(Session));
            }
        }

        [Fact]
        public void ExpressionBuilder()
        {
            Expression<Predicate<TestEntity>> condition1 = x => x.ProductName == "Chai";
            Expression<Func<TestEntity, bool>> condition2 = x => x.ProductId == 1;
            var filter = new ODataExpression(condition1);
            filter = filter || new ODataExpression(condition2);
            Assert.Equal("ProductName eq 'Chai' or ProductId eq 1", filter.AsString(Session));
        }

        [Fact]
        public void ExpressionBuilderGeneric()
        {
            var filter = new ODataExpression<TestEntity>(x => x.ProductName == "Chai");
            filter = filter || new ODataExpression<TestEntity>(x => x.ProductId == 1);
            Assert.Equal("ProductName eq 'Chai' or ProductId eq 1", filter.AsString(Session));
        }

        [Fact]
        public void ExpressionBuilderGrouping()
        {
            Expression<Predicate<TestEntity>> condition1 = x => x.ProductName == "Chai";
            Expression<Func<TestEntity, bool>> condition2 = x => x.ProductId == 1;
            Expression<Predicate<TestEntity>> condition3 = x => x.ProductName == "Kaffe";
            Expression<Func<TestEntity, bool>> condition4 = x => x.ProductId == 2;
            var filter1 = new ODataExpression(condition1) || new ODataExpression(condition2);
            var filter2 = new ODataExpression(condition3) || new ODataExpression(condition4);
            var filter = filter1 && filter2;
            Assert.Equal("(ProductName eq 'Chai' or ProductId eq 1) and (ProductName eq 'Kaffe' or ProductId eq 2)", filter.AsString(Session));
        }

        [Fact]
        public void AnyNested()
        {
            Expression<Predicate<TestEntity>> filter = x => x.Collection.Any(c => c.Collection.Any(d => d.ProductId == 2));
            var actual = ODataExpression.FromLinqExpression(filter).AsString(Session);
            Assert.Equal("Collection/any(x1:x1/Collection/any(x2:x2/ProductId eq 2))", actual);
        }

        [Fact]
        public void AnyStringEqual()
        {
            Expression<Predicate<TestEntity>> filter = x => x.Collection.Any(c => c.Address.City == "Melbourne");
            var actual = ODataExpression.FromLinqExpression(filter).AsString(Session);
            Assert.Equal("Collection/any(x1:x1/Address/City eq 'Melbourne')", actual);
        }

        [Fact]
        public void AnyEnumEqual()
        {
            Expression<Predicate<TestEntity>> filter = x => x.Collection.Any(c => c.Address.Type == AddressType.Corporate);
            var actual = ODataExpression.FromLinqExpression(filter).AsString(Session);
            Assert.Equal("Collection/any(x1:x1/Address/Type eq OData.Linq.Tests.AddressType'Corporate')", actual);
        }

        [Fact]
        public void AllDecimalLessThan()
        {
            Expression<Predicate<TestEntity>> filter = x => x.Collection.All(c => c.Price < 10);
            var actual = ODataExpression.FromLinqExpression(filter).AsString(Session);
            Assert.Equal("Collection/all(x1:x1/Price lt 10)", actual);
        }

        [Fact]
        public void AllEnumEqual()
        {
            Expression<Predicate<TestEntity>> filter = x => x.Collection.All(c => c.Address.Type == AddressType.Corporate);
            var actual = ODataExpression.FromLinqExpression(filter).AsString(Session);
            Assert.Equal("Collection/all(x1:x1/Address/Type eq OData.Linq.Tests.AddressType'Corporate')", actual);
        }

        [Fact]
        public void AllMultipleExpression()
        {
            Expression<Predicate<TestEntity>> filter = x => x.Collection.All(c => c.Address.Type == AddressType.Corporate && c.Price > 10);
            var actual = ODataExpression.FromLinqExpression(filter).AsString(Session);
            Assert.Equal("Collection/all(x1:x1/Address/Type eq OData.Linq.Tests.AddressType'Corporate' and x1/Price gt 10)", actual);
        }

    }
}
