using Bulky.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repositories.Abstractions
{
    public interface ICompanyRepository : IRepository<Company>
    {
        void Update(Company company);
    }
}
