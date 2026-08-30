namespace PaymentGateway.Exceptions;

public class ConflictException : AppExceptions
{
    public ConflictException(string message) : base(message)
    {
    }
}