using Microsoft.EntityFrameworkCore;
using edusync_backend.Models;
using edusync_backend.Data;

namespace edusync_backend.Repositories
{
    public class BaseRepository<T> where T : class, ISoftDelete
    {
        protected readonly EduSyncDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(EduSyncDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.Where(x => !x.IsDeleted).ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FirstOrDefaultAsync(x => !x.IsDeleted && EF.Property<Guid>(x, "UserId") == id);
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<T> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task SoftDeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public virtual async Task RestoreAsync(Guid id)
        {
            var entity = await _dbSet.FirstOrDefaultAsync(x => x.IsDeleted && EF.Property<Guid>(x, "UserId") == id);
            if (entity != null)
            {
                entity.IsDeleted = false;
                entity.DeletedAt = null;
                await _context.SaveChangesAsync();
            }
        }

        public virtual async Task HardDeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
} 