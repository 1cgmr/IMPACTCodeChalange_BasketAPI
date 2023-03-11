namespace BasketAPI.Exceptions
{
    public class ProductNotFoundException : Exception
    {
        public ProductNotFoundException()
        {
        }

        public ProductNotFoundException(int id) : base(String.Format("Product with ID: {0} not found", id))
        {
        }

        public ProductNotFoundException(string message)
            : base(message)
        {
        }

        public ProductNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
