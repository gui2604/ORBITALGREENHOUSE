using Microsoft.EntityFrameworkCore;
using OrbitalGreenhouse.Api.Data;
using OrbitalGreenhouse.Api.Models;

namespace OrbitalGreenhouse.Api.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public Task<User?> GetByEmailAsync(string email) =>
        Set.FirstOrDefaultAsync(u => u.Email == email);
}
