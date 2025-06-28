namespace StoreManagement.Application.DTOs.Stores;

public class StoreDto 
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? ManagerName { get; set; }
    public string? ContactNumber { get; set; }
    public string? Email { get; set; }

    // Flattened PhoneNumber property based on PhoneNumber.Value
    public string Phone_Number { get; set; } = string.Empty; // Holds PhoneNumber.Value

    // Flattened Address properties based on Address.City and Address.FullAddress
    public string Address_City { get; set; } = string.Empty;
    public string Address_FullAddress { get; set; } = string.Empty; // Changed from Address_Full
}