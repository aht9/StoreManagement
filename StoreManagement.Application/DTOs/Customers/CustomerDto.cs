namespace StoreManagement.Application.DTOs.Customers;

public class CustomerDto
{
    public long Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public string PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string City { get; set; }
    public string FullAddress { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public long? NationalCode { get; set; }
    public DateTime CreationDate { get; set; }
}