using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repositories.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public ICategoryRepository Category { get; private set; }
        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _context = dbContext;
            Category = new CategoryRepository(dbContext);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
