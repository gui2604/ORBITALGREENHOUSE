using Microsoft.EntityFrameworkCore;
using OrbitalGreenhouse.Api.Data;
using OrbitalGreenhouse.Api.Models;

namespace OrbitalGreenhouse.Api.Repositories;

public interface IRegionRepository : IRepository<Region>
{
    Task<Region?> GetByCodeAsync(string code);
    Task<List<Region>> GetAllWithDeviceCountAsync();
}

public class RegionRepository : Repository<Region>, IRegionRepository
{
    public RegionRepository(ApplicationDbContext context) : base(context) { }

    public Task<Region?> GetByCodeAsync(string code) =>
        Set.AsNoTracking().FirstOrDefaultAsync(r => r.Code == code);

    public Task<List<Region>> GetAllWithDeviceCountAsync() =>
        Set.AsNoTracking().Include(r => r.Devices).OrderBy(r => r.Code).ToListAsync();
}
