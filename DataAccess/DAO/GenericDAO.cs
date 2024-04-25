using System.Linq.Expressions;
using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO
{
	public class GenericDAO<TEntity> where TEntity: class
	{
        internal AppDbContext _context;
        internal DbSet<TEntity> _dbSet;

		public GenericDAO(AppDbContext context)
		{
			_context = context;
			_dbSet = context.Set<TEntity>();
		}

        public virtual async Task<IEnumerable<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string includeProperties = "")
        {
            IQueryable<TEntity> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync();
            }
            else
            {
                return await query.ToListAsync();
            }
        }

        public async virtual Task<TEntity?> GetById(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async virtual Task Insert(TEntity entity)
        {
           await _dbSet.AddAsync(entity);
        }

        public virtual void DeleteById(Guid id)
        {
            TEntity? entityToDelete = _dbSet.Find(id);
            if(entityToDelete != null)
            {
                Delete(entityToDelete);
            }
        }

        public virtual void Delete(TEntity entityToDelete)
        {
            if (_context.Entry(entityToDelete).State == EntityState.Detached)
            {
                _dbSet.Attach(entityToDelete);
            }
            _dbSet.Remove(entityToDelete);
        }

        public virtual void Update(TEntity entityToUpdate)
        {
            _dbSet.Attach(entityToUpdate);
            _context.Entry(entityToUpdate).State = EntityState.Modified;
        }
    }
}

