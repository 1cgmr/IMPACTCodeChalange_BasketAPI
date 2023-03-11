namespace BasketAPI.Models
{
    public class PaginationParameters
    {
        const int MAX_PAGE_SIZE = 1000;
        public int pageNumber { get; set; } = 1;
        private int _pageSize = 10;
        public int pageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value > MAX_PAGE_SIZE) ? MAX_PAGE_SIZE : value;
            }
        }
    }
}
