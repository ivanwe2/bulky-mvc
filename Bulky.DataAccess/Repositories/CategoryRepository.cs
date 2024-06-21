using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repositories.Abstractions;
using Bulky.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repositories
{
	public class CategoryRepository : Repository<Category>, ICategoryRepository
	{
        public CategoryRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext) { }

		public void Update(Category category)
		{
			_context.Update(category);
		}
	}
}
