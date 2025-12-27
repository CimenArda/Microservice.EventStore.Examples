using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events
{
    public class NewProductAddedEvent 
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int Count { get; set; }
        public bool IsAvailable { get; set; }
        public decimal Price { get; set; }
    }
}
