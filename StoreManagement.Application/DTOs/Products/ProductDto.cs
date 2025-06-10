namespace StoreManagement.Application.DTOs.Products;

public class ProductDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public long? CategoryId { get; set; }
    public string CategoryName { get; set; }
}