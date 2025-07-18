﻿namespace StoreManagement.Domain.Aggregates.Products;

public class Product : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; }
    public string? Description { get; private set; }

    public long? CategoryId { get; private set; }
    public ProductCategory? Category { get; private set; }

    private readonly List<ProductVariant> _variants = new List<ProductVariant>();
    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();

    private Product() { }

    public Product(string name, string? description,long? categoryId)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        CategoryId = categoryId;
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name cannot be empty.", nameof(newName));

        Name = newName;
        UpdateTimestamp();
    }

    public void UpdateDescription(string newDescription)
    {
        if (string.IsNullOrWhiteSpace(newDescription))
            throw new ArgumentException("Description cannot be empty.", nameof(newDescription));

        Description = newDescription;
        UpdateTimestamp();
    }

    public void AddVariant(ProductVariant variant)
    {
        if (variant == null)
            throw new ArgumentNullException(nameof(variant));

        if (string.IsNullOrWhiteSpace(variant.SKU))
            throw new ArgumentNullException("SKU واریانت نمی‌تواند خالی باشد.");

        if (_variants.Any(v => v.SKU.Equals(variant.SKU, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentNullException($"SKU '{variant.SKU}' قبلاً اضافه شده است.");

        _variants.Add(variant);
    }

    public void RemoveVariant(ProductVariant variant)
    {
        if (variant == null)
            throw new ArgumentNullException(nameof(variant));

        var v = _variants.SingleOrDefault(x => x.Id == variant.Id)
                ?? throw new ArgumentNullException("واریانت با این شناسه یافت نشد.");

        _variants.Remove(variant);
    }


    public void SetCategory(ProductCategory category)
    {
        Category = category ?? throw new ArgumentNullException(nameof(category));
        CategoryId = category.Id;
        UpdateTimestamp();
    }

    public void ClearCategory()
    {
        Category = null;
        CategoryId = null;
        UpdateTimestamp();
    }

    public void Update(string name, string description, long? categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        Name = name;
        Description = description;
        CategoryId = categoryId;
        UpdateTimestamp();
    }
}