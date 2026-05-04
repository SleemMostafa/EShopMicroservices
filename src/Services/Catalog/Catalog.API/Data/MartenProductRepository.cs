using Catalog.API.Domain.Products;

namespace Catalog.API.Data;

public sealed class MartenProductRepository(IDocumentSession session) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await session.Query<Product>()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(
        string category,
        CancellationToken cancellationToken = default)
    {
        return await session.Query<Product>()
            .Where(product => product.Category.Contains(category))
            .ToListAsync(cancellationToken);
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return session.LoadAsync<Product>(id, cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        session.Store(product);
        await session.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        session.Update(product);
        await session.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        session.Delete<Product>(id);
        await session.SaveChangesAsync(cancellationToken);
    }
}
