using FluentValidation;
using PaymentGateway.Models.Enums;

namespace PaymentGateway.Models;

public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
{
    public PaymentRequestValidator() 
    {
        RuleFor(request => request.Amount)
            .NotNull()
            .WithMessage("Количество не введено")
            .GreaterThan(0)
            .WithMessage("Количество должно быть больше 0");

        RuleFor(request => request.Currency)
            .NotNull()
            .WithMessage("Валюта не введена")
            .IsEnumName(typeof(Currency), caseSensitive: false)
            .WithMessage("Данная валюта не поддерживается");
    }
}