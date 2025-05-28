namespace StoreManagement.Domain.ValueObjects;

/// <summary>
/// Represents an address as a value object in the domain.
/// A value object is immutable and defined by its properties rather than its identity.
/// </summary>
/// <remarks>
/// This class encapsulates the city and full address properties and provides
/// methods for equality comparison, hash code generation, and string representation.
/// </remarks>
public class Address : ValueObject
{
    public string City { get; }
    public string FullAddress { get; }
    
    public Address(string city, string fullAddress)
    {
        City = city;
        FullAddress = fullAddress;
    }

    public override string ToString()
    {
        return $"{FullAddress}, {City}";
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not Address other)
        {
            return false;
        }

        return City == other.City && FullAddress == other.FullAddress;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(City, FullAddress);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return City;
        yield return FullAddress;
    }
}