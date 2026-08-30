namespace PaymentGateway.Exceptions;

public class AppExceptions : Exception
{
    protected AppExceptions(string message) : base(message)
    {
    } 
}