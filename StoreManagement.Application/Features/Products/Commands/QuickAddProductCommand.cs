namespace StoreManagement.Application.Features.Products.Commands;

public class QuickAddProductCommand : IRequest<ProductSearchResultDto>
{
    public string ProductName { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string Sku { get; set; }
}

public class QuickAddProductCommandHandler(
    IGenericRepository<Product> productRepository,
    IGenericRepository<ProductVariant> variantRepository)
    : IRequestHandler<QuickAddProductCommand, ProductSearchResultDto>
{
    public async Task<ProductSearchResultDto> Handle(QuickAddProductCommand request, CancellationToken cancellationToken)
    {
        var productSpec = new CustomExpressionSpecification<Product>(p => p.Name == request.ProductName && !p.IsDeleted);
        var existingProduct = await productRepository.FirstOrDefaultAsync(productSpec, cancellationToken);

        if (existingProduct == null)
        {
            existingProduct = new Product(request.ProductName, string.Empty, null);
            await productRepository.AddAsync(existingProduct, cancellationToken);
            await productRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken); 
        }

        var variantSpec = new CustomExpressionSpecification<ProductVariant>(v => v.ProductId == existingProduct.Id && v.SKU == request.Sku && !v.IsDeleted);
        if (await variantRepository.AnyAsync(variantSpec, cancellationToken))
        {
            throw new InvalidOperationException("واریانتی با این SKU برای این محصول از قبل موجود است.");
        }

        var newVariant = new ProductVariant(request.Sku, request.Color, request.Size, existingProduct.Id);
        await variantRepository.AddAsync(newVariant, cancellationToken);
        await variantRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken); 

        return new ProductSearchResultDto
        {
            ProductId = existingProduct.Id,
            VariantId = newVariant.Id,
            Name = existingProduct.Name,
            Sku = newVariant.SKU,
            Color = newVariant.Color,
            Size = newVariant.Size,
            Stock = 0, 
            LastSalePrice = null
        };
    }
}