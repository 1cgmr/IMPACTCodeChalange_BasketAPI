namespace BasketAPI.Exceptions
{
    public class BasketNotFoundException : Exception
    {
        public BasketNotFoundException()
        {
        }

        public BasketNotFoundException(Guid id) : base(String.Format("Basket with ID: {0} not found", id))
        {
        }

        public BasketNotFoundException(string message)
            : base(message)
        {
        }

        public BasketNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
