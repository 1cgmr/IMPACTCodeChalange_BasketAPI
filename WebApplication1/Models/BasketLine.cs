using Newtonsoft.Json;

namespace BasketAPI.Models
{
    public class BasketLine
    {
        public int productId { get; set; }
        public string productName { get; set; }
        public decimal productUnitPrice { get; set; }
        public int quantity { get; set; }
        public decimal totalPrice { get; set; }

        public int size { get; set; }
    }
}
