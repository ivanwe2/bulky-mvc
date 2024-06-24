using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repositories
{
	public abstract class Repository<T> : IRepository<T> where T : class
	{
		protected readonly ApplicationDbContext _context;
		protected DbSet<T> _dbSet;
        public Repository(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
			_dbSet = applicationDbContext.Set<T>();
        }
        public void Add(T entity)
		{
			_dbSet.Add(entity);
		}

		public void Delete(T entity)
		{
			_dbSet.Remove(entity);
		}

		public void DeleteRange(IEnumerable<T> range)
		{
			_dbSet.RemoveRange(range);
		}

		public IEnumerable<T> GetAllBy(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
		{
			IQueryable<T> query = _dbSet.AsNoTracking();
			if (!string.IsNullOrEmpty(includeProperties))
			{
				foreach (var property in includeProperties
					.Split(',', StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(property);
				}
			}

			if(filter is null)
				return query.ToList();

			return query.Where(filter).ToList();
		}

		public T GetBy(Expression<Func<T, bool>> filter, string? includeProperties = null)
		{
			IQueryable<T> query = _dbSet.AsNoTracking();
			if (!string.IsNullOrEmpty(includeProperties))
			{
				foreach (var property in includeProperties
					.Split(',', StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(property);
				}
			}

			return query.FirstOrDefault(filter) ?? throw new ArgumentException($"{typeof(T)} entity not found!");
		}
	}
}
