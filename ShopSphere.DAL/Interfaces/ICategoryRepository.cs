using ShopSphere.Domain.Entities;

namespace ShopSphere.DAL.Interfaces;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<IReadOnlyList<Category>> GetAllWithParentsAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetByIdWithChildrenAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken cancellationToken = default);
}

