using Microsoft.EntityFrameworkCore;
using edusync_backend.Data;
using edusync_backend.Models;

namespace edusync_backend.Repositories
{
    public class UserRepository : BaseRepository<User>
    {
        public UserRepository(EduSyncDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(x => !x.IsDeleted && x.Email == email);
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(string role)
        {
            return await _dbSet.Where(x => !x.IsDeleted && x.Role == role).ToListAsync();
        }
    }
} 