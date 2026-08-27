using FluentValidation;
using PaymentGateway.Models.Enums;

namespace PaymentGateway.Models;

public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
{
    public PaymentRequestValidator() // <-- Все правила описываются внутри конструктора!
    {
        RuleFor(request => request.Amount)
            .NotNull()
            .WithMessage("Amount is required")
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero");

        RuleFor(request => request.Currency)
            .NotNull()
            .WithMessage("Currency is required")
            .IsEnumName(typeof(Currency), caseSensitive: false)
            .WithMessage("Currency is not available");
    }
}