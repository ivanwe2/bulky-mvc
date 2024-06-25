using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repositories.Abstractions
{
	public interface IRepository<T> where T : class
	{
		IEnumerable<T> GetAllBy(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
		T GetBy(Expression<Func<T,bool>> filter, string? includeProperties = null, bool tracked = false);
		void Add(T entity);
		void Delete(T entity);
		void DeleteRange(IEnumerable<T> range);
	}
}
