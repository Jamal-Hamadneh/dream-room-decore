using FluentValidation;
using WebApplication1.Contracts.Requests;

namespace WebApplication1.Validators;

public class CartRequestValidator : AbstractValidator<CartRequest>
{
    public CartRequestValidator() => RuleFor(x => x.UserId).GreaterThan(0);
}
