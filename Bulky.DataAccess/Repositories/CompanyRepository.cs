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
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        public CompanyRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }

        public void Update(Company company)
        {
            _dbSet.Update(company);
        }
    }
}
