using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FYP.Models.ViewModels
{
	public class OrderVM
	{
		public IEnumerable<OrderDetail> OrderDetail { get; set; }
		public Order Order { get; set; }

	}
}
