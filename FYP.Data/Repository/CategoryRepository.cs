using FYP.Data.Repository.IRepository;
using FYP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FYP.Data.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private ApplicationDbContext _db;
        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Category obj)
        {
            _db.Categories.Update(obj);
        }

        IEnumerable<Category> ICategoryRepository.OrderByDescending()
        {
            return _db.Categories.OrderByDescending(c => c.Id);
        }
    }
}
