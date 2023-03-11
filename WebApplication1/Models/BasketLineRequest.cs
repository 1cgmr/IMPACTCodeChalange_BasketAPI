using Newtonsoft.Json;

namespace BasketAPI.Models
{
    public class BasketLineRequest
    {
        [JsonRequired]
        public int productId { get; set; }
        [JsonRequired]
        public int quantity { get; set; }
    }
}
