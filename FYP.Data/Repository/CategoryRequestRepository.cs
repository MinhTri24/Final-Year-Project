using FYP.Data.Repository.IRepository;
using FYP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FYP.Data.Repository
{
    public class CategoryRequestRepository : Repository<CategoryRequest>, ICategoryRequestRepository
    {
        private ApplicationDbContext _db;
        public CategoryRequestRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(CategoryRequest obj)
        {
            _db.CategoryRequests.Update(obj);
        }
    }
}
