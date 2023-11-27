using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FYP.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Category Name")]
        [MaxLength(50)]
        [Required(ErrorMessage = "Category name is required")]
        public required string Name { get; set; }
    }
}
