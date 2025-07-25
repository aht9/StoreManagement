﻿namespace StoreManagement.Application.DTOs.Products;

public class ProductCategoryTreeDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Order { get; set; }
    public long? ParentCategoryId { get; set; } 
    public List<ProductCategoryTreeDto> Subcategories { get; set; } = new List<ProductCategoryTreeDto>();
}