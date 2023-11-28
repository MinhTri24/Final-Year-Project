using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FYP.Data.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        ICategoryRequestRepository CategoryRequest { get; }
        IProductRepository Product { get; }
        ICartRepository Cart { get; }
        IApplicationUserRepository ApplicationUser { get; }

        void Save();
    }
}
