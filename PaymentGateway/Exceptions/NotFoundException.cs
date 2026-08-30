namespace PaymentGateway.Exceptions;

public class NotFoundException : AppExceptions
{
    public NotFoundException(string message) : base(message)
    {
    }
}