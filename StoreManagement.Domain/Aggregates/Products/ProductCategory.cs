namespace StoreManagement.Domain.Aggregates.Products;

public class ProductCategory : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int Order { get; private set; }

    private readonly List<Product> _products = new List<Product>();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    private readonly List<ProductCategory> _subcategories = new List<ProductCategory>();
    public IReadOnlyCollection<ProductCategory> Subcategories => _subcategories.AsReadOnly();

    public long? ParentCategoryId { get; private set; }
    public ProductCategory? ParentCategory { get; private set; }

    // Parameterless constructor for EF Core
    private ProductCategory() { }

    // Constructor for creating a new ProductCategory
    public ProductCategory(string name, string description, int order, ProductCategory? parentCategory = null)
    {
        Name = name;
        Description = description;
        Order = order;
        ParentCategory = parentCategory;
        Validate();
    }

    // Method to update the category details
    public void Update(string name, string description, int order)
    {
        Name = name;
        Description = description;
        Order = order;
        Validate();
        UpdateTimestamp();
    }

    // Method to update the name
    public void UpdateName(string newName)
    {
        Name = newName;
        Validate();
        UpdateTimestamp();
    }

    // Method to update the description
    public void UpdateDescription(string newDescription)
    {
        Description = newDescription;
        Validate();
        UpdateTimestamp();
    }

    // Method to update the order
    public void UpdateOrder(int newOrder)
    {
        Order = newOrder;
        Validate();
        UpdateTimestamp();
    }

    // Method to add a subcategory
    public void AddSubcategory(ProductCategory subcategory)
    {
        if (subcategory == null) throw new ArgumentNullException(nameof(subcategory));
        if (_subcategories.Contains(subcategory)) throw new InvalidOperationException("Subcategory already exists.");
        subcategory.SetParentCategory(this);
        _subcategories.Add(subcategory);
    }

    // Method to remove a subcategory
    public void RemoveSubcategory(ProductCategory subcategory)
    {
        if (subcategory == null) throw new ArgumentNullException(nameof(subcategory));
        if (!_subcategories.Contains(subcategory)) throw new InvalidOperationException("Subcategory does not exist.");
        subcategory.ClearParentCategory();
        _subcategories.Remove(subcategory);
        UpdateTimestamp();
    }

    // Method to set the parent category
    public void SetParentCategory(ProductCategory parentCategory)
    {
        ParentCategory = parentCategory;
        UpdateTimestamp();
    }

    // Method to clear the parent category
    private void ClearParentCategory()
    {
        ParentCategory = null;
        UpdateTimestamp();
    }

    // Validation logic for the entity
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name)) throw new InvalidOperationException("Name cannot be empty.");
        if (Order < 0) throw new InvalidOperationException("Order must be a non-negative integer.");
    }
}