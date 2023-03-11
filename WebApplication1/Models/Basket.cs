namespace BasketAPI.Models
{
    public class Basket
    {
        public Guid basketId { get; set; }
        public decimal totalAmount { get; set; }
        public string userEmail { get; set; }
        public ICollection<BasketLine> basketLines { get; set; }
    }
}
