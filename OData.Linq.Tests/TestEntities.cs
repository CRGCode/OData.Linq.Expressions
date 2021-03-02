using System;

namespace OData.Linq.Tests
{
    public class Transport
    {
        public static readonly string TransportPropertyKey = @"Test Value";

        public int TransportID { get; set; }
    }

    public class Ship : Transport
    {
        public string ShipName { get; set; }
    }

    public interface IAddress
    {
        AddressType Type { get; set; }

        string City { get; set; }

        string Region { get; set; }

        string PostalCode { get; set; }

        string Country { get; set; }
    }

    public class Address : IAddress
    {
        public AddressType Type { get; set; }

        AddressType IAddress.Type
        {
            get => Type;
            set => Type = value;
        }

        public string City { get; set; }

        public string Region { get; set; }

        public string PostalCode { get; set; }

        public string Country { get; set; }
    }

    public enum AddressType
    {
        Unknown = 0,
        Postal,
        Corporate
    }

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