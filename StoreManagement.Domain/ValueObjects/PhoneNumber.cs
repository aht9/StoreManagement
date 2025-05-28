namespace StoreManagement.Domain.ValueObjects;

/// <summary>
/// Represents a phone number as a value object in the domain.
/// A value object is immutable and defined by its properties rather than its identity.
/// </summary>
/// <remarks>
/// This class encapsulates the logic for creating, validating, and formatting Iranian phone numbers.
/// It ensures that phone numbers conform to the expected format and provides utility methods
/// for cleaning and formatting phone numbers.
/// </remarks>
public class PhoneNumber : ValueObject
{
    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static Result<PhoneNumber> Create(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return Result.Failure<PhoneNumber>(StrFaSD.PhoneNumber_Cannot_Be_Empty);
        var cleaned = CleanPhoneNumber(phoneNumber);
        if (!IsValidIranianPhoneNumber(cleaned))
            return Result.Failure<PhoneNumber>(StrFaSD.PhoneNumber_Invalid_Format);
        return Result.Success(new PhoneNumber(cleaned));
    }
    
    private static string CleanPhoneNumber(string phoneNumber)
    {
        //Remove all non-digit characters
        var digitsOnly = new string(phoneNumber.Where(c => char.IsDigit(c)).ToArray());

        //if the number starts with 0, remove it
        if (digitsOnly.StartsWith("0"))
            digitsOnly = digitsOnly.Substring(1);

        //if the number starts with 98, remove it
        if (digitsOnly.StartsWith("98"))
            digitsOnly = digitsOnly.Substring(2);

        return digitsOnly;
    }
    
    private static bool IsValidIranianPhoneNumber(string phoneNumber)
    {
        //Iranian phone numbers are 10 digits long and start with 9
        return phoneNumber.Length == 10 && phoneNumber.StartsWith("9");
    }
    
    public string GetFormattedNumber(bool includeCountryCode = true)
    {
        return includeCountryCode ? $"+98{Value}" : $"0{Value}";
    }
    
    protected override IEnumerable<object> GetAtomicValues()
    {
        //phone number is atomic, so we just return its value
        yield return Value;
    }
}