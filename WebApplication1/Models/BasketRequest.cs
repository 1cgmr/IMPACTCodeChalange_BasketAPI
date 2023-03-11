using CodeChallengeApiClientNamespace;
using Newtonsoft.Json;

namespace BasketAPI.Models
{
    public class BasketRequest
    {
        public string userEmail { get; set; }
        public ICollection<BasketLineRequest> basketLines { get; set; }
    }

}
