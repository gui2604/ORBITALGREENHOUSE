using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OrbitalGreenhouse.Api.Data;

namespace OrbitalGreenhouse.Api.Repositories;

/// <summary>Default EF Core implementation of <see cref="IRepository{T}"/>.</summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<T> Set;

    public Repository(ApplicationDbContext context)
    {
        Context = context;
        Set = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id) => await Set.FindAsync(id);

    public virtual async Task<List<T>> GetAllAsync() => await Set.AsNoTracking().ToListAsync();

    public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate) => Set.AnyAsync(predicate);

    public async Task AddAsync(T entity) => await Set.AddAsync(entity);

    public void Update(T entity) => Set.Update(entity);

    public void Remove(T entity) => Set.Remove(entity);

    public Task<int> SaveChangesAsync() => Context.SaveChangesAsync();

    public IQueryable<T> Query() => Set.AsQueryable();
}
