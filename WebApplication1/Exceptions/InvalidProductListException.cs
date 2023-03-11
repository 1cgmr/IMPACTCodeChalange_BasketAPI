namespace BasketAPI.Exceptions
{
    public class InvalidProductListException : Exception
    {
        public InvalidProductListException() : base(String.Format("The provided product list contains products not referenced by the catalogue."))
        {
        }

        public InvalidProductListException(string message)
            : base(message)
        {
        }

        public InvalidProductListException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
