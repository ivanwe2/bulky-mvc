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
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }

        public void Update(Product product)
        {
            _dbSet.Update(product);
        }
    }
}
