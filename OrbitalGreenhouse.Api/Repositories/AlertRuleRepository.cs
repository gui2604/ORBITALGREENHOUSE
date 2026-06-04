using Microsoft.EntityFrameworkCore;
using OrbitalGreenhouse.Api.Data;
using OrbitalGreenhouse.Api.Models;

namespace OrbitalGreenhouse.Api.Repositories;

public interface IAlertRuleRepository : IRepository<AlertRule>
{
    /// <summary>
    /// Returns the active rules that apply to a given metric in a given region
    /// (region-specific rules plus global rules where RegionId is null).
    /// </summary>
    Task<List<AlertRule>> GetActiveRulesAsync(int metricTypeId, int regionId);

    Task<AlertRule?> GetWithDetailsAsync(int id);
    Task<List<AlertRule>> GetAllWithDetailsAsync();
}

public class AlertRuleRepository : Repository<AlertRule>, IAlertRuleRepository
{
    public AlertRuleRepository(ApplicationDbContext context) : base(context) { }

    public Task<List<AlertRule>> GetActiveRulesAsync(int metricTypeId, int regionId) =>
        Set.AsNoTracking()
            .Where(r => r.IsActive
                        && r.MetricTypeId == metricTypeId
                        && (r.RegionId == null || r.RegionId == regionId))
            .ToListAsync();

    public Task<AlertRule?> GetWithDetailsAsync(int id) =>
        Set.AsNoTracking()
            .Include(r => r.MetricType)
            .Include(r => r.Region)
            .FirstOrDefaultAsync(r => r.Id == id);

    public Task<List<AlertRule>> GetAllWithDetailsAsync() =>
        Set.AsNoTracking()
            .Include(r => r.MetricType)
            .Include(r => r.Region)
            .OrderBy(r => r.Id)
            .ToListAsync();
}
